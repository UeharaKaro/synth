# ESC 메뉴 설정 가이드

## 📌 개요

각 씬에서 ESC 키로 표시되는 메뉴 시스템:
- **메인 메뉴**: 옵션, 크레딧, 게임 종료
- **곡 선택 화면**: 메인 메뉴로, 옵션, 게임 종료
- **게임 플레이**: 일시정지 메뉴 (PauseMenuUI)

---

## 1️⃣ 메인 메뉴 ESC 메뉴 설정

### 파일: `MainMenuESCMenu.cs`

### Unity Editor 설정:

#### 1단계: 패널 생성
1. MainMenu 씬의 Canvas 하위에 우클릭 → `UI` → `Panel` → 이름: `ESCMenuPanel`
2. **Inspector 설정**:
   - Color: 반투명 검은색 (R:0, G:0, B:0, A:200)
   - Anchor: Stretch (전체 화면)

#### 2단계: 제목 추가
1. `ESCMenuPanel` 하위에 `UI` → `Text - TextMeshPro` → 이름: `TitleText`
2. Position: (0, 100, 0), Width: 300, Height: 60
3. Text: "메뉴", Font Size: 48, 가운데 정렬

#### 3단계: 버튼 추가

**설정 버튼:**
- Position: (0, 30, 0), Width: 250, Height: 50
- Text: "설정"

**크레딧 버튼:**
- Position: (0, -30, 0), Width: 250, Height: 50
- Text: "크레딧"

**게임 종료 버튼:**
- Position: (0, -90, 0), Width: 250, Height: 50
- Text: "게임 종료"

**취소 버튼:**
- Position: (0, -150, 0), Width: 250, Height: 50
- Text: "취소"

#### 4단계: 스크립트 연결
1. Hierarchy에 빈 GameObject 생성 → 이름: `MainMenuESCManager`
2. `MainMenuESCMenu` 스크립트 추가
3. **참조 연결**:
   - ESC Menu Panel: `ESCMenuPanel`
   - Options Button: `OptionsButton`
   - Credits Button: `CreditsButton`
   - Quit Game Button: `QuitGameButton`
   - Cancel Button: `CancelButton`
   - 각 텍스트 참조 (선택사항)

---

## 2️⃣ 곡 선택 화면 ESC 메뉴 설정

### 파일: `SongSelectionESCMenu.cs`

### Unity Editor 설정:

#### 1단계: 패널 생성
1. SongSelection 씬의 Canvas 하위에 우클릭 → `UI` → `Panel` → 이름: `ESCMenuPanel`
2. **Inspector 설정**:
   - Color: 반투명 검은색 (R:0, G:0, B:0, A:200)
   - Anchor: Stretch (전체 화면)

#### 2단계: 제목 추가
1. `ESCMenuPanel` 하위에 `UI` → `Text - TextMeshPro` → 이름: `TitleText`
2. Position: (0, 100, 0), Width: 300, Height: 60
3. Text: "메뉴", Font Size: 48, 가운데 정렬

#### 3단계: 버튼 추가

**메인 메뉴로 버튼:**
- Position: (0, 30, 0), Width: 250, Height: 50
- Text: "메인 메뉴로"

**설정 버튼:**
- Position: (0, -30, 0), Width: 250, Height: 50
- Text: "설정"

**게임 종료 버튼:**
- Position: (0, -90, 0), Width: 250, Height: 50
- Text: "게임 종료"

**취소 버튼:**
- Position: (0, -150, 0), Width: 250, Height: 50
- Text: "취소"

#### 4단계: 스크립트 연결
1. Hierarchy에 빈 GameObject 생성 → 이름: `SongSelectionESCManager`
2. `SongSelectionESCMenu` 스크립트 추가
3. **참조 연결**:
   - ESC Menu Panel: `ESCMenuPanel`
   - Back To Main Menu Button: `BackToMainMenuButton`
   - Options Button: `OptionsButton`
   - Quit Game Button: `QuitGameButton`
   - Cancel Button: `CancelButton`
   - Main Menu Scene Name: "MainMenu"
   - Show Confirm Dialog: ✅ (메인 메뉴로 돌아갈 때 확인)

---

## 📋 기능 설명

### 메인 메뉴 ESC 메뉴
```
ESC 키 → 메뉴 열림
├── 설정: 옵션 화면으로 (TODO)
├── 크레딧: 크레딧 화면으로 (TODO)
├── 게임 종료: 게임 종료
└── 취소: 메뉴 닫기
```

### 곡 선택 화면 ESC 메뉴
```
ESC 키 → 메뉴 열림 + 미리듣기 일시정지
├── 메인 메뉴로: MainMenu 씬 로드
├── 설정: 옵션 화면으로 (TODO)
├── 게임 종료: 게임 종료
└── 취소: 메뉴 닫기 + 미리듣기 재개
```

---

## 🎨 커스터마이징

### 버튼 색상 변경
각 버튼의 **Image** 컴포넌트:
- Normal Color: 기본 색상
- Highlighted Color: 마우스 오버
- Pressed Color: 클릭 시
- Selected Color: 선택됨

### 패널 배경 효과
- 블러 효과 추가 (Post-Processing)
- 그라데이션 배경
- 애니메이션 효과

### 사운드 추가
1. 각 버튼에 Audio Source 추가
2. OnClick 이벤트에 PlayOneShot 연결
3. ESC 키 누를 때 메뉴 열림/닫힘 사운드

---

## 🔧 고급 기능 (TODO)

### 확인 다이얼로그
메인 메뉴로 돌아가기 전 확인:
```csharp
// SongSelectionESCMenu.cs
if (showConfirmDialog)
{
    // "정말 메인 메뉴로 돌아가시겠습니까?"
    // 예/아니오 다이얼로그 표시
}
```

### 설정 화면 통합
옵션 버튼 클릭 시:
- **방법 1**: 별도 씬으로 이동 (`OptionsScene`)
- **방법 2**: 오버레이 패널 표시 (권장)

### 크레딧 화면
크레딧 버튼 클릭 시:
- 개발자, 음악, 아트 크레딧 표시
- 스크롤 뷰로 구현

---

## ⚠️ 주의사항

### ESC 키 충돌 방지
- 메인 메뉴: `MainMenuESCMenu` 사용
- 곡 선택: `SongSelectionESCMenu` 사용
- 게임 플레이: `PauseMenuUI` 사용 (자동 활성화)
- 각 씬에 하나만 배치

### AudioManager 연동
곡 선택 화면에서:
- ESC 메뉴 열 때 → 미리듣기 일시정지
- ESC 메뉴 닫을 때 → 미리듣기 재개
- 메인 메뉴로 이동 시 → 오디오 정지

### 게임 종료 처리
```csharp
#if UNITY_EDITOR
    // 에디터에서는 플레이 모드 종료
    UnityEditor.EditorApplication.isPlaying = false;
#else
    // 빌드에서는 애플리케이션 종료
    Application.Quit();
#endif
```

---

## 📊 계층 구조 예시

### 메인 메뉴
```
Canvas
└── ESCMenuPanel
    ├── TitleText
    ├── OptionsButton
    │   └── Text (TMP)
    ├── CreditsButton
    │   └── Text (TMP)
    ├── QuitGameButton
    │   └── Text (TMP)
    └── CancelButton
        └── Text (TMP)

MainMenuESCManager (Empty GameObject)
└── MainMenuESCMenu (Script)
```

### 곡 선택 화면
```
Canvas
└── ESCMenuPanel
    ├── TitleText
    ├── BackToMainMenuButton
    │   └── Text (TMP)
    ├── OptionsButton
    │   └── Text (TMP)
    ├── QuitGameButton
    │   └── Text (TMP)
    └── CancelButton
        └── Text (TMP)

SongSelectionESCManager (Empty GameObject)
└── SongSelectionESCMenu (Script)
```

---

## ✅ 완료 체크리스트

### 메인 메뉴
- [ ] ESCMenuPanel 생성 및 배경 설정
- [ ] 4개 버튼 생성 (설정, 크레딧, 게임 종료, 취소)
- [ ] MainMenuESCManager GameObject 생성
- [ ] MainMenuESCMenu 스크립트 추가 및 참조 연결
- [ ] ESC 키 테스트
- [ ] 모든 버튼 동작 테스트

### 곡 선택 화면
- [ ] ESCMenuPanel 생성 및 배경 설정
- [ ] 4개 버튼 생성 (메인 메뉴로, 설정, 게임 종료, 취소)
- [ ] SongSelectionESCManager GameObject 생성
- [ ] SongSelectionESCMenu 스크립트 추가 및 참조 연결
- [ ] ESC 키 테스트
- [ ] AudioManager 일시정지/재개 테스트
- [ ] 메인 메뉴 이동 테스트

---

**작성일**: 2025-10-26  
**버전**: 1.0
