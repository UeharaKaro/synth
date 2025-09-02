using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public enum EditorMode
{
    Normal,     // 일반 노트 배치 모드 (N키)
    LongNote,   // 롱노트 배치 모드 (L키)
    Select,     // 선택 모드
    Preview     // 미리보기 모드 (P키)
}

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour
{
    [Header("UI 요소")]
    public InputField audioPathInputField;
    public Slider timelineSlider;
    public Text currentTimeText;
    public Text totalTimeText;
    public Text bpmText;
    public Text modeText;
    public Text laneCountText;
    public Text divisionText;
    public Button playButton;
    public Button stopButton;
    public Button saveButton;
    public Button loadButton;
    
    [Header("에디터 설정")]
    public Camera editorCamera;
    public Transform laneContainer;
    public GameObject lanePrefab;
    public GameObject normalNotePrefab;
    public GameObject longNotePrefab;
    public GameObject gridLinePrefab;
    public float laneWidth = 1.0f;
    public float laneSpacing = 0.1f;
    public float noteSnapDistance = 0.5f;
    
    [Header("스크롤 및 타임라인")]
    public float scrollRange = 20f; // 화면에 보이는 시간 범위 (초)
    public float judgmentLineY = -8f; // 판정선 Y 위치
    public float noteSpawnY = 12f; // 노트 생성 Y 위치
    public bool showGrid = true;
    public bool showTimeline = true;
    
    // 에디터 상태
    private EditorMode currentMode = EditorMode.Normal;
    private ChartData currentChart;
    private AudioSource audioSource;
    private string audioFilePath;
    private bool isPlaying = false;
    private bool isPreviewMode = false;
    
    // 레인 시스템
    private int currentLaneCount = 4;
    private int[] supportedLaneCounts = { 4, 5, 6, 7, 8, 10 };
    private int currentLaneIndex = 0;
    private List<Transform> lanes = new List<Transform>();
    
    // 마디/분할 시스템
    private int beatDivision = 4; // 1/4 분할
    private float gridSnapTime = 0f;
    
    // 노트 배치 시스템
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private GameObject previewNote;
    private int currentTrack = 0;
    private double longNoteStartTime = 0;
    
    // 오디오 제어
    private float playbackSpeed = 1.0f;
    private float audioOffset = 0f; // ms 단위
    
    // 실행 취소/다시 실행
    private Stack<ChartData> undoStack = new Stack<ChartData>();
    private Stack<ChartData> redoStack = new Stack<ChartData>();
    private const int maxUndoSteps = 50;
    
    // 노트 오브젝트 관리
    private List<GameObject> noteObjects = new List<GameObject>();
    private List<NoteData> selectedNotes = new List<NoteData>();
    private List<GameObject> gridLines = new List<GameObject>();
    
    void Start()
    {
        InitializeEditor();
    }
    
    void InitializeEditor()
    {
        audioSource = GetComponent<AudioSource>();
        currentChart = new ChartData();
        
        // UI 이벤트 연결
        if (timelineSlider != null)
            timelineSlider.onValueChanged.AddListener(SeekAudio);
        if (playButton != null)
            playButton.onClick.AddListener(TogglePlayback);
        if (stopButton != null)
            stopButton.onClick.AddListener(StopPlayback);
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveChart);
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadChart);
            
        SetupLanes();
        SetupGrid();
        UpdateUI();
        
        Debug.Log("차트 에디터 초기화 완료");
    }
    
    void Update()
    {
        HandleInput();
        UpdateTimeline();
        UpdateGrid();
        UpdateNoteDisplay();
        UpdateUI();
    }
    
    void HandleInput()
    {
        // 모드 전환 키
        if (Input.GetKeyDown(KeyCode.N))
            SetMode(EditorMode.Normal);
        else if (Input.GetKeyDown(KeyCode.L))
            SetMode(EditorMode.LongNote);
        else if (Input.GetKeyDown(KeyCode.P))
            TogglePreviewMode();
            
        // 레인 수 조절
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            ChangeLaneCount(1);
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            ChangeLaneCount(-1);
            
        // 마디 분할 조절
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetBeatDivision(4);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetBeatDivision(8);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetBeatDivision(16);
        
        // 오디오 속도 조절 (Shift + 화살표)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                SetPlaybackSpeed(playbackSpeed + 0.1f);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                SetPlaybackSpeed(playbackSpeed - 0.1f);
        }
        
        // 스크롤 속도 조절 (Ctrl + 화살표)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                ChangeScrollSpeed(1f);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                ChangeScrollSpeed(-1f);
        }
        
        // 오디오 오프셋 조절 (Alt + 화살표)
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                SetAudioOffset(audioOffset - 5f);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                SetAudioOffset(audioOffset + 5f);
        }
            
        // 재생 제어
        if (Input.GetKeyDown(KeyCode.Space))
            TogglePlayback();
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            SaveChart();
        if (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftControl))
            LoadChart();
            
        // 실행 취소/다시 실행
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
            Undo();
        else if ((Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl)) || 
                 (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift)))
            Redo();
            
        // 삭제
        if (Input.GetKeyDown(KeyCode.Delete))
            DeleteSelectedNotes();
        
        // 전체 선택
        if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftControl))
            SelectAllNotes();
            
        // 마우스 입력 (미리보기 모드가 아닐 때만)
        if (!isPreviewMode)
        {
            HandleMouseInput();
        }
    }
    
    void ChangeScrollSpeed(float delta)
    {
        currentChart.scrollSpeed = Mathf.Clamp(currentChart.scrollSpeed + delta, 1f, 20f);
        Debug.Log($"스크롤 속도 변경: {currentChart.scrollSpeed:F1}");
    }
    
    void SelectAllNotes()
    {
        selectedNotes.Clear();
        selectedNotes.AddRange(currentChart.notes);
        Debug.Log($"전체 노트 선택: {selectedNotes.Count}개");
    }
    
    void HandleMouseInput()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown(mousePos);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag(mousePos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp(mousePos);
        }
    }
    
    void OnMouseDown(Vector3 worldPos)
    {
        currentTrack = GetTrackFromPosition(worldPos.x);
        if (currentTrack < 0 || currentTrack >= currentLaneCount) return;
        
        switch (currentMode)
        {
            case EditorMode.Normal:
                PlaceNormalNote(worldPos);
                break;
            case EditorMode.LongNote:
                StartLongNote(worldPos);
                break;
            case EditorMode.Select:
                StartSelection(worldPos);
                break;
        }
    }
    
    void OnMouseDrag(Vector3 worldPos)
    {
        if (!isDragging) return;
        
        switch (currentMode)
        {
            case EditorMode.LongNote:
                UpdateLongNotePreview(worldPos);
                break;
            case EditorMode.Select:
                UpdateSelection(worldPos);
                break;
        }
    }
    
    void OnMouseUp(Vector3 worldPos)
    {
        switch (currentMode)
        {
            case EditorMode.LongNote:
                FinishLongNote(worldPos);
                break;
            case EditorMode.Select:
                FinishSelection(worldPos);
                break;
        }
        
        isDragging = false;
        if (previewNote != null)
        {
            DestroyImmediate(previewNote);
            previewNote = null;
        }
    }
    
    void PlaceNormalNote(Vector3 worldPos)
    {
        double timing = GetTimingFromPosition(worldPos.y);
        timing = SnapToGrid(timing);
        
        // 같은 위치에 노트가 있는지 확인
        if (HasNoteAt(timing, currentTrack))
        {
            RemoveNoteAt(timing, currentTrack);
            return;
        }
        
        SaveUndoState();
        NoteData newNote = new NoteData(timing, currentTrack, KeySoundType.None, false, 0);
        currentChart.AddNote(newNote);
        
        Debug.Log($"일반 노트 배치: Track {currentTrack}, Time {timing:F2}");
    }
    
    void StartLongNote(Vector3 worldPos)
    {
        isDragging = true;
        dragStartPosition = worldPos;
        longNoteStartTime = SnapToGrid(GetTimingFromPosition(worldPos.y));
        
        // 프리뷰 노트 생성
        if (longNotePrefab != null)
        {
            previewNote = Instantiate(longNotePrefab);
            previewNote.transform.position = worldPos;
        }
    }
    
    void UpdateLongNotePreview(Vector3 worldPos)
    {
        if (previewNote == null) return;
        
        // 롱노트 프리뷰 업데이트 로직
        Vector3 startPos = dragStartPosition;
        Vector3 endPos = worldPos;
        
        // 프리뷰 노트의 길이 조정
        float distance = Vector3.Distance(startPos, endPos);
        previewNote.transform.position = (startPos + endPos) / 2;
        previewNote.transform.localScale = new Vector3(1, distance, 1);
    }
    
    void FinishLongNote(Vector3 worldPos)
    {
        double endTiming = SnapToGrid(GetTimingFromPosition(worldPos.y));
        
        if (endTiming <= longNoteStartTime)
        {
            Debug.LogWarning("롱노트 길이가 너무 짧습니다");
            return;
        }
        
        SaveUndoState();
        NoteData longNote = new NoteData(longNoteStartTime, currentTrack, KeySoundType.None, true, endTiming);
        currentChart.AddNote(longNote);
        
        Debug.Log($"롱노트 배치: Track {currentTrack}, Start {longNoteStartTime:F2}, End {endTiming:F2}");
    }
    
    int GetTrackFromPosition(float xPos)
    {
        float totalWidth = (currentLaneCount - 1) * (laneWidth + laneSpacing);
        float startX = -totalWidth / 2;
        
        for (int i = 0; i < currentLaneCount; i++)
        {
            float laneX = startX + i * (laneWidth + laneSpacing);
            if (Mathf.Abs(xPos - laneX) < laneWidth / 2)
            {
                return i;
            }
        }
        return -1;
    }
    
    double GetTimingFromPosition(float yPos)
    {
        if (!isPlaying && audioSource.clip != null)
        {
            // 정지 상태에서는 타임라인 기준
            float normalizedY = (yPos - judgmentLineY) / (noteSpawnY - judgmentLineY);
            return timelineSlider.value + normalizedY * scrollRange;
        }
        else
        {
            // 재생 중에는 현재 시간 기준
            float normalizedY = (yPos - judgmentLineY) / (noteSpawnY - judgmentLineY);
            return audioSource.time + normalizedY * scrollRange;
        }
    }
    
    double SnapToGrid(double timing)
    {
        if (currentChart.bpm <= 0) return timing;
        
        double beatLength = 60.0 / currentChart.bpm; // 한 박자 길이 (초)
        double snapInterval = beatLength / beatDivision; // 스냅 간격
        
        return Math.Round(timing / snapInterval) * snapInterval;
    }
    
    bool HasNoteAt(double timing, int track)
    {
        foreach (var note in currentChart.notes)
        {
            if (Mathf.Approximately((float)note.timing, (float)timing) && note.track == track)
                return true;
        }
        return false;
    }
    
    void RemoveNoteAt(double timing, int track)
    {
        SaveUndoState();
        currentChart.RemoveNoteAt(timing, track);
        Debug.Log($"노트 삭제: Track {track}, Time {timing:F2}");
    }
    
    void SetMode(EditorMode mode)
    {
        currentMode = mode;
        Debug.Log($"에디터 모드 변경: {mode}");
    }
    
    void TogglePreviewMode()
    {
        isPreviewMode = !isPreviewMode;
        if (isPreviewMode)
        {
            currentMode = EditorMode.Preview;
            if (!isPlaying) TogglePlayback();
        }
        else
        {
            currentMode = EditorMode.Normal;
        }
        Debug.Log($"미리보기 모드: {isPreviewMode}");
    }
    
    void ChangeLaneCount(int direction)
    {
        currentLaneIndex += direction;
        currentLaneIndex = Mathf.Clamp(currentLaneIndex, 0, supportedLaneCounts.Length - 1);
        
        int newLaneCount = supportedLaneCounts[currentLaneIndex];
        if (newLaneCount != currentLaneCount)
        {
            SaveUndoState();
            currentLaneCount = newLaneCount;
            currentChart.laneCount = currentLaneCount;
            SetupLanes();
            Debug.Log($"레인 수 변경: {currentLaneCount}");
        }
    }
    
    void SetBeatDivision(int division)
    {
        beatDivision = division;
        currentChart.beatDivision = division;
        Debug.Log($"마디 분할 변경: 1/{division}");
    }
    
    void SetupLanes()
    {
        // 기존 레인 정리
        foreach (var lane in lanes)
        {
            if (lane != null) DestroyImmediate(lane.gameObject);
        }
        lanes.Clear();
        
        if (laneContainer == null || lanePrefab == null) return;
        
        float totalWidth = (currentLaneCount - 1) * (laneWidth + laneSpacing);
        float startX = -totalWidth / 2;
        
        for (int i = 0; i < currentLaneCount; i++)
        {
            GameObject laneObj = Instantiate(lanePrefab, laneContainer);
            float laneX = startX + i * (laneWidth + laneSpacing);
            laneObj.transform.localPosition = new Vector3(laneX, 0, 0);
            laneObj.name = $"Lane_{i}";
            lanes.Add(laneObj.transform);
        }
    }
    
    void SetupGrid()
    {
        ClearGrid();
        
        if (!showGrid || gridLinePrefab == null || laneContainer == null) return;
        
        // 그리드 라인 생성은 UpdateGrid에서 동적으로 처리
    }
    
    void UpdateGrid()
    {
        if (!showGrid) return;
        
        ClearGrid();
        
        if (gridLinePrefab == null || laneContainer == null || currentChart.bpm <= 0) return;
        
        double currentTime = GetCurrentTime();
        double beatLength = 60.0 / currentChart.bpm; // 한 박자 길이 (초)
        double gridInterval = beatLength / beatDivision; // 그리드 간격
        
        // 화면에 보이는 범위의 그리드 라인 생성
        double displayStart = currentTime;
        double displayEnd = currentTime + scrollRange;
        
        int startBeat = (int)Math.Floor(displayStart / gridInterval);
        int endBeat = (int)Math.Ceil(displayEnd / gridInterval);
        
        for (int i = startBeat; i <= endBeat; i++)
        {
            double beatTime = i * gridInterval;
            if (beatTime >= displayStart && beatTime <= displayEnd)
            {
                CreateGridLine(beatTime, currentTime, i % beatDivision == 0);
            }
        }
    }
    
    void CreateGridLine(double beatTime, double currentTime, bool isMajorBeat)
    {
        GameObject gridLine = Instantiate(gridLinePrefab, laneContainer);
        
        // 그리드 라인 위치 계산
        double timeDiff = beatTime - currentTime;
        float normalizedTime = (float)(timeDiff / scrollRange);
        float y = judgmentLineY + normalizedTime * (noteSpawnY - judgmentLineY);
        
        float totalWidth = (currentLaneCount - 1) * (laneWidth + laneSpacing) + laneWidth;
        float startX = -totalWidth / 2;
        float endX = totalWidth / 2;
        
        gridLine.transform.position = new Vector3(0, y, 0.1f);
        
        // 메이저 비트 (마디 시작)는 더 진하게 표시
        var renderer = gridLine.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = isMajorBeat ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 0.2f);
            renderer.size = new Vector2(totalWidth, 0.1f);
        }
        
        gridLines.Add(gridLine);
    }
    
    void ClearGrid()
    {
        foreach (var gridLine in gridLines)
        {
            if (gridLine != null) DestroyImmediate(gridLine);
        }
        gridLines.Clear();
    }
    
    void UpdateNoteDisplay()
    {
        // 기존 노트 오브젝트 정리
        foreach (var noteObj in noteObjects)
        {
            if (noteObj != null) DestroyImmediate(noteObj);
        }
        noteObjects.Clear();
        
        if (normalNotePrefab == null || longNotePrefab == null) return;
        
        double currentTime = GetCurrentTime();
        
        if (isPreviewMode)
        {
            // 미리보기 모드: 스크롤 속도에 비례한 노트 하강
            double displayStart = currentTime;
            double displayEnd = currentTime + (scrollRange / currentChart.scrollSpeed);
            var visibleNotes = currentChart.GetNotesInRange(displayStart, displayEnd);
            
            foreach (var note in visibleNotes)
            {
                CreatePreviewNoteObject(note, currentTime);
            }
        }
        else
        {
            // 제작 모드: 일정한 마디 길이로 표시
            double displayStart = currentTime;
            double displayEnd = currentTime + scrollRange;
            var visibleNotes = currentChart.GetNotesInRange(displayStart, displayEnd);
            
            foreach (var note in visibleNotes)
            {
                CreateEditNoteObject(note, currentTime);
            }
        }
    }
    
    void CreateEditNoteObject(NoteData note, double currentTime)
    {
        GameObject prefab = note.isLongNote ? longNotePrefab : normalNotePrefab;
        GameObject noteObj = Instantiate(prefab);
        
        // 제작 모드: 일정한 간격으로 노트 배치
        Vector3 position = GetEditNotePosition(note, currentTime);
        noteObj.transform.position = position;
        
        // 선택된 노트 하이라이트
        if (selectedNotes.Contains(note))
        {
            var renderer = noteObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.yellow;
            }
        }
        
        // 롱노트인 경우 길이 조정
        if (note.isLongNote)
        {
            Vector3 endPos = GetEditNotePosition(new NoteData(note.longNoteEndTiming, note.track, note.keySoundType), currentTime);
            float length = Vector3.Distance(position, endPos);
            noteObj.transform.localScale = new Vector3(1, length / noteObj.transform.localScale.y, 1);
        }
        
        noteObjects.Add(noteObj);
    }
    
    void CreatePreviewNoteObject(NoteData note, double currentTime)
    {
        GameObject prefab = note.isLongNote ? longNotePrefab : normalNotePrefab;
        GameObject noteObj = Instantiate(prefab);
        
        // 미리보기 모드: 스크롤 속도에 따른 하강
        Vector3 position = GetPreviewNotePosition(note, currentTime);
        noteObj.transform.position = position;
        
        // 롱노트인 경우 길이 조정
        if (note.isLongNote)
        {
            Vector3 endPos = GetPreviewNotePosition(new NoteData(note.longNoteEndTiming, note.track, note.keySoundType), currentTime);
            float length = Vector3.Distance(position, endPos);
            noteObj.transform.localScale = new Vector3(1, length / noteObj.transform.localScale.y, 1);
        }
        
        noteObjects.Add(noteObj);
    }
    
    Vector3 GetEditNotePosition(NoteData note, double currentTime)
    {
        // 트랙 X 위치
        float totalWidth = (currentLaneCount - 1) * (laneWidth + laneSpacing);
        float startX = -totalWidth / 2;
        float x = startX + note.track * (laneWidth + laneSpacing);
        
        // 제작 모드: 시간 기반 Y 위치 (일정한 간격)
        double timeDiff = note.timing - currentTime;
        float normalizedTime = (float)(timeDiff / scrollRange);
        float y = judgmentLineY + normalizedTime * (noteSpawnY - judgmentLineY);
        
        return new Vector3(x, y, 0);
    }
    
    Vector3 GetPreviewNotePosition(NoteData note, double currentTime)
    {
        // 트랙 X 위치
        float totalWidth = (currentLaneCount - 1) * (laneWidth + laneSpacing);
        float startX = -totalWidth / 2;
        float x = startX + note.track * (laneWidth + laneSpacing);
        
        // 미리보기 모드: 스크롤 속도에 비례한 Y 위치
        double timeDiff = note.timing - currentTime;
        float scrollSpeed = currentChart.scrollSpeed;
        float normalizedTime = (float)(timeDiff * scrollSpeed / scrollRange);
        float y = judgmentLineY + normalizedTime * (noteSpawnY - judgmentLineY);
        
        return new Vector3(x, y, 0);
    }
    
    void UpdateTimeline()
    {
        if (audioSource.clip == null) return;
        
        if (isPlaying)
        {
            timelineSlider.value = audioSource.time;
        }
        
        // 타임라인 슬라이더 범위 설정
        timelineSlider.maxValue = audioSource.clip.length;
    }
    
    void UpdateUI()
    {
        double currentTime = GetCurrentTime();
        
        if (currentTimeText != null)
            currentTimeText.text = FormatTime((float)currentTime);
        if (totalTimeText != null && audioSource.clip != null)
            totalTimeText.text = FormatTime(audioSource.clip.length);
        if (bpmText != null)
            bpmText.text = $"BPM: {currentChart.bpm:F1}";
        if (modeText != null)
        {
            string modeStr = currentMode.ToString();
            if (isPreviewMode) modeStr += " (Preview)";
            modeText.text = $"Mode: {modeStr}";
        }
        if (laneCountText != null)
            laneCountText.text = $"Lanes: {currentLaneCount}";
        if (divisionText != null)
            divisionText.text = $"Division: 1/{beatDivision}";
        
        // 추가 정보 표시
        if (selectedNotes.Count > 0 && modeText != null)
        {
            modeText.text += $" | Selected: {selectedNotes.Count}";
        }
        
        // 재생 속도 및 오프셋 정보
        string additionalInfo = "";
        if (Math.Abs(playbackSpeed - 1.0f) > 0.01f)
        {
            additionalInfo += $" | Speed: {playbackSpeed:F1}x";
        }
        if (Math.Abs(audioOffset) > 0.01f)
        {
            additionalInfo += $" | Offset: {audioOffset:F0}ms";
        }
        if (Math.Abs(currentChart.scrollSpeed - 8f) > 0.01f)
        {
            additionalInfo += $" | Scroll: {currentChart.scrollSpeed:F1}";
        }
        
        if (!string.IsNullOrEmpty(additionalInfo) && modeText != null)
        {
            modeText.text += additionalInfo;
        }
        
        // 차트 통계
        var stats = currentChart.GetStatistics();
        if (totalTimeText != null && stats.totalNotes > 0)
        {
            totalTimeText.text += $" | Notes: {stats.totalNotes} | NPS: {stats.averageNPS:F1}";
        }
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds % 1f) * 1000f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
    
    void SeekAudio(float time)
    {
        if (audioSource.clip != null && !isPlaying)
        {
            audioSource.time = time;
        }
    }
    
    void TogglePlayback()
    {
        if (audioSource.clip == null)
        {
            Debug.LogWarning("오디오 파일이 로드되지 않았습니다");
            return;
        }
        
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
        }
        else
        {
            audioSource.Play();
            isPlaying = true;
        }
    }
    
    void StopPlayback()
    {
        audioSource.Stop();
        audioSource.time = 0;
        timelineSlider.value = 0;
        isPlaying = false;
    }
    
    void SaveUndoState()
    {
        undoStack.Push(currentChart.Clone());
        redoStack.Clear();
        
        // 스택 크기 제한
        while (undoStack.Count > maxUndoSteps)
        {
            var tempStack = new Stack<ChartData>();
            for (int i = 0; i < maxUndoSteps; i++)
            {
                tempStack.Push(undoStack.Pop());
            }
            undoStack = tempStack;
        }
    }
    
    void Undo()
    {
        if (undoStack.Count > 0)
        {
            redoStack.Push(currentChart.Clone());
            currentChart = undoStack.Pop();
            Debug.Log("실행 취소");
        }
    }
    
    void Redo()
    {
        if (redoStack.Count > 0)
        {
            undoStack.Push(currentChart.Clone());
            currentChart = redoStack.Pop();
            Debug.Log("다시 실행");
        }
    }
    
    void DeleteSelectedNotes()
    {
        if (selectedNotes.Count > 0)
        {
            SaveUndoState();
            foreach (var note in selectedNotes)
            {
                currentChart.RemoveNote(note);
            }
            selectedNotes.Clear();
            Debug.Log($"선택된 노트 삭제");
        }
    }
    
    void StartSelection(Vector3 worldPos)
    {
        // 선택 모드 구현 (추후 확장)
        isDragging = true;
        dragStartPosition = worldPos;
    }
    
    void UpdateSelection(Vector3 worldPos)
    {
        // 드래그 선택 영역 업데이트 (추후 구현)
    }
    
    void FinishSelection(Vector3 worldPos)
    {
        // 선택 완료 처리 (추후 구현)
    }
    
    public void LoadAudioFile(string filePath)
    {
        StartCoroutine(LoadAudioCoroutine(filePath));
    }
    
    public void LoadEncryptedAudioFile(TextAsset encryptedAudioAsset)
    {
        StartCoroutine(LoadEncryptedAudioCoroutine(encryptedAudioAsset));
    }
    
    IEnumerator LoadAudioCoroutine(string filePath)
    {
        // 일반 오디오 파일 로드
        using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                currentChart.audioFileName = System.IO.Path.GetFileName(filePath);
                Debug.Log($"오디오 파일 로드 완료: {currentChart.audioFileName}");
            }
            else
            {
                Debug.LogError($"오디오 파일 로드 실패: {www.error}");
            }
        }
    }
    
    IEnumerator LoadEncryptedAudioCoroutine(TextAsset encryptedAudioAsset)
    {
        // AES 암호화된 오디오 파일 로드
        if (encryptedAudioAsset == null)
        {
            Debug.LogError("암호화된 오디오 에셋이 null입니다.");
            yield break;
        }
        
        // 동기 WAV 로더 사용 (빠른 성능)
        AudioClip clip = RuntimeAudioLoader.LoadEncryptedAudio(encryptedAudioAsset);
        if (clip != null)
        {
            audioSource.clip = clip;
            currentChart.audioFileName = encryptedAudioAsset.name;
            Debug.Log($"암호화된 오디오 파일 로드 완료: {currentChart.audioFileName}");
        }
        else
        {
            Debug.LogError("암호화된 오디오 파일 로드 실패");
        }
        
        yield return null;
    }
    
    // 오디오 속도 조절 기능 추가
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Clamp(speed, 0.1f, 2.0f);
        audioSource.pitch = playbackSpeed;
        Debug.Log($"재생 속도 변경: {playbackSpeed:F1}x");
    }
    
    // 오디오 오프셋 조절 기능
    public void SetAudioOffset(float offsetMs)
    {
        audioOffset = offsetMs;
        currentChart.audioOffset = offsetMs;
        Debug.Log($"오디오 오프셋 변경: {offsetMs}ms");
    }
    
    // 현재 재생 시간 (오프셋 적용)
    public double GetCurrentTime()
    {
        double baseTime = isPlaying ? audioSource.time : timelineSlider.value;
        return baseTime + (audioOffset / 1000.0);
    }
    
    void SaveChart()
    {
        string json = JsonUtility.ToJson(currentChart, true);
        string path = Application.persistentDataPath + "/chart.json";
        File.WriteAllText(path, json);
        Debug.Log($"차트 저장 완료: {path}");
    }
    
    void LoadChart()
    {
        string path = Application.persistentDataPath + "/chart.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentChart = JsonUtility.FromJson<ChartData>(json);
            SetupLanes();
            Debug.Log("차트 로드 완료");
        }
        else
        {
            Debug.LogWarning("저장된 차트 파일이 없습니다");
        }
    }
    
    public void LoadChart(ChartData chartData)
    {
        if (chartData != null && chartData.ValidateChart())
        {
            currentChart = chartData;
            currentLaneCount = chartData.laneCount;
            beatDivision = chartData.beatDivision;
            SetupLanes();
            Debug.Log("차트 데이터 로드 완료");
        }
        else
        {
            Debug.LogError("유효하지 않은 차트 데이터입니다");
        }
    }
    
    // 선택 관련 메서드들
    public bool IsPreviewMode()
    {
        return isPreviewMode;
    }
    
    public void AddToSelection(NoteData note)
    {
        if (!selectedNotes.Contains(note))
        {
            selectedNotes.Add(note);
        }
    }
    
    public void RemoveFromSelection(NoteData note)
    {
        selectedNotes.Remove(note);
    }
    
    public void ClearSelection()
    {
        selectedNotes.Clear();
    }
    
    public ChartData GetCurrentChart()
    {
        return currentChart;
    }
    
    // 키보드 단축키 도움말 표시
    void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1)
        {
            showHelp = !showHelp;
        }
        
        if (showHelp)
        {
            ShowHelpWindow();
        }
    }
    
    private bool showHelp = false;
    
    void ShowHelpWindow()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 600), GUI.skin.box);
        GUILayout.Label("Chart Editor 도움말", EditorGUIStyle.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("모드 전환:");
        GUILayout.Label("N - 일반 노트 모드");
        GUILayout.Label("L - 롱 노트 모드"); 
        GUILayout.Label("P - 미리보기 모드");
        GUILayout.Space(5);
        
        GUILayout.Label("레인 조절:");
        GUILayout.Label("+ - 레인 수 증가");
        GUILayout.Label("- - 레인 수 감소");
        GUILayout.Space(5);
        
        GUILayout.Label("마디 분할:");
        GUILayout.Label("1 - 1/4 분할");
        GUILayout.Label("2 - 1/8 분할");
        GUILayout.Label("3 - 1/16 분할");
        GUILayout.Space(5);
        
        GUILayout.Label("오디오 제어:");
        GUILayout.Label("Space - 재생/일시정지");
        GUILayout.Label("Shift + ↑/↓ - 재생 속도 조절");
        GUILayout.Label("Ctrl + ↑/↓ - 스크롤 속도 조절");
        GUILayout.Label("Alt + ←/→ - 오디오 오프셋 조절");
        GUILayout.Space(5);
        
        GUILayout.Label("편집:");
        GUILayout.Label("Ctrl + S - 저장");
        GUILayout.Label("Ctrl + O - 로드");
        GUILayout.Label("Ctrl + Z - 실행 취소");
        GUILayout.Label("Ctrl + Y - 다시 실행");
        GUILayout.Label("Ctrl + A - 전체 선택");
        GUILayout.Label("Delete - 선택된 노트 삭제");
        GUILayout.Space(5);
        
        GUILayout.Label("F1 - 도움말 토글");
        
        GUILayout.EndArea();
    }
    
    // 에디터 GUI 스타일 (Unity Editor에서만 사용)
    private static class EditorGUIStyle
    {
        public static GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
    }
}