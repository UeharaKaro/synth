# PauseMenuUI 설정 가이드

## 📌 작동 방식

**PauseMenuUI는 게임플레이 씬에서만 작동합니다:**
- ✅ **게임 플레이 중**: ESC 키로 일시정지/재개 가능
- ❌ **메인 메뉴**: 작동하지 않음
- ❌ **노래 선택 화면**: 작동하지 않음
- ❌ **결과 화면**: 작동하지 않음

**자동 제어:**
- `GameManager.StartGame()` 호출 시 → 일시정지 활성화
- `GameManager.HandleGameOver()` 호출 시 → 일시정지 비활성화
- `GameManager.HandleGameClear()` 호출 시 → 일시정지 비활성화

## Unity Editor 설정 방법

### 1단계: 캔버스 및 패널 생성

1. **Hierarchy**에서 우클릭 → `UI` → `Canvas` (이미 있다면 생략)
2. Canvas 하위에 우클릭 → `UI` → `Panel` → 이름: `PauseMenuPanel`

### 2단계: PauseMenuPanel 설정

**Inspector 설정:**
- **Image 컴포넌트**:
  - Color: 검은색 (R:0, G:0, B:0, A:200) - 반투명 배경
- **RectTransform**:
  - Anchor: Stretch (전체 화면)
  - Left: 0, Top: 0, Right: 0, Bottom: 0

### 3단계: 제목 텍스트 추가

1. `PauseMenuPanel` 하위에 우클릭 → `UI` → `Text - TextMeshPro` → 이름: `TitleText`
2. **RectTransform**:
   - Position: (0, 150, 0)
   - Width: 400, Height: 80
3. **TextMeshProUGUI**:
   - Text: "일시정지"
   - Font Size: 60
   - Alignment: 가운데 정렬
   - Color: 흰색

### 4단계: 버튼 추가

각 버튼을 `PauseMenuPanel` 하위에 추가:

#### 재개 버튼
1. 우클릭 → `UI` → `Button - TextMeshPro` → 이름: `ResumeButton`
2. **RectTransform**:
   - Position: (0, 50, 0)
   - Width: 300, Height: 60
3. 하위 `Text (TMP)` 수정:
   - Text: "재개"
   - Font Size: 32

#### 재시작 버튼
1. 우클릭 → `UI` → `Button - TextMeshPro` → 이름: `RestartButton`
2. **RectTransform**:
   - Position: (0, -30, 0)
   - Width: 300, Height: 60
3. 하위 `Text (TMP)` 수정:
   - Text: "재시작"
   - Font Size: 32

#### 설정 버튼
1. 우클릭 → `UI` → `Button - TextMeshPro` → 이름: `OptionsButton`
2. **RectTransform**:
   - Position: (0, -110, 0)
   - Width: 300, Height: 60
3. 하위 `Text (TMP)` 수정:
   - Text: "설정"
   - Font Size: 32

#### 메인 메뉴 버튼
1. 우클릭 → `UI` → `Button - TextMeshPro` → 이름: `MainMenuButton`
2. **RectTransform**:
   - Position: (0, -190, 0)
   - Width: 300, Height: 60
3. 하위 `Text (TMP)` 수정:
   - Text: "메인 메뉴"
   - Font Size: 32

### 5단계: PauseMenuUI 스크립트 연결

1. **빈 GameObject 생성**:
   - Hierarchy에서 우클릭 → `Create Empty` → 이름: `PauseMenuManager`

2. **스크립트 추가**:
   - `PauseMenuManager` 선택
   - Inspector에서 `Add Component` → `PauseMenuUI` 스크립트 추가

3. **참조 연결** (Inspector):
   - **Pause Menu Panel**: `PauseMenuPanel` 드래그
   - **Resume Button**: `ResumeButton` 드래그
   - **Restart Button**: `RestartButton` 드래그
   - **Options Button**: `OptionsButton` 드래그
   - **Main Menu Button**: `MainMenuButton` 드래그
   - **Title Text**: `TitleText` 드래그 (선택사항)
   - **Resume Text**: `ResumeButton/Text (TMP)` 드래그 (선택사항)
   - **Restart Text**: `RestartButton/Text (TMP)` 드래그 (선택사항)
   - **Options Text**: `OptionsButton/Text (TMP)` 드래그 (선택사항)
   - **Main Menu Text**: `MainMenuButton/Text (TMP)` 드래그 (선택사항)

4. **설정값 확인**:
   - **Pause Key**: Escape (기본값)
   - **Main Menu Scene Name**: "MainMenu" (메인 메뉴 씬 이름에 맞게 수정)
   - **Options Scene Name**: "OptionsScene" (설정 씬 이름)
   - **Enable Only In Gameplay**: ✅ 체크 (게임플레이 중에만 활성화)

### 6단계: 테스트

1. Unity Play 버튼 클릭
2. **게임이 시작되면** (GameManager.StartGame() 호출 후) ESC 키 활성화
3. **ESC 키** 누르면 일시정지 메뉴 표시
4. 버튼 동작 확인:
   - **재개**: 게임 계속
   - **재시작**: 씬 재로드
   - **설정**: 로그만 출력 (설정 씬 구현 필요)
   - **메인 메뉴**: MainMenu 씬으로 이동

**중요:**
- 메인 메뉴나 노래 선택 화면에서는 ESC 키가 작동하지 않습니다
- GameManager가 없는 씬에서는 자동으로 비활성화됩니다
- 게임 시작 전에는 ESC 키가 작동하지 않습니다

---

## 추가 커스터마이징

### 버튼 색상 변경
각 버튼의 **Image** 컴포넌트에서:
- **Normal Color**: 기본 색상
- **Highlighted Color**: 마우스 오버 색상
- **Pressed Color**: 클릭 시 색상
- **Disabled Color**: 비활성화 색상

### 애니메이션 추가 (선택)
1. `PauseMenuPanel` 선택
2. Window → Animation → Animation
3. Fade In/Out 애니메이션 제작

### 사운드 이펙트 추가 (선택)
1. 각 버튼에 `Audio Source` 컴포넌트 추가
2. Button의 `OnClick` 이벤트에 `AudioSource.Play()` 연결

---

## 트러블슈팅

### ESC 키가 작동하지 않을 때
- Input System이 활성화되어 있는지 확인
- PauseMenuUI 스크립트가 활성화되어 있는지 확인

### Time.timeScale이 복원되지 않을 때
- OnDestroy()에서 자동으로 복원됨
- 수동으로 복원: `Time.timeScale = 1f;`

### GameManager를 찾지 못할 때
- GameManager.Instance가 Singleton으로 설정되어 있는지 확인
- GameManager가 씬에 존재하는지 확인

---

## 계층 구조 예시

```
Canvas
└── PauseMenuPanel
    ├── TitleText (TextMeshProUGUI)
    ├── ResumeButton
    │   └── Text (TMP)
    ├── RestartButton
    │   └── Text (TMP)
    ├── OptionsButton
    │   └── Text (TMP)
    └── MainMenuButton
        └── Text (TMP)

PauseMenuManager (Empty GameObject)
└── PauseMenuUI (Script)
```

---

## 완료 체크리스트

- [ ] PauseMenuPanel 생성 및 배경 설정
- [ ] TitleText 추가
- [ ] 4개 버튼 생성 (재개, 재시작, 설정, 메인 메뉴)
- [ ] PauseMenuManager GameObject 생성
- [ ] PauseMenuUI 스크립트 추가 및 참조 연결
- [ ] ESC 키 테스트
- [ ] 모든 버튼 동작 테스트
- [ ] 씬 이름 설정 확인 (MainMenu)
