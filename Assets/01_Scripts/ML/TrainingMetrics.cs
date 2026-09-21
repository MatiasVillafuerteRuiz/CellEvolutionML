using UnityEngine;

public class TrainingMetrics : MonoBehaviour
{
    public static TrainingMetrics Instance { get; private set; }

    private int clickedCells;
    private int survivedCells;

    private float totalSurvivorSize;
    private float totalColorDistance;

    private int currentRound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterClickedCell()
    {
        clickedCells++;
    }

    public void RegisterSurvivor(float size,Color cellColor,Color backgroundColor)
    {
        survivedCells++;
        totalSurvivorSize += size;
        float colorDistance =Vector3.Distance(new Vector3(cellColor.r,cellColor.g,cellColor.b),new Vector3(backgroundColor.r,backgroundColor.g,backgroundColor.b));
        totalColorDistance += colorDistance;
    }

    public void EndRound()
    {
        currentRound++;

        int totalCells = clickedCells + survivedCells;

        float survivalRate = totalCells > 0 ? (float)survivedCells / totalCells : 0f;

        float averageSize = survivedCells > 0 ? totalSurvivorSize / survivedCells : 0f;

        float averageColorDistance = survivedCells > 0 ? totalColorDistance / survivedCells : 0f;

        Debug.Log(
            "========== MÉTRICAS RONDA " +
            currentRound +
            " ==========\n" +

            "Eliminadas: " +
            clickedCells + "\n" +

            "Sobrevivientes: " +
            survivedCells + "\n" +

            "Supervivencia: " +
            (survivalRate * 100f).ToString("F1") +
            "%\n" +

            "Tamaño promedio supervivientes: " +
            averageSize.ToString("F3") + "\n" +

            "Distancia promedio al fondo: " +
            averageColorDistance.ToString("F3") + "\n" +

            "=================================="
        );

        ResetRound();
    }

    private void ResetRound()
    {
        clickedCells = 0;
        survivedCells = 0;

        totalSurvivorSize = 0f;
        totalColorDistance = 0f;
    }
}