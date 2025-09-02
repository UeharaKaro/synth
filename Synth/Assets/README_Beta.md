# Chart Editor Beta System

## Overview

The Chart Editor Beta system is a completely independent and self-contained implementation of the rhythm game chart editing functionality. It requires no external dependencies beyond Unity's built-in components and can be used standalone without the original ChartEditor system.

## Components

### Core Classes

1. **ChartEditorBeta.cs**
   - Main chart editing interface
   - Handles audio playback and note input recording
   - Provides UI for loading/saving charts
   - Supports real-time note placement during audio playback

2. **ChartDataBeta.cs**
   - Contains chart metadata (song name, artist, BPM)
   - Manages collection of note data
   - Provides utility methods for chart manipulation

3. **NoteDataBeta.cs**
   - Represents individual note information
   - Stores timing, track, and key sound data
   - Handles BPM-based timing calculations

4. **NoteBeta.cs**
   - MonoBehaviour component for individual notes
   - Handles note movement and visual representation
   - Implements judgment system with configurable difficulty modes
   - Supports both normal and long notes

5. **AudioManagerBeta.cs**
   - Simplified audio management using Unity AudioSource
   - Handles music, SFX, and key sound playback
   - No external FMOD dependencies
   - Singleton pattern for global access

### Enums

- **KeySoundTypeBeta**: Defines different instrument sounds (Kick, Snare, Piano, etc.)
- **SFXTypeBeta**: Basic sound effects (Metronome, Hit, Miss)
- **JudgmentModeBeta**: Difficulty modes (Normal, Hard, Super)
- **JudgmentTypeBeta**: Judgment results (S_Perfect, Perfect, Great, Good, Bad, Miss)

## Usage

### Setting Up the Chart Editor

1. Create an empty GameObject and attach `ChartEditorBeta` component
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
using Beta;

ChartDataBeta chart = new ChartDataBeta("My Song", "My Artist", 120f);

// Add a note
NoteDataBeta note = new NoteDataBeta(2.5f, 0, KeySoundTypeBeta.Kick);
chart.AddNote(note);

// Play audio (requires AudioManagerBeta)
AudioClip audioClip = // load your audio clip
AudioManagerBeta.Instance.PlayMusic(audioClip);
```

### Note Judgment System

The beta system includes three judgment modes:

- **Normal**: Beginner-friendly with wider timing windows
- **Hard**: Intermediate difficulty with tighter timing
- **Super**: Expert difficulty with very precise timing requirements

## Key Features

### Independence
- **No external dependencies**: Only uses Unity built-in systems
- **Self-contained namespace**: All classes in `Beta` namespace
- **No FMOD requirements**: Uses Unity AudioSource instead
- **No settings dependencies**: Contains own configuration system

### Functionality
- **Real-time chart editing**: Place notes while audio plays
- **Multiple judgment modes**: Configurable difficulty levels
- **Long note support**: Hold-type notes with start/end timing
- **Audio management**: Built-in audio playback system
- **Chart serialization**: Save/load charts as JSON files

### Testing
Use `BetaSystemTest.cs` to verify all components work correctly:
- Attach to a GameObject in your scene
- Check "Run Automatic Tests" to test on Start()
- Use context menu options for manual testing

## Integration

The beta system can be:
1. Used completely standalone for chart editing
2. Merged with existing systems when ready
3. Extended with additional features as needed

Since all components are in the `Beta` namespace, they won't conflict with existing implementations and can coexist in the same project.

## File Structure

```
Assets/
├── ChartEditorBeta.cs      # Main editor component
├── ChartDataBeta.cs        # Chart and note data structures
├── NoteBeta.cs             # Individual note behavior
├── AudioManagerBeta.cs     # Audio management system
├── BetaSystemTest.cs       # Testing and verification
└── README_Beta.md          # This documentation
```

## Migration Notes

When ready to merge with the main system:
1. The beta components can replace corresponding original components
2. Or can be used as reference for improving the original system
3. Charts created with the beta system are compatible and portable
4. Enum values and structures are designed to be easily convertible

The beta system demonstrates that all chart editing functionality can work independently without complex external dependencies.