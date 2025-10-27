# Chart Format Customization Guide

이 문서는 Synth 리듬 게임의 차트 파일 형식을 커스터마이징하는 방법을 설명합니다.

## 목차
- [메타데이터 추가하기](#메타데이터-추가하기)
- [확장자 변경하기](#확장자-변경하기)
- [패턴 난이도 타입 추가하기](#패턴-난이도-타입-추가하기)

---

## 메타데이터 추가하기

새로운 메타데이터 필드를 추가하려면 다음 5개 파일을 수정해야 합니다.

### 1. ChartDataNew.cs (에디터용 차트 데이터)

**위치:** `Synth/Assets/edit/ChartDataNew.cs`

```csharp
// 1. 적절한 Header 섹션에 필드 추가
[Header("기본 음악 정보")]
public string songName = "";
public string artistName = "";
// ✅ 새로운 필드 추가
public string albumName = ""; // 앨범 이름

// 2. 생성자에서 기본값 설정 (필요한 경우)
public ChartDataNew()
{
    notes = new List<NoteData>();
    // ...
    albumName = ""; // 기본값 설정
}

// 3. Clear() 메서드에 초기화 로직 추가
public void Clear()
{
    // ...
    songName = "";
    artistName = "";
    albumName = ""; // ✅ 추가
}
```

### 2. ChartData.cs (플레이용 차트 데이터)

**위치:** `Synth/Assets/Play/ChartData.cs`

ChartDataNew.cs와 동일한 필드를 추가합니다 (동기화 유지).

```csharp
[Header("기본 음악 정보")]
public string songName = "";
public string artistName = "";
public string albumName = ""; // ✅ 추가

public void Clear()
{
    // ...
    albumName = "";
}
```

### 3. CustomChartWriter.cs (저장 로직)

**위치:** `Synth/Assets/Play/CustomChartWriter.cs`

```csharp
private static string GenerateSynthFormat(ChartSystem.ChartDataNew chart)
{
    StringBuilder sb = new StringBuilder();

    // 기본 음악 정보
    sb.AppendLine("[METADATA]");
    sb.AppendLine($"Title: {EscapeValue(chart.songName)}");
    sb.AppendLine($"Artist: {EscapeValue(chart.artistName)}");
    sb.AppendLine($"Album: {EscapeValue(chart.albumName)}"); // ✅ 추가
    // ...
}
```

### 4. CustomChartParser.cs (로드 로직)

**위치:** `Synth/Assets/Play/CustomChartParser.cs`

```csharp
private static void ParseMetadata(ChartSystem.ChartDataNew chart, List<string> lines)
{
    foreach (string line in lines)
    {
        var kvp = ParseKeyValue(line);
        switch (kvp.Key)
        {
            case "Title": chart.songName = UnescapeValue(kvp.Value); break;
            case "Artist": chart.artistName = UnescapeValue(kvp.Value); break;
            case "Album": chart.albumName = UnescapeValue(kvp.Value); break; // ✅ 추가
            // ...
        }
    }
}
```

### 5. ChartLoader.cs (변환 로직)

**위치:** `Synth/Assets/Play/ChartLoader.cs`

두 개의 변환 메서드를 모두 수정합니다.

```csharp
// ChartDataNew → ChartData 변환
private ChartData ConvertFromChartDataNew(ChartSystem.ChartDataNew source)
{
    ChartData chart = new ChartData
    {
        songName = source.songName,
        artistName = source.artistName,
        albumName = source.albumName, // ✅ 추가
        // ...
    };
    return chart;
}

// ChartData → ChartDataNew 변환
private ChartSystem.ChartDataNew ConvertToChartDataNew(ChartData source)
{
    ChartSystem.ChartDataNew chart = new ChartSystem.ChartDataNew
    {
        songName = source.songName,
        artistName = source.artistName,
        albumName = source.albumName, // ✅ 추가
        // ...
    };
    return chart;
}
```

### 완료!

이제 새로운 메타데이터가 `.synth` 파일에 저장되고 로드됩니다.

**결과 .synth 파일:**
```
[METADATA]
Title: Synthesis
Artist: Sample Artist
Album: Greatest Hits  ← 새로 추가됨!
```

---

## 확장자 변경하기

`.synth` 확장자를 원하는 이름으로 변경하는 방법입니다.

### 변경할 파일 목록

1. **CustomChartWriter.cs** - 저장 시 확장자
2. **CustomChartParser.cs** - 주석 업데이트
3. **ChartLoader.cs** - 확장자 인식
4. **ChartEditor.cs** - 에디터 저장 경로

---

### 1. CustomChartWriter.cs

**위치:** `Synth/Assets/Play/CustomChartWriter.cs`

```csharp
private static string GenerateSynthFormat(ChartSystem.ChartDataNew chart)
{
    StringBuilder sb = new StringBuilder();

    // 헤더 - 버전 정보 업데이트
    sb.AppendLine("# Synth Chart Format v1.0");  // ← 원하는 이름으로 변경
    sb.AppendLine($"# Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine();
    // ...
}
```

### 2. ChartLoader.cs

**위치:** `Synth/Assets/Play/ChartLoader.cs`

```csharp
public ChartData LoadChartFromFile(string filePath)
{
    // ...
    string extension = Path.GetExtension(filePath).ToLower();

    // ✅ 확장자 변경 (예: .synth → .chart)
    if (extension == ".chart") // 원래: ".synth"
    {
        return LoadChartFromSynthFile(filePath);
    }
    // ...
}

public void SaveChart(ChartData chart, string fileName, bool useSynthFormat = false)
{
    // ...
    // ✅ 확장자 변경
    string extension = useSynthFormat ? ".chart" : ".json"; // 원래: ".synth"
    // ...
}
```

### 3. ChartEditor.cs

**위치:** `Synth/Assets/edit/ChartEditor.cs`

```csharp
void SaveChart()
{
    // ...
    // ✅ 확장자 변경
    string path = Path.Combine(Application.persistentDataPath, $"{songName}_chart.chart");
    // 원래: $"{songName}_chart.synth"

    // JSON 백업도 파일명 변경 (선택사항)
    string jsonPath = Path.Combine(Application.persistentDataPath, $"{songName}_chart.json");
    // ...
}

public void LoadChart()
{
    // ✅ 확장자 변경
    string synthPath = Path.Combine(Application.persistentDataPath, $"{songName}_chart.chart");
    string jsonPath = Path.Combine(Application.persistentDataPath, $"{songName}_chart.json");
    // ...
}
```

### 4. 주석 및 문서 업데이트

**CustomChartParser.cs** 파일 상단 주석:

```csharp
/// <summary>
/// 커스텀 .chart 파일 형식을 파싱하는 클래스  // ← 변경
/// </summary>
public class CustomChartParser
```

**CustomChartWriter.cs** 파일 상단 주석:

```csharp
/// <summary>
/// 커스텀 .chart 파일 형식으로 차트를 저장하는 클래스  // ← 변경
/// </summary>
public class CustomChartWriter
```

### 확장자 변경 체크리스트

- [ ] `ChartLoader.cs` - LoadChartFromFile() 확장자 체크 (2곳)
- [ ] `ChartLoader.cs` - SaveChart() 확장자 설정
- [ ] `ChartEditor.cs` - SaveChart() 파일 경로
- [ ] `ChartEditor.cs` - LoadChart() 파일 경로
- [ ] `CustomChartWriter.cs` - 헤더 주석
- [ ] `CustomChartParser.cs` - 클래스 주석
- [ ] 이 문서 (CHART_FORMAT_GUIDE.md) 업데이트

---

## 패턴 난이도 타입 추가하기

새로운 패턴 난이도 타입(예: "스크래치")을 추가하는 방법입니다.

### 1. PatternDifficulty.cs 수정

**위치:** `Synth/Assets/edit/PatternDifficulty.cs`

```csharp
[System.Serializable]
public class PatternDifficulty
{
    [Header("패턴별 난이도 (0-10 스케일)")]

    [Tooltip("트릴: 두 트랙에 배치된 노트가 일정 간격으로 번갈아 나오는 구조")]
    [Range(0, 10)] public int trill = 0;

    // ... 기존 패턴들 ...

    // ✅ 새로운 패턴 추가
    [Tooltip("스크래치: DJ 스타일의 스크래치 패턴")]
    [Range(0, 10)] public int scratch = 0;

    // 생성자
    public PatternDifficulty()
    {
        // ...
        scratch = 0; // ✅ 추가
    }

    public void Clear()
    {
        // ...
        scratch = 0; // ✅ 추가
    }

    public float GetAverageDifficulty()
    {
        // ✅ 분모 수정 (8 → 9)
        return (trill + stairs + chord + denim + jacks +
                longNoteHybrid + burst + offbeat + scratch) / 9f;
    }

    public int GetMaxDifficulty()
    {
        // ✅ 추가
        return Mathf.Max(trill, stairs, chord, denim, jacks,
                        longNoteHybrid, burst, offbeat, scratch);
    }

    public string ToSynthFormat()
    {
        string result = "[PATTERN_DIFFICULTY]\n";
        result += $"Trill: {trill}\n";
        // ... 기존 패턴들 ...
        result += $"Scratch: {scratch}\n"; // ✅ 추가
        return result;
    }

    public static PatternDifficulty ParseFromSynthFormat(string[] lines)
    {
        PatternDifficulty pd = new PatternDifficulty();

        foreach (string line in lines)
        {
            // ... 기존 패턴들 ...
            else if (line.StartsWith("Scratch:")) // ✅ 추가
                int.TryParse(line.Split(':')[1].Trim(), out pd.scratch);
        }

        return pd;
    }
}
```

### 완료!

이제 차트에서 새로운 패턴 난이도를 설정하고 저장/로드할 수 있습니다.

**결과 .synth 파일:**
```
[PATTERN_DIFFICULTY]
Trill: 7
Stairs: 5
Chord: 8
Denim: 6
Jacks: 9
LongNoteHybrid: 4
Burst: 7
Offbeat: 5
Scratch: 8  ← 새로 추가됨!
```

---

## 추가 팁

### 통계 자동 계산

`noteCount`, `longNoteCount`, `maxCombo`, `density` 같은 통계 필드는 자동으로 계산됩니다.

```csharp
// ChartEditor에서 저장 시
currentChart.UpdateStatistics(); // 자동 계산

// 커스텀 통계 필드 추가 예시
public int customStat = 0;

public void UpdateStatistics()
{
    noteCount = notes.Count;
    // ...

    // ✅ 커스텀 통계 계산
    customStat = CalculateCustomStat();
}

private int CalculateCustomStat()
{
    // 예: 동시치기 노트 개수 계산
    int count = 0;
    // 구현...
    return count;
}
```

### 값 검증

`IsValid()` 메서드에 새로운 검증 로직을 추가할 수 있습니다.

```csharp
// ChartDataNew.cs
public bool IsValid()
{
    if (string.IsNullOrEmpty(songName)) return false;
    if (string.IsNullOrEmpty(audioFileName)) return false;

    // ✅ 커스텀 검증 추가
    if (!string.IsNullOrEmpty(albumName) && albumName.Length > 100)
        return false; // 앨범명 길이 제한

    return true;
}
```

### 섹션 추가

완전히 새로운 섹션을 `.synth` 파일에 추가하려면:

1. **CustomChartWriter.cs**: 섹션 작성 로직 추가
2. **CustomChartParser.cs**: `ProcessSection()` switch문에 케이스 추가
3. **파싱 메서드 구현**: `ParseYourSection()` 메서드 작성

예시:
```csharp
// CustomChartWriter.cs
sb.AppendLine("[CUSTOM_SECTION]");
sb.AppendLine($"CustomField: {chart.customField}");

// CustomChartParser.cs
switch (sectionName)
{
    case "CUSTOM_SECTION":
        ParseCustomSection(chart, lines);
        break;
    // ...
}

private static void ParseCustomSection(ChartSystem.ChartDataNew chart, List<string> lines)
{
    foreach (string line in lines)
    {
        var kvp = ParseKeyValue(line);
        if (kvp.Key == "CustomField")
            chart.customField = kvp.Value;
    }
}
```

---

## 문제 해결

### Q: 메타데이터를 추가했는데 저장/로드가 안 돼요!

**A:** 다음을 확인하세요:
1. ChartDataNew와 ChartData에 모두 필드 추가했나요?
2. CustomChartWriter의 `GenerateSynthFormat()`에 추가했나요?
3. CustomChartParser의 해당 Parse 메서드에 추가했나요?
4. ChartLoader의 두 변환 메서드에 모두 추가했나요?

### Q: 확장자를 변경했는데 파일이 안 열려요!

**A:** 다음을 확인하세요:
1. ChartLoader.cs의 `LoadChartFromFile()`에서 확장자 체크 변경했나요?
2. ChartEditor.cs의 SaveChart/LoadChart 경로 변경했나요?
3. 기존 파일의 확장자도 변경했나요?

### Q: 패턴 난이도가 저장은 되는데 평균/최대값이 이상해요!

**A:** `GetAverageDifficulty()`와 `GetMaxDifficulty()`의 분모를 업데이트했는지 확인하세요.

---

## 버전 관리

파일 형식을 크게 변경할 때는 버전을 올리는 것을 권장합니다:

```csharp
// CustomChartWriter.cs
sb.AppendLine("# Synth Chart Format v2.0"); // v1.0 → v2.0

// CustomChartParser.cs - 버전별 파싱 분기 (선택사항)
if (firstLine.Contains("v2.0"))
{
    // v2.0 파싱 로직
}
else
{
    // v1.0 파싱 로직 (하위 호환성)
}
```

---

## 참고 파일 위치

```
synth/
├── Synth/Assets/
│   ├── Play/
│   │   ├── ChartData.cs           ← 플레이용 데이터
│   │   ├── ChartLoader.cs         ← 로더 (확장자 체크)
│   │   ├── CustomChartParser.cs   ← 파싱
│   │   └── CustomChartWriter.cs   ← 저장
│   └── edit/
│       ├── ChartDataNew.cs        ← 에디터용 데이터
│       ├── ChartEditor.cs         ← 에디터 (저장/로드)
│       └── PatternDifficulty.cs   ← 패턴 난이도
└── CHART_FORMAT_GUIDE.md          ← 이 문서
```

---

**작성일:** 2025-10-27
**버전:** 1.0
**Chart Format Version:** v1.0 (.synth)
