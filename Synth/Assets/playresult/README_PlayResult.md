# 플레이 결과 시스템 사용 가이드

플레이 결과 화면을 구현하는 스크립트들입니다.

## 파일 구성

1. **PlayResultData.cs** - 플레이 결과 데이터를 저장하는 클래스
2. **PlayResultUI.cs** - 플레이 결과 UI를 표시하는 메인 스크립트
3. **GameResultManager.cs** - 게임 결과를 관리하고 씬 간 데이터 전달을 담당하는 싱글톤 매니저
4. **ResultSceneLoader.cs** - 결과 씬 로드 시 자동으로 데이터를 표시하는 헬퍼 스크립트

## Unity 설정 방법

### 1. 게임 씬 설정

게임 씬에 빈 GameObject를 생성하고 `GameResultManager` 스크립트를 추가합니다.

```
Hierarchy:
  - GameManager (GameObject)
    - GameResultManager (Component)
```

### 2. 결과 씬 설정

결과 씬에 UI를 구성하고 스크립트를 연결합니다.

```
Hierarchy:
  - Canvas
    - ResultPanel
      - SongInfoPanel
        - SongTitleText (TextMeshProUGUI)
        - ArtistNameText (TextMeshProUGUI)
        - DifficultyText (TextMeshProUGUI)
        - KeyCountText (TextMeshProUGUI)

      - ResultPanel
        - ScoreText (TextMeshProUGUI)
        - AccuracyText (TextMeshProUGUI)
        - MaxComboText (TextMeshProUGUI)
        - RankText (TextMeshProUGUI)

      - JudgmentPanel
        - SPerfectCountText (TextMeshProUGUI)
        - PerfectCountText (TextMeshProUGUI)
        - GreatCountText (TextMeshProUGUI)
        - GoodCountText (TextMeshProUGUI)
        - BadCountText (TextMeshProUGUI)
        - MissCountText (TextMeshProUGUI)

      - SpecialIndicators
        - FullComboIndicator (GameObject)
        - PerfectPlayIndicator (GameObject)

      - ButtonPanel
        - RetryButton (Button)
        - BackToSongSelectButton (Button)
        - BackToMainMenuButton (Button)

  - ResultSceneManager (GameObject)
    - PlayResultUI (Component) - UI 참조 연결
    - ResultSceneLoader (Component) - PlayResultUI 참조 연결
```

### 3. Inspector 설정

**PlayResultUI 컴포넌트:**
- 모든 TextMeshProUGUI 필드에 해당 UI 요소 연결
- 버튼 연결
- 씬 이름 설정:
  - `currentGameSceneName`: 현재 게임 씬 이름
  - `songSelectionSceneName`: 곡 선택 씬 이름
  - `mainMenuSceneName`: 메인 메뉴 씬 이름

**ResultSceneLoader 컴포넌트:**
- `playResultUI`: PlayResultUI 컴포넌트 연결

## 코드 사용 방법

### 게임 시작 시 (게임 씬)

```csharp
void StartGame()
{
    // 게임 시작 시 곡 정보 설정
    GameResultManager.Instance.SetCurrentSongInfo(
        songTitle: "Test Song",
        artistName: "Test Artist",
        difficulty: "Hard",
        keyCount: 4
    );
}
```

### 게임 종료 시 (게임 씬)

```csharp
void OnGameEnd()
{
    // RhythmScoreSystem에서 결과 가져오기
    GameResult gameResult = rhythmScoreSystem.GetGameResult();

    // 결과 저장 및 결과 화면으로 전환
    GameResultManager.Instance.SaveResultAndShowResultScreen(
        gameResult,
        resultSceneName: "ResultScene"
    );
}
```

### 직접 PlayResultUI 사용하기

결과 씬에서 직접 데이터를 설정할 수도 있습니다:

```csharp
// 방법 1: PlayResultData 직접 생성
PlayResultData data = new PlayResultData
{
    songTitle = "Test Song",
    artistName = "Test Artist",
    difficulty = "Hard",
    keyCount = 4,
    score = 950000,
    accuracy = 98.5f,
    maxCombo = 512,
    sPerfectCount = 450,
    perfectCount = 50,
    greatCount = 10,
    goodCount = 2,
    badCount = 0,
    missCount = 0
};
data.CalculatePlayStats();
playResultUI.SetResultData(data);

// 방법 2: GameResult로부터 생성
GameResult gameResult = /* ... */;
playResultUI.SetResultFromGameResult(
    gameResult,
    songTitle: "Test Song",
    artistName: "Test Artist",
    difficulty: "Hard",
    keyCount: 4
);
```

## 랭크 시스템

정확도에 따라 자동으로 랭크가 결정됩니다:

- **SSS**: Perfect Play (S Perfect + Perfect만 존재, S Perfect > Perfect)
- **SS**: Perfect Play (S Perfect + Perfect만 존재)
- **S**: 정확도 99% 이상
- **A**: 정확도 95% 이상
- **B**: 정확도 90% 이상
- **C**: 정확도 80% 이상
- **D**: 정확도 70% 이상
- **F**: 정확도 70% 미만

## 특수 표시

- **Full Combo**: Miss와 Bad가 0개일 때 표시
- **Perfect Play**: Great, Good, Bad, Miss가 모두 0개일 때 표시

## 애니메이션

결과 표시 시 다음과 같은 순서로 애니메이션이 재생됩니다:

1. 곡 정보 표시 (0.5초 딜레이)
2. 랭크 표시
3. 점수, 정확도, 콤보 카운트 애니메이션 (1.5초)
4. 판정 카운트 표시
5. 특수 표시 (Full Combo, Perfect Play)

## 테스트 방법

결과 씬을 단독으로 테스트하려면:

```csharp
// PlayResultUI에서 제공하는 테스트 데이터 로드
playResultUI.LoadTestData();
```

또는 Inspector에서 Play 모드로 진입하면 `ResultSceneLoader`가 자동으로 테스트 데이터를 로드합니다.

## 주의사항

1. **GameResultManager는 씬 전환 시 파괴되지 않습니다** (DontDestroyOnLoad)
2. 게임 시작 시 반드시 `SetCurrentSongInfo()`를 호출하여 곡 정보를 설정하세요
3. UI 요소들은 TextMeshPro를 사용합니다 (TextMeshProUGUI)
4. 버튼 이벤트는 스크립트에서 자동으로 등록됩니다 (Inspector에서 OnClick 설정 불필요)

## 확장 방법

### 추가 통계 표시

`PlayResultData` 클래스에 필드를 추가하고 `PlayResultUI`에서 표시:

```csharp
// PlayResultData.cs
public int earlyCount = 0;  // Fast 판정 수
public int lateCount = 0;   // Late 판정 수

// PlayResultUI.cs
public TextMeshProUGUI earlyCountText;
public TextMeshProUGUI lateCountText;

private void UpdateAdditionalStats()
{
    if (earlyCountText != null)
        earlyCountText.text = resultData.earlyCount.ToString();

    if (lateCountText != null)
        lateCountText.text = resultData.lateCount.ToString();
}
```

### 온라인 순위 저장

```csharp
void OnGameEnd()
{
    GameResult gameResult = rhythmScoreSystem.GetGameResult();

    // 온라인 순위에 저장
    await UploadScoreToLeaderboard(gameResult.score);

    // 결과 화면으로 전환
    GameResultManager.Instance.SaveResultAndShowResultScreen(gameResult);
}
```
