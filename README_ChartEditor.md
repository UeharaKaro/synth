# Unity 리듬게임 차트 에디터

Unity 기반 리듬게임을 위한 전문적인 차트 에디터 시스템입니다. 허가된 유저가 직접 노래를 들으면서 차트(노트모음)를 제작할 수 있도록 설계되었습니다.

## 주요 기능

### 🎵 노트 배치 시스템
- **N키**: 일반노트 모드 - 마우스 왼쪽 클릭 + 드래그로 각 레인에 일반노트 배치
- **L키**: 롱노트 모드 - 마우스 왼쪽 클릭 + 드래그로 롱노트 배치 (시작점과 끝점 설정)
- 그리드 스냅 기능으로 정확한 타이밍 배치

### 🎯 레인 시스템
- **+/-키**: 레인 수 조절 (4, 5, 6, 7, 8, 10개 레인 지원)
- 레인 수에 따른 UI 자동 조정
- 시각적 레인 가이드라인

### 🔊 오디오 시스템
- **AES 암호화된 .wav 파일** 로드 및 재생 지원
- **Shift + 화살표**: 차트 편집을 위한 재생 속도 변경 (0.1x ~ 2.0x)
- **Alt + 화살표**: 오디오 오프셋 ms 단위 조정
- **Ctrl + 화살표**: 스크롤 속도 강제 범위 제한

### 📐 마디 및 분할 시스템
- **1키**: 1/4 분할 (기본)
- **2키**: 1/8 분할
- **3키**: 1/16 분할
- 제작 모드와 미리보기 모드 구분

### 👁️ 미리보기 시스템
- **P키**: 미리보기 모드 진입
- 사용자 설정 스크롤 속도로 노트가 하강
- 실제 플레이 환경과 동일한 노트 이동

### ⚙️ 추가 기능
- **Ctrl + S/O**: 차트 데이터 저장/로드
- **Ctrl + Z/Y**: 실행 취소/다시 실행
- **Ctrl + A**: 전체 노트 선택
- **Delete**: 선택된 노트 삭제
- **F1**: 도움말 표시
- **F2**: 설정 패널 열기

## 설치 및 설정

### 필요 조건
- Unity 2022.3 LTS 이상
- FMOD 오디오 시스템 (포함됨)

### 설치 방법
1. 프로젝트 파일을 Unity에서 열기
2. `Assets/ChartEditor.cs`를 게임 오브젝트에 추가
3. 필요한 UI 요소들 연결
4. 오디오 파일을 `StreamingAssets/Audio/` 폴더에 배치

## 파일 구조

```
Assets/
├── Scripts/
│   ├── ChartEditor.cs          # 메인 에디터 스크립트
│   ├── ChartData.cs            # 차트 데이터 구조
│   ├── ChartValidator.cs       # 차트 유효성 검증
│   ├── ChartEditorManager.cs   # 에디터 매니저
│   ├── NoteData.cs             # 노트 데이터 클래스
│   ├── AudioManager.cs         # 오디오 관리
│   ├── EditorNote.cs           # 노트 프리팹 컨트롤러
│   ├── EditorLane.cs           # 레인 프리팹 컨트롤러
│   ├── TimelineDisplay.cs      # 타임라인 표시
│   ├── AudioVisualization.cs   # 오디오 시각화
│   └── ChartEditorTest.cs      # 테스트 스크립트
├── Prefabs/
│   ├── NotePrefab.prefab
│   ├── LongNotePrefab.prefab
│   ├── LanePrefab.prefab
│   └── EditorUI.prefab
└── StreamingAssets/
    └── Audio/
        ├── BGM/
        ├── KeySounds/
        └── Encrypted/
```

## 사용법

### 기본 차트 제작 과정

1. **오디오 로드**
   ```csharp
   chartEditor.LoadAudioFile("path/to/audio.wav");
   // 또는 암호화된 파일의 경우
   chartEditor.LoadEncryptedAudioFile(encryptedAudioAsset);
   ```

2. **BPM 설정**
   - Inspector에서 직접 설정하거나 코드로 조정

3. **레인 수 조절**
   - `+/-` 키로 4~10개 레인 선택

4. **노트 배치**
   - `N` 키로 일반노트 모드 선택
   - 마우스로 클릭하여 노트 배치
   - `L` 키로 롱노트 모드 선택 후 드래그로 배치

5. **미리보기**
   - `P` 키로 미리보기 모드 진입
   - 실제 게임과 같은 환경에서 테스트

6. **저장**
   - `Ctrl + S`로 차트 저장

### 고급 기능

#### 사용자 정의 설정
```csharp
var settings = ChartEditorManager.Instance.settings;
settings.noteSize = 1.2f;
settings.scrollSpeed = 10f;
settings.autoSave = true;
ChartEditorManager.Instance.ApplySettings();
```

#### 차트 유효성 검사
```csharp
var result = ChartValidator.ValidateChart(chartData);
if (!result.IsValid)
{
    Debug.LogError("차트 오류: " + result.GetSummary());
}
```

#### 노트 최적화
```csharp
var optimizedChart = ChartOptimizer.OptimizeChart(originalChart);
```

## API 참조

### ChartEditor 주요 메서드
- `LoadAudioFile(string filePath)`: 일반 오디오 파일 로드
- `LoadEncryptedAudioFile(TextAsset asset)`: 암호화된 오디오 파일 로드
- `SetPlaybackSpeed(float speed)`: 재생 속도 설정
- `SetAudioOffset(float offsetMs)`: 오디오 오프셋 설정
- `SaveChart()`: 차트 저장
- `LoadChart()`: 차트 로드

### ChartData 주요 속성
- `songTitle`: 곡 제목
- `artist`: 아티스트
- `bpm`: BPM
- `laneCount`: 레인 수
- `notes`: 노트 데이터 리스트
- `audioOffset`: 오디오 오프셋

## 테스트

프로젝트에는 자동 테스트 시스템이 포함되어 있습니다:

```csharp
// 테스트 실행
ChartEditorTest testRunner = FindObjectOfType<ChartEditorTest>();
testRunner.RunTestsManually();
```

## 성능 최적화

- 최적화된 노트 렌더링으로 60fps 유지
- 메모리 효율적인 오브젝트 풀링
- 실시간 오디오 동기화
- 스마트 그리드 생성 (필요한 부분만)

## 키보드 단축키 전체 목록

| 키 | 기능 |
|---|---|
| N | 일반 노트 모드 |
| L | 롱 노트 모드 |
| P | 미리보기 모드 토글 |
| + | 레인 수 증가 |
| - | 레인 수 감소 |
| 1 | 1/4 분할 |
| 2 | 1/8 분할 |
| 3 | 1/16 분할 |
| Space | 재생/일시정지 |
| Shift + ↑/↓ | 재생 속도 조절 |
| Ctrl + ↑/↓ | 스크롤 속도 조절 |
| Alt + ←/→ | 오디오 오프셋 조절 |
| Ctrl + S | 저장 |
| Ctrl + O | 로드 |
| Ctrl + Z | 실행 취소 |
| Ctrl + Y | 다시 실행 |
| Ctrl + A | 전체 선택 |
| Delete | 선택된 노트 삭제 |
| F1 | 도움말 표시 |
| F2 | 설정 패널 |
| ESC | 패널 닫기 |

## 문제 해결

### 일반적인 문제들

1. **오디오가 로드되지 않는 경우**
   - 파일 경로 확인
   - 오디오 형식 지원 여부 확인 (.wav 권장)
   - StreamingAssets 폴더 확인

2. **노트가 제대로 배치되지 않는 경우**
   - 그리드 스냅 설정 확인
   - BPM 설정 확인
   - 레인 수 설정 확인

3. **성능 이슈**
   - 최대 표시 노트 수 줄이기
   - 그리드 라인 수 줄이기
   - 오디오 품질 조정

## 라이선스

이 프로젝트는 MIT 라이선스 하에 제공됩니다.

## 기여하기

버그 리포트나 기능 제안은 GitHub Issues를 통해 제출해주세요.

---

더 자세한 정보는 프로젝트 내 스크립트의 주석을 참조하세요.