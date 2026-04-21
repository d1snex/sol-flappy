using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Solana.Unity.SDK;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button connectWalletButton;
    [SerializeField] private Text balanceLabel;

    private Text _connectButtonText;
    private Image _connectButtonImage;

    private readonly Color _defaultButtonColor = new Color(0.85f, 0.55f, 0.15f, 1f);
    private readonly Color _connectedButtonColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayPressed);
        connectWalletButton.onClick.AddListener(OnConnectWalletPressed);

        _connectButtonText = connectWalletButton.GetComponentInChildren<Text>();
        _connectButtonImage = connectWalletButton.GetComponent<Image>();

        if (balanceLabel != null)
            balanceLabel.gameObject.SetActive(false);

        WalletManager.OnWalletConnected += OnWalletConnected;
        UpdateConnectButton();
    }

    private void OnDestroy()
    {
        WalletManager.OnWalletConnected -= OnWalletConnected;
        Web3.OnBalanceChange -= OnBalanceChange;
    }

    private void OnPlayPressed()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySwoosh();
        SceneManager.LoadScene("Game");
    }

    private void OnConnectWalletPressed()
    {
        if (WalletManager.Instance != null)
            WalletManager.Instance.ConnectWallet();
    }

    private void OnWalletConnected(string address)
    {
        UpdateConnectButton();
        Web3.OnBalanceChange += OnBalanceChange;
    }

    private void OnBalanceChange(double sol)
    {
        if (balanceLabel == null)
            return;
        balanceLabel.gameObject.SetActive(true);
        balanceLabel.text = $"{sol:F2} SOL";
    }

    private void UpdateConnectButton()
    {
        if (WalletManager.Instance != null && WalletManager.Instance.IsConnected)
        {
            if (_connectButtonText != null)
                _connectButtonText.text = WalletManager.Instance.GetTruncatedAddress();
            if (_connectButtonImage != null)
                _connectButtonImage.color = _connectedButtonColor;
            connectWalletButton.interactable = false;
        }
        else
        {
            if (_connectButtonText != null)
                _connectButtonText.text = "Connect";
            if (_connectButtonImage != null)
                _connectButtonImage.color = _defaultButtonColor;
            connectWalletButton.interactable = true;
        }
    }
}
