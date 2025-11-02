# osu! mania Import 테스트 가이드

osu! mania .osu 파일 Import 기능을 테스트하는 방법입니다.

## 방법 1: Unity 에디터에서 테스트 (권장)

### 단계 1: 테스트 Scene 설정

1. Unity 에디터를 엽니다
2. 새 Scene 생성 또는 기존 Scene 사용
3. Hierarchy에서 빈 GameObject 생성 → 이름: "OsuImportTest"
4. `OsuImportTest` 스크립트를 GameObject에 추가:
   - Inspector → Add Component → "OsuImportTest"

### 단계 2: ChartLoader 설정

`ChartLoader`가 Scene에 있어야 합니다:

```
방법 A: 기존 ChartLoader 사용
- Scene에 ChartLoader가 이미 있으면 그대로 사용

방법 B: 새로 생성
1. 빈 GameObject 생성 → 이름: "ChartLoader"
2. ChartLoader.cs 컴포넌트 추가
```

### 단계 3: 테스트 실행

#### 옵션 A: 자동 실행 (기본)

1. `OsuImportTest` Inspector 확인:
   - **Osu File Path**: `sample_mania.osu` (기본값)
   - **Run On Start**: ✅ 체크됨
2. Play 버튼 클릭 ▶️
3. Console 창에서 결과 확인

#### 옵션 B: 수동 실행

1. `Run On Start` 체크 해제
2. Play 모드로 진입
3. Inspector에서 `OsuImportTest` 컴포넌트 우클릭
4. **Run Test** 선택

#### 옵션 C: 파일 선택

1. Play 모드가 **아닌** 상태에서
2. `OsuImportTest` 컴포넌트 우클릭
3. **Select .osu File** 선택
4. 파일 브라우저에서 .osu 파일 선택
5. Play 모드로 진입하여 테스트

### 단계 4: 결과 확인

Console 창에 다음과 같은 출력이 표시됩니다:

```
===========================================
osu! mania Import Test Started
===========================================

[1] 파일 경로 확인
    입력: sample_mania.osu
    전체 경로: /path/to/synth/sample_mania.osu
    ✅ 파일 존재 확인

[2] osu!mania 모드 검증
    Mode: osu!mania (3)
    ✅ osu!mania 확인

[3] 차트 로드
    ✅ 로드 성공

[4] 메타데이터
    제목: Sample Song
    아티스트: Sample Artist
    제작자: SampleMapper
    난이도: 4K Hard
    출처: (없음)
    태그: test mania 4k
    비트맵 ID: 12345

[5] 난이도 정보
    키 개수: 4K
    레벨: 16.0
    BPM: 120.00

[6] 차트 통계
    총 노트: 16
    롱노트: 4
    일반 노트: 12
    최대 콤보: 16
    노트 밀도: 1.78 notes/sec
    차트 길이: 9.00초

[7] 노트 샘플 (최대 10개)
    [ 0] 일반   | Track 0 |  1.000s
    [ 1] 일반   | Track 1 |  1.500s
    [ 2] 일반   | Track 2 |  2.000s
    [ 3] 일반   | Track 3 |  2.500s
    [ 4] 롱노트 | Track 0 |  3.000s ~  4.000s (1.000s)
    [ 5] 일반   | Track 1 |  3.500s
    [ 6] 일반   | Track 2 |  4.000s
    [ 7] 롱노트 | Track 3 |  4.500s ~  5.500s (1.000s)
    [ 8] 일반   | Track 0 |  5.000s
    [ 9] 일반   | Track 1 |  6.000s

[8] 트랙별 노트 분포
    Track 0:   4개 (25.0%) █████
    Track 1:   4개 (25.0%) █████
    Track 2:   4개 (25.0%) █████
    Track 3:   4개 (25.0%) █████

[9] 유효성 검사
    IsValid(): ✅ 통과

===========================================
✅ osu! mania Import Test PASSED
===========================================
Inspector에서 loadedChart를 확인할 수 있습니다.
```

### 단계 5: Inspector에서 ChartData 확인

1. Play 모드에서 `OsuImportTest` GameObject 선택
2. Inspector 하단의 **Loaded Chart** 펼치기
3. 로드된 차트 데이터 확인:
   - Song Name
   - Artist Name
   - Notes (리스트)
   - 등등

---

## 방법 2: 실제 osu! 비트맵으로 테스트

### 단계 1: osu! 비트맵 다운로드

1. https://osu.ppy.sh/beatmapsets 방문
2. 검색 필터:
   - **Mode**: osu!mania
   - **Key Count**: 4, 5, 6, 7 등 원하는 키 개수
3. 비트맵 선택 후 **Download** 클릭
4. `.osz` 파일 다운로드

### 단계 2: .osz 파일 압축 해제

`.osz` 파일은 ZIP 압축 파일입니다:

```bash
# Windows: 파일 확장자를 .zip으로 변경 후 압축 해제
# Mac/Linux: unzip 사용
unzip beatmap.osz -d beatmap_folder/
```

압축 해제하면:
```
beatmap_folder/
├── audio.mp3
├── bg.jpg
├── Song [Easy].osu
├── Song [Normal].osu
├── Song [Hard].osu
└── Song [Insane].osu
```

### 단계 3: .osu 파일 배치

1. 원하는 난이도의 .osu 파일 선택
2. 프로젝트 루트에 복사:
   ```
   synth/
   ├── sample_mania.osu
   ├── real_beatmap.osu  ← 여기에 배치
   └── Synth/
   ```

### 단계 4: 테스트 실행

1. Unity 에디터에서 `OsuImportTest` Inspector 확인
2. **Osu File Path**를 `real_beatmap.osu`로 변경
3. Play 버튼 클릭 ▶️
4. Console에서 결과 확인

---

## 방법 3: 스크립트로 직접 테스트

Unity Scene 없이 코드로 직접 테스트:

```csharp
using UnityEngine;

public class QuickOsuTest : MonoBehaviour
{
    void Start()
    {
        // 파일 경로
        string osuPath = Application.dataPath + "/../sample_mania.osu";

        // 로드
        ChartData chart = OsuManiaParser.ParseFromFile(osuPath);

        // 결과 출력
        if (chart != null && chart.IsValid())
        {
            Debug.Log($"✅ {chart.songName} - {chart.difficulty}");
            Debug.Log($"   {chart.keyCount}K, {chart.noteCount}개 노트, BPM {chart.bpm}");
        }
        else
        {
            Debug.LogError("❌ 로드 실패");
        }
    }
}
```

---

## 문제 해결

### ❌ "파일이 존재하지 않습니다"

**원인**: 파일 경로가 잘못되었습니다.

**해결**:
1. 파일이 프로젝트 루트에 있는지 확인:
   ```
   synth/sample_mania.osu  ← 여기
   synth/Synth/Assets/...
   ```
2. 절대 경로로 테스트:
   ```
   /Users/yourname/synth/sample_mania.osu (Mac)
   C:/Users/yourname/synth/sample_mania.osu (Windows)
   ```

### ❌ "ChartLoader Instance가 null입니다"

**원인**: ChartLoader가 Scene에 없습니다.

**해결**:
1. Hierarchy에 빈 GameObject 생성
2. ChartLoader.cs 컴포넌트 추가
3. Play 모드 재시작

### ⚠️ "이 파일은 osu!mania가 아닙니다"

**원인**: Mode가 3이 아닙니다.

**확인**:
.osu 파일 열어서 `[General]` 섹션 확인:
```ini
[General]
Mode: 3  ← 3이어야 osu!mania
```

Mode 값:
- 0 = osu!standard
- 1 = osu!taiko
- 2 = osu!catch
- 3 = osu!mania

### ❌ "유효하지 않은 차트 데이터"

**원인**: 차트가 최소 요구사항을 충족하지 못했습니다.

**확인**:
- 노트가 1개 이상 있는지
- keyCount가 4-10 범위인지
- BPM이 0보다 큰지
- 제목/아티스트/오디오 파일명이 있는지

### ❌ "BPM이 0 또는 이상한 값"

**원인**: TimingPoints 섹션에 문제가 있습니다.

**해결**:
.osu 파일의 `[TimingPoints]` 확인:
```ini
[TimingPoints]
0,500,4,2,0,100,1,0  ← uninherited(1) timing point
```

첫 번째 uninherited(마지막에서 두 번째 값이 1) 포인트가 있어야 합니다.

---

## 추가 테스트 시나리오

### 1. 다양한 키 개수 테스트

4K, 5K, 6K, 7K, 8K 맵을 각각 테스트:
```csharp
string[] testFiles = {
    "4k_map.osu",
    "5k_map.osu",
    "6k_map.osu",
    "7k_map.osu"
};

foreach (var file in testFiles)
{
    ChartData chart = ChartLoader.Instance.LoadChartFromFile(file);
    Debug.Log($"{chart.keyCount}K: {chart.noteCount}개 노트");
}
```

### 2. 롱노트 테스트

롱노트가 많은 맵으로 테스트:
```csharp
ChartData chart = /* 로드 */;
float lnRatio = (chart.longNoteCount / (float)chart.noteCount) * 100f;
Debug.Log($"롱노트 비율: {lnRatio:F1}%");
```

### 3. 고난이도 맵 테스트

노트 수가 많은(1000개 이상) 맵 테스트:
```csharp
ChartData chart = /* 로드 */;
Debug.Log($"노트 수: {chart.noteCount}");
Debug.Log($"노트 밀도: {chart.density:F2} notes/sec");
Debug.Log($"파싱 성공: {chart.IsValid()}");
```

### 4. 유니코드 제목 테스트

한글/일본어 제목이 있는 맵 테스트:
```csharp
ChartData chart = /* 로드 */;
Debug.Log($"제목: {chart.songName}");
Debug.Log($"아티스트: {chart.artistName}");
// 깨지지 않고 정상 출력되는지 확인
```

---

## 성능 테스트

### 큰 파일 로딩 시간 측정

```csharp
using System.Diagnostics;

Stopwatch sw = Stopwatch.StartNew();
ChartData chart = ChartLoader.Instance.LoadChartFromFile(osuPath);
sw.Stop();

Debug.Log($"로딩 시간: {sw.ElapsedMilliseconds}ms");
Debug.Log($"노트 수: {chart.noteCount}");
Debug.Log($"평균: {sw.ElapsedMilliseconds / (float)chart.noteCount:F3}ms/note");
```

예상 결과:
- 100 노트: ~5ms
- 1000 노트: ~20ms
- 5000 노트: ~100ms

---

## 자동화 테스트

여러 파일을 한 번에 테스트:

```csharp
[ContextMenu("Batch Test")]
public void BatchTest()
{
    string[] testFiles = System.IO.Directory.GetFiles(
        Application.dataPath + "/../",
        "*.osu"
    );

    Debug.Log($"=== Batch Test: {testFiles.Length}개 파일 ===\n");

    int passed = 0;
    int failed = 0;

    foreach (var file in testFiles)
    {
        ChartData chart = ChartLoader.Instance.LoadChartFromFile(file);

        if (chart != null && chart.IsValid())
        {
            passed++;
            Debug.Log($"✅ {Path.GetFileName(file)}");
        }
        else
        {
            failed++;
            Debug.LogError($"❌ {Path.GetFileName(file)}");
        }
    }

    Debug.Log($"\n결과: {passed}개 성공, {failed}개 실패");
}
```

---

## 다음 단계

테스트가 성공하면:

1. **게임 플레이 통합**
   ```csharp
   ChartData chart = ChartLoader.Instance.LoadChartFromFile("map.osu");
   GameManager.Instance.StartGame(chart);
   ```

2. **차트 선택 UI**
   - osu 맵 목록 표시
   - 난이도 선택
   - 미리듣기

3. **추가 기능**
   - .osz 파일 자동 압축 해제
   - 여러 난이도 동시 로드
   - 배경 이미지 로드

---

**작성일**: 2025-11-02
**버전**: 1.0
