# ChartEditorBeta - Unity Rhythm Game Chart Editor

## Overview

ChartEditorBeta is an enhanced version of the Unity rhythm game chart editor with improved functionality, better user experience, and advanced features for professional chart creation.

## Features

### Core Functionality
- **Multi-lane support**: 4, 5, 6, 7, 8, and 10 lane configurations
- **Note types**: Normal notes and long notes with visual trails
- **Real-time preview**: Play mode with accurate timing simulation
- **Grid snapping**: Precise note placement with customizable divisions
- **AES encryption**: Secure audio file encryption for distribution

### Editor Controls

#### Mode Switching
- **N key**: Switch to Normal note mode
- **L key**: Switch to Long note mode
- **P key**: Toggle Preview mode

#### Lane Management
- **+ key / = key**: Increase lane count
- **- key**: Decrease lane count

#### Audio Controls
- **Space**: Play/Pause audio
- **Timeline Slider**: Seek to specific time

#### Note Operations
- **Left Click + Drag**: Place notes (mode dependent)
- **Shift + Click**: Multi-select notes
- **Ctrl + A**: Select all notes
- **Delete / Backspace**: Delete selected notes
- **Escape**: Clear selection

#### Editor Functions
- **Ctrl + Z**: Undo
- **Ctrl + Shift + Z**: Redo
- **Ctrl + S**: Save chart (if UI button connected)
- **Ctrl + O**: Load chart (if UI button connected)

#### Grid and Timing
- **G key**: Cycle grid snap modes (None, 1/4, 1/8, 1/16, 1/32)
- **[ key**: Decrease beat division
- **] key**: Increase beat division

#### Preview Mode Keys (Lane-dependent)
- **4 lanes**: D, F, J, K
- **5 lanes**: D, F, Space, J, K
- **6 lanes**: S, D, F, J, K, L
- **7 lanes**: S, D, F, Space, J, K, L
- **8 lanes**: A, S, D, F, J, K, L, ;
- **10 lanes**: A, S, D, F, G, H, J, K, L, ;

## Setup Instructions

### 1. Basic Setup
1. Create an empty GameObject in your scene
2. Add the `ChartEditorBeta` component
3. Configure the public fields in the inspector

### 2. Required Components
```csharp
// Audio components
public AudioSource audioSource; // Automatically added
public AudioManagerBeta audioManagerBeta; // Automatically created

// UI elements (optional but recommended)
public InputField audioPathInputField;
public Slider timelineSlider;
public Text currentTimeText;
public Text totalTimeText;
public Button playButton;
public Button pauseButton;
public Button stopButton;
public Button saveButton;
public Button loadButton;

// Visual elements
public GameObject notePrefab;
public GameObject longNotePrefab;
public Transform noteContainer;
public Transform[] tracks;
public Material gridLineMaterial;
```

### 3. Prefab Creation
Use the `ChartEditorBetaPrefabCreator` script to automatically generate required prefabs:

1. Attach `ChartEditorBetaPrefabCreator` to any GameObject
2. Configure the materials and settings
3. Call `CreatePrefabs()` or use the context menu
4. Assign the created prefabs to ChartEditorBeta

### 4. UI Setup
The editor can work without UI, but for full functionality:

1. Create a Canvas
2. Add UI elements for timeline, buttons, and info displays
3. Connect them to the ChartEditorBeta component
4. Use `ChartEditorBetaPrefabCreator.CreateEditorUIPrefab()` for automatic UI creation

## Advanced Features

### Audio Encryption
```csharp
// Encrypt audio file
ChartEditorBetaFileUtils.EncryptAudioFile(
    "path/to/audio.wav", 
    "path/to/encrypted.eaw", 
    "your-encryption-key"
);

// AudioManagerBeta automatically decrypts during loading
audioManagerBeta.LoadAudioFile("path/to/encrypted.eaw");
```

### Chart Export/Import
```csharp
// Export with metadata
chartEditor.SaveChartAs(
    "path/to/chart.chart",
    "Song Title",
    "Artist Name", 
    "Charter Name"
);

// Load from specific path
chartEditor.LoadChartFrom("path/to/chart.chart");
```

### Preview System Integration
```csharp
// Access preview system
ChartEditorBetaPreview preview = chartEditor.previewSystem;

// Subscribe to events
preview.OnNoteJudged += (judgment) => Debug.Log($"Judgment: {judgment}");
preview.OnNoteSpawned += (noteData) => Debug.Log($"Note spawned: {noteData.track}");
```

## File Format

### Chart Data Structure
```json
{
    "audioFileName": "song.wav",
    "bpm": 120.0,
    "laneCount": 4,
    "scrollSpeed": 8.0,
    "beatDivision": 4,
    "audioOffset": 0.0,
    "notes": [
        {
            "timing": 1.0,
            "beatTiming": 2.0,
            "track": 0,
            "keySoundType": 0,
            "isLongNote": false,
            "longNoteEndTiming": 0.0
        }
    ]
}
```

### Export Data with Metadata
```json
{
    "metadata": {
        "title": "Song Title",
        "artist": "Artist Name",
        "charter": "Charter Name",
        "createdDate": "2023-12-07 10:30:00",
        "version": "1.0"
    },
    "chartData": { /* Chart data structure */ }
}
```

## Performance Considerations

### Note Pooling
The preview system uses object pooling for optimal performance:
- Notes are reused rather than constantly created/destroyed
- Pool size automatically adjusts based on chart complexity
- Memory usage is minimized for long charts

### Grid System
- Grid lines are generated dynamically based on BPM and beat division
- Only visible grid lines are rendered
- Grid updates only when settings change

### Audio Management
- Audio decryption is performed once during loading
- Encrypted files are cached in memory for quick access
- Support for streaming large audio files

## Integration with Existing Systems

### Compatibility
- **AudioManager.cs**: Maintains compatibility, uses FMOD when available
- **NoteData.cs**: Extends with NoteDataBeta, preserves original structure
- **RhythmManager.cs**: Uses existing judgment system and enums
- **GameSettings.cs**: Integrates with existing settings system

### Migration
To use ChartEditorBeta with existing charts:
```csharp
// Convert existing ChartData to ChartDataBeta
ChartDataBeta betaChart = new ChartDataBeta();
betaChart.audioFileName = originalChart.audioFileName;
betaChart.bpm = originalChart.bpm;
betaChart.laneCount = 4; // Set appropriate lane count

// Convert notes
foreach(var note in originalChart.notes)
{
    var betaNote = new NoteDataBeta(
        note.timing, 
        note.track, 
        note.keySoundType, 
        note.isLongNote, 
        note.longNoteEndTiming
    );
    betaChart.notes.Add(betaNote);
}
```

## Troubleshooting

### Common Issues

1. **Notes not appearing**: Check that `notePrefab` and `noteContainer` are assigned
2. **Grid not showing**: Ensure `gridLineMaterial` is assigned and `showGrid` is true
3. **Audio not loading**: Verify file path and encryption key if using encrypted files
4. **Preview mode not working**: Check that lanes are properly configured and input keys are not conflicting

### Debug Options
```csharp
// Enable debug logging in ChartEditorBeta
Debug.Log("Current mode: " + currentEditMode);
Debug.Log("Selected notes: " + selectedNotes.Count);
Debug.Log("Preview active: " + previewSystem.IsActive);
```

### Performance Monitoring
```csharp
// Monitor performance
Debug.Log("Active preview notes: " + previewSystem.ActiveNoteCount);
Debug.Log("Remaining notes: " + previewSystem.RemainingNoteCount);
```

## Examples

### Basic Usage
```csharp
public class ChartEditorController : MonoBehaviour
{
    public ChartEditorBeta chartEditor;
    
    void Start()
    {
        // Load audio file
        chartEditor.LoadAudioFile("Assets/Audio/song.wav");
        
        // Set BPM
        chartEditor.UpdateBPM(140f);
        
        // Set lane count
        chartEditor.UpdateLaneCount(6);
    }
    
    void Update()
    {
        // Custom controls
        if (Input.GetKeyDown(KeyCode.F1))
        {
            chartEditor.SaveChart();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            chartEditor.LoadChart();
        }
    }
}
```

### Advanced Integration
```csharp
public class AdvancedChartEditor : MonoBehaviour
{
    public ChartEditorBeta chartEditor;
    
    void Start()
    {
        // Subscribe to events
        chartEditor.previewSystem.OnNoteJudged += HandleJudgment;
        chartEditor.previewSystem.OnPreviewStarted += OnPreviewStart;
        
        // Configure settings
        chartEditor.scrollSpeed = 10f;
        chartEditor.gridSnapMode = GridSnapMode.Beat_1_8;
        
        // Load encrypted audio
        var audioManager = AudioManagerBeta.Instance;
        audioManager.SetEncryptionKey("MySecretKey123");
        audioManager.LoadAudioFile("path/to/encrypted.eaw");
    }
    
    void HandleJudgment(JudgmentType judgment)
    {
        // Handle judgment feedback
        Debug.Log($"Player hit: {judgment}");
    }
    
    void OnPreviewStart()
    {
        Debug.Log("Preview mode started!");
    }
}
```

## Contributing

When extending ChartEditorBeta:

1. Follow the existing code structure and patterns
2. Add proper error handling and debug logging
3. Maintain compatibility with existing systems
4. Add unit tests using `ChartEditorBetaTest`
5. Update documentation for new features

## License

This is part of the Unity rhythm game project. Follow the project's licensing terms.