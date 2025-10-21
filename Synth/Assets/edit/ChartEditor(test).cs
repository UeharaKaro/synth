
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 필요
using System.IO;
using UnityEngine.Networking;
/*
[System.Serializable]
public class ChartData
{
    public string audioFileName;
    
    public float bpm; // 곡의 BPM 정보 추가
    public List<NoteData> notes = new List<NoteData>();
}

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour
{
    // UI 요소 연결
    [Header("UI 요소")] public InputField audioPathInputField; // 오디오 파일 경로 입력 필드
    public Slider timelineSlider; // 노래 진행 상태를 보여줄 슬라이더
    public Text currentTimeText; // 현재 시간 표시 텍스트
    public Text totalTimeText; // 전체 시간 표시 텍스트

    // --- 내부 변수 ---
    private AudioSource audioSource;
    private ChartData currentChart;
    private string audioFilePath;
} // 임시 저장용 중괄호 , 편집시 해제할것


void Start()
{
    audioSource = GetComponent<AudioSource>();
    currentChart = new ChartData();

    // 슬라이더 값 변경 시 오디오 위치 이동
    timelineSlider.onValueChanged.AddListener(SeekAudio);
}

void Update()
{
    // 오디오가 재생 중일 때 슬라이더와 시간 텍스트 업데이트
    if (audioSource.isPlaying)
    {
        timelineSlider.value = audioSource.time;
        currentTimeText.text = FormatTime(audioSource.time);
    }
}
} */
 // 임시 저장용 주석, 편집시 해제할것

// --- UI 상호작용 함수 ---
/*
// "오디오 파일 불러오기" 버튼 클릭 시 호출
public void OnLoadAudioButtonClicked()
{
    // 사용자가 입력한 경로를 가져옴
    string path = audioPathInputField.text;
    StartCoroutine(LoadAudio(path));
}

// "차트 저장" 버튼 클릭 시 호출
public void OnSaveChartButtonClicked()
{
    SaveChart();
}

// "차트 불러오기" 버튼 클릭 시 호출
public void OnLoadChartButtonClicked()
{
    LoadChart();
}

// --- 핵심 로직 함수 ---

// 오디오 파일을 불러와 재생
IEnumerator LoadAudio(string path)
{
    // 파일 경로가 비어있으면 기본 경로 사용
    if (string.IsNullOrEmpty(path))
    {
        path = "C:/Users/Kdh39/Downloads/test.mp3"; // 기본 경로
    }

    // 파일 경로를 URL 형식으로 변환
    string url = string.Format("file:///{0}", path);
    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
    {
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            // 오디오 클립을 성공적으로 불러왔을 때
            audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.Play();

            // UI 업데이트
            timelineSlider.maxValue = audioSource.clip.length;
            totalTimeText.text = FormatTime(audioSource.clip.length);
            audioFilePath = path; // 파일 경로 저장
            currentChart.audioFileName = Path.GetFileName(path); // 파일 이름 저장
        }
    }
}

// 슬라이더를 조작하여 오디오 재생 위치 변경
void SeekAudio(float time)
{
    // 오디오가 재생 중이 아닐 때만 조작 가능하도록
    if (!audioSource.isPlaying)
    {
        audioSource.time = time;
        currentTimeText.text = FormatTime(time);
    }
}

// 노트 추가 (예시: 'A' 키를 누르면 현재 시간에 노트 추가)
void AddNote()
{
    if (Input.GetKeyDown(KeyCode.A)) // 'A' 키 입력 감지
    {
        NoteData newNote = new NoteData();
        newNote.time = audioSource.time;
        newNote.type = "normal"; // 노트 타입 (예시)
        currentChart.notes.Add(newNote);
        Debug.Log("Note added at: " + newNote.time);
    }
}

// 차트 데이터를 JSON 파일로 저장
void SaveChart()
{
    string json = JsonUtility.ToJson(currentChart, true);
    string path = Path.Combine(Application.persistentDataPath, "chart.json");
    File.WriteAllText(path, json);
    Debug.Log("Chart saved to: " + path);
}

// JSON 파일에서 차트 데이터를 불러오기
void LoadChart()
{
    string path = Path.Combine(Application.persistentDataPath, "chart.json");
    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        currentChart = JsonUtility.FromJson<ChartData>(json);
        Debug.Log("Chart loaded from: " + path);

        // 오디오 파일도 함께 불러오기
        if (!string.IsNullOrEmpty(currentChart.audioFileName))
        {
            // 저장된 오디오 파일 이름으로 경로 재구성 (경로는 상황에 맞게 조정 필요)
            string audioPath = "C:/Users/Kdh39/Downloads/" + currentChart.audioFileName;
            StartCoroutine(LoadAudio(audioPath));
        }
    }
    else
    {
        Debug.LogWarning("Chart file not found!");
    }
}

// 시간을 "분:초" 형식의 문자열로 변환
string FormatTime(float time)
{
    int minutes = (int)time / 60;
    int seconds = (int)time % 60;
    return string.Format("{0:00}:{1:00}", minutes, seconds);
}

*/
// ----- GEMINI가 생성한 코드 시작 (참고 및 병합용) -----
/*
// using UnityEngine;
// using UnityEditor;
// using FMODUnity;
// using FMOD.Studio;
// using System.Collections.Generic;
// using System.Linq;
// 
// /// <summary>
// /// FMOD와 연동하여 리듬 게임의 차트를 제작하는 에디터 클래스입니다.
// /// </summary>
// public class ChartEditor_Gemini : EditorWindow
// {
//     #region Variables
//     // 데이터 에셋
//     private ChartData currentChart;
//     private GameSettings gameSettings;
// 
//     // FMOD
//     private EventInstance musicInstance;
//     private PLAYBACK_STATE playbackState;
//     private int currentPlaybackTimeMs = 0;
//     private int totalPlaybackTimeMs = 0;
// 
//     // UI & 타임라인
//     private int selectedLaneCount = 4;
//     private Vector2 scrollPosition;
//     private bool isSeeking = false;
//     private Rect timelineRect;
//     public enum GridSnap { None, Beat_1_4, Beat_1_8, Beat_1_16, Beat_1_32 }
//     private GridSnap gridSnapValue = GridSnap.Beat_1_16;
//     private float timelineZoom = 1.0f;
//     private NoteData.NoteType currentNoteType = NoteData.NoteType.Normal;
//     private Dictionary<int, NoteData> activeLongNotes = new Dictionary<int, NoteData>();
//     #endregion
// 
//     [MenuItem("Window/Rhythm Game/Chart Editor by Gemini")]
//     public static void ShowWindow() { GetWindow<ChartEditor_Gemini>("Chart Editor by Gemini"); }
// 
//     #region Unity Lifecycle
//     private void OnEnable() { EditorApplication.update += EditorUpdate; }
//     private void OnDisable() { EditorApplication.update -= EditorUpdate; StopMusic(); }
// 
//     /// <summary> 에디터 프레임마다 호출되어 오디오 상태를 업데이트합니다. </summary>
//     private void EditorUpdate()
//     {
//         if (isSeeking) return;
//         if (musicInstance.isValid())
//         {
//             musicInstance.getPlaybackState(out playbackState);
//             if (playbackState == PLAYBACK_STATE.PLAYING)
//             {
//                 musicInstance.getTimelinePosition(out currentPlaybackTimeMs);
//                 Repaint();
//             }
//         }
//     }
// 
//     /// <summary> 에디터의 모든 UI를 그리고 이벤트를 처리하는 메인 함수입니다. </summary>
//     private void OnGUI()
//     {
//         HandleInputs();
// 
//         scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
//         
//         DrawSetupUI();
//         if (currentChart == null || gameSettings == null) { EditorGUILayout.EndScrollView(); return; }
//         
//         DrawAudioControlsUI();
//         EditorGUILayout.Space(20);
//         DrawTimelineUI();
//         
//         if (GUI.changed) { EditorUtility.SetDirty(currentChart); EditorUtility.SetDirty(gameSettings); }
//         
//         EditorGUILayout.EndScrollView();
//     }
//     #endregion
// 
//     #region Input Handling
//     void HandleInputs()
//     {
//         Event e = Event.current;
//         if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None) HandleKeyDown(e);
//         if (e.type == EventType.KeyUp && e.keyCode != KeyCode.None) HandleKeyUp(e);
//         if (timelineRect.Contains(e.mousePosition))
//         {
//             if (e.type == EventType.MouseDown && e.button == 0) HandleLeftClick(e.mousePosition);
//             if (e.type == EventType.MouseDown && e.button == 1) HandleRightClick(e.mousePosition);
//             Repaint();
//         }
//     }
// 
//     void HandleKeyDown(Event e)
//     {
//         int lane = gameSettings.keybindings.IndexOf(e.keyCode);
//         if (lane != -1 && lane < selectedLaneCount)
//         {
//             if (currentNoteType == NoteData.NoteType.Normal) CreateNote(lane, currentPlaybackTimeMs);
//             else if (currentNoteType == NoteData.NoteType.Long && !activeLongNotes.ContainsKey(lane)) StartLongNote(lane, currentPlaybackTimeMs);
//             e.Use();
//         }
//     }
// 
//     void HandleKeyUp(Event e)
//     {
//         int lane = gameSettings.keybindings.IndexOf(e.keyCode);
//         if (lane != -1 && activeLongNotes.ContainsKey(lane)) { EndLongNote(lane, currentPlaybackTimeMs); e.Use(); }
//     }
// 
//     void HandleLeftClick(Vector2 mousePos)
//     {
//         int lane = GetLaneFromMousePosition(mousePos.y);
//         long time = GetTimeFromMousePosition(mousePos.x);
//         CreateNote(lane, time);
//     }
// 
//     void HandleRightClick(Vector2 mousePos)
//     {
//         int lane = GetLaneFromMousePosition(mousePos.y);
//         long time = GetTimeFromMousePosition(mousePos.x);
//         NoteData noteToDelete = FindNoteAt(lane, time, 10f);
//         if (noteToDelete != null) { currentChart.notes.Remove(noteToDelete); SortAndSaveChanges(); }
//     }
//     #endregion
// 
//     #region Note Management
//     void CreateNote(int laneIndex, long timeMs)
//     {
//         if (currentNoteType == NoteData.NoteType.Normal)
//         {
//             currentChart.notes.Add(new NoteData { laneIndex = laneIndex, timestamp = SnapToGrid(timeMs), type = NoteData.NoteType.Normal, duration = 0 });
//             SortAndSaveChanges();
//         }
//         else if (currentNoteType == NoteData.NoteType.Long)
//         {
//             Debug.Log("롱노트는 키보드를 누르고 떼서 입력해주세요.");
//         }
//     }
// 
//     void StartLongNote(int laneIndex, long timeMs)
//     {
//         NoteData newNote = new NoteData { laneIndex = laneIndex, timestamp = SnapToGrid(timeMs), type = NoteData.NoteType.Long, duration = 0 };
//         currentChart.notes.Add(newNote);
//         activeLongNotes[laneIndex] = newNote;
//         SortAndSaveChanges();
//     }
// 
//     void EndLongNote(int laneIndex, long timeMs)
//     {
//         NoteData note = activeLongNotes[laneIndex];
//         note.duration = SnapToGrid(timeMs) - note.timestamp;
//         if (note.duration < 50) { currentChart.notes.Remove(note); } 
//         activeLongNotes.Remove(laneIndex);
//         SortAndSaveChanges();
//     }
// 
//     NoteData FindNoteAt(int lane, long time, float pixelThreshold)
//     {
//         float pixelsPerMs = (timelineRect.width / 1000.0f) * timelineZoom;
//         long timeThreshold = (long)(pixelThreshold / pixelsPerMs);
//         return currentChart.notes.FirstOrDefault(n => n.laneIndex == lane && Mathf.Abs(n.timestamp - time) < timeThreshold);
//     }
// 
//     void SortAndSaveChanges()
//     {
//         currentChart.notes.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
//         EditorUtility.SetDirty(currentChart);
//         Repaint();
//     }
//     #endregion
// 
//     #region UI Drawing
//     void DrawSetupUI()
//     {
//         EditorGUILayout.LabelField("1. 데이터 에셋 설정", EditorStyles.centeredGreyMiniLabel);
//         currentChart = (ChartData)EditorGUILayout.ObjectField("현재 차트 (ChartData)", currentChart, typeof(ChartData), false);
//         gameSettings = (GameSettings)EditorGUILayout.ObjectField("게임 설정 (GameSettings)", gameSettings, typeof(GameSettings), false);
//         EditorGUILayout.Space(20);
//         if (currentChart == null || gameSettings == null) { EditorGUILayout.HelpBox("먼저 ChartData와 GameSettings 에셋을 할당해주세요.", MessageType.Info); return; }
// 
//         EditorGUILayout.LabelField("2. 차트 기본 정보", EditorStyles.centeredGreyMiniLabel);
//         SerializedObject chartObject = new SerializedObject(currentChart);
//         EditorGUILayout.PropertyField(chartObject.FindProperty("songName"));
//         EditorGUILayout.PropertyField(chartObject.FindProperty("artistName"));
//         EditorGUILayout.PropertyField(chartObject.FindProperty("initialBpm"));
//         EditorGUILayout.PropertyField(chartObject.FindProperty("fmodEventPath"));
//         chartObject.ApplyModifiedProperties();
//         EditorGUILayout.Space(20);
// 
//         EditorGUILayout.LabelField("3. 에디터 설정", EditorStyles.centeredGreyMiniLabel);
//         selectedLaneCount = EditorGUILayout.IntSlider("레인 수", selectedLaneCount, 4, 8);
//         if (gameSettings.keybindings.Count != selectedLaneCount) { while (gameSettings.keybindings.Count < selectedLaneCount) gameSettings.keybindings.Add(KeyCode.None); while (gameSettings.keybindings.Count > selectedLaneCount) gameSettings.keybindings.RemoveAt(gameSettings.keybindings.Count - 1); }
//         EditorGUILayout.LabelField("키 설정:");
//         for (int i = 0; i < selectedLaneCount; i++) { gameSettings.keybindings[i] = (KeyCode)EditorGUILayout.EnumPopup($"  레인 {i + 1} 키", gameSettings.keybindings[i]); }
//         EditorGUILayout.Space(20);
//     }
// 
//     void DrawAudioControlsUI()
//     {
//         EditorGUILayout.LabelField("4. 오디오 제어", EditorStyles.centeredGreyMiniLabel);
//         EditorGUI.BeginChangeCheck();
//         var newTime = EditorGUILayout.IntSlider("타임라인", currentPlaybackTimeMs, 0, totalPlaybackTimeMs);
//         if (EditorGUI.EndChangeCheck()) { isSeeking = true; currentPlaybackTimeMs = newTime; if(musicInstance.isValid()) musicInstance.setTimelinePosition(currentPlaybackTimeMs); }
//         if (Event.current.type == EventType.MouseUp && isSeeking) { isSeeking = false; }
//         EditorGUILayout.LabelField("시간", $"{currentPlaybackTimeMs / 1000.0f:00.000} / {totalPlaybackTimeMs / 1000.0f:00.000} 초");
//         EditorGUILayout.BeginHorizontal();
//         if (playbackState != PLAYBACK_STATE.PLAYING) { if (GUILayout.Button("Play")) PlayMusic(); }
//         else { if (GUILayout.Button("Pause")) musicInstance.setPaused(true); }
//         if (playbackState == PLAYBACK_STATE.PAUSED) { if (GUILayout.Button("Resume")) musicInstance.setPaused(false); }
//         if (GUILayout.Button("Stop")) StopMusic();
//         EditorGUILayout.EndHorizontal();
//     }
// 
//     void DrawTimelineUI()
//     {
//         EditorGUILayout.LabelField("5. 타임라인 & 노트", EditorStyles.centeredGreyMiniLabel);
//         currentNoteType = (NoteData.NoteType)EditorGUILayout.EnumPopup("노트 종류", currentNoteType);
//         gridSnapValue = (GridSnap)EditorGUILayout.EnumPopup("그리드 분할", gridSnapValue);
//         timelineZoom = EditorGUILayout.Slider("타임라인 확대", timelineZoom, 0.1f, 10f);
//         timelineRect = GUILayoutUtility.GetRect(100, 10000, 300, 300);
//         GUI.Box(timelineRect, "");
//         Handles.BeginGUI();
//         DrawGridLines();
//         DrawNotes();
//         DrawPlayhead();
//         Handles.EndGUI();
//     }
// 
//     void DrawNotes()
//     {
//         if (currentChart == null) return;
//         float pixelsPerMs = (timelineRect.width / 1000.0f) * timelineZoom;
//         float laneHeight = timelineRect.height / selectedLaneCount;
//         foreach (var note in currentChart.notes)
//         {
//             float noteX = GetXForTime(note.timestamp);
//             float noteY = timelineRect.y + note.laneIndex * laneHeight;
//             float noteWidth = (note.type == NoteData.NoteType.Long && note.duration > 0) ? note.duration * pixelsPerMs : 10f;
//             if (noteX < timelineRect.x - noteWidth || noteX > timelineRect.xMax) continue;
//             Rect noteRect = new Rect(noteX, noteY, noteWidth, laneHeight);
//             GUI.color = note.type == NoteData.NoteType.Long ? Color.green : Color.blue;
//             GUI.DrawTexture(noteRect, EditorGUIUtility.whiteTexture);
//             GUI.color = Color.white;
//         }
//     }
// 
//     void DrawGridLines()
//     {
//         if (currentChart == null || currentChart.initialBpm <= 0) return;
//         float pixelsPerMs = (timelineRect.width / 1000.0f) * timelineZoom;
//         float beatDurationMs = (60.0f / currentChart.initialBpm) * 1000.0f;
//         if (beatDurationMs <= 0) return;
//         float stepMs = beatDurationMs / 4.0f; // 1/16 beat
//         long startTime = GetTimeFromMousePosition(timelineRect.x);
//         long endTime = GetTimeFromMousePosition(timelineRect.xMax);
//         int startStep = Mathf.FloorToInt(startTime / stepMs);
//         int endStep = Mathf.CeilToInt(endTime / stepMs);
//         for (int i = startStep; i < endStep; i++)
//         {
//             long time = (long)(i * stepMs);
//             float x = GetXForTime(time);
//             if (i % 16 == 0) Handles.color = Color.white;
//             else if (i % 4 == 0) Handles.color = Color.gray;
//             else Handles.color = new Color(0.3f, 0.3f, 0.3f);
//             Handles.DrawLine(new Vector3(x, timelineRect.y), new Vector3(x, timelineRect.yMax));
//         }
//     }
// 
//     void DrawPlayhead()
//     {
//         float playheadX = GetXForTime(currentPlaybackTimeMs);
//         Handles.color = Color.red;
//         Handles.DrawLine(new Vector3(playheadX, timelineRect.y), new Vector3(playheadX, timelineRect.yMax));
//     }
//     #endregion
// 
//     #region Utility Methods
//     long GetTimeFromMousePosition(float mouseX)
//     {
//         float pixelsPerMs = (timelineRect.width / 1000.0f) * timelineZoom;
//         float relativeX = mouseX - timelineRect.x;
//         return currentPlaybackTimeMs + (long)((relativeX - (timelineRect.width / 2)) / pixelsPerMs);
//     }
// 
//     float GetXForTime(long timeMs)
//     {
//         float pixelsPerMs = (timelineRect.width / 1000.0f) * timelineZoom;
//         return timelineRect.x + (timelineRect.width / 2) + (timeMs - currentPlaybackTimeMs) * pixelsPerMs;
//     }
// 
//     int GetLaneFromMousePosition(float mouseY) { return Mathf.Clamp(Mathf.FloorToInt((mouseY - timelineRect.y) / (timelineRect.height / selectedLaneCount)), 0, selectedLaneCount - 1); }
// 
//     long SnapToGrid(long timeMs)
//     {
//         if (gridSnapValue == GridSnap.None || currentChart.initialBpm <= 0) return timeMs;
//         float beatDurationMs = (60.0f / currentChart.initialBpm) * 1000.0f;
//         float division = 4.0f; 
//         if (gridSnapValue == GridSnap.Beat_1_8) division = 8.0f; else if (gridSnapValue == GridSnap.Beat_1_16) division = 16.0f; else if (gridSnapValue == GridSnap.Beat_1_32) division = 32.0f;
//         float stepMs = beatDurationMs / (division / 4.0f);
//         return (long)(Mathf.Round(timeMs / stepMs) * stepMs);
//     }
//     #endregion
// 
//     #region FMOD Control
//     void PlayMusic()
//     {
//         if (currentChart == null || string.IsNullOrEmpty(currentChart.fmodEventPath)) return;
//         StopMusic();
//         musicInstance = RuntimeManager.CreateInstance(currentChart.fmodEventPath);
//         EventDescription desc;
//         musicInstance.getDescription(out desc);
//         desc.getLength(out totalPlaybackTimeMs);
//         musicInstance.start();
//         if(currentPlaybackTimeMs > 0) musicInstance.setTimelinePosition(currentPlaybackTimeMs);
//     }
// 
//     void StopMusic()
//     {
//         if (musicInstance.isValid()) { musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); musicInstance.release(); musicInstance.clearHandle(); currentPlaybackTimeMs = 0; playbackState = PLAYBACK_STATE.STOPPED; }
//     }
//     #endregion
// }
*/

