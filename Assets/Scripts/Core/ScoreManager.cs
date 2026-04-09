using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }

    public static event Action<int> OnScoreChanged;

    private const string HighScoreKey = "HighScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        CurrentScore = 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddPoint()
    {
        CurrentScore++;
        OnScoreChanged?.Invoke(CurrentScore);
        AudioManager.Instance.PlayPoint();
    }

    public void SaveHighScore()
    {
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }
}
