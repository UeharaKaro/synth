using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Handles preview mode functionality for ChartEditorBeta
/// Manages note spawning, movement, and judgement simulation
/// </summary>
public class ChartEditorBetaPreview : MonoBehaviour
{
    [Header("Preview Settings")]
    public float noteScrollSpeed = 8f;
    public float judgeLineY = -5f;
    public float spawnDistanceY = 10f;
    public bool showJudgmentFeedback = true;
    
    [Header("Visual Settings")]
    public Color previewNoteColor = Color.green;
    public Color hitNoteColor = Color.white;
    public float noteHitEffectDuration = 0.2f;
    
    // Internal state
    private ChartEditorBeta parentEditor;
    private bool isActive = false;
    private List<GameObject> previewNotes = new List<GameObject>();
    private List<NoteDataBeta> chartNotes = new List<NoteDataBeta>();
    private int nextNoteIndex = 0;
    private float currentTime = 0f;
    private AudioManagerBeta audioManager;
    
    // Note pools for performance
    private Queue<GameObject> normalNotePool = new Queue<GameObject>();
    private Queue<GameObject> longNotePool = new Queue<GameObject>();
    
    // Events
    public event System.Action<JudgmentType> OnNoteJudged;
    public event System.Action<NoteDataBeta> OnNoteSpawned;
    public event System.Action OnPreviewStarted;
    public event System.Action OnPreviewStopped;
    
    void Awake()
    {
        audioManager = AudioManagerBeta.Instance;
    }
    
    void Update()
    {
        if (!isActive) return;
        
        currentTime = audioManager != null ? audioManager.GetCurrentTime() : Time.time;
        
        UpdateNoteSpawning();
        UpdateNotePositions();
        HandlePreviewInput();
    }
    
    #region Public Interface
    public void Initialize(ChartEditorBeta editor)
    {
        parentEditor = editor;
        InitializeNotePools();
    }
    
    public void StartPreview(List<NoteDataBeta> notes)
    {
        if (isActive) return;
        
        chartNotes = new List<NoteDataBeta>(notes);
        chartNotes.Sort((a, b) => a.timing.CompareTo(b.timing));
        
        isActive = true;
        nextNoteIndex = 0;
        currentTime = 0f;
        
        ClearPreviewNotes();
        OnPreviewStarted?.Invoke();
        
        Debug.Log($"Preview started with {chartNotes.Count} notes");
    }
    
    public void StopPreview()
    {
        if (!isActive) return;
        
        isActive = false;
        ClearPreviewNotes();
        ReturnAllNotesToPool();
        OnPreviewStopped?.Invoke();
        
        Debug.Log("Preview stopped");
    }
    
    public void SetScrollSpeed(float speed)
    {
        noteScrollSpeed = speed;
    }
    
    public void SetJudgeLinePosition(float y)
    {
        judgeLineY = y;
    }
    #endregion
    
    #region Note Pool Management
    void InitializeNotePools()
    {
        // Pre-create some notes for the pool
        for (int i = 0; i < 20; i++)
        {
            if (parentEditor.notePrefab != null)
            {
                GameObject note = Instantiate(parentEditor.notePrefab);
                note.SetActive(false);
                normalNotePool.Enqueue(note);
            }
            
            if (parentEditor.longNotePrefab != null)
            {
                GameObject longNote = Instantiate(parentEditor.longNotePrefab);
                longNote.SetActive(false);
                longNotePool.Enqueue(longNote);
            }
        }
    }
    
    GameObject GetNoteFromPool(bool isLongNote)
    {
        Queue<GameObject> targetPool = isLongNote ? longNotePool : normalNotePool;
        GameObject prefab = isLongNote ? parentEditor.longNotePrefab : parentEditor.notePrefab;
        
        if (targetPool.Count > 0)
        {
            return targetPool.Dequeue();
        }
        else if (prefab != null)
        {
            return Instantiate(prefab);
        }
        
        return null;
    }
    
    void ReturnNoteToPool(GameObject note, bool isLongNote)
    {
        if (note == null) return;
        
        note.SetActive(false);
        note.transform.SetParent(transform);
        
        Queue<GameObject> targetPool = isLongNote ? longNotePool : normalNotePool;
        targetPool.Enqueue(note);
    }
    
    void ReturnAllNotesToPool()
    {
        foreach (var note in previewNotes)
        {
            if (note != null)
            {
                NoteBeta noteBeta = note.GetComponent<NoteBeta>();
                bool isLongNote = noteBeta != null ? noteBeta.IsLongNote : false;
                ReturnNoteToPool(note, isLongNote);
            }
        }
        previewNotes.Clear();
    }
    #endregion
    
    #region Note Spawning and Movement
    void UpdateNoteSpawning()
    {
        while (nextNoteIndex < chartNotes.Count)
        {
            NoteDataBeta noteData = chartNotes[nextNoteIndex];
            
            // Calculate when to spawn the note based on scroll speed
            float spawnTime = (float)noteData.timing - (spawnDistanceY / noteScrollSpeed);
            
            if (currentTime >= spawnTime)
            {
                SpawnPreviewNote(noteData);
                nextNoteIndex++;
            }
            else
            {
                break; // Notes are sorted by timing, so we can break here
            }
        }
    }
    
    void SpawnPreviewNote(NoteDataBeta noteData)
    {
        GameObject noteObj = GetNoteFromPool(noteData.isLongNote);
        if (noteObj == null) return;
        
        // Set up the note
        NoteBeta noteBeta = noteObj.GetComponent<NoteBeta>();
        if (noteBeta == null)
        {
            noteBeta = noteObj.AddComponent<NoteBeta>();
        }
        
        // Initialize for preview
        noteBeta.InitializeForPreview(noteData, currentTime, noteScrollSpeed, judgeLineY);
        noteBeta.SetNoteColor(previewNoteColor);
        
        // Position the note
        Vector3 spawnPos = CalculateNoteSpawnPosition(noteData);
        noteObj.transform.position = spawnPos;
        noteObj.SetActive(true);
        
        if (parentEditor.noteContainer != null)
        {
            noteObj.transform.SetParent(parentEditor.noteContainer);
        }
        
        previewNotes.Add(noteObj);
        OnNoteSpawned?.Invoke(noteData);
    }
    
    Vector3 CalculateNoteSpawnPosition(NoteDataBeta noteData)
    {
        // Calculate X position based on track
        float totalWidth = (parentEditor.laneCount - 1) * parentEditor.trackSpacing;
        float startX = -totalWidth / 2f;
        float x = startX + noteData.track * parentEditor.trackSpacing;
        
        // Y position is above the visible area
        float y = judgeLineY + spawnDistanceY;
        
        return new Vector3(x, y, 0);
    }
    
    void UpdateNotePositions()
    {
        for (int i = previewNotes.Count - 1; i >= 0; i--)
        {
            GameObject noteObj = previewNotes[i];
            if (noteObj == null || !noteObj.activeInHierarchy)
            {
                previewNotes.RemoveAt(i);
                continue;
            }
            
            NoteBeta noteBeta = noteObj.GetComponent<NoteBeta>();
            if (noteBeta == null) continue;
            
            // Move note towards judge line
            float targetY = judgeLineY + ((float)noteBeta.Timing - currentTime) * noteScrollSpeed;
            Vector3 currentPos = noteObj.transform.position;
            noteObj.transform.position = new Vector3(currentPos.x, targetY, currentPos.z);
            
            // Check if note should be removed (passed judge line too far)
            if (targetY < judgeLineY - 2f && !noteBeta.IsHit)
            {
                // Simulate miss
                HandleNoteMiss(noteBeta);
                ReturnNoteToPool(noteObj, noteBeta.IsLongNote);
                previewNotes.RemoveAt(i);
            }
        }
    }
    #endregion
    
    #region Input Handling (for preview interaction)
    void HandlePreviewInput()
    {
        // Check for key presses that correspond to tracks
        for (int track = 0; track < parentEditor.laneCount; track++)
        {
            KeyCode key = GetKeyForTrack(track);
            
            if (Input.GetKeyDown(key))
            {
                HandleTrackInput(track, currentTime, true);
            }
            else if (Input.GetKeyUp(key))
            {
                HandleTrackInput(track, currentTime, false);
            }
        }
    }
    
    KeyCode GetKeyForTrack(int track)
    {
        // Default key mapping for different lane counts
        KeyCode[] keys4 = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
        KeyCode[] keys5 = { KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K };
        KeyCode[] keys6 = { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
        KeyCode[] keys7 = { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K, KeyCode.L };
        KeyCode[] keys8 = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
        KeyCode[] keys10 = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
        
        KeyCode[] currentKeys;
        switch (parentEditor.laneCount)
        {
            case 4: currentKeys = keys4; break;
            case 5: currentKeys = keys5; break;
            case 6: currentKeys = keys6; break;
            case 7: currentKeys = keys7; break;
            case 8: currentKeys = keys8; break;
            case 10: currentKeys = keys10; break;
            default: currentKeys = keys4; break;
        }
        
        return track < currentKeys.Length ? currentKeys[track] : KeyCode.None;
    }
    
    void HandleTrackInput(int track, double inputTime, bool isPress)
    {
        // Find the closest note in this track
        NoteBeta closestNote = FindClosestNoteInTrack(track);
        
        if (closestNote != null)
        {
            if (isPress && !closestNote.IsHit)
            {
                // Handle note hit
                JudgmentType judgment = closestNote.OnNoteHit(inputTime);
                HandleNoteHit(closestNote, judgment);
            }
            else if (!isPress && closestNote.IsLongNote && closestNote.IsHit)
            {
                // Handle long note release
                JudgmentType judgment = closestNote.OnLongNoteRelease(inputTime);
                HandleNoteHit(closestNote, judgment);
            }
        }
    }
    
    NoteBeta FindClosestNoteInTrack(int track)
    {
        NoteBeta closestNote = null;
        float closestDistance = float.MaxValue;
        
        foreach (var noteObj in previewNotes)
        {
            if (noteObj == null || !noteObj.activeInHierarchy) continue;
            
            NoteBeta noteBeta = noteObj.GetComponent<NoteBeta>();
            if (noteBeta == null || noteBeta.Track != track) continue;
            
            float distance = Mathf.Abs(noteObj.transform.position.y - judgeLineY);
            if (distance < closestDistance && distance < 1f) // Within reasonable range
            {
                closestDistance = distance;
                closestNote = noteBeta;
            }
        }
        
        return closestNote;
    }
    #endregion
    
    #region Judgment Handling
    void HandleNoteHit(NoteBeta note, JudgmentType judgment)
    {
        OnNoteJudged?.Invoke(judgment);
        
        if (showJudgmentFeedback)
        {
            StartCoroutine(ShowHitEffect(note.gameObject));
        }
        
        // Play key sound
        if (audioManager != null)
        {
            audioManager.PlayKeySound(note.KeySound);
        }
        
        Debug.Log($"Note hit: {judgment} (Track: {note.Track}, Timing: {note.Timing:F3})");
    }
    
    void HandleNoteMiss(NoteBeta note)
    {
        OnNoteJudged?.Invoke(JudgmentType.Miss);
        Debug.Log($"Note missed: Track {note.Track}, Timing: {note.Timing:F3}");
    }
    
    IEnumerator ShowHitEffect(GameObject noteObj)
    {
        SpriteRenderer sr = noteObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = hitNoteColor;
            
            // Scale effect
            Vector3 originalScale = noteObj.transform.localScale;
            noteObj.transform.localScale = originalScale * 1.2f;
            
            yield return new WaitForSeconds(noteHitEffectDuration);
            
            if (sr != null)
            {
                sr.color = originalColor;
                noteObj.transform.localScale = originalScale;
            }
        }
    }
    #endregion
    
    #region Utility
    void ClearPreviewNotes()
    {
        foreach (var note in previewNotes)
        {
            if (note != null)
            {
                Destroy(note);
            }
        }
        previewNotes.Clear();
    }
    
    public bool IsActive => isActive;
    public int ActiveNoteCount => previewNotes.Count;
    public int RemainingNoteCount => chartNotes.Count - nextNoteIndex;
    #endregion
}