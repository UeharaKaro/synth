# ChartEditorBeta - Unity 리듬 게임 차트 에디터

## 개요

ChartEditorBeta는 향상된 기능, 더 나은 사용자 경험, 그리고 전문적인 차트 제작을 위한 고급 기능을 갖춘 Unity 리듬 게임 차트 에디터의 개선된 버전입니다.

## 기능

### 핵심 기능
- **멀티 레인 지원**: 4, 5, 6, 7, 8, 10 레인 구성
- **노트 타입**: 일반 노트와 비주얼 트레일을 가진 롱노트
- **실시간 프리뷰**: 정확한 타이밍 시뮬레이션을 가진 플레이 모드
- **그리드 스냅**: 사용자 정의 분할을 통한 정밀한 노트 배치
- **AES 암호화**: 배포용 오디오 파일 보안 암호화

### 에디터 컨트롤

#### 모드 전환
- **N key**: 일반 노트 모드로 전환
- **L key**: 롱노트 모드로 전환
- **P key**: 프리뷰 모드 토글

#### 레인 관리
- **+ key / = key**: 레인 수 증가
- **- key**: 레인 수 감소

#### 오디오 컨트롤
- **Space**: 오디오 재생/일시정지
- **Timeline Slider**: 특정 시간으로 이동

#### 노트 조작
- **Left Click + Drag**: 노트 배치 (모드에 따름)
- **Shift + Click**: 노트 다중 선택
- **Ctrl + A**: 모든 노트 선택
- **Delete / Backspace**: 선택된 노트 삭제
- **Escape**: 선택 해제

#### 에디터 기능
- **Ctrl + Z**: 실행 취소
- **Ctrl + Shift + Z**: 다시 실행
- **Ctrl + S**: 차트 저장 (UI 버튼이 연결된 경우)
- **Ctrl + O**: 차트 로드 (UI 버튼이 연결된 경우)

#### 그리드 및 타이밍
- **G key**: 그리드 스냅 모드 순환 (None, 1/4, 1/8, 1/16, 1/32)
- **[ key**: 비트 분할 감소
- **] key**: 비트 분할 증가

#### 프리뷰 모드 키 (레인별)
- **4 레인**: D, F, J, K
- **5 레인**: D, F, Space, J, K
- **6 레인**: S, D, F, J, K, L
- **7 레인**: S, D, F, Space, J, K, L
- **8 레인**: A, S, D, F, J, K, L, ;
- **10 레인**: A, S, D, F, G, H, J, K, L, ;

## 설정 지침

### 1. 기본 설정
1. 씬에 빈 GameObject를 생성합니다
2. `ChartEditorBeta` 컴포넌트를 추가합니다
3. 인스펙터에서 공개 필드를 구성합니다

### 2. 필수 컴포넌트
```csharp
// 오디오 컴포넌트
public AudioSource audioSource; // 자동으로 추가됨
public AudioManagerBeta audioManagerBeta; // 자동으로 생성됨

// UI 요소 (선택사항이지만 권장)
public InputField audioPathInputField;
public Slider timelineSlider;
public Text currentTimeText;
public Text totalTimeText;
public Button playButton;
public Button pauseButton;
public Button stopButton;
public Button saveButton;
public Button loadButton;

// 비주얼 요소
public GameObject notePrefab;
public GameObject longNotePrefab;
public Transform noteContainer;
public Transform[] tracks;
public Material gridLineMaterial;
```

### 3. 프리팹 생성
`ChartEditorBetaPrefabCreator` 스크립트를 사용하여 필요한 프리팹을 자동으로 생성합니다:

1. 임의의 GameObject에 `ChartEditorBetaPrefabCreator`를 연결합니다
2. 머티리얼과 설정을 구성합니다
3. `CreatePrefabs()`를 호출하거나 컨텍스트 메뉴를 사용합니다
4. 생성된 프리팹을 ChartEditorBeta에 할당합니다

### 4. UI 설정
에디터는 UI 없이도 작동할 수 있지만, 완전한 기능을 위해서는:

1. Canvas를 생성합니다
2. 타임라인, 버튼, 정보 표시용 UI 요소를 추가합니다
3. 이들을 ChartEditorBeta 컴포넌트에 연결합니다
4. 자동 UI 생성을 위해 `ChartEditorBetaPrefabCreator.CreateEditorUIPrefab()`를 사용합니다

## 고급 기능

### 오디오 암호화
```csharp
// 오디오 파일 암호화
ChartEditorBetaFileUtils.EncryptAudioFile(
    "path/to/audio.wav", 
    "path/to/encrypted.eaw", 
    "your-encryption-key"
);

// AudioManagerBeta가 로딩 중 자동으로 복호화
audioManagerBeta.LoadAudioFile("path/to/encrypted.eaw");
```

### 차트 내보내기/가져오기
```csharp
// 메타데이터와 함께 내보내기
chartEditor.SaveChartAs(
    "path/to/chart.chart",
    "Song Title",
    "Artist Name", 
    "Charter Name"
);

// 특정 경로에서 로드
chartEditor.LoadChartFrom("path/to/chart.chart");
```

### 프리뷰 시스템 통합
```csharp
// 프리뷰 시스템 접근
ChartEditorBetaPreview preview = chartEditor.previewSystem;

// 이벤트 구독
preview.OnNoteJudged += (judgment) => Debug.Log($"Judgment: {judgment}");
preview.OnNoteSpawned += (noteData) => Debug.Log($"Note spawned: {noteData.track}");
```

## 파일 형식

### 차트 데이터 구조
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

### 메타데이터를 포함한 내보내기 데이터
```json
{
    "metadata": {
        "title": "Song Title",
        "artist": "Artist Name",
        "charter": "Charter Name",
        "createdDate": "2023-12-07 10:30:00",
        "version": "1.0"
    },
    "chartData": { /* 차트 데이터 구조 */ }
}
```

## 성능 고려사항

### 노트 풀링
프리뷰 시스템은 최적의 성능을 위해 오브젝트 풀링을 사용합니다:
- 노트들이 계속해서 생성/파괴되지 않고 재사용됩니다
- 풀 크기는 차트 복잡도에 따라 자동으로 조정됩니다
- 긴 차트에 대한 메모리 사용량이 최소화됩니다

### 그리드 시스템
- 그리드 라인은 BPM과 비트 분할에 따라 동적으로 생성됩니다
- 보이는 그리드 라인만 렌더링됩니다
- 그리드는 설정이 변경될 때만 업데이트됩니다

### 오디오 관리
- 오디오 복호화는 로딩 중 한 번만 수행됩니다
- 암호화된 파일은 빠른 접근을 위해 메모리에 캐시됩니다
- 대용량 오디오 파일의 스트리밍을 지원합니다

## 기존 시스템과의 통합

### 호환성
- **AudioManager.cs**: 호환성을 유지하며, FMOD가 사용 가능할 때 사용합니다
- **NoteData.cs**: NoteDataBeta로 확장하여 원본 구조를 보존합니다
- **RhythmManager.cs**: 기존 판정 시스템과 열거형을 사용합니다
- **GameSettings.cs**: 기존 설정 시스템과 통합됩니다

### 마이그레이션
기존 차트와 함께 ChartEditorBeta를 사용하려면:
```csharp
// 기존 ChartData를 ChartDataBeta로 변환
ChartDataBeta betaChart = new ChartDataBeta();
betaChart.audioFileName = originalChart.audioFileName;
betaChart.bpm = originalChart.bpm;
betaChart.laneCount = 4; // 적절한 레인 수 설정

// 노트 변환
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

## 문제 해결

### 일반적인 문제

1. **노트가 나타나지 않음**: `notePrefab`과 `noteContainer`가 할당되었는지 확인하세요
2. **그리드가 보이지 않음**: `gridLineMaterial`이 할당되고 `showGrid`가 true인지 확인하세요
3. **오디오가 로드되지 않음**: 파일 경로와 암호화된 파일을 사용하는 경우 암호화 키를 확인하세요
4. **프리뷰 모드가 작동하지 않음**: 레인이 올바르게 구성되고 입력 키가 충돌하지 않는지 확인하세요

### 디버그 옵션
```csharp
// ChartEditorBeta에서 디버그 로깅 활성화
Debug.Log("Current mode: " + currentEditMode);
Debug.Log("Selected notes: " + selectedNotes.Count);
Debug.Log("Preview active: " + previewSystem.IsActive);
```

### 성능 모니터링
```csharp
// 성능 모니터링
Debug.Log("Active preview notes: " + previewSystem.ActiveNoteCount);
Debug.Log("Remaining notes: " + previewSystem.RemainingNoteCount);
```

## 예제

### 기본 사용법
```csharp
public class ChartEditorController : MonoBehaviour
{
    public ChartEditorBeta chartEditor;
    
    void Start()
    {
        // 오디오 파일 로드
        chartEditor.LoadAudioFile("Assets/Audio/song.wav");
        
        // BPM 설정
        chartEditor.UpdateBPM(140f);
        
        // 레인 수 설정
        chartEditor.UpdateLaneCount(6);
    }
    
    void Update()
    {
        // 사용자 정의 컨트롤
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

### 고급 통합
```csharp
public class AdvancedChartEditor : MonoBehaviour
{
    public ChartEditorBeta chartEditor;
    
    void Start()
    {
        // 이벤트 구독
        chartEditor.previewSystem.OnNoteJudged += HandleJudgment;
        chartEditor.previewSystem.OnPreviewStarted += OnPreviewStart;
        
        // 설정 구성
        chartEditor.scrollSpeed = 10f;
        chartEditor.gridSnapMode = GridSnapMode.Beat_1_8;
        
        // 암호화된 오디오 로드
        var audioManager = AudioManagerBeta.Instance;
        audioManager.SetEncryptionKey("MySecretKey123");
        audioManager.LoadAudioFile("path/to/encrypted.eaw");
    }
    
    void HandleJudgment(JudgmentType judgment)
    {
        // 판정 피드백 처리
        Debug.Log($"Player hit: {judgment}");
    }
    
    void OnPreviewStart()
    {
        Debug.Log("Preview mode started!");
    }
}
```

## 기여하기

ChartEditorBeta를 확장할 때:

1. 기존 코드 구조와 패턴을 따르세요
2. 적절한 오류 처리와 디버그 로깅을 추가하세요
3. 기존 시스템과의 호환성을 유지하세요
4. `ChartEditorBetaTest`를 사용하여 단위 테스트를 추가하세요
5. 새로운 기능에 대한 문서를 업데이트하세요

## 라이선스

이것은 Unity 리듬 게임 프로젝트의 일부입니다. 프로젝트의 라이선스 조건을 따르세요.