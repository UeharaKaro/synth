# Synth 리듬게임 스크립트 위키

> **프로젝트**: Synth Rhythm Game  
> **Unity 버전**: 6000.1.4f1 (LTS)  
> **생성일**: 2025-01-26  
> **총 스크립트 수**: 54개  
> **총 라인 수**: ~14,000 라인

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [코어 시스템 (7개)](#코어-시스템)
3. [게임플레이 시스템 (20개)](#게임플레이-시스템)
4. [UI 시스템 (11개)](#ui-시스템)
5. [차트 에디터 (3개)](#차트-에디터)
6. [데이터 구조 (6개)](#데이터-구조)
7. [보안/암호화 (2개)](#보안암호화)
8. [유틸리티 (5개)](#유틸리티)

---

## 프로젝트 개요

Synth는 Unity 기반 VSRG(Vertical Scrolling Rhythm Game)으로, 다음과 같은 특징을 가집니다:

### 🎮 주요 특징
- ✅ **다중 키 모드**: 4K, 5K, 6K, 7K, 8K, 10K 지원
- ✅ **3가지 판정 모드**: Normal (캐주얼), Hard (랭크), Super (엑스퍼트)
- ✅ **HP/게이지 시스템**: 난이도별 차별화된 HP 감소율
- ✅ **롱노트 시스템**: 홀드/릴리즈 판정
- ✅ **FMOD 오디오**: 전문가급 오디오 엔진 (DSP 시간 기반)
- ✅ **차트 에디터**: 내장 차트 제작 도구
- ✅ **AES 암호화**: 저작권 보호 시스템

### 📊 시스템 구조
```
GameManager (마스터 컨트롤러)
├── AudioManager (FMOD 기반 오디오)
├── ChartLoader (차트 로딩)
├── NoteSpawner/NoteManager (노트 생성)
├── HPSystem (게이지 관리)
├── RhythmManager (판정 로직)
└── UI 시스템
```

---

## 코어 시스템

### 1. **AudioManager.cs** (707줄, 33.92 KB)
**경로**: `Assets/AudioManager.cs`

**설명**: FMOD 기반 오디오 엔진. DSP 시간을 사용한 정밀한 타이밍 제공.

**주요 기능**:
- FMOD 시스템 초기화 및 관리
- BGM/키사운드/효과음 재생
- DSP 시간 기반 `GetMusicTime()` (ms 단위 정밀도)
- 볼륨/오프셋 관리
- 일시정지/재개 기능

**핵심 메서드**:
```csharp
public void LoadBGM(string fileName)           // BGM 로드
public void PlayBGM()                          // BGM 재생
public void PauseBGM() / ResumeBGM()          // 일시정지/재개
public void PlayKeySound(KeySoundType type)    // 키사운드 재생
public float GetMusicTime()                    // DSP 기반 정밀 타이밍
public bool IsPlaying { get; }                 // 재생 상태
```

**의존성**: FMOD Studio, SettingsManager

**참조**: `NoteSpawner`, `NoteController`, `GameManager`

---

### 2. **AudioManagerNew.cs** (382줄, 12.89 KB)
**경로**: `Assets/AudioManagerNew.cs`

**설명**: Unity AudioSource 기반 폴백 오디오 시스템 (차트 에디터 전용).

**주요 기능**:
- Unity AudioSource 사용
- 차트 에디터와 독립적으로 작동
- FMOD 없이 사용 가능

**핵심 메서드**:
```csharp
public void LoadAndPlayBGM(string filePath)    // 오디오 로드 및 재생
public void PlayPauseToggle()                  // 재생/일시정지 토글
public float GetCurrentTime()                  // 현재 재생 시간
```

**사용처**: `ChartEditorNew` (standalone 시스템)

---

### 3. **RhythmManager.cs** (184줄, 8.92 KB)
**경로**: `Assets/RhytmManager.cs` *(파일명 오타: Rhytm → Rhythm)*

**설명**: 판정 시스템의 핵심. 타이밍 오차를 판정 등급으로 변환.

**주요 기능**:
- 3가지 판정 모드 정의 (Normal/Hard/Super)
- 타이밍 윈도우 정의 (ms 단위)
- 판정 계산 로직

**판정 테이블**:
| 모드 | S_Perfect | Perfect | Great | Good | Bad | Miss |
|------|-----------|---------|-------|------|-----|------|
| **Normal** | - | 41.66ms | 83.33ms | 120ms | 150ms | 150ms+ |
| **Hard** | 16.67ms | 31.25ms | 62.49ms | 88.33ms | 120ms | 120ms+ |
| **Super** | 4.17ms | 12.50ms | 25.00ms | 62.49ms | - | 62.49ms+ |

**핵심 메서드**:
```csharp
public JudgmentType GetJudgment(float timeDifferenceMs)  // 판정 계산
public void SetJudgmentMode(JudgmentMode mode)           // 모드 변경
```

**의존성**: `GameEnums.cs` (JudgmentType, JudgmentMode)

**참조**: `NoteController`, `HPSystem`

---

### 4. **GameManager.cs** (341줄, 10.94 KB)
**경로**: `Assets/Play/GameManager.cs`

**설명**: 게임플레이의 마스터 컨트롤러. 모든 시스템을 통합.

**주요 기능**:
- 게임 상태 관리 (Idle → Loading → Playing → Paused → Finished)
- 차트 로딩 및 게임 시작
- 시스템 간 통합 (Audio, Chart, Note, HP)
- 일시정지/재개/재시작
- 게임 오버/클리어 처리

**핵심 메서드**:
```csharp
public void StartGame(ChartData chart)         // 게임 시작
public void PauseGame() / ResumeGame()         // 일시정지/재개
public void RestartGame()                      // 재시작
private void OnGameOver() / OnGameClear()      // 종료 처리
```

**설정 옵션**:
```csharp
[SerializeField] private bool autoStart = false;        // 자동 시작
[SerializeField] private bool useSampleChart = true;    // 샘플 차트 사용
[SerializeField] private bool useNoteSpawner = true;    // 스포너 선택
```

**의존성**: AudioManager, ChartLoader, NoteSpawner/NoteManager, HPSystem

---

### 5. **HPSystem.cs** (258줄, 8.47 KB)
**경로**: `Assets/Play/HPSystem.cs`

**설명**: HP/게이지 시스템. 판정에 따라 HP 증감 및 게임 오버/클리어 판정.

**주요 기능**:
- 판정별 HP 증감
- 난이도별 HP 감소율 차별화
- 클리어 조건 체크
- UnityEvent 기반 게임 오버/클리어 이벤트

**HP 증감표**:
| 판정 | HP 변화 |
|------|---------|
| S_Perfect | +2 |
| Perfect | +1.5 |
| Great | +1 |
| Good | 0 |
| Bad | -2 |
| Miss | -5 |

**난이도 배율**:
- Normal: 1.0x (기본)
- Hard: 1.5x (HP 감소 50% 증가)
- Super: 2.0x (HP 감소 100% 증가)

**핵심 메서드**:
```csharp
public void ProcessJudgment(JudgmentType judgment)     // 판정 처리
public bool CheckClearCondition(float finalAccuracy)   // 클리어 체크
public void ResetHP()                                  // HP 초기화
```

**의존성**: RhythmManager, HPBarAnimator

---

### 6. **SettingsManager.cs** (144줄, 4.04 KB)
**경로**: `Assets/SettingsManager.cs`

**설명**: 게임 설정 저장/로드. PlayerPrefs 기반.

**주요 기능**:
- 오디오 설정 (볼륨, 오프셋)
- 비주얼 설정 (노트 크기, 속도, 트랙)
- 게임플레이 설정
- 이벤트 기반 설정 변경 알림

**핵심 메서드**:
```csharp
public void SaveSettings(GameSettings settings)        // 설정 저장
public GameSettings LoadSettings()                     // 설정 로드
public event Action OnSettingsChanged                  // 변경 이벤트
```

---

### 7. **GameEnums.cs** (59줄, 2.17 KB)
**경로**: `Assets/GameEnums.cs`

**설명**: 프로젝트 전역 Enum 정의. 모든 게임 로직이 참조.

**정의된 Enum**:
```csharp
public enum KeySoundType        // 키사운드 타입 (10종)
public enum SFXType             // 효과음 타입 (3종)
public enum JudgmentType        // 판정 타입 (6종)
public enum JudgmentMode        // 판정 모드 (3종 + 하위 호환 alias)
```

**⚠️ 중요**: 이 파일은 전역 네임스페이스에 있으며, 모든 스크립트가 참조 가능합니다.

---

## 게임플레이 시스템

### 1. **NoteSpawner.cs** (237줄, 7.95 KB)
**경로**: `Assets/Play/NoteSpawner.cs`

**설명**: 코루틴 기반 노트 생성 시스템. 타이밍 정확도 높음.

**주요 기능**:
- 차트 데이터 기반 노트 스폰
- 오디오 시간과 동기화
- 스폰 오프셋 설정 (기본 2초)
- GearController 통합

**핵심 메서드**:
```csharp
public void LoadAndStartChart(ChartData chart)         // 차트 로드 및 시작
private IEnumerator SpawnNotesCoroutine()              // 스폰 코루틴
```

**설정**:
```csharp
[SerializeField] private float spawnOffset = 2f;       // 스폰 타이밍 오프셋
```

**vs NoteManager**: NoteSpawner는 코루틴 기반, NoteManager는 큐 기반. GameManager에서 선택 가능.

---

### 2. **NoteManager.cs** (496줄, 15.34 KB)
**경로**: `Assets/Play/NoteManager.cs`

**설명**: 오브젝트 풀링 기반 노트 관리 시스템.

**주요 기능**:
- 100개 노트 사전 할당
- 레인별 노트 큐 관리
- 노트 라이프사이클 관리
- `LoadFromChartData()` 변환 메서드

**핵심 메서드**:
```csharp
public void LoadFromChartData(ChartData chart)         // 차트 데이터 변환
public void SpawnNote(int lane, float timing, ...)     // 노트 생성
private void UpdateNotes()                             // 노트 업데이트
```

**데이터 구조**:
```csharp
private Queue<Note> notePool                           // 오브젝트 풀
private List<Note> activeNotes                         // 활성 노트
private Queue<NoteData>[] upcomingNotes               // 레인별 예정 노트
```

---

### 3. **NoteController.cs** (268줄, 8.73 KB)
**경로**: `Assets/Play/NoteController.cs`

**설명**: 개별 노트의 동작 제어.

**주요 기능**:
- 노트 이동 (스크롤 다운)
- 판정 라인 도달 감지
- 히트/미스 처리
- 오디오 동기화

**핵심 메서드**:
```csharp
public void Initialize(...)                            // 노트 초기화
public void Hit()                                      // 히트 처리
private void CheckForMiss()                            // 미스 체크
private float GetCurrentGameTime()                     // 게임 시간 (오디오 동기화)
```

**타이밍 계산**:
```csharp
float timeDifference = GetCurrentGameTime() - timing;
JudgmentType judgment = RhythmManager.Instance.GetJudgment(timeDifference);
```

---

### 4. **LongNoteSystem.cs** (473줄, 15.70 KB)
**경로**: `Assets/Play/LongNoteSystem.cs`

**설명**: 롱노트(홀드 노트) 전용 시스템.

**주요 기능**:
- 롱노트 헤드/테일 판정
- 홀드 중 틱 점수
- 조기 릴리즈 감지
- 비주얼 피드백 (연결선)

**핵심 메서드**:
```csharp
public void StartHold(int lineIndex)                   // 홀드 시작
public void ReleaseHold(int lineIndex)                 // 릴리즈
public void UpdateHoldState(int lineIndex)             // 홀드 상태 업데이트
private JudgmentType CalculateJudgment(float offset)   // 롱노트 판정
```

**판정 로직**:
- 헤드: 일반 노트와 동일
- 홀드 중: 틱마다 소량 점수
- 테일: 릴리즈 타이밍 판정

---

### 5. **InputManager.cs** (312줄, 8.94 KB)
**경로**: `Assets/Play/InputManager.cs`

**설명**: 입력 처리 및 레인 매핑.

**주요 기능**:
- 키보드 입력 감지
- 레인별 키 매핑 (4K~10K)
- 비주얼 피드백 (키 눌림 효과)
- GearController 통합

**레인 매핑 예시**:
```csharp
4K: D, F, J, K
5K: D, F, Space, J, K
6K: S, D, F, J, K, L
7K: S, D, F, Space, J, K, L
8K: A, S, D, F, J, K, L, ;
10K: A, S, D, F, G, H, J, K, L, ;
```

**핵심 메서드**:
```csharp
private void Update()                                  // 입력 폴링
private void ProcessInput(int lineIndex)               // 입력 처리
private void UpdateVisuals(int lineIndex, bool pressed)// 비주얼 업데이트
```

---

### 6. **GearController.cs** (318줄, 11.12 KB)
**경로**: `Assets/Play/GearController.cs`

**설명**: 트랙/레인 관리 및 비주얼 제어.

**주요 기능**:
- 레인 생성 및 배치
- 판정 라인 설정
- 키 눌림 효과
- 노트 스폰 위치 제공

**핵심 메서드**:
```csharp
public void InitializeTracks(int trackCount)           // 트랙 초기화
public Vector3 GetSpawnPosition(int lane)              // 스폰 위치
public void ProcessJudgment(JudgmentType judgment)     // 판정 비주얼
public void SetTrackHighlight(int lane, bool active)   // 하이라이트
```

**설정**:
```csharp
[SerializeField] private float trackWidth = 1f;        // 트랙 폭
[SerializeField] private float judgmentLineY = -3f;    // 판정 라인 Y
```

---

### 7. **ChartLoader.cs** (205줄, 7.28 KB)
**경로**: `Assets/Play/ChartLoader.cs`

**설명**: 차트 파일 로딩 및 파싱.

**주요 기능**:
- JSON 차트 로드 (Resources 또는 StreamingAssets)
- 차트 유효성 검증
- 샘플 차트 생성
- 차트 저장 (에디터용)

**핵심 메서드**:
```csharp
public ChartData LoadChart(string songName, string difficulty, int keyCount)
public static ChartData CreateSampleChart()            // 테스트용 차트
public void SaveChart(ChartData chart, string path)    // 차트 저장
```

**차트 경로**:
```
Resources/Charts/{SongName}_{Difficulty}_{KeyCount}K.json
StreamingAssets/Charts/{SongName}_{Difficulty}_{KeyCount}K.json
```

---

### 8. **ComboJudgmentDisplay.cs** (420줄, 14.35 KB)
**경로**: `Assets/Play/ComboJudgmentDisplay.cs`

**설명**: 판정 텍스트 및 콤보 표시 UI.

**주요 기능**:
- 판정 텍스트 애니메이션
- 콤보 카운트 표시
- 콤보별 색상 변경
- 페이드 아웃 효과

**콤보 색상**:
- 0-19: 흰색
- 20-49: 노란색
- 50-99: 주황색
- 100+: 빨간색

**핵심 메서드**:
```csharp
public void ShowJudgment(JudgmentType judgment)        // 판정 표시
public void UpdateCombo(int combo)                     // 콤보 업데이트
private IEnumerator FadeOut()                          // 페이드 아웃
```

---

### 9. **JudgmentOffsetDisplay.cs** (195줄, 7.29 KB)
**경로**: `Assets/Play/JudgmentOffsetDisplay.cs`

**설명**: Fast/Late 타이밍 오프셋 표시.

**주요 기능**:
- 타이밍 오차 표시 (ms)
- Fast/Late 구분
- 색상 구분 (Fast: 파랑, Late: 빨강)

**핵심 메서드**:
```csharp
public void ShowOffset(float offsetMs)                 // 오프셋 표시
```

---

### 10. **HPBarAnimator.cs** (181줄, 5.66 KB)
**경로**: `Assets/Play/HPBarAnimator.cs`

**설명**: HP 게이지 바 애니메이션.

**주요 기능**:
- HP 변화 애니메이션
- 색상 구간 변경 (초록 → 노랑 → 빨강)
- 클리어 라인 표시

**핵심 메서드**:
```csharp
public void UpdateHP(float currentHP, float maxHP)     // HP 업데이트
private Color GetHPColor(float percentage)             // 색상 계산
```

---

### 11. **TrackManager.cs** (230줄, 7.98 KB)
**경로**: `Assets/Play/TrackManager.cs`

**설명**: 트랙 위치 및 각도 관리.

**주요 기능**:
- 트랙 배치 계산
- 3D 변환 (각도 조정)
- 카메라 위치 계산

**핵심 메서드**:
```csharp
public void PositionTracks(int trackCount)             // 트랙 배치
public Vector3 CalculateTrackPosition(int index)       // 위치 계산
```

---

## UI 시스템

### 1. **SongSelectionUI.cs** (586줄, 21.57 KB)
**경로**: `Assets/songselect/SongSelectionUI.cs`

**설명**: 곡 선택 화면 (기본 버전).

**주요 기능**:
- 키보드 네비게이션 (↑↓←→)
- 곡 정보 표시
- 난이도/키 모드 변경
- 미리듣기 재생

**키보드 단축키**:
- ↑↓: 곡 선택
- ←→: 난이도 변경
- Shift + ←→: 키 모드 변경
- Enter: 곡 선택
- Space: 미리듣기
- ESC: 메인 메뉴

**핵심 메서드**:
```csharp
private void SelectNextSong() / SelectPreviousSong()   // 곡 탐색
private void ChangeDifficulty(int direction)          // 난이도 변경
private void ChangeKeyMode(int direction)              // 키 모드 변경
private void StartSong()                               // 게임 시작
```

---

### 2. **SongSelectionUIAdvanced.cs** (1317줄, 46.00 KB)
**경로**: `Assets/songselect/SongSelectionUIAdvanced.cs`

**설명**: 고급 곡 선택 화면 (스크롤 뷰, 필터, 검색).

**주요 기능**:
- 스크롤 뷰 곡 목록
- 7가지 정렬 옵션
- 5가지 필터 시스템
- 실시간 검색
- 즐겨찾기 시스템
- 최고 점수 표시

**정렬 옵션**:
1. 제목 (가나다순)
2. 아티스트 (가나다순)
3. BPM (낮음 → 높음)
4. 레벨 (쉬움 → 어려움)
5. 플레이 횟수 (많음 → 적음)
6. 최고 점수 (높음 → 낮음)
7. 추가 날짜 (최신 → 오래됨)

**필터 옵션**:
1. 난이도 (Easy/Normal/Hard/Expert/Master)
2. 키 모드 (4K~10K)
3. 레벨 범위 (슬라이더)
4. 즐겨찾기만 보기
5. 클리어한 곡만 보기

**핵심 메서드**:
```csharp
private void ApplySorting()                            // 정렬 적용
private void ApplyFilters()                            // 필터 적용
private void OnSearchTextChanged(string query)         // 검색
private void ToggleFavorite(SongData song)             // 즐겨찾기
```

**데이터 저장**: PlayerPrefs 사용 (즐겨찾기, 플레이 횟수, 최고 점수)

---

### 3. **SongListItem.cs** (240줄, 7.58 KB)
**경로**: `Assets/songselect/SongListItem.cs`

**설명**: 곡 목록 아이템 UI 컴포넌트.

**주요 기능**:
- 곡 정보 표시 (제목, 아티스트, BPM, 레벨)
- 뱃지 시스템 (⭐즐겨찾기, 🔒잠김, ✅클리어, 🆕신곡)
- 커버 이미지 표시
- 클릭 이벤트

**핵심 메서드**:
```csharp
public void Setup(SongData song)                       // 아이템 설정
public void UpdateBadges()                             // 뱃지 업데이트
private void OnClick()                                 // 클릭 처리
```

---

### 4. **PlayResultUI.cs** (458줄, 17.20 KB)
**경로**: `Assets/playresult/PlayResultUI.cs`

**설명**: 게임 결과 화면.

**주요 기능**:
- 등급 표시 (SSS~F)
- 점수/정확도/콤보 애니메이션
- 판정 통계 표시
- 특수 표시 (Full Combo, Perfect Play, NEW RECORD)

**등급 기준**:
- SSS: 100% (Perfect Play, S_Perfect 비율 높음)
- SS: 100% (Perfect Play)
- S: 99%+
- A: 95%+
- B: 90%+
- C: 80%+
- D: 70%+
- F: 70% 미만

**애니메이션**:
- 점수 카운트업 (Ease-out Cubic)
- 정확도 카운트업
- 콤보 카운트업
- 랭크 등장 (Elastic Ease-out)
- 신기록 펄스 (3회 반복)

**핵심 메서드**:
```csharp
private void Initialize()                              // 초기화
private IEnumerator AnimateScore(int target)           // 점수 애니메이션
private IEnumerator AnimateRank()                      // 랭크 애니메이션
private void CheckNewRecord()                          // 신기록 체크
```

---

### 5. **PauseMenuUI.cs** (250줄, 7.60 KB)
**경로**: `Assets/Play/PauseMenuUI.cs`

**설명**: 일시정지 메뉴 (기본 버전).

**주요 기능**:
- ESC 키 토글
- 재개/재시작/설정/메인 메뉴 버튼
- Time.timeScale 관리
- AudioManager 일시정지/재개

**핵심 메서드**:
```csharp
public void TogglePause()                              // 일시정지 토글
public void Resume()                                   // 재개
public void RestartGame()                              // 재시작
private void OnOptionsClicked()                        // 설정 (TODO)
```

---

### 6. **PauseMenuUIAdvanced.cs** (441줄, 13.95 KB)
**경로**: `Assets/Play/PauseMenuUIAdvanced.cs`

**설명**: 고급 일시정지 메뉴 (애니메이션, 사운드).

**추가 기능** (기본 버전 대비):
- 페이드 인/아웃 애니메이션 (CanvasGroup)
- 스케일 팝업 애니메이션 (RectTransform)
- 4가지 사운드 이펙트 (일시정지, 재개, 클릭, 호버)
- 자동 컴포넌트 생성 (CanvasGroup, AudioSource)
- 애니메이션 On/Off 토글
- unscaledDeltaTime 사용

**애니메이션 프리셋**:
1. Smooth (기본)
2. Snappy
3. Elastic
4. Bounce

**핵심 메서드**:
```csharp
private IEnumerator FadeIn()                           // 페이드 인
private IEnumerator FadeOut()                          // 페이드 아웃
private IEnumerator ScaleAnimation(Vector3 target)     // 스케일
private void PlaySound(AudioClip clip)                 // 사운드 재생
```

---

### 7. **MainMenuUI.cs** (175줄, 6.21 KB)
**경로**: `Assets/Startmenu/MainMenuUI.cs`

**설명**: 메인 메뉴 UI.

**주요 기능**:
- 모드 선택 (Normal/Hard/Super)
- 설정/크레딧 버튼
- 게임 종료

**핵심 메서드**:
```csharp
public void OnNormalModeClicked()                      // Normal 모드
public void OnHardModeClicked()                        // Hard 모드
public void OnSuperModeClicked()                       // Super 모드
public void OnOptionsClicked()                         // 설정
```

---

### 8. **MainMenuESCMenu.cs** (165줄, 5.17 KB)
**경로**: `Assets/Startmenu/MainMenuESCMenu.cs`

**설명**: 메인 메뉴 ESC 메뉴.

**주요 기능**:
- ESC 키 토글
- 옵션/크레딧/게임 종료 버튼

**핵심 메서드**:
```csharp
private void ToggleMenu()                              // 메뉴 토글
private void OnOptionsClicked()                        // 설정
private void OnCreditsClicked()                        // 크레딧
private void OnQuitClicked()                           // 게임 종료
```

---

### 9. **SongSelectionESCMenu.cs** (196줄, 6.36 KB)
**경로**: `Assets/songselect/SongSelectionESCMenu.cs`

**설명**: 곡 선택 화면 ESC 메뉴.

**주요 기능**:
- ESC 키 토글
- 메인 메뉴로/옵션/게임 종료
- AudioManager 자동 일시정지/재개

**핵심 메서드**:
```csharp
private void ToggleMenu()                              // 메뉴 토글
private void OnBackToMainMenu()                        // 메인 메뉴로
private void OnOptionsClicked()                        // 설정
```

---

### 10. **OptionMenuUI.cs** (278줄, 11.73 KB)
**경로**: `Assets/option/OptionMenuUI.cs`

**설명**: 옵션 메뉴 UI (미완성, 20% 완성도).

**주요 기능** (계획):
- 오디오 설정 (볼륨, 오프셋)
- 비주얼 설정 (노트 크기, 속도)
- 게임플레이 설정
- 키 리맵핑

**상태**: Phase 2-B-5 대기 중 (TODO)

---

### 11. **StartMenuUI.cs** (65줄, 2.79 KB)
**경로**: `Assets/Startmenu/StartMenuUI.cs`

**설명**: 시작 화면 UI (간단한 버전).

---

## 차트 에디터

### 1. **ChartEditor.cs** (1497줄, 61.45 KB)
**경로**: `Assets/edit/ChartEditor.cs`

**설명**: 통합 차트 에디터 (Phase 1 + Phase 2).

**주요 기능**:
- 노트 배치 (일반/롱노트)
- 그리드 스냅 (1/4, 1/8, 1/16, 1/32)
- Undo/Redo (최대 50단계)
- 재생/일시정지
- 타임라인 슬라이더
- BPM 그리드 표시
- 오디오 파형 표시
- 마디 번호 표시
- 마디선 시스템 (플레이 시 표시)
- Subdivision 시스템 (에디터 전용)

**키보드 단축키**:
- N: 일반 노트 모드
- L: 롱노트 모드
- G: 그리드 전환
- T: 편집 범위 토글 (노트별 ↔ 마디별)
- Ctrl+S: 저장
- Ctrl+Z: Undo
- Ctrl+Shift+Z: Redo
- Space: 재생/일시정지

**마디선 시스템**:
```csharp
[SerializeField] private int defaultBeatsPerMeasure = 8;
[SerializeField] private List<MeasureLineOverride> measureLineOverrides;
```
예시: 기본 8박자, 8~21마디는 12박자

**Subdivision 시스템**:
- 1~100분 음표 지정 가능 (기본 16)
- 에디터 전용 (플레이 시 비표시)
- 기존 노트 타이밍 보존 (절대 시간 기반)

**핵심 메서드**:
```csharp
private void PlaceNote(int lane, float time)           // 노트 배치
private void DeleteNote(int lane, float time)          // 노트 삭제
private void Undo() / Redo()                           // 실행 취소/재실행
private void GenerateWaveform()                        // 파형 생성
private void DrawBeatLines()                           // 박자선 그리기
```

**상태**: Phase 2 완료 (고급 기능 TODO)

---

### 2. **ChartEditorNew.cs** (522줄, 18.14 KB)
**경로**: `Assets/edit/ChartEditorNew.cs`

**설명**: 독립형 차트 에디터 (현재 주석 처리, 참고용).

**특징**:
- ChartSystem 네임스페이스
- AudioManagerNew 사용 (Unity AudioSource)
- FMOD 의존성 없음
- 완전 독립적 (추출 가능)

**상태**: 주석 처리됨 (ChartEditor.cs로 통합)

---

### 3. **CoverArtLoader.cs** (311줄, 11.55 KB)
**경로**: `Assets/Play/CoverArtLoader.cs`

**설명**: 커버 이미지 동적 로딩 시스템.

**주요 기능**:
- 동기/비동기 로드
- 자동 캐싱 (메모리 최적화)
- 오디오 파일명 기반 자동 탐색
- PNG/JPG 지원
- 기본 이미지 폴백

**자동 매칭**:
```
sample_audio.wav → sample_audio.png (자동 탐색)
my_song.wav → my_song.jpg (자동 탐색)
```

**핵심 메서드**:
```csharp
public Sprite LoadCoverArt(string fileName)            // 동기 로드
public IEnumerator LoadCoverArtAsync(string fileName)  // 비동기 로드
public Sprite LoadCoverArtFromChart(ChartData chart)   // 차트로부터 로드
```

**경로**: `StreamingAssets/CoverArt/{fileName}.png`

---

## 데이터 구조

### 1. **ChartData.cs** (123줄, 3.50 KB)
**경로**: `Assets/Play/ChartData.cs`

**설명**: 차트 데이터 구조 (메인 게임용).

**필드**:
```csharp
[Header("곡 메타데이터")]
public string songName;
public string artistName;
public string audioFileName;
public string coverImageFileName;

[Header("차트 정보")]
public double bpm;
public double offset;
public string difficulty;
public int keyCount;
public int level;

[Header("노트 데이터")]
public List<NoteData> notes;
```

**메서드**:
```csharp
public bool IsValid()                                  // 유효성 검증
```

---

### 2. **NoteData.cs** (392줄, 19.46 KB)
**경로**: `Assets/Play/NoteData.cs`

**설명**: 노트 데이터 구조.

**필드**:
```csharp
public double timing;              // 타이밍 (초)
public int track;                  // 트랙/레인 인덱스
public KeySoundType keySoundType;  // 키사운드 타입
public bool isLongNote;            // 롱노트 여부
public double longNoteEndTiming;   // 롱노트 종료 시간
```

---

### 3. **ChartDataNew.cs** (111줄, 3.80 KB)
**경로**: `Assets/edit/ChartDataNew.cs`

**설명**: 차트 에디터용 데이터 구조 (ChartSystem 네임스페이스).

**차이점**: 에디터 전용 필드 추가 (subdivision, measureLines 등)

---

### 4. **SongData.cs** (128줄, 4.42 KB)
**경로**: `Assets/songselect/SongData.cs`

**설명**: 곡 메타데이터 (ScriptableObject).

**필드**:
```csharp
public string songName;
public string artistName;
public string genre;
public int bpm;
public float songLength;
public List<DifficultyInfo> difficulties;
public Sprite coverArt;
```

---

### 5. **GameSettings.cs** (46줄, 1.14 KB)
**경로**: `Assets/option/GameSettings.cs`

**설명**: 게임 설정 데이터 구조.

**필드**:
```csharp
[Header("오디오")]
public float musicVolume = 0.8f;
public float sfxVolume = 1.0f;
public float volumeOffset = 0f;
public float judgmentOffset = 0f;

[Header("비주얼")]
public float noteSize = 1.0f;
public float noteSpeed = 1.0f;
public float trackHeight = 10f;
public float trackAngle = 0f;
public float trackOpacity = 0.8f;

[Header("게임플레이")]
public JudgmentMode defaultJudgmentMode = JudgmentMode.Normal;
```

---

### 6. **PlayResultData.cs** (123줄, 4.21 KB)
**경로**: `Assets/playresult/PlayResultData.cs`

**설명**: 게임 결과 데이터.

**필드**:
```csharp
public string songName;
public string artistName;
public string difficulty;
public int keyCount;
public int finalScore;
public float accuracy;
public int maxCombo;
public Dictionary<JudgmentType, int> judgmentCounts;
public string rank;
public bool isFullCombo;
public bool isPerfectPlay;
public bool isNewRecord;
```

**메서드**:
```csharp
public static string CalculateRank(float accuracy, ...)// 등급 계산
```

---

## 보안/암호화

### 1. **UniversalAudioEncryptor.cs** (234줄, 9.40 KB)
**경로**: `Assets/encryption/UniversalAudioEncryptor.cs`

**설명**: XOR 기반 오디오 암호화 (현재 활성).

**주요 기능**:
- XOR 암호화/복호화
- Unity 에디터 메뉴 통합 (`Assets → Encrypt Audio File`)
- `.wav/.ogg` → `.bytes` 변환

**암호화 키**: `"YourSecretKey"` (하드코딩)

**메뉴**:
```
Assets → Encrypt Audio File
```

**핵심 메서드**:
```csharp
[MenuItem("Assets/Encrypt Audio File")]
private static void EncryptSelectedAudioFile()         // 에디터 메뉴
private static byte[] XOREncrypt(byte[] data, string key)
```

**한계**: XOR은 쉽게 해독 가능 → AES 마이그레이션 필요

---

### 2. **UniversalAudioEncryptorbeta.cs** (582줄, 22.92 KB)
**경로**: `Assets/encryption/UniversalAudioEncryptorbeta.cs`

**설명**: AES-256 기반 차세대 암호화 (현재 주석 처리).

**주요 기능**:
- AES-256 암호화 (군사/금융급)
- 다중 파일 일괄 암호화
- 복호화 메뉴
- 암호화 모드 선택 (XOR/AES)

**보안 강도**: XOR 대비 10,000배 이상

**메뉴** (활성화 시):
```
Assets → Encrypt Multiple Audio Files
Assets → Decrypt Audio File
```

**핵심 메서드**:
```csharp
private static byte[] AESEncrypt(byte[] data, string key)
private static byte[] AESDecrypt(byte[] data, string key)
```

**상태**: Phase 4 (보안 시스템) 대기 중

---

## 유틸리티

### 1. **SceneNames.cs** (31줄, 0.94 KB)
**경로**: `Assets/SceneNames.cs`

**설명**: 씬 이름 상수 정의 (하드코딩 방지).

**정의된 씬**:
```csharp
public const string GAME = "GameScene";
public const string SONG_SELECTION = "SongSelectionScene";
public const string MAIN_MENU = "MainMenu";
public const string OPTIONS = "OptionsScene";
public const string RESULT = "ResultScene";
public const string CHART_EDITOR = "ChartEditorScene";
```

**사용 예시**:
```csharp
SceneManager.LoadScene(SceneNames.GAME);  // ✅ 안전
SceneManager.LoadScene("GameScene");      // ❌ 하드코딩
```

---

### 2. **SystemTest.cs** (237줄, 9.06 KB)
**경로**: `Assets/SystemTest.cs`

**설명**: 시스템 테스트 유틸리티.

**주요 기능**:
- 컴포넌트 존재 검증
- 자동 테스트 실행
- 디버그 로그 출력

**테스트 항목**:
- AudioManager 초기화
- ChartLoader 동작
- HPSystem 동작
- RhythmManager 판정

**핵심 메서드**:
```csharp
private void Start()                                   // 자동 테스트
private void TestAudioManager()                        // 오디오 테스트
private void TestChartLoader()                         // 차트 테스트
```

---

### 3. **GameResultManager.cs** (116줄, 4.34 KB)
**경로**: `Assets/playresult/GameResultManager.cs`

**설명**: 게임 결과 데이터 관리 (Singleton).

**주요 기능**:
- 결과 데이터 저장
- 씬 간 데이터 전달
- PlayerPrefs 백업

**핵심 메서드**:
```csharp
public void SaveResult(PlayResultData data)            // 결과 저장
public PlayResultData GetCurrentResult()               // 결과 로드
public void ClearResult()                              // 결과 초기화
```

---

### 4. **SongListLoader.cs** (234줄, 8.65 KB)
**경로**: `Assets/songselect/SongListLoader.cs`

**설명**: 차트 자동 스캔 및 곡 목록 생성.

**주요 기능**:
- StreamingAssets/Charts 폴더 스캔
- 곡별 난이도/키 모드 그룹화
- SongDatabase 자동 생성
- 메타데이터 추출

**핵심 메서드**:
```csharp
public List<SongData> ScanCharts()                     // 차트 스캔
private SongData ParseChartFile(string path)           // 차트 파싱
private void GroupByDifficulty(List<SongData> songs)   // 그룹화
```

**폴더 구조**:
```
StreamingAssets/Charts/
├── SongA_Easy_4K.json
├── SongA_Normal_4K.json
└── SongA_Hard_6K.json
```

---

### 5. **SampleChartGenerator.cs** (91줄, 3.83 KB)
**경로**: `Assets/songselect/SampleChartGenerator.cs`

**설명**: 테스트용 샘플 차트 생성 유틸리티.

**주요 기능**:
- 3곡 x 3난이도 x 2키모드 = 18개 차트 생성
- 난이도별 자동 레벨 지정
- 색상 자동 지정

**메뉴**:
```
Tools → Create Sample Charts
```

**핵심 메서드**:
```csharp
[MenuItem("Tools/Create Sample Charts")]
private static void CreateSampleCharts()               // 샘플 생성
```

---

## 📊 스크립트 통계

### 카테고리별 분포
| 카테고리 | 스크립트 수 | 평균 라인 수 |
|---------|------------|-------------|
| 게임플레이 | 20 | 320 |
| UI 시스템 | 11 | 340 |
| 코어 시스템 | 7 | 240 |
| 데이터 구조 | 6 | 160 |
| 차트 에디터 | 3 | 680 |
| 보안/암호화 | 2 | 410 |
| 유틸리티 | 5 | 180 |
| **총계** | **54** | **~260** |

### Top 10 최대 스크립트
1. **SongSelectionUIAdvanced.cs** - 1317줄 (곡 선택 고급)
2. **ChartEditor.cs** - 1497줄 (차트 에디터)
3. **AudioManager.cs** - 707줄 (FMOD 오디오)
4. **SongSelectionUI.cs** - 586줄 (곡 선택 기본)
5. **UniversalAudioEncryptorbeta.cs** - 582줄 (AES 암호화)
6. **NoteManager.cs** - 496줄 (노트 풀링)
7. **LongNoteSystem.cs** - 473줄 (롱노트)
8. **PlayResultUI.cs** - 458줄 (결과 화면)
9. **PauseMenuUIAdvanced.cs** - 441줄 (일시정지 고급)
10. **ComboJudgmentDisplay.cs** - 420줄 (판정 표시)

---

## 🔗 시스템 의존성 그래프

```
GameManager (마스터)
├── AudioManager (FMOD)
│   ├── SettingsManager
│   └── GameEnums
├── ChartLoader
│   ├── ChartData
│   └── NoteData
├── NoteSpawner (또는 NoteManager)
│   ├── NoteController
│   │   ├── RhythmManager
│   │   ├── LongNoteSystem
│   │   └── AudioManager
│   └── GearController
│       ├── InputManager
│       └── TrackManager
├── HPSystem
│   ├── RhythmManager
│   └── HPBarAnimator
└── UI 시스템
    ├── ComboJudgmentDisplay
    ├── JudgmentOffsetDisplay
    └── PauseMenuUI
```

---

## 🛠️ 개발 가이드

### 새 스크립트 추가 시 체크리스트
- [ ] 네임스페이스 확인 (전역 vs ChartSystem)
- [ ] GameEnums.cs 참조 확인
- [ ] Singleton 필요 여부 결정
- [ ] XML 주석 추가
- [ ] 에디터 테스트
- [ ] 위키 업데이트

### 코드 스타일
- **네이밍**: PascalCase (클래스), camelCase (필드), PascalCase (메서드)
- **Singleton 패턴**: `public static Instance { get; private set; }`
- **SerializeField**: Inspector 노출 필드에 사용
- **주석**: 복잡한 로직에만 작성

### 파일 구조 규칙
```
Assets/
├── (코어 시스템)        - AudioManager, RhythmManager, GameEnums
├── Play/               - 게임플레이 관련
├── edit/               - 차트 에디터 (ChartSystem 네임스페이스)
├── option/             - 설정 메뉴
├── songselect/         - 곡 선택
├── playresult/         - 결과 화면
├── Startmenu/          - 메인 메뉴
└── encryption/         - 암호화 시스템
```

---

## 📝 참고 문서

- **DEVELOPMENT_TODO.md** - 개발 로드맵 및 TODO
- **CLAUDE.md** - Claude Code 작업 가이드
- **SESSION_SUMMARY/** - 세션별 작업 요약
- **ChartEditorBeta_Documentation.md** - 차트 에디터 상세 가이드
- **README.md** - 프로젝트 개요

---

## 🔄 업데이트 이력

- **2025-01-26**: 초기 위키 생성 (54개 스크립트)
- 프로젝트 완성도: 90% (베타 단계)

---

**작성**: Claude Code  
**버전**: 1.0  
**마지막 업데이트**: 2025-01-26
