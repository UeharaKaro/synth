# Pattern Radar System Guide

패턴 레이더 시스템 사용 가이드 (사운드 볼텍스 이펙터 레이더 스타일)

## 개요

패턴 레이더 시스템은 플레이어의 실력을 8가지 패턴 타입별로 시각화하는 기능입니다.
플레이어가 플레이한 곡들 중 각 패턴별로 상위 N곡(기본 50곡)의 점수를 종합하여 8각형 레이더 차트로 표시합니다.

## 시스템 구성 요소

### 1. 핵심 클래스

- **PlayRecord** - 개별 플레이 기록
- **PlayerProfile** - 플레이어 프로필 및 전체 기록 관리
- **PatternRadarData** - 레이더 차트 계산 결과 데이터
- **PatternRadarChart** - Unity UI 레이더 차트 시각화
- **ProfileUIManager** - 프로필 UI 관리
- **GameResultRecorder** - 게임 결과 자동 기록

### 2. 파일 위치

```
synth/Synth/Assets/Play/
├── PlayRecord.cs              ← 플레이 기록 데이터
├── PlayerProfile.cs           ← 프로필 관리 + 레이더 계산
├── PatternRadarChart.cs       ← 레이더 차트 UI
├── ProfileUIManager.cs        ← 프로필 UI 매니저
└── GameResultRecorder.cs      ← 게임 결과 기록 헬퍼

synth/Synth/Assets/edit/
└── PatternDifficulty.cs       ← 패턴 난이도 정의 (0-20)
```

---

## 패턴 타입 (8가지)

| 패턴 | 설명 | 난이도 범위 |
|------|------|------------|
| **트릴** | 두 트랙에 배치된 노트가 번갈아 나오는 교차 연타 | 0-20 (소수점 1자리) |
| **계단** | 계단을 옆에서 본 것처럼 노트가 배치된 패턴 | 0-20 (소수점 1자리) |
| **동치** | 여러 개의 시퀀스를 동시에 치는 패턴 | 0-20 (소수점 1자리) |
| **데님** | 135-24 / 1357-246 식으로 거미줄처럼 짜인 배치 | 0-20 (소수점 1자리) |
| **따닥이** | 짧은 연타와 잡노트가 섞인 배치, 고속 처리 | 0-20 (소수점 1자리) |
| **롱잡** | 롱노트를 처리하는 중에 다른 노트를 처리 | 0-20 (소수점 1자리) |
| **폭타** | 순간적/지속적으로 많은 노트 처리 필요 | 0-20 (소수점 1자리) |
| **즈레** | 정박에서 약간 어긋나는 엇박 패턴 | 0-20 (소수점 1자리) |

---

## 1. 게임 플레이 후 기록 저장

게임이 끝나면 `GameResultRecorder`를 사용하여 자동으로 기록을 저장합니다.

### 방법 1: ChartDataNew 사용 (에디터 차트)

```csharp
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ChartSystem.ChartDataNew currentChart;

    void OnGameEnd()
    {
        // 게임 결과 데이터
        int score = 950000;
        float accuracy = 98.5f;
        int maxCombo = 850;
        int perfect = 800;
        int great = 45;
        int good = 5;
        int bad = 0;
        int miss = 0;
        bool isCleared = true;

        // 자동으로 기록 저장
        GameResultRecorder.RecordGameResult(
            currentChart,
            score,
            accuracy,
            maxCombo,
            perfect,
            great,
            good,
            bad,
            miss,
            isCleared
        );

        Debug.Log("플레이 기록이 저장되었습니다!");
    }
}
```

### 방법 2: ChartData 사용 (플레이 차트)

```csharp
void OnGameEnd()
{
    ChartData currentChart = GetCurrentChart();

    GameResultRecorder.RecordGameResult(
        currentChart,
        score: 920000,
        accuracy: 96.8f,
        maxCombo: 750,
        perfect: 700,
        great: 40,
        good: 8,
        bad: 2,
        miss: 0,
        isCleared: true
    );
}
```

**주의:** ChartData는 패턴 난이도 정보를 포함하지 않으므로, 레이더 차트 계산에 포함되지 않습니다.

---

## 2. Unity에서 레이더 차트 UI 설정

### 단계 1: Canvas 생성

1. Hierarchy에서 우클릭 → UI → Canvas
2. Canvas Scaler 설정:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

### 단계 2: 레이더 차트 오브젝트 생성

1. Canvas 하위에 새 GameObject 생성 → 이름: "RadarChart"
2. **PatternRadarChart** 컴포넌트 추가
3. RectTransform 설정:
   - Width: 400
   - Height: 400
   - Position: 원하는 위치

### 단계 3: PatternRadarChart 설정

Inspector에서 다음 항목을 설정:

```
[레이더 차트 설정]
Top Songs Count: 50        ← 상위 몇 곡을 계산에 포함할지
Chart Radius: 150          ← 차트 크기 (반지름)
Max Value: 20              ← 최대값 (패턴 난이도 최대값)

[색상 설정]
Fill Color: (0.2, 0.6, 1, 0.3)      ← 반투명 파란색
Outline Color: (0.2, 0.6, 1, 1)    ← 진한 파란색
Outline Width: 2

[격자선 설정]
Grid Color: (1, 1, 1, 0.2)         ← 반투명 흰색
Grid Levels: 4                      ← 격자선 개수
Show Grid Lines: ✓

[라벨 설정]
Label Prefab: (텍스트 프리팹)       ← 아래 참조
Label Distance: 180                 ← 라벨과 차트 간 거리
Show Labels: ✓
```

### 단계 4: 라벨 프리팹 생성

1. 새 GameObject 생성 → UI → Text
2. Text 설정:
   - Font Size: 14
   - Color: White
   - Alignment: Center
3. Prefab으로 저장 → "PatternLabel"
4. PatternRadarChart의 Label Prefab에 할당

### 단계 5: 프로필 UI 매니저 설정

1. Canvas에 새 GameObject 생성 → 이름: "ProfileUI"
2. **ProfileUIManager** 컴포넌트 추가
3. UI 요소들 생성 및 할당:

```
[UI 레퍼런스]
Player Name Text: (Text 컴포넌트)
Total Plays Text: (Text 컴포넌트)
Total Clears Text: (Text 컴포넌트)
Total Full Combos Text: (Text 컴포넌트)
Total All Perfects Text: (Text 컴포넌트)
Radar Chart: (PatternRadarChart 컴포넌트)
```

---

## 3. 레이더 차트 계산 로직

### 계산 방식

1. **필터링**: 각 패턴 난이도가 5 이상인 곡만 포함
2. **가중 점수 계산**: `패턴 난이도 × 정확도`
3. **상위 N곡 선택**: 각 패턴별로 가중 점수가 높은 N곡 선택
4. **평균 계산**: 선택된 곡들의 평균 가중 점수 계산

### 예시

플레이어가 다음과 같은 기록을 가지고 있다고 가정:

| 곡 | 트릴 난이도 | 정확도 | 가중 점수 |
|----|------------|--------|----------|
| Song A | 15.0 | 95% | 14.25 |
| Song B | 18.5 | 90% | 16.65 |
| Song C | 12.5 | 98% | 12.25 |

트릴 레이더 점수 = (14.25 + 16.65 + 12.25) / 3 = **14.38**

### 코드로 확인

```csharp
// 레이더 데이터 계산
PatternRadarData radarData = PlayerProfile.Instance.CalculateRadarData(topCount: 50);

// 각 패턴 점수 확인
Debug.Log($"트릴: {radarData.trill}");
Debug.Log($"계단: {radarData.stairs}");
Debug.Log($"동치: {radarData.chord}");
// ...
```

---

## 4. 레이더 차트 업데이트

### 자동 업데이트

`ProfileUIManager`는 1초마다 자동으로 UI를 업데이트합니다.

```csharp
public class ProfileUIManager : MonoBehaviour
{
    public float updateInterval = 1f; // 업데이트 주기
}
```

### 수동 업데이트

```csharp
// 프로필 UI 매니저에서
ProfileUIManager profileUI = GetComponent<ProfileUIManager>();
profileUI.UpdateProfileUI();

// 또는 레이더 차트에서 직접
PatternRadarChart radarChart = GetComponent<PatternRadarChart>();
radarChart.UpdateRadarChart();
```

---

## 5. 설정 변경

### 상위 곡 수 변경

기본값은 50곡이지만, 변경 가능합니다:

```csharp
// 30곡으로 변경
PatternRadarData radarData = PlayerProfile.Instance.CalculateRadarData(topCount: 30);

// 또는 UI에서
radarChart.topSongsCount = 30;
radarChart.UpdateRadarChart();
```

### 패턴 타입 추가

새로운 패턴을 추가하려면:

1. **PatternDifficulty.cs** 수정 (CHART_FORMAT_GUIDE.md 참조)
2. **PlayRecord.cs** - 필드 추가
3. **PlayerProfile.cs** - `CalculatePatternScore()` 수정
4. **PatternRadarData.cs** - 필드 및 메서드 수정
5. **PatternRadarChart.cs** - 자동으로 패턴 개수에 맞춰 다각형 생성

---

## 6. 데이터 관리

### 프로필 저장 위치

```
Application.persistentDataPath/player_profile.json
```

- **Windows**: `C:/Users/[사용자]/AppData/LocalLow/[회사명]/[게임명]/`
- **Mac**: `~/Library/Application Support/[회사명]/[게임명]/`
- **Android**: `/storage/emulated/0/Android/data/[패키지명]/files/`

### 프로필 초기화

```csharp
PlayerProfile.Instance.Reset();
```

**경고**: 모든 플레이 기록이 삭제됩니다!

### 수동 저장

```csharp
PlayerProfile.Instance.Save();
```

일반적으로는 `AddPlayRecord()` 호출 시 자동으로 저장됩니다.

---

## 7. 테스트

### 더미 데이터 생성

```csharp
// GameResultRecorder에 컴포넌트 추가 후
// Inspector에서 우클릭 → Generate Test Records
```

10개의 테스트 플레이 기록이 생성됩니다.

### 레이더 차트 테스트 데이터

```csharp
// PatternRadarChart 컴포넌트에서
// Inspector에서 우클릭 → Generate Test Data
```

랜덤한 레이더 데이터가 생성되어 즉시 표시됩니다.

---

## 8. 고급 사용

### 특정 곡의 최고 기록 조회

```csharp
PlayRecord bestRecord = PlayerProfile.Instance.GetBestRecord(
    songName: "Synthesis",
    difficulty: "Hard",
    keyCount: 6
);

if (bestRecord != null)
{
    Debug.Log($"최고 점수: {bestRecord.score}점");
    Debug.Log($"정확도: {bestRecord.accuracy}%");
}
```

### 레이더 데이터 수동 설정

```csharp
PatternRadarData customData = new PatternRadarData
{
    trill = 15.5f,
    stairs = 12.0f,
    chord = 18.5f,
    denim = 10.5f,
    jacks = 16.0f,
    longNoteHybrid = 14.5f,
    burst = 17.0f,
    offbeat = 11.5f
};

radarChart.SetData(customData);
```

### 통계 조회

```csharp
PlayerProfile profile = PlayerProfile.Instance;

Debug.Log($"총 플레이: {profile.totalPlays}회");
Debug.Log($"클리어율: {(float)profile.totalClears / profile.totalPlays * 100f}%");
Debug.Log($"풀콤보: {profile.totalFullCombos}회");
Debug.Log($"올퍼펙: {profile.totalAllPerfects}회");
```

---

## 9. 커스터마이징

### 레이더 차트 스타일 변경

```csharp
// 색상 변경
radarChart.fillColor = new Color(1f, 0.5f, 0.2f, 0.3f);  // 주황색
radarChart.outlineColor = new Color(1f, 0.5f, 0.2f, 1f);

// 크기 변경
radarChart.chartRadius = 200f;

// 격자선 개수 변경
radarChart.gridLevels = 5;
```

### 라벨 스타일 변경

라벨 프리팹의 Text 컴포넌트를 수정하세요:
- 폰트
- 크기
- 색상
- 아웃라인

---

## 10. 문제 해결

### Q: 레이더 차트가 표시되지 않아요

**A:** 다음을 확인하세요:
1. Canvas가 Screen Space - Overlay로 설정되어 있나요?
2. PatternRadarChart의 Raycast Target이 체크되어 있나요?
3. 플레이 기록이 최소 1개 이상 있나요?

### Q: 레이더 점수가 모두 0이에요

**A:** 다음을 확인하세요:
1. 플레이 기록의 패턴 난이도가 5 이상인가요?
2. `SetPatternDifficultyFromChart()`를 호출했나요?
3. ChartDataNew의 `patternDifficulty` 필드가 설정되어 있나요?

### Q: 상위 50곡이 없으면 어떻게 되나요?

**A:** 있는 만큼만 계산합니다. 10곡만 있으면 10곡의 평균을 계산합니다.

### Q: 패턴을 9개로 늘리고 싶어요

**A:** CHART_FORMAT_GUIDE.md의 "패턴 난이도 타입 추가하기"를 참조하세요.
PatternRadarChart는 자동으로 N각형으로 조정됩니다.

---

## 참고 자료

- **CHART_FORMAT_GUIDE.md** - 메타데이터 및 패턴 추가 가이드
- **사운드 볼텍스 이펙터 레이더** - 원본 시스템 참고

---

**작성일:** 2025-10-27
**버전:** 1.0
**Pattern Radar System Version:** v1.0
