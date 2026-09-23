using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject cellPrefab;

    [Header("Contenedor")]
    [SerializeField] private Transform cellsContainer;

    [Header("Cantidad")]
    [SerializeField] private int cellsPerRound = 10;

    [Header("Zona de aparición")]
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;
    [SerializeField] private float minY = -3.5f;
    [SerializeField] private float maxY = 3.5f;

    [Header("Variación inicial")]
    [SerializeField] private float initialMinSize = 0.6f;
    [SerializeField] private float initialMaxSize = 1.1f;

    [Header("Depredador automático (IA de detección)")]
    [Tooltip("Simula un depredador que 've' a las células según qué tan bien camufladas estén. Necesario para entrenar con mlagents-learn sin depender de que un humano haga clic. Puedes dejarlo activo también mientras juegas: se suma al clic del jugador, no lo reemplaza.")]
    [SerializeField] private bool enableAutoDetector = true;
    [Tooltip("Cada cuánto (segundos) el depredador revisa a las células vivas.")]
    [SerializeField] private float detectionCheckInterval = 0.5f;
    [Tooltip("Cuánto pesa el contraste de color con el fondo en la probabilidad de ser detectada.")]
    [SerializeField] private float colorSensitivity = 1.4f;
    [Tooltip("Cuánto pesa el tamaño (células más grandes son más fáciles de ver).")]
    [SerializeField] private float sizeSensitivity = 0.5f;
    [Tooltip("Probabilidad mínima de detección por chequeo, incluso si está perfectamente camuflada (deja algo de aleatoriedad/exploración).")]
    [SerializeField] private float minDetectionChance = 0.02f;
    [Tooltip("Probabilidad máxima de detección por chequeo.")]
    [SerializeField] private float maxDetectionChance = 0.9f;

    private List<GameObject> activeCells = new List<GameObject>();
    private Camera mainCamera;
    private Coroutine detectorRoutine;


    public void SpawnCells()
    {
        ClearCells();

        for (int i = 0; i < cellsPerRound; i++)
        {
            SpawnCell();
        }

        Debug.Log(
            "Se crearon " +
            activeCells.Count +
            " células."
        );

        if (enableAutoDetector)
        {
            detectorRoutine = StartCoroutine(AutoDetectorRoutine());
        }
    }

    // DEPREDADOR AUTOMÁTICO

    private IEnumerator AutoDetectorRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(detectionCheckInterval);

        while (true)
        {
            yield return wait;
            EvaluateAllCells();
        }
    }

    private void EvaluateAllCells()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Color backgroundColor =
            mainCamera != null ? mainCamera.backgroundColor : Color.black;

        // Recorremos hacia atrás porque NotifyCellEliminated
        // modifica activeCells mientras iteramos.
        for (int i = activeCells.Count - 1; i >= 0; i--)
        {
            GameObject cellObject = activeCells[i];

            if (cellObject == null)
                continue;

            Cell cell = cellObject.GetComponent<Cell>();

            if (cell == null || !cell.IsAlive)
                continue;

            float detectionChance =
                CalculateDetectionChance(cell, backgroundColor);

            if (Random.value < detectionChance)
            {
                cell.Eliminate("el depredador (IA)");
            }
        }
    }

    private float CalculateDetectionChance(
        Cell cell,
        Color backgroundColor)
    {
        Color cellColor = cell.CellColor;

        float colorDistance =
            Vector3.Distance(
                new Vector3(cellColor.r, cellColor.g, cellColor.b),
                new Vector3(backgroundColor.r, backgroundColor.g, backgroundColor.b)
            );

        // Distancia máxima posible entre dos colores RGB es sqrt(3).
        float normalizedColorDistance = Mathf.Clamp01(colorDistance / 1.732f);

        float normalizedSize =
            Mathf.InverseLerp(cell.MinSize, cell.MaxSize, cell.Size);

        float rawChance =
            normalizedColorDistance * colorSensitivity +
            normalizedSize * sizeSensitivity;

        return Mathf.Clamp(rawChance, minDetectionChance, maxDetectionChance);
    }

    private void SpawnCell()
    {
        Vector3 randomPosition =GetRandomPosition();

        GameObject newCell =Instantiate(cellPrefab,randomPosition,Quaternion.identity,cellsContainer);

        newCell.name ="Cell_" + activeCells.Count;

        Cell cellComponent =newCell.GetComponent<Cell>();

        if (cellComponent == null)
        {
            Debug.LogError("El prefab no tiene Cell.cs");

            Destroy(newCell);
            return;
        }
        cellComponent.Initialize(this);
        ConfigureRandomCell(cellComponent);
        CellAgent agent =newCell.GetComponent<CellAgent>();
        if (agent == null)
        {
            Debug.LogError( "El prefab no tiene CellAgent.cs");
        }
        else
        {
            agent.RequestAdaptation();
        }

        activeCells.Add(newCell);
    }
    private void ConfigureRandomCell(
        Cell cell)
    {
        float randomSize =
            Random.Range(
                initialMinSize,
                initialMaxSize
            );

        Color randomColor =
            new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                1f
            );

        cell.SetSize(randomSize);
        cell.SetColor(randomColor);

        Debug.Log(
            "Nueva célula | Size: " +
            randomSize.ToString("F2") +
            " | RGB: " +
            randomColor.r.ToString("F2") +
            ", " +
            randomColor.g.ToString("F2") +
            ", " +
            randomColor.b.ToString("F2")
        );
    }

    private Vector3 GetRandomPosition()
    {
        float randomX =
            Random.Range(minX, maxX);

        float randomY =
            Random.Range(minY, maxY);

        return new Vector3(
            randomX,
            randomY,
            0f
        );
    }

    public void NotifyCellEliminated(
        GameObject cell)
    {
        activeCells.Remove(cell);
    }
    public int GetSurvivingCellCount()
    {
        return activeCells.Count;
    }

    public void RewardSurvivingCells()
    {
        foreach (GameObject cellObject
                 in activeCells)
        {
            if (cellObject == null)
                continue;

            CellAgent agent =
                cellObject.GetComponent<CellAgent>();

            if (agent != null)
            {
                agent.OnSurvivedRound();
            }
        }
    }
    public void ClearCells()
    {
        if (detectorRoutine != null)
        {
            StopCoroutine(detectorRoutine);
            detectorRoutine = null;
        }

        foreach (GameObject cell
                 in activeCells)
        {
            if (cell != null)
            {
                Destroy(cell);
            }
        }

        activeCells.Clear();
    }
}