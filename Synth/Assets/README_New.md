# Chart Editor System

## Overview

The Chart Editor system is a completely independent and self-contained implementation of the rhythm game chart editing functionality. It requires no external dependencies beyond Unity's built-in components and can be used standalone without complex external systems.

## Components

### Core Classes

1. **ChartEditorNew.cs**
   - Main chart editing interface
   - Handles audio playback and note input recording
   - Provides UI for loading/saving charts
   - Supports real-time note placement during audio playback

2. **ChartDataNew.cs**
   - Contains chart metadata (song name, artist, BPM)
   - Manages collection of note data
   - Provides utility methods for chart manipulation

3. **NoteData.cs**
   - Represents individual note information
   - Stores timing, track, and key sound data
   - Handles BPM-based timing calculations

4. **NoteNew.cs**
   - MonoBehaviour component for individual notes
   - Handles note movement and visual representation
   - Implements judgment system with configurable difficulty modes
   - Supports both normal and long notes

5. **AudioManagerNew.cs**
   - Simplified audio management using Unity AudioSource
   - Handles music, SFX, and key sound playback
   - No external FMOD dependencies
   - Singleton pattern for global access

### Enums

- **KeySoundType**: Defines different instrument sounds (Kick, Snare, Piano, etc.)
- **SFXType**: Basic sound effects (Metronome, Hit, Miss)
- **JudgmentMode**: Difficulty modes (Normal, Hard, Super)
- **JudgmentType**: Judgment results (S_Perfect, Perfect, Great, Good, Bad, Miss)

## Usage

### Setting Up the Chart Editor

1. Create an empty GameObject and attach `ChartEditorNew` component
2. Assign UI elements (buttons, sliders, text fields) in the inspector
3. Set up note spawn points and track keys
4. Configure BPM and other chart settings

### Basic Chart Creation Workflow

1. Load an audio file using the audio path input field
2. Press Play to start audio playback
3. Press the configured track keys (A, S, D, F by default) during playback to place notes
4. Save the chart when finished

### Using Individual Components

```csharp
// Create a new chart
using ChartSystem;

ChartDataNew chart = new ChartDataNew("My Song", "My Artist", 120f);

// Add a note
NoteData note = new NoteData(2.5f, 0, KeySoundType.Kick);
chart.AddNote(note);

// Play audio (requires AudioManagerNew)
AudioClip audioClip = // load your audio clip
AudioManagerNew.Instance.PlayMusic(audioClip);
```

### Note Judgment System

The system includes three judgment modes:

- **Normal**: Beginner-friendly with wider timing windows
- **Hard**: Intermediate difficulty with tighter timing
- **Super**: Expert difficulty with very precise timing requirements

## Key Features

### Independence
- **No external dependencies**: Only uses Unity built-in systems
- **Self-contained namespace**: All classes in `ChartSystem` namespace
- **No FMOD requirements**: Uses Unity AudioSource instead
- **No settings dependencies**: Contains own configuration system

### Functionality
- **Real-time chart editing**: Place notes while audio plays
- **Multiple judgment modes**: Configurable difficulty levels
- **Long note support**: Hold-type notes with start/end timing
- **Audio management**: Built-in audio playback system
- **Chart serialization**: Save/load charts as JSON files

### Testing
Use `SystemTest.cs` to verify all components work correctly:
- Attach to a GameObject in your scene
- Check "Run Automatic Tests" to test on Start()
- Use context menu options for manual testing

## Integration

The system can be:
1. Used completely standalone for chart editing
2. Merged with existing systems when ready
3. Extended with additional features as needed

Since all components are in the `ChartSystem` namespace, they won't conflict with existing implementations and can coexist in the same project.

## File Structure

```
Assets/
├── ChartEditorNew.cs      # Main editor component
├── ChartDataNew.cs        # Chart and note data structures
├── NoteNew.cs             # Individual note behavior
├── AudioManagerNew.cs     # Audio management system
├── SystemTest.cs          # Testing and verification
└── README_New.md          # This documentation
```

## Migration Notes

When ready to merge with the main system:
1. The components can replace corresponding original components
2. Or can be used as reference for improving the original system
3. Charts created with the system are compatible and portable
4. Enum values and structures are designed to be easily convertible

The system demonstrates that all chart editing functionality can work independently without complex external dependencies.