using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// ResultScene 자동 생성 빌더
/// Unity 메뉴: Tools → Build Result Scene
/// 
/// 생성 시간: 약 5초 (자동)
/// 생성일: 2025-11-03
/// </summary>
public class ResultSceneBuilder : EditorWindow
{
    private const string SCENE_NAME = "ResultScene";
    private const string SCENE_PATH = "Assets/Scenes/ResultScene.unity";
    
    [MenuItem("Tools/Build Result Scene")]
    public static void BuildScene()
    {
        if (EditorUtility.DisplayDialog("ResultScene 자동 생성",
            "ResultScene을 자동으로 생성하시겠습니까?\n\n" +
            "생성 내용:\n" +
            "- Canvas (Screen Space Overlay)\n" +
            "- PlayResultUI + 모든 UI 요소\n" +
            "- 곡 정보 (제목, 아티스트, 난이도, 키 개수)\n" +
            "- 결과 정보 (점수, 정확도, 콤보, 랭크)\n" +
            "- 판정 통계 (S Perfect ~ Miss)\n" +
            "- 특수 표시 (Full Combo, Perfect Play)\n" +
            "- 3개 버튼 (재시작, 곡 선택, 메인 메뉴)\n\n" +
            "예상 시간: 5초",
            "생성", "취소"))
        {
            CreateResultScene();
        }
    }
    
    private static void CreateResultScene()
    {
        // 새 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Canvas 생성
        GameObject canvasObj = CreateCanvas();
        
        // PlayResultUI 생성 (모든 UI 포함)
        GameObject resultUIObj = CreatePlayResultUI(canvasObj.transform);
        
        // GameResultManager 생성
        GameObject gameResultManagerObj = CreateGameResultManager();
        
        // ResultSceneLoader 생성
        GameObject resultSceneLoaderObj = CreateResultSceneLoader(resultUIObj);
        
        // 씬 저장
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        
        Debug.Log($"✅ ResultScene 생성 완료!\n경로: {SCENE_PATH}\n\n" +
                  $"생성된 오브젝트:\n" +
                  $"- Canvas\n" +
                  $"- PlayResultUI (+ 60개 이상의 UI 요소)\n" +
                  $"- GameResultManager\n" +
                  $"- ResultSceneLoader\n\n" +
                  $"다음 단계:\n" +
                  $"1. Play 버튼을 눌러 테스트\n" +
                  $"2. 결과 표시 확인\n" +
                  $"3. Build Settings에 씬 추가");
        
        EditorUtility.DisplayDialog("생성 완료!", 
            "ResultScene이 성공적으로 생성되었습니다!\n\n" +
            $"경로: {SCENE_PATH}\n\n" +
            "이제 Play 버튼을 눌러 테스트해보세요.\n" +
            "테스트 데이터가 자동으로 로드됩니다.", "확인");
    }
    
    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // EventSystem 생성
        if (GameObject.Find("EventSystem") == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        return canvasObj;
    }
    
    private static GameObject CreatePlayResultUI(Transform parent)
    {
        GameObject mainObj = new GameObject("PlayResultUI");
        mainObj.transform.SetParent(parent, false);
        
        RectTransform mainRect = mainObj.AddComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = Vector2.zero;
        
        PlayResultUI resultUI = mainObj.AddComponent<PlayResultUI>();
        
        // 배경 생성
        GameObject background = CreateBackground(mainObj.transform);
        
        // 곡 정보 패널
        GameObject songInfoPanel = CreateSongInfoPanel(mainObj.transform);
        
        // 결과 정보 패널
        GameObject resultPanel = CreateResultPanel(mainObj.transform);
        
        // 판정 통계 패널
        GameObject judgmentPanel = CreateJudgmentPanel(mainObj.transform);
        
        // 특수 표시 패널
        GameObject specialPanel = CreateSpecialIndicatorsPanel(mainObj.transform);
        
        // 버튼 패널
        GameObject buttonPanel = CreateButtonPanel(mainObj.transform);
        
        // PlayResultUI 참조 연결
        ConnectReferences(resultUI, songInfoPanel, resultPanel, judgmentPanel, specialPanel, buttonPanel);
        
        return mainObj;
    }
    
    private static GameObject CreateBackground(Transform parent)
    {
        GameObject obj = new GameObject("Background");
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.1f, 1f); // 어두운 파란색 배경
        
        return obj;
    }
    
    private static GameObject CreateSongInfoPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "SongInfoPanel", new Vector2(0, 400), new Vector2(800, 150));
        
        // 제목
        CreateLabeledText(panel.transform, "SongTitleText", "곡 제목", "Song Title", 
            new Vector2(0, 40), 36, TextAlignmentOptions.Center, Color.white);
        
        // 아티스트
        CreateLabeledText(panel.transform, "ArtistNameText", "", "Artist Name", 
            new Vector2(0, 0), 24, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f));
        
        // 난이도와 키 개수 (가로로 배치)
        GameObject difficultyObj = CreateText(panel.transform, "DifficultyText", "HARD", 
            new Vector2(-100, -40), 20, TextAlignmentOptions.Center, new Color(1f, 0.5f, 0.5f));
        
        GameObject keyCountObj = CreateText(panel.transform, "KeyCountText", "4K", 
            new Vector2(100, -40), 20, TextAlignmentOptions.Center, new Color(0.5f, 0.8f, 1f));
        
        return panel;
    }
    
    private static GameObject CreateResultPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "ResultPanel", new Vector2(0, 150), new Vector2(1000, 300));
        
        // 랭크 (대형 텍스트)
        GameObject rankObj = CreateText(panel.transform, "RankText", "S", 
            new Vector2(0, 80), 120, TextAlignmentOptions.Center, new Color(1f, 0.84f, 0f)); // 금색
        
        // 점수, 정확도, 콤보 (3열 레이아웃)
        float spacing = 300f;
        
        CreateLabeledValue(panel.transform, "ScoreText", "SCORE", "950000", 
            new Vector2(-spacing, -40), 16, 32);
        
        CreateLabeledValue(panel.transform, "AccuracyText", "ACCURACY", "98.5%", 
            new Vector2(0, -40), 16, 32);
        
        CreateLabeledValue(panel.transform, "MaxComboText", "MAX COMBO", "512", 
            new Vector2(spacing, -40), 16, 32);
        
        return panel;
    }
    
    private static GameObject CreateJudgmentPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "JudgmentPanel", new Vector2(0, -150), new Vector2(800, 300));
        
        // 판정 행들
        float yStart = 100f;
        float spacing = 40f;
        
        CreateJudgmentRow(panel.transform, "SPerfect", "S PERFECT", yStart, new Color(1f, 0.84f, 0f));
        CreateJudgmentRow(panel.transform, "Perfect", "PERFECT", yStart - spacing, Color.yellow);
        CreateJudgmentRow(panel.transform, "Great", "GREAT", yStart - spacing * 2, Color.green);
        CreateJudgmentRow(panel.transform, "Good", "GOOD", yStart - spacing * 3, Color.cyan);
        CreateJudgmentRow(panel.transform, "Bad", "BAD", yStart - spacing * 4, Color.magenta);
        CreateJudgmentRow(panel.transform, "Miss", "MISS", yStart - spacing * 5, Color.red);
        
        return panel;
    }
    
    private static void CreateJudgmentRow(Transform parent, string name, string label, float yPos, Color color)
    {
        GameObject row = new GameObject($"{name}Row");
        row.transform.SetParent(parent, false);
        
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchoredPosition = new Vector2(0, yPos);
        rowRect.sizeDelta = new Vector2(600, 35);
        
        // 라벨
        GameObject labelObj = CreateText(row.transform, $"{name}Label", label, 
            new Vector2(-200, 0), 20, TextAlignmentOptions.Left, color);
        
        // 카운트
        GameObject countObj = CreateText(row.transform, $"{name}CountText", "0", 
            new Vector2(200, 0), 24, TextAlignmentOptions.Right, Color.white);
    }
    
    private static GameObject CreateSpecialIndicatorsPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "SpecialIndicatorsPanel", new Vector2(0, -380), new Vector2(800, 100));
        
        // Full Combo 표시
        GameObject fullComboObj = new GameObject("FullComboIndicator");
        fullComboObj.transform.SetParent(panel.transform, false);
        fullComboObj.SetActive(false); // 기본 비활성화
        
        RectTransform fcRect = fullComboObj.AddComponent<RectTransform>();
        fcRect.anchoredPosition = new Vector2(-150, 0);
        fcRect.sizeDelta = new Vector2(250, 60);
        
        Image fcImage = fullComboObj.AddComponent<Image>();
        fcImage.color = new Color(0.2f, 0.8f, 0.2f, 0.3f); // 초록색 반투명 배경
        
        GameObject fcText = CreateText(fullComboObj.transform, "FullComboText", "FULL COMBO", 
            Vector2.zero, 24, TextAlignmentOptions.Center, Color.green);
        
        // Perfect Play 표시
        GameObject perfectPlayObj = new GameObject("PerfectPlayIndicator");
        perfectPlayObj.transform.SetParent(panel.transform, false);
        perfectPlayObj.SetActive(false); // 기본 비활성화
        
        RectTransform ppRect = perfectPlayObj.AddComponent<RectTransform>();
        ppRect.anchoredPosition = new Vector2(150, 0);
        ppRect.sizeDelta = new Vector2(250, 60);
        
        Image ppImage = perfectPlayObj.AddComponent<Image>();
        ppImage.color = new Color(0.8f, 0.2f, 0.8f, 0.3f); // 마젠타 반투명 배경
        
        GameObject ppText = CreateText(perfectPlayObj.transform, "PerfectPlayText", "PERFECT PLAY", 
            Vector2.zero, 24, TextAlignmentOptions.Center, new Color(1f, 0.5f, 1f));
        
        return panel;
    }
    
    private static GameObject CreateButtonPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "ButtonPanel", new Vector2(0, -480), new Vector2(1000, 80));
        
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        CreateButton(panel.transform, "RetryButton", "재시작", new Color(0.3f, 0.6f, 1f, 1f));
        CreateButton(panel.transform, "BackToSongSelectButton", "곡 선택", new Color(0.6f, 0.6f, 0.6f, 1f));
        CreateButton(panel.transform, "BackToMainMenuButton", "메인 메뉴", new Color(0.5f, 0.5f, 0.5f, 1f));
        
        return panel;
    }
    
    private static GameObject CreateUIPanel(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        return obj;
    }
    
    private static GameObject CreateText(Transform parent, string name, string text, Vector2 position, 
        float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);
        
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        
        return obj;
    }
    
    private static void CreateLabeledText(Transform parent, string name, string label, string value, 
        Vector2 position, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject container = new GameObject($"{name}_Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(600, 60);
        
        if (!string.IsNullOrEmpty(label))
        {
            GameObject labelObj = CreateText(container.transform, "Label", label, 
                new Vector2(0, 15), fontSize * 0.5f, alignment, new Color(0.7f, 0.7f, 0.7f));
        }
        
        GameObject valueObj = CreateText(container.transform, name, value, 
            new Vector2(0, -10), fontSize, alignment, color);
    }
    
    private static void CreateLabeledValue(Transform parent, string name, string label, string value, 
        Vector2 position, float labelSize, float valueSize)
    {
        GameObject container = new GameObject($"{name}_Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(250, 80);
        
        // 라벨
        GameObject labelObj = CreateText(container.transform, "Label", label, 
            new Vector2(0, 20), labelSize, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f));
        
        // 값
        GameObject valueObj = CreateText(container.transform, name, value, 
            new Vector2(0, -15), valueSize, TextAlignmentOptions.Center, Color.white);
    }
    
    private static GameObject CreateButton(Transform parent, string name, string text, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 60);
        
        Image img = obj.AddComponent<Image>();
        img.color = color;
        
        Button btn = obj.AddComponent<Button>();
        
        // 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return obj;
    }
    
    private static GameObject CreateGameResultManager()
    {
        GameObject obj = new GameObject("GameResultManager");
        obj.AddComponent<GameResultManager>();
        return obj;
    }
    
    private static GameObject CreateResultSceneLoader(GameObject playResultUIObj)
    {
        PlayResultUI resultUI = playResultUIObj.GetComponent<PlayResultUI>();
        
        GameObject obj = new GameObject("ResultSceneLoader");
        ResultSceneLoader loader = obj.AddComponent<ResultSceneLoader>();
        
        // PlayResultUI 참조 연결
        var serializedObject = new SerializedObject(loader);
        var playResultUIProperty = serializedObject.FindProperty("playResultUI");
        playResultUIProperty.objectReferenceValue = resultUI;
        serializedObject.ApplyModifiedProperties();
        
        return obj;
    }
    
    private static void ConnectReferences(PlayResultUI resultUI, GameObject songInfoPanel, GameObject resultPanel, 
        GameObject judgmentPanel, GameObject specialPanel, GameObject buttonPanel)
    {
        // 곡 정보
        resultUI.songTitleText = songInfoPanel.transform.Find("SongTitleText_Container/SongTitleText").GetComponent<TextMeshProUGUI>();
        resultUI.artistNameText = songInfoPanel.transform.Find("ArtistNameText_Container/ArtistNameText").GetComponent<TextMeshProUGUI>();
        resultUI.difficultyText = songInfoPanel.transform.Find("DifficultyText").GetComponent<TextMeshProUGUI>();
        resultUI.keyCountText = songInfoPanel.transform.Find("KeyCountText").GetComponent<TextMeshProUGUI>();
        
        // 결과 정보
        resultUI.rankText = resultPanel.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
        resultUI.scoreText = resultPanel.transform.Find("ScoreText_Container/ScoreText").GetComponent<TextMeshProUGUI>();
        resultUI.accuracyText = resultPanel.transform.Find("AccuracyText_Container/AccuracyText").GetComponent<TextMeshProUGUI>();
        resultUI.maxComboText = resultPanel.transform.Find("MaxComboText_Container/MaxComboText").GetComponent<TextMeshProUGUI>();
        
        // 판정 통계
        resultUI.sPerfectCountText = judgmentPanel.transform.Find("SPerfectRow/SPerfectCountText").GetComponent<TextMeshProUGUI>();
        resultUI.perfectCountText = judgmentPanel.transform.Find("PerfectRow/PerfectCountText").GetComponent<TextMeshProUGUI>();
        resultUI.greatCountText = judgmentPanel.transform.Find("GreatRow/GreatCountText").GetComponent<TextMeshProUGUI>();
        resultUI.goodCountText = judgmentPanel.transform.Find("GoodRow/GoodCountText").GetComponent<TextMeshProUGUI>();
        resultUI.badCountText = judgmentPanel.transform.Find("BadRow/BadCountText").GetComponent<TextMeshProUGUI>();
        resultUI.missCountText = judgmentPanel.transform.Find("MissRow/MissCountText").GetComponent<TextMeshProUGUI>();
        
        // 특수 표시
        resultUI.fullComboIndicator = specialPanel.transform.Find("FullComboIndicator").gameObject;
        resultUI.perfectPlayIndicator = specialPanel.transform.Find("PerfectPlayIndicator").gameObject;
        
        // 버튼
        resultUI.retryButton = buttonPanel.transform.Find("RetryButton").GetComponent<Button>();
        resultUI.backToSongSelectButton = buttonPanel.transform.Find("BackToSongSelectButton").GetComponent<Button>();
        resultUI.backToMainMenuButton = buttonPanel.transform.Find("BackToMainMenuButton").GetComponent<Button>();
        
        EditorUtility.SetDirty(resultUI);
    }
}
