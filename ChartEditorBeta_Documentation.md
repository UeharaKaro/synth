# ChartEditor - Unity Rhythm Game Chart Editor

## Overview

ChartEditor is a unified chart editing tool for Unity rhythm games, built based on the requirements in DEVELOPMENT_TODO.md. It provides complete chart creation and editing capabilities with an improved user experience and advanced features.

**Location**: `Assets/edit/ChartEditor.cs`
**Namespace**: `ChartSystem`
**Status**: Phase 1 Complete

## Key Features

### Phase 1 Features (Complete)
- ✅ **Multi-lane support**: 4K, 5K, 6K, 7K, 8K, 10K configurations
- ✅ **Note type switching**: Normal (N key) / Long (L key) modes
- ✅ **Bidirectional long notes**: Place from top-to-bottom or bottom-to-top
- ✅ **Grid snap system**: G key cycles through 1/4, 1/8, 1/16, 1/32, OFF
- ✅ **Undo/Redo**: Up to 50 steps (Ctrl+Z, Ctrl+Shift+Z)
- ✅ **Keyboard shortcuts**: Ctrl+S (save), Space (play/pause), T (edit scope toggle)
- ✅ **Audio control**: Load, play, pause, stop, seek
- ✅ **Chart management**: Load, save, create new

### Phase 2 Features (Planned)
- ⏳ Visual timeline with waveform
- ⏳ Advanced editing (copy, paste, mirror)
- ⏳ BPM change support
- ⏳ Slide notes
- ⏳ Multi-select and batch operations

## Editor Controls

### Mode Switching
| Key | Function | Description |
|-----|----------|-------------|
| **N** | Normal Note Mode | Place standard notes |
| **L** | Long Note Mode | Place long notes (hold notes) |
| **S** | Slide Note Mode | ⏳ Coming in Phase 2 |

### Note Placement

#### Normal Notes
1. Press **N** key to enter Normal note mode
2. Click on a track to place a note
3. Notes snap to grid if grid snap is enabled

#### Long Notes (Bidirectional)
1. Press **L** key to enter Long note mode
2. Click on start position (either top or bottom)
3. Click on end position (can be above or below start)
4. System automatically determines start/end timing

**Example:**
```
Top-to-Bottom:     Bottom-to-Top:
   [Click 1] ●        [Click 2] ●
      |                   |
      ↓                   ↑
   [Click 2] ●        [Click 1] ●
```

### Grid Snapping

**G Key**: Cycle through grid snap modes
- **1/4 beat** → **1/8 beat** → **1/16 beat** → **1/32 beat** → **OFF** → (repeat)

**Beat Division:**
- `[` key: Decrease beat division
- `]` key: Increase beat division

Grid snap calculates note timing based on:
```csharp
double beatInterval = 60.0 / bpm;  // Seconds per beat
double snapInterval = beatInterval / (int)currentBeatDivision;
```

### Audio Controls

| Key/Button | Function |
|------------|----------|
| **Space** | Play/Pause toggle |
| **Stop Button** | Stop playback and reset to start |
| **Timeline Slider** | Seek to specific time |

### Editing Functions

| Shortcut | Function | Description |
|----------|----------|-------------|
| **Ctrl + Z** | Undo | Undo last action (max 50 steps) |
| **Ctrl + Shift + Z** | Redo | Redo last undone action |
| **Ctrl + S** | Save | Save current chart |
| **T** | Toggle Edit Scope | Switch between Per-Note / Per-Measure |
| **Delete** | Delete Note | Delete selected notes |

### Edit Scope (T Key)

**Per-Note Mode**: Apply changes to individual notes
**Per-Measure Mode**: Apply changes to note ranges by measure

This feature is prepared for future BPM/tempo change functionality.

## Setup Instructions

### 1. Basic Setup

1. Create an empty GameObject in your scene
2. Add the `ChartEditor` component (from `ChartSystem` namespace)
3. Configure public fields in the Inspector

### 2. Required UI Components

```csharp
[Header("오디오 컨트롤 UI")]
public InputField audioPathInputField;    // Path to audio file
public Slider timelineSlider;             // Timeline seek slider
public Text currentTimeText;              // Display current time
public Text totalTimeText;                // Display total duration
public Button loadAudioButton;            // Load audio button
public Button playButton;                 // Play button
public Button pauseButton;                // Pause button
public Button stopButton;                 // Stop button

[Header("차트 정보 UI")]
public InputField songNameInput;          // Song name input
public InputField artistNameInput;        // Artist name input
public InputField bpmInput;               // BPM input
public InputField offsetInput;            // Audio offset (ms)

[Header("에디터 상태 UI")]
public Text modeText;                     // Current mode display
public Text gridSnapText;                 // Grid snap display
public Text statusText;                   // Status messages
```

### 3. Chart Settings

```csharp
[Header("차트 설정")]
public string songName = "";              // Song title
public string artistName = "";            // Artist name
public float bpm = 120f;                  // Beats per minute
public float offset = 0f;                 // Audio offset (seconds)

[Header("에디터 설정")]
public int keyCount = 4;                  // Lane count (4/5/6/7/8/10)
public KeyCode[] trackKeys;               // Input keys for each lane
public Transform[] noteSpawnPoints;       // Spawn positions per lane
public GameObject notePrefab;             // Note prefab
```

### 4. Prefab Requirements

**Note Prefab:**
- Must have `SpriteRenderer` component
- Can have custom visuals
- Will be pooled for performance

**Auto-generation:**
If `notePrefab` is null, the editor automatically creates a simple white square sprite.

## Technical Details

### Undo/Redo System

The editor uses JSON serialization for undo/redo:

```csharp
private Stack<ChartDataNew> undoStack = new Stack<ChartDataNew>();
private Stack<ChartDataNew> redoStack = new Stack<ChartDataNew>();
private const int MAX_UNDO_STACK = 50;
```

**How it works:**
1. Before each modification, current chart state is serialized to JSON
2. JSON is pushed onto undo stack
3. Ctrl+Z deserializes previous state
4. Maximum 50 undo steps to prevent memory issues

### Grid Snap Calculation

```csharp
double CalculateSnappedTiming(double currentTime)
{
    if (!gridSnapEnabled) return currentTime;

    double beatInterval = 60.0 / bpm;
    double snapInterval = beatInterval / (int)currentBeatDivision;

    return System.Math.Round(currentTime / snapInterval) * snapInterval;
}
```

### Long Note Bidirectional Placement

```csharp
void HandleLongNoteInput(double timing, int track)
{
    if (!isPlacingLongNote)
    {
        // First click - store start position
        longNoteStart = new NoteData(timing, track, selectedKeySoundType);
        longNoteTrack = track;
        isPlacingLongNote = true;
    }
    else
    {
        // Second click - determine start and end
        double startTime = System.Math.Min(longNoteStart.timing, timing);
        double endTime = System.Math.Max(longNoteStart.timing, timing);

        NoteData longNote = new NoteData(
            startTime,
            track,
            selectedKeySoundType,
            true,  // isLongNote
            endTime
        );

        AddNoteToChart(longNote);
        isPlacingLongNote = false;
    }
}
```

## Data Structures

### ChartDataNew

```csharp
[System.Serializable]
public class ChartDataNew
{
    public string songName;
    public string artistName;
    public string audioFileName;
    public float bpm;
    public float chartDifficulty;
    public List<NoteData> notes;
}
```

### NoteData

```csharp
[System.Serializable]
public class NoteData
{
    public double timing;              // Note hit time (seconds)
    public float beatTiming;           // Beat-based timing
    public int track;                  // Lane index (0-based)
    public KeySoundType keySoundType;  // Key sound to play
    public bool isLongNote;            // Is this a long note?
    public double longNoteEndTiming;   // End time for long notes
}
```

### Enums

All enums are defined in `Assets/GameEnums.cs`:

```csharp
public enum KeySoundType
{
    None, Kick, Snare, Hihat, Vocal1, Vocal2,
    Synth1, Synth2, Bass, Piano, Guitar
}

public enum JudgmentMode
{
    Normal, Hard, Super,
    // Backward compatibility aliases
    JudgmentMode_Normal = Normal,
    JudgmentMode_Hard = Hard,
    JudgmentMode_Super = Super
}
```

## Workflow Example

### Creating a New Chart

1. **Setup**
   - Assign UI elements in Inspector
   - Set default BPM (e.g., 120)
   - Set key count (e.g., 4K)

2. **Load Audio**
   - Enter audio file path in `audioPathInputField`
   - Click "Load Audio" button
   - Audio loads via UnityWebRequest

3. **Chart Metadata**
   - Enter song name in `songNameInput`
   - Enter artist name in `artistNameInput`
   - Adjust BPM if needed in `bpmInput`

4. **Place Notes**
   - Press **N** for normal notes
   - Click on tracks to place notes
   - Press **L** for long notes
   - Click start and end positions

5. **Adjust Grid**
   - Press **G** to cycle grid snap
   - Use `[` `]` for beat division

6. **Save Chart**
   - Press **Ctrl + S**
   - Chart saved as JSON

## Keyboard Reference

### Essential Shortcuts
```
N              - Normal note mode
L              - Long note mode
Space          - Play/Pause
G              - Cycle grid snap
T              - Toggle edit scope
Ctrl + Z       - Undo
Ctrl + Shift+Z - Redo
Ctrl + S       - Save chart
```

### Grid Control
```
G              - Cycle snap: 1/4 → 1/8 → 1/16 → 1/32 → OFF
[              - Decrease beat division
]              - Increase beat division
```

## Advanced Features

### Edit Scope System

**Purpose**: Prepared for future BPM change support

**Two Modes:**
1. **Per-Note**: Apply changes to individual notes
2. **Per-Measure**: Apply changes to note ranges

**Toggle**: Press **T** key

Currently acts as a placeholder for future measure-based editing features.

### Undo Stack Management

```csharp
void SaveUndoState()
{
    string json = JsonUtility.ToJson(currentChart);
    ChartDataNew snapshot = JsonUtility.FromJson<ChartDataNew>(json);

    undoStack.Push(snapshot);

    // Limit stack size
    if (undoStack.Count > MAX_UNDO_STACK)
    {
        // Remove oldest entry
        var temp = undoStack.ToArray();
        undoStack.Clear();
        for (int i = 1; i < temp.Length; i++)
            undoStack.Push(temp[i]);
    }

    redoStack.Clear(); // Clear redo on new action
}
```

## Known Limitations

### Phase 1 (Current)
- No visual waveform display
- No copy/paste functionality
- No BPM changes during chart
- No slide notes
- Manual JSON save only (no file dialog)

### Planned for Phase 2
- Visual timeline with waveform
- Advanced clipboard operations
- Multiple BPM sections
- Slide note support
- File browser integration

## Troubleshooting

### Issue: Notes don't snap to grid
**Solution**: Press **G** until desired snap mode is active. Check `gridSnapText` UI display.

### Issue: Long notes don't connect
**Solution**: Ensure you're in Long note mode (press **L**). Long notes must be on the same track.

### Issue: Undo doesn't work
**Solution**: Check that you've made changes after editor initialization. First action cannot be undone.

### Issue: Audio doesn't load
**Solution**:
- Check file path is correct
- File must be in `StreamingAssets` or accessible path
- Supported formats: .wav, .ogg, .mp3

### Issue: Can't find ChartEditor component
**Solution**: Make sure you're looking in `ChartSystem` namespace. Use `using ChartSystem;` in scripts.

## Integration with Main Game

### Loading Charts in Gameplay

```csharp
using ChartSystem;

// Load chart created in editor
ChartDataNew editorChart = LoadChartFromJSON("path/to/chart.json");

// Convert to gameplay format if needed
ChartData gameplayChart = ConvertToGameplayFormat(editorChart);

// Use in game
GameManager.Instance.LoadChart(gameplayChart);
```

### Enum Compatibility

All enums (JudgmentMode, JudgmentType, KeySoundType) are shared between:
- Editor (`ChartSystem` namespace)
- Gameplay (global namespace)

Defined in: `Assets/GameEnums.cs`

## File Structure

```
Assets/
├── edit/
│   ├── ChartEditor.cs          ← Main editor (this file)
│   ├── ChartDataNew.cs         ← Editor chart data
│   └── ChartEditorNew.cs       ← DEPRECATED (commented out)
├── GameEnums.cs                 ← All game enums
└── Play/
    ├── NoteData.cs              ← Shared note data structure
    └── ...
```

## Version History

### Version 1.0 (Phase 1) - 2025-10-25
- ✅ Initial release
- ✅ Basic note placement (Normal/Long)
- ✅ Bidirectional long notes
- ✅ Grid snap system (G key)
- ✅ Undo/Redo (50 steps)
- ✅ Keyboard shortcuts
- ✅ Audio control
- ✅ Chart save/load

### Version 2.0 (Phase 2) - Planned
- ⏳ Visual timeline
- ⏳ Advanced editing
- ⏳ BPM changes
- ⏳ Slide notes

## Support

For issues or questions:
- Check `DEVELOPMENT_TODO.md` for roadmap
- See `CLAUDE.md` for architecture details
- Refer to `SESSION_SUMMARY_2025-10-25_2.md` for recent changes

---

**Last Updated**: 2025-10-25
**Phase**: 1 Complete
**Status**: Production Ready
