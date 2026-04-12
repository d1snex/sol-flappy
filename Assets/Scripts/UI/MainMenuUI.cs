using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayPressed);
    }

    private void OnPlayPressed()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySwoosh();
        SceneManager.LoadScene("Game");
    }
}
