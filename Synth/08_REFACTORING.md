# 코드 리팩토링 (Code Refactoring)

> **최근 업데이트**: 2025-10-27
> **우선순위**: 낮음

[← 메인 TODO로 돌아가기](DEVELOPMENT_TODO.md)

---

## 🔧 코드 리팩토링 (Refactoring)

### 1. 하드코딩된 값 제거

**현재 문제**:
```csharp
// ❌ 하드코딩
if (accuracy < 0.041f) { return JudgmentType.S_Perfect; }

// ✅ 상수화
private const float S_PERFECT_TIMING = 0.041f;
if (accuracy < S_PERFECT_TIMING) { return JudgmentType.S_Perfect; }
```

**수정 필요 파일**:
- `RhythmManager.cs`: 타이밍 윈도우
- `HPSystem.cs`: HP 증감량
- `GearController.cs`: 트랙 크기, 위치

---

### 2. 클래스 책임 분리

**GameManager.cs가 너무 많은 책임을 가짐**:
```
[ ] GameStateManager 분리
    [ ] 게임 상태 관리만 담당

[ ] GameFlowController 분리
    [ ] 씬 전환, 게임 흐름 제어

[ ] GameDataManager 분리
    [ ] 플레이 데이터 저장/로드
```

---

### 3. 매직 넘버 제거
```csharp
// ❌ 매직 넘버
if (hp < 70) { GameOver(); }

// ✅ 명명된 상수
private const int NORMAL_CLEAR_THRESHOLD = 70;
if (hp < NORMAL_CLEAR_THRESHOLD) { GameOver(); }
```

---

### 4. 네이밍 일관성
- `StartGame()` vs `GameStart()` → `StartGame()`로 통일
- `UpdateHP()` vs `OnHPChanged()` → `UpdateHP()`로 통일

---

### 5. 주석 처리된 코드 정리
```
[ ] 완전 삭제
    [ ] ScoreSystem.cs (이미 삭제됨) ✅
    [ ] ChartEditorNew.cs (주석 처리됨) ✅
    [ ] 기타 미사용 코드 블록

[ ] 주석 설명 추가
    [ ] 복잡한 알고리즘
    [ ] 타이밍 계산 로직
```

---

### 6. 문서화 추가
```
[ ] XML 문서 주석
    /// <summary>
    /// 판정을 계산합니다.
    /// </summary>
    /// <param name="timeDifference">타이밍 차이 (초)</param>
    /// <returns>판정 타입</returns>

[ ] README 파일 업데이트
    [ ] 각 시스템별 상세 설명
    [ ] API 문서 생성
```
