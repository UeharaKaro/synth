# osu! mania Import Guide

osu! mania의 .osu 파일을 Synth 시스템으로 가져오는 가이드입니다.

## 개요

Synth는 osu! mania의 비트맵 파일(.osu)을 직접 읽어서 플레이할 수 있습니다.
기존 osu! mania 차트를 변환 없이 바로 사용할 수 있습니다.

## 지원되는 기능

### ✅ 완전 지원
- **메타데이터**: 곡 제목, 아티스트, 난이도명, 제작자
- **키 개수**: 4K, 5K, 6K, 7K, 8K, 9K, 10K
- **노트 타입**: 일반 노트 (Circle), 롱노트 (Hold Note)
- **타이밍**: BPM, 노트 타이밍
- **유니코드**: 한글/일본어 등 유니코드 제목/아티스트 지원

### ⚠️ 제한 사항
- **BPM 변경**: 첫 번째 BPM만 사용 (BPM 변경 무시)
- **SV 변경**: 슬라이더 속도 변경 무시
- **히트사운드**: 키음 정보 손실
- **패턴 난이도**: osu의 난이도 정보를 Synth의 패턴 난이도로 변환 불가
- **스토리보드**: 배경 영상/이미지 정보 손실

## 사용 방법

### 1. Unity 에디터에서 사용

```csharp
using UnityEngine;

public class OsuImportExample : MonoBehaviour
{
    void Start()
    {
        string osuFilePath = "/path/to/beatmap.osu";

        ChartData chart = ChartLoader.Instance.LoadChartFromFile(osuFilePath);

        if (chart != null)
        {
            Debug.Log($"로드 성공: {chart.songName} - {chart.artistName}");
            Debug.Log($"난이도: {chart.difficulty} ({chart.keyCount}K)");
            Debug.Log($"노트 수: {chart.noteCount}");
        }
    }
}
```

### 2. 런타임에서 사용

```csharp
// osu 파일 경로
string osuPath = Application.persistentDataPath + "/Charts/mymap.osu";

// 파일 로드
ChartData chart = ChartLoader.Instance.LoadChartFromFile(osuPath);

if (chart != null && chart.IsValid())
{
    // 게임 플레이 시작
    StartGame(chart);
}
```

### 3. osu! mania 모드 검증

.osu 파일이 mania 모드인지 먼저 확인:

```csharp
bool isMania = OsuManiaParser.IsManiaMode(osuFilePath);

if (isMania)
{
    ChartData chart = ChartLoader.Instance.LoadChartFromFile(osuFilePath);
}
else
{
    Debug.LogWarning("이 파일은 osu!mania 차트가 아닙니다.");
}
```

## 변환 상세

### 메타데이터 매핑

| osu! | Synth ChartData | 설명 |
|------|----------------|------|
| `Title` | `songName` | 곡 제목 |
| `TitleUnicode` | `songName` | 유니코드 제목 (우선) |
| `Artist` | `artistName` | 아티스트 |
| `ArtistUnicode` | `artistName` | 유니코드 아티스트 (우선) |
| `Creator` | `chartAuthor` | 차트 제작자 |
| `Version` | `difficulty` | 난이도명 |
| `Source` | `source` | 출처 |
| `Tags` | `tags` | 태그 |
| `BeatmapID` | `beatmapId` | 비트맵 ID |
| `AudioFilename` | `audioFileName` | 오디오 파일명 |

### 난이도 매핑

| osu! | Synth ChartData | 변환 방법 |
|------|----------------|----------|
| `CircleSize` (1-10) | `keyCount` | 직접 매핑 (반올림) |
| `OverallDifficulty` (0-10) | `level` (0-20) | OD × 2 |

### 타이밍 변환

| osu! | Synth | 변환 공식 |
|------|-------|----------|
| `time` (밀리초) | `timing` (초) | `time / 1000.0` |
| `beatLength` (ms/beat) | `bpm` | `60000 / beatLength` |

### 노트 변환

#### 컬럼(트랙) 계산
```
track = floor(x * keyCount / 512)
```

- `x`: 0-512 범위의 x 좌표
- `keyCount`: 키 개수 (4K, 5K 등)
- `track`: 0부터 시작하는 컬럼 인덱스

예시 (4K):
- x = 0-127 → track 0
- x = 128-255 → track 1
- x = 256-383 → track 2
- x = 384-511 → track 3

#### 노트 타입

| osu! type | Synth | 설명 |
|-----------|-------|------|
| `type & 1` | 일반 노트 | Circle |
| `type & 128` | 롱노트 | Hold Note |

```csharp
// osu 형식
256,192,1000,1,0,0:0:0:0:
// → Synth: 일반 노트, track=1, time=1.0초

256,192,2000,128,0,3000:0:0:0:0:
// → Synth: 롱노트, track=1, start=2.0초, end=3.0초
```

## 예제

### 샘플 .osu 파일

프로젝트에 `sample_mania.osu` 파일이 포함되어 있습니다.

```
Title: Sample Song
Artist: Sample Artist
Version: 4K Hard
CircleSize: 4 (4K)
OverallDifficulty: 8 (레벨 16으로 변환)
BPM: 120 (beatLength=500ms)
노트: 16개 (일반 12개, 롱노트 4개)
```

### 테스트 코드

```csharp
using UnityEngine;

public class OsuImportTest : MonoBehaviour
{
    void Start()
    {
        TestOsuImport();
    }

    void TestOsuImport()
    {
        string osuPath = Application.dataPath + "/../sample_mania.osu";

        Debug.Log("=== osu! mania Import Test ===");

        // 1. mania 모드 확인
        if (!OsuManiaParser.IsManiaMode(osuPath))
        {
            Debug.LogError("mania 모드가 아닙니다!");
            return;
        }

        // 2. 차트 로드
        ChartData chart = ChartLoader.Instance.LoadChartFromFile(osuPath);

        if (chart == null)
        {
            Debug.LogError("차트 로드 실패!");
            return;
        }

        // 3. 결과 출력
        Debug.Log($"제목: {chart.songName}");
        Debug.Log($"아티스트: {chart.artistName}");
        Debug.Log($"난이도: {chart.difficulty}");
        Debug.Log($"키 개수: {chart.keyCount}K");
        Debug.Log($"레벨: {chart.level}");
        Debug.Log($"BPM: {chart.bpm}");
        Debug.Log($"총 노트: {chart.noteCount}");
        Debug.Log($"롱노트: {chart.longNoteCount}");

        // 4. 노트 상세 정보
        Debug.Log("\n=== 노트 목록 ===");
        for (int i = 0; i < Mathf.Min(5, chart.notes.Count); i++)
        {
            var note = chart.notes[i];
            string type = note.isLongNote ? "롱노트" : "일반";
            string timing = note.isLongNote
                ? $"{note.timing:F3}s ~ {note.longNoteEndTiming:F3}s"
                : $"{note.timing:F3}s";
            Debug.Log($"[{i}] {type}, Track {note.track}, {timing}");
        }

        Debug.Log("=== Import 성공! ===");
    }
}
```

## osu! 비트맵 다운로드

1. **osu! 공식 사이트**: https://osu.ppy.sh/beatmapsets
2. **검색**: "mania" 필터 적용
3. **다운로드**: .osz 파일 다운로드
4. **압축 해제**: .osz는 ZIP 파일입니다. 압축 해제하면 .osu 파일들이 나옵니다.
5. **Import**: Synth에서 .osu 파일 로드

### .osz 파일 구조
```
beatmap.osz (ZIP)
├── audio.mp3
├── bg.jpg
├── Song [Easy].osu
├── Song [Normal].osu
├── Song [Hard].osu
└── Song [Insane].osu
```

각 .osu 파일은 하나의 난이도를 나타냅니다.

## 파일 배치

### StreamingAssets 사용 (권장)
```
Synth/Assets/StreamingAssets/
└── Charts/
    ├── mymap.osu
    └── audio.mp3
```

### persistentDataPath 사용
```
Application.persistentDataPath/
└── Charts/
    ├── mymap.osu
    └── audio.mp3
```

## 문제 해결

### Q: "이 파일은 osu!mania가 아닙니다" 경고

**A:** .osu 파일의 `[General]` 섹션에서 `Mode: 3`인지 확인하세요.
- Mode 0 = osu!standard
- Mode 1 = osu!taiko
- Mode 2 = osu!catch
- Mode 3 = osu!mania

### Q: BPM이 이상하게 표시됩니다

**A:** osu는 변속(BPM 변경)을 지원하지만, Synth는 첫 번째 BPM만 사용합니다.
`[TimingPoints]` 섹션의 첫 번째 uninherited timing point를 확인하세요.

### Q: 노트가 잘못된 트랙에 배치됩니다

**A:** CircleSize와 실제 노트 배치가 일치하는지 확인하세요.
osu!mania 에디터에서 올바르게 매핑된 차트인지 확인하세요.

### Q: 롱노트가 일반 노트로 변환됩니다

**A:** HitObject의 type 값이 128인지 확인하세요.
```
x,y,time,128,hitSound,endTime:hitSample
```

### Q: 유니코드 제목이 깨집니다

**A:** .osu 파일이 UTF-8 인코딩인지 확인하세요.
osu! 공식 비트맵은 기본적으로 UTF-8을 사용합니다.

## 기술 세부사항

### OsuManiaParser.cs

주요 메서드:
- `ParseFromFile(string filePath)` - .osu 파일 읽기
- `ParseFromString(string content)` - 문자열 파싱
- `IsManiaMode(string filePath)` - mania 모드 검증

### ChartLoader.cs

확장자 자동 감지:
- `.osu` → OsuManiaParser 사용
- `.synth` → CustomChartParser 사용
- `.json` → JsonUtility 사용

## 참고 자료

- **osu! Wiki**: https://osu.ppy.sh/wiki/en/Client/File_formats/osu_(file_format)
- **osu! Beatmaps**: https://osu.ppy.sh/beatmapsets
- **CHART_FORMAT_GUIDE.md**: Synth 차트 형식 가이드
- **PATTERN_RADAR_GUIDE.md**: 패턴 레이더 시스템 가이드

---

**작성일**: 2025-11-02
**버전**: 1.0
**지원 형식**: osu! file format v14
