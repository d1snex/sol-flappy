using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneSetup
{
    private static readonly Color SkyBlue = new Color(0.306f, 0.753f, 0.792f, 1f);

    [MenuItem("FlappyBird/Setup All", priority = 0)]
    public static void SetupAll()
    {
        SetupPipePrefab();
        SetupBirdPrefab();
        SetupMainMenuScene();
        SetupGameScene();
        UpdateBuildSettings();
        Debug.Log("FlappyBird: All setup complete!");
    }

    [MenuItem("FlappyBird/Setup Pipe Prefab")]
    public static void SetupPipePrefab()
    {
        string prefabPath = "Assets/Prefabs/Pipe.prefab";

        var pipeRoot = new GameObject("Pipe");
        var pipeRb = pipeRoot.AddComponent<Rigidbody2D>();
        pipeRb.bodyType = RigidbodyType2D.Kinematic;
        pipeRoot.AddComponent<Pipe>();

        // Top pipe
        var topPipe = new GameObject("TopPipe");
        topPipe.transform.SetParent(pipeRoot.transform);
        topPipe.transform.localPosition = new Vector3(0f, 5f, 0f);
        topPipe.transform.localScale = new Vector3(1f, 2f, 1f);
        topPipe.layer = LayerMask.NameToLayer("Obstacle");

        var topSR = topPipe.AddComponent<SpriteRenderer>();
        topSR.sprite = LoadSprite("Assets/Art/Sprites/pipe-green.png");
        topSR.sortingLayerName = "Pipes";
        topSR.flipY = true;

        var topCol = topPipe.AddComponent<BoxCollider2D>();
        topCol.size = new Vector2(0.52f, 3.2f);

        // Bottom pipe
        var bottomPipe = new GameObject("BottomPipe");
        bottomPipe.transform.SetParent(pipeRoot.transform);
        bottomPipe.transform.localPosition = new Vector3(0f, -5f, 0f);
        bottomPipe.transform.localScale = new Vector3(1f, 2f, 1f);
        bottomPipe.layer = LayerMask.NameToLayer("Obstacle");

        var bottomSR = bottomPipe.AddComponent<SpriteRenderer>();
        bottomSR.sprite = LoadSprite("Assets/Art/Sprites/pipe-green.png");
        bottomSR.sortingLayerName = "Pipes";

        var bottomCol = bottomPipe.AddComponent<BoxCollider2D>();
        bottomCol.size = new Vector2(0.52f, 3.2f);

        // Score gate
        var scoreGate = new GameObject("ScoreGate");
        scoreGate.transform.SetParent(pipeRoot.transform);
        scoreGate.transform.localPosition = Vector3.zero;
        scoreGate.layer = LayerMask.NameToLayer("ScoreZone");
        scoreGate.tag = "ScoreGate";

        var gateCol = scoreGate.AddComponent<BoxCollider2D>();
        gateCol.isTrigger = true;
        gateCol.size = new Vector2(0.52f, 2.5f);

        PrefabUtility.SaveAsPrefabAsset(pipeRoot, prefabPath);
        Object.DestroyImmediate(pipeRoot);
        Debug.Log("FlappyBird: Pipe prefab created.");
    }

    [MenuItem("FlappyBird/Setup Bird Prefab")]
    public static void SetupBirdPrefab()
    {
        string prefabPath = "Assets/Prefabs/Bird.prefab";

        var birdGO = new GameObject("Bird");
        birdGO.layer = LayerMask.NameToLayer("Bird");
        birdGO.tag = "Untagged";

        var sr = birdGO.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Art/Sprites/yellowbird-midflap.png");
        sr.sortingLayerName = "Bird";
        sr.sortingOrder = 0;

        var rb = birdGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        var col = birdGO.AddComponent<CircleCollider2D>();
        col.radius = 0.15f;

        birdGO.AddComponent<BirdController>();

        var animator = birdGO.AddComponent<BirdAnimator>();
        var framesField = typeof(BirdAnimator).GetField("frames",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (framesField != null)
        {
            framesField.SetValue(animator, new Sprite[]
            {
                LoadSprite("Assets/Art/Sprites/yellowbird-downflap.png"),
                LoadSprite("Assets/Art/Sprites/yellowbird-midflap.png"),
                LoadSprite("Assets/Art/Sprites/yellowbird-upflap.png")
            });
        }

        PrefabUtility.SaveAsPrefabAsset(birdGO, prefabPath);
        Object.DestroyImmediate(birdGO);
        Debug.Log("FlappyBird: Bird prefab created.");
    }

    [MenuItem("FlappyBird/Setup MainMenu Scene")]
    public static void SetupMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cameraGO = CreateCamera("Main Camera");

        // Background
        CreateBackground();

        // Ground (decorative, no collider — 5 tiles to cover any viewport)
        for (int i = 0; i < 5; i++)
            CreateGroundTile($"Ground_{i}", 3.36f * (i - 1));

        // EventSystem
        CreateEventSystem();

        // Canvas
        var canvas = CreateCanvas("Canvas");

        // Message image
        var messageGO = CreateUIImage(canvas.transform, "MessageImage",
            LoadSprite("Assets/Art/Sprites/message.png"),
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
            Vector2.zero, new Vector2(184, 267));

        // Play button
        var playBtnGO = CreateButton(canvas.transform, "PlayButton", "PLAY",
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f),
            Vector2.zero, new Vector2(120, 50));

        // Connect Wallet button (below Play)
        var walletBtnGO = CreateButton(canvas.transform, "ConnectWalletButton", "Connect Wallet",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f),
            Vector2.zero, new Vector2(160, 50));

        // Managers
        var managers = new GameObject("Managers");

        var audioMgrGO = new GameObject("AudioManager");
        audioMgrGO.transform.SetParent(managers.transform);
        var audioSource = audioMgrGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        var audioMgr = audioMgrGO.AddComponent<AudioManager>();
        SetAudioClips(audioMgr);

        // WalletManager and Web3 must be root GameObjects for DontDestroyOnLoad to work
        var walletMgrGO = new GameObject("WalletManager");
        walletMgrGO.AddComponent<WalletManager>();

        var web3GO = new GameObject("Web3");
        var web3 = web3GO.AddComponent<Solana.Unity.SDK.Web3>();
        web3.solanaWalletAdapterOptions ??= new Solana.Unity.SDK.SolanaWalletAdapterOptions();
        web3.solanaWalletAdapterOptions.phantomWalletOptions ??= new Solana.Unity.SDK.PhantomWalletOptions();
        web3.solanaWalletAdapterOptions.phantomWalletOptions.DeeplinkUrlScheme = "solflappy";
        EditorUtility.SetDirty(web3);

        var menuUIGO = new GameObject("MainMenuUI");
        menuUIGO.transform.SetParent(managers.transform);
        var menuUI = menuUIGO.AddComponent<MainMenuUI>();
        SetPrivateField(menuUI, "playButton", playBtnGO.GetComponent<Button>());
        SetPrivateField(menuUI, "connectWalletButton", walletBtnGO.GetComponent<Button>());
        SetPrivateField(menuUI, "walletButtonText", walletBtnGO.GetComponentInChildren<UnityEngine.UI.Text>());

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("FlappyBird: MainMenu scene created.");
    }

    [MenuItem("FlappyBird/Setup Game Scene")]
    public static void SetupGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        CreateCamera("Main Camera");

        // Background
        CreateBackground();

        // Ground with scroller (5 tiles to cover any viewport aspect ratio)
        float tileW = 3.36f;
        int tileCount = 5;
        var groundParent = new GameObject("Ground");
        groundParent.tag = "Untagged";
        var scroller = groundParent.AddComponent<GroundScroller>();

        var groundTiles = new Transform[tileCount];
        for (int i = 0; i < tileCount; i++)
        {
            // Center tiles around x=0: positions at -3.36, 0, 3.36, 6.72, 10.08
            var tile = CreateGroundTile($"Ground_{i}", tileW * (i - 1));
            tile.transform.SetParent(groundParent.transform);
            tile.layer = LayerMask.NameToLayer("Obstacle");
            var rb = tile.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var col = tile.AddComponent<BoxCollider2D>();
            col.size = new Vector2(tileW, 1.12f);
            groundTiles[i] = tile.transform;
        }

        SetPrivateField(scroller, "tiles", groundTiles);
        SetPrivateField(scroller, "tileWidth", tileW);

        // Ceiling
        var ceiling = new GameObject("Ceiling");
        ceiling.tag = "Ceiling";
        ceiling.layer = LayerMask.NameToLayer("Boundary");
        ceiling.transform.position = new Vector3(0f, 5.2f, 0f);
        var ceilCol = ceiling.AddComponent<BoxCollider2D>();
        ceilCol.isTrigger = true;
        ceilCol.size = new Vector2(20f, 0.5f);

        // Bird
        var birdPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bird.prefab");
        if (birdPrefab != null)
        {
            var bird = (GameObject)PrefabUtility.InstantiatePrefab(birdPrefab);
            bird.transform.position = new Vector3(-1.5f, 1f, 0f);
        }

        // Pipe Spawner
        var pipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe.prefab");
        var spawnerGO = new GameObject("PipeSpawner");
        spawnerGO.transform.position = new Vector3(0f, 0f, 0f);
        var pool = spawnerGO.AddComponent<PipePool>();
        SetPrivateField(pool, "pipePrefab", pipePrefab);
        spawnerGO.AddComponent<PipeSpawner>();

        // Managers
        var managers = new GameObject("Managers");

        var gmGO = new GameObject("GameManager");
        gmGO.transform.SetParent(managers.transform);
        gmGO.AddComponent<GameManager>();

        var smGO = new GameObject("ScoreManager");
        smGO.transform.SetParent(managers.transform);
        smGO.AddComponent<ScoreManager>();

        var audioMgrGO = new GameObject("AudioManager");
        audioMgrGO.transform.SetParent(managers.transform);
        var audioSource = audioMgrGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        var audioMgr = audioMgrGO.AddComponent<AudioManager>();
        SetAudioClips(audioMgr);

        var inputGO = new GameObject("InputHandler");
        inputGO.transform.SetParent(managers.transform);
        inputGO.AddComponent<InputHandler>();

        // EventSystem
        CreateEventSystem();

        // HUD Canvas
        var hudCanvas = CreateCanvas("Canvas_HUD");

        // Score display (top center)
        var scoreParent = CreateUIObject(hudCanvas.transform, "ScoreDisplay",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -90f), new Vector2(200f, 50f));
        var scoreDisplay = scoreParent.AddComponent<ScoreDisplay>();
        SetDigitSprites(scoreDisplay);

        // GameUI script on HUD canvas
        var gameUI = hudCanvas.AddComponent<GameUI>();
        SetPrivateField(gameUI, "scoreDisplay", scoreDisplay);

        // GameOver Canvas
        var goCanvas = CreateCanvas("Canvas_GameOver");

        // GameOver panel (container)
        var goPanel = CreateUIObject(goCanvas.transform, "GameOverPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(300f, 400f));

        // GameOver image
        CreateUIImage(goPanel.transform, "GameOverImage",
            LoadSprite("Assets/Art/Sprites/gameover.png"),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -20f), new Vector2(192, 42));

        // Score label + display
        var currentLabel = CreateUIText(goPanel.transform, "ScoreLabel", "Score",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 40f), new Vector2(100f, 30f));

        var currentScoreObj = CreateUIObject(goPanel.transform, "CurrentScore",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 10f), new Vector2(150f, 40f));
        var currentScoreDisplay = currentScoreObj.AddComponent<ScoreDisplay>();
        SetDigitSprites(currentScoreDisplay);

        // Best label + display
        var bestLabel = CreateUIText(goPanel.transform, "BestLabel", "Best",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -30f), new Vector2(100f, 30f));

        var bestScoreObj = CreateUIObject(goPanel.transform, "BestScore",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -60f), new Vector2(150f, 40f));
        var bestScoreDisplay = bestScoreObj.AddComponent<ScoreDisplay>();
        SetDigitSprites(bestScoreDisplay);

        // Play Again button (just below best score)
        var playAgainBtn = CreateButton(goPanel.transform, "PlayAgainButton", "Play Again",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -110f), new Vector2(140f, 45f));

        // GameOverUI script
        var goUI = goCanvas.AddComponent<GameOverUI>();
        SetPrivateField(goUI, "panel", goPanel);
        SetPrivateField(goUI, "currentScoreDisplay", currentScoreDisplay);
        SetPrivateField(goUI, "bestScoreDisplay", bestScoreDisplay);
        SetPrivateField(goUI, "playAgainButton", playAgainBtn.GetComponent<Button>());

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
        Debug.Log("FlappyBird: Game scene created.");
    }

    [MenuItem("FlappyBird/Update Build Settings")]
    public static void UpdateBuildSettings()
    {
        var scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("FlappyBird: Build settings updated.");
    }

    // ─── Helpers ───

    private static GameObject CreateCamera(string name)
    {
        var go = new GameObject(name);
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, 0f, -10f);

        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = SkyBlue;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = -1f;
        cam.farClipPlane = 100f;

        go.AddComponent<AudioListener>();

        var urpData = go.AddComponent<UniversalAdditionalCameraData>();
        urpData.renderType = CameraRenderType.Base;

        return go;
    }

    private static GameObject CreateBackground()
    {
        var go = new GameObject("Background");
        go.transform.position = new Vector3(0f, 0f, 10f);
        go.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Art/Sprites/background-day.png");
        sr.sortingLayerName = "Background";
        sr.sortingOrder = 0;

        return go;
    }

    private static GameObject CreateGroundTile(string name, float xOffset)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(xOffset, -4.5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Art/Sprites/base.png");
        sr.sortingLayerName = "Ground";
        sr.sortingOrder = 0;

        return go;
    }

    private static void CreateEventSystem()
    {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject CreateCanvas(string name)
    {
        var go = new GameObject(name);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(480f, 852f);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        return go;
    }

    private static GameObject CreateUIObject(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        return go;
    }

    private static GameObject CreateUIImage(Transform parent, string name, Sprite sprite,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = CreateUIObject(parent, name, anchorMin, anchorMax, anchoredPos, sizeDelta);

        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = CreateUIObject(parent, name, anchorMin, anchorMax, anchoredPos, sizeDelta);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.85f, 0.55f, 0.15f, 1f);

        go.AddComponent<Button>();

        // Text child
        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);

        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var textComp = textGO.AddComponent<UnityEngine.UI.Text>();
        textComp.text = text;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.fontSize = 24;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return go;
    }

    private static GameObject CreateUIText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = CreateUIObject(parent, name, anchorMin, anchorMax, anchoredPos, sizeDelta);

        var textComp = go.AddComponent<UnityEngine.UI.Text>();
        textComp.text = text;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.fontSize = 20;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return go;
    }

    private static Sprite LoadSprite(string path)
    {
        // For spriteMode=Multiple, sprites are sub-assets
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        var sprite = assets.OfType<Sprite>().FirstOrDefault();
        if (sprite != null)
            return sprite;

        // Fallback: try loading as single sprite
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SetAudioClips(AudioManager mgr)
    {
        SetPrivateField(mgr, "clipWing", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/wing.ogg"));
        SetPrivateField(mgr, "clipPoint", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/point.ogg"));
        SetPrivateField(mgr, "clipHit", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/hit.ogg"));
        SetPrivateField(mgr, "clipDie", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/die.ogg"));
        SetPrivateField(mgr, "clipSwoosh", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/swoosh.ogg"));
    }

    private static void SetDigitSprites(ScoreDisplay display)
    {
        var digits = new Sprite[10];
        for (int i = 0; i < 10; i++)
        {
            digits[i] = LoadSprite($"Assets/Art/Sprites/Numbers/{i}.png");
        }
        SetPrivateField(display, "digitSprites", digits);
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
            EditorUtility.SetDirty((Object)obj);
        }
        else
        {
            Debug.LogWarning($"Could not find field '{fieldName}' on {obj.GetType().Name}");
        }
    }
}
