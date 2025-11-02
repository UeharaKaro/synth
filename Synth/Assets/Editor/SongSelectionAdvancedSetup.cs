using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// SongSelection 씬에 Advanced UI를 자동으로 생성하는 Editor Script
/// Tools → Setup Song Selection Advanced 메뉴로 실행
/// </summary>
public class SongSelectionAdvancedSetup : EditorWindow
{
    private bool backupScene = true;
    private bool createSongListItem = true;
    
    [MenuItem("Tools/Setup Song Selection Advanced")]
    public static void ShowWindow()
    {
        GetWindow<SongSelectionAdvancedSetup>("Song Selection Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("고급 곡 선택 UI 자동 생성", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "현재 씬에 SongSelectionUIAdvanced 구조를 자동으로 생성합니다.\n" +
            "SongSelectionScene.unity를 열고 실행하세요.", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        backupScene = EditorGUILayout.Toggle("씬 백업 생성", backupScene);
        createSongListItem = EditorGUILayout.Toggle("SongListItem Prefab 생성", createSongListItem);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🚀 UI 자동 생성 시작", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "확인", 
                "현재 씬에 고급 UI를 생성합니다. 계속하시겠습니까?", 
                "예", "아니오"))
            {
                SetupSongSelectionAdvanced();
            }
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("📝 수동 설정 가이드 열기"))
        {
            string guidePath = Path.Combine(Application.dataPath, "..", "SONGSELECTION_UPGRADE_GUIDE.md");
            if (File.Exists(guidePath))
            {
                Application.OpenURL("file://" + guidePath);
            }
            else
            {
                EditorUtility.DisplayDialog("파일 없음", "SONGSELECTION_UPGRADE_GUIDE.md 파일을 찾을 수 없습니다.", "확인");
            }
        }
    }
    
    private void SetupSongSelectionAdvanced()
    {
        // 1. 씬 백업
        if (backupScene)
        {
            string scenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(scenePath))
            {
                string backupPath = scenePath.Replace(".unity", "_Backup.unity");
                AssetDatabase.CopyAsset(scenePath, backupPath);
                Debug.Log($"✅ 씬 백업 생성: {backupPath}");
            }
        }
        
        // 2. Canvas 찾기 또는 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Canvas Scaler 설정
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        // 3. EventSystem 확인
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // 4. 기존 UI 정리 (비활성화)
        foreach (Transform child in canvas.transform)
        {
            if (child.name != "Background" && child.name != "EventSystem")
            {
                child.gameObject.SetActive(false);
            }
        }
        
        // 5. 메인 컨테이너 생성
        GameObject mainContainer = CreateMainContainer(canvas.transform);
        
        // 6. 왼쪽 패널 (곡 목록)
        GameObject leftPanel = CreateLeftPanel(mainContainer.transform);
        GameObject filterPanel = CreateFilterPanel(leftPanel.transform);
        GameObject scrollView = CreateScrollView(leftPanel.transform);
        
        // 7. 오른쪽 패널 (상세 정보)
        GameObject rightPanel = CreateRightPanel(mainContainer.transform);
        CreateAlbumArtPanel(rightPanel.transform);
        CreateSongInfoPanel(rightPanel.transform);
        CreateDifficultyPanel(rightPanel.transform);
        CreateKeyModePanel(rightPanel.transform);
        CreateHighScorePanel(rightPanel.transform);
        CreateActionPanel(rightPanel.transform);
        
        // 8. SongListItem Prefab 생성
        if (createSongListItem)
        {
            CreateSongListItemPrefab();
        }
        
        // 9. SongSelectionUIAdvanced 컴포넌트 추가 및 연결
        SetupSongSelectionComponent(canvas.gameObject);
        
        // 10. 완료
        EditorUtility.DisplayDialog(
            "완료!", 
            "고급 곡 선택 UI가 생성되었습니다!\n\n" +
            "다음 단계:\n" +
            "1. Canvas의 SongSelectionUIAdvanced Inspector 확인\n" +
            "2. SongDatabase 연결\n" +
            "3. Play 모드로 테스트\n\n" +
            "상세 가이드: SONGSELECTION_UPGRADE_GUIDE.md", 
            "확인");
        
        Debug.Log("✅ SongSelection Advanced UI 생성 완료!");
    }
    
    private GameObject CreateMainContainer(Transform parent)
    {
        GameObject obj = new GameObject("MainContainer");
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, -100);
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        
        HorizontalLayoutGroup layout = obj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        
        return obj;
    }
    
    private GameObject CreateLeftPanel(Transform parent)
    {
        GameObject obj = new GameObject("LeftPanel");
        obj.transform.SetParent(parent, false);
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        LayoutElement layout = obj.AddComponent<LayoutElement>();
        layout.preferredWidth = 800;
        layout.flexibleWidth = 0;
        
        return obj;
    }
    
    private GameObject CreateFilterPanel(Transform parent)
    {
        GameObject obj = new GameObject("FilterPanel");
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 250);
        rect.anchoredPosition = Vector2.zero;
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        VerticalLayoutGroup layout = obj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        // 검색 바
        CreateSearchInputField(obj.transform);
        
        // 검색 버튼들
        CreateSearchButtons(obj.transform);
        
        // 정렬 드롭다운
        CreateSortDropdown(obj.transform);
        
        // 정렬 순서 토글
        CreateSortToggle(obj.transform);
        
        // 필터 드롭다운들
        CreateFilterDropdowns(obj.transform);
        
        // 레벨 슬라이더
        CreateLevelSliders(obj.transform);
        
        // 토글들
        CreateFilterToggles(obj.transform);
        
        return obj;
    }
    
    private void CreateSearchInputField(Transform parent)
    {
        GameObject obj = new GameObject("SearchInputField");
        obj.transform.SetParent(parent, false);
        
        TMP_InputField inputField = obj.AddComponent<TMP_InputField>();
        
        // Text Area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(obj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = Vector2.zero;
        textArea.AddComponent<RectMask2D>();
        
        // Text
        GameObject text = new GameObject("Text");
        text.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI textComp = text.AddComponent<TextMeshProUGUI>();
        textComp.text = "";
        textComp.fontSize = 18;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Placeholder
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholderComp = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderComp.text = "곡 제목, 아티스트, 장르 검색...";
        placeholderComp.fontSize = 18;
        placeholderComp.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        
        inputField.textViewport = textAreaRect;
        inputField.textComponent = textComp;
        inputField.placeholder = placeholderComp;
        inputField.characterLimit = 50;
        
        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        LayoutElement layoutElem = obj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 40;
        layoutElem.preferredHeight = 40;
    }
    
    private void CreateSearchButtons(Transform parent)
    {
        GameObject container = new GameObject("SearchButtonsContainer");
        container.transform.SetParent(parent, false);
        
        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childForceExpandWidth = true;
        
        LayoutElement layoutElem = container.AddComponent<LayoutElement>();
        layoutElem.minHeight = 40;
        
        // 검색 버튼
        CreateButton(container.transform, "SearchButton", "🔍 검색");
        
        // 초기화 버튼
        CreateButton(container.transform, "ClearSearchButton", "✖ 초기화");
    }
    
    private GameObject CreateButton(Transform parent, string name, string text)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        Button button = obj.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 18;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return obj;
    }
    
    private void CreateSortDropdown(Transform parent)
    {
        GameObject obj = new GameObject("SortDropdown");
        obj.transform.SetParent(parent, false);
        
        TMP_Dropdown dropdown = obj.AddComponent<TMP_Dropdown>();
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("제목"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("아티스트"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("BPM"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("레벨"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("플레이 횟수"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("최고 점수"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("추가 날짜"));
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        LayoutElement layoutElem = obj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 35;
        
        // Label, Arrow, Template 등은 자동 생성됨
    }
    
    private void CreateSortToggle(Transform parent)
    {
        GameObject obj = new GameObject("SortOrderToggle");
        obj.transform.SetParent(parent, false);
        
        Toggle toggle = obj.AddComponent<Toggle>();
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(obj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.sizeDelta = new Vector2(40, 0);
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        Image checkImg = checkmark.AddComponent<Image>();
        checkImg.color = Color.green;
        RectTransform checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.sizeDelta = new Vector2(20, 20);
        
        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(obj.transform, false);
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "▲ 오름차순";
        labelText.fontSize = 16;
        labelText.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(50, 0);
        labelRect.offsetMax = new Vector2(0, 0);
        
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        
        LayoutElement layoutElem = obj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 30;
    }
    
    private void CreateFilterDropdowns(Transform parent)
    {
        // 난이도 필터
        GameObject diffDropdown = new GameObject("DifficultyFilterDropdown");
        diffDropdown.transform.SetParent(parent, false);
        TMP_Dropdown dd1 = diffDropdown.AddComponent<TMP_Dropdown>();
        dd1.options.Clear();
        dd1.options.Add(new TMP_Dropdown.OptionData("난이도: 전체"));
        dd1.options.Add(new TMP_Dropdown.OptionData("Easy"));
        dd1.options.Add(new TMP_Dropdown.OptionData("Normal"));
        dd1.options.Add(new TMP_Dropdown.OptionData("Hard"));
        dd1.options.Add(new TMP_Dropdown.OptionData("Expert"));
        dd1.options.Add(new TMP_Dropdown.OptionData("Master"));
        dd1.options.Add(new TMP_Dropdown.OptionData("Special"));
        diffDropdown.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        diffDropdown.AddComponent<LayoutElement>().minHeight = 35;
        
        // 키 모드 필터
        GameObject keyDropdown = new GameObject("KeyModeFilterDropdown");
        keyDropdown.transform.SetParent(parent, false);
        TMP_Dropdown dd2 = keyDropdown.AddComponent<TMP_Dropdown>();
        dd2.options.Clear();
        dd2.options.Add(new TMP_Dropdown.OptionData("키 모드: 전체"));
        dd2.options.Add(new TMP_Dropdown.OptionData("4K"));
        dd2.options.Add(new TMP_Dropdown.OptionData("5K"));
        dd2.options.Add(new TMP_Dropdown.OptionData("6K"));
        dd2.options.Add(new TMP_Dropdown.OptionData("7K"));
        dd2.options.Add(new TMP_Dropdown.OptionData("8K"));
        dd2.options.Add(new TMP_Dropdown.OptionData("10K"));
        keyDropdown.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        keyDropdown.AddComponent<LayoutElement>().minHeight = 35;
    }
    
    private void CreateLevelSliders(Transform parent)
    {
        GameObject container = new GameObject("LevelFilterContainer");
        container.transform.SetParent(parent, false);
        
        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        
        LayoutElement layoutElem = container.AddComponent<LayoutElement>();
        layoutElem.minHeight = 40;
        
        // Min 슬라이더
        CreateSlider(container.transform, "MinLevelSlider", 1, 20, 1);
        CreateText(container.transform, "MinLevelText", "Min: 1", 80);
        
        // Max 슬라이더
        CreateSlider(container.transform, "MaxLevelSlider", 1, 20, 20);
        CreateText(container.transform, "MaxLevelText", "Max: 20", 80);
    }
    
    private void CreateSlider(Transform parent, string name, float min, float max, float value)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        Slider slider = obj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.wholeNumbers = true;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(obj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.sizeDelta = Vector2.zero;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.sizeDelta = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        
        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(obj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);
        handleAreaRect.sizeDelta = Vector2.zero;
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
    }
    
    private GameObject CreateText(Transform parent, string name, string text, float minWidth = 0)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        TextMeshProUGUI textComp = obj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 14;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;
        
        if (minWidth > 0)
        {
            LayoutElement layoutElem = obj.AddComponent<LayoutElement>();
            layoutElem.minWidth = minWidth;
        }
        
        return obj;
    }
    
    private void CreateFilterToggles(Transform parent)
    {
        CreateSimpleToggle(parent, "FavoritesOnlyToggle", "⭐ 즐겨찾기만");
        CreateSimpleToggle(parent, "ClearedOnlyToggle", "✅ 클리어한 곡만");
    }
    
    private void CreateSimpleToggle(Transform parent, string name, string labelText)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        Toggle toggle = obj.AddComponent<Toggle>();
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(obj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.sizeDelta = new Vector2(40, 0);
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        Image checkImg = checkmark.AddComponent<Image>();
        checkImg.color = Color.green;
        RectTransform checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.sizeDelta = new Vector2(20, 20);
        
        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(obj.transform, false);
        TextMeshProUGUI labelComp = label.AddComponent<TextMeshProUGUI>();
        labelComp.text = labelText;
        labelComp.fontSize = 16;
        labelComp.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(50, 0);
        labelRect.offsetMax = new Vector2(0, 0);
        
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        
        LayoutElement layoutElem = obj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 30;
    }
    
    private GameObject CreateScrollView(Transform parent)
    {
        GameObject obj = new GameObject("SongListScrollView");
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, -250);
        
        ScrollRect scrollRect = obj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30;
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(obj.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        
        return obj;
    }
    
    private GameObject CreateRightPanel(Transform parent)
    {
        GameObject obj = new GameObject("RightPanel");
        obj.transform.SetParent(parent, false);
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        LayoutElement layout = obj.AddComponent<LayoutElement>();
        layout.flexibleWidth = 1;
        
        return obj;
    }
    
    private void CreateAlbumArtPanel(Transform parent)
    {
        GameObject panel = new GameObject("AlbumArtPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(400, 400);
        rect.anchoredPosition = new Vector2(0, -20);
        
        panel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Album Art Image
        GameObject img = new GameObject("AlbumArtImage");
        img.transform.SetParent(panel.transform, false);
        RectTransform imgRect = img.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = new Vector2(10, 10);
        imgRect.offsetMax = new Vector2(-10, -10);
        Image albumImg = img.AddComponent<Image>();
        albumImg.preserveAspect = true;
        
        // Favorite Button
        GameObject btn = CreateButton(panel.transform, "FavoriteToggleButton", "★");
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.sizeDelta = new Vector2(50, 50);
        btnRect.anchoredPosition = new Vector2(-10, -10);
        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 36;
        btn.GetComponent<Image>().color = new Color(1, 1, 0, 0.3f);
    }
    
    private void CreateSongInfoPanel(Transform parent)
    {
        GameObject panel = new GameObject("SongInfoPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(-40, 200);
        rect.anchoredPosition = new Vector2(0, -440);
        
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.padding = new RectOffset(10, 10, 10, 10);
        
        CreateText(panel.transform, "SongTitleText", "곡 제목").GetComponent<TextMeshProUGUI>().fontSize = 32;
        CreateText(panel.transform, "ArtistText", "아티스트").GetComponent<TextMeshProUGUI>().fontSize = 24;
        CreateText(panel.transform, "BPMText", "BPM: 120").GetComponent<TextMeshProUGUI>().fontSize = 18;
        CreateText(panel.transform, "SongLengthText", "길이: 3:00").GetComponent<TextMeshProUGUI>().fontSize = 18;
        CreateText(panel.transform, "GenreText", "장르: Electronic").GetComponent<TextMeshProUGUI>().fontSize = 16;
        CreateText(panel.transform, "DescriptionText", "곡 설명").GetComponent<TextMeshProUGUI>().fontSize = 14;
    }
    
    private void CreateDifficultyPanel(Transform parent)
    {
        GameObject panel = new GameObject("DifficultyPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(-40, 60);
        rect.anchoredPosition = new Vector2(0, -660);
        
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        
        CreateButton(panel.transform, "PreviousDifficultyButton", "◀");
        CreateText(panel.transform, "DifficultyText", "Normal");
        CreateButton(panel.transform, "NextDifficultyButton", "▶");
        CreateText(panel.transform, "DifficultyLevelText", "Lv. 5");
        CreateText(panel.transform, "TotalNotesText", "350 Notes");
        
        GameObject indicator = new GameObject("DifficultyIndicatorImage");
        indicator.transform.SetParent(panel.transform, false);
        RectTransform indRect = indicator.AddComponent<RectTransform>();
        indRect.sizeDelta = new Vector2(30, 30);
        indicator.AddComponent<Image>().color = Color.yellow;
    }
    
    private void CreateKeyModePanel(Transform parent)
    {
        GameObject panel = new GameObject("KeyModePanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(-40, 50);
        rect.anchoredPosition = new Vector2(0, -740);
        
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        CreateButton(panel.transform, "PreviousKeyCountButton", "◀");
        GameObject keyText = CreateText(panel.transform, "KeyCountText", "4K");
        keyText.GetComponent<TextMeshProUGUI>().fontSize = 28;
        CreateButton(panel.transform, "NextKeyCountButton", "▶");
    }
    
    private void CreateHighScorePanel(Transform parent)
    {
        GameObject panel = new GameObject("HighScorePanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(-40, 120);
        rect.anchoredPosition = new Vector2(0, -810);
        
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.padding = new RectOffset(10, 10, 10, 10);
        
        CreateText(panel.transform, "HighScoreText", "최고 점수: 0");
        CreateText(panel.transform, "HighRankText", "등급: -");
        CreateText(panel.transform, "PlayCountText", "플레이: 0회");
        CreateText(panel.transform, "ClearStatusText", "NOT CLEARED");
    }
    
    private void CreateActionPanel(Transform parent)
    {
        GameObject panel = new GameObject("ActionPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(-40, 80);
        rect.anchoredPosition = new Vector2(0, 20);
        
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        
        GameObject startBtn = CreateButton(panel.transform, "SelectSongButton", "🎮 게임 시작");
        startBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 24;
        startBtn.GetComponent<Image>().color = new Color(0, 0.5f, 0, 1f);
        
        CreateButton(panel.transform, "PreviewButton", "🎵 미리듣기");
        CreateButton(panel.transform, "BackButton", "← 뒤로");
    }
    
    private void CreateSongListItemPrefab()
    {
        // SongListItem Prefab 생성 로직은 복잡하므로
        // 여기서는 스킵하고 수동으로 생성하도록 안내
        Debug.Log("⚠️ SongListItem Prefab은 수동으로 생성하세요. 가이드 참조: SONGSELECTION_UPGRADE_GUIDE.md");
    }
    
    private void SetupSongSelectionComponent(GameObject canvas)
    {
        // 기존 SongSelectionUI 제거
        var oldUI = canvas.GetComponent<SongSelectionUI>();
        if (oldUI != null)
        {
            DestroyImmediate(oldUI);
            Debug.Log("✅ 기존 SongSelectionUI 제거됨");
        }
        
        // SongSelectionUIAdvanced 추가
        var advancedUI = canvas.GetComponent<SongSelectionUIAdvanced>();
        if (advancedUI == null)
        {
            advancedUI = canvas.AddComponent<SongSelectionUIAdvanced>();
            Debug.Log("✅ SongSelectionUIAdvanced 컴포넌트 추가됨");
        }
        
        // 자동 참조 연결
        AutoConnectReferences(advancedUI, canvas.transform);
        
        EditorUtility.SetDirty(canvas);
    }
    
    private void AutoConnectReferences(SongSelectionUIAdvanced ui, Transform root)
    {
        // 검색 및 자동 연결
        ui.songListScrollView = FindComponent<ScrollRect>(root, "SongListScrollView");
        ui.songListContent = FindTransform(root, "Content");
        
        ui.searchInputField = FindComponent<TMP_InputField>(root, "SearchInputField");
        ui.sortDropdown = FindComponent<TMP_Dropdown>(root, "SortDropdown");
        ui.sortOrderToggle = FindComponent<Toggle>(root, "SortOrderToggle");
        
        ui.songTitleText = FindComponent<TextMeshProUGUI>(root, "SongTitleText");
        ui.artistText = FindComponent<TextMeshProUGUI>(root, "ArtistText");
        ui.bpmText = FindComponent<TextMeshProUGUI>(root, "BPMText");
        ui.songLengthText = FindComponent<TextMeshProUGUI>(root, "SongLengthText");
        ui.genreText = FindComponent<TextMeshProUGUI>(root, "GenreText");
        ui.descriptionText = FindComponent<TextMeshProUGUI>(root, "DescriptionText");
        
        ui.difficultyText = FindComponent<TextMeshProUGUI>(root, "DifficultyText");
        ui.difficultyLevelText = FindComponent<TextMeshProUGUI>(root, "DifficultyLevelText");
        ui.totalNotesText = FindComponent<TextMeshProUGUI>(root, "TotalNotesText");
        ui.keyCountText = FindComponent<TextMeshProUGUI>(root, "KeyCountText");
        
        ui.highScoreText = FindComponent<TextMeshProUGUI>(root, "HighScoreText");
        ui.highRankText = FindComponent<TextMeshProUGUI>(root, "HighRankText");
        ui.playCountText = FindComponent<TextMeshProUGUI>(root, "PlayCountText");
        ui.clearStatusText = FindComponent<TextMeshProUGUI>(root, "ClearStatusText");
        
        ui.albumArtImage = FindComponent<Image>(root, "AlbumArtImage");
        ui.difficultyIndicatorImage = FindComponent<Image>(root, "DifficultyIndicatorImage");
        
        Debug.Log("✅ 참조 자동 연결 완료 (일부는 수동 연결 필요)");
    }
    
    private T FindComponent<T>(Transform root, string name) where T : Component
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }
        return null;
    }
    
    private RectTransform FindTransform(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child.GetComponent<RectTransform>();
        }
        return null;
    }
}
