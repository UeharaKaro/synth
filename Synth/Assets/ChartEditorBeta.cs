using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace Beta
{
    /// <summary>
    /// Beta version of ChartEditor - completely self-contained and independent
    /// Provides basic chart editing functionality without external dependencies
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ChartEditorBeta : MonoBehaviour
    {
        [Header("UI Elements")]
        public InputField audioPathInputField;
        public Slider timelineSlider;
        public Text currentTimeText;
        public Text totalTimeText;
        public Button loadAudioButton;
        public Button saveChartButton;
        public Button loadChartButton;
        public Button playButton;
        public Button pauseButton;
        public Button stopButton;
        
        [Header("Chart Settings")]
        public float bpm = 120f;
        public string songName = "";
        public string artistName = "";
        
        [Header("Note Input Settings")]
        public KeyCode[] trackKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
        public Transform[] noteSpawnPoints;
        public GameObject notePrefab;
        
        [Header("Judgment Settings")]
        public JudgmentModeBeta currentJudgmentMode = JudgmentModeBeta.Normal;
        
        // Private variables
        private AudioSource audioSource;
        private ChartDataBeta currentChart;
        private string audioFilePath;
        private List<NoteBeta> activeNotes = new List<NoteBeta>();
        private bool isPlaying = false;
        private bool isRecording = false;
        
        // Chart editing state
        private float lastNoteTime = 0f;
        private int selectedTrack = 0;
        private KeySoundTypeBeta selectedKeySoundType = KeySoundTypeBeta.None;
        
        void Start()
        {
            InitializeEditor();
        }
        
        void InitializeEditor()
        {
            // Initialize audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            // Initialize chart data
            currentChart = new ChartDataBeta();
            
            // Setup UI events
            SetupUIEvents();
            
            // Initialize note prefab if not assigned
            if (notePrefab == null)
            {
                notePrefab = CreateDefaultNotePrefab();
            }
            
            Debug.Log("ChartEditorBeta initialized successfully");
        }
        
        void SetupUIEvents()
        {
            // Audio control buttons
            if (loadAudioButton != null)
                loadAudioButton.onClick.AddListener(OnLoadAudioButtonClicked);
                
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
                
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
                
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopButtonClicked);
            
            // Chart control buttons    
            if (saveChartButton != null)
                saveChartButton.onClick.AddListener(OnSaveChartButtonClicked);
                
            if (loadChartButton != null)
                loadChartButton.onClick.AddListener(OnLoadChartButtonClicked);
            
            // Timeline slider
            if (timelineSlider != null)
                timelineSlider.onValueChanged.AddListener(SeekAudio);
        }
        
        GameObject CreateDefaultNotePrefab()
        {
            GameObject prefab = new GameObject("NoteBeta");
            prefab.AddComponent<SpriteRenderer>();
            prefab.AddComponent<NoteBeta>();
            
            // Create a simple white sprite
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = Color.white;
            
            return prefab;
        }
        
        Sprite CreateSimpleSprite()
        {
            // Create a simple 1x1 white texture
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        
        void Update()
        {
            // Update timeline UI
            if (audioSource.isPlaying && timelineSlider != null)
            {
                timelineSlider.value = audioSource.time;
                if (currentTimeText != null)
                    currentTimeText.text = FormatTime(audioSource.time);
            }
            
            // Handle note input during recording
            if (isRecording && audioSource.isPlaying)
            {
                HandleNoteInput();
            }
            
            // Update judgment mode with number keys
            HandleJudgmentModeInput();
        }
        
        void HandleNoteInput()
        {
            for (int i = 0; i < trackKeys.Length; i++)
            {
                if (Input.GetKeyDown(trackKeys[i]))
                {
                    AddNoteAtCurrentTime(i);
                }
            }
        }
        
        void HandleJudgmentModeInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentJudgmentMode = JudgmentModeBeta.Normal;
                Debug.Log("Judgment mode changed to Normal");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentJudgmentMode = JudgmentModeBeta.Hard;
                Debug.Log("Judgment mode changed to Hard");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentJudgmentMode = JudgmentModeBeta.Super;
                Debug.Log("Judgment mode changed to Super");
            }
        }
        
        void AddNoteAtCurrentTime(int track)
        {
            if (track < 0 || track >= trackKeys.Length) return;
            
            float currentTime = audioSource.time;
            
            // Prevent adding notes too close together
            if (currentTime - lastNoteTime < 0.1f) return;
            
            NoteDataBeta noteData = new NoteDataBeta(currentTime, track, selectedKeySoundType);
            noteData.CalculateBeatTiming(bpm);
            
            currentChart.AddNote(noteData);
            lastNoteTime = currentTime;
            
            Debug.Log($"Added note at time: {currentTime:F2}s, track: {track}");
        }
        
        // UI Button Event Handlers
        public void OnLoadAudioButtonClicked()
        {
            string path = audioPathInputField != null ? audioPathInputField.text : "";
            if (string.IsNullOrEmpty(path))
            {
                path = "C:/Users/Default/Music/test.mp3"; // Default path
            }
            StartCoroutine(LoadAudio(path));
        }
        
        public void OnPlayButtonClicked()
        {
            if (audioSource.clip != null)
            {
                audioSource.Play();
                isPlaying = true;
                isRecording = true; // Enable note recording when playing
                Debug.Log("Audio playback started - Recording mode enabled");
            }
        }
        
        public void OnPauseButtonClicked()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                isRecording = false;
                Debug.Log("Audio paused - Recording mode disabled");
            }
            else if (audioSource.clip != null)
            {
                audioSource.UnPause();
                isRecording = true;
                Debug.Log("Audio resumed - Recording mode enabled");
            }
        }
        
        public void OnStopButtonClicked()
        {
            audioSource.Stop();
            isPlaying = false;
            isRecording = false;
            audioSource.time = 0f;
            
            if (timelineSlider != null)
                timelineSlider.value = 0f;
                
            Debug.Log("Audio stopped - Recording mode disabled");
        }
        
        public void OnSaveChartButtonClicked()
        {
            SaveChart();
        }
        
        public void OnLoadChartButtonClicked()
        {
            LoadChart();
        }
        
        // Core functionality methods
        IEnumerator LoadAudio(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Audio path is empty");
                yield break;
            }
            
            string url = "file:///" + path.Replace("\\", "/");
            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load audio: {www.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    
                    // Update UI
                    if (timelineSlider != null)
                    {
                        timelineSlider.maxValue = clip.length;
                        timelineSlider.value = 0f;
                    }
                    
                    if (totalTimeText != null)
                        totalTimeText.text = FormatTime(clip.length);
                        
                    if (currentTimeText != null)
                        currentTimeText.text = FormatTime(0f);
                    
                    audioFilePath = path;
                    currentChart.audioFileName = Path.GetFileName(path);
                    
                    Debug.Log($"Audio loaded successfully: {clip.name} ({clip.length:F2}s)");
                }
            }
        }
        
        void SeekAudio(float time)
        {
            if (audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.time = time;
                if (currentTimeText != null)
                    currentTimeText.text = FormatTime(time);
            }
        }
        
        void SaveChart()
        {
            // Update chart metadata
            currentChart.songName = songName;
            currentChart.artistName = artistName;
            currentChart.bpm = bpm;
            
            try
            {
                string json = JsonUtility.ToJson(currentChart, true);
                string path = Path.Combine(Application.persistentDataPath, "chart_beta.json");
                File.WriteAllText(path, json);
                
                Debug.Log($"Chart saved successfully to: {path}");
                Debug.Log($"Total notes saved: {currentChart.GetNoteCount()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save chart: {e.Message}");
            }
        }
        
        void LoadChart()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "chart_beta.json");
                
                if (!File.Exists(path))
                {
                    Debug.LogWarning("Chart file not found!");
                    return;
                }
                
                string json = File.ReadAllText(path);
                currentChart = JsonUtility.FromJson<ChartDataBeta>(json);
                
                // Update editor settings
                songName = currentChart.songName;
                artistName = currentChart.artistName;
                bpm = currentChart.bpm;
                
                Debug.Log($"Chart loaded successfully from: {path}");
                Debug.Log($"Total notes loaded: {currentChart.GetNoteCount()}");
                
                // Try to load associated audio file
                if (!string.IsNullOrEmpty(currentChart.audioFileName))
                {
                    string audioPath = Path.Combine(Path.GetDirectoryName(audioFilePath), currentChart.audioFileName);
                    if (File.Exists(audioPath))
                    {
                        StartCoroutine(LoadAudio(audioPath));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load chart: {e.Message}");
                currentChart = new ChartDataBeta(); // Reset to empty chart
            }
        }
        
        string FormatTime(float time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            int milliseconds = (int)((time - (int)time) * 100);
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
        
        // Public methods for external access
        public ChartDataBeta GetCurrentChart()
        {
            return currentChart;
        }
        
        public void SetChart(ChartDataBeta chart)
        {
            if (chart != null)
            {
                currentChart = chart;
                songName = chart.songName;
                artistName = chart.artistName;
                bpm = chart.bpm;
            }
        }
        
        public bool IsRecording()
        {
            return isRecording;
        }
        
        public float GetCurrentTime()
        {
            return audioSource != null ? audioSource.time : 0f;
        }
        
        public void SetBPM(float newBPM)
        {
            bpm = Mathf.Max(60f, newBPM);
            currentChart.bpm = bpm;
            
            // Recalculate beat timings for existing notes
            foreach (var note in currentChart.notes)
            {
                note.CalculateBeatTiming(bpm);
            }
        }
        
        public void SetSelectedKeySoundType(KeySoundTypeBeta keySoundType)
        {
            selectedKeySoundType = keySoundType;
        }
        
        public void ClearChart()
        {
            currentChart.Clear();
            Debug.Log("Chart cleared");
        }
        
        // Utility methods
        public void RemoveNotesInTimeRange(float startTime, float endTime)
        {
            currentChart.notes.RemoveAll(note => note.timing >= startTime && note.timing <= endTime);
            Debug.Log($"Removed notes between {startTime:F2}s and {endTime:F2}s");
        }
        
        public void QuantizeNotes(float beatDivision = 16f)
        {
            float beatLength = 60f / bpm;
            float snapTime = beatLength / beatDivision;
            
            foreach (var note in currentChart.notes)
            {
                float snappedTime = Mathf.Round(note.timing / snapTime) * snapTime;
                note.timing = snappedTime;
                note.CalculateBeatTiming(bpm);
            }
            
            currentChart.SortNotesByTime();
            Debug.Log($"Notes quantized to 1/{beatDivision} beat");
        }
        
        void OnDestroy()
        {
            // Cleanup
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        // Debug and testing methods
        [ContextMenu("Add Test Notes")]
        void AddTestNotes()
        {
            for (int i = 0; i < 10; i++)
            {
                float time = i * 0.5f; // Note every 0.5 seconds
                int track = i % trackKeys.Length;
                
                NoteDataBeta noteData = new NoteDataBeta(time, track, KeySoundTypeBeta.Kick);
                noteData.CalculateBeatTiming(bpm);
                currentChart.AddNote(noteData);
            }
            Debug.Log("Added 10 test notes");
        }
        
        [ContextMenu("Print Chart Info")]
        void PrintChartInfo()
        {
            Debug.Log($"Chart Info:");
            Debug.Log($"Song: {currentChart.songName} by {currentChart.artistName}");
            Debug.Log($"BPM: {currentChart.bpm}");
            Debug.Log($"Notes: {currentChart.GetNoteCount()}");
            Debug.Log($"Duration: {currentChart.GetChartDuration():F2}s");
        }
    }
}