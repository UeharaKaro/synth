using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// SongListItem Prefab을 자동으로 생성하는 독립 Editor Script
/// Tools → Create SongListItem Prefab 메뉴로 실행
/// </summary>
public class CreateSongListItemPrefab : EditorWindow
{
    private string prefabName = "SongListItem";
    private string savePath = "Assets/songselect/";
    private bool overwriteExisting = false;
    
    [MenuItem("Tools/Create SongListItem Prefab")]
    public static void ShowWindow()
    {
        GetWindow<CreateSongListItemPrefab>("Create Prefab");
    }
    
    void OnGUI()
    {
        GUILayout.Label("SongListItem Prefab 자동 생성", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "곡 목록에 사용될 SongListItem Prefab을 자동으로 생성합니다.\n" +
            "SongSelectionUIAdvanced와 함께 사용됩니다.", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        prefabName = EditorGUILayout.TextField("Prefab 이름", prefabName);
        savePath = EditorGUILayout.TextField("저장 경로", savePath);
        overwriteExisting = EditorGUILayout.Toggle("기존 파일 덮어쓰기", overwriteExisting);
        
        GUILayout.Space(10);
        
        // 프리뷰 정보
        EditorGUILayout.LabelField("생성될 파일:", $"{savePath}{prefabName}.prefab");
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🎨 Prefab 생성", GUILayout.Height(40)))
        {
            string fullPath = $"{savePath}{prefabName}.prefab";
            
            // 기존 파일 확인
            if (!overwriteExisting && System.IO.File.Exists(fullPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "파일 존재", 
                    $"{fullPath} 파일이 이미 존재합니다.\n덮어쓰시겠습니까?", 
                    "덮어쓰기", "취소"))
                {
                    return;
                }
            }
            
            CreatePrefab();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("📁 저장 위치 열기"))
        {
            string fullPath = System.IO.Path.GetFullPath(savePath);
            if (System.IO.Directory.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
            }
            else
            {
                EditorUtility.DisplayDialog("폴더 없음", "저장 경로가 존재하지 않습니다.", "확인");
            }
        }
    }
    
    private void CreatePrefab()
    {
        Debug.Log("🎨 SongListItem Prefab 생성 시작...");
        
        // 1. 루트 GameObject 생성
        GameObject prefabRoot = new GameObject(prefabName);
        
        RectTransform rootRect = prefabRoot.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(780, 100);
        
        LayoutElement rootLayout = prefabRoot.AddComponent<LayoutElement>();
        rootLayout.minHeight = 100;
        rootLayout.preferredHeight = 100;
        
        // 2. Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(prefabRoot.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = new Color(1, 1, 1, 0.1f);
        
        // 3. Selection Indicator
        GameObject indicator = new GameObject("SelectionIndicator");
        indicator.transform.SetParent(prefabRoot.transform, false);
        RectTransform indicatorRect = indicator.AddComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0, 0);
        indicatorRect.anchorMax = new Vector2(0, 1);
        indicatorRect.pivot = new Vector2(0, 0.5f);
        indicatorRect.sizeDelta = new Vector2(5, 0);
        indicatorRect.anchoredPosition = Vector2.zero;
        Image indicatorImg = indicator.AddComponent<Image>();
        indicatorImg.color = Color.yellow;
        indicator.SetActive(false); // 기본 비활성화
        
        // 4. Thumbnail
        GameObject thumbnail = new GameObject("ThumbnailImage");
        thumbnail.transform.SetParent(prefabRoot.transform, false);
        RectTransform thumbRect = thumbnail.AddComponent<RectTransform>();
        thumbRect.anchorMin = new Vector2(0, 0.5f);
        thumbRect.anchorMax = new Vector2(0, 0.5f);
        thumbRect.pivot = new Vector2(0, 0.5f);
        thumbRect.sizeDelta = new Vector2(80, 80);
        thumbRect.anchoredPosition = new Vector2(15, 0);
        Image thumbImg = thumbnail.AddComponent<Image>();
        thumbImg.preserveAspect = true;
        thumbImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // 5. Info Container
        GameObject infoContainer = new GameObject("InfoContainer");
        infoContainer.transform.SetParent(prefabRoot.transform, false);
        RectTransform infoRect = infoContainer.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.offsetMin = new Vector2(105, 10);
        infoRect.offsetMax = new Vector2(-150, -10);
        
        VerticalLayoutGroup infoLayout = infoContainer.AddComponent<VerticalLayoutGroup>();
        infoLayout.spacing = 5;
        infoLayout.childAlignment = TextAnchor.UpperLeft;
        infoLayout.childControlWidth = true;
        infoLayout.childControlHeight = false;
        infoLayout.childForceExpandWidth = true;
        infoLayout.childForceExpandHeight = false;
        
        // 6. Info Texts
        GameObject titleText = CreateText(infoContainer.transform, "TitleText", "Song Title", 20, true);
        GameObject artistText = CreateText(infoContainer.transform, "ArtistText", "Artist Name", 16, false);
        artistText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
        GameObject bpmText = CreateText(infoContainer.transform, "BPMText", "BPM: 120", 14, false);
        bpmText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f, 1f);
        
        // 7. Level Range Text
        GameObject levelText = CreateText(prefabRoot.transform, "LevelRangeText", "Lv. 1~10", 16, false);
        RectTransform levelRect = levelText.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(1, 1);
        levelRect.anchorMax = new Vector2(1, 1);
        levelRect.pivot = new Vector2(1, 1);
        levelRect.sizeDelta = new Vector2(100, 30);
        levelRect.anchoredPosition = new Vector2(-10, -10);
        levelText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        levelText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        
        // 8. Icons Container
        GameObject iconsContainer = new GameObject("IconsContainer");
        iconsContainer.transform.SetParent(prefabRoot.transform, false);
        RectTransform iconsRect = iconsContainer.AddComponent<RectTransform>();
        iconsRect.anchorMin = new Vector2(1, 0);
        iconsRect.anchorMax = new Vector2(1, 0);
        iconsRect.pivot = new Vector2(1, 0);
        iconsRect.sizeDelta = new Vector2(120, 30);
        iconsRect.anchoredPosition = new Vector2(-10, 10);
        
        HorizontalLayoutGroup iconsLayout = iconsContainer.AddComponent<HorizontalLayoutGroup>();
        iconsLayout.spacing = 5;
        iconsLayout.childAlignment = TextAnchor.MiddleCenter;
        
        // 9. Icons (4개)
        GameObject favoriteIcon = CreateIcon(iconsContainer.transform, "FavoriteIcon", Color.yellow);
        GameObject lockIcon = CreateIcon(iconsContainer.transform, "LockIcon", Color.red);
        GameObject clearedBadge = CreateIcon(iconsContainer.transform, "ClearedBadge", Color.green);
        GameObject newBadge = CreateIcon(iconsContainer.transform, "NewBadge", Color.cyan);
        
        // 모든 아이콘 기본 비활성화
        favoriteIcon.SetActive(false);
        lockIcon.SetActive(false);
        clearedBadge.SetActive(false);
        newBadge.SetActive(false);
        
        // 10. Button 컴포넌트
        Button button = prefabRoot.AddComponent<Button>();
        button.targetGraphic = bgImg;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 0);
        colors.highlightedColor = new Color(1, 1, 1, 0.2f);
        colors.pressedColor = new Color(1, 1, 1, 0.4f);
        colors.selectedColor = new Color(1, 1, 1, 0.2f);
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        
        // 11. SongListItem 스크립트 추가
        SongListItem itemScript = prefabRoot.AddComponent<SongListItem>();
        
        // 스크립트 참조 자동 연결
        itemScript.titleText = titleText.GetComponent<TextMeshProUGUI>();
        itemScript.artistText = artistText.GetComponent<TextMeshProUGUI>();
        itemScript.bpmText = bpmText.GetComponent<TextMeshProUGUI>();
        itemScript.levelRangeText = levelText.GetComponent<TextMeshProUGUI>();
        itemScript.thumbnailImage = thumbImg;
        // backgroundImage는 private이므로 Awake에서 자동으로 GetComponent 됨
        itemScript.selectionIndicator = indicatorImg;
        itemScript.favoriteIcon = favoriteIcon;
        itemScript.lockIcon = lockIcon;
        itemScript.clearedBadge = clearedBadge;
        itemScript.newBadge = newBadge;
        
        // 12. Prefab으로 저장
        string prefabPath = $"{savePath}{prefabName}.prefab";
        
        // 폴더 확인 및 생성
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            string[] folders = directory.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = nextPath;
            }
        }
        
        // Prefab 생성
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        
        if (savedPrefab != null)
        {
            Debug.Log($"✅ SongListItem Prefab 생성 완료: {prefabPath}");
            
            EditorUtility.DisplayDialog(
                "완료!", 
                $"Prefab이 생성되었습니다!\n\n경로: {prefabPath}\n\n" +
                "이제 SongSelectionUIAdvanced의 Inspector에서\n" +
                "'Song List Item Prefab' 필드에 이 Prefab을 연결하세요.", 
                "확인");
            
            // Project 창에서 선택
            Selection.activeObject = savedPrefab;
            EditorGUIUtility.PingObject(savedPrefab);
        }
        else
        {
            Debug.LogError("❌ Prefab 생성 실패!");
            EditorUtility.DisplayDialog("오류", "Prefab 생성에 실패했습니다.", "확인");
        }
        
        // 씬에서 임시 오브젝트 삭제
        DestroyImmediate(prefabRoot);
    }
    
    private GameObject CreateText(Transform parent, string name, string text, int fontSize, bool bold)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        TextMeshProUGUI textComp = obj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.Left;
        
        return obj;
    }
    
    private GameObject CreateIcon(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(24, 24);
        
        Image img = obj.AddComponent<Image>();
        img.color = color;
        
        LayoutElement layout = obj.AddComponent<LayoutElement>();
        layout.preferredWidth = 24;
        layout.preferredHeight = 24;
        
        return obj;
    }
}
