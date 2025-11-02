using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameScene을 자동으로 생성하는 에디터 스크립트
/// 메뉴: Tools → Build GameScene
/// </summary>
public class GameSceneBuilder : EditorWindow
{
    private int keyCount = 4; // 기본 4K
    private bool createSampleChart = true;
    private bool autoConnectReferences = true;

    [MenuItem("Tools/Build GameScene")]
    public static void ShowWindow()
    {
        GetWindow<GameSceneBuilder>("GameScene Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("GameScene 자동 생성", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        keyCount = EditorGUILayout.IntSlider("Key Count", keyCount, 4, 10);
        createSampleChart = EditorGUILayout.Toggle("Create Sample Chart", createSampleChart);
        autoConnectReferences = EditorGUILayout.Toggle("Auto Connect References", autoConnectReferences);

        EditorGUILayout.Space();

        if (GUILayout.Button("Build Complete GameScene", GUILayout.Height(40)))
        {
            BuildGameScene();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "이 도구는 GameScene을 자동으로 생성합니다:\n" +
            "• Core Systems (GameManager, ChartLoader, etc.)\n" +
            "• Gameplay Objects (GearController, NoteSpawner, HPSystem)\n" +
            "• UI Canvas (Score, Progress, Combo displays)\n" +
            "• 모든 참조 자동 연결",
            MessageType.Info
        );
    }

    private void BuildGameScene()
    {
        if (!EditorUtility.DisplayDialog(
            "GameScene 생성",
            "현재 씬에 GameScene 오브젝트를 생성하시겠습니까?\n\n" +
            "주의: 기존 오브젝트와 충돌할 수 있습니다.",
            "생성", "취소"))
        {
            return;
        }

        // 1. Core Systems
        GameObject gameManager = CreateGameManager();
        GameObject chartLoader = CreateChartLoader();
        GameObject audioManager = FindOrCreateAudioManager();
        GameObject inputManager = CreateInputManager();
        GameObject rhythmManager = FindOrCreateRhythmManager();

        // 2. Gameplay Objects
        GameObject gearController = CreateGearController(keyCount);
        GameObject noteSpawner = CreateNoteSpawner();
        GameObject hpSystem = CreateHPSystem();

        // 3. UI Canvas
        GameObject canvas = CreateUICanvas();
        GameObject progressDisplay = CreateProgressDisplay(canvas);
        GameObject scoreDisplay = CreateScoreDisplay(canvas);
        GameObject comboDisplay = CreateComboJudgmentDisplay(canvas);
        GameObject offsetDisplay = CreateJudgmentOffsetDisplay(canvas);
        GameObject pauseMenu = CreatePauseMenu(canvas);

        // 4. Camera Setup
        SetupMainCamera();

        // 5. Auto Connect References
        if (autoConnectReferences)
        {
            ConnectAllReferences(
                gameManager, chartLoader, audioManager, inputManager, rhythmManager,
                gearController, noteSpawner, hpSystem,
                progressDisplay, scoreDisplay, comboDisplay, offsetDisplay, pauseMenu
            );
        }

        // 6. Create Sample Chart
        if (createSampleChart)
        {
            CreateSampleChartData();
        }

        // 7. Save Scene
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("<color=green>✅ GameScene 생성 완료!</color>");
        EditorUtility.DisplayDialog("완료", "GameScene이 성공적으로 생성되었습니다!", "확인");
    }

    #region Core Systems

    private GameObject CreateGameManager()
    {
        GameObject obj = new GameObject("GameManager");
        GameManager gm = obj.AddComponent<GameManager>();

        // 기본값 설정 (Reflection 사용)
        SerializedObject so = new SerializedObject(gm);
        so.FindProperty("autoStart").boolValue = false;
        so.FindProperty("useSampleChart").boolValue = true;
        so.FindProperty("useNoteSpawner").boolValue = true;
        so.ApplyModifiedProperties();

        Debug.Log("✓ GameManager 생성");
        return obj;
    }

    private GameObject CreateChartLoader()
    {
        GameObject obj = new GameObject("ChartLoader");
        obj.AddComponent<ChartLoader>();
        Debug.Log("✓ ChartLoader 생성");
        return obj;
    }

    private GameObject FindOrCreateAudioManager()
    {
        AudioManager existing = FindObjectOfType<AudioManager>();
        if (existing != null)
        {
            Debug.Log("✓ 기존 AudioManager 사용");
            return existing.gameObject;
        }

        GameObject obj = new GameObject("AudioManager");
        obj.AddComponent<AudioManager>();
        Debug.Log("✓ AudioManager 생성");
        return obj;
    }

    private GameObject CreateInputManager()
    {
        GameObject obj = new GameObject("InputManager");
        obj.AddComponent<InputManager>();
        Debug.Log("✓ InputManager 생성");
        return obj;
    }

    private GameObject FindOrCreateRhythmManager()
    {
        // RhythmManager 찾기 (RhytmManager 오타 버전도 체크)
        GameObject obj = GameObject.Find("RhythmManager");
        if (obj == null)
        {
            obj = GameObject.Find("RhytmManager");
        }

        if (obj == null)
        {
            obj = new GameObject("RhythmManager");
            // RhythmManager 스크립트가 있으면 추가
            var type = System.Type.GetType("RhythmManager");
            if (type != null)
            {
                obj.AddComponent(type);
            }
            Debug.Log("✓ RhythmManager 생성");
        }
        else
        {
            Debug.Log("✓ 기존 RhythmManager 사용");
        }

        return obj;
    }

    #endregion

    #region Gameplay Objects

    private GameObject CreateGearController(int keys)
    {
        GameObject obj = new GameObject("GearController");
        GearController gc = obj.AddComponent<GearController>();

        // Settings 생성
        GearSettings settings = ScriptableObject.CreateInstance<GearSettings>();
        settings.lineCount = keys;
        settings.lineWidth = 1f;
        settings.lineSpacing = 0.1f;
        settings.gearHeight = 8f;
        settings.judgmentLineY = -3f;

        SerializedObject so = new SerializedObject(gc);
        so.FindProperty("settings").objectReferenceValue = settings;
        so.ApplyModifiedProperties();

        // 트랙 생성
        CreateTracks(obj, keys);

        // 판정선 생성
        CreateJudgmentLine(obj);

        Debug.Log($"✓ GearController 생성 ({keys}K)");
        return obj;
    }

    private void CreateTracks(GameObject parent, int count)
    {
        GameObject tracksContainer = new GameObject("Tracks");
        tracksContainer.transform.SetParent(parent.transform);

        float trackWidth = 0.6f; // 트랙 폭 (얇게 조정)
        float spacing = 0.8f; // 트랙 간격
        float startX = -(count - 1) * spacing / 2f; // 중앙 정렬

        for (int i = 0; i < count; i++)
        {
            GameObject track = GameObject.CreatePrimitive(PrimitiveType.Quad);
            track.name = $"Track_{i}";
            track.transform.SetParent(tracksContainer.transform);
            track.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            track.transform.localScale = new Vector3(trackWidth, 8f, 1f);

            // 머티리얼 설정
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            track.GetComponent<Renderer>().material = mat;

            // Tag 설정
            track.tag = "Track";
        }

        Debug.Log($"  → {count}개 트랙 생성");
    }

    private void CreateJudgmentLine(GameObject parent)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "JudgmentLine";
        line.transform.SetParent(parent.transform);
        line.transform.localPosition = new Vector3(0, -3f, -0.1f);
        line.transform.localScale = new Vector3(keyCount * 0.8f + 0.5f, 0.1f, 1f); // 트랙 간격에 맞춤

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 1f, 0f, 0.8f); // 노란색
        line.GetComponent<Renderer>().material = mat;

        Debug.Log("  → 판정선 생성");
    }

    private GameObject CreateNoteSpawner()
    {
        GameObject obj = new GameObject("NoteSpawner");
        NoteSpawner ns = obj.AddComponent<NoteSpawner>();

        SerializedObject so = new SerializedObject(ns);
        so.FindProperty("spawnOffset").floatValue = 2f;
        so.FindProperty("noteSpeed").floatValue = 5f;
        so.ApplyModifiedProperties();

        Debug.Log("✓ NoteSpawner 생성");
        return obj;
    }

    private GameObject CreateHPSystem()
    {
        GameObject obj = new GameObject("HPSystem");
        HPSystem hp = obj.AddComponent<HPSystem>();

        // HP Bar 생성
        GameObject hpBar = CreateHPBar(obj);

        Debug.Log("✓ HPSystem 생성");
        return obj;
    }

    private GameObject CreateHPBar(GameObject parent)
    {
        GameObject hpBarContainer = new GameObject("HPBar");
        hpBarContainer.transform.SetParent(parent.transform);
        hpBarContainer.transform.localPosition = Vector3.zero;

        // Background (세로 HP바 - 왼쪽 배치)
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "HPBackground";
        bg.transform.SetParent(hpBarContainer.transform);
        bg.transform.localPosition = new Vector3(-6f, 0f, 0); // 왼쪽 배치
        bg.transform.localScale = new Vector3(0.4f, 7f, 1f); // 세로: 폭 0.4, 높이 7
        bg.transform.localRotation = Quaternion.identity; // 회전 없음 (세로)

        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bg.GetComponent<Renderer>().material = bgMat;

        // Fill (세로 HP바 - 왼쪽 배치)
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "HPFill";
        fill.transform.SetParent(hpBarContainer.transform);
        fill.transform.localPosition = new Vector3(-6f, -3.5f, -0.05f); // 아래부터 채우기
        fill.transform.localScale = new Vector3(0.4f, 7f, 1f); // 세로: 폭 0.4, 높이 7
        fill.transform.localRotation = Quaternion.identity; // 회전 없음 (세로)

        Material fillMat = new Material(Shader.Find("Sprites/Default"));
        fillMat.color = new Color(0.2f, 1f, 0.2f, 1f); // 초록색
        fill.GetComponent<Renderer>().material = fillMat;

        // HPBarAnimator 추가
        hpBarContainer.AddComponent<HPBarAnimator>();

        Debug.Log("  → HP Bar 생성 (세로형, 왼쪽 배치)");
        return hpBarContainer;
    }

    #endregion

    #region UI Canvas

    private GameObject CreateUICanvas()
    {
        // 기존 Canvas 찾기
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("✓ 기존 Canvas 사용");
            return existingCanvas.gameObject;
        }

        GameObject canvas = new GameObject("Canvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;

        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvas.AddComponent<GraphicRaycaster>();

        Debug.Log("✓ UI Canvas 생성");
        return canvas;
    }

    private GameObject CreateProgressDisplay(GameObject canvas)
    {
        GameObject obj = new GameObject("ProgressDisplay");
        obj.transform.SetParent(canvas.transform);
        ProgressDisplay pd = obj.AddComponent<ProgressDisplay>();
        
        // 화면 최상단으로 위치 설정
        SerializedObject pdSO = new SerializedObject(pd);
        pdSO.FindProperty("barPosition").vector3Value = new Vector3(0, 8f, -0.1f);
        pdSO.ApplyModifiedProperties();

        Debug.Log("  → ProgressDisplay 생성 (화면 최상단)");
        return obj;
    }

    private GameObject CreateScoreDisplay(GameObject canvas)
    {
        GameObject obj = new GameObject("ScoreDisplay");
        obj.transform.SetParent(canvas.transform);
        obj.AddComponent<ScoreDisplay>();

        Debug.Log("  → ScoreDisplay 생성");
        return obj;
    }

    private GameObject CreateComboJudgmentDisplay(GameObject canvas)
    {
        GameObject obj = new GameObject("ComboJudgmentDisplay");
        obj.transform.SetParent(canvas.transform);
        obj.AddComponent<ComboJudgmentDisplay>();

        Debug.Log("  → ComboJudgmentDisplay 생성");
        return obj;
    }

    private GameObject CreateJudgmentOffsetDisplay(GameObject canvas)
    {
        GameObject obj = new GameObject("JudgmentOffsetDisplay");
        obj.transform.SetParent(canvas.transform);
        obj.AddComponent<JudgmentOffsetDisplay>();

        Debug.Log("  → JudgmentOffsetDisplay 생성");
        return obj;
    }

    private GameObject CreatePauseMenu(GameObject canvas)
    {
        GameObject obj = new GameObject("PauseMenuUI");
        obj.transform.SetParent(canvas.transform);
        obj.AddComponent<PauseMenuUI>();

        Debug.Log("  → PauseMenuUI 생성");
        return obj;
    }

    #endregion

    #region Utilities

    private void SetupMainCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            Debug.Log("✓ 메인 카메라 설정 완료");
        }
    }

    private void ConnectAllReferences(
        GameObject gameManager, GameObject chartLoader, GameObject audioManager,
        GameObject inputManager, GameObject rhythmManager, GameObject gearController,
        GameObject noteSpawner, GameObject hpSystem, GameObject progressDisplay,
        GameObject scoreDisplay, GameObject comboDisplay, GameObject offsetDisplay,
        GameObject pauseMenu)
    {
        Debug.Log("🔗 참조 자동 연결 시작...");

        // GameManager 참조 연결
        GameManager gm = gameManager.GetComponent<GameManager>();
        SerializedObject gmSO = new SerializedObject(gm);

        gmSO.FindProperty("chartLoader").objectReferenceValue = chartLoader.GetComponent<ChartLoader>();
        gmSO.FindProperty("noteSpawner").objectReferenceValue = noteSpawner.GetComponent<NoteSpawner>();
        gmSO.FindProperty("audioManager").objectReferenceValue = audioManager.GetComponent<AudioManager>();
        gmSO.FindProperty("hpSystem").objectReferenceValue = hpSystem.GetComponent<HPSystem>();
        gmSO.FindProperty("gearController").objectReferenceValue = gearController.GetComponent<GearController>();
        gmSO.FindProperty("inputManager").objectReferenceValue = inputManager.GetComponent<InputManager>();
        gmSO.FindProperty("progressDisplay").objectReferenceValue = progressDisplay.GetComponent<ProgressDisplay>();
        gmSO.FindProperty("scoreDisplay").objectReferenceValue = scoreDisplay.GetComponent<ScoreDisplay>();
        gmSO.FindProperty("comboJudgmentDisplay").objectReferenceValue = comboDisplay.GetComponent<ComboJudgmentDisplay>();
        gmSO.FindProperty("judgmentOffsetDisplay").objectReferenceValue = offsetDisplay.GetComponent<JudgmentOffsetDisplay>();
        gmSO.FindProperty("pauseMenuUI").objectReferenceValue = pauseMenu.GetComponent<PauseMenuUI>();

        gmSO.ApplyModifiedProperties();

        // NoteSpawner 참조 연결
        NoteSpawner ns = noteSpawner.GetComponent<NoteSpawner>();
        SerializedObject nsSO = new SerializedObject(ns);
        nsSO.FindProperty("chartLoader").objectReferenceValue = chartLoader.GetComponent<ChartLoader>();
        nsSO.FindProperty("audioManager").objectReferenceValue = audioManager.GetComponent<AudioManager>();
        nsSO.FindProperty("gearController").objectReferenceValue = gearController.GetComponent<GearController>();
        nsSO.ApplyModifiedProperties();

        // InputManager 참조 연결
        InputManager im = inputManager.GetComponent<InputManager>();
        SerializedObject imSO = new SerializedObject(im);
        imSO.FindProperty("gearController").objectReferenceValue = gearController.GetComponent<GearController>();
        imSO.ApplyModifiedProperties();

        // ProgressDisplay 참조 연결
        ProgressDisplay pd = progressDisplay.GetComponent<ProgressDisplay>();
        SerializedObject pdSO = new SerializedObject(pd);
        pdSO.FindProperty("audioManager").objectReferenceValue = audioManager.GetComponent<AudioManager>();
        pdSO.ApplyModifiedProperties();

        Debug.Log("<color=cyan>✓ 모든 참조 연결 완료</color>");
    }

    private void CreateSampleChartData()
    {
        Debug.Log("📄 샘플 차트 생성 중...");

        // ChartLoader를 통해 샘플 차트 생성
        ChartLoader loader = FindObjectOfType<ChartLoader>();
        if (loader != null)
        {
            // Reflection으로 CreateSampleChart 호출
            var method = typeof(ChartLoader).GetMethod("CreateSampleChart");
            if (method != null)
            {
                method.Invoke(loader, null);
                Debug.Log("<color=green>✓ 샘플 차트 생성 완료</color>");
            }
        }
    }

    #endregion
}
