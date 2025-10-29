# Unity 씬 설정 (Scene Setup)

> **최근 업데이트**: 2025-10-27
> **완성도**: 75%

[← 메인 TODO로 돌아가기](DEVELOPMENT_TODO.md)

---

## 🎮 Unity 씬 설정 (Scene Setup)

### 필수 씬 구성 체크리스트

#### 1. MainMenu 씬 (메인 메뉴)
**씬 경로**: `Assets/Scenes/MainMenu.unity`

**필수 GameObject 구조**:
```
MainMenu (씬)
├── Canvas
│   ├── MainMenuUI (MainMenuUI.cs)
│   │   ├── Title
│   │   ├── ButtonNormalMode
│   │   ├── ButtonHardMode
│   │   ├── ButtonSuperMode
│   │   ├── ButtonOptions
│   │   ├── ButtonQuit
│   │   └── Background
│   └── MainMenuESCMenu (MainMenuESCMenu.cs)
│       ├── Panel (반투명 배경)
│       ├── ButtonOptions
│       ├── ButtonCredits
│       └── ButtonQuit
├── EventSystem
└── AudioManager (DontDestroyOnLoad)
```

**설정 체크리스트**:
```
[ ] Canvas 설정
    [ ] Render Mode: Screen Space - Overlay
    [ ] Canvas Scaler: Scale With Screen Size
    [ ] Reference Resolution: 1920x1080

[ ] MainMenuUI 컴포넌트 할당
    [ ] 모든 버튼 참조 연결
    [ ] OnClick 이벤트 설정

[ ] MainMenuESCMenu 설정
    [ ] ESC 키 바인딩 확인
    [ ] Panel 색상: RGBA(0, 0, 0, 0.5)

[ ] AudioManager 설정
    [ ] BGM 로드 및 재생
    [ ] DontDestroyOnLoad 체크
```

---

#### 2. SongSelection 씬 (곡 선택)
**씬 경로**: `Assets/Scenes/SongSelectionScene.unity`

**필수 GameObject 구조**:
```
SongSelection (씬)
├── Canvas
│   ├── SongSelectionUI (SongSelectionUI.cs 또는 SongSelectionUIAdvanced.cs)
│   │   ├── SongInfoPanel
│   │   │   ├── TitleText
│   │   │   ├── ArtistText
│   │   │   ├── BPMText
│   │   │   ├── DifficultyText
│   │   │   └── CoverImage
│   │   ├── ScrollView (Advanced 버전)
│   │   │   └── Content
│   │   │       └── SongListItem (Prefab)
│   │   ├── SearchInputField (Advanced)
│   │   ├── SortDropdown (Advanced)
│   │   └── FilterPanel (Advanced)
│   └── SongSelectionESCMenu (SongSelectionESCMenu.cs)
│       ├── Panel
│       ├── ButtonBackToMainMenu
│       ├── ButtonOptions
│       └── ButtonQuit
├── EventSystem
├── SongDatabase (ScriptableObject)
└── CoverArtLoader (Singleton)
```

**설정 체크리스트**:
```
[ ] SongSelectionUI 설정 (기본 또는 Advanced 선택)
    [ ] AudioManager 참조 연결
    [ ] SongDatabase 할당
    [ ] CoverArtLoader 참조

[ ] Advanced 버전 추가 설정
    [ ] ScrollView 설정 (Vertical, Elastic)
    [ ] SongListItem Prefab 할당
    [ ] SearchInputField 연결
    [ ] SortDropdown 옵션 설정
    [ ] FilterPanel 모든 필터 연결

[ ] SongSelectionESCMenu 설정
    [ ] AudioManager 자동 일시정지 확인

[ ] StreamingAssets 폴더 확인
    [ ] Charts/ 폴더 존재
    [ ] CoverArt/ 폴더 존재
    [ ] 샘플 차트 생성 (Tools → Create Sample Charts)
```

---

#### 3. GameScene 씬 (게임플레이)
**씬 경로**: `Assets/Scenes/GameScene.unity`

**필수 GameObject 구조**:
```
GameScene (씬)
├── Main Camera
├── GameManager (GameManager.cs)
│   └── 모든 시스템 참조 연결
├── RhythmManager (Singleton)
├── AudioManager (이미 DontDestroyOnLoad)
├── ChartLoader (Singleton)
├── NoteSpawner (또는 NoteManager)
├── GearController
│   ├── Tracks (4K~10K 트랙)
│   │   ├── Track_0
│   │   ├── Track_1
│   │   └── ...
│   └── JudgmentLine
├── HPSystem
│   └── HPBar
│       ├── HPFill
│       └── HPBarAnimator (HPBarAnimator.cs)
├── UI Canvas
│   ├── ProgressDisplay (ProgressDisplay.cs) ✨ 신규
│   ├── ScoreDisplay (ScoreDisplay.cs) ✨ 신규
│   ├── ComboJudgmentDisplay
│   ├── JudgmentOffsetDisplay
│   └── PauseMenuUI (PauseMenuUI.cs 또는 Advanced)
├── InputManager
└── EventSystem
```

**설정 체크리스트**:
```
[ ] GameManager 설정
    [ ] autoStart: false (기본값)
    [ ] useSampleChart: true (테스트용)
    [ ] useNoteSpawner: true (권장)
    [ ] 모든 시스템 참조 연결:
        [ ] AudioManager
        [ ] ChartLoader
        [ ] NoteSpawner
        [ ] HPSystem
        [ ] ProgressDisplay ✨
        [ ] ScoreDisplay ✨

[ ] ProgressDisplay 설정 ✨ 신규
    [ ] GameObject 생성 및 컴포넌트 추가
    [ ] AudioManager 참조 연결
    [ ] Bar Position: (0, 4, -0.1)
    [ ] Bar Width: 12
    [ ] Bar Height: 0.3
    [ ] Show Time Text: ✅
    [ ] Show BPM: ✅
    [ ] Show Percentage: ✅

[ ] ScoreDisplay 설정 ✨ 신규
    [ ] GameObject 생성 및 컴포넌트 추가
    [ ] Score Position: (-5, 4, -0.1)
    [ ] Use Comma: ✅
    [ ] Show Label: ✅
    [ ] Animate On Increase: ✅
    [ ] Show Score Popup: ✅

[ ] HPBarAnimator 설정 (개선됨) ✨
    [ ] Show Clear Line: ✅
    [ ] Clear Threshold: 70 (Normal 기준)
    [ ] Show HP Percentage: ✅
    [ ] Clear Line Color: 초록색

[ ] GearController 설정
    [ ] Track 개수: 4~10 (keyCount에 따라)
    [ ] Track Width: 1f
    [ ] Judgment Line Y: -3f

[ ] NoteSpawner 설정
    [ ] Spawn Offset: 2f
    [ ] GearController 참조

[ ] PauseMenuUI 설정
    [ ] enableOnlyInGameplay: true
    [ ] GameManager 자동 연동 확인
```

**참고 문서**:
- `Assets/Play/GameplayUI_Setup.md` (진행도/점수/HP 상세 가이드)

---

#### 4. ResultScene 씬 (결과 화면)
**씬 경로**: `Assets/Scenes/ResultScene.unity`

**필수 GameObject 구조**:
```
ResultScene (씬)
├── Canvas
│   └── PlayResultUI (PlayResultUI.cs)
│       ├── SongInfoPanel
│       │   ├── TitleText
│       │   ├── ArtistText
│       │   ├── DifficultyText
│       │   └── KeyCountText
│       ├── ResultPanel
│       │   ├── RankImage (SSS~F)
│       │   ├── ScoreText
│       │   ├── AccuracyText
│       │   ├── MaxComboText
│       │   ├── FullComboIndicator
│       │   ├── PerfectPlayIndicator
│       │   └── NewRecordIndicator
│       ├── JudgmentStatsPanel
│       │   ├── S_PerfectText
│       │   ├── PerfectText
│       │   ├── GreatText
│       │   ├── GoodText
│       │   ├── BadText
│       │   └── MissText
│       └── ButtonsPanel
│           ├── ButtonRetry
│           ├── ButtonSongSelect
│           └── ButtonMainMenu
├── EventSystem
└── GameResultManager (DontDestroyOnLoad)
```

**설정 체크리스트**:
```
[ ] PlayResultUI 설정
    [ ] 모든 UI 텍스트 참조 연결
    [ ] 등급 이미지 Sprite 할당 (SSS~F)
    [ ] AudioSource 자동 생성 확인

[ ] 애니메이션 설정
    [ ] animateScore: true
    [ ] animateRank: true
    [ ] Score Count Duration: 1.5초
    [ ] Rank Animation Curve: Elastic

[ ] 사운드 이펙트 할당
    [ ] resultDisplaySound
    [ ] rankRevealSound
    [ ] newRecordSound
    [ ] buttonClickSound

[ ] GameResultManager 연동 확인
    [ ] 씬 전환 시 데이터 전달 확인
    [ ] 신기록 체크 시스템
```

**참고 문서**:
- `Assets/playresult/PlayResultUI_Setup.md` (528줄)

---

#### 5. OptionsScene 씬 (옵션 메뉴)
**씬 경로**: `Assets/Scenes/OptionsScene.unity` (TODO - 미구현)

**필수 GameObject 구조** (예정):
```
OptionsScene (씬)
├── Canvas
│   └── OptionMenuUI (OptionMenuUI.cs)
│       ├── TabPanel
│       │   ├── TabAudio
│       │   ├── TabVisual
│       │   ├── TabGameplay
│       │   └── TabControls
│       ├── AudioPanel
│       │   ├── MusicVolumeSlider
│       │   ├── SFXVolumeSlider
│       │   ├── VolumeOffsetSlider
│       │   └── JudgmentOffsetSlider
│       ├── VisualPanel
│       │   ├── NoteSizeSlider
│       │   ├── NoteSpeedSlider
│       │   └── TrackOpacitySlider
│       ├── GameplayPanel
│       │   ├── DefaultModeDropdown
│       │   └── JudgmentDisplayToggle
│       ├── ControlsPanel
│       │   └── KeyRemapButtons
│       └── ButtonsPanel
│           ├── ButtonApply
│           ├── ButtonCancel
│           └── ButtonReset
└── EventSystem
```

**설정 체크리스트** (TODO):
```
[ ] OptionMenuUI 생성 (현재 20% 완성)
[ ] SettingsManager 연동
[ ] 모든 슬라이더/토글 연결
[ ] 실시간 프리뷰 기능
[ ] PlayerPrefs 저장/로드
```

---

### 씬 전환 흐름

```
MainMenu
   ↓ (Normal/Hard/Super 선택)
SongSelection
   ↓ (곡 선택, Enter 키)
GameScene
   ↓ (게임 종료)
ResultScene
   ↓ (버튼 클릭)
   ├→ GameScene (Retry)
   ├→ SongSelection (곡 선택)
   └→ MainMenu (메인 메뉴)
```

**씬 전환 코드**:
```csharp
using UnityEngine.SceneManagement;

// SceneNames.cs 사용 (하드코딩 방지)
SceneManager.LoadScene(SceneNames.GAME);
SceneManager.LoadScene(SceneNames.SONG_SELECTION);
SceneManager.LoadScene(SceneNames.MAIN_MENU);
SceneManager.LoadScene(SceneNames.RESULT);
```

---

### 테스트 체크리스트

#### 씬별 테스트
```
[ ] MainMenu 씬
    [ ] 모드 선택 버튼 작동
    [ ] ESC 메뉴 토글
    [ ] BGM 재생
    [ ] 씬 전환 확인

[ ] SongSelection 씬
    [ ] 곡 목록 표시 (자동 스캔)
    [ ] 키보드 네비게이션 (↑↓←→)
    [ ] 미리듣기 재생
    [ ] 커버 이미지 로딩
    [ ] Enter 키로 게임 시작
    [ ] ESC 메뉴 토글

[ ] GameScene 씬
    [ ] 차트 로딩
    [ ] 노트 스폰
    [ ] 입력 감지
    [ ] 판정 시스템
    [ ] HP 시스템
    [ ] 진행도 바 업데이트 ✨
    [ ] 점수 카운트업 ✨
    [ ] HP 퍼센트 표시 ✨
    [ ] 일시정지 (ESC)
    [ ] 게임 종료 → ResultScene

[ ] ResultScene 씬
    [ ] 결과 데이터 로드
    [ ] 등급 표시
    [ ] 점수 애니메이션
    [ ] 판정 통계
    [ ] 신기록 체크
    [ ] 버튼 작동
```

---

### 필수 Prefab 목록

```
Assets/Prefabs/
├── Note.prefab                    # 노트 오브젝트
├── LongNote.prefab               # 롱노트 오브젝트
├── Track.prefab                  # 트랙 오브젝트
├── JudgmentLine.prefab           # 판정선
├── SongListItem.prefab           # 곡 목록 아이템 (Advanced)
└── ScorePopup.prefab (선택)      # 점수 팝업 (+500)
```

**Prefab 설정**:
```
[ ] Note Prefab
    [ ] SpriteRenderer 컴포넌트
    [ ] NoteController.cs 추가
    [ ] Collider2D (판정용)

[ ] SongListItem Prefab (SongSelectionUIAdvanced용)
    [ ] SongListItem.cs 컴포넌트
    [ ] 모든 UI 요소 연결
    [ ] 뱃지 시스템 (⭐🔒✅🆕)
```

---

### StreamingAssets 폴더 구조

```
Assets/StreamingAssets/
├── Audio/
│   ├── BGM/
│   │   ├── sample_song_1.wav
│   │   ├── sample_song_2.wav
│   │   └── sample_song_3.wav
│   └── KeySounds/
│       ├── kick.wav
│       ├── snare.wav
│       └── hihat.wav
├── Charts/
│   ├── sample_song_1_easy_4k.json
│   ├── sample_song_1_normal_4k.json
│   ├── sample_song_1_hard_6k.json
│   └── ...
├── CoverArt/
│   ├── sample_song_1.png
│   ├── sample_song_2.png
│   └── sample_song_3.png
└── README.md
```

**폴더 생성 방법**:
1. Unity Editor에서 `Tools → Create Sample Charts` 실행
2. 또는 PowerShell:
```powershell
New-Item -ItemType Directory -Path "Assets/StreamingAssets/Audio/BGM" -Force
New-Item -ItemType Directory -Path "Assets/StreamingAssets/Audio/KeySounds" -Force
New-Item -ItemType Directory -Path "Assets/StreamingAssets/Charts" -Force
New-Item -ItemType Directory -Path "Assets/StreamingAssets/CoverArt" -Force
```

---

### TextMeshPro 설정

**필수 설정**:
```
[ ] TextMeshPro 임포트
    [ ] Window → TextMeshPro → Import TMP Essential Resources

[ ] 폰트 에셋 생성 (한글 지원)
    [ ] Window → TextMeshPro → Font Asset Creator
    [ ] 한글 유니코드 범위 추가: AC00-D7A3

[ ] UI 텍스트 교체
    [ ] 모든 Unity Text → TextMeshPro - Text (UI) 교체
```

---

### FMOD 설정 (선택사항)

현재 FMOD 통합 완료. Unity AudioSource로 대체 가능.
