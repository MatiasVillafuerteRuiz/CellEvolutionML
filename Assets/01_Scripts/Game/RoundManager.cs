using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Configuración de ronda")]
    [SerializeField] private float roundDuration = 10f;

    [Header("Interfaz")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundText;
     public ScoreManager scoreManager;

    [Header("Células")]
    [SerializeField] private CellSpawner cellSpawner;
   
    private float timeRemaining;
    private int currentRound = 1;

    void Start()
    {
        StartRound();
    }

    void Update()
    {
        UpdateTimer();
    }

    private void StartRound()
    {
        timeRemaining = roundDuration;

        cellSpawner.SpawnCells();

        UpdateRoundUI();

        scoreManager.RefreshUI();

        Debug.Log("Comenzó la ronda " + currentRound);
    }
    private void UpdateTimer()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            UpdateTimerUI();
            EndRound();

            return;
        }

        UpdateTimerUI();
    }

    private void EndRound()
    {
        int survivors =
            cellSpawner.GetSurvivingCellCount();

        Debug.Log("Terminó la ronda " + currentRound + ". Sobrevivieron " + survivors + " células."
        );

        // Primero registramos los supervivientes.
        cellSpawner.RewardSurvivingCells();

        // Después calculamos las métricas.
        if (TrainingMetrics.Instance != null)
        {
            TrainingMetrics.Instance.EndRound();
        }

        currentRound++;

        StartRound();
    }

    private void UpdateTimerUI()
    {
        timerText.text = "TIEMPO: " + timeRemaining.ToString("F1");
    }

    private void UpdateRoundUI()
    {
        roundText.text = "RONDA " + currentRound;

        UpdateTimerUI();
    }
}