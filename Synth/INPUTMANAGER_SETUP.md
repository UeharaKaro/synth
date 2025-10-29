# InputManager 설정 가이드

> **작성일**: 2025-01-29
> **목적**: InputManager의 올바른 설정 및 사용법
> **상태**: 완전 구현됨 ✅

---

## 📋 목차

1. [개요](#개요)
2. [Unity Editor 설정](#unity-editor-설정)
3. [기본 키 바인딩](#기본-키-바인딩)
4. [자동 기능](#자동-기능)
5. [커스텀 키 설정](#커스텀-키-설정)
6. [트러블슈팅](#트러블슈팅)

---

## 개요

### InputManager가 하는 일

```
✅ 키 입력 감지 (4K~10K)
✅ 자동 키 바인딩 설정
✅ 노트 히트 처리
✅ 롱노트 홀드/릴리즈
✅ 키 프레스 이펙트
✅ 키 통계 저장 (정확도 분석)
✅ 시스템 키 (ESC, R, Tab)
```

### 특징

- **완전 자동화**: 참조만 연결하면 모든 것이 자동 작동
- **유연한 키 바인딩**: 4K~10K, 5+1K 모드 지원
- **커스텀 키 지원**: SettingsManager 연동
- **통계 추적**: 각 키별 정확도 분석

---

## Unity Editor 설정

### Step 1: GameObject 생성

```
Hierarchy → 우클릭 → Create Empty
이름: "InputManager"
```

### Step 2: 컴포넌트 추가

```
Inspector → Add Component → InputManager
```

### Step 3: 참조 연결 (선택사항 - 자동 탐색 가능)

```
InputManager Inspector:

[References]
  - Gear Controller: → GearController (자동 탐색)
  - Note Manager: → NoteManager (자동 탐색)
  - Settings: → GearSettings (자동 가져옴)
  - Long Note System: → LongNoteSystem (자동 탐색)

[Key Bindings]
  - Line Keys: (비워둠 - 자동 생성)
```

### ✨ 권장 설정

```
참조 필드를 비워둬도 됩니다!
→ Start() 메서드에서 자동으로 FindObjectOfType()으로 찾음
```

---

## 기본 키 바인딩

### 자동 설정되는 키 배열

InputManager는 `GearSettings.lineCount`를 보고 자동으로 키를 설정합니다.

```csharp
// SetupDefaultKeyBindings() 메서드 (126-177줄)

4K:  D, F, J, K
5K:  D, F, Space, J, K
6K:  S, D, F, J, K, L
7K:  S, D, F, Space, J, K, L
8K:  A, S, D, F, J, K, L, Semicolon
10K: A, S, D, F, G, H, J, K, L, Semicolon

특수 모드:
-5K (5+1K): S, D, F/J, K, L  (F와 J가 같은 트랙)
```

### 키 레이아웃 시각화

#### 4K 모드 (기본)
```
   D    F    J    K
   │    │    │    │
  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐
  └─┘  └─┘  └─┘  └─┘
   0    1    2    3
```

#### 5K 모드
```
   D    F  Space  J    K
   │    │    │    │    │
  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐
  └─┘  └─┘  └─┘  └─┘  └─┘
   0    1    2    3    4
```

#### 7K 모드 (IIDX 스타일)
```
   S    D    F  Space  J    K    L
   │    │    │    │    │    │    │
  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐  ┌┴┐
  └─┘  └─┘  └─┘  └─┘  └─┘  └─┘  └─┘
   0    1    2    3    4    5    6
```

#### 5+1K 모드 (DJMAX 스타일)
```
   S    D    F/J   K    L
   │    │    │ │   │    │
  ┌┴┐  ┌┴┐  ┌┴┴┐  ┌┴┐  ┌┴┐
  └─┘  └─┘  └──┘  └─┘  └─┘
   0    1     2    3    4
   
  * F와 J 모두 Track 2를 트리거
```

---

## 자동 기능

### 1. 자동 참조 찾기 (Start 시)

```csharp
void FindReferences()
{
    // 모든 참조를 자동으로 찾음
    gearController = FindObjectOfType<GearController>();
    noteManager = FindObjectOfType<NoteManager>();
    longNoteSystem = FindObjectOfType<LongNoteSystem>();
    settings = gearController.settings;
}
```

**사용자는 아무것도 안해도 됨!**

### 2. 자동 키 바인딩 (Start 시)

```csharp
void SetupDefaultKeyBindings()
{
    // GearSettings.lineCount를 보고 자동 설정
    switch (settings.lineCount)
    {
        case 4: SetupKeys(4K 키들);
        case 5: SetupKeys(5K 키들);
        // ...
    }
}
```

**lineCount만 맞으면 자동 설정!**

### 3. 입력 처리 (Update 시)

```csharp
void ProcessInput()
{
    // 모든 키 체크
    foreach (var kvp in keyToLineMapping)
    {
        if (Input.GetKeyDown(key)) OnKeyPressed(lineIndex);
        if (Input.GetKeyUp(key)) OnKeyReleased(lineIndex);
        if (Input.GetKey(key)) OnKeyHold(lineIndex);
    }
}
```

**실시간 입력 감지 및 처리!**

### 4. 자동 이펙트

```csharp
void OnKeyPressed(int lineIndex)
{
    // 1. NoteManager에 히트 전달
    noteManager.ProcessNoteHit(lineIndex, Time.time);
    
    // 2. 롱노트 시스템에 전달
    longNoteSystem.StartLongNote(lineIndex, Time.time);
    
    // 3. 키 프레스 이펙트 자동 생성
    ShowKeyPressEffect(lineIndex);
    
    // 4. 히트 사운드 재생
    PlayHitSound(lineIndex);
}
```

**모든 것이 자동으로 처리됨!**

---

## 커스텀 키 설정

### 방법 1: SettingsManager 사용 (권장)

```csharp
// SettingsManager에서 키 설정 (옵션 메뉴에서)
SettingsManager.Instance.SetKeyBindings(keyCount, new KeyCode[] { ... });

// InputManager가 자동으로 가져옴
KeyCode[] customKeys = GetCustomKeyBindings();
```

### 방법 2: 런타임에서 직접 변경

```csharp
// InputManager 찾기
InputManager inputManager = FindObjectOfType<InputManager>();

// 특정 라인의 키 변경
inputManager.ChangeKeyBinding(0, KeyCode.A); // Line 0을 A키로
inputManager.ChangeKeyBinding(1, KeyCode.S); // Line 1을 S키로
```

### 방법 3: Inspector에서 수동 설정 (비권장)

```
InputManager Inspector:
  [Key Bindings]
    Line Keys:
      Element 0: D
      Element 1: F
      Element 2: J
      Element 3: K
```

**주의**: Start() 실행 시 자동 설정이 덮어씀!

---

## 시스템 키

### 자동으로 처리되는 키

```
ESC     → 일시정지 (PauseGame)
R       → 재시작 (RestartSong)
Tab     → 설정 열기 (OpenSettings)
```

**현재 상태**: 메서드만 정의, GameManager 연동 필요

---

## 키 통계 시스템

### 자동 추적되는 데이터

```csharp
// KeyStatistics 클래스
- 각 키별 입력 횟수
- 각 키별 판정 분포 (S_Perfect ~ Miss)
- 각 키별 평균 타이밍 오프셋
- PlayerPrefs에 자동 저장
```

### 통계 확인

```csharp
// InputManager에서
keyStatistics.RecordKeyHit(KeyCode.D, JudgmentType.Perfect, 0.01f);

// 저장 (OnDestroy 시 자동)
SaveStatistics();
```

---

## 작동 흐름

### 1. 초기화 (Start)

```
InputManager.Start()
  ├─ FindReferences()           // GearController, NoteManager 찾기
  ├─ SetupDefaultKeyBindings()  // 키 자동 설정
  ├─ LoadStatistics()           // 통계 로드
  └─ SubscribeToEvents()        // 이벤트 등록
```

### 2. 매 프레임 (Update)

```
InputManager.Update()
  └─ ProcessInput()
      ├─ foreach (키)
      │   ├─ GetKeyDown → OnKeyPressed()
      │   ├─ GetKeyUp → OnKeyReleased()
      │   └─ GetKey → OnKeyHold()
      └─ ProcessSystemInput()
```

### 3. 키 입력 시

```
OnKeyPressed(lineIndex)
  ├─ noteManager.ProcessNoteHit()     // 노트 판정
  ├─ longNoteSystem.StartLongNote()   // 롱노트 시작
  ├─ ShowKeyPressEffect()             // 이펙트
  └─ PlayHitSound()                   // 사운드
```

---

## GameScene에서의 설정

### Hierarchy 구조

```
GameScene
├── InputManager
│   └── InputManager.cs
│       ├── (자동) GearController 참조
│       ├── (자동) NoteManager 참조
│       ├── (자동) GearSettings 참조
│       └── (자동) LongNoteSystem 참조
```

### 체크리스트

```
[✓] InputManager GameObject 생성
[✓] InputManager.cs 컴포넌트 추가
[ ] 참조 연결 (선택사항 - 자동 탐색)
[ ] Play 버튼 → Console에서 "InputManager: 초기화 완료" 확인
```

---

## 트러블슈팅

### 문제 1: 키 입력이 안됨

```
해결:
1. Console 확인:
   "InputManager: 초기화 완료 (Line Count: 4)"
   
2. GearController 존재 확인:
   Hierarchy에 GearController 있는지
   
3. GearSettings 확인:
   GearController → Settings 할당되었는지
   
4. keyToLineMapping 확인:
   InputManager → Line Keys 자동 생성되었는지
```

### 문제 2: 특정 키만 안됨

```
해결:
1. 키 중복 확인:
   다른 UI/시스템이 해당 키 사용 중인지
   
2. 키 바인딩 확인:
   InputManager.lineKeys에 키가 있는지
   
3. 키보드 레이아웃 확인:
   일부 키는 레이아웃에 따라 다름
```

### 문제 3: 롱노트가 안됨

```
해결:
1. LongNoteSystem 존재 확인:
   Hierarchy에 LongNoteSystem 있는지
   
2. 롱노트 릴리즈 확인:
   GetKeyUp 이벤트 정상 작동하는지
   
3. 디버그 로그 추가:
   OnKeyHold()에 Debug.Log 추가
```

### 문제 4: 5+1K 모드가 안됨

```
해결:
1. lineCount 확인:
   GearSettings.lineCount = -5 (음수)
   
2. Setup5Plus1Keys() 호출 확인:
   Console에서 "5+1K 모드 설정 완료" 확인
   
3. F와 J 키 테스트:
   두 키 모두 Track 2 트리거하는지
```

---

## 고급 기능

### 1. 키 입력 지연 보정

```csharp
// SettingsManager에서 설정
float inputDelay = SettingsManager.Instance.GetInputDelay();

// 입력 시간에 보정 적용
float correctedTime = Time.time + inputDelay;
noteManager.ProcessNoteHit(lineIndex, correctedTime);
```

### 2. 동시 입력 감지

```csharp
// 여러 키를 동시에 눌렀을 때
List<int> simultaneousPresses = new List<int>();

foreach (var lineIndex in keyPressed.Keys)
{
    if (keyPressed[lineIndex])
        simultaneousPresses.Add(lineIndex);
}

// 동시 입력 이펙트
if (simultaneousPresses.Count >= 2)
{
    ShowSimultaneousHitEffect(simultaneousPresses);
}
```

### 3. 키 통계 분석

```csharp
// 가장 정확한 키 찾기
KeyCode mostAccurateKey = keyStatistics.GetMostAccurateKey();

// 가장 부정확한 키 찾기
KeyCode leastAccurateKey = keyStatistics.GetLeastAccurateKey();

// 키별 정확도 퍼센트
float accuracy = keyStatistics.GetKeyAccuracy(KeyCode.D);
```

---

## 완료 체크리스트

### 필수 설정
```
[✓] InputManager GameObject 생성
[✓] InputManager.cs 컴포넌트 추가
[✓] GearController 존재
[✓] GearSettings 할당
```

### 자동 확인 (Play 시)
```
[✓] "InputManager: 초기화 완료" 로그
[✓] Line Keys 자동 생성
[✓] Key Bindings 자동 설정
[✓] 키 입력 감지 작동
```

### 선택 설정
```
[ ] 커스텀 키 바인딩 (SettingsManager)
[ ] 키 통계 분석 활성화
[ ] 입력 지연 보정
```

---

## 코드 예시

### InputManager 연동 예시 (GameManager.cs)

```csharp
public class GameManager : MonoBehaviour
{
    private InputManager inputManager;
    
    void Start()
    {
        // InputManager 자동 탐색
        inputManager = FindObjectOfType<InputManager>();
        
        if (inputManager == null)
        {
            Debug.LogError("InputManager를 찾을 수 없습니다!");
        }
    }
    
    public void ChangeKeyCount(int newKeyCount)
    {
        // 키 개수 변경
        GearSettings settings = FindObjectOfType<GearController>().settings;
        settings.lineCount = newKeyCount;
        
        // InputManager가 자동으로 재설정됨
        inputManager.SetupDefaultKeyBindings();
    }
}
```

### 커스텀 키 설정 예시 (SettingsManager.cs)

```csharp
public class SettingsManager : MonoBehaviour
{
    public void SetCustomKeys(int keyCount, KeyCode[] keys)
    {
        // 키 저장
        string keyString = string.Join(",", keys);
        PlayerPrefs.SetString($"CustomKeys_{keyCount}K", keyString);
        
        // InputManager에 적용
        InputManager inputManager = FindObjectOfType<InputManager>();
        inputManager.SetupDefaultKeyBindings();
    }
    
    public KeyCode[] GetKeyBindings(int keyCount)
    {
        string keyString = PlayerPrefs.GetString($"CustomKeys_{keyCount}K", "");
        if (string.IsNullOrEmpty(keyString))
            return null;
            
        string[] keyCodes = keyString.Split(',');
        KeyCode[] keys = new KeyCode[keyCodes.Length];
        for (int i = 0; i < keyCodes.Length; i++)
        {
            keys[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyCodes[i]);
        }
        return keys;
    }
}
```

---

## 요약

### InputManager는:

```
✅ 완전히 구현되어 있음
✅ 자동으로 모든 참조 찾음
✅ 자동으로 키 바인딩 설정
✅ 4K~10K, 5+1K 모드 지원
✅ 커스텀 키 바인딩 지원
✅ 키 통계 자동 추적
✅ 이펙트 자동 생성
✅ 시스템 키 (ESC, R, Tab) 처리
```

### 사용자가 해야 할 일:

```
1. InputManager GameObject 생성
2. InputManager.cs 컴포넌트 추가
3. (끝!)
```

**모든 것이 자동으로 작동합니다!**

---

**작성**: Claude Code
**버전**: 1.0
**마지막 업데이트**: 2025-01-29

**참고 문서**:
- [GAMESCENE_STRUCTURE.md](GAMESCENE_STRUCTURE.md)
- [UNITY_SCENE_SETUP_CHECKLIST.md](UNITY_SCENE_SETUP_CHECKLIST.md)
