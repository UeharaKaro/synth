# 게임플레이 UI 개선 - 설정 가이드

> **작성일**: 2025-01-26  
> **버전**: 1.0  
> **새로 추가된 컴포넌트**: 3개

---

## 📋 목차

1. [개요](#개요)
2. [ProgressDisplay (진행도 표시)](#progressdisplay-진행도-표시)
3. [ScoreDisplay (점수 표시)](#scoredisplay-점수-표시)
4. [HPBarAnimator (개선)](#hpbaranimator-개선)
5. [통합 설정](#통합-설정)
6. [트러블슈팅](#트러블슈팅)

---

## 개요

게임플레이 UI를 개선하여 다음 기능들이 추가되었습니다:

### 🎯 새로운 기능
1. **진행도 표시** (`ProgressDisplay.cs`)
   - 현재 시간 / 전체 시간
   - 진행률 바
   - BPM 표시
   - 퍼센트 표시

2. **점수 표시** (`ScoreDisplay.cs`)
   - 실시간 점수 업데이트
   - 카운트업 애니메이션
   - 천 단위 콤마
   - 점수 증가 팝업

3. **HP 게이지 개선** (`HPBarAnimator.cs`)
   - 클리어 라인 표시
   - HP 퍼센트 텍스트
   - 위험 구간 펄스 효과
   - 클리어 임계값 조정 가능

---

## ProgressDisplay (진행도 표시)

### Unity Editor 설정

#### 1. GameObject 생성
```
Hierarchy → 우클릭 → Create Empty
이름: "ProgressDisplay"
```

#### 2. 컴포넌트 추가
```
Inspector → Add Component → ProgressDisplay
```

#### 3. 설정값 조정

##### **References**
- `Audio Manager`: AudioManager 오브젝트 드래그 (자동 탐색 가능)

##### **UI Position**
- `Bar Position`: `(0, 4, -0.1)` (화면 상단)
- `Bar Width`: `12` (진행도 바 폭)
- `Bar Height`: `0.3` (진행도 바 높이)

##### **Display Settings**
- `Show Time Text`: ✅ 시간 표시 (00:00 / 03:45)
- `Show BPM`: ✅ BPM 표시 (BPM: 120)
- `Show Percentage`: ✅ 퍼센트 표시 (45.3%)

##### **Colors**
- `Bar Background Color`: `RGBA(0.1, 0.1, 0.1, 0.8)` (어두운 배경)
- `Bar Fill Color`: `RGBA(0.2, 0.8, 1.0, 0.9)` (하늘색 진행률)
- `Bar Border Color`: `RGBA(0.5, 0.5, 0.5, 1.0)` (회색 테두리)

---

### 스크립트 사용법

#### 차트 로드 시 총 길이 설정
```csharp
// GameManager.cs에서
ProgressDisplay progressDisplay = FindObjectOfType<ProgressDisplay>();
if (progressDisplay != null)
{
    progressDisplay.SetSongLength(chartData.songLength); // 총 길이 (초)
    progressDisplay.SetBPM(chartData.bpm);
}
```

#### 진행 중 BPM 변경
```csharp
// 변속 구간 진입 시
progressDisplay.SetBPM(newBPM);
```

#### 진행도 바 색상 변경 (선택)
```csharp
// 특정 구간에서 색상 변경
progressDisplay.SetBarColor(Color.red); // 위험 구간
```

---

## ScoreDisplay (점수 표시)

### Unity Editor 설정

#### 1. GameObject 생성
```
Hierarchy → 우클릭 → Create Empty
이름: "ScoreDisplay"
```

#### 2. 컴포넌트 추가
```
Inspector → Add Component → ScoreDisplay
```

#### 3. 설정값 조정

##### **UI Position**
- `Score Position`: `(-5, 4, -0.1)` (화면 좌상단)

##### **Display Settings**
- `Use Comma`: ✅ 천 단위 콤마 (123,456)
- `Show Label`: ✅ "SCORE" 라벨 표시
- `Animate On Increase`: ✅ 점수 증가 시 애니메이션
- `Count Up Speed`: `5` (카운트업 속도)

##### **Colors**
- `Label Color`: `RGBA(0.8, 0.8, 0.8, 1.0)` (회색 라벨)
- `Score Color`: `RGBA(1, 1, 1, 1)` (흰색 점수)
- `Increase Color`: `RGBA(1, 0.8, 0.2, 1)` (골드 플래시)

##### **Visual Effects**
- `Show Score Popup`: ✅ 점수 증가량 팝업 (+500)
- `Popup Duration`: `0.5` (팝업 지속 시간)

---

### 스크립트 사용법

#### 점수 추가
```csharp
// NoteController.cs 또는 RhythmManager.cs에서
ScoreDisplay scoreDisplay = FindObjectOfType<ScoreDisplay>();

// 판정에 따라 점수 추가
switch (judgment)
{
    case JudgmentType.S_Perfect:
        scoreDisplay.AddScore(300);
        break;
    case JudgmentType.Perfect:
        scoreDisplay.AddScore(200);
        break;
    case JudgmentType.Great:
        scoreDisplay.AddScore(100);
        break;
    // ...
}
```

#### 현재 점수 가져오기
```csharp
int currentScore = scoreDisplay.GetScore();
```

#### 점수 초기화 (게임 시작 시)
```csharp
scoreDisplay.ResetScore();
```

#### 점수 색상 변경 (선택)
```csharp
// 특정 조건에서 색상 변경
scoreDisplay.SetScoreColor(Color.yellow); // Full Combo 시
```

---

## HPBarAnimator (개선)

### 새로 추가된 기능

#### 1. 클리어 라인 표시
HP 게이지에 클리어 기준선이 표시됩니다.

**설정**:
- `Show Clear Line`: ✅ 클리어 라인 표시
- `Clear Threshold`: `70` (Normal 모드 기준 70%)
- `Clear Line Color`: `RGBA(0, 1, 0, 0.8)` (초록색)

#### 2. HP 퍼센트 텍스트
HP 게이지 위에 현재 HP가 숫자로 표시됩니다.

**설정**:
- `Show HP Percentage`: ✅ HP 퍼센트 표시
- `HP Text Color`: `RGBA(1, 1, 1, 1)` (흰색)

#### 3. 위험 구간 펄스 효과
HP가 클리어 라인 아래로 내려가면 자동으로 펄스 효과가 작동합니다.

---

### 스크립트 사용법

#### 난이도별 클리어 임계값 설정
```csharp
// GameManager.cs에서
HPBarAnimator hpBarAnimator = FindObjectOfType<HPBarAnimator>();

// 난이도에 따라 클리어 라인 변경
switch (currentDifficulty)
{
    case JudgmentMode.Normal:
        hpBarAnimator.SetClearThreshold(70f);
        break;
    case JudgmentMode.Hard:
        hpBarAnimator.SetClearThreshold(80f);
        break;
    case JudgmentMode.Super:
        hpBarAnimator.SetClearThreshold(90f);
        break;
}
```

---

## 통합 설정

### GameManager.cs 통합 예시

```csharp
public class GameManager : MonoBehaviour
{
    // UI References
    private ProgressDisplay progressDisplay;
    private ScoreDisplay scoreDisplay;
    private HPBarAnimator hpBarAnimator;
    
    void Start()
    {
        // UI 컴포넌트 찾기
        progressDisplay = FindObjectOfType<ProgressDisplay>();
        scoreDisplay = FindObjectOfType<ScoreDisplay>();
        hpBarAnimator = FindObjectOfType<HPBarAnimator>();
    }
    
    public void StartGame(ChartData chart)
    {
        // 진행도 표시 설정
        if (progressDisplay != null)
        {
            progressDisplay.SetSongLength(chart.songLength);
            progressDisplay.SetBPM(chart.bpm);
        }
        
        // 점수 초기화
        if (scoreDisplay != null)
        {
            scoreDisplay.ResetScore();
        }
        
        // HP 클리어 라인 설정
        if (hpBarAnimator != null)
        {
            float clearThreshold = GetClearThreshold(currentMode);
            hpBarAnimator.SetClearThreshold(clearThreshold);
        }
    }
    
    float GetClearThreshold(JudgmentMode mode)
    {
        switch (mode)
        {
            case JudgmentMode.Normal: return 70f;
            case JudgmentMode.Hard: return 80f;
            case JudgmentMode.Super: return 90f;
            default: return 70f;
        }
    }
}
```

---

### NoteController.cs 통합 예시

```csharp
public class NoteController : MonoBehaviour
{
    private ScoreDisplay scoreDisplay;
    
    void Start()
    {
        scoreDisplay = FindObjectOfType<ScoreDisplay>();
    }
    
    public void Hit()
    {
        // 판정 계산
        JudgmentType judgment = CalculateJudgment();
        
        // 점수 추가
        if (scoreDisplay != null)
        {
            int score = GetScoreForJudgment(judgment);
            scoreDisplay.AddScore(score);
        }
    }
    
    int GetScoreForJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return 300;
            case JudgmentType.Perfect: return 200;
            case JudgmentType.Great: return 100;
            case JudgmentType.Good: return 50;
            default: return 0;
        }
    }
}
```

---

## UI 레이아웃 예시

```
화면 구조:

┌────────────────────────────────────────────┐
│  SCORE          [===진행도 바===] 45.3%    │  ← 상단
│  123,456        00:45 / 03:00    BPM: 120  │
├────────────────────────────────────────────┤
│                                            │
│              게임플레이 영역                │
│                                            │
│         HP [████████░░] 80%                │  ← HP 게이지
│            ─── CLEAR ───  (클리어 라인)    │
│                                            │
│              PERFECT!                      │  ← 판정
│                                            │
│              COMBO                         │  ← 콤보
│                45                          │
└────────────────────────────────────────────┘
```

---

## 커스터마이징

### 위치 조정
모든 UI 요소의 위치는 Inspector에서 조정 가능합니다.

**추천 레이아웃**:
- `ProgressDisplay`: `(0, 4, -0.1)` (상단 중앙)
- `ScoreDisplay`: `(-5, 4, -0.1)` (좌상단)
- `HPBarAnimator`: 기존 위치 유지 (GearController 기준)

### 색상 테마
프로젝트 색상에 맞게 조정하세요.

**다크 테마 예시**:
```
진행도 바: 파란색 → RGBA(0.2, 0.8, 1.0, 0.9)
점수: 흰색 → RGBA(1, 1, 1, 1)
HP: 초록→빨강 그라데이션
```

**라이트 테마 예시**:
```
진행도 바: 보라색 → RGBA(0.6, 0.3, 0.9, 0.9)
점수: 검은색 → RGBA(0, 0, 0, 1)
HP: 파랑→빨강 그라데이션
```

---

## 트러블슈팅

### 문제: UI가 표시되지 않음
**해결**:
1. Inspector에서 컴포넌트가 활성화되어 있는지 확인
2. 카메라 위치 확인 (Z축이 UI보다 앞에 있어야 함)
3. TextMeshPro가 임포트되어 있는지 확인

### 문제: 점수가 업데이트되지 않음
**해결**:
1. `ScoreDisplay.AddScore()` 호출 확인
2. NoteController와 ScoreDisplay 연동 확인
3. Console에서 에러 메시지 확인

### 문제: 진행도 바가 움직이지 않음
**해결**:
1. `ProgressDisplay.SetSongLength()` 호출 확인
2. AudioManager가 올바르게 연결되어 있는지 확인
3. AudioManager.IsPlaying이 true인지 확인

### 문제: HP 클리어 라인이 보이지 않음
**해결**:
1. `Show Clear Line` 옵션 활성화 확인
2. HPBarAnimator.Initialize() 호출 확인
3. GearSettings에 HP 색상이 설정되어 있는지 확인

---

## 성능 최적화

### 권장 사항
1. **Update 최소화**: UI 업데이트는 값이 변경될 때만 수행
2. **오브젝트 풀링**: 점수 팝업 등은 재사용 가능
3. **코루틴 관리**: 중복 코루틴 방지

### 모바일 최적화
모바일 디바이스에서는 다음을 고려하세요:
- 텍스트 해상도 낮추기 (fontSize 조정)
- 애니메이션 프레임 수 줄이기
- 파티클 효과 단순화

---

## 다음 단계

### 추가 개선 가능 사항
1. **정확도 표시** (Accuracy %)
2. **판정 통계** (Perfect/Great/Good 실시간 개수)
3. **목표 점수 표시** (S등급 점수선)
4. **미니맵** (노트 밀도 시각화)

---

**작성**: Claude Code  
**버전**: 1.0  
**마지막 업데이트**: 2025-01-26
