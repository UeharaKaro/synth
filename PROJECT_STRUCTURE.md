# Synth Project Structure

This document provides a detailed overview of the project's file and folder structure.

## Root Directory

```
synth/
├── .github/                    # GitHub configuration
│   ├── ISSUE_TEMPLATE/        # Issue templates
│   │   ├── bug_report.md
│   │   └── feature_request.md
│   ├── workflows/             # GitHub Actions CI/CD
│   │   └── qodana_code_quality.yml
│   └── PULL_REQUEST_TEMPLATE.md
├── Synth/                      # Unity project folder
├── .gitignore                 # Git ignore rules
├── CONTRIBUTING.md            # Contribution guidelines
├── LICENSE                    # MIT License
├── PROJECT_STRUCTURE.md       # This file
├── README.md                  # Main documentation
├── ChartEditorBeta_Documentation.md      # Chart editor docs (EN)
├── ChartEditorBeta_Documentation_Kr.md   # Chart editor docs (KR)
└── qodana.yaml               # Code quality config
```

## Synth/Assets/ Directory

### Core Systems

```
Assets/
├── AudioManager.cs            # Main audio management system
├── AudioManagerNew.cs         # Updated audio manager
├── RhytmManager.cs           # Core rhythm game manager
├── SystemTest.cs             # System testing utilities
└── UniversalAudioEncryptor.cs # Audio encryption
```

### Play/ - Gameplay Systems

```
Play/
├── NoteManager.cs            # Note spawning and lifecycle
├── NoteController.cs         # Individual note control
├── NoteData.cs              # Note data structures
├── NoteNew.cs               # Updated note system
├── ScoreSystem.cs           # Scoring and accuracy calculation
├── GearController.cs        # Gear/track visualization
├── GearSettings.cs          # Gear configuration
├── TrackManager.cs          # Track/lane management
├── InputManager.cs          # Input handling
├── GameInputController.cs   # Game-specific input
├── LongNoteSystem.cs        # Long note mechanics
├── ComboJudgmentDisplay.cs  # Combo UI display
├── JudgmentOffsetDisplay.cs # Judgment timing display
└── HPBarAnimator.cs         # HP bar animations
```

### Startmenu/ - Main Menu

```
Startmenu/
├── MainMenuUI.cs            # New main menu system
│   - PLAY button → Song selection
│   - OPTION button → Settings
│   - EXIT button → Quit game
├── MainMenuManager.cs       # Menu navigation manager
└── StartMenuUI.cs          # Judgment mode selector UI
```

### songselect/ - Song Selection

```
songselect/
├── SongSelectionUI.cs       # Main song selection UI
│   - Song navigation (↑↓)
│   - Difficulty selection (←→)
│   - Key mode selection (Shift)
│   - Song preview
│   - Album art display
├── SongData.cs             # Song data structure
│   - Song metadata
│   - Difficulty info
│   - Chart paths per key mode
├── SongDatabase.cs         # ScriptableObject database
│   - Song management
│   - Search and filtering
│   - Sort functionality
├── SongSelectionManager.cs # Legacy manager
└── README_SongSelection.md # Documentation
```

### playresult/ - Result Screen

```
playresult/
├── PlayResultUI.cs          # Result screen UI
│   - Score display with animation
│   - Judgment breakdown
│   - Rank display (SSS~F)
│   - Retry/Menu buttons
├── PlayResultData.cs        # Result data structure
│   - Score and accuracy
│   - Judgment counts
│   - Rank calculation
├── GameResultManager.cs     # Singleton result manager
│   - Scene data transfer
│   - Song info management
├── ResultSceneLoader.cs     # Auto-load results
├── JudgmentResult.cs       # Judgment data
└── README_PlayResult.md    # Documentation
```

### option/ - Settings

```
option/
├── OptionMenuUI.cs          # Settings UI
│   - Audio settings
│   - Visual settings
│   - Control settings
├── GameSettings.cs          # Settings data structure
└── GameSettingsManager.cs   # Settings persistence
```

### edit/ - Chart Editor

```
edit/
├── ChartEditorNew.cs        # Main chart editor
├── ChartEditor(test).cs     # Test version
└── ChartDataNew.cs         # Chart data structures
```

### Plugins/ - Third-party

```
Plugins/
└── FMOD/                    # FMOD audio engine
    ├── addons/
    ├── platforms/
    └── src/
```

## Data Flow

### Game Flow

```
MainMenuUI
    ↓ (PLAY)
SongSelectionUI
    ↓ (Select Song)
GameScene (Gameplay)
    ↓ (Song Complete)
PlayResultUI
    ↓ (Retry/Back)
SongSelectionUI or MainMenuUI
```

### Data Transfer

```
SongSelectionUI
    → GameResultManager.SetCurrentSongInfo()
    → PlayerPrefs (backup)

GameScene
    → ScoreSystem.GetGameResult()
    → GameResultManager.SaveResultAndShowResultScreen()

PlayResultUI
    → GameResultManager.GetCurrentResultData()
    → Display results
```

## Key Files by Feature

### Song Selection
- `SongSelectionUI.cs` - Main UI controller
- `SongDatabase.cs` - Song data management
- `SongData.cs` - Individual song info

### Gameplay
- `NoteManager.cs` - Note spawning
- `ScoreSystem.cs` - Scoring logic
- `GearController.cs` - Visual feedback

### Results
- `PlayResultUI.cs` - Result display
- `GameResultManager.cs` - Data persistence

### Settings
- `OptionMenuUI.cs` - Settings UI
- `GameSettings.cs` - Settings data

## ScriptableObjects

### SongDatabase
**Location**: Create via `Right-click → Create → Rhythm Game → Song Database`

**Purpose**: Centralized song data management

**Fields**:
- Song list
- Default difficulty
- Default key count

## Naming Conventions

- **Classes**: `PascalCase` (e.g., `SongSelectionUI`)
- **Methods**: `PascalCase` (e.g., `LoadSong()`)
- **Fields**: `camelCase` for private, `PascalCase` for public
- **Folders**: `lowercase` or `PascalCase`

## Adding New Features

When adding a new feature:

1. Create a new folder in `Assets/[feature-name]/`
2. Add main script(s)
3. Create `README_[Feature].md` for documentation
4. Update this `PROJECT_STRUCTURE.md`
5. Update main `README.md` if user-facing

## Dependencies

### Required Unity Packages
- TextMeshPro (auto-installed)
- Unity UI

### Third-party Assets
- FMOD (`Assets/Plugins/FMOD/`)

## Build Output

Builds are generated in `/[Bb]uild/` or `/[Bb]uilds/` (gitignored)

## Documentation Files

| File | Purpose |
|------|---------|
| `README.md` | Main project documentation |
| `CONTRIBUTING.md` | Contribution guidelines |
| `LICENSE` | MIT License |
| `PROJECT_STRUCTURE.md` | This file |
| `ChartEditorBeta_Documentation.md` | Chart editor guide (EN) |
| `ChartEditorBeta_Documentation_Kr.md` | Chart editor guide (KR) |
| `Assets/songselect/README_SongSelection.md` | Song selection system |
| `Assets/playresult/README_PlayResult.md` | Result screen system |

## Version Control

### Tracked Files
- `.cs` scripts
- `.md` documentation
- `.yaml` configurations
- Chart data files

### Ignored Files (see `.gitignore`)
- Unity Library/
- Build outputs
- Temporary files
- IDE-specific files
- User settings

## Questions?

If you have questions about the project structure, please:
1. Check this document
2. Check individual README files in feature folders
3. Open an issue with the "question" label
