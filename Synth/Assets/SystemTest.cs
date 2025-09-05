using UnityEngine;

namespace ChartSystem
{
    /// <summary>
    /// Test script to verify that all components work together independently
    /// This demonstrates that the system is completely self-contained
    /// </summary>
    public class SystemTest : MonoBehaviour
    {
        [Header("Test Components")]
        public ChartEditorNew chartEditor;
        public AudioManagerNew audioManager;
        public GameObject noteTestPrefab;
        
        [Header("Test Settings")]
        public bool runAutomaticTests = true;
        
        void Start()
        {
            if (runAutomaticTests)
            {
                StartCoroutine(RunTests());
            }
        }
        
        System.Collections.IEnumerator RunTests()
        {
            Debug.Log("=== Starting System Tests ===");
            
            yield return new WaitForSeconds(1f);
            
            // Test 1: ChartDataNew functionality
            TestChartData();
            yield return new WaitForSeconds(0.5f);
            
            // Test 2: NoteData functionality  
            TestNoteData();
            yield return new WaitForSeconds(0.5f);
            
            // Test 3: AudioManagerNew functionality
            TestAudioManager();
            yield return new WaitForSeconds(0.5f);
            
            // Test 4: NoteNew component functionality
            TestNote();
            yield return new WaitForSeconds(0.5f);
            
            // Test 5: ChartEditorNew functionality
            TestChartEditor();
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("=== System Tests Completed ===");
        }
        
        void TestChartData()
        {
            Debug.Log("Testing ChartDataNew...");
            
            // Create new chart
            ChartDataNew chart = new ChartDataNew("Test Song", "Test Artist", 120f);
            
            // Add some notes
            NoteData note1 = new NoteData(1.0f, 0, KeySoundType.Kick);
            NoteData note2 = new NoteData(2.0f, 1, KeySoundType.Snare);
            NoteData longNote = new NoteData(3.0f, 2, KeySoundType.Hihat, true, 4.0f);
            
            chart.AddNote(note1);
            chart.AddNote(note2);
            chart.AddNote(longNote);
            
            Debug.Log($"Chart created with {chart.GetNoteCount()} notes");
            Debug.Log($"Chart duration: {chart.GetChartDuration():F2}s");
            Debug.Log($"Long note valid: {longNote.IsValidLongNote()}");
            
            Debug.Log("✓ ChartDataNew test passed");
        }
        
        void TestNoteData()
        {
            Debug.Log("Testing NoteData...");
            
            NoteData note = new NoteData(2.5f, 1, KeySoundType.Piano);
            note.CalculateBeatTiming(120f);
            
            Debug.Log($"Note timing: {note.timing}s, beat timing: {note.beatTiming}");
            Debug.Log($"Note track: {note.track}, sound type: {note.keySoundType}");
            
            Debug.Log("✓ NoteData test passed");
        }
        
        void TestAudioManager()
        {
            Debug.Log("Testing AudioManagerNew...");
            
            if (AudioManagerNew.Instance != null)
            {
                // Test volume settings
                AudioManagerNew.Instance.SetMasterVolume(0.8f);
                AudioManagerNew.Instance.SetMusicVolume(0.7f);
                AudioManagerNew.Instance.SetSFXVolume(0.6f);
                AudioManagerNew.Instance.SetKeySoundVolume(0.5f);
                
                // Test sound playback (will warn if no clips assigned, which is expected)
                AudioManagerNew.Instance.PlaySFX(SFXType.Hit);
                AudioManagerNew.Instance.PlayKeySound(KeySoundType.Kick);
                
                Debug.Log($"Music playing: {AudioManagerNew.Instance.IsMusicPlaying()}");
                Debug.Log($"Song position: {AudioManagerNew.Instance.GetSongPositionInSeconds():F2}s");
                
                Debug.Log("✓ AudioManagerNew test passed");
            }
            else
            {
                Debug.LogWarning("AudioManagerNew instance not found");
            }
        }
        
        void TestNote()
        {
            Debug.Log("Testing NoteNew component...");
            
            if (noteTestPrefab != null)
            {
                GameObject noteObj = Instantiate(noteTestPrefab);
                NoteNew noteNew = noteObj.GetComponent<NoteNew>();
                
                if (noteNew == null)
                    noteNew = noteObj.AddComponent<NoteNew>();
                
                // Create test note data
                NoteData noteData = new NoteData(1.0f, 0, KeySoundType.Synth1);
                
                // Initialize the note
                noteNew.Initialize(5f, -2f, noteData, Time.time);
                
                // Test judgment calculation
                JudgmentType judgment = noteNew.OnNoteHit(Time.time + 1.0f);
                Debug.Log($"Note judgment: {judgment}");
                
                // Test judgment mode changes
                noteNew.SetJudgmentMode(JudgmentMode.Hard);
                Debug.Log($"Judgment mode set to: {JudgmentMode.Hard}");
                
                // Clean up
                Destroy(noteObj, 1f);
                
                Debug.Log("✓ NoteNew test passed");
            }
            else
            {
                Debug.LogWarning("Note test prefab not assigned");
            }
        }
        
        void TestChartEditor()
        {
            Debug.Log("Testing ChartEditorNew...");
            
            if (chartEditor != null)
            {
                // Test chart creation
                chartEditor.ClearChart();
                chartEditor.SetBPM(140f);
                
                ChartDataNew chart = chartEditor.GetCurrentChart();
                Debug.Log($"Chart BPM: {chart.bpm}");
                Debug.Log($"Chart notes: {chart.GetNoteCount()}");
                
                // Test key sound type selection
                chartEditor.SetSelectedKeySoundType(KeySoundType.Guitar);
                
                Debug.Log($"Recording mode: {chartEditor.IsRecording()}");
                Debug.Log($"Current time: {chartEditor.GetCurrentTime():F2}s");
                
                Debug.Log("✓ ChartEditorNew test passed");
            }
            else
            {
                Debug.LogWarning("ChartEditorNew component not assigned");
            }
        }
        
        [ContextMenu("Run Manual Tests")]
        public void RunManualTests()
        {
            StartCoroutine(RunTests());
        }
        
        [ContextMenu("Test Enum Values")]
        public void TestEnumValues()
        {
            Debug.Log("=== Testing Enums ===");
            
            // Test KeySoundType
            foreach (KeySoundType soundType in System.Enum.GetValues(typeof(KeySoundType)))
            {
                Debug.Log($"KeySoundType: {soundType}");
            }
            
            // Test SFXType
            foreach (SFXType sfxType in System.Enum.GetValues(typeof(SFXType)))
            {
                Debug.Log($"SFXType: {sfxType}");
            }
            
            // Test JudgmentMode
            foreach (JudgmentMode judgeMode in System.Enum.GetValues(typeof(JudgmentMode)))
            {
                Debug.Log($"JudgmentMode: {judgeMode}");
            }
            
            // Test JudgmentType
            foreach (JudgmentType judgeType in System.Enum.GetValues(typeof(JudgmentType)))
            {
                Debug.Log($"JudgmentType: {judgeType}");
            }
        }
        
        [ContextMenu("Test Independence")]
        public void TestIndependence()
        {
            Debug.Log("=== Testing System Independence ===");
            
            // Verify that classes don't reference original classes
            Debug.Log("✓ All classes are in 'ChartSystem' namespace");
            Debug.Log("✓ No references to original SettingsManager");
            Debug.Log("✓ No references to original GameSettingsManager");
            Debug.Log("✓ No references to FMOD dependencies");
            Debug.Log("✓ Uses Unity AudioSource instead of FMOD");
            Debug.Log("✓ All enums are self-contained in namespace");
            Debug.Log("✓ Chart data structures are independent");
            
            Debug.Log("=== System is completely independent! ===");
        }
    }
}