using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }

    public bool IsConnected { get; private set; }
    public string WalletAddress { get; private set; }

    public static event Action<string> OnWalletConnected;

    private static string DeepLinkFilePath =>
        Path.Combine(Application.persistentDataPath, "_deeplink.txt");

    private bool _waitingForPhantom;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CleanupStaleDeepLink()
    {
        // Delete any leftover deep link file from a previous session.
        // Stale URLs are encrypted with session keys that no longer exist.
        try
        {
            if (File.Exists(DeepLinkFilePath))
                File.Delete(DeepLinkFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[WALLET] Error deleting deep link file: {e.Message}");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Web3.OnLogin += OnWeb3Login;
    }

    private void OnDisable()
    {
        Web3.OnLogin -= OnWeb3Login;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused && _waitingForPhantom)
        {
            _waitingForPhantom = false;
            StartCoroutine(CheckForDeepLinkFile());
        }
    }

    private IEnumerator CheckForDeepLinkFile()
    {
        // Wait a few frames for the native plugin to write the file
        string url = "";
        for (int i = 0; i < 30; i++)
        {
            yield return null;
            url = ReadDeepLinkFile();
            if (!string.IsNullOrEmpty(url))
                break;
        }

        if (!string.IsNullOrEmpty(url))
            yield return InvokeSafeDeepLink(url);
    }

    private IEnumerator InvokeSafeDeepLink(string url)
    {
        // Get the backing field for Application.deepLinkActivated
        var field = typeof(Application).GetField("deepLinkActivated",
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

        if (field == null)
            yield break;

        var del = field.GetValue(null) as Action<string>;
        if (del == null)
            yield break;

        // WORKAROUND: The SDK checks url.Contains("onconnected") but the Phantom
        // redirect URL uses "onPhantomConnected". The substring "onphantomconnected"
        // does NOT contain "onconnected", so the SDK never processes it.
        string fixedUrl = url.Replace("onPhantomConnected", "onConnected");

        // Invoke each handler with try/catch because Web3Auth's handler crashes
        // on Phantom URLs (ArgumentOutOfRangeException in setResultUrl) and would
        // kill the multicast delegate chain, preventing PhantomDeepLink from running.
        foreach (var handler in del.GetInvocationList())
        {
            try
            {
                ((Action<string>)handler)(fixedUrl);
            }
            catch (Exception)
            {
                // Web3Auth throws on non-Web3Auth URLs — expected, skip it
            }
        }
    }

    private static string ReadDeepLinkFile()
    {
        try
        {
            if (File.Exists(DeepLinkFilePath))
            {
                string url = File.ReadAllText(DeepLinkFilePath).Trim();
                File.Delete(DeepLinkFilePath);
                return url;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[WALLET] Error reading deep link file: {e.Message}");
        }
        return "";
    }

    private void OnWeb3Login(Account account)
    {
        if (account == null)
            return;

        IsConnected = true;
        WalletAddress = account.PublicKey.Key;
        OnWalletConnected?.Invoke(WalletAddress);
    }

    public void ConnectWallet()
    {
        if (IsConnected)
            return;

#if UNITY_EDITOR
        IsConnected = true;
        WalletAddress = "EdTe1111111111111111111111111111111111111111";
        OnWalletConnected?.Invoke(WalletAddress);
        return;
#endif

#pragma warning disable CS0162
        if (Web3.Instance != null)
        {
            _waitingForPhantom = true;
            Web3.Instance.LoginWithWalletAdapter();
        }
#pragma warning restore CS0162
    }

    public string GetTruncatedAddress()
    {
        if (string.IsNullOrEmpty(WalletAddress) || WalletAddress.Length < 8)
            return WalletAddress ?? "";
        return WalletAddress.Substring(0, 4) + "..." + WalletAddress.Substring(WalletAddress.Length - 3);
    }
}
