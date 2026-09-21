using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Interfaz")]
    [SerializeField] private TMP_Text scoreText;

    private int score = 0;

    public int Score => score;

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoint()
    {
        score++;
        Debug.Log("Puntuación actual: " + score);
        UpdateScoreUI();

        
    }

    public void RefreshUI()
    {
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreText NO está asignado en ScoreManager.");
            return;
        }

        scoreText.text = "PUNTOS: " + score;
    }
}