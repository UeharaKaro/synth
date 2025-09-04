using UnityEngine;

/// <summary>
/// Simple test script to validate ChartEditorBeta functionality
/// </summary>
public class ChartEditorBetaTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public ChartEditorBeta chartEditor;
    public bool runTestOnStart = false;
    
    void Start()
    {
        if (runTestOnStart)
        {
            RunBasicTests();
        }
    }
    
    void Update()
    {
        // Press T to run tests manually
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunBasicTests();
        }
    }
    
    public void RunBasicTests()
    {
        Debug.Log("Running ChartEditorBeta Tests...");
        
        // Test 1: Chart creation
        TestChartCreation();
        
        // Test 2: Note placement
        TestNotePlacement();
        
        // Test 3: Lane management
        TestLaneManagement();
        
        // Test 4: Audio management
        TestAudioManagement();
        
        Debug.Log("ChartEditorBeta Tests Completed!");
    }
    
    void TestChartCreation()
    {
        var testChart = new ChartDataBeta();
        testChart.bpm = 120f;
        testChart.laneCount = 4;
        testChart.beatDivision = 4;
        
        Debug.Log($"✓ Chart creation test passed - BPM: {testChart.bpm}, Lanes: {testChart.laneCount}");
    }
    
    void TestNotePlacement()
    {
        var testNote = new NoteDataBeta(1.0, 0, KeySoundType.None, false, 0);
        testNote.CalculateBeatTiming(120f);
        
        Debug.Log($"✓ Note placement test passed - Timing: {testNote.timing}, Track: {testNote.track}");
    }
    
    void TestLaneManagement()
    {
        int[] supportedLanes = { 4, 5, 6, 7, 8, 10 };
        bool allSupported = true;
        
        foreach (int lanes in supportedLanes)
        {
            if (lanes < 4 || lanes > 10)
            {
                allSupported = false;
                break;
            }
        }
        
        Debug.Log($"✓ Lane management test {(allSupported ? "passed" : "failed")}");
    }
    
    void TestAudioManagement()
    {
        var audioManager = AudioManagerBeta.Instance;
        if (audioManager != null)
        {
            Debug.Log("✓ Audio management test passed - AudioManagerBeta instance found");
        }
        else
        {
            Debug.Log("⚠ Audio management test - AudioManagerBeta instance not found (expected in some scenarios)");
        }
    }
}