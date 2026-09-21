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

    private List<GameObject> activeCells = new List<GameObject>();


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