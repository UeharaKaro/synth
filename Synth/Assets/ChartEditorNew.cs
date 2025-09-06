using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace ChartSystem
{
    /// <summary>
    /// 독립적인 차트 에디터 - 완전히 자율적
    /// 외부 의존성 없이 기본적인 차트 편집 기능을 제공
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ChartEditorNew : MonoBehaviour
    {
        [Header("UI 요소들")]
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
        
        [Header("차트 설정")]
        public float bpm = 120f;
        public string songName = "";
        public string artistName = "";
        
        [Header("노트 입력 설정")]
        public KeyCode[] trackKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
        public Transform[] noteSpawnPoints;
        public GameObject notePrefab;
        
        [Header("판정 설정")]
        public JudgmentMode currentJudgmentMode = JudgmentMode.Normal;
        
        // 개인 변수들
        private AudioSource audioSource;
        private ChartDataNew currentChart;
        private string audioFilePath;
        private List<NoteNew> activeNotes = new List<NoteNew>();
        private bool isPlaying = false;
        private bool isRecording = false;
        
        // 차트 편집 상태
        private float lastNoteTime = 0f;
        private int selectedTrack = 0;
        private KeySoundType selectedKeySoundType = KeySoundType.None;
        
        void Start()
        {
            InitializeEditor();
        }
        
        void InitializeEditor()
        {
            // 오디오 소스 초기화
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            // 차트 데이터 초기화
            currentChart = new ChartDataNew();
            
            // UI 이벤트 설정
            SetupUIEvents();
            
            // 노트 프리팹이 할당되지 않은 경우 초기화
            if (notePrefab == null)
            {
                notePrefab = CreateDefaultNotePrefab();
            }
            
            Debug.Log("ChartEditorNew 성공적으로 초기화됨");
        }
        
        void SetupUIEvents()
        {
            // 오디오 컨트롤 버튼들
            if (loadAudioButton != null)
                loadAudioButton.onClick.AddListener(OnLoadAudioButtonClicked);
                
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
                
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
                
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopButtonClicked);
            
            // 차트 컨트롤 버튼들    
            if (saveChartButton != null)
                saveChartButton.onClick.AddListener(OnSaveChartButtonClicked);
                
            if (loadChartButton != null)
                loadChartButton.onClick.AddListener(OnLoadChartButtonClicked);
            
            // 타임라인 슬라이더
            if (timelineSlider != null)
                timelineSlider.onValueChanged.AddListener(SeekAudio);
        }
        
        GameObject CreateDefaultNotePrefab()
        {
            GameObject prefab = new GameObject("Note");
            prefab.AddComponent<SpriteRenderer>();
            prefab.AddComponent<NoteNew>();
            
            // 간단한 흰색 스프라이트 생성
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = Color.white;
            
            return prefab;
        }
        
        Sprite CreateSimpleSprite()
        {
            // 간단한 1x1 흰색 텍스처 생성
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
            // 타임라인 UI 업데이트
            if (audioSource.isPlaying && timelineSlider != null)
            {
                timelineSlider.value = audioSource.time;
                if (currentTimeText != null)
                    currentTimeText.text = FormatTime(audioSource.time);
            }
            
            // 녹음 중 노트 입력 처리
            if (isRecording && audioSource.isPlaying)
            {
                HandleNoteInput();
            }
            
            // 숫자 키로 판정 모드 업데이트
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
                currentJudgmentMode = JudgmentMode.Normal;
                Debug.Log("판정 모드가 일반으로 변경됨");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentJudgmentMode = JudgmentMode.Hard;
                Debug.Log("판정 모드가 하드로 변경됨");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentJudgmentMode = JudgmentMode.Super;
                Debug.Log("판정 모드가 슈퍼로 변경됨");
            }
        }
        
        void AddNoteAtCurrentTime(int track)
        {
            if (track < 0 || track >= trackKeys.Length) return;
            
            float currentTime = audioSource.time;
            
            // 너무 가까운 노트 추가 방지
            if (currentTime - lastNoteTime < 0.1f) return;
            
            NoteData noteData = new NoteData(currentTime, track, selectedKeySoundType);
            noteData.CalculateBeatTiming(bpm);
            
            currentChart.AddNote(noteData);
            lastNoteTime = currentTime;
            
            Debug.Log($"노트 추가됨 - 시간: {currentTime:F2}초, 트랙: {track}");
        }
        
        // UI 버튼 이벤트 핸들러들
        public void OnLoadAudioButtonClicked()
        {
            string path = audioPathInputField != null ? audioPathInputField.text : "";
            if (string.IsNullOrEmpty(path))
            {
                path = "C:/Users/Default/Music/test.mp3"; // 기본 경로
            }
            StartCoroutine(LoadAudio(path));
        }
        
        public void OnPlayButtonClicked()
        {
            if (audioSource.clip != null)
            {
                audioSource.Play();
                isPlaying = true;
                isRecording = true; // 재생 시 노트 녹음 활성화
                Debug.Log("오디오 재생 시작 - 녹음 모드 활성화");
            }
        }
        
        public void OnPauseButtonClicked()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                isRecording = false;
                Debug.Log("오디오 일시정지 - 녹음 모드 비활성화");
            }
            else if (audioSource.clip != null)
            {
                audioSource.UnPause();
                isRecording = true;
                Debug.Log("오디오 재개 - 녹음 모드 활성화");
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
                
            Debug.Log("오디오 정지 - 녹음 모드 비활성화");
        }
        
        public void OnSaveChartButtonClicked()
        {
            SaveChart();
        }
        
        public void OnLoadChartButtonClicked()
        {
            LoadChart();
        }
        
        // 핵심 기능 메서드들
        IEnumerator LoadAudio(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("오디오 경로가 비어있습니다");
                yield break;
            }
            
            string url = "file:///" + path.Replace("\\", "/");
            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"오디오 로드 실패: {www.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    
                    // UI 업데이트
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
                    
                    Debug.Log($"오디오 성공적으로 로드됨: {clip.name} ({clip.length:F2}초)");
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
            // 차트 메타데이터 업데이트
            currentChart.songName = songName;
            currentChart.artistName = artistName;
            currentChart.bpm = bpm;
            
            try
            {
                string json = JsonUtility.ToJson(currentChart, true);
                string path = Path.Combine(Application.persistentDataPath, "chart.json");
                File.WriteAllText(path, json);
                
                Debug.Log($"차트가 성공적으로 저장됨: {path}");
                Debug.Log($"저장된 노트 총 개수: {currentChart.GetNoteCount()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"차트 저장 실패: {e.Message}");
            }
        }
        
        void LoadChart()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "chart.json");
                
                if (!File.Exists(path))
                {
                    Debug.LogWarning("차트 파일을 찾을 수 없습니다!");
                    return;
                }
                
                string json = File.ReadAllText(path);
                currentChart = JsonUtility.FromJson<ChartDataNew>(json);
                
                // 에디터 설정 업데이트
                songName = currentChart.songName;
                artistName = currentChart.artistName;
                bpm = currentChart.bpm;
                
                Debug.Log($"차트가 성공적으로 로드됨: {path}");
                Debug.Log($"로드된 노트 총 개수: {currentChart.GetNoteCount()}");
                
                // 연관된 오디오 파일 로드 시도
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
                Debug.LogError($"차트 로드 실패: {e.Message}");
                currentChart = new ChartDataNew(); // 빈 차트로 재설정
            }
        }
        
        string FormatTime(float time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            int milliseconds = (int)((time - (int)time) * 100);
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
        
        // 외부 접근을 위한 공개 메서드들
        public ChartDataNew GetCurrentChart()
        {
            return currentChart;
        }
        
        public void SetChart(ChartDataNew chart)
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
            
            // 기존 노트들의 비트 타이밍 재계산
            foreach (var note in currentChart.notes)
            {
                note.CalculateBeatTiming(bpm);
            }
        }
        
        public void SetSelectedKeySoundType(KeySoundType keySoundType)
        {
            selectedKeySoundType = keySoundType;
        }
        
        public void ClearChart()
        {
            currentChart.Clear();
            Debug.Log("차트가 초기화됨");
        }
        
        // 유틸리티 메서드들
        public void RemoveNotesInTimeRange(float startTime, float endTime)
        {
            currentChart.notes.RemoveAll(note => note.timing >= startTime && note.timing <= endTime);
            Debug.Log($"{startTime:F2}초와 {endTime:F2}초 사이의 노트들이 제거됨");
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
            Debug.Log($"노트들이 1/{beatDivision} 박자로 양자화됨");
        }
        
        void OnDestroy()
        {
            // 정리
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        // 디버그 및 테스트 메서드들
        [ContextMenu("테스트 노트 추가")]
        void AddTestNotes()
        {
            for (int i = 0; i < 10; i++)
            {
                float time = i * 0.5f; // 0.5초마다 노트
                int track = i % trackKeys.Length;
                
                NoteData noteData = new NoteData(time, track, KeySoundType.Kick);
                noteData.CalculateBeatTiming(bpm);
                currentChart.AddNote(noteData);
            }
            Debug.Log("10개의 테스트 노트가 추가됨");
        }
        
        [ContextMenu("차트 정보 출력")]
        void PrintChartInfo()
        {
            Debug.Log($"차트 정보:");
            Debug.Log($"곡: {currentChart.songName} by {currentChart.artistName}");
            Debug.Log($"BPM: {currentChart.bpm}");
            Debug.Log($"노트: {currentChart.GetNoteCount()}");
            Debug.Log($"길이: {currentChart.GetChartDuration():F2}초");
        }
    }
}