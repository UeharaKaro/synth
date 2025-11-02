# GameScene Builder 사용 가이드

> **생성일**: 2025-01-29  
> **목적**: GameScene 자동 생성 에디터 도구 사용법  
> **파일 위치**: `Assets/Editor/GameSceneBuilder.cs`

---

## 🎯 개요

**GameSceneBuilder**는 Unity Editor에서 GameScene을 **원클릭**으로 자동 생성하는 도구입니다.

### ✅ 자동 생성되는 것들

1. **Core Systems** (7개)
   - GameManager
   - ChartLoader
   - AudioManager
   - RhythmManager
   - InputManager

2. **Gameplay Objects** (3개)
   - GearController + 트랙 (4K~10K)
   - NoteSpawner
   - HPSystem + HP Bar

3. **UI Canvas** (5개 디스플레이)
   - ProgressDisplay (진행도 바)
   - ScoreDisplay (점수 표시)
   - ComboJudgmentDisplay (판정/콤보)
   - JudgmentOffsetDisplay (타이밍 오프셋)
   - PauseMenuUI (일시정지 메뉴)

4. **자동 참조 연결**
   - GameManager → 모든 시스템 연결
   - NoteSpawner → GearController, AudioManager 연결
   - UI → AudioManager 연결

---

## 🚀 사용 방법

### Step 1: Unity 에디터 열기
```
Unity Hub → Synth 프로젝트 열기
```

### Step 2: GameScene 생성
1. 상단 메뉴에서 **`Tools → Build GameScene`** 클릭
2. GameScene Builder 창이 열림

### Step 3: 설정 조정
```
Key Count: 4~10 (기본값: 4)
  - 4K, 5K, 6K, 7K, 8K, 10K 선택 가능

Create Sample Chart: ✅ (권장)
  - 테스트용 샘플 차트 자동 생성 (32개 노트)

Auto Connect References: ✅ (필수)
  - 모든 GameObject 참조 자동 연결
```

### Step 4: 빌드 실행
```
"Build Complete GameScene" 버튼 클릭
```

### Step 5: 완료 확인
```
✅ GameScene 생성 완료!
```

씬 Hierarchy에 다음 오브젝트들이 생성됩니다:
```
GameScene
├── GameManager
├── ChartLoader
├── AudioManager
├── RhythmManager
├── InputManager
├── GearController
│   ├── Tracks
│   │   ├── Track_0
│   │   ├── Track_1
│   │   ├── Track_2
│   │   └── Track_3
│   └── JudgmentLine
├── NoteSpawner
├── HPSystem
│   └── HPBar
├── Canvas
│   ├── ProgressDisplay
│   ├── ScoreDisplay
│   ├── ComboJudgmentDisplay
│   ├── JudgmentOffsetDisplay
│   └── PauseMenuUI
└── Main Camera (자동 설정)
```

---

## ⚙️ 자동 설정되는 값

### GameManager
```csharp
autoStart: false          // 수동 시작
useSampleChart: true      // 샘플 차트 사용
useNoteSpawner: true      // NoteSpawner 시스템 사용
```

### GearController
```csharp
lineCount: 4~10           // 선택한 키 수
lineWidth: 1f
lineSpacing: 0.1f
gearHeight: 8f
judgmentLineY: -3f
```

### NoteSpawner
```csharp
spawnOffset: 2f           // 2초 미리 스폰
noteSpeed: 5f             // 노트 이동 속도
```

### Camera
```csharp
orthographic: true
orthographicSize: 5f
position: (0, 0, -10)
backgroundColor: 어두운 파랑
```

---

## 🔧 생성 후 수동 작업

### 필수 작업 없음!
모든 참조가 자동으로 연결되므로, **즉시 플레이 가능**합니다.

### 선택적 작업

1. **Note Prefab 할당** (노트 비주얼 커스터마이징)
   ```
   NoteSpawner → Inspector → notePrefab 할당
   ```

2. **UI 위치 조정** (선택사항)
   ```
   Canvas → ProgressDisplay, ScoreDisplay 등 위치 조정
   ```

3. **트랙 색상 변경** (선택사항)
   ```
   GearController → Tracks → 각 트랙 Material 색상 변경
   ```

4. **HP 바 커스터마이징**
   ```
   HPSystem → HPBar → 색상, 크기 조정
   ```

---

## 🧪 테스트 방법

### 즉시 테스트
1. Unity Editor에서 **Play 버튼** 클릭
2. 샘플 차트가 자동으로 로드됨 (autoStart는 false이므로 수동 시작 필요)

### GameManager 테스트 모드 활성화
```
GameManager → Inspector
  - autoStart: ✅ true
  - useSampleChart: ✅ true
```
→ Play 버튼 누르면 즉시 게임 시작

### 키 입력 테스트 (4K 기준)
```
D, F, J, K 키로 노트 히트
ESC 키로 일시정지
R 키로 재시작
```

---

## ❓ 문제 해결

### 1. "GameManager를 찾을 수 없습니다" 에러
**원인**: 스크립트 컴파일 전에 빌더 실행  
**해결**:
```
1. Unity 메뉴: Assets → Reimport All
2. 컴파일 완료 대기 (Console 확인)
3. Tools → Build GameScene 재실행
```

### 2. 참조가 연결되지 않음
**원인**: Auto Connect References 옵션 체크 해제  
**해결**:
```
GameScene Builder 창에서
Auto Connect References: ✅ 체크
→ Build 재실행
```

### 3. UI가 보이지 않음
**원인**: Canvas 렌더 모드 문제  
**해결**:
```
Canvas → Inspector
  - Render Mode: Screen Space - Overlay
  - Sorting Order: 10 이상
```

### 4. 트랙이 화면 밖에 생성됨
**원인**: 카메라 크기 문제  
**해결**:
```
Main Camera → Inspector
  - Orthographic Size: 5~6 조정
  - Position: (0, 0, -10)
```

### 5. 샘플 차트가 로드되지 않음
**원인**: ChartLoader 초기화 실패  
**해결**:
```
1. ChartLoader → Inspector 확인
2. Resources/Charts 폴더 생성:
   Assets → Create → Folder → "Resources/Charts"
3. 또는 GameManager:
   useSampleChart: ✅ true (CreateSampleChart 사용)
```

---

## 🔄 씬 재생성

기존 GameScene을 삭제하고 다시 생성하려면:

### 방법 1: 수동 삭제 후 재생성
```
1. Hierarchy에서 모든 오브젝트 선택 (Ctrl+A)
2. Delete 키
3. Tools → Build GameScene 재실행
```

### 방법 2: 새 씬에서 생성
```
1. File → New Scene
2. Tools → Build GameScene 실행
3. File → Save Scene As → "GameScene.unity"
```

---

## 📝 고급 커스터마이징

### Key Count 변경 (4K → 6K)
```
1. Tools → Build GameScene
2. Key Count: 6 선택
3. Build 실행
→ 6개 트랙 자동 생성
```

### 스크립트 확장
`GameSceneBuilder.cs` 파일을 수정하여 추가 오브젝트 생성 가능:

```csharp
// GameSceneBuilder.cs

private GameObject CreateMyCustomObject()
{
    GameObject obj = new GameObject("MyObject");
    obj.AddComponent<MyComponent>();
    Debug.Log("✓ MyObject 생성");
    return obj;
}

// BuildGameScene() 메서드에 추가
GameObject myObj = CreateMyCustomObject();
```

---

## ✅ 체크리스트

씬 생성 후 확인 사항:

```
[ ] Hierarchy에 모든 오브젝트 생성됨
[ ] Console에 에러 없음
[ ] GameManager Inspector에 모든 참조 연결됨
[ ] Main Camera Orthographic 설정됨
[ ] Canvas UI 요소들 보임
[ ] Play 버튼 눌러서 동작 확인
[ ] 샘플 차트 로드 확인 (autoStart: true 설정 시)
[ ] 키 입력 반응 확인 (D, F, J, K)
```

---

## 🎓 추가 정보

### 관련 문서
- `02_SCENE_SETUP.md` - 씬 설정 상세 가이드
- `GAMESCENE_STRUCTURE.md` - GameScene 구조 설명
- `DEVELOPMENT_TODO.md` - 개발 진행 상황

### 스크립트 위치
```
Assets/Editor/GameSceneBuilder.cs
```

### 생성된 씬 저장 위치 (권장)
```
Assets/Scenes/GameScene.unity
```

### 샘플 차트 저장 위치
```
Resources/Charts/SampleChart.json (자동 생성)
또는
StreamingAssets/Charts/ (수동 배치)
```

---

## 🚨 주의사항

1. **기존 오브젝트 백업**
   - 빌더는 기존 씬에 추가로 생성하므로, 중요한 오브젝트는 미리 백업하세요.

2. **Prefab 연결 필요**
   - Note Prefab, LongNote Prefab은 수동으로 할당 필요
   - `Assets/Prefabs/` 폴더에 생성 후 NoteSpawner에 할당

3. **TextMeshPro 필수**
   - UI 텍스트는 TextMeshPro 사용
   - Window → TextMeshPro → Import TMP Essential Resources

4. **FMOD 설정**
   - AudioManager는 FMOD 사용
   - FMOD 없으면 AudioManagerNew.cs 사용

5. **씬 저장 필수**
   - 빌더 실행 후 반드시 씬 저장 (Ctrl+S)

---

## 📞 지원

문제가 발생하면:
1. Console 로그 확인
2. `DEVELOPMENT_TODO.md` 버그 섹션 참고
3. 씬 재생성 시도

**Happy Coding! 🎮**
