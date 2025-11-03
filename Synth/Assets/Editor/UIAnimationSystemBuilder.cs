using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI 애니메이션 시스템을 자동으로 생성하는 Unity Editor 도구
/// 
/// 사용법:
/// 1. GameScene (또는 SampleScene)을 열기
/// 2. Unity 메뉴: Tools → Synth → Create UI Animation System
/// 3. 자동으로 모든 UI 요소와 매니저가 생성됨
/// 
/// 생성되는 항목:
/// - Canvas (없으면 생성)
/// - UI 텍스트 4개 (ComboText, ScoreText, PercentText, JudgmentText)
/// - HitEffect Prefab
/// - GameplayUIManager
/// - GameScoreManager
/// - HitEffectPool
/// </summary>
public class UIAnimationSystemBuilder : EditorWindow
{
    private bool createCanvas = true;
    private bool createUITexts = true;
    private bool createHitEffectPrefab = true;
    private bool createManagers = true;
    private bool autoLinkReferences = true;

    [MenuItem("Tools/Synth/Create UI Animation System")]
    public static void ShowWindow()
    {
        UIAnimationSystemBuilder window = GetWindow<UIAnimationSystemBuilder>("UI Animation System Builder");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("UI 애니메이션 시스템 자동 생성", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "이 도구는 UI 애니메이션 시스템을 자동으로 생성합니다.\n" +
            "GameScene을 열고 실행하세요.",
            MessageType.Info
        );
        EditorGUILayout.Space();

        // 옵션 선택
        GUILayout.Label("생성 옵션:", EditorStyles.boldLabel);
        createCanvas = EditorGUILayout.Toggle("Canvas 생성/설정", createCanvas);
        createUITexts = EditorGUILayout.Toggle("UI 텍스트 생성", createUITexts);
        createHitEffectPrefab = EditorGUILayout.Toggle("HitEffect Prefab 생성", createHitEffectPrefab);
        createManagers = EditorGUILayout.Toggle("매니저 오브젝트 생성", createManagers);
        autoLinkReferences = EditorGUILayout.Toggle("참조 자동 연결", autoLinkReferences);

        EditorGUILayout.Space();

        // 실행 버튼
        if (GUILayout.Button("🚀 자동 생성 시작", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "UI 애니메이션 시스템 생성",
                "UI 애니메이션 시스템을 자동으로 생성하시겠습니까?\n\n" +
                "기존에 같은 이름의 오브젝트가 있으면 건너뜁니다.",
                "생성",
                "취소"))
            {
                CreateUIAnimationSystem();
            }
        }

        EditorGUILayout.Space();

        // 도움말
        EditorGUILayout.HelpBox(
            "생성 후 할 일:\n" +
            "1. Play 버튼을 눌러 테스트\n" +
            "2. GameManager의 Auto Start 체크\n" +
            "3. 콤보/점수/판정 애니메이션 확인",
            MessageType.Info
        );
    }

    private void CreateUIAnimationSystem()
    {
        Debug.Log("=== UI 애니메이션 시스템 자동 생성 시작 ===");

        GameObject canvas = null;
        GameObject comboText = null;
        GameObject scoreText = null;
        GameObject percentText = null;
        GameObject judgmentText = null;
        GameObject hitEffectPrefab = null;
        GameObject gameplayUIManager = null;
        GameObject gameScoreManager = null;
        GameObject hitEffectPool = null;

        try
        {
            // 1. Canvas 생성/설정
            if (createCanvas)
            {
                canvas = CreateOrGetCanvas();
            }

            // 2. UI 텍스트 생성
            if (createUITexts && canvas != null)
            {
                comboText = CreateComboText(canvas);
                scoreText = CreateScoreText(canvas);
                percentText = CreatePercentText(canvas);
                judgmentText = CreateJudgmentText(canvas);
            }

            // 3. HitEffect Prefab 생성
            if (createHitEffectPrefab)
            {
                hitEffectPrefab = CreateHitEffectPrefab();
            }

            // 4. 매니저 오브젝트 생성
            if (createManagers)
            {
                gameplayUIManager = CreateGameplayUIManager();
                gameScoreManager = CreateGameScoreManager();
                hitEffectPool = CreateHitEffectPool();
            }

            // 5. 참조 자동 연결
            if (autoLinkReferences && gameplayUIManager != null)
            {
                LinkGameplayUIManagerReferences(
                    gameplayUIManager,
                    comboText,
                    scoreText,
                    percentText,
                    judgmentText
                );
            }

            if (autoLinkReferences && hitEffectPool != null && hitEffectPrefab != null)
            {
                LinkHitEffectPoolReferences(hitEffectPool, hitEffectPrefab);
            }

            // 씬 저장
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("=== UI 애니메이션 시스템 생성 완료! ===");
            EditorUtility.DisplayDialog(
                "완료!",
                "UI 애니메이션 시스템이 성공적으로 생성되었습니다!\n\n" +
                "Play 버튼을 눌러 테스트하세요.",
                "확인"
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UI 애니메이션 시스템 생성 중 오류: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("오류", $"생성 중 오류 발생:\n{e.Message}", "확인");
        }
    }

    #region Canvas 생성
    private GameObject CreateOrGetCanvas()
    {
        // 기존 Canvas 찾기
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log($"✓ 기존 Canvas 사용: {existingCanvas.name}");
            ConfigureCanvas(existingCanvas.gameObject);
            return existingCanvas.gameObject;
        }

        // Canvas 생성
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Canvas Scaler 추가
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Graphic Raycaster 추가
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("✓ Canvas 생성 완료");
        return canvasObj;
    }

    private void ConfigureCanvas(GameObject canvasObj)
    {
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObj.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        Debug.Log("✓ Canvas 설정 완료");
    }
    #endregion

    #region UI 텍스트 생성
    private GameObject CreateComboText(GameObject canvas)
    {
        GameObject existingObj = GameObject.Find("ComboText");
        if (existingObj != null)
        {
            Debug.Log("✓ ComboText 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject comboTextObj = new GameObject("ComboText");
        comboTextObj.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = comboTextObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, 200);
        rectTransform.sizeDelta = new Vector2(400, 150);

        TextMeshProUGUI tmpText = comboTextObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "0\nCOMBO";
        tmpText.fontSize = 72;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        Debug.Log("✓ ComboText 생성 완료");
        return comboTextObj;
    }

    private GameObject CreateScoreText(GameObject canvas)
    {
        GameObject existingObj = GameObject.Find("ScoreText");
        if (existingObj != null)
        {
            Debug.Log("✓ ScoreText 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = scoreTextObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(150, -50);
        rectTransform.sizeDelta = new Vector2(300, 80);

        TextMeshProUGUI tmpText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "0";
        tmpText.fontSize = 48;
        tmpText.alignment = TextAlignmentOptions.Left;
        tmpText.color = Color.white;

        Debug.Log("✓ ScoreText 생성 완료");
        return scoreTextObj;
    }

    private GameObject CreatePercentText(GameObject canvas)
    {
        GameObject existingObj = GameObject.Find("PercentText");
        if (existingObj != null)
        {
            Debug.Log("✓ PercentText 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject percentTextObj = new GameObject("PercentText");
        percentTextObj.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = percentTextObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-150, -50);
        rectTransform.sizeDelta = new Vector2(200, 60);

        TextMeshProUGUI tmpText = percentTextObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "0.0%";
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Right;
        tmpText.color = Color.white;

        Debug.Log("✓ PercentText 생성 완료");
        return percentTextObj;
    }

    private GameObject CreateJudgmentText(GameObject canvas)
    {
        GameObject existingObj = GameObject.Find("JudgmentText");
        if (existingObj != null)
        {
            Debug.Log("✓ JudgmentText 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject judgmentTextObj = new GameObject("JudgmentText");
        judgmentTextObj.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = judgmentTextObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, -100);
        rectTransform.sizeDelta = new Vector2(600, 150);

        TextMeshProUGUI tmpText = judgmentTextObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "";
        tmpText.fontSize = 96;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.enableWordWrapping = false;

        Debug.Log("✓ JudgmentText 생성 완료");
        return judgmentTextObj;
    }
    #endregion

    #region HitEffect Prefab 생성
    private GameObject CreateHitEffectPrefab()
    {
        // Prefabs 폴더 확인/생성
        string prefabPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
            Debug.Log("✓ Prefabs 폴더 생성");
        }

        string prefabFullPath = prefabPath + "/HitEffect.prefab";

        // 이미 존재하는지 확인
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath);
        if (existingPrefab != null)
        {
            Debug.Log("✓ HitEffect Prefab 이미 존재 (건너뜀)");
            return existingPrefab;
        }

        // 임시 오브젝트 생성
        GameObject hitEffectObj = new GameObject("HitEffect");

        // SpriteRenderer 추가
        SpriteRenderer spriteRenderer = hitEffectObj.AddComponent<SpriteRenderer>();
        
        // 기본 스프라이트 찾기 (Unity 내장 스프라이트)
        Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
        
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 100;

        // HitEffect 스크립트 추가
        hitEffectObj.AddComponent<HitEffect>();

        // Prefab으로 저장
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(hitEffectObj, prefabFullPath);

        // 임시 오브젝트 삭제
        DestroyImmediate(hitEffectObj);

        Debug.Log($"✓ HitEffect Prefab 생성 완료: {prefabFullPath}");
        return prefab;
    }
    #endregion

    #region 매니저 오브젝트 생성
    private GameObject CreateGameplayUIManager()
    {
        GameObject existingObj = GameObject.Find("GameplayUIManager");
        if (existingObj != null)
        {
            Debug.Log("✓ GameplayUIManager 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject managerObj = new GameObject("GameplayUIManager");
        managerObj.AddComponent<GameplayUIManager>();

        Debug.Log("✓ GameplayUIManager 생성 완료");
        return managerObj;
    }

    private GameObject CreateGameScoreManager()
    {
        GameObject existingObj = GameObject.Find("GameScoreManager");
        if (existingObj != null)
        {
            Debug.Log("✓ GameScoreManager 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject managerObj = new GameObject("GameScoreManager");
        managerObj.AddComponent<GameScoreManager>();

        Debug.Log("✓ GameScoreManager 생성 완료");
        return managerObj;
    }

    private GameObject CreateHitEffectPool()
    {
        GameObject existingObj = GameObject.Find("HitEffectPool");
        if (existingObj != null)
        {
            Debug.Log("✓ HitEffectPool 이미 존재 (건너뜀)");
            return existingObj;
        }

        GameObject poolObj = new GameObject("HitEffectPool");
        poolObj.AddComponent<HitEffectPool>();

        Debug.Log("✓ HitEffectPool 생성 완료");
        return poolObj;
    }
    #endregion

    #region 참조 연결
    private void LinkGameplayUIManagerReferences(
        GameObject managerObj,
        GameObject comboText,
        GameObject scoreText,
        GameObject percentText,
        GameObject judgmentText)
    {
        GameplayUIManager manager = managerObj.GetComponent<GameplayUIManager>();
        if (manager == null)
        {
            Debug.LogWarning("GameplayUIManager 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        SerializedObject serializedManager = new SerializedObject(manager);

        if (comboText != null)
        {
            SerializedProperty comboTextProp = serializedManager.FindProperty("comboText");
            if (comboTextProp != null)
            {
                comboTextProp.objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (scoreText != null)
        {
            SerializedProperty scoreTextProp = serializedManager.FindProperty("scoreText");
            if (scoreTextProp != null)
            {
                scoreTextProp.objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (percentText != null)
        {
            SerializedProperty percentTextProp = serializedManager.FindProperty("percentText");
            if (percentTextProp != null)
            {
                percentTextProp.objectReferenceValue = percentText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (judgmentText != null)
        {
            SerializedProperty judgmentTextProp = serializedManager.FindProperty("judgmentText");
            if (judgmentTextProp != null)
            {
                judgmentTextProp.objectReferenceValue = judgmentText.GetComponent<TextMeshProUGUI>();
            }
        }

        serializedManager.ApplyModifiedProperties();

        Debug.Log("✓ GameplayUIManager 참조 연결 완료");
    }

    private void LinkHitEffectPoolReferences(GameObject poolObj, GameObject prefab)
    {
        HitEffectPool pool = poolObj.GetComponent<HitEffectPool>();
        if (pool == null)
        {
            Debug.LogWarning("HitEffectPool 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        SerializedObject serializedPool = new SerializedObject(pool);
        SerializedProperty prefabProp = serializedPool.FindProperty("hitEffectPrefab");
        
        if (prefabProp != null)
        {
            prefabProp.objectReferenceValue = prefab;
            serializedPool.ApplyModifiedProperties();
            Debug.Log("✓ HitEffectPool Prefab 참조 연결 완료");
        }
    }
    #endregion
}
