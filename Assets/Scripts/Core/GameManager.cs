using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { WaitingToStart, Playing, Dead, GameOver }
    public GameState State { get; private set; }

    public static event Action OnGameStarted;
    public static event Action OnBirdDied;
    public static event Action OnGameOver;

    [SerializeField] private float deathPauseDelay = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        State = GameState.WaitingToStart;
    }

    private void Start()
    {
        // Auto-start the game immediately when the scene loads
        StartGame();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartGame()
    {
        State = GameState.Playing;
        OnGameStarted?.Invoke();
    }

    public void BirdDied()
    {
        if (State == GameState.Dead || State == GameState.GameOver)
            return;

        State = GameState.Dead;
        OnBirdDied?.Invoke();
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        AudioManager.Instance.PlayHit();
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.PlayDie();
        yield return new WaitForSeconds(deathPauseDelay);
        State = GameState.GameOver;
        ScoreManager.Instance.SaveHighScore();
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
