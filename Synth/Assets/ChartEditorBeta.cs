using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

[System.Serializable]
public class ChartDataBeta
{
    public string audioFileName;
    public float bpm = 120f; // Default BPM
    public int laneCount = 4; // Default lane count
    public List<NoteDataBeta> notes = new List<NoteDataBeta>();
    public float scrollSpeed = 8f; // Default scroll speed
    public int beatDivision = 4; // Default beat division (1/4)
    public float audioOffset = 0f; // Audio offset in milliseconds
}

[System.Serializable]
public class NoteDataBeta
{
    public double timing; // Note timing in seconds
    public float beatTiming; // BPM-based beat timing
    public int track; // Track/lane number
    public KeySoundType keySoundType = KeySoundType.None;
    public bool isLongNote = false;
    public double longNoteEndTiming = 0.0; // End timing for long notes

    public NoteDataBeta(double timing, int track, KeySoundType keySoundType = KeySoundType.None, bool isLongNote = false, double endTiming = 0)
    {
        this.timing = timing;
        this.track = track;
        this.keySoundType = keySoundType;
        this.isLongNote = isLongNote;
        this.longNoteEndTiming = endTiming;
    }

    public void CalculateBeatTiming(float bpm)
    {
        beatTiming = (float)(timing * bpm / 60.0f);
    }
}

public enum EditMode
{
    Normal,
    LongNote
}

public enum GridSnapMode
{
    None,
    Beat_1_4,
    Beat_1_8,
    Beat_1_16,
    Beat_1_32
}

[RequireComponent(typeof(AudioSource))]
public class ChartEditorBeta : MonoBehaviour
{
    [Header("UI Elements")]
    public InputField audioPathInputField;
    public Slider timelineSlider;
    public Text currentTimeText;
    public Text totalTimeText;
    public Text bpmText;
    public Text laneCountText;
    public Text beatDivisionText;
    public Text editModeText;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Button saveButton;
    public Button loadButton;

    [Header("Editor Settings")]
    [Range(1f, 20f)]
    public float scrollSpeed = 8f;
    [Range(60f, 300f)]
    public float bpm = 120f;
    [Range(4, 10)]
    public int laneCount = 4;
    public int[] supportedLaneCounts = { 4, 5, 6, 7, 8, 10 };

    [Header("Visual Settings")]
    public GameObject notePrefab;
    public GameObject longNotePrefab;
    public Transform noteContainer;
    public Transform[] tracks;
    public float trackSpacing = 1f;
    public float judgeLineY = -5f;

    [Header("Grid Settings")]
    public GridSnapMode gridSnapMode = GridSnapMode.Beat_1_4;
    public bool showGrid = true;
    public Material gridLineMaterial;

    // Internal variables
    private AudioSource audioSource;
    private ChartDataBeta currentChart;
    private string audioFilePath;
    private EditMode currentEditMode = EditMode.Normal;
    private bool isPlaying = false;
    private bool isPreviewMode = false;
    private List<GameObject> spawnedNotes = new List<GameObject>();
    private List<NoteBeta> selectedNotes = new List<NoteBeta>();
    private List<GameObject> gridLines = new List<GameObject>();
    private Camera editorCamera;
    private AudioManagerBeta audioManagerBeta;
    
    // Input handling
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private double noteStartTime;
    
    // Undo/Redo system
    private List<ChartDataBeta> undoStack = new List<ChartDataBeta>();
    private List<ChartDataBeta> redoStack = new List<ChartDataBeta>();
    private const int maxUndoSteps = 50;
    
    // Public accessors for note management
    public float judgeLineY = -5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentChart = new ChartDataBeta();
        editorCamera = Camera.main;
        
        // Initialize audio manager beta
        audioManagerBeta = AudioManagerBeta.Instance;
        if (audioManagerBeta == null)
        {
            GameObject audioGO = new GameObject("AudioManagerBeta");
            audioManagerBeta = audioGO.AddComponent<AudioManagerBeta>();
        }
        
        InitializeUI();
        SetupTracks();
        UpdateGridLines();
        
        // Set default values
        UpdateBPM(bpm);
        UpdateLaneCount(laneCount);
        UpdateBeatDivision(4);
    }

    void Update()
    {
        HandleInput();
        UpdateUI();
        
        if (isPlaying && audioSource.isPlaying)
        {
            UpdateTimeline();
        }
        
        if (isPreviewMode)
        {
            UpdatePreviewMode();
        }
    }

    #region UI Management
    void InitializeUI()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayAudio);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseAudio);
        if (stopButton != null)
            stopButton.onClick.AddListener(StopAudio);
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveChart);
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadChart);
        
        if (timelineSlider != null)
            timelineSlider.onValueChanged.AddListener(SeekAudio);
    }

    void UpdateUI()
    {
        float totalTime = GetTotalAudioTime();
        
        if (currentTimeText != null && totalTime > 0)
        {
            currentTimeText.text = FormatTime(GetCurrentAudioTime());
        }
        
        if (totalTimeText != null && totalTime > 0)
        {
            totalTimeText.text = FormatTime(totalTime);
        }
        
        if (bpmText != null)
            bpmText.text = $"BPM: {bpm:F1}";
        
        if (laneCountText != null)
            laneCountText.text = $"Lanes: {laneCount}";
        
        if (beatDivisionText != null)
            beatDivisionText.text = $"Division: 1/{currentChart.beatDivision}";
        
        if (editModeText != null)
            editModeText.text = $"Mode: {currentEditMode}";
    }

    void UpdateTimeline()
    {
        if (timelineSlider != null)
        {
            float totalTime = GetTotalAudioTime();
            if (totalTime > 0)
            {
                timelineSlider.value = GetCurrentAudioTime() / totalTime;
            }
        }
    }

    string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int millisecs = Mathf.FloorToInt((seconds % 1f) * 1000f);
        return $"{minutes:00}:{secs:00}.{millisecs:000}";
    }
    #endregion

    #region Input Handling
    void HandleInput()
    {
        // Mode switching
        if (Input.GetKeyDown(KeyCode.N))
        {
            SetEditMode(EditMode.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            SetEditMode(EditMode.LongNote);
        }
        
        // Lane count adjustment
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            DecreaseLaneCount();
        }
        else if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
        {
            IncreaseLaneCount();
        }
        
        // Preview mode
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePreviewMode();
        }
        
        // Audio controls
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isPlaying)
                PauseAudio();
            else
                PlayAudio();
        }
        
        // Undo/Redo
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                Redo();
            else
                Undo();
        }
        
        // Delete selected notes
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            DeleteSelectedNotes();
        }
        
        // Select all notes
        if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftControl))
        {
            SelectAllNotes();
        }
        
        // Clear selection
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }
        
        // Grid snap mode cycling
        if (Input.GetKeyDown(KeyCode.G))
        {
            CycleGridSnapMode();
        }
        
        // Beat division adjustment
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            DecreaseBeatDivision();
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            IncreaseBeatDivision();
        }
        
        // Note placement
        HandleNotePlacement();
    }

    void SelectAllNotes()
    {
        ClearSelection();
        foreach (var noteObj in spawnedNotes)
        {
            var noteBeta = noteObj.GetComponent<NoteBeta>();
            if (noteBeta != null)
            {
                AddToSelection(noteBeta);
            }
        }
    }

    void CycleGridSnapMode()
    {
        gridSnapMode = (GridSnapMode)(((int)gridSnapMode + 1) % System.Enum.GetValues(typeof(GridSnapMode)).Length);
        UpdateGridLines();
        Debug.Log($"Grid snap mode: {gridSnapMode}");
    }

    void IncreaseBeatDivision()
    {
        int[] divisions = { 4, 8, 16, 32 };
        int currentIndex = System.Array.IndexOf(divisions, currentChart.beatDivision);
        if (currentIndex >= 0 && currentIndex < divisions.Length - 1)
        {
            UpdateBeatDivision(divisions[currentIndex + 1]);
        }
    }

    void DecreaseBeatDivision()
    {
        int[] divisions = { 4, 8, 16, 32 };
        int currentIndex = System.Array.IndexOf(divisions, currentChart.beatDivision);
        if (currentIndex > 0)
        {
            UpdateBeatDivision(divisions[currentIndex - 1]);
        }
    }

    void HandleNotePlacement()
    {
        if (isPreviewMode) return;
        
        Vector3 mousePos = Input.mousePosition;
        Ray ray = editorCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                isDragging = true;
                dragStartPos = hit.point;
                noteStartTime = GetTimeFromWorldPosition(hit.point);
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (Physics.Raycast(ray, out hit))
            {
                PlaceNote(dragStartPos, hit.point);
            }
            isDragging = false;
        }
    }

    void PlaceNote(Vector3 startPos, Vector3 endPos)
    {
        int track = GetTrackFromWorldPosition(startPos);
        if (track < 0 || track >= laneCount) return;
        
        double startTime = GetTimeFromWorldPosition(startPos);
        double endTime = GetTimeFromWorldPosition(endPos);
        
        // Apply grid snap
        startTime = SnapToGrid(startTime);
        
        if (currentEditMode == EditMode.LongNote)
        {
            endTime = SnapToGrid(endTime);
            if (endTime <= startTime) endTime = startTime + (60.0 / bpm / currentChart.beatDivision);
            
            var longNote = new NoteDataBeta(startTime, track, KeySoundType.None, true, endTime);
            AddNoteToChart(longNote);
        }
        else
        {
            var note = new NoteDataBeta(startTime, track, KeySoundType.None, false, 0);
            AddNoteToChart(note);
        }
        
        RefreshNoteDisplay();
    }
    #endregion

    #region Track Management
    void SetupTracks()
    {
        // Initialize track positions based on lane count
        UpdateTrackPositions();
    }

    void UpdateTrackPositions()
    {
        if (tracks == null || tracks.Length < laneCount) return;
        
        float totalWidth = (laneCount - 1) * trackSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < laneCount; i++)
        {
            if (tracks[i] != null)
            {
                Vector3 pos = tracks[i].position;
                pos.x = startX + i * trackSpacing;
                tracks[i].position = pos;
                tracks[i].gameObject.SetActive(true);
            }
        }
        
        // Hide unused tracks
        for (int i = laneCount; i < tracks.Length; i++)
        {
            if (tracks[i] != null)
                tracks[i].gameObject.SetActive(false);
        }
    }

    int GetTrackFromWorldPosition(Vector3 worldPos)
    {
        float totalWidth = (laneCount - 1) * trackSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < laneCount; i++)
        {
            float trackX = startX + i * trackSpacing;
            if (Mathf.Abs(worldPos.x - trackX) < trackSpacing / 2f)
            {
                return i;
            }
        }
        
        return -1; // Invalid track
    }

    double GetTimeFromWorldPosition(Vector3 worldPos)
    {
        float totalTime = GetTotalAudioTime();
        if (totalTime <= 0) return 0;
        
        // Convert Y position to time based on scroll speed
        float timePerUnit = 1f / scrollSpeed;
        double time = GetCurrentAudioTime() + (judgeLineY - worldPos.y) * timePerUnit;
        return Mathf.Max(0, (float)time);
    }

    double SnapToGrid(double time)
    {
        if (gridSnapMode == GridSnapMode.None) return time;
        
        float beatsPerSecond = bpm / 60f;
        float snapInterval = 1f / beatsPerSecond;
        
        switch (gridSnapMode)
        {
            case GridSnapMode.Beat_1_4:
                snapInterval /= 4f;
                break;
            case GridSnapMode.Beat_1_8:
                snapInterval /= 8f;
                break;
            case GridSnapMode.Beat_1_16:
                snapInterval /= 16f;
                break;
            case GridSnapMode.Beat_1_32:
                snapInterval /= 32f;
                break;
        }
        
        return Mathf.Round((float)(time / snapInterval)) * snapInterval;
    }
    #endregion

    #region Chart Management
    void AddNoteToChart(NoteDataBeta note)
    {
        SaveUndoState();
        currentChart.notes.Add(note);
        note.CalculateBeatTiming(bpm);
    }

    void RefreshNoteDisplay()
    {
        // Clear existing note display
        foreach (var note in spawnedNotes)
        {
            if (note != null) DestroyImmediate(note);
        }
        spawnedNotes.Clear();
        
        // Respawn notes based on current chart data
        foreach (var noteData in currentChart.notes)
        {
            SpawnNoteDisplay(noteData);
        }
    }

    void SpawnNoteDisplay(NoteDataBeta noteData)
    {
        GameObject prefab = noteData.isLongNote ? longNotePrefab : notePrefab;
        if (prefab == null) return;
        
        GameObject noteObj = Instantiate(prefab, noteContainer);
        
        // Add NoteBeta component if not present
        NoteBeta noteBeta = noteObj.GetComponent<NoteBeta>();
        if (noteBeta == null)
        {
            noteBeta = noteObj.AddComponent<NoteBeta>();
        }
        
        // Initialize the note for editor use
        noteBeta.InitializeForEditor(noteData, this);
        
        // Position the note based on track and timing
        Vector3 pos = GetWorldPositionFromNote(noteData);
        noteObj.transform.position = pos;
        
        spawnedNotes.Add(noteObj);
    }

    public void SelectNote(NoteBeta note)
    {
        // Clear previous selection
        ClearSelection();
        
        // Select this note
        selectedNotes.Add(note);
        note.SetSelected(true);
    }

    public void AddToSelection(NoteBeta note)
    {
        if (!selectedNotes.Contains(note))
        {
            selectedNotes.Add(note);
            note.SetSelected(true);
        }
    }

    public void RemoveFromSelection(NoteBeta note)
    {
        if (selectedNotes.Contains(note))
        {
            selectedNotes.Remove(note);
            note.SetSelected(false);
        }
    }

    public void ClearSelection()
    {
        foreach (var note in selectedNotes)
        {
            if (note != null)
                note.SetSelected(false);
        }
        selectedNotes.Clear();
    }

    public void DeleteNote(NoteBeta note)
    {
        SaveUndoState();
        
        // Remove from chart data
        var noteData = note.ToNoteData();
        for (int i = currentChart.notes.Count - 1; i >= 0; i--)
        {
            var chartNote = currentChart.notes[i];
            if (Mathf.Approximately((float)chartNote.timing, (float)noteData.timing) && 
                chartNote.track == noteData.track)
            {
                currentChart.notes.RemoveAt(i);
                break;
            }
        }
        
        // Remove from selection
        RemoveFromSelection(note);
        
        // Remove from display
        if (spawnedNotes.Contains(note.gameObject))
        {
            spawnedNotes.Remove(note.gameObject);
        }
    }

    public void DeleteSelectedNotes()
    {
        if (selectedNotes.Count == 0) return;
        
        SaveUndoState();
        
        var notesToDelete = new List<NoteBeta>(selectedNotes);
        foreach (var note in notesToDelete)
        {
            DeleteNote(note);
        }
        
        ClearSelection();
        RefreshNoteDisplay();
    }

    Vector3 GetWorldPositionFromNote(NoteDataBeta noteData)
    {
        float totalWidth = (laneCount - 1) * trackSpacing;
        float startX = -totalWidth / 2f;
        float x = startX + noteData.track * trackSpacing;
        
        // Calculate Y position based on timing and current playback position
        float currentTime = GetCurrentAudioTime();
        float timeOffset = (float)(noteData.timing - currentTime);
        float y = judgeLineY + timeOffset * scrollSpeed;
        
        return new Vector3(x, y, 0);
    }
    #endregion

    #region Audio Management
    void PlayAudio()
    {
        if (audioManagerBeta != null && audioManagerBeta.HasAudio)
        {
            audioManagerBeta.PlayAudio();
            isPlaying = true;
        }
        else if (audioSource.clip != null)
        {
            audioSource.Play();
            isPlaying = true;
        }
    }

    void PauseAudio()
    {
        if (audioManagerBeta != null)
        {
            audioManagerBeta.PauseAudio();
            isPlaying = false;
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
        }
    }

    void StopAudio()
    {
        if (audioManagerBeta != null)
        {
            audioManagerBeta.StopAudio();
            isPlaying = false;
        }
        else
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }

    void SeekAudio(float value)
    {
        float totalTime = GetTotalAudioTime();
        if (totalTime > 0)
        {
            float targetTime = value * totalTime;
            
            if (audioManagerBeta != null)
            {
                audioManagerBeta.SeekToTime(targetTime);
            }
            else if (audioSource.clip != null)
            {
                audioSource.time = targetTime;
            }
            
            RefreshNoteDisplay();
        }
    }

    float GetCurrentAudioTime()
    {
        if (audioManagerBeta != null)
        {
            return audioManagerBeta.GetCurrentTime();
        }
        else if (audioSource.clip != null)
        {
            return audioSource.time;
        }
        return 0f;
    }

    float GetTotalAudioTime()
    {
        if (audioManagerBeta != null)
        {
            return audioManagerBeta.GetTotalTime();
        }
        else if (audioSource.clip != null)
        {
            return audioSource.clip.length;
        }
        return 0f;
    }

    public void LoadAudioFile(string filePath)
    {
        if (audioManagerBeta != null)
        {
            audioManagerBeta.LoadAudioFile(filePath);
            audioFilePath = filePath;
        }
        else
        {
            // Fallback to basic Unity AudioSource loading
            StartCoroutine(LoadAudioClipCoroutine(filePath));
        }
    }

    System.Collections.IEnumerator LoadAudioClipCoroutine(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioFilePath = filePath;
                Debug.Log($"Audio loaded: {filePath}");
            }
            else
            {
                Debug.LogError($"Failed to load audio: {www.error}");
            }
        }
    }
    #endregion

    #region Settings Management
    void SetEditMode(EditMode mode)
    {
        currentEditMode = mode;
        Debug.Log($"Edit mode changed to: {mode}");
    }

    void IncreaseLaneCount()
    {
        int currentIndex = System.Array.IndexOf(supportedLaneCounts, laneCount);
        if (currentIndex >= 0 && currentIndex < supportedLaneCounts.Length - 1)
        {
            UpdateLaneCount(supportedLaneCounts[currentIndex + 1]);
        }
    }

    void DecreaseLaneCount()
    {
        int currentIndex = System.Array.IndexOf(supportedLaneCounts, laneCount);
        if (currentIndex > 0)
        {
            UpdateLaneCount(supportedLaneCounts[currentIndex - 1]);
        }
    }

    void UpdateLaneCount(int newLaneCount)
    {
        laneCount = newLaneCount;
        currentChart.laneCount = laneCount;
        UpdateTrackPositions();
        UpdateGridLines();
        RefreshNoteDisplay();
        Debug.Log($"Lane count changed to: {laneCount}");
    }

    void UpdateBPM(float newBPM)
    {
        bpm = newBPM;
        currentChart.bpm = bpm;
        UpdateGridLines();
        
        // Recalculate beat timing for all notes
        foreach (var note in currentChart.notes)
        {
            note.CalculateBeatTiming(bpm);
        }
    }

    void UpdateBeatDivision(int division)
    {
        currentChart.beatDivision = division;
        UpdateGridLines();
    }

    void TogglePreviewMode()
    {
        isPreviewMode = !isPreviewMode;
        Debug.Log($"Preview mode: {(isPreviewMode ? "ON" : "OFF")}");
        
        if (isPreviewMode)
        {
            PlayAudio();
        }
    }

    void UpdatePreviewMode()
    {
        if (!isPlaying) return;
        
        // Update note positions based on scroll speed and current time
        RefreshNoteDisplay();
    }
    #endregion

    #region Grid System
    void UpdateGridLines()
    {
        ClearGridLines();
        if (!showGrid) return;
        
        CreateGridLines();
    }

    void ClearGridLines()
    {
        foreach (var line in gridLines)
        {
            if (line != null) DestroyImmediate(line);
        }
        gridLines.Clear();
    }

    void CreateGridLines()
    {
        // Create horizontal grid lines based on beat division
        float beatsPerSecond = bpm / 60f;
        float beatInterval = 1f / beatsPerSecond / currentChart.beatDivision;
        
        // Create enough grid lines to cover the visible area
        float visibleTime = 10f; // seconds
        int lineCount = Mathf.CeilToInt(visibleTime / beatInterval);
        
        for (int i = 0; i < lineCount; i++)
        {
            float time = i * beatInterval;
            float y = judgeLineY + time * scrollSpeed;
            CreateHorizontalGridLine(y);
        }
        
        // Create vertical grid lines for tracks
        float totalWidth = (laneCount - 1) * trackSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i <= laneCount; i++)
        {
            float x = startX + i * trackSpacing - trackSpacing / 2f;
            if (i == 0) x = startX - trackSpacing / 2f;
            if (i == laneCount) x = startX + (laneCount - 1) * trackSpacing + trackSpacing / 2f;
            
            CreateVerticalGridLine(x);
        }
    }

    void CreateHorizontalGridLine(float y)
    {
        GameObject line = new GameObject("GridLine_H");
        line.transform.SetParent(noteContainer);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = gridLineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        
        float totalWidth = (laneCount - 1) * trackSpacing;
        lr.SetPosition(0, new Vector3(-totalWidth / 2f - trackSpacing / 2f, y, 0));
        lr.SetPosition(1, new Vector3(totalWidth / 2f + trackSpacing / 2f, y, 0));
        
        gridLines.Add(line);
    }

    void CreateVerticalGridLine(float x)
    {
        GameObject line = new GameObject("GridLine_V");
        line.transform.SetParent(noteContainer);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = gridLineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        
        lr.SetPosition(0, new Vector3(x, judgeLineY - 2f, 0));
        lr.SetPosition(1, new Vector3(x, judgeLineY + 10f, 0));
        
        gridLines.Add(line);
    }
    #endregion

    #region Undo/Redo System
    void SaveUndoState()
    {
        // Create a deep copy of the current chart
        var chartCopy = JsonUtility.FromJson<ChartDataBeta>(JsonUtility.ToJson(currentChart));
        undoStack.Add(chartCopy);
        
        // Limit undo stack size
        if (undoStack.Count > maxUndoSteps)
        {
            undoStack.RemoveAt(0);
        }
        
        // Clear redo stack when new action is performed
        redoStack.Clear();
    }

    void Undo()
    {
        if (undoStack.Count == 0) return;
        
        // Save current state to redo stack
        var currentCopy = JsonUtility.FromJson<ChartDataBeta>(JsonUtility.ToJson(currentChart));
        redoStack.Add(currentCopy);
        
        // Restore previous state
        currentChart = undoStack[undoStack.Count - 1];
        undoStack.RemoveAt(undoStack.Count - 1);
        
        RefreshNoteDisplay();
        Debug.Log("Undo performed");
    }

    void Redo()
    {
        if (redoStack.Count == 0) return;
        
        // Save current state to undo stack
        SaveUndoState();
        
        // Restore next state
        currentChart = redoStack[redoStack.Count - 1];
        redoStack.RemoveAt(redoStack.Count - 1);
        
        RefreshNoteDisplay();
        Debug.Log("Redo performed");
    }
    #endregion

    #region Save/Load System
    void SaveChart()
    {
        string chartJson = JsonUtility.ToJson(currentChart, true);
        string savePath = Application.persistentDataPath + "/chart_beta.json";
        
        try
        {
            File.WriteAllText(savePath, chartJson);
            Debug.Log($"Chart saved to: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save chart: {e.Message}");
        }
    }

    void LoadChart()
    {
        string loadPath = Application.persistentDataPath + "/chart_beta.json";
        
        if (!File.Exists(loadPath))
        {
            Debug.LogWarning("No chart file found to load");
            return;
        }
        
        try
        {
            string chartJson = File.ReadAllText(loadPath);
            currentChart = JsonUtility.FromJson<ChartDataBeta>(chartJson);
            
            // Apply loaded settings
            UpdateBPM(currentChart.bpm);
            UpdateLaneCount(currentChart.laneCount);
            UpdateBeatDivision(currentChart.beatDivision);
            
            RefreshNoteDisplay();
            Debug.Log($"Chart loaded from: {loadPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load chart: {e.Message}");
        }
    }
    #endregion
}