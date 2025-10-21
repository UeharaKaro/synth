# Synth - Unity Rhythm Game

<div align="center">

![Unity](https://img.shields.io/badge/Unity-2021.3+-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Mac%20%7C%20Linux-lightgrey.svg)

**A modern rhythm game built with Unity featuring advanced gameplay mechanics and comprehensive UI systems**

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage) • [Documentation](#-documentation) • [Contributing](#-contributing)

</div>

---

## 📖 Overview

Synth is a feature-rich rhythm game developed in Unity, offering multiple key modes (4K-10K), various difficulty levels, and a complete game system including song selection, gameplay, and result screens.

### Key Highlights

- 🎵 Multiple key modes support (4K, 5K, 6K, 7K, 8K, 10K)
- 🎯 Advanced judgment system with multiple difficulty modes (Normal, Hard, Super)
- 📊 Comprehensive scoring system with accuracy calculation
- 🎨 Modern UI with smooth animations
- 🎼 Chart editor for creating custom beatmaps
- 🔊 FMOD audio integration
- 📈 Result screen with detailed statistics and rankings

## ✨ Features

### Gameplay Systems

- **Multi-Key Support**: Play with 4 to 10 keys
- **Judgment Modes**:
  - Normal: Casual-friendly timing windows
  - Hard: Competitive timing for skilled players
  - Super: Expert-level precision (planned)
- **Note Types**:
  - Normal notes
  - Long notes (hold)
  - Slide notes (planned)

### UI Systems

#### Main Menu
- Clean and intuitive interface
- Keyboard navigation support
- Easy access to all game modes

#### Song Selection
- Comprehensive song database with ScriptableObject architecture
- Multiple difficulty support per song
- Album art and background image display
- Song preview functionality
- Filtering by genre, artist, key count
- Song locking system for progression

#### Result Screen
- Detailed play statistics
- Judgment breakdown (S Perfect, Perfect, Great, Good, Bad, Miss)
- Accuracy calculation
- Ranking system (SSS to F)
- Full Combo and Perfect Play indicators
- Smooth animations and transitions

### Chart Editor (Beta)

- Visual chart editor for creating beatmaps
- BPM and offset configuration
- Note placement and timing
- Export to custom chart format
- See [ChartEditorBeta_Documentation.md](ChartEditorBeta_Documentation.md) for details

## 🚀 Installation

### Prerequisites

- **Unity**: 2021.3 or higher recommended
- **Git**: For cloning the repository
- **Operating System**: Windows, macOS, or Linux

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/UeharaKaro/synth.git
   cd synth
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" and select the `Synth` folder
   - Open the project with Unity 2021.3+

3. **Install Dependencies**
   - FMOD is already included in the project
   - TextMeshPro should auto-install when opening the project

4. **Setup Song Database**
   - Navigate to `Assets/songselect/`
   - Right-click → Create → Rhythm Game → Song Database
   - Add your songs to the database
   - See [Song Selection README](Synth/Assets/songselect/README_SongSelection.md)

## 📚 Usage

### Playing the Game

1. **Start the game** from the Main Menu scene
2. **Select a song** from the Song Selection screen
3. **Choose difficulty** and key mode
4. **Play!** Hit notes in time with the music
5. **View results** after completing the song

### Creating Charts

1. Open the Chart Editor scene
2. Load your audio file
3. Set BPM and offset
4. Place notes on the timeline
5. Export your chart
6. See [Chart Editor Documentation](ChartEditorBeta_Documentation_Kr.md) (Korean)

### Keyboard Controls

**Song Selection:**
- ↑/↓: Navigate songs
- ←/→: Change difficulty
- Shift: Change key mode
- Enter: Select song
- Space: Preview song
- ESC: Back to menu

**Gameplay:**
- Default: D, F, J, K (4K mode)
- Configurable in game settings

## 📁 Project Structure

```
synth/
├── Synth/                      # Unity project folder
│   └── Assets/
│       ├── AudioManager.cs     # Audio management system
│       ├── Play/               # Gameplay scripts
│       │   ├── NoteManager.cs
│       │   ├── ScoreSystem.cs
│       │   ├── GearController.cs
│       │   └── ...
│       ├── Startmenu/          # Main menu UI
│       │   ├── MainMenuUI.cs
│       │   └── ...
│       ├── songselect/         # Song selection system
│       │   ├── SongSelectionUI.cs
│       │   ├── SongDatabase.cs
│       │   └── SongData.cs
│       ├── playresult/         # Result screen system
│       │   ├── PlayResultUI.cs
│       │   ├── PlayResultData.cs
│       │   └── GameResultManager.cs
│       ├── option/             # Settings/Options
│       │   ├── OptionMenuUI.cs
│       │   └── GameSettings.cs
│       ├── edit/               # Chart editor
│       │   └── ChartEditorNew.cs
│       └── Plugins/            # Third-party plugins
│           └── FMOD/           # FMOD audio engine
├── .github/                    # GitHub configuration
│   └── workflows/              # CI/CD workflows
├── README.md                   # This file
├── LICENSE                     # MIT License
└── .gitignore                  # Git ignore rules
```

## 📖 Documentation

Detailed documentation for each system:

- **[Song Selection System](Synth/Assets/songselect/README_SongSelection.md)**: Song database, UI setup, keyboard controls
- **[Play Result System](Synth/Assets/playresult/README_PlayResult.md)**: Result screen, ranking, statistics
- **[Chart Editor](ChartEditorBeta_Documentation_Kr.md)**: Creating and editing beatmaps (Korean)
- **[Chart Editor (English)](ChartEditorBeta_Documentation.md)**: Chart editor guide in English

## 🎮 Game Systems

### Scoring System

The game features a comprehensive scoring system with:
- **Score calculation** based on judgment accuracy
- **Combo system** for consecutive hits
- **Accuracy percentage** calculation
- **Rank determination** (SSS, SS, S, A, B, C, D, F)

### Judgment System

Multiple timing windows for different skill levels:

**Normal Mode:**
- Perfect: ±41.66ms
- Great: ±83.33ms
- Good: ±120ms
- Bad: ±150ms

**Hard Mode:**
- S Perfect: ±16.67ms
- Perfect: ±32.25ms
- Great: ±62.49ms
- Good: ±88.33ms
- Bad: ±120ms

## 🛠️ Technology Stack

- **Engine**: Unity 2021.3+
- **Language**: C#
- **Audio**: FMOD
- **UI**: Unity UI + TextMeshPro
- **Data Management**: ScriptableObjects
- **Version Control**: Git

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### How to Contribute

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **UeharaKaro** - *Initial work* - [GitHub](https://github.com/UeharaKaro)

## 🙏 Acknowledgments

- FMOD for audio engine
- Unity Technologies for the game engine
- All contributors and testers

## 📮 Contact

Project Link: [https://github.com/UeharaKaro/synth](https://github.com/UeharaKaro/synth)

---

<div align="center">
Made with ❤️ using Unity
</div>
