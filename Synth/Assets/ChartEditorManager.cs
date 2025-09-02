using UnityEngine;

[System.Serializable]
public class EditorSettings
{
    [Header("Visual Settings")]
    [Range(0.5f, 2f)]
    public float noteSize = 1f;
    [Range(0.1f, 1f)]
    public float laneOpacity = 0.3f;
    [Range(1f, 5f)]
    public float laneWidth = 1f;
    [Range(0.1f, 2f)]
    public float laneSpacing = 0.1f;
    
    [Header("Grid Settings")]
    public bool showGrid = true;
    public bool showMeasureNumbers = true;
    [Range(0.1f, 1f)]
    public float gridOpacity = 0.5f;
    
    [Header("Audio Settings")]
    [Range(0.1f, 2f)]
    public float defaultPlaybackSpeed = 1f;
    [Range(-1000f, 1000f)]
    public float defaultAudioOffset = 0f;
    [Range(1f, 20f)]
    public float defaultScrollSpeed = 8f;
    
    [Header("Editor Behavior")]
    public bool autoSave = true;
    [Range(1, 10)]
    public int autoSaveInterval = 5; // minutes
    [Range(10, 100)]
    public int maxUndoSteps = 50;
    public bool snapToGrid = true;
    
    [Header("Performance")]
    [Range(10, 100)]
    public int maxVisibleNotes = 50;
    [Range(5, 50)]
    public int maxGridLines = 20;
    
    public void ResetToDefault()
    {
        noteSize = 1f;
        laneOpacity = 0.3f;
        laneWidth = 1f;
        laneSpacing = 0.1f;
        showGrid = true;
        showMeasureNumbers = true;
        gridOpacity = 0.5f;
        defaultPlaybackSpeed = 1f;
        defaultAudioOffset = 0f;
        defaultScrollSpeed = 8f;
        autoSave = true;
        autoSaveInterval = 5;
        maxUndoSteps = 50;
        snapToGrid = true;
        maxVisibleNotes = 50;
        maxGridLines = 20;
    }
}

public class ChartEditorManager : MonoBehaviour
{
    [Header("Editor Settings")]
    public EditorSettings settings = new EditorSettings();
    
    [Header("Components")]
    public ChartEditor chartEditor;
    public TimelineDisplay timelineDisplay;
    public AudioManager audioManager;
    
    [Header("UI Panels")]
    public GameObject mainEditorPanel;
    public GameObject settingsPanel;
    public GameObject helpPanel;
    
    private static ChartEditorManager instance;
    public static ChartEditorManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChartEditorManager>();
            return instance;
        }
    }
    
    private float lastAutoSaveTime;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        LoadEditorSettings();
    }
    
    void Start()
    {
        ApplySettings();
        lastAutoSaveTime = Time.time;
    }
    
    void Update()
    {
        HandleGlobalInput();
        CheckAutoSave();
    }
    
    void HandleGlobalInput()
    {
        // 전역 단축키
        if (Input.GetKeyDown(KeyCode.F1))
            ToggleHelp();
        if (Input.GetKeyDown(KeyCode.F2))
            ToggleSettings();
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseAllPanels();
    }
    
    void CheckAutoSave()
    {
        if (settings.autoSave && Time.time - lastAutoSaveTime > settings.autoSaveInterval * 60f)
        {
            if (chartEditor != null)
            {
                chartEditor.SaveChart();
                Debug.Log("Auto-saved chart");
            }
            lastAutoSaveTime = Time.time;
        }
    }
    
    public void ApplySettings()
    {
        if (chartEditor != null)
        {
            chartEditor.laneWidth = settings.laneWidth;
            chartEditor.laneSpacing = settings.laneSpacing;
            chartEditor.showGrid = settings.showGrid;
        }
        
        if (audioManager != null)
        {
            // AudioManager 설정 적용
        }
    }
    
    public void SaveEditorSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        string path = Application.persistentDataPath + "/editor_settings.json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log("Editor settings saved");
    }
    
    public void LoadEditorSettings()
    {
        string path = Application.persistentDataPath + "/editor_settings.json";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            settings = JsonUtility.FromJson<EditorSettings>(json);
            Debug.Log("Editor settings loaded");
        }
        else
        {
            settings.ResetToDefault();
        }
    }
    
    public void ToggleHelp()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(!helpPanel.activeSelf);
            if (settingsPanel != null && helpPanel.activeSelf)
                settingsPanel.SetActive(false);
        }
    }
    
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
            if (helpPanel != null && settingsPanel.activeSelf)
                helpPanel.SetActive(false);
        }
    }
    
    public void CloseAllPanels()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    public void ResetSettings()
    {
        settings.ResetToDefault();
        ApplySettings();
        SaveEditorSettings();
    }
    
    public void ImportChart(string filePath)
    {
        if (chartEditor != null)
        {
            // 차트 파일 임포트 로직
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                ChartData imported = JsonUtility.FromJson<ChartData>(json);
                
                if (imported.ValidateChart())
                {
                    chartEditor.LoadChart(imported);
                    Debug.Log($"Chart imported successfully from {filePath}");
                }
                else
                {
                    Debug.LogError("Invalid chart data");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to import chart: {e.Message}");
            }
        }
    }
    
    public void ExportChart(string filePath)
    {
        if (chartEditor != null && chartEditor.GetCurrentChart() != null)
        {
            try
            {
                string json = JsonUtility.ToJson(chartEditor.GetCurrentChart(), true);
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"Chart exported to {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to export chart: {e.Message}");
            }
        }
    }
}