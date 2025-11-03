using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// OptionsScene 자동 생성 빌더
/// Unity 메뉴: Tools → Build Options Scene
/// 
/// 생성 시간: 약 5분 (자동)
/// 생성일: 2025-11-03
/// </summary>
public class OptionsSceneBuilder : EditorWindow
{
    private const string SCENE_NAME = "OptionsScene";
    private const string SCENE_PATH = "Assets/Scenes/OptionsScene.unity";
    
    [MenuItem("Tools/Build Options Scene")]
    public static void BuildScene()
    {
        if (EditorUtility.DisplayDialog("OptionsScene 자동 생성",
            "OptionsScene을 자동으로 생성하시겠습니까?\n\n" +
            "생성 내용:\n" +
            "- Canvas (Screen Space Overlay)\n" +
            "- SettingsManager\n" +
            "- OptionMenuUI + 모든 UI 요소\n" +
            "- 3개 탭 (오디오/비주얼/게임플레이)\n" +
            "- 모든 슬라이더, 버튼, 토글 자동 연결\n\n" +
            "예상 시간: 5초",
            "생성", "취소"))
        {
            CreateOptionsScene();
        }
    }
    
    private static void CreateOptionsScene()
    {
        // 새 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Canvas 생성
        GameObject canvasObj = CreateCanvas();
        
        // SettingsManager 생성
        GameObject settingsManagerObj = CreateSettingsManager(canvasObj.transform);
        
        // OptionMenuUI 생성 (모든 UI 포함)
        GameObject optionMenuUIObj = CreateOptionMenuUI(canvasObj.transform);
        
        // 씬 저장
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        
        Debug.Log($"✅ OptionsScene 생성 완료!\n경로: {SCENE_PATH}\n\n" +
                  $"생성된 오브젝트:\n" +
                  $"- Canvas\n" +
                  $"- SettingsManager\n" +
                  $"- OptionMenuUI (+ 50개 이상의 UI 요소)\n\n" +
                  $"다음 단계:\n" +
                  $"1. Play 버튼을 눌러 테스트\n" +
                  $"2. 설정 변경 확인\n" +
                  $"3. Build Settings에 씬 추가");
        
        EditorUtility.DisplayDialog("생성 완료!", 
            "OptionsScene이 성공적으로 생성되었습니다!\n\n" +
            $"경로: {SCENE_PATH}\n\n" +
            "이제 Play 버튼을 눌러 테스트해보세요.", "확인");
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
    
    private static GameObject CreateSettingsManager(Transform parent)
    {
        GameObject obj = new GameObject("SettingsManager");
        obj.transform.SetParent(parent, false);
        obj.AddComponent<SettingsManager>();
        return obj;
    }
    
    private static GameObject CreateOptionMenuUI(Transform parent)
    {
        GameObject mainObj = new GameObject("OptionMenuUI");
        mainObj.transform.SetParent(parent, false);
        
        RectTransform mainRect = mainObj.AddComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = Vector2.zero;
        
        OptionMenuUI optionMenu = mainObj.AddComponent<OptionMenuUI>();
        
        // 배경 생성
        GameObject background = CreateBackground(mainObj.transform);
        
        // 탭 버튼 패널 생성
        GameObject tabPanel = CreateTabPanel(mainObj.transform);
        
        // 3개 패널 생성
        GameObject audioPanel = CreateAudioPanel(mainObj.transform);
        GameObject visualPanel = CreateVisualPanel(mainObj.transform);
        GameObject gameplayPanel = CreateGameplayPanel(mainObj.transform);
        
        // 버튼 패널 생성
        GameObject buttonPanel = CreateButtonPanel(mainObj.transform);
        
        // OptionMenuUI 참조 연결
        ConnectReferences(optionMenu, tabPanel, audioPanel, visualPanel, gameplayPanel, buttonPanel);
        
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
        img.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        return obj;
    }
    
    private static GameObject CreateTabPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "TabPanel", new Vector2(0, 450), new Vector2(1920, 80));
        
        // 가로 레이아웃
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        
        // 3개 탭 버튼
        CreateTabButton(panel.transform, "AudioTabButton", "오디오");
        CreateTabButton(panel.transform, "VisualTabButton", "비주얼");
        CreateTabButton(panel.transform, "GameplayTabButton", "게임플레이");
        
        return panel;
    }
    
    private static GameObject CreateTabButton(Transform parent, string name, string text)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 60);
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        
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
        tmp.color = Color.black;
        
        return obj;
    }
    
    private static GameObject CreateAudioPanel(Transform parent)
    {
        GameObject panel = CreateContentPanel(parent, "AudioPanel", new Vector2(0, 0));
        
        float yPos = 250;
        float spacing = 100;
        
        CreateSliderWithLabel(panel.transform, "MusicVolumeSlider", "MusicVolumeText", "음악 볼륨", 0f, 1f, 0.8f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "SFXVolumeSlider", "SFXVolumeText", "효과음 볼륨", 0f, 1f, 0.8f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "VolumeOffsetSlider", "VolumeOffsetText", "볼륨 오프셋", -200f, 200f, 0f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "JudgmentOffsetSlider", "JudgmentOffsetText", "판정 오프셋", -200f, 200f, 0f, new Vector2(0, yPos));
        
        return panel;
    }
    
    private static GameObject CreateVisualPanel(Transform parent)
    {
        GameObject panel = CreateContentPanel(parent, "VisualPanel", new Vector2(0, 0));
        panel.SetActive(false);
        
        float yPos = 250;
        float spacing = 80;
        
        CreateSliderWithLabel(panel.transform, "NoteSizeSlider", "NoteSizeText", "노트 크기", 0.5f, 3f, 1f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "TrackHeightSlider", "TrackHeightText", "트랙 높이", 5f, 30f, 15f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "TrackAngleSlider", "TrackAngleText", "트랙 각도", -45f, 45f, 0f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "TrackOpacitySlider", "TrackOpacityText", "트랙 투명도", 0.1f, 1f, 0.8f, new Vector2(0, yPos));
        yPos -= spacing;
        
        CreateSliderWithLabel(panel.transform, "NoteScrollSpeedSlider", "NoteScrollSpeedText", "노트 속도", 1f, 20f, 8f, new Vector2(0, yPos));
        
        return panel;
    }
    
    private static GameObject CreateGameplayPanel(Transform parent)
    {
        GameObject panel = CreateContentPanel(parent, "GameplayPanel", new Vector2(0, 0));
        panel.SetActive(false);
        
        float yPos = 200;
        float spacing = 100;
        
        // 판정 모드 드롭다운
        CreateDropdown(panel.transform, "JudgmentModeDropdown", "판정 모드", new Vector2(0, yPos));
        yPos -= spacing;
        
        // 판정 표시 토글
        CreateToggle(panel.transform, "ShowJudgmentToggle", "판정 텍스트 표시 (Perfect, Great 등)", true, new Vector2(0, yPos));
        yPos -= spacing;
        
        // 오프셋 표시 토글
        CreateToggle(panel.transform, "ShowOffsetToggle", "타이밍 오프셋 표시 (+3ms, -5ms 등)", true, new Vector2(0, yPos));
        
        return panel;
    }
    
    private static GameObject CreateButtonPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel(parent, "ButtonPanel", new Vector2(0, -450), new Vector2(1920, 80));
        
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        CreateButton(panel.transform, "ApplyButton", "적용", new Color(0.4f, 0.8f, 0.4f, 1f));
        CreateButton(panel.transform, "ResetButton", "초기화", new Color(0.8f, 0.4f, 0.4f, 1f));
        CreateButton(panel.transform, "BackButton", "뒤로 가기", new Color(0.6f, 0.6f, 0.6f, 1f));
        
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
    
    private static GameObject CreateContentPanel(Transform parent, string name, Vector2 position)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = position;
        
        return obj;
    }
    
    private static void CreateSliderWithLabel(Transform parent, string sliderName, string textName, string label, float min, float max, float value, Vector2 position)
    {
        GameObject container = new GameObject($"{sliderName}_Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(800, 60);
        
        // 라벨
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(0, 0);
        labelRect.sizeDelta = new Vector2(200, 50);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 20;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        
        // 슬라이더
        GameObject sliderObj = new GameObject(sliderName);
        sliderObj.transform.SetParent(container.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.3f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.3f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0, 0);
        sliderRect.sizeDelta = new Vector2(400, 20);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.wholeNumbers = false;
        
        // 슬라이더 배경
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // 슬라이더 Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-20, 0);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.6f, 1f, 1f);
        
        slider.fillRect = fillRect;
        
        // 슬라이더 Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        
        // 값 텍스트
        GameObject valueTextObj = new GameObject(textName);
        valueTextObj.transform.SetParent(container.transform, false);
        
        RectTransform valueTextRect = valueTextObj.AddComponent<RectTransform>();
        valueTextRect.anchorMin = new Vector2(0.8f, 0.5f);
        valueTextRect.anchorMax = new Vector2(0.8f, 0.5f);
        valueTextRect.anchoredPosition = new Vector2(0, 0);
        valueTextRect.sizeDelta = new Vector2(150, 50);
        
        TextMeshProUGUI valueText = valueTextObj.AddComponent<TextMeshProUGUI>();
        valueText.text = value.ToString("F2");
        valueText.fontSize = 20;
        valueText.alignment = TextAlignmentOptions.MidlineLeft;
    }
    
    private static void CreateDropdown(Transform parent, string name, string label, Vector2 position)
    {
        GameObject container = new GameObject($"{name}_Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(800, 60);
        
        // 라벨
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(0, 0);
        labelRect.sizeDelta = new Vector2(200, 50);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 20;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        
        // 드롭다운
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(container.transform, false);
        
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0.3f, 0.5f);
        dropdownRect.anchorMax = new Vector2(0.3f, 0.5f);
        dropdownRect.anchoredPosition = new Vector2(0, 0);
        dropdownRect.sizeDelta = new Vector2(400, 40);
        
        Image dropdownImg = dropdownObj.AddComponent<Image>();
        dropdownImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        
        // Label
        GameObject dropdownLabel = new GameObject("Label");
        dropdownLabel.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform dropdownLabelRect = dropdownLabel.AddComponent<RectTransform>();
        dropdownLabelRect.anchorMin = Vector2.zero;
        dropdownLabelRect.anchorMax = Vector2.one;
        dropdownLabelRect.sizeDelta = new Vector2(-30, 0);
        dropdownLabelRect.anchoredPosition = new Vector2(-5, 0);
        
        TextMeshProUGUI dropdownLabelText = dropdownLabel.AddComponent<TextMeshProUGUI>();
        dropdownLabelText.text = "Normal (일반)";
        dropdownLabelText.fontSize = 18;
        dropdownLabelText.alignment = TextAlignmentOptions.MidlineLeft;
        
        dropdown.captionText = dropdownLabelText;
        
        // Arrow
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform arrowRect = arrow.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-15, 0);
        arrowRect.sizeDelta = new Vector2(20, 20);
        
        Image arrowImg = arrow.AddComponent<Image>();
        arrowImg.color = Color.white;
        
        // Template (드롭다운 리스트)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);
        
        template.SetActive(false);
        
        dropdown.template = templateRect;
        
        // 옵션 추가
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("Normal (일반)"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Hard (어려움)"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Super (최고난이도)"));
    }
    
    private static void CreateToggle(Transform parent, string name, string label, bool isOn, Vector2 position)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(600, 50);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = isOn;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(toggleObj.transform, false);
        
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.anchoredPosition = new Vector2(20, 0);
        bgRect.sizeDelta = new Vector2(40, 40);
        
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        toggle.targetGraphic = bgImg;
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = Vector2.zero;
        
        Image checkImg = checkmark.AddComponent<Image>();
        checkImg.color = new Color(0.3f, 0.8f, 0.3f, 1f);
        
        toggle.graphic = checkImg;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(1, 0.5f);
        labelRect.anchoredPosition = new Vector2(30, 0);
        labelRect.sizeDelta = new Vector2(-80, 50);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 18;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
    }
    
    private static GameObject CreateButton(Transform parent, string name, string text, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 60);
        
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
    
    private static void ConnectReferences(OptionMenuUI optionMenu, GameObject tabPanel, GameObject audioPanel, GameObject visualPanel, GameObject gameplayPanel, GameObject buttonPanel)
    {
        // 패널 참조
        optionMenu.audioPanel = audioPanel;
        optionMenu.visualPanel = visualPanel;
        optionMenu.gameplayPanel = gameplayPanel;
        
        // 탭 버튼
        optionMenu.audioTabButton = tabPanel.transform.Find("AudioTabButton").GetComponent<Button>();
        optionMenu.visualTabButton = tabPanel.transform.Find("VisualTabButton").GetComponent<Button>();
        optionMenu.gameplayTabButton = tabPanel.transform.Find("GameplayTabButton").GetComponent<Button>();
        
        // 오디오 설정
        optionMenu.musicVolumeSlider = audioPanel.transform.Find("MusicVolumeSlider_Container/MusicVolumeSlider").GetComponent<Slider>();
        optionMenu.musicVolumeText = audioPanel.transform.Find("MusicVolumeSlider_Container/MusicVolumeText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.sfxVolumeSlider = audioPanel.transform.Find("SFXVolumeSlider_Container/SFXVolumeSlider").GetComponent<Slider>();
        optionMenu.sfxVolumeText = audioPanel.transform.Find("SFXVolumeSlider_Container/SFXVolumeText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.volumeOffsetSlider = audioPanel.transform.Find("VolumeOffsetSlider_Container/VolumeOffsetSlider").GetComponent<Slider>();
        optionMenu.volumeOffsetText = audioPanel.transform.Find("VolumeOffsetSlider_Container/VolumeOffsetText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.judgmentOffsetSlider = audioPanel.transform.Find("JudgmentOffsetSlider_Container/JudgmentOffsetSlider").GetComponent<Slider>();
        optionMenu.judgmentOffsetText = audioPanel.transform.Find("JudgmentOffsetSlider_Container/JudgmentOffsetText").GetComponent<TextMeshProUGUI>();
        
        // 비주얼 설정
        optionMenu.noteSizeSlider = visualPanel.transform.Find("NoteSizeSlider_Container/NoteSizeSlider").GetComponent<Slider>();
        optionMenu.noteSizeText = visualPanel.transform.Find("NoteSizeSlider_Container/NoteSizeText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.trackHeightSlider = visualPanel.transform.Find("TrackHeightSlider_Container/TrackHeightSlider").GetComponent<Slider>();
        optionMenu.trackHeightText = visualPanel.transform.Find("TrackHeightSlider_Container/TrackHeightText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.trackAngleSlider = visualPanel.transform.Find("TrackAngleSlider_Container/TrackAngleSlider").GetComponent<Slider>();
        optionMenu.trackAngleText = visualPanel.transform.Find("TrackAngleSlider_Container/TrackAngleText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.trackOpacitySlider = visualPanel.transform.Find("TrackOpacitySlider_Container/TrackOpacitySlider").GetComponent<Slider>();
        optionMenu.trackOpacityText = visualPanel.transform.Find("TrackOpacitySlider_Container/TrackOpacityText").GetComponent<TextMeshProUGUI>();
        
        optionMenu.noteScrollSpeedSlider = visualPanel.transform.Find("NoteScrollSpeedSlider_Container/NoteScrollSpeedSlider").GetComponent<Slider>();
        optionMenu.noteScrollSpeedText = visualPanel.transform.Find("NoteScrollSpeedSlider_Container/NoteScrollSpeedText").GetComponent<TextMeshProUGUI>();
        
        // 게임플레이 설정
        optionMenu.judgmentModeDropdown = gameplayPanel.transform.Find("JudgmentModeDropdown_Container/JudgmentModeDropdown").GetComponent<TMP_Dropdown>();
        optionMenu.showJudgmentToggle = gameplayPanel.transform.Find("ShowJudgmentToggle").GetComponent<Toggle>();
        optionMenu.showOffsetToggle = gameplayPanel.transform.Find("ShowOffsetToggle").GetComponent<Toggle>();
        
        // 버튼
        optionMenu.applyButton = buttonPanel.transform.Find("ApplyButton").GetComponent<Button>();
        optionMenu.resetButton = buttonPanel.transform.Find("ResetButton").GetComponent<Button>();
        optionMenu.backButton = buttonPanel.transform.Find("BackButton").GetComponent<Button>();
        
        EditorUtility.SetDirty(optionMenu);
    }
}
