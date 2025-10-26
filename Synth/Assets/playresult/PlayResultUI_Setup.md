# PlayResultUI 설정 가이드

> **버전**: 1.0  
> **최종 업데이트**: 2025-10-26  
> **파일**: `Assets/playresult/PlayResultUI.cs`

## 📋 목차

1. [개요](#개요)
2. [Unity Editor 설정](#unity-editor-설정)
3. [UI 계층 구조](#ui-계층-구조)
4. [상세 설정 가이드](#상세-설정-가이드)
5. [애니메이션 설정](#애니메이션-설정)
6. [사운드 설정](#사운드-설정)
7. [씬 통합](#씬-통합)
8. [트러블슈팅](#트러블슈팅)

---

## 개요

**PlayResultUI**는 리듬 게임 플레이 종료 후 결과를 표시하는 UI 시스템입니다.

### 주요 기능
- ✅ 곡 정보 표시 (제목, 아티스트, 난이도, 키 개수)
- ✅ 게임 결과 표시 (점수, 정확도, 최대 콤보, 랭크)
- ✅ 판정 통계 표시 (S Perfect ~ Miss)
- ✅ 특수 표시 (Full Combo, Perfect Play, NEW RECORD)
- ✅ 숫자 카운트 애니메이션 (점수, 정확도, 콤보)
- ✅ 랭크 등장 애니메이션 (스케일 + 이징)
- ✅ 신기록 표시 애니메이션
- ✅ 사운드 이펙트 (결과 표시, 랭크, 신기록, 버튼)
- ✅ 네비게이션 버튼 (재시작, 곡 선택, 메인 메뉴)

### 시스템 요구사항
- Unity 2021.3 이상
- TextMesh Pro (필수)
- GameResultManager (싱글톤)
- PlayResultData 클래스

---

## Unity Editor 설정

### 1단계: 결과 화면 씬 생성

1. **새 씬 생성**
   - `File → New Scene → Empty`
   - 씬 이름: `ResultScene.unity`
   - 저장 위치: `Assets/Scenes/ResultScene.unity`

2. **Canvas 추가**
   - `Hierarchy 우클릭 → UI → Canvas`
   - Canvas 이름: `ResultCanvas`
   
3. **Canvas 설정**
   - Canvas Scaler:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Match: `0.5` (Width와 Height 중간)

### 2단계: UI 계층 구조 생성

**전체 구조**:
```
ResultCanvas
├── BackgroundPanel (배경)
├── ContentPanel (메인 컨텐츠)
│   ├── SongInfoPanel (곡 정보)
│   │   ├── SongTitleText (TMP)
│   │   ├── ArtistNameText (TMP)
│   │   ├── DifficultyText (TMP)
│   │   └── KeyCountText (TMP)
│   ├── ResultPanel (결과 정보)
│   │   ├── RankText (TMP) - 대형 텍스트
│   │   ├── ScoreText (TMP)
│   │   ├── AccuracyText (TMP)
│   │   └── MaxComboText (TMP)
│   ├── JudgmentPanel (판정 통계)
│   │   ├── JudgmentRow_SPerfect
│   │   │   ├── LabelText: "S PERFECT"
│   │   │   └── CountText (TMP)
│   │   ├── JudgmentRow_Perfect
│   │   ├── JudgmentRow_Great
│   │   ├── JudgmentRow_Good
│   │   ├── JudgmentRow_Bad
│   │   └── JudgmentRow_Miss
│   └── SpecialIndicatorsPanel
│       ├── FullComboIndicator (Image/Text)
│       ├── PerfectPlayIndicator (Image/Text)
│       └── NewRecordIndicator (Image/Text)
└── ButtonPanel (하단 버튼)
    ├── RetryButton
    ├── BackToSongSelectButton
    └── BackToMainMenuButton
```

---

## 상세 설정 가이드

### ContentPanel - 메인 컨텐츠

1. **Panel 생성**
   - `Hierarchy → ResultCanvas 우클릭 → UI → Panel`
   - 이름: `ContentPanel`

2. **RectTransform 설정**
   - Anchor: `Middle Center`
   - Width: `1200`, Height: `900`
   - Pos X: `0`, Pos Y: `0`

3. **Image 설정** (배경)
   - Color: 반투명 검정 `RGBA(0, 0, 0, 200)`
   - 또는 Custom Sprite 사용

---

### SongInfoPanel - 곡 정보 표시

**위치**: ContentPanel 상단

1. **Panel 생성**
   - `ContentPanel 우클릭 → UI → Panel`
   - 이름: `SongInfoPanel`

2. **RectTransform 설정**
   - Anchor: `Top Center`
   - Width: `1100`, Height: `150`
   - Pos Y: `-75`

3. **텍스트 추가**

#### SongTitleText (곡 제목)
```
- 컴포넌트: TextMeshProUGUI
- Text: "Song Title"
- Font Size: 48
- Alignment: Center
- Color: White
- Font Style: Bold
```

#### ArtistNameText (아티스트)
```
- Font Size: 28
- Text: "Artist Name"
- Color: RGBA(200, 200, 200, 255)
```

#### DifficultyText (난이도)
```
- Font Size: 24
- Text: "Difficulty: Hard"
- Color: Yellow (난이도별 색상 변경)
```

#### KeyCountText (키 개수)
```
- Font Size: 24
- Text: "4K"
- Color: Cyan
```

---

### ResultPanel - 결과 정보 표시

**위치**: ContentPanel 중앙

1. **Panel 생성**
   - 이름: `ResultPanel`
   - Anchor: `Middle Center`
   - Width: `1100`, Height: `350`

2. **RankText (등급) - 중앙 대형 텍스트**
```
- Font Size: 120
- Text: "S"
- Alignment: Center
- Font Style: Bold
- Pos Y: 100
- Color: 동적 (PlayResultData.GetRankColor())
```

3. **ScoreText (점수)**
```
- Font Size: 42
- Text: "950,000"
- Alignment: Center
- Pos Y: 0
- Enable Rich Text
```

4. **AccuracyText (정확도)**
```
- Font Size: 36
- Text: "98.50%"
- Pos Y: -50
```

5. **MaxComboText (최대 콤보)**
```
- Font Size: 36
- Text: "MAX COMBO: 512"
- Pos Y: -100
```

---

### JudgmentPanel - 판정 통계 표시

**위치**: ContentPanel 하단

1. **Panel 생성**
   - 이름: `JudgmentPanel`
   - Anchor: `Bottom Center`
   - Width: `1100`, Height: `250`
   - Pos Y: `125`

2. **Vertical Layout Group 추가**
   - Spacing: `10`
   - Child Alignment: `Upper Center`
   - Child Force Expand: Width ✅, Height ❌

3. **JudgmentRow 프리팹 생성** (6개 필요)

#### JudgmentRow 구조:
```
JudgmentRow_SPerfect (Horizontal Layout Group)
├── LabelText (TMP) - "S PERFECT"
└── CountText (TMP) - "450"
```

#### LabelText 설정:
```
- Font Size: 28
- Alignment: Left
- Color: Gold (판정별 색상)
- Flexible Width: 1
```

#### CountText 설정:
```
- Font Size: 28
- Alignment: Right
- Color: White
- Flexible Width: 1
```

#### 판정별 색상 추천:
```
- S Perfect: Gold (255, 215, 0)
- Perfect: Yellow (255, 255, 0)
- Great: Green (0, 255, 0)
- Good: Blue (0, 128, 255)
- Bad: Orange (255, 128, 0)
- Miss: Red (255, 0, 0)
```

---

### SpecialIndicatorsPanel - 특수 표시

**위치**: ResultPanel 상단 또는 하단

1. **Panel 생성**
   - 이름: `SpecialIndicatorsPanel`
   - Anchor: `Top Center` (ResultPanel 기준)
   - Width: `800`, Height: `80`

2. **Horizontal Layout Group 추가**
   - Spacing: `30`
   - Child Alignment: `Middle Center`

3. **FullComboIndicator (풀 콤보)**
```
- Image + TextMeshProUGUI
- Text: "FULL COMBO"
- Font Size: 36
- Color: Cyan
- 초기 상태: Inactive (코드에서 활성화)
```

4. **PerfectPlayIndicator (퍼펙트 플레이)**
```
- Text: "PERFECT PLAY"
- Font Size: 36
- Color: Gold
- 초기 상태: Inactive
```

5. **NewRecordIndicator (신기록)**
```
- Text: "★ NEW RECORD ★"
- Font Size: 42
- Color: Rainbow (애니메이션으로 색상 변경 가능)
- 초기 상태: Inactive
```

---

### ButtonPanel - 네비게이션 버튼

**위치**: Canvas 하단

1. **Panel 생성**
   - 이름: `ButtonPanel`
   - Anchor: `Bottom Center`
   - Width: `1200`, Height: `120`
   - Pos Y: `60`

2. **Horizontal Layout Group 추가**
   - Spacing: `40`
   - Child Alignment: `Middle Center`

3. **버튼 3개 생성**

#### RetryButton (재시작)
```
- Text: "RETRY"
- Width: 300, Height: 80
- Normal Color: Gray
- Highlighted Color: Yellow
- Pressed Color: Orange
- Font Size: 32
- Event: PlayResultUI.OnRetryButtonClicked()
```

#### BackToSongSelectButton (곡 선택)
```
- Text: "SONG SELECT"
- Width: 300, Height: 80
- Normal Color: Blue
- Highlighted Color: Cyan
- Event: PlayResultUI.OnBackToSongSelectButtonClicked()
```

#### BackToMainMenuButton (메인 메뉴)
```
- Text: "MAIN MENU"
- Width: 300, Height: 80
- Normal Color: Red
- Highlighted Color: Pink
- Event: PlayResultUI.OnBackToMainMenuButtonClicked()
```

---

## 상세 설정 가이드 - PlayResultUI 컴포넌트

### 3단계: PlayResultUI 스크립트 연결

1. **GameObject 선택**
   - `ResultCanvas` 또는 `ContentPanel` 선택

2. **컴포넌트 추가**
   - `Add Component → Scripts → Play Result UI`

3. **Inspector 설정**

#### 곡 정보 UI
```
Song Title Text: SongInfoPanel/SongTitleText
Artist Name Text: SongInfoPanel/ArtistNameText
Difficulty Text: SongInfoPanel/DifficultyText
Key Count Text: SongInfoPanel/KeyCountText
```

#### 결과 정보 UI
```
Score Text: ResultPanel/ScoreText
Accuracy Text: ResultPanel/AccuracyText
Max Combo Text: ResultPanel/MaxComboText
Rank Text: ResultPanel/RankText
```

#### 판정 카운트 UI
```
S Perfect Count Text: JudgmentPanel/JudgmentRow_SPerfect/CountText
Perfect Count Text: JudgmentPanel/JudgmentRow_Perfect/CountText
Great Count Text: JudgmentPanel/JudgmentRow_Great/CountText
Good Count Text: JudgmentPanel/JudgmentRow_Good/CountText
Bad Count Text: JudgmentPanel/JudgmentRow_Bad/CountText
Miss Count Text: JudgmentPanel/JudgmentRow_Miss/CountText
```

#### 특수 표시 UI
```
Full Combo Indicator: SpecialIndicatorsPanel/FullComboIndicator
Perfect Play Indicator: SpecialIndicatorsPanel/PerfectPlayIndicator
New Record Indicator: SpecialIndicatorsPanel/NewRecordIndicator
```

#### 버튼
```
Retry Button: ButtonPanel/RetryButton
Back To Song Select Button: ButtonPanel/BackToSongSelectButton
Back To Main Menu Button: ButtonPanel/BackToMainMenuButton
```

#### 씬 설정
```
Current Game Scene Name: "GameScene" (또는 플레이한 씬 이름)
Song Selection Scene Name: "SongSelectionScene"
Main Menu Scene Name: "MainMenuScene"
```

#### 애니메이션 설정
```
Animation Duration: 0.5 (초)
Count Animation Duration: 1.5 (초)
Rank Animation Duration: 0.8 (초)
```

#### 사운드 설정 (선택사항)
```
Result Show Sound: 결과 화면 표시 시 사운드
Rank Reveal Sound: 랭크 등장 시 사운드
New Record Sound: 신기록 달성 시 사운드
Button Click Sound: 버튼 클릭 사운드
```

#### 신기록 설정 (선택사항)
```
Previous Best Score: 0 (이전 최고 점수, 0이면 비활성화)
```

---

## 애니메이션 설정

### 1. 점수 카운트 애니메이션

**동작**: 0부터 최종 점수까지 부드럽게 카운트업

**설정**:
- `countAnimationDuration`: 1.5초 (기본값)
- Easing: Ease-out Cubic (부드러운 감속)

**커스터마이징**:
```csharp
// PlayResultUI.cs - AnimateScoreDisplay() 메서드
float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease-out
```

### 2. 랭크 등장 애니메이션

**동작**: 스케일 0 → 1 (Elastic Ease-out)

**설정**:
- `rankAnimationDuration`: 0.8초 (기본값)
- Easing: Elastic Ease-out (튕기는 효과)

**커스터마이징**:
```csharp
// 탄성 계수 조절
float overshoot = 1.5f; // 값이 클수록 더 튕김
```

### 3. 신기록 표시 애니메이션

**동작**: 펄스 애니메이션 (3회 반복)

**설정**:
- Scale 범위: 0.9 ~ 1.1
- 속도: 2.0 (초당 사이클)

---

## 사운드 설정

### 권장 사운드 이펙트

1. **Result Show Sound**
   - 타이밍: 결과 화면 표시 시작
   - 유형: 신디사이저 스윕, 밝은 종소리
   - 길이: 1-2초

2. **Rank Reveal Sound**
   - 타이밍: 랭크 텍스트 등장 시
   - 유형: 임팩트, 드럼 히트
   - 길이: 0.5-1초
   - 랭크별 다른 사운드 (선택사항)

3. **New Record Sound**
   - 타이밍: 신기록 달성 시
   - 유형: 팡파르, 승리 음악
   - 길이: 2-3초

4. **Button Click Sound**
   - 타이밍: 버튼 클릭 시
   - 유형: 클릭, 탭 사운드
   - 길이: 0.1-0.3초

### 사운드 파일 설정

1. **AudioClip Import**
   - 파일 형식: `.wav`, `.ogg`, `.mp3`
   - Import Settings:
     - Load Type: `Decompress On Load` (짧은 사운드)
     - Compression Format: `Vorbis` (용량 절약)

2. **Inspector에 할당**
   - PlayResultUI 컴포넌트 → 사운드 설정 섹션
   - Drag & Drop AudioClip

---

## 씬 통합

### GameManager에서 결과 화면 호출

**게임 종료 시 호출**:
```csharp
// Assets/Play/GameManager.cs (또는 게임 종료 로직)

// 게임 종료 시
void OnGameFinished()
{
    // GameResult 생성
    GameResult result = new GameResult
    {
        score = currentScore,
        accuracy = currentAccuracy,
        maxCombo = currentMaxCombo,
        sPerfectCount = sPerfectCount,
        perfectCount = perfectCount,
        greatCount = greatCount,
        goodCount = goodCount,
        badCount = badCount,
        missCount = missCount
    };

    // 결과 저장 및 결과 화면으로 전환
    if (GameResultManager.Instance != null)
    {
        GameResultManager.Instance.SaveResultAndShowResultScreen(result, "ResultScene");
    }
    else
    {
        Debug.LogError("GameResultManager가 없습니다!");
    }
}
```

### Build Settings에 씬 추가

1. **File → Build Settings**
2. **Scenes in Build**
   - `ResultScene` 추가 (Drag & Drop)
   - 씬 순서 확인

---

## 트러블슈팅

### 문제 1: 결과 데이터가 표시되지 않음

**증상**: 모든 텍스트가 기본값 또는 비어있음

**원인**:
- GameResultManager가 씬에 없음
- 게임 씬에서 `SaveResultAndShowResultScreen()` 호출 안됨

**해결**:
1. GameResultManager GameObject 확인
   - `Hierarchy → Create Empty → GameResultManager`
   - `Add Component → GameResultManager`
   - DontDestroyOnLoad 확인

2. 게임 종료 시 호출 확인
```csharp
// 게임 종료 로직에서
GameResultManager.Instance.SaveResultAndShowResultScreen(gameResult, "ResultScene");
```

---

### 문제 2: 텍스트가 화면 밖으로 나감

**원인**: RectTransform 설정 오류

**해결**:
1. Canvas Scaler 확인
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

2. Anchor 확인
   - 모든 UI 요소가 올바른 Anchor 사용 확인

---

### 문제 3: 애니메이션이 작동하지 않음

**원인**: 코루틴 실행 실패 또는 참조 누락

**해결**:
1. PlayResultUI가 활성화된 GameObject에 있는지 확인
2. 텍스트 참조가 모두 할당되었는지 확인
3. Console 에러 확인 (NullReferenceException)

---

### 문제 4: 버튼 클릭이 안됨

**원인**: EventSystem 누락

**해결**:
1. `Hierarchy → UI → Event System` 추가
2. 씬에 EventSystem이 하나만 있는지 확인

---

### 문제 5: 신기록 표시가 나타나지 않음

**원인**: `previousBestScore` 설정 안됨

**해결**:
```csharp
// 곡 선택 시 또는 게임 시작 시
PlayResultUI resultUI = FindObjectOfType<PlayResultUI>();
if (resultUI != null)
{
    resultUI.previousBestScore = PlayerPrefs.GetInt("BestScore_SongName_Difficulty", 0);
}
```

---

## 체크리스트

### UI 설정 완료 체크
- [ ] Canvas 생성 완료
- [ ] Canvas Scaler 설정 완료
- [ ] 모든 UI 요소 생성 완료
- [ ] TextMeshProUGUI 사용 (Unity UI Text 아님)
- [ ] PlayResultUI 스크립트 연결 완료
- [ ] Inspector에 모든 참조 할당 완료

### 기능 테스트
- [ ] 테스트 데이터로 결과 표시 확인
- [ ] 점수 카운트 애니메이션 작동
- [ ] 랭크 표시 애니메이션 작동
- [ ] 특수 표시 (Full Combo, Perfect Play) 작동
- [ ] 신기록 표시 작동 (조건 충족 시)
- [ ] 버튼 3개 모두 작동

### 씬 통합 완료
- [ ] GameResultManager 씬에 존재
- [ ] 게임 종료 시 결과 저장 호출
- [ ] Build Settings에 ResultScene 추가
- [ ] 씬 전환 작동 확인

### 사운드 설정 (선택사항)
- [ ] Result Show Sound 할당
- [ ] Rank Reveal Sound 할당
- [ ] New Record Sound 할당
- [ ] Button Click Sound 할당

---

## 추가 커스터마이징

### 랭크별 색상 변경

PlayResultData.cs에 정의된 색상:
```csharp
- SSS: Gold (1, 0.84, 0)
- SS: Silver (0.9, 0.9, 0.9)
- S: Orange (1, 0.5, 0)
- A: Sky Blue (0.2, 0.8, 1)
- B: Green (0.3, 1, 0.3)
- C: Yellow (1, 1, 0.3)
- D: Light Red (1, 0.5, 0.5)
- F: Gray (0.5, 0.5, 0.5)
```

### 랭크별 기준 변경

PlayResultData.cs - CalculateRank() 메서드:
```csharp
if (accuracy >= 99.0f) return "S";
else if (accuracy >= 95.0f) return "A";
// ... 수정 가능
```

---

## 참고 파일

**스크립트**:
- `Assets/playresult/PlayResultUI.cs` - 메인 UI 컨트롤러
- `Assets/playresult/PlayResultData.cs` - 결과 데이터 구조
- `Assets/playresult/GameResultManager.cs` - 싱글톤 매니저
- `Assets/playresult/GameResult.cs` - 게임 결과 구조체

**문서**:
- `Assets/playresult/README_PlayResult.md` - 시스템 개요

---

**작성일**: 2025-10-26  
**작성자**: Claude AI Assistant  
**버전**: 1.0
