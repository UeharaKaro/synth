# ChartEditor - Unity 리듬 게임 채보 에디터

## 개요

ChartEditor는 DEVELOPMENT_TODO.md의 요구사항을 기반으로 구축된 Unity 리듬 게임용 통합 채보 편집 도구입니다. 향상된 사용자 경험과 고급 기능을 통해 완전한 채보 생성 및 편집 기능을 제공합니다.

**위치**: `Assets/edit/ChartEditor.cs`
**네임스페이스**: `ChartSystem`
**상태**: Phase 1 완료

## 주요 기능

### Phase 1 기능 (완료)
- ✅ **다중 레인 지원**: 4K, 5K, 6K, 7K, 8K, 10K 구성
- ✅ **노트 타입 전환**: Normal (N 키) / Long (L 키) 모드
- ✅ **양방향 롱노트**: 위에서 아래 또는 아래에서 위로 배치 가능
- ✅ **그리드 스냅 시스템**: G 키로 1/4, 1/8, 1/16, 1/32, OFF 순환
- ✅ **실행 취소/다시 실행**: 최대 50단계 (Ctrl+Z, Ctrl+Shift+Z)
- ✅ **키보드 단축키**: Ctrl+S (저장), Space (재생/일시정지), T (편집 범위 전환)
- ✅ **오디오 컨트롤**: 로드, 재생, 일시정지, 정지, 탐색
- ✅ **차트 관리**: 로드, 저장, 새로 만들기

### Phase 2 기능 (계획됨)
- ⏳ 파형이 있는 비주얼 타임라인
- ⏳ 고급 편집 (복사, 붙여넣기, 미러)
- ⏳ BPM 변경 지원
- ⏳ 슬라이드 노트
- ⏳ 다중 선택 및 일괄 작업

## 에디터 컨트롤

### 모드 전환
| 키 | 기능 | 설명 |
|-----|----------|-------------|
| **N** | 일반 노트 모드 | 표준 노트 배치 |
| **L** | 롱노트 모드 | 롱노트(홀드 노트) 배치 |
| **S** | 슬라이드 노트 모드 | ⏳ Phase 2에서 제공 예정 |

### 노트 배치

#### 일반 노트
1. **N** 키를 눌러 일반 노트 모드로 진입
2. 트랙을 클릭하여 노트 배치
3. 그리드 스냅이 활성화되어 있으면 노트가 그리드에 맞춰짐

#### 롱노트 (양방향)
1. **L** 키를 눌러 롱노트 모드로 진입
2. 시작 위치 클릭 (위쪽 또는 아래쪽 모두 가능)
3. 끝 위치 클릭 (시작점 위 또는 아래 모두 가능)
4. 시스템이 자동으로 시작/끝 타이밍 결정

**예시:**
```
위에서 아래:         아래에서 위:
   [클릭 1] ●        [클릭 2] ●
      |                   |
      ↓                   ↑
   [클릭 2] ●        [클릭 1] ●
```

### 그리드 스냅

**G 키**: 그리드 스냅 모드 순환
- **1/4 박자** → **1/8 박자** → **1/16 박자** → **1/32 박자** → **OFF** → (반복)

**비트 분할:**
- `[` 키: 비트 분할 감소
- `]` 키: 비트 분할 증가

그리드 스냅은 다음을 기반으로 노트 타이밍 계산:
```csharp
double beatInterval = 60.0 / bpm;  // 박자당 초
double snapInterval = beatInterval / (int)currentBeatDivision;
```

### 오디오 컨트롤

| 키/버튼 | 기능 |
|------------|----------|
| **Space** | 재생/일시정지 전환 |
| **정지 버튼** | 재생 정지 및 시작 지점으로 리셋 |
| **타임라인 슬라이더** | 특정 시간으로 탐색 |

### 편집 기능

| 단축키 | 기능 | 설명 |
|----------|----------|-------------|
| **Ctrl + Z** | 실행 취소 | 마지막 작업 취소 (최대 50단계) |
| **Ctrl + Shift + Z** | 다시 실행 | 취소한 작업 다시 실행 |
| **Ctrl + S** | 저장 | 현재 차트 저장 |
| **T** | 편집 범위 전환 | 노트별 / 마디별 전환 |
| **Delete** | 노트 삭제 | 선택한 노트 삭제 |

### 편집 범위 (T 키)

**노트별 모드**: 개별 노트에 변경사항 적용
**마디별 모드**: 마디 범위의 노트에 변경사항 적용

이 기능은 향후 BPM/템포 변경 기능을 위해 준비되었습니다.

## 설정 지침

### 1. 기본 설정

1. 씬에 빈 GameObject 생성
2. `ChartEditor` 컴포넌트 추가 (`ChartSystem` 네임스페이스에서)
3. Inspector에서 public 필드 구성

### 2. 필수 UI 컴포넌트

```csharp
[Header("오디오 컨트롤 UI")]
public InputField audioPathInputField;    // 오디오 파일 경로
public Slider timelineSlider;             // 타임라인 탐색 슬라이더
public Text currentTimeText;              // 현재 시간 표시
public Text totalTimeText;                // 총 길이 표시
public Button loadAudioButton;            // 오디오 로드 버튼
public Button playButton;                 // 재생 버튼
public Button pauseButton;                // 일시정지 버튼
public Button stopButton;                 // 정지 버튼

[Header("차트 정보 UI")]
public InputField songNameInput;          // 곡명 입력
public InputField artistNameInput;        // 아티스트명 입력
public InputField bpmInput;               // BPM 입력
public InputField offsetInput;            // 오디오 오프셋 (ms)

[Header("에디터 상태 UI")]
public Text modeText;                     // 현재 모드 표시
public Text gridSnapText;                 // 그리드 스냅 표시
public Text statusText;                   // 상태 메시지
```

### 3. 차트 설정

```csharp
[Header("차트 설정")]
public string songName = "";              // 곡 제목
public string artistName = "";            // 아티스트명
public float bpm = 120f;                  // 분당 박자
public float offset = 0f;                 // 오디오 오프셋 (초)

[Header("에디터 설정")]
public int keyCount = 4;                  // 레인 개수 (4/5/6/7/8/10)
public KeyCode[] trackKeys;               // 각 레인의 입력 키
public Transform[] noteSpawnPoints;       // 레인별 스폰 위치
public GameObject notePrefab;             // 노트 프리팹
```

### 4. 프리팹 요구사항

**노트 프리팹:**
- `SpriteRenderer` 컴포넌트 필수
- 커스텀 비주얼 가능
- 성능을 위해 풀링됨

**자동 생성:**
`notePrefab`이 null이면 에디터가 자동으로 간단한 흰색 사각형 스프라이트를 생성합니다.

## 기술 세부사항

### 실행 취소/다시 실행 시스템

에디터는 실행 취소/다시 실행을 위해 JSON 직렬화 사용:

```csharp
private Stack<ChartDataNew> undoStack = new Stack<ChartDataNew>();
private Stack<ChartDataNew> redoStack = new Stack<ChartDataNew>();
private const int MAX_UNDO_STACK = 50;
```

**작동 방식:**
1. 각 수정 전에 현재 차트 상태를 JSON으로 직렬화
2. JSON을 undo 스택에 푸시
3. Ctrl+Z로 이전 상태 역직렬화
4. 메모리 문제 방지를 위해 최대 50단계

### 그리드 스냅 계산

```csharp
double CalculateSnappedTiming(double currentTime)
{
    if (!gridSnapEnabled) return currentTime;

    double beatInterval = 60.0 / bpm;
    double snapInterval = beatInterval / (int)currentBeatDivision;

    return System.Math.Round(currentTime / snapInterval) * snapInterval;
}
```

### 롱노트 양방향 배치

```csharp
void HandleLongNoteInput(double timing, int track)
{
    if (!isPlacingLongNote)
    {
        // 첫 번째 클릭 - 시작 위치 저장
        longNoteStart = new NoteData(timing, track, selectedKeySoundType);
        longNoteTrack = track;
        isPlacingLongNote = true;
    }
    else
    {
        // 두 번째 클릭 - 시작과 끝 결정
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

## 데이터 구조

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
    public double timing;              // 노트 히트 시간 (초)
    public float beatTiming;           // 박자 기반 타이밍
    public int track;                  // 레인 인덱스 (0부터 시작)
    public KeySoundType keySoundType;  // 재생할 키 사운드
    public bool isLongNote;            // 롱노트 여부
    public double longNoteEndTiming;   // 롱노트 종료 시간
}
```

### Enum

모든 enum은 `Assets/GameEnums.cs`에 정의:

```csharp
public enum KeySoundType
{
    None, Kick, Snare, Hihat, Vocal1, Vocal2,
    Synth1, Synth2, Bass, Piano, Guitar
}

public enum JudgmentMode
{
    Normal, Hard, Super,
    // 하위 호환성 별칭
    JudgmentMode_Normal = Normal,
    JudgmentMode_Hard = Hard,
    JudgmentMode_Super = Super
}
```

## 워크플로우 예제

### 새 차트 만들기

1. **설정**
   - Inspector에서 UI 요소 할당
   - 기본 BPM 설정 (예: 120)
   - 키 개수 설정 (예: 4K)

2. **오디오 로드**
   - `audioPathInputField`에 오디오 파일 경로 입력
   - "Load Audio" 버튼 클릭
   - UnityWebRequest를 통해 오디오 로드

3. **차트 메타데이터**
   - `songNameInput`에 곡명 입력
   - `artistNameInput`에 아티스트명 입력
   - 필요시 `bpmInput`에서 BPM 조정

4. **노트 배치**
   - **N** 누르면 일반 노트
   - 트랙을 클릭하여 노트 배치
   - **L** 누르면 롱노트
   - 시작 및 끝 위치 클릭

5. **그리드 조정**
   - **G** 눌러 그리드 스냅 순환
   - `[` `]` 로 비트 분할 조정

6. **차트 저장**
   - **Ctrl + S** 누르기
   - JSON으로 차트 저장

## 키보드 참조

### 필수 단축키
```
N              - 일반 노트 모드
L              - 롱노트 모드
Space          - 재생/일시정지
G              - 그리드 스냅 순환
T              - 편집 범위 전환
Ctrl + Z       - 실행 취소
Ctrl + Shift+Z - 다시 실행
Ctrl + S       - 차트 저장
```

### 그리드 컨트롤
```
G              - 스냅 순환: 1/4 → 1/8 → 1/16 → 1/32 → OFF
[              - 비트 분할 감소
]              - 비트 분할 증가
```

## 고급 기능

### 편집 범위 시스템

**목적**: 향후 BPM 변경 지원을 위한 준비

**두 가지 모드:**
1. **노트별**: 개별 노트에 변경사항 적용
2. **마디별**: 노트 범위에 변경사항 적용

**전환**: **T** 키 누르기

현재는 향후 마디 기반 편집 기능을 위한 플레이스홀더로 작동합니다.

### Undo 스택 관리

```csharp
void SaveUndoState()
{
    string json = JsonUtility.ToJson(currentChart);
    ChartDataNew snapshot = JsonUtility.FromJson<ChartDataNew>(json);

    undoStack.Push(snapshot);

    // 스택 크기 제한
    if (undoStack.Count > MAX_UNDO_STACK)
    {
        // 가장 오래된 항목 제거
        var temp = undoStack.ToArray();
        undoStack.Clear();
        for (int i = 1; i < temp.Length; i++)
            undoStack.Push(temp[i]);
    }

    redoStack.Clear(); // 새 작업 시 redo 지우기
}
```

## 알려진 제한사항

### Phase 1 (현재)
- 비주얼 파형 표시 없음
- 복사/붙여넣기 기능 없음
- 차트 중 BPM 변경 없음
- 슬라이드 노트 없음
- 수동 JSON 저장만 가능 (파일 대화상자 없음)

### Phase 2 계획
- 파형이 있는 비주얼 타임라인
- 고급 클립보드 작업
- 다중 BPM 섹션
- 슬라이드 노트 지원
- 파일 브라우저 통합

## 문제 해결

### 문제: 노트가 그리드에 스냅되지 않음
**해결책**: **G**를 눌러 원하는 스냅 모드가 활성화될 때까지 순환합니다. `gridSnapText` UI 표시를 확인하세요.

### 문제: 롱노트가 연결되지 않음
**해결책**: 롱노트 모드인지 확인 (**L** 누르기). 롱노트는 같은 트랙에 있어야 합니다.

### 문제: 실행 취소가 작동하지 않음
**해결책**: 에디터 초기화 후 변경사항이 있는지 확인하세요. 첫 번째 작업은 취소할 수 없습니다.

### 문제: 오디오가 로드되지 않음
**해결책**:
- 파일 경로가 올바른지 확인
- 파일이 `StreamingAssets` 또는 접근 가능한 경로에 있어야 함
- 지원되는 형식: .wav, .ogg, .mp3

### 문제: ChartEditor 컴포넌트를 찾을 수 없음
**해결책**: `ChartSystem` 네임스페이스를 확인하세요. 스크립트에서 `using ChartSystem;`을 사용하세요.

## 메인 게임과의 통합

### 게임플레이에서 차트 로드

```csharp
using ChartSystem;

// 에디터에서 생성한 차트 로드
ChartDataNew editorChart = LoadChartFromJSON("path/to/chart.json");

// 필요시 게임플레이 형식으로 변환
ChartData gameplayChart = ConvertToGameplayFormat(editorChart);

// 게임에서 사용
GameManager.Instance.LoadChart(gameplayChart);
```

### Enum 호환성

모든 enum (JudgmentMode, JudgmentType, KeySoundType)은 다음 간에 공유:
- 에디터 (`ChartSystem` 네임스페이스)
- 게임플레이 (global 네임스페이스)

정의 위치: `Assets/GameEnums.cs`

## 파일 구조

```
Assets/
├── edit/
│   ├── ChartEditor.cs          ← 메인 에디터 (이 파일)
│   ├── ChartDataNew.cs         ← 에디터 차트 데이터
│   └── ChartEditorNew.cs       ← DEPRECATED (주석 처리됨)
├── GameEnums.cs                 ← 모든 게임 enum
└── Play/
    ├── NoteData.cs              ← 공유 노트 데이터 구조
    └── ...
```

## 버전 히스토리

### Version 1.0 (Phase 1) - 2025-10-25
- ✅ 초기 릴리스
- ✅ 기본 노트 배치 (Normal/Long)
- ✅ 양방향 롱노트
- ✅ 그리드 스냅 시스템 (G 키)
- ✅ 실행 취소/다시 실행 (50 단계)
- ✅ 키보드 단축키
- ✅ 오디오 컨트롤
- ✅ 차트 저장/로드

### Version 2.0 (Phase 2) - 계획됨
- ⏳ 비주얼 타임라인
- ⏳ 고급 편집
- ⏳ BPM 변경
- ⏳ 슬라이드 노트

## 지원

문제 또는 질문이 있으시면:
- 로드맵은 `DEVELOPMENT_TODO.md` 확인
- 아키텍처 세부사항은 `CLAUDE.md` 참조
- 최근 변경사항은 `SESSION_SUMMARY_2025-10-25_2.md` 참조

---

**최종 업데이트**: 2025-10-25
**단계**: 1 완료
**상태**: 프로덕션 준비 완료
