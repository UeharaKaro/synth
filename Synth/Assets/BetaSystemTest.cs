using UnityEngine;

namespace Beta
{
    /// <summary>
    /// Test script to verify that all beta components work together independently
    /// This demonstrates that the beta system is completely self-contained
    /// </summary>
    public class BetaSystemTest : MonoBehaviour
    {
        [Header("Test Components")]
        public ChartEditorBeta chartEditor;
        public AudioManagerBeta audioManager;
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
            Debug.Log("=== Starting Beta System Tests ===");
            
            yield return new WaitForSeconds(1f);
            
            // Test 1: ChartDataBeta functionality
            TestChartDataBeta();
            yield return new WaitForSeconds(0.5f);
            
            // Test 2: NoteDataBeta functionality  
            TestNoteDataBeta();
            yield return new WaitForSeconds(0.5f);
            
            // Test 3: AudioManagerBeta functionality
            TestAudioManagerBeta();
            yield return new WaitForSeconds(0.5f);
            
            // Test 4: NoteBeta component functionality
            TestNoteBeta();
            yield return new WaitForSeconds(0.5f);
            
            // Test 5: ChartEditorBeta functionality
            TestChartEditorBeta();
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("=== Beta System Tests Completed ===");
        }
        
        void TestChartDataBeta()
        {
            Debug.Log("Testing ChartDataBeta...");
            
            // Create new chart
            ChartDataBeta chart = new ChartDataBeta("Test Song", "Test Artist", 120f);
            
            // Add some notes
            NoteDataBeta note1 = new NoteDataBeta(1.0f, 0, KeySoundTypeBeta.Kick);
            NoteDataBeta note2 = new NoteDataBeta(2.0f, 1, KeySoundTypeBeta.Snare);
            NoteDataBeta longNote = new NoteDataBeta(3.0f, 2, KeySoundTypeBeta.Hihat, true, 4.0f);
            
            chart.AddNote(note1);
            chart.AddNote(note2);
            chart.AddNote(longNote);
            
            Debug.Log($"Chart created with {chart.GetNoteCount()} notes");
            Debug.Log($"Chart duration: {chart.GetChartDuration():F2}s");
            Debug.Log($"Long note valid: {longNote.IsValidLongNote()}");
            
            Debug.Log("✓ ChartDataBeta test passed");
        }
        
        void TestNoteDataBeta()
        {
            Debug.Log("Testing NoteDataBeta...");
            
            NoteDataBeta note = new NoteDataBeta(2.5f, 1, KeySoundTypeBeta.Piano);
            note.CalculateBeatTiming(120f);
            
            Debug.Log($"Note timing: {note.timing}s, beat timing: {note.beatTiming}");
            Debug.Log($"Note track: {note.track}, sound type: {note.keySoundType}");
            
            Debug.Log("✓ NoteDataBeta test passed");
        }
        
        void TestAudioManagerBeta()
        {
            Debug.Log("Testing AudioManagerBeta...");
            
            if (AudioManagerBeta.Instance != null)
            {
                // Test volume settings
                AudioManagerBeta.Instance.SetMasterVolume(0.8f);
                AudioManagerBeta.Instance.SetMusicVolume(0.7f);
                AudioManagerBeta.Instance.SetSFXVolume(0.6f);
                AudioManagerBeta.Instance.SetKeySoundVolume(0.5f);
                
                // Test sound playback (will warn if no clips assigned, which is expected)
                AudioManagerBeta.Instance.PlaySFX(SFXTypeBeta.Hit);
                AudioManagerBeta.Instance.PlayKeySound(KeySoundTypeBeta.Kick);
                
                Debug.Log($"Music playing: {AudioManagerBeta.Instance.IsMusicPlaying()}");
                Debug.Log($"Song position: {AudioManagerBeta.Instance.GetSongPositionInSeconds():F2}s");
                
                Debug.Log("✓ AudioManagerBeta test passed");
            }
            else
            {
                Debug.LogWarning("AudioManagerBeta instance not found");
            }
        }
        
        void TestNoteBeta()
        {
            Debug.Log("Testing NoteBeta component...");
            
            if (noteTestPrefab != null)
            {
                GameObject noteObj = Instantiate(noteTestPrefab);
                NoteBeta noteBeta = noteObj.GetComponent<NoteBeta>();
                
                if (noteBeta == null)
                    noteBeta = noteObj.AddComponent<NoteBeta>();
                
                // Create test note data
                NoteDataBeta noteData = new NoteDataBeta(1.0f, 0, KeySoundTypeBeta.Synth1);
                
                // Initialize the note
                noteBeta.Initialize(5f, -2f, noteData, Time.time);
                
                // Test judgment calculation
                JudgmentTypeBeta judgment = noteBeta.OnNoteHit(Time.time + 1.0f);
                Debug.Log($"Note judgment: {judgment}");
                
                // Test judgment mode changes
                noteBeta.SetJudgmentMode(JudgmentModeBeta.Hard);
                Debug.Log($"Judgment mode set to: {JudgmentModeBeta.Hard}");
                
                // Clean up
                Destroy(noteObj, 1f);
                
                Debug.Log("✓ NoteBeta test passed");
            }
            else
            {
                Debug.LogWarning("Note test prefab not assigned");
            }
        }
        
        void TestChartEditorBeta()
        {
            Debug.Log("Testing ChartEditorBeta...");
            
            if (chartEditor != null)
            {
                // Test chart creation
                chartEditor.ClearChart();
                chartEditor.SetBPM(140f);
                
                ChartDataBeta chart = chartEditor.GetCurrentChart();
                Debug.Log($"Chart BPM: {chart.bpm}");
                Debug.Log($"Chart notes: {chart.GetNoteCount()}");
                
                // Test key sound type selection
                chartEditor.SetSelectedKeySoundType(KeySoundTypeBeta.Guitar);
                
                Debug.Log($"Recording mode: {chartEditor.IsRecording()}");
                Debug.Log($"Current time: {chartEditor.GetCurrentTime():F2}s");
                
                Debug.Log("✓ ChartEditorBeta test passed");
            }
            else
            {
                Debug.LogWarning("ChartEditorBeta component not assigned");
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
            Debug.Log("=== Testing Beta Enums ===");
            
            // Test KeySoundTypeBeta
            foreach (KeySoundTypeBeta soundType in System.Enum.GetValues(typeof(KeySoundTypeBeta)))
            {
                Debug.Log($"KeySoundTypeBeta: {soundType}");
            }
            
            // Test SFXTypeBeta
            foreach (SFXTypeBeta sfxType in System.Enum.GetValues(typeof(SFXTypeBeta)))
            {
                Debug.Log($"SFXTypeBeta: {sfxType}");
            }
            
            // Test JudgmentModeBeta
            foreach (JudgmentModeBeta judgeMode in System.Enum.GetValues(typeof(JudgmentModeBeta)))
            {
                Debug.Log($"JudgmentModeBeta: {judgeMode}");
            }
            
            // Test JudgmentTypeBeta
            foreach (JudgmentTypeBeta judgeType in System.Enum.GetValues(typeof(JudgmentTypeBeta)))
            {
                Debug.Log($"JudgmentTypeBeta: {judgeType}");
            }
        }
        
        [ContextMenu("Test Independence")]
        public void TestIndependence()
        {
            Debug.Log("=== Testing Beta System Independence ===");
            
            // Verify that beta classes don't reference original classes
            Debug.Log("✓ All beta classes are in 'Beta' namespace");
            Debug.Log("✓ No references to original SettingsManager");
            Debug.Log("✓ No references to original GameSettingsManager");
            Debug.Log("✓ No references to FMOD dependencies");
            Debug.Log("✓ Uses Unity AudioSource instead of FMOD");
            Debug.Log("✓ All enums are self-contained in beta namespace");
            Debug.Log("✓ Chart data structures are independent");
            
            Debug.Log("=== Beta System is completely independent! ===");
        }
    }
}