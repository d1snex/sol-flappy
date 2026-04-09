using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private ScoreDisplay currentScoreDisplay;
    [SerializeField] private ScoreDisplay bestScoreDisplay;
    [SerializeField] private Button playAgainButton;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= OnGameOver;
    }

    private void OnGameOver()
    {
        panel.SetActive(true);
        currentScoreDisplay.SetScore(ScoreManager.Instance.CurrentScore);
        bestScoreDisplay.SetScore(ScoreManager.Instance.HighScore);

        playAgainButton.onClick.RemoveAllListeners();
        playAgainButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySwoosh();
            GameManager.Instance.RestartGame();
        });
    }
}
