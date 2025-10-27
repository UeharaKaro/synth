# OptionsScene 설정 가이드

> **Phase 2-B-5**: 옵션 메뉴 UI 구현
> **작성일**: 2025-10-27
> **업데이트 내역**: 초안 작성

---

## 📋 목차

1. [개요](#개요)
2. [씬 구조](#씬-구조)
3. [GameObject 설정](#gameobject-설정)
4. [UI 레이아웃](#ui-레이아웃)
5. [컴포넌트 설정](#컴포넌트-설정)
6. [테스트 가이드](#테스트-가이드)

---

## 개요

**OptionsScene**은 게임 설정을 조절할 수 있는 화면입니다.

### 주요 기능
- ✅ 탭 시스템 (오디오/비주얼/게임플레이)
- ✅ 오디오 설정 (음악/효과음 볼륨, 오프셋)
- ✅ 비주얼 설정 (노트 크기, 트랙 설정)
- ✅ 게임플레이 설정 (판정 모드, 표시 옵션)
- ✅ 실시간 AudioManager 연동
- ✅ SettingsManager 통합 (JSON 저장)

### 관련 파일
- `Assets/option/OptionMenuUI.cs` (630줄) - UI 관리 스크립트
- `Assets/option/GameSettings.cs` (68줄) - 설정 데이터 구조
- `Assets/SettingsManager.cs` (174줄) - 설정 저장/로드
- `Assets/Scenes/OptionsScene.unity` (생성 필요)

---

## 씬 구조

### 필수 GameObject 트리

```
OptionsScene (씬)
├── Canvas
│   ├── SettingsManager (DontDestroyOnLoad)
│   ├── OptionMenuUI (OptionMenuUI.cs)
│   │   ├── TabPanel
│   │   │   ├── AudioTabButton
│   │   │   ├── VisualTabButton
│   │   │   └── GameplayTabButton
│   │   ├── AudioPanel
│   │   │   ├── MusicVolumeSlider
│   │   │   ├── MusicVolumeText
│   │   │   ├── SFXVolumeSlider
│   │   │   ├── SFXVolumeText
│   │   │   ├── VolumeOffsetSlider
│   │   │   ├── VolumeOffsetText
│   │   │   ├── JudgmentOffsetSlider
│   │   │   └── JudgmentOffsetText
│   │   ├── VisualPanel
│   │   │   ├── NoteSizeSlider
│   │   │   ├── NoteSizeText
│   │   │   ├── TrackHeightSlider
│   │   │   ├── TrackHeightText
│   │   │   ├── TrackAngleSlider
│   │   │   ├── TrackAngleText
│   │   │   ├── TrackOpacitySlider
│   │   │   ├── TrackOpacityText
│   │   │   ├── NoteScrollSpeedSlider
│   │   │   └── NoteScrollSpeedText
│   │   ├── GameplayPanel
│   │   │   ├── JudgmentModeDropdown
│   │   │   ├── ShowJudgmentToggle
│   │   │   └── ShowOffsetToggle
│   │   └── ButtonPanel
│   │       ├── ApplyButton
│   │       ├── ResetButton
│   │       └── BackButton
└── EventSystem
```

---

## GameObject 설정

### 1. Canvas 설정

```
Canvas (GameObject)
├── Component: Canvas
│   ├── Render Mode: Screen Space - Overlay
│   └── Pixel Perfect: ✅
├── Component: CanvasScaler
│   ├── UI Scale Mode: Scale With Screen Size
│   ├── Reference Resolution: 1920 x 1080
│   ├── Screen Match Mode: Match Width Or Height
│   └── Match: 0.5
└── Component: GraphicRaycaster
```

### 2. SettingsManager 설정

```
SettingsManager (GameObject)
├── Component: SettingsManager (MonoBehaviour)
│   └── Settings: (자동 생성)
└── DontDestroyOnLoad: ✅ (코드에서 자동 설정)
```

**중요**: SettingsManager는 씬이 전환되어도 유지되어야 합니다.

### 3. OptionMenuUI 설정

```
OptionMenuUI (GameObject)
├── Component: OptionMenuUI (MonoBehaviour)
│   ├── [탭 시스템]
│   │   ├── Audio Panel: → AudioPanel
│   │   ├── Visual Panel: → VisualPanel
│   │   ├── Gameplay Panel: → GameplayPanel
│   │   ├── Audio Tab Button: → AudioTabButton
│   │   ├── Visual Tab Button: → VisualTabButton
│   │   └── Gameplay Tab Button: → GameplayTabButton
│   ├── [오디오 설정]
│   │   ├── Music Volume Slider: → MusicVolumeSlider
│   │   ├── Music Volume Text: → MusicVolumeText
│   │   ├── SFX Volume Slider: → SFXVolumeSlider
│   │   ├── SFX Volume Text: → SFXVolumeText
│   │   ├── Volume Offset Slider: → VolumeOffsetSlider
│   │   ├── Volume Offset Text: → VolumeOffsetText
│   │   ├── Judgment Offset Slider: → JudgmentOffsetSlider
│   │   └── Judgment Offset Text: → JudgmentOffsetText
│   ├── [비주얼 설정]
│   │   ├── Note Size Slider: → NoteSizeSlider
│   │   ├── Note Size Text: → NoteSizeText
│   │   ├── Track Height Slider: → TrackHeightSlider
│   │   ├── Track Height Text: → TrackHeightText
│   │   ├── Track Angle Slider: → TrackAngleSlider
│   │   ├── Track Angle Text: → TrackAngleText
│   │   ├── Track Opacity Slider: → TrackOpacitySlider
│   │   ├── Track Opacity Text: → TrackOpacityText
│   │   ├── Note Scroll Speed Slider: → NoteScrollSpeedSlider
│   │   └── Note Scroll Speed Text: → NoteScrollSpeedText
│   ├── [게임플레이 설정]
│   │   ├── Judgment Mode Dropdown: → JudgmentModeDropdown
│   │   ├── Show Judgment Toggle: → ShowJudgmentToggle
│   │   └── Show Offset Toggle: → ShowOffsetToggle
│   ├── [버튼]
│   │   ├── Apply Button: → ApplyButton
│   │   ├── Reset Button: → ResetButton
│   │   └── Back Button: → BackButton
│   ├── [씬 설정]
│   │   └── Previous Scene Name: "MainMenu"
│   └── [실시간 프리뷰]
│       └── Enable Realtime Preview: ✅
```

---

## UI 레이아웃

### 1. 탭 시스템

**TabPanel** (상단):
```
Position: (0, 450, 0)
Size: (1920, 80)

[AudioTab] [VisualTab] [GameplayTab]
   ↑ 선택됨      보통         보통
```

**선택된 탭 색상**: `RGB(255, 255, 255)`
**일반 탭 색상**: `RGB(179, 179, 179)`

### 2. 오디오 패널 레이아웃

```
AudioPanel (중앙)
├── 음악 볼륨: [======|====] 0.80
├── 효과음 볼륨: [======|====] 0.80
├── 볼륨 오프셋: [====|======] 0ms
└── 판정 오프셋: [====|======] 0ms
```

**슬라이더 설정**:
- Width: 400px
- Height: 20px
- Handle Size: 30px

### 3. 비주얼 패널 레이아웃

```
VisualPanel (중앙)
├── 노트 크기: [====|======] 1.00
├── 트랙 높이: [======|====] 15.0
├── 트랙 각도: [====|======] 0.0°
├── 트랙 투명도: [======|====] 0.80
└── 노트 속도: [======|====] 8.0
```

### 4. 게임플레이 패널 레이아웃

```
GameplayPanel (중앙)
├── 판정 모드: [Normal (일반) ▼]
├── 판정 표시: [✓] 판정 텍스트 표시
└── 오프셋 표시: [✓] 타이밍 오프셋 표시
```

### 5. 버튼 패널 레이아웃

```
ButtonPanel (하단)
Position: (0, -450, 0)

[적용]    [초기화]    [뒤로 가기]
```

---

## 컴포넌트 설정

### 슬라이더 설정 (Slider Component)

#### 음악 볼륨 슬라이더
```
Min Value: 0
Max Value: 1
Whole Numbers: ❌
Value: 0.8
```

#### 효과음 볼륨 슬라이더
```
Min Value: 0
Max Value: 1
Whole Numbers: ❌
Value: 0.8
```

#### 볼륨 오프셋 슬라이더
```
Min Value: -200
Max Value: 200
Whole Numbers: ❌
Value: 0
```

#### 판정 오프셋 슬라이더
```
Min Value: -200
Max Value: 200
Whole Numbers: ❌
Value: 0
```

#### 노트 크기 슬라이더
```
Min Value: 0.5
Max Value: 3.0
Whole Numbers: ❌
Value: 1.0
```

#### 트랙 높이 슬라이더
```
Min Value: 5
Max Value: 30
Whole Numbers: ❌
Value: 15
```

#### 트랙 각도 슬라이더
```
Min Value: -45
Max Value: 45
Whole Numbers: ❌
Value: 0
```

#### 트랙 투명도 슬라이더
```
Min Value: 0.1
Max Value: 1.0
Whole Numbers: ❌
Value: 0.8
```

#### 노트 스크롤 속도 슬라이더
```
Min Value: 1
Max Value: 20
Whole Numbers: ❌
Value: 8
```

### 드롭다운 설정 (TMP_Dropdown)

#### 판정 모드 드롭다운
```
Options:
  0: Normal (일반)
  1: Hard (어려움)
  2: Super (최고난이도)

Value: 0 (Normal)
```

**코드에서 자동 설정됨** - Inspector에서 수동 설정 불필요

### 토글 설정 (Toggle Component)

#### 판정 표시 토글
```
Is On: ✅
Label: "판정 텍스트 표시 (Perfect, Great 등)"
```

#### 오프셋 표시 토글
```
Is On: ✅
Label: "타이밍 오프셋 표시 (+3ms, -5ms 등)"
```

### 버튼 설정 (Button Component)

#### 적용 버튼
```
Text: "적용"
OnClick: OptionMenuUI.OnApplyButtonClicked()
Color: RGB(100, 200, 100) - 초록색
```

#### 초기화 버튼
```
Text: "초기화"
OnClick: OptionMenuUI.OnResetButtonClicked()
Color: RGB(200, 100, 100) - 빨간색
```

#### 뒤로 가기 버튼
```
Text: "뒤로 가기"
OnClick: OptionMenuUI.OnBackButtonClicked()
Color: RGB(150, 150, 150) - 회색
```

---

## 테스트 가이드

### 1. 씬 로딩 테스트

```
[ ] OptionsScene 로드 성공
[ ] SettingsManager 자동 생성
[ ] 기존 설정 불러오기 (PlayerPrefs)
[ ] 기본값 설정 (설정 없을 시)
```

### 2. 탭 시스템 테스트

```
[ ] 오디오 탭 클릭 → 오디오 패널 표시
[ ] 비주얼 탭 클릭 → 비주얼 패널 표시
[ ] 게임플레이 탭 클릭 → 게임플레이 패널 표시
[ ] 탭 색상 변경 (선택됨/보통)
```

### 3. 슬라이더 테스트

**오디오 설정**:
```
[ ] 음악 볼륨 변경 → AudioManager 즉시 반영 (실시간 프리뷰)
[ ] 효과음 볼륨 변경 → AudioManager 즉시 반영
[ ] 볼륨 오프셋 변경 → 텍스트 업데이트
[ ] 판정 오프셋 변경 → 텍스트 업데이트
```

**비주얼 설정**:
```
[ ] 노트 크기 변경 → 텍스트 업데이트
[ ] 트랙 높이 변경 → 텍스트 업데이트
[ ] 트랙 각도 변경 → 텍스트 업데이트 (°표시)
[ ] 트랙 투명도 변경 → 텍스트 업데이트
[ ] 노트 속도 변경 → 텍스트 업데이트
```

### 4. 게임플레이 설정 테스트

```
[ ] 판정 모드 드롭다운 작동
[ ] Normal/Hard/Super 선택 가능
[ ] 판정 표시 토글 작동
[ ] 오프셋 표시 토글 작동
```

### 5. 버튼 테스트

```
[ ] 적용 버튼 → SettingsManager.SaveSettings() 호출
[ ] 적용 버튼 → PlayerPrefs 저장 확인
[ ] 초기화 버튼 → 모든 설정 기본값으로 리셋
[ ] 초기화 버튼 → UI 즉시 업데이트
[ ] 뒤로 가기 → MainMenu 씬으로 전환
```

### 6. 저장/로드 테스트

```
[ ] 설정 변경 후 적용 → 씬 재로드 → 설정 유지됨
[ ] 초기화 → 씬 재로드 → 기본값 유지됨
[ ] PlayerPrefs 데이터 확인:
    - Registry (Windows): HKEY_CURRENT_USER\Software\[CompanyName]\[ProductName]
    - 또는 PlayerPrefs 삭제 후 테스트
```

### 7. 통합 테스트

```
[ ] OptionsScene → GameScene 전환 → 설정 적용됨
[ ] GameScene → OptionsScene → 현재 설정 표시됨
[ ] MainMenu → OptionsScene → 뒤로 가기 정상 작동
```

---

## 문제 해결 (Troubleshooting)

### 문제 1: SettingsManager를 찾을 수 없음

**증상**:
```
SettingsManager를 찾을 수 없습니다! GameObject에 SettingsManager 추가 필요
```

**해결**:
1. Hierarchy에서 빈 GameObject 생성 (`GameObject → Create Empty`)
2. 이름을 "SettingsManager"로 변경
3. SettingsManager.cs 컴포넌트 추가
4. DontDestroyOnLoad 확인 (코드에서 자동 설정)

### 문제 2: AudioManager 실시간 프리뷰 비활성화

**증상**:
```
AudioManager를 찾을 수 없습니다. 실시간 오디오 프리뷰가 비활성화됩니다.
```

**해결**:
- 정상 동작 (AudioManager는 게임플레이 씬에서만 존재)
- 옵션: `enableRealtimePreview = false` 설정 (Inspector)

### 문제 3: 슬라이더 값이 저장되지 않음

**원인**: 적용 버튼을 누르지 않음

**해결**:
- 슬라이더 변경 후 **반드시 "적용" 버튼 클릭**
- 또는 자동 저장 기능 구현 (슬라이더 변경 시 즉시 저장)

### 문제 4: 씬 전환 후 설정 초기화됨

**원인**: SettingsManager가 DontDestroyOnLoad로 설정되지 않음

**해결**:
1. SettingsManager.cs의 Awake() 확인:
   ```csharp
   DontDestroyOnLoad(gameObject);
   ```
2. Hierarchy에서 SettingsManager가 씬 루트에 있는지 확인

---

## 추가 개선 사항 (선택)

### 1. 자동 저장 기능

슬라이더 변경 시 자동 저장:

```csharp
private void OnMusicVolumeChanged(float value)
{
    if (settingsManager != null)
    {
        settingsManager.SetMusicVolume(value);
        settingsManager.SaveSettings(); // 자동 저장
    }
    // ...
}
```

### 2. 변경 사항 확인 다이얼로그

뒤로 가기 시 변경 사항 경고:

```csharp
public void OnBackButtonClicked()
{
    if (HasUnsavedChanges())
    {
        // "변경 사항이 있습니다. 저장하시겠습니까?" 다이얼로그 표시
    }
    else
    {
        SceneManager.LoadScene(previousSceneName);
    }
}
```

### 3. 키보드 단축키

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        OnBackButtonClicked();
    }

    if (Input.GetKeyDown(KeyCode.Return))
    {
        OnApplyButtonClicked();
    }
}
```

### 4. 설정 프리셋 시스템

```csharp
public void LoadPreset(string presetName)
{
    // "Casual", "Normal", "Pro" 프리셋
}
```

---

## 요약

### 체크리스트

```
[X] OptionMenuUI.cs 구현 (630줄)
[X] GameSettings.cs 업데이트 (sfxVolume, 게임플레이 설정)
[X] SettingsManager.cs 업데이트 (SetSFXVolume 등)
[ ] OptionsScene.unity 생성
[ ] Canvas 및 GameObject 트리 구성
[ ] OptionMenuUI 컴포넌트 참조 연결
[ ] 슬라이더/드롭다운/토글 설정
[ ] 버튼 OnClick 이벤트 설정
[ ] 테스트 실행
```

### 예상 작업 시간

- 씬 생성 및 GameObject 구성: 1시간
- UI 레이아웃 및 디자인: 1.5시간
- 컴포넌트 연결 및 설정: 1시간
- 테스트 및 버그 수정: 30분
- **총 예상 시간**: 4시간

---

**문서 버전**: 1.0
**최종 수정**: 2025-10-27
