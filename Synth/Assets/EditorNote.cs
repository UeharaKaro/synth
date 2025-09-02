using UnityEngine;

public class EditorNote : MonoBehaviour
{
    [Header("Visual Settings")]
    public SpriteRenderer noteRenderer;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color longNoteColor = Color.cyan;
    
    [Header("Note Data")]
    public NoteData noteData;
    public bool isSelected = false;
    
    private ChartEditor chartEditor;
    
    void Start()
    {
        chartEditor = FindObjectOfType<ChartEditor>();
        UpdateVisuals();
    }
    
    void OnMouseDown()
    {
        if (chartEditor != null && !chartEditor.IsPreviewMode())
        {
            ToggleSelection();
        }
    }
    
    void ToggleSelection()
    {
        isSelected = !isSelected;
        if (chartEditor != null)
        {
            if (isSelected)
                chartEditor.AddToSelection(noteData);
            else
                chartEditor.RemoveFromSelection(noteData);
        }
        UpdateVisuals();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        if (noteRenderer == null) return;
        
        if (isSelected)
        {
            noteRenderer.color = selectedColor;
        }
        else if (noteData != null && noteData.isLongNote)
        {
            noteRenderer.color = longNoteColor;
        }
        else
        {
            noteRenderer.color = normalColor;
        }
    }
    
    public void SetNoteData(NoteData data)
    {
        noteData = data;
        UpdateVisuals();
    }
}