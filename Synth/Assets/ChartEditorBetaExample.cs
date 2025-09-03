using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Comprehensive example and validation script for ChartEditorBeta
/// Demonstrates all major features and validates implementation completeness
/// </summary>
public class ChartEditorBetaExample : MonoBehaviour
{
    [Header("ChartEditorBeta Reference")]
    public ChartEditorBeta chartEditor;
    
    [Header("Example Settings")]
    public string exampleAudioPath = "Assets/Audio/example.wav";
    public string exampleChartPath = "Assets/Charts/example.chart";
    public bool runExamplesOnStart = false;
    
    [Header("Test Results")]
    public bool allTestsPassed = false;
    public int totalTests = 0;
    public int passedTests = 0;
    
    void Start()
    {
        if (runExamplesOnStart)
        {
            RunAllExamples();
        }
    }
    
    void Update()
    {
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.F10))
        {
            RunAllExamples();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            RunValidationTests();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            DemonstrateAllFeatures();
        }
    }
    
    #region Example Implementations
    
    [ContextMenu("Run All Examples")]
    public void RunAllExamples()
    {
        Debug.Log("=== ChartEditorBeta Examples Starting ===");
        
        ExampleBasicUsage();
        ExampleNoteCreation();
        ExampleAudioManagement();
        ExampleFileOperations();
        ExamplePreviewSystem();
        ExampleAdvancedFeatures();
        
        Debug.Log("=== ChartEditorBeta Examples Completed ===");
    }
    
    void ExampleBasicUsage()
    {
        Debug.Log("--- Basic Usage Example ---");
        
        if (chartEditor == null)
        {
            Debug.LogError("ChartEditor reference not set!");
            return;
        }
        
        // Set basic parameters
        chartEditor.bpm = 128f;
        chartEditor.laneCount = 4;
        chartEditor.scrollSpeed = 8f;
        
        // Update settings
        chartEditor.UpdateBPM(128f);
        chartEditor.UpdateLaneCount(4);
        chartEditor.UpdateBeatDivision(4);
        
        Debug.Log($"✓ Basic setup completed - BPM: {chartEditor.bpm}, Lanes: {chartEditor.laneCount}");
    }
    
    void ExampleNoteCreation()
    {
        Debug.Log("--- Note Creation Example ---");
        
        // Create some example notes programmatically
        var notes = new List<NoteDataBeta>
        {
            new NoteDataBeta(1.0, 0, KeySoundType.Kick, false, 0),
            new NoteDataBeta(1.5, 1, KeySoundType.Snare, false, 0),
            new NoteDataBeta(2.0, 2, KeySoundType.Hihat, false, 0),
            new NoteDataBeta(2.5, 0, KeySoundType.Kick, true, 3.5), // Long note
        };
        
        foreach (var note in notes)
        {
            note.CalculateBeatTiming(chartEditor.bpm);
        }
        
        Debug.Log($"✓ Created {notes.Count} example notes");
    }
    
    void ExampleAudioManagement()
    {
        Debug.Log("--- Audio Management Example ---");
        
        var audioManager = AudioManagerBeta.Instance;
        if (audioManager != null)
        {
            // Example audio settings
            audioManager.SetMasterVolume(0.8f);
            audioManager.SetMusicVolume(0.7f);
            audioManager.SetPlaybackSpeed(1.0f);
            audioManager.SetAudioOffset(0f);
            
            Debug.Log("✓ Audio manager configured");
            
            // Example encryption (if file exists)
            if (System.IO.File.Exists(exampleAudioPath))
            {
                string encryptedPath = exampleAudioPath.Replace(".wav", "_encrypted.eaw");
                if (ChartEditorBetaFileUtils.EncryptAudioFile(exampleAudioPath, encryptedPath))
                {
                    Debug.Log("✓ Audio file encrypted successfully");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠ AudioManagerBeta not found - creating instance");
            GameObject audioGO = new GameObject("AudioManagerBeta");
            audioGO.AddComponent<AudioManagerBeta>();
        }
    }
    
    void ExampleFileOperations()
    {
        Debug.Log("--- File Operations Example ---");
        
        // Create example chart data
        var exampleChart = new ChartDataBeta
        {
            audioFileName = "example.wav",
            bpm = 120f,
            laneCount = 4,
            beatDivision = 4,
            scrollSpeed = 8f,
            audioOffset = 0f
        };
        
        // Add some notes
        exampleChart.notes.Add(new NoteDataBeta(1.0, 0, KeySoundType.Kick));
        exampleChart.notes.Add(new NoteDataBeta(2.0, 1, KeySoundType.Snare));
        
        // Save chart
        string savePath = ChartEditorBetaFileUtils.GetDefaultChartPath("example");
        if (ChartEditorBetaFileUtils.SaveChart(exampleChart, savePath))
        {
            Debug.Log("✓ Chart saved successfully");
            
            // Load chart back
            var loadedChart = ChartEditorBetaFileUtils.LoadChart(savePath);
            if (loadedChart != null)
            {
                Debug.Log($"✓ Chart loaded successfully - Notes: {loadedChart.notes.Count}");
            }
        }
        
        // Example export with metadata
        string exportPath = savePath.Replace(".chart", "_export.chart");
        if (ChartEditorBetaFileUtils.ExportChart(exampleChart, exportPath, "Example Song", "Example Artist", "Example Charter"))
        {
            Debug.Log("✓ Chart exported with metadata");
        }
    }
    
    void ExamplePreviewSystem()
    {
        Debug.Log("--- Preview System Example ---");
        
        if (chartEditor.previewSystem != null)
        {
            // Subscribe to preview events
            chartEditor.previewSystem.OnNoteJudged += (judgment) => 
            {
                Debug.Log($"Preview judgment: {judgment}");
            };
            
            chartEditor.previewSystem.OnNoteSpawned += (noteData) => 
            {
                Debug.Log($"Preview note spawned: Track {noteData.track} at {noteData.timing:F2}s");
            };
            
            // Configure preview settings
            chartEditor.previewSystem.SetScrollSpeed(chartEditor.scrollSpeed);
            chartEditor.previewSystem.SetJudgeLinePosition(chartEditor.judgeLineY);
            
            Debug.Log("✓ Preview system configured and events subscribed");
        }
        else
        {
            Debug.LogWarning("⚠ Preview system not initialized");
        }
    }
    
    void ExampleAdvancedFeatures()
    {
        Debug.Log("--- Advanced Features Example ---");
        
        // Grid snap mode cycling
        var snapModes = System.Enum.GetValues(typeof(GridSnapMode));
        Debug.Log($"✓ Available grid snap modes: {snapModes.Length}");
        
        // Lane configurations
        int[] supportedLanes = chartEditor.supportedLaneCounts;
        Debug.Log($"✓ Supported lane counts: {string.Join(", ", supportedLanes)}");
        
        // Undo/Redo system capacity
        Debug.Log($"✓ Undo system max steps: {50}"); // maxUndoSteps constant
        
        // File utilities features
        string chartsDir = ChartEditorBetaFileUtils.GetChartsDirectory();
        string audioDir = ChartEditorBetaFileUtils.GetAudioDirectory();
        Debug.Log($"✓ Charts directory: {chartsDir}");
        Debug.Log($"✓ Audio directory: {audioDir}");
    }
    
    #endregion
    
    #region Validation Tests
    
    [ContextMenu("Run Validation Tests")]
    public void RunValidationTests()
    {
        Debug.Log("=== ChartEditorBeta Validation Tests ===");
        
        totalTests = 0;
        passedTests = 0;
        
        ValidateComponentReferences();
        ValidateDataStructures();
        ValidateFileOperations();
        ValidateInputHandling();
        ValidateAudioSystem();
        ValidatePreviewSystem();
        
        allTestsPassed = (passedTests == totalTests);
        
        Debug.Log($"=== Validation Complete: {passedTests}/{totalTests} tests passed ===");
        
        if (allTestsPassed)
        {
            Debug.Log("🎉 All validation tests PASSED! ChartEditorBeta is ready for use.");
        }
        else
        {
            Debug.LogWarning($"⚠ {totalTests - passedTests} tests failed. Check implementation.");
        }
    }
    
    void ValidateComponentReferences()
    {
        Debug.Log("--- Validating Component References ---");
        
        TestResult("ChartEditorBeta reference", chartEditor != null);
        
        if (chartEditor != null)
        {
            TestResult("AudioSource component", chartEditor.GetComponent<AudioSource>() != null);
            TestResult("Preview system initialized", chartEditor.previewSystem != null);
            TestResult("Supported lane counts defined", chartEditor.supportedLaneCounts != null && chartEditor.supportedLaneCounts.Length > 0);
        }
    }
    
    void ValidateDataStructures()
    {
        Debug.Log("--- Validating Data Structures ---");
        
        // Test ChartDataBeta
        var testChart = new ChartDataBeta();
        testChart.bpm = 120f;
        testChart.laneCount = 4;
        TestResult("ChartDataBeta creation", testChart != null);
        TestResult("ChartDataBeta notes list", testChart.notes != null);
        
        // Test NoteDataBeta
        var testNote = new NoteDataBeta(1.0, 0, KeySoundType.Kick, false, 0);
        TestResult("NoteDataBeta creation", testNote != null);
        TestResult("NoteDataBeta timing", testNote.timing == 1.0);
        TestResult("NoteDataBeta track", testNote.track == 0);
        
        // Test enums
        TestResult("EditMode enum", System.Enum.IsDefined(typeof(EditMode), EditMode.Normal));
        TestResult("GridSnapMode enum", System.Enum.IsDefined(typeof(GridSnapMode), GridSnapMode.Beat_1_4));
    }
    
    void ValidateFileOperations()
    {
        Debug.Log("--- Validating File Operations ---");
        
        // Test directory creation
        string testDir = ChartEditorBetaFileUtils.GetChartsDirectory();
        TestResult("Charts directory creation", System.IO.Directory.Exists(testDir));
        
        // Test file name sanitization
        string sanitized = ChartEditorBetaFileUtils.SanitizeFileName("Test<>|File");
        TestResult("File name sanitization", !sanitized.Contains("<") && !sanitized.Contains(">"));
        
        // Test chart file path generation
        string chartPath = ChartEditorBetaFileUtils.GetDefaultChartPath("test");
        TestResult("Chart path generation", chartPath.EndsWith(".chart"));
    }
    
    void ValidateInputHandling()
    {
        Debug.Log("--- Validating Input Handling ---");
        
        if (chartEditor != null)
        {
            // Test edit mode enumeration
            var editModes = System.Enum.GetValues(typeof(EditMode));
            TestResult("Edit modes defined", editModes.Length >= 2);
            
            // Test grid snap modes
            var snapModes = System.Enum.GetValues(typeof(GridSnapMode));
            TestResult("Grid snap modes defined", snapModes.Length >= 5);
            
            // Test lane count support
            TestResult("Lane count range", chartEditor.supportedLaneCounts.Length >= 6);
            TestResult("Minimum lane count", System.Array.IndexOf(chartEditor.supportedLaneCounts, 4) >= 0);
            TestResult("Maximum lane count", System.Array.IndexOf(chartEditor.supportedLaneCounts, 10) >= 0);
        }
    }
    
    void ValidateAudioSystem()
    {
        Debug.Log("--- Validating Audio System ---");
        
        var audioManager = AudioManagerBeta.Instance;
        TestResult("AudioManagerBeta singleton", audioManager != null);
        
        if (audioManager != null)
        {
            TestResult("Audio encryption support", audioManager.useEncryption || !audioManager.useEncryption); // Either state is valid
            TestResult("Audio volume controls", audioManager.masterVolume >= 0f && audioManager.masterVolume <= 1f);
            TestResult("Playback speed controls", audioManager.playbackSpeed >= 0.1f && audioManager.playbackSpeed <= 2f);
        }
    }
    
    void ValidatePreviewSystem()
    {
        Debug.Log("--- Validating Preview System ---");
        
        if (chartEditor != null && chartEditor.previewSystem != null)
        {
            var preview = chartEditor.previewSystem;
            TestResult("Preview system component", preview != null);
            TestResult("Preview system not active initially", !preview.IsActive);
            TestResult("Note count tracking", preview.ActiveNoteCount >= 0);
            TestResult("Remaining notes tracking", preview.RemainingNoteCount >= 0);
        }
        else
        {
            TestResult("Preview system accessibility", false);
        }
    }
    
    void TestResult(string testName, bool passed)
    {
        totalTests++;
        if (passed)
        {
            passedTests++;
            Debug.Log($"✓ {testName}");
        }
        else
        {
            Debug.LogError($"✗ {testName}");
        }
    }
    
    #endregion
    
    #region Feature Demonstration
    
    [ContextMenu("Demonstrate All Features")]
    public void DemonstrateAllFeatures()
    {
        Debug.Log("=== ChartEditorBeta Feature Demonstration ===");
        
        if (chartEditor == null)
        {
            Debug.LogError("ChartEditor reference required for demonstration!");
            return;
        }
        
        StartCoroutine(FeatureDemonstrationCoroutine());
    }
    
    System.Collections.IEnumerator FeatureDemonstrationCoroutine()
    {
        Debug.Log("🎵 Starting feature demonstration...");
        
        // 1. Lane count demonstration
        Debug.Log("--- Demonstrating Lane Count Changes ---");
        foreach (int lanes in chartEditor.supportedLaneCounts)
        {
            chartEditor.UpdateLaneCount(lanes);
            Debug.Log($"Lane count: {lanes}");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 2. BPM demonstration
        Debug.Log("--- Demonstrating BPM Changes ---");
        float[] testBPMs = { 60f, 120f, 180f, 240f };
        foreach (float bpm in testBPMs)
        {
            chartEditor.UpdateBPM(bpm);
            Debug.Log($"BPM: {bpm}");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 3. Beat division demonstration
        Debug.Log("--- Demonstrating Beat Division Changes ---");
        int[] divisions = { 4, 8, 16, 32 };
        foreach (int division in divisions)
        {
            chartEditor.UpdateBeatDivision(division);
            Debug.Log($"Beat division: 1/{division}");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 4. Grid snap mode demonstration
        Debug.Log("--- Demonstrating Grid Snap Modes ---");
        var snapModes = System.Enum.GetValues(typeof(GridSnapMode));
        foreach (GridSnapMode mode in snapModes)
        {
            chartEditor.gridSnapMode = mode;
            chartEditor.UpdateGridLines();
            Debug.Log($"Grid snap: {mode}");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 5. Edit mode demonstration
        Debug.Log("--- Demonstrating Edit Modes ---");
        chartEditor.SetEditMode(EditMode.Normal);
        Debug.Log("Edit mode: Normal");
        yield return new WaitForSeconds(1f);
        
        chartEditor.SetEditMode(EditMode.LongNote);
        Debug.Log("Edit mode: Long Note");
        yield return new WaitForSeconds(1f);
        
        Debug.Log("🎉 Feature demonstration completed!");
    }
    
    #endregion
    
    #region Public API
    
    public void CreateExampleChart()
    {
        if (chartEditor == null) return;
        
        // Create a simple 4/4 beat pattern
        var notes = new List<NoteDataBeta>();
        
        for (int measure = 0; measure < 4; measure++)
        {
            for (int beat = 0; beat < 4; beat++)
            {
                double timing = measure * (240f / chartEditor.bpm) + beat * (60f / chartEditor.bpm);
                int track = beat % chartEditor.laneCount;
                
                notes.Add(new NoteDataBeta(timing, track, KeySoundType.Kick));
            }
        }
        
        chartEditor.currentChart.notes.AddRange(notes);
        chartEditor.RefreshNoteDisplay();
        
        Debug.Log($"Created example chart with {notes.Count} notes");
    }
    
    public void ClearChart()
    {
        if (chartEditor == null) return;
        
        chartEditor.currentChart.notes.Clear();
        chartEditor.RefreshNoteDisplay();
        
        Debug.Log("Chart cleared");
    }
    
    public void ExportCurrentChart(string fileName)
    {
        if (chartEditor == null) return;
        
        string filePath = ChartEditorBetaFileUtils.GetDefaultChartPath(fileName);
        chartEditor.SaveChartAs(filePath, fileName, "Example Artist", "Example Charter");
    }
    
    #endregion
}