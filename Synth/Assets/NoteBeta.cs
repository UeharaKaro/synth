using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced Note class for ChartEditorBeta with improved functionality
/// </summary>
public class NoteBeta : MonoBehaviour
{
    [Header("Note Configuration")]
    public KeySoundType keySoundType = KeySoundType.None;
    public int track = 0;
    public double timing = 0.0;
    public bool isLongNote = false;
    public double longNoteEndTiming = 0.0;

    [Header("Visual Settings")]
    public SpriteRenderer noteRenderer;
    public Color evenTrackColor = Color.white;
    public Color oddTrackColor = Color.cyan;
    public Color selectedColor = Color.yellow;
    public Color previewColor = Color.green;

    [Header("Long Note Settings")]
    public LineRenderer longNoteTrail;
    public float trailWidth = 0.1f;
    public Material trailMaterial;

    [Header("Animation Settings")]
    public float pulseSpeed = 2f;
    public float pulseScale = 1.1f;
    public bool enablePulse = false;

    // Internal state
    private bool isSelected = false;
    private bool isInPreviewMode = false;
    private bool isHit = false;
    private bool isLongNoteHeld = false;
    private Vector3 originalScale;
    private Color originalColor;
    private double spawnTime;
    private bool initialized = false;

    // Editor-specific properties
    private bool isInEditMode = true;
    private ChartEditorBeta parentEditor;

    void Awake()
    {
        originalScale = transform.localScale;
        if (noteRenderer != null)
        {
            originalColor = noteRenderer.color;
        }
    }

    void Start()
    {
        SetupVisualAppearance();
        if (isLongNote)
        {
            SetupLongNoteVisual();
        }
    }

    void Update()
    {
        if (enablePulse && (isSelected || isInPreviewMode))
        {
            UpdatePulseAnimation();
        }

        if (isLongNote && longNoteTrail != null && isInEditMode)
        {
            UpdateLongNoteTrailInEditor();
        }
    }

    #region Initialization
    /// <summary>
    /// Initialize note with editor-specific settings
    /// </summary>
    public void InitializeForEditor(NoteDataBeta noteData, ChartEditorBeta editor)
    {
        parentEditor = editor;
        keySoundType = noteData.keySoundType;
        track = noteData.track;
        timing = noteData.timing;
        isLongNote = noteData.isLongNote;
        longNoteEndTiming = noteData.longNoteEndTiming;
        spawnTime = Time.time;
        initialized = true;
        isInEditMode = true;

        SetupVisualAppearance();
        
        if (isLongNote)
        {
            SetupLongNoteVisual();
        }
    }

    /// <summary>
    /// Initialize note for gameplay (preview mode)
    /// </summary>
    public void InitializeForPreview(NoteDataBeta noteData, double currentTime, float scrollSpeed, float targetY)
    {
        keySoundType = noteData.keySoundType;
        track = noteData.track;
        timing = noteData.timing;
        isLongNote = noteData.isLongNote;
        longNoteEndTiming = noteData.longNoteEndTiming;
        spawnTime = currentTime;
        initialized = true;
        isInEditMode = false;
        isInPreviewMode = true;

        SetupVisualAppearance();
        SetNoteColor(previewColor);
        
        if (isLongNote)
        {
            SetupLongNoteVisual();
        }
    }
    #endregion

    #region Visual Setup
    void SetupVisualAppearance()
    {
        if (noteRenderer == null) return;

        // Set color based on track (even/odd)
        Color baseColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
        
        if (isInPreviewMode)
        {
            baseColor = previewColor;
        }
        else if (isSelected)
        {
            baseColor = selectedColor;
        }

        noteRenderer.color = baseColor;
        originalColor = baseColor;

        // Adjust appearance for long notes
        if (isLongNote)
        {
            Color longColor = baseColor;
            longColor.a = 0.8f; // Slightly transparent
            noteRenderer.color = longColor;
        }
    }

    void SetupLongNoteVisual()
    {
        if (longNoteTrail == null)
        {
            longNoteTrail = gameObject.AddComponent<LineRenderer>();
        }

        if (trailMaterial != null)
        {
            longNoteTrail.material = trailMaterial;
        }
        else
        {
            longNoteTrail.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Set trail color to match note color
        Color trailColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
        if (isInPreviewMode) trailColor = previewColor;
        
        longNoteTrail.startColor = trailColor;
        longNoteTrail.endColor = trailColor;
        longNoteTrail.startWidth = trailWidth;
        longNoteTrail.endWidth = trailWidth;
        longNoteTrail.positionCount = 2;
        longNoteTrail.useWorldSpace = false;
    }

    void UpdateLongNoteTrailInEditor()
    {
        if (!isLongNote || longNoteTrail == null || parentEditor == null) return;

        // Calculate end position based on timing difference
        double duration = longNoteEndTiming - timing;
        float yOffset = (float)duration * parentEditor.scrollSpeed;

        Vector3 startPos = Vector3.zero;
        Vector3 endPos = new Vector3(0, yOffset, 0);

        longNoteTrail.SetPosition(0, startPos);
        longNoteTrail.SetPosition(1, endPos);
    }
    #endregion

    #region Visual Effects
    void UpdatePulseAnimation()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f);
        transform.localScale = originalScale * pulse;
    }

    public void SetNoteColor(Color color)
    {
        if (noteRenderer != null)
        {
            noteRenderer.color = color;
            originalColor = color;
        }

        // Update long note trail color if applicable
        if (isLongNote && longNoteTrail != null)
        {
            longNoteTrail.startColor = color;
            longNoteTrail.endColor = color;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        enablePulse = selected;
        
        if (selected)
        {
            SetNoteColor(selectedColor);
        }
        else
        {
            SetupVisualAppearance();
        }
    }

    public void HighlightNote(bool highlight)
    {
        if (highlight)
        {
            SetNoteColor(Color.Lerp(originalColor, Color.white, 0.5f));
        }
        else
        {
            SetNoteColor(originalColor);
        }
    }
    #endregion

    #region Editor Functionality
    public void OnMouseDown()
    {
        if (!isInEditMode || parentEditor == null) return;
        
        // Handle note selection in editor
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Multi-select mode
            ToggleSelection();
        }
        else
        {
            // Single select mode
            SelectNote();
        }
    }

    public void OnMouseEnter()
    {
        if (!isInEditMode) return;
        
        HighlightNote(true);
    }

    public void OnMouseExit()
    {
        if (!isInEditMode) return;
        
        HighlightNote(false);
    }

    void SelectNote()
    {
        if (parentEditor != null)
        {
            // Notify editor of selection
            parentEditor.SelectNote(this);
        }
        SetSelected(true);
    }

    void ToggleSelection()
    {
        SetSelected(!isSelected);
        if (parentEditor != null)
        {
            if (isSelected)
                parentEditor.AddToSelection(this);
            else
                parentEditor.RemoveFromSelection(this);
        }
    }

    public void DeleteNote()
    {
        if (parentEditor != null)
        {
            parentEditor.DeleteNote(this);
        }
        Destroy(gameObject);
    }
    #endregion

    #region Gameplay Functionality (for preview mode)
    public JudgmentType OnNoteHit(double hitTime)
    {
        if (isHit) return JudgmentType.Miss;

        isHit = true;
        double timeDifference = System.Math.Abs(hitTime - timing) * 1000.0;
        JudgmentType judgment = CalculateJudgment(timeDifference);

        // Visual feedback for hit
        StartCoroutine(HitEffect());

        // Handle long note start
        if (isLongNote)
        {
            isLongNoteHeld = true;
            if (longNoteTrail != null)
            {
                longNoteTrail.positionCount = 2;
                UpdateLongNoteTrailActive();
            }
        }
        else
        {
            // Regular note - hide after hit
            gameObject.SetActive(false);
        }

        return judgment;
    }

    public JudgmentType OnLongNoteRelease(double releaseTime)
    {
        if (!isLongNote || !isLongNoteHeld) return JudgmentType.Miss;

        isLongNoteHeld = false;
        double timeDifference = System.Math.Abs(releaseTime - longNoteEndTiming) * 1000.0;
        JudgmentType judgment = CalculateJudgment(timeDifference);

        // Hide long note trail
        if (longNoteTrail != null)
        {
            longNoteTrail.positionCount = 0;
        }

        gameObject.SetActive(false);
        return judgment;
    }

    void UpdateLongNoteTrailActive()
    {
        if (!isLongNote || longNoteTrail == null) return;

        if (isLongNoteHeld)
        {
            // Show trail from note to judgment line
            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(startPos.x, parentEditor?.judgeLineY ?? -5f, startPos.z);
            
            longNoteTrail.SetPosition(0, Vector3.zero);
            longNoteTrail.SetPosition(1, transform.InverseTransformPoint(endPos));
        }
    }

    JudgmentType CalculateJudgment(double timeDifferenceMs)
    {
        // Use the same judgment logic as the original Note class
        // This is a simplified version - in practice, you'd want to use
        // the judgment settings from GameSettingsManager
        
        if (timeDifferenceMs <= 16.67f) return JudgmentType.S_Perfect;
        else if (timeDifferenceMs <= 41.66f) return JudgmentType.Perfect;
        else if (timeDifferenceMs <= 83.33f) return JudgmentType.Great;
        else if (timeDifferenceMs <= 120f) return JudgmentType.Good;
        else if (timeDifferenceMs <= 150f) return JudgmentType.Bad;
        else return JudgmentType.Miss;
    }

    IEnumerator HitEffect()
    {
        // Simple hit effect - flash white briefly
        Color originalColor = noteRenderer.color;
        noteRenderer.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        noteRenderer.color = originalColor;
    }
    #endregion

    #region Public Interface
    public bool IsSelected => isSelected;
    public bool IsLongNote => isLongNote;
    public bool IsHit => isHit;
    public double Timing => timing;
    public double LongNoteEndTiming => longNoteEndTiming;
    public int Track => track;
    public KeySoundType KeySound => keySoundType;

    public void SetTrackColors(Color evenColor, Color oddColor)
    {
        evenTrackColor = evenColor;
        oddTrackColor = oddColor;
        SetupVisualAppearance();
    }

    public NoteDataBeta ToNoteData()
    {
        return new NoteDataBeta(timing, track, keySoundType, isLongNote, longNoteEndTiming);
    }

    public void UpdateFromNoteData(NoteDataBeta noteData)
    {
        timing = noteData.timing;
        track = noteData.track;
        keySoundType = noteData.keySoundType;
        isLongNote = noteData.isLongNote;
        longNoteEndTiming = noteData.longNoteEndTiming;
        
        SetupVisualAppearance();
        if (isLongNote)
        {
            SetupLongNoteVisual();
        }
    }
    #endregion
}