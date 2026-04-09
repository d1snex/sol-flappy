using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;

    private void OnEnable()
    {
        ScoreManager.OnScoreChanged += OnScoreChanged;
        GameManager.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreChanged -= OnScoreChanged;
        GameManager.OnGameOver -= OnGameOver;
    }

    private void Start()
    {
        scoreDisplay.SetScore(0);
    }

    private void OnScoreChanged(int score)
    {
        scoreDisplay.SetScore(score);
    }

    private void OnGameOver()
    {
        // Hide the HUD score so only the game over panel score is visible
        scoreDisplay.gameObject.SetActive(false);
    }
}
