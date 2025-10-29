# Unity 씬 설정 가이드 (Scene Setup Guide)

> **작업 시간**: 약 15-20분  
> **난이도**: 쉬움  
> **목표**: 게임 실행 시 MainMenu부터 시작하도록 씬 구성

---

## 📋 목차
1. [필수 씬 생성](#1-필수-씬-생성)
2. [Build Settings 구성](#2-build-settings-구성)
3. [씬별 GameObject 배치](#3-씬별-gameobject-배치)
4. [테스트](#4-테스트)

---

## 1. 필수 씬 생성

### 현재 상태
- ✅ **MainMenu.unity** - 이미 존재
- ✅ **GameScene.unity** - 이미 존재
- ❌ **SongSelectionScene.unity** - 생성 필요
- ❌ **ResultScene.unity** - 생성 필요
- ❌ **OptionsScene.unity** - 생성 필요 (선택사항)

---

### 1.1 SongSelectionScene 생성

1. **Unity Editor 열기**
   - `Assets/Scenes` 폴더 선택

2. **새 씬 생성**
   - 마우스 우클릭 → `Create → Scene`
   - 이름: `SongSelectionScene`

3. **씬 열기**
   - `SongSelectionScene.unity` 더블클릭

4. **기본 구조 설정**
   ```
   Hierarchy 구조:
   ├── Main Camera (기본 생성됨)
   ├── Canvas (UI 생성 필요)
   ├── EventSystem (UI 생성 시 자동)
   └── SongDatabase (ScriptableObject 또는 GameObject)
   ```

5. **Canvas 생성**
   - Hierarchy 우클릭 → `UI → Canvas`
   - Canvas Inspector 설정:
     - **Render Mode**: Screen Space - Overlay
     - **Canvas Scaler** 컴포넌트 추가:
       - UI Scale Mode: **Scale With Screen Size**
       - Reference Resolution: **1920 x 1080**

6. **SongSelectionUI 스크립트 추가**
   - Canvas에 GameObject 추가 (Hierarchy 우클릭 → `Create Empty`)
   - 이름: `SongSelectionUI`
   - **Add Component** → `SongSelectionUIAdvanced.cs` (또는 `SongSelectionUI.cs`)

7. **씬 저장**
   - `Ctrl+S` 또는 `File → Save`

---

### 1.2 ResultScene 생성

1. **새 씬 생성**
   - `Assets/Scenes` 폴더에서 우클릭 → `Create → Scene`
   - 이름: `ResultScene`

2. **씬 열기**
   - `ResultScene.unity` 더블클릭

3. **기본 구조 설정**
   ```
   Hierarchy 구조:
   ├── Main Camera
   ├── Canvas
   │   └── PlayResultUI (GameObject)
   ├── EventSystem
   └── GameResultManager (DontDestroyOnLoad)
   ```

4. **Canvas 생성 및 설정**
   - Hierarchy 우클릭 → `UI → Canvas`
   - Canvas Scaler: Scale With Screen Size (1920x1080)

5. **PlayResultUI 추가**
   - Canvas 하위에 Empty GameObject 생성
   - 이름: `PlayResultUI`
   - **Add Component** → `PlayResultUI.cs`

6. **GameResultManager 확인**
   - 이미 DontDestroyOnLoad로 존재하는지 확인
   - 없으면 Empty GameObject 생성 후 `GameResultManager.cs` 추가

7. **씬 저장**
   - `Ctrl+S`

---

### 1.3 OptionsScene 생성 (선택사항)

1. **새 씬 생성**
   - `Assets/Scenes` 폴더에서 우클릭 → `Create → Scene`
   - 이름: `OptionsScene`

2. **씬 열기**
   - `OptionsScene.unity` 더블클릭

3. **기본 구조 설정**
   ```
   Hierarchy 구조:
   ├── Main Camera
   ├── Canvas
   │   └── OptionMenuUI (GameObject)
   └── EventSystem
   ```

4. **Canvas 생성 및 설정**
   - Hierarchy 우클릭 → `UI → Canvas`
   - Canvas Scaler: Scale With Screen Size (1920x1080)

5. **OptionMenuUI 추가**
   - Canvas 하위에 Empty GameObject 생성
   - 이름: `OptionMenuUI`
   - **Add Component** → `OptionMenuUI.cs`

6. **씬 저장**
   - `Ctrl+S`

---

## 2. Build Settings 구성

### 2.1 Build Settings 열기

1. Unity Editor 상단 메뉴
   - `File → Build Settings` (단축키: `Ctrl+Shift+B`)

2. Build Settings 창이 열림

---

### 2.2 씬 추가 및 순서 설정

**중요**: 씬의 **순서**가 게임 실행 순서를 결정합니다!

#### 방법 1: 드래그 앤 드롭
1. **Project 창**에서 `Assets/Scenes` 폴더 열기
2. 다음 씬들을 **순서대로** Build Settings 창의 "Scenes In Build" 영역으로 드래그:
   ```
   [0] MainMenu.unity             ← 첫 번째 (게임 시작 지점)
   [1] SongSelectionScene.unity   ← 두 번째
   [2] GameScene.unity            ← 세 번째
   [3] ResultScene.unity          ← 네 번째
   [4] OptionsScene.unity         ← 다섯 번째 (선택사항)
   ```

3. **순서 확인**
   - 각 씬 왼쪽에 [0], [1], [2] 등 인덱스 번호 표시됨
   - **[0] MainMenu.unity**가 최상단에 있어야 함 (가장 중요!)

#### 방법 2: Add Open Scenes
1. Unity Editor에서 각 씬을 **순서대로** 열기
2. 씬을 연 상태에서 `File → Build Settings`
3. `Add Open Scenes` 버튼 클릭
4. 순서 확인 및 조정 (드래그로 순서 변경 가능)

---

### 2.3 씬 순서 확인

**최종 Build Settings 구성**:
```
Scenes In Build:
✅ [0] Scenes/MainMenu           (체크박스 ON)
✅ [1] Scenes/SongSelectionScene (체크박스 ON)
✅ [2] Scenes/GameScene          (체크박스 ON)
✅ [3] Scenes/ResultScene        (체크박스 ON)
✅ [4] Scenes/OptionsScene       (체크박스 ON, 선택사항)
```

**주의사항**:
- ❌ 체크박스가 **꺼진** 씬은 빌드에 포함되지 않음
- ⚠️ **[0] 번 씬이 게임 시작 씬**입니다! 반드시 MainMenu가 0번이어야 함

---

### 2.4 기본 씬 설정 (Editor Play Mode)

Unity Editor에서 Play 버튼을 눌렀을 때 MainMenu부터 시작하도록 설정:

#### 옵션 1: MainMenu 씬 열고 Play
1. `Assets/Scenes/MainMenu.unity` 더블클릭하여 열기
2. Play 버튼 클릭 → MainMenu부터 시작

#### 옵션 2: Enter Play Mode Settings
1. `Edit → Project Settings → Editor`
2. **Enter Play Mode Settings** 섹션 찾기
3. (선택사항) "Enter Play Mode Options" 활성화
   - Reload Domain: OFF (빠른 시작)
   - Reload Scene: OFF (현재 씬 유지)

**참고**: 이 설정은 Editor 전용이며, 빌드된 게임은 항상 [0]번 씬부터 시작합니다.

---

## 3. 씬별 GameObject 배치

각 씬에 필수 컴포넌트를 배치합니다.

### 3.1 MainMenu.unity

**필수 GameObject**:
```
MainMenu (씬)
├── Main Camera
├── Canvas
│   ├── MainMenuUI (MainMenuUI.cs 또는 MainMenuManager.cs)
│   │   ├── Title (Text)
│   │   ├── ButtonNormalMode (Button)
│   │   ├── ButtonHardMode (Button)
│   │   ├── ButtonSuperMode (Button)
│   │   ├── ButtonOptions (Button)
│   │   └── ButtonQuit (Button)
│   └── MainMenuESCMenu (MainMenuESCMenu.cs)
│       ├── Panel (Image - 반투명 배경)
│       ├── ButtonOptions (Button)
│       ├── ButtonCredits (Button)
│       └── ButtonQuit (Button)
├── EventSystem
└── AudioManager (DontDestroyOnLoad)
```

**설정 체크리스트**:
- [ ] MainMenuUI 스크립트의 모든 버튼 참조 연결
- [ ] 버튼 OnClick 이벤트 설정 (SongSelection 씬 로드 등)
- [ ] AudioManager가 DontDestroyOnLoad 설정되어 있는지 확인

---

### 3.2 SongSelectionScene.unity

**필수 GameObject**:
```
SongSelectionScene (씬)
├── Main Camera
├── Canvas
│   ├── SongSelectionUI (SongSelectionUIAdvanced.cs)
│   │   ├── SongInfoPanel (곡 정보 표시)
│   │   ├── ScrollView (곡 목록)
│   │   │   └── Content
│   │   │       └── SongListItem (Prefab)
│   │   ├── SearchInputField (검색)
│   │   ├── SortDropdown (정렬)
│   │   └── FilterPanel (필터)
│   └── SongSelectionESCMenu (SongSelectionESCMenu.cs)
├── EventSystem
├── SongDatabase (ScriptableObject 또는 GameObject)
└── CoverArtLoader (Singleton)
```

**설정 체크리스트**:
- [ ] SongSelectionUIAdvanced 스크립트의 AudioManager 참조
- [ ] SongDatabase ScriptableObject 할당
- [ ] StreamingAssets/Charts 폴더 존재 확인
- [ ] CoverArtLoader 설정 확인

---

### 3.3 GameScene.unity

**필수 GameObject**:
```
GameScene (씬)
├── Main Camera
├── GameManager (GameManager.cs) ⭐ 핵심
│   └── 모든 시스템 참조 연결
├── RhythmManager (Singleton)
├── AudioManager (DontDestroyOnLoad)
├── ChartLoader (Singleton)
├── NoteSpawner (또는 NoteManager)
├── GearController (트랙 관리)
│   ├── Tracks (4K~10K 트랙)
│   └── JudgmentLine
├── HPSystem
│   └── HPBar (UI)
├── UI Canvas
│   ├── ProgressDisplay (ProgressDisplay.cs)
│   ├── ScoreDisplay (ScoreDisplay.cs)
│   ├── ComboJudgmentDisplay
│   ├── JudgmentOffsetDisplay
│   └── PauseMenuUI (PauseMenuUI.cs)
├── InputManager
└── EventSystem
```

**설정 체크리스트**:
- [ ] GameManager의 모든 시스템 참조 연결
- [ ] ProgressDisplay 설정 (Bar Position, Width, Height)
- [ ] ScoreDisplay 설정 (Score Position, Use Comma)
- [ ] HPBarAnimator 설정 (Clear Threshold)
- [ ] GearController의 Track 개수 설정
- [ ] NoteSpawner의 Spawn Offset 설정

**참고 문서**:
- `Assets/Play/GameplayUI_Setup.md`
- `DEVELOPMENT_TODO.md` → Unity 씬 설정 섹션

---

### 3.4 ResultScene.unity

**필수 GameObject**:
```
ResultScene (씬)
├── Main Camera
├── Canvas
│   └── PlayResultUI (PlayResultUI.cs)
│       ├── SongInfoPanel (곡 정보)
│       ├── ResultPanel (점수/등급)
│       │   ├── RankImage (SSS~F)
│       │   ├── ScoreText
│       │   ├── AccuracyText
│       │   ├── MaxComboText
│       │   ├── FullComboIndicator
│       │   ├── PerfectPlayIndicator
│       │   └── NewRecordIndicator
│       ├── JudgmentStatsPanel (판정 통계)
│       └── ButtonsPanel (버튼들)
│           ├── ButtonRetry
│           ├── ButtonSongSelect
│           └── ButtonMainMenu
├── EventSystem
└── GameResultManager (DontDestroyOnLoad)
```

**설정 체크리스트**:
- [ ] PlayResultUI의 모든 UI 텍스트 참조 연결
- [ ] 등급 이미지 Sprite 할당 (SSS~F)
- [ ] 사운드 이펙트 할당
- [ ] 버튼 OnClick 이벤트 설정

**참고 문서**:
- `Assets/playresult/PlayResultUI_Setup.md`

---

### 3.5 OptionsScene.unity

**필수 GameObject**:
```
OptionsScene (씬)
├── Main Camera
├── Canvas
│   └── OptionMenuUI (OptionMenuUI.cs)
│       ├── TabPanel (탭 시스템)
│       ├── AudioPanel (오디오 설정)
│       ├── VisualPanel (비주얼 설정)
│       ├── GameplayPanel (게임플레이 설정)
│       └── ButtonsPanel (저장/취소 버튼)
└── EventSystem
```

**설정 체크리스트**:
- [ ] OptionMenuUI의 모든 슬라이더/토글 연결
- [ ] SettingsManager 연동 확인
- [ ] 버튼 OnClick 이벤트 설정

**참고 문서**:
- `Assets/option/OptionsScene_Setup.md`

---

## 4. 테스트

### 4.1 Unity Editor 테스트

1. **MainMenu 씬 열기**
   - `Assets/Scenes/MainMenu.unity` 더블클릭

2. **Play 버튼 클릭**
   - ✅ MainMenu 화면이 표시되어야 함
   - ✅ BGM이 재생되어야 함 (AudioManager 설정 시)

3. **씬 전환 테스트**
   - MainMenu → SongSelection (모드 선택 버튼)
   - SongSelection → GameScene (곡 선택 후 Enter)
   - GameScene → ResultScene (게임 종료 후)
   - ResultScene → MainMenu (버튼 클릭)

4. **ESC 메뉴 테스트**
   - 각 씬에서 ESC 키 동작 확인

---

### 4.2 Build & Run 테스트

1. **빌드 실행**
   - `File → Build Settings`
   - `Build And Run` 버튼 클릭
   - 저장 위치 선택 (예: `Builds/Synth_v1.0.exe`)

2. **빌드된 게임 실행**
   - ✅ MainMenu부터 시작해야 함
   - ✅ 씬 전환 정상 작동 확인

3. **문제 발생 시**
   - Console 창 확인 (오류 메시지)
   - Build Settings에서 씬 순서 재확인
   - 씬별 필수 GameObject 존재 확인

---

### 4.3 테스트 체크리스트

```
[ ] Unity Editor Play 모드
    [ ] MainMenu 씬이 자동 로드됨
    [ ] 모든 UI 요소 정상 표시
    [ ] 버튼 클릭 동작 확인

[ ] 씬 전환 흐름
    [ ] MainMenu → SongSelection (정상)
    [ ] SongSelection → GameScene (정상)
    [ ] GameScene → ResultScene (정상)
    [ ] ResultScene → MainMenu (정상)

[ ] ESC 메뉴
    [ ] 각 씬에서 ESC 키 동작 확인
    [ ] 일시정지/재개 정상 작동
    [ ] 메뉴 선택 시 씬 전환 확인

[ ] Build & Run
    [ ] 빌드 성공 (오류 없음)
    [ ] 실행 파일 정상 작동
    [ ] MainMenu부터 시작
    [ ] 전체 게임 플로우 정상
```

---

## 5. 씬 전환 코드 예시

각 씬에서 다른 씬으로 전환할 때 사용하는 코드:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionExample : MonoBehaviour
{
    // MainMenu → SongSelection
    public void GoToSongSelection()
    {
        SceneManager.LoadScene("SongSelectionScene");
        // 또는 인덱스 사용: SceneManager.LoadScene(1);
    }

    // SongSelection → GameScene
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        // 또는: SceneManager.LoadScene(2);
    }

    // GameScene → ResultScene
    public void ShowResults()
    {
        SceneManager.LoadScene("ResultScene");
        // 또는: SceneManager.LoadScene(3);
    }

    // Any → MainMenu
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        // 또는: SceneManager.LoadScene(0);
    }

    // 게임 종료
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
```

**권장**: `SceneNames.cs` 상수 클래스 사용 (하드코딩 방지)
```csharp
public static class SceneNames
{
    public const string MAIN_MENU = "MainMenu";
    public const string SONG_SELECTION = "SongSelectionScene";
    public const string GAME = "GameScene";
    public const string RESULT = "ResultScene";
    public const string OPTIONS = "OptionsScene";
}

// 사용 예시
SceneManager.LoadScene(SceneNames.MAIN_MENU);
```

---

## 6. 문제 해결 (Troubleshooting)

### 문제 1: Play 버튼 클릭 시 MainMenu가 아닌 다른 씬 로드
**원인**: Editor에서 마지막으로 열었던 씬이 로드됨

**해결**:
1. `Assets/Scenes/MainMenu.unity` 더블클릭하여 열기
2. 그 상태에서 Play 버튼 클릭

**또는**:
- `Edit → Project Settings → Editor → Enter Play Mode Settings` 확인

---

### 문제 2: Build & Run 시 MainMenu가 아닌 다른 씬부터 시작
**원인**: Build Settings에서 씬 순서가 잘못됨

**해결**:
1. `File → Build Settings` 열기
2. **MainMenu가 [0]번**인지 확인
3. 아니면 드래그로 최상단으로 이동
4. 다시 Build And Run

---

### 문제 3: 씬 전환 시 "Scene 'XXX' couldn't be loaded" 오류
**원인**: Build Settings에 씬이 추가되지 않음

**해결**:
1. `File → Build Settings` 열기
2. 해당 씬을 "Scenes In Build"에 추가
3. 체크박스가 **ON**인지 확인

---

### 문제 4: DontDestroyOnLoad 오브젝트가 중복 생성됨
**원인**: Singleton 패턴 미적용

**해결**:
```csharp
void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject); // 중복 방지
    }
}
```

---

## 7. 다음 단계

씬 설정이 완료되면:

1. **각 씬의 UI 구성** (DEVELOPMENT_TODO.md 참조)
   - MainMenu UI 완성
   - SongSelection UI 완성
   - GameScene UI 완성 (ProgressDisplay, ScoreDisplay 등)
   - ResultScene UI 완성

2. **씬 간 데이터 전달**
   - GameResultManager 활용
   - PlayerPrefs 또는 ScriptableObject 사용

3. **테스트 및 폴리싱**
   - 애니메이션 추가
   - 사운드 이펙트
   - 로딩 화면 (선택사항)

---

## 📚 참고 문서

- **DEVELOPMENT_TODO.md** - Unity 씬 설정 섹션 (라인 200-800)
- **Assets/Play/GameplayUI_Setup.md** - GameScene UI 상세 가이드
- **Assets/playresult/PlayResultUI_Setup.md** - ResultScene UI 상세 가이드
- **Assets/option/OptionsScene_Setup.md** - OptionsScene UI 상세 가이드

---

**작성일**: 2025-10-28  
**버전**: 1.0  
**프로젝트 완성도**: 95% (베타)
