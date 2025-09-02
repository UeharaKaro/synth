using UnityEngine;

/// <summary>
/// Script to help create the necessary prefabs for ChartEditorBeta
/// Attach this to a GameObject and run the CreatePrefabs method
/// </summary>
public class ChartEditorBetaPrefabCreator : MonoBehaviour
{
    [Header("Prefab Creation")]
    public Material noteMaterial;
    public Material longNoteMaterial;
    public Material gridLineMaterial;
    
    [Header("Note Settings")]
    public Color defaultNoteColor = Color.white;
    public float noteSize = 0.5f;
    
    [Header("Paths")]
    public string prefabSavePath = "Assets/Prefabs/";
    
    #if UNITY_EDITOR
    void Start()
    {
        // Auto-create prefabs on start for convenience
        if (Application.isPlaying)
        {
            CreatePrefabs();
        }
    }
    
    [ContextMenu("Create All Prefabs")]
    public void CreatePrefabs()
    {
        CreateNotePrefab();
        CreateLongNotePrefab();
        CreateEditorUIPrefab();
        Debug.Log("All ChartEditorBeta prefabs created!");
    }
    
    void CreateNotePrefab()
    {
        // Create regular note prefab
        GameObject noteObj = new GameObject("NoteBetaPrefab");
        
        // Add SpriteRenderer
        SpriteRenderer sr = noteObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateNoteSprite();
        sr.material = noteMaterial;
        sr.color = defaultNoteColor;
        
        // Add Collider for mouse interaction
        BoxCollider2D collider = noteObj.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one * noteSize;
        
        // Add NoteBeta component
        NoteBeta noteBeta = noteObj.AddComponent<NoteBeta>();
        noteBeta.noteRenderer = sr;
        
        // Save as prefab (in runtime, you would save this manually)
        Debug.Log($"Note prefab created: {noteObj.name}");
    }
    
    void CreateLongNotePrefab()
    {
        // Create long note prefab
        GameObject longNoteObj = new GameObject("LongNoteBetaPrefab");
        
        // Add SpriteRenderer
        SpriteRenderer sr = longNoteObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateLongNoteSprite();
        sr.material = longNoteMaterial;
        sr.color = defaultNoteColor;
        
        // Add Collider for mouse interaction
        BoxCollider2D collider = longNoteObj.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one * noteSize;
        
        // Add LineRenderer for long note trail
        LineRenderer lr = longNoteObj.AddComponent<LineRenderer>();
        lr.material = longNoteMaterial;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = 2;
        
        // Add NoteBeta component
        NoteBeta noteBeta = longNoteObj.AddComponent<NoteBeta>();
        noteBeta.noteRenderer = sr;
        noteBeta.longNoteTrail = lr;
        noteBeta.isLongNote = true;
        
        Debug.Log($"Long note prefab created: {longNoteObj.name}");
    }
    
    void CreateEditorUIPrefab()
    {
        // Create editor UI prefab
        GameObject uiObj = new GameObject("EditorBetaUI");
        
        // Add Canvas
        Canvas canvas = uiObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler
        UnityEngine.UI.CanvasScaler scaler = uiObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        uiObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create main panel
        GameObject panel = CreateUIPanel(uiObj.transform, "MainPanel", new Vector2(300, 600));
        panel.transform.SetAsFirstSibling();
        
        // Create control buttons
        CreateControlButtons(panel.transform);
        
        // Create info displays
        CreateInfoDisplays(panel.transform);
        
        Debug.Log($"Editor UI prefab created: {uiObj.name}");
    }
    
    GameObject CreateUIPanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        
        UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0.8f);
        
        return panel;
    }
    
    void CreateControlButtons(Transform parent)
    {
        string[] buttonNames = { "Play", "Pause", "Stop", "Save", "Load" };
        
        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject button = CreateButton(parent, buttonNames[i]);
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 200 - i * 50);
        }
    }
    
    GameObject CreateButton(Transform parent, string name)
    {
        GameObject button = new GameObject(name + "Button");
        button.transform.SetParent(parent);
        
        RectTransform rt = button.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 40);
        
        UnityEngine.UI.Image image = button.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.grey;
        
        UnityEngine.UI.Button btn = button.AddComponent<UnityEngine.UI.Button>();
        
        // Create button text
        GameObject text = new GameObject("Text");
        text.transform.SetParent(button.transform);
        
        RectTransform textRt = text.AddComponent<RectTransform>();
        textRt.sizeDelta = Vector2.zero;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Text textComp = text.AddComponent<UnityEngine.UI.Text>();
        textComp.text = name;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        return button;
    }
    
    void CreateInfoDisplays(Transform parent)
    {
        string[] infoNames = { "BPM", "Lanes", "Mode", "Time" };
        
        for (int i = 0; i < infoNames.Length; i++)
        {
            GameObject info = CreateInfoDisplay(parent, infoNames[i]);
            RectTransform rt = info.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -50 - i * 30);
        }
    }
    
    GameObject CreateInfoDisplay(Transform parent, string name)
    {
        GameObject info = new GameObject(name + "Text");
        info.transform.SetParent(parent);
        
        RectTransform rt = info.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 25);
        
        UnityEngine.UI.Text text = info.AddComponent<UnityEngine.UI.Text>();
        text.text = name + ": ";
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        
        return info;
    }
    
    Sprite CreateNoteSprite()
    {
        // Create a simple square sprite for notes
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
    }
    
    Sprite CreateLongNoteSprite()
    {
        // Create a slightly different sprite for long notes
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            // Create a gradient effect for long notes
            int x = i % 64;
            int y = i / 64;
            float gradient = (float)y / 64f;
            pixels[i] = Color.Lerp(Color.white, Color.yellow, gradient);
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
    }
    #endif
}