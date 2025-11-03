# osu! mania 실제 적용 가이드

osu! mania .osu 파일을 실제 게임에 적용하는 방법입니다.

## 개요

이미 **osu 파일 Import 기능이 구현되어 있으므로**, 기존 시스템에 바로 연결하면 됩니다.
게임의 ChartLoader가 이미 .osu 파일을 자동으로 감지하고 로드합니다.

---

## 방법 1: 가장 간단한 방법 (즉시 사용)

기존 코드 수정 없이 **파일 경로만 .osu로 변경**

### 단계 1: osu 파일 배치

```
Synth/Assets/StreamingAssets/
└── Charts/
    ├── mymap.osu          ← osu 파일
    └── audio.mp3          ← 오디오 파일 (같은 폴더)
```

### 단계 2: GameManager에서 로드

기존 코드에서 파일 경로만 .osu로 변경:

```csharp
public class GameManager : MonoBehaviour
{
    void Start()
    {
        // 기존: JSON 파일
        // string chartPath = "Charts/mysong.json";

        // 변경: osu 파일 (자동 감지)
        string chartPath = Application.streamingAssetsPath + "/Charts/mymap.osu";

        ChartData chart = ChartLoader.Instance.LoadChartFromFile(chartPath);

        if (chart != null)
        {
            StartGame(chart);
        }
    }
}
```

**끝!** 이제 osu 맵이 바로 플레이됩니다.

---

## 방법 2: 파일 브라우저로 선택

런타임에 사용자가 .osu 파일을 직접 선택하게 하기

### OsuFileSelector.cs 생성

```csharp
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class OsuFileSelector : MonoBehaviour
{
    [Header("UI")]
    public Button selectFileButton;
    public Text filePathText;

    private string selectedOsuPath = "";

    void Start()
    {
        if (selectFileButton != null)
        {
            selectFileButton.onClick.AddListener(SelectOsuFile);
        }
    }

    /// <summary>
    /// .osu 파일 선택 (Windows 파일 탐색기)
    /// </summary>
    public void SelectOsuFile()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // Windows 런타임에서만 작동
        var extensions = new[] {
            new SFB.ExtensionFilter("osu! Beatmap", "osu"),
            new SFB.ExtensionFilter("All Files", "*")
        };

        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel(
            "Select osu! mania beatmap",
            "",
            extensions,
            false
        );

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedOsuPath = paths[0];

            if (filePathText != null)
            {
                filePathText.text = Path.GetFileName(selectedOsuPath);
            }

            Debug.Log($"선택된 파일: {selectedOsuPath}");
        }
#else
        Debug.LogWarning("파일 브라우저는 Windows 빌드에서만 작동합니다.");
#endif
    }

    /// <summary>
    /// 선택된 파일로 게임 시작
    /// </summary>
    public void StartGameWithSelectedFile()
    {
        if (string.IsNullOrEmpty(selectedOsuPath))
        {
            Debug.LogError("파일을 먼저 선택하세요!");
            return;
        }

        // 차트 로드
        ChartData chart = ChartLoader.Instance.LoadChartFromFile(selectedOsuPath);

        if (chart != null && chart.IsValid())
        {
            // GameManager를 통해 게임 시작
            GameManager.Instance.StartGame(chart);
        }
        else
        {
            Debug.LogError("차트 로드 실패!");
        }
    }
}
```

**필요한 패키지**: [StandaloneFileBrowser](https://github.com/gkngkc/UnityStandaloneFileBrowser)
```
Unity Package Manager → Add package from git URL:
https://github.com/gkngkc/UnityStandaloneFileBrowser.git
```

---

## 방법 3: 폴더 스캔 (곡 목록 표시)

특정 폴더의 모든 .osu 파일을 자동으로 스캔하여 목록 표시

### OsuBeatmapScanner.cs 생성

```csharp
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// osu! 비트맵 정보
/// </summary>
[System.Serializable]
public class OsuBeatmapInfo
{
    public string filePath;
    public string title;
    public string artist;
    public string difficulty;
    public int keyCount;
    public float level;
    public string creator;
}

/// <summary>
/// 폴더에서 .osu 파일 스캔 및 관리
/// </summary>
public class OsuBeatmapScanner : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("osu 맵이 있는 폴더 경로")]
    public string beatmapFolderPath = "OsuMaps";

    [Header("결과")]
    public List<OsuBeatmapInfo> scannedBeatmaps = new List<OsuBeatmapInfo>();

    // Singleton
    public static OsuBeatmapScanner Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ScanBeatmaps();
    }

    /// <summary>
    /// 비트맵 폴더 스캔
    /// </summary>
    [ContextMenu("Scan Beatmaps")]
    public void ScanBeatmaps()
    {
        scannedBeatmaps.Clear();

        // 폴더 경로 결정
        string fullPath = Path.Combine(Application.streamingAssetsPath, beatmapFolderPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"OsuBeatmapScanner: 폴더가 존재하지 않습니다 - {fullPath}");
            return;
        }

        // .osu 파일 검색
        string[] osuFiles = Directory.GetFiles(fullPath, "*.osu", SearchOption.AllDirectories);

        Debug.Log($"OsuBeatmapScanner: {osuFiles.Length}개의 .osu 파일 발견");

        // 각 파일 정보 추출
        foreach (string filePath in osuFiles)
        {
            OsuBeatmapInfo info = ExtractBeatmapInfo(filePath);
            if (info != null)
            {
                scannedBeatmaps.Add(info);
            }
        }

        // 정렬 (제목 기준)
        scannedBeatmaps = scannedBeatmaps.OrderBy(b => b.title).ToList();

        Debug.Log($"OsuBeatmapScanner: {scannedBeatmaps.Count}개 비트맵 스캔 완료");
    }

    /// <summary>
    /// .osu 파일에서 메타데이터 추출 (빠른 스캔)
    /// </summary>
    private OsuBeatmapInfo ExtractBeatmapInfo(string filePath)
    {
        try
        {
            OsuBeatmapInfo info = new OsuBeatmapInfo();
            info.filePath = filePath;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool inMetadata = false;
                bool inDifficulty = false;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    // 섹션 체크
                    if (line == "[Metadata]")
                    {
                        inMetadata = true;
                        inDifficulty = false;
                        continue;
                    }
                    else if (line == "[Difficulty]")
                    {
                        inMetadata = false;
                        inDifficulty = true;
                        continue;
                    }
                    else if (line.StartsWith("["))
                    {
                        inMetadata = false;
                        inDifficulty = false;
                        continue;
                    }

                    // 메타데이터 파싱
                    if (inMetadata)
                    {
                        if (line.StartsWith("Title:"))
                            info.title = line.Substring(6).Trim();
                        else if (line.StartsWith("TitleUnicode:"))
                            info.title = line.Substring(13).Trim();
                        else if (line.StartsWith("Artist:"))
                            info.artist = line.Substring(7).Trim();
                        else if (line.StartsWith("ArtistUnicode:"))
                            info.artist = line.Substring(14).Trim();
                        else if (line.StartsWith("Creator:"))
                            info.creator = line.Substring(8).Trim();
                        else if (line.StartsWith("Version:"))
                            info.difficulty = line.Substring(8).Trim();
                    }

                    // 난이도 정보 파싱
                    if (inDifficulty)
                    {
                        if (line.StartsWith("CircleSize:"))
                        {
                            float cs;
                            if (float.TryParse(line.Substring(11).Trim(), out cs))
                            {
                                info.keyCount = Mathf.RoundToInt(cs);
                            }
                        }
                        else if (line.StartsWith("OverallDifficulty:"))
                        {
                            float od;
                            if (float.TryParse(line.Substring(18).Trim(), out od))
                            {
                                info.level = od * 2f;
                            }
                        }
                    }

                    // 충분한 정보를 얻었으면 중단
                    if (!string.IsNullOrEmpty(info.title) &&
                        !string.IsNullOrEmpty(info.difficulty) &&
                        info.keyCount > 0)
                    {
                        break;
                    }
                }
            }

            return info;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"OsuBeatmapScanner: 파일 읽기 실패 - {filePath}\n{e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 특정 조건으로 비트맵 검색
    /// </summary>
    public List<OsuBeatmapInfo> FindBeatmaps(int keyCount = -1, string searchQuery = "")
    {
        var results = scannedBeatmaps.AsEnumerable();

        // 키 개수 필터
        if (keyCount > 0)
        {
            results = results.Where(b => b.keyCount == keyCount);
        }

        // 검색어 필터
        if (!string.IsNullOrEmpty(searchQuery))
        {
            searchQuery = searchQuery.ToLower();
            results = results.Where(b =>
                b.title.ToLower().Contains(searchQuery) ||
                b.artist.ToLower().Contains(searchQuery)
            );
        }

        return results.ToList();
    }

    /// <summary>
    /// 비트맵 로드
    /// </summary>
    public ChartData LoadBeatmap(OsuBeatmapInfo info)
    {
        if (info == null || string.IsNullOrEmpty(info.filePath))
        {
            Debug.LogError("OsuBeatmapScanner: 유효하지 않은 비트맵 정보");
            return null;
        }

        return ChartLoader.Instance.LoadChartFromFile(info.filePath);
    }
}
```

### 사용 예시

```csharp
// 모든 비트맵 가져오기
List<OsuBeatmapInfo> allMaps = OsuBeatmapScanner.Instance.scannedBeatmaps;

// 4K 맵만 필터링
List<OsuBeatmapInfo> fourKeyMaps = OsuBeatmapScanner.Instance.FindBeatmaps(keyCount: 4);

// 검색
List<OsuBeatmapInfo> searchResults = OsuBeatmapScanner.Instance.FindBeatmaps(searchQuery: "freedom dive");

// 비트맵 로드 및 게임 시작
OsuBeatmapInfo selectedMap = fourKeyMaps[0];
ChartData chart = OsuBeatmapScanner.Instance.LoadBeatmap(selectedMap);
GameManager.Instance.StartGame(chart);
```

---

## 방법 4: 기존 곡 선택 UI에 통합

**SongSelectionManager.cs** 수정하여 osu 맵 표시

### 수정 방법

```csharp
public class SongSelectionManager : MonoBehaviour
{
    // 기존 코드...

    [Header("osu! Beatmap Support")]
    public bool useOsuMaps = false;
    private List<OsuBeatmapInfo> osuBeatmaps;
    private int currentOsuMapIndex = 0;

    void Start()
    {
        // 기존 코드...

        // osu 맵 로드
        if (useOsuMaps)
        {
            LoadOsuBeatmaps();
        }
    }

    private void LoadOsuBeatmaps()
    {
        if (OsuBeatmapScanner.Instance != null)
        {
            osuBeatmaps = OsuBeatmapScanner.Instance.FindBeatmaps(keyCount: currentKeyCount);

            if (osuBeatmaps.Count > 0)
            {
                UpdateOsuMapUI();
            }
        }
    }

    private void UpdateOsuMapUI()
    {
        if (osuBeatmaps == null || osuBeatmaps.Count == 0)
            return;

        var currentMap = osuBeatmaps[currentOsuMapIndex];

        if (songTitleText != null)
            songTitleText.text = currentMap.title;

        if (artistText != null)
            artistText.text = currentMap.artist;

        if (difficultyText != null)
            difficultyText.text = currentMap.difficulty;

        if (keyCountText != null)
            keyCountText.text = $"{currentMap.keyCount}K";
    }

    // 곡 선택 버튼
    private void OnSelectSong()
    {
        if (useOsuMaps && osuBeatmaps != null && osuBeatmaps.Count > 0)
        {
            var selectedMap = osuBeatmaps[currentOsuMapIndex];
            ChartData chart = OsuBeatmapScanner.Instance.LoadBeatmap(selectedMap);

            if (chart != null)
            {
                // GameManager에 전달하고 Play 씬으로 이동
                PlayerPrefs.SetString("LoadedChartPath", selectedMap.filePath);
                SceneManager.LoadScene("PlayScene");
            }
        }
        else
        {
            // 기존 로직...
        }
    }
}
```

### PlayScene의 GameManager에서 로드

```csharp
public class GameManager : MonoBehaviour
{
    void Start()
    {
        // PlayerPrefs에서 선택된 차트 경로 가져오기
        string chartPath = PlayerPrefs.GetString("LoadedChartPath", "");

        if (!string.IsNullOrEmpty(chartPath) && File.Exists(chartPath))
        {
            ChartData chart = ChartLoader.Instance.LoadChartFromFile(chartPath);

            if (chart != null)
            {
                StartGame(chart);
            }
        }
    }
}
```

---

## 방법 5: .osz 파일 자동 압축 해제 (고급)

.osz 파일(ZIP)을 자동으로 압축 해제하여 사용

### OszExtractor.cs 생성

```csharp
using UnityEngine;
using System.IO;
using System.IO.Compression;

public class OszExtractor : MonoBehaviour
{
    /// <summary>
    /// .osz 파일 압축 해제
    /// </summary>
    public static string ExtractOsz(string oszPath, string outputFolder = "")
    {
        if (!File.Exists(oszPath))
        {
            Debug.LogError($"OszExtractor: 파일이 존재하지 않습니다 - {oszPath}");
            return null;
        }

        try
        {
            // 출력 폴더 설정
            if (string.IsNullOrEmpty(outputFolder))
            {
                string oszName = Path.GetFileNameWithoutExtension(oszPath);
                outputFolder = Path.Combine(Application.streamingAssetsPath, "OsuMaps", oszName);
            }

            // 이미 압축 해제되어 있으면 스킵
            if (Directory.Exists(outputFolder))
            {
                Debug.Log($"OszExtractor: 이미 압축 해제됨 - {outputFolder}");
                return outputFolder;
            }

            // 압축 해제
            Debug.Log($"OszExtractor: 압축 해제 중... {oszPath}");
            ZipFile.ExtractToDirectory(oszPath, outputFolder);
            Debug.Log($"OszExtractor: 압축 해제 완료 - {outputFolder}");

            return outputFolder;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OszExtractor: 압축 해제 실패 - {e.Message}");
            return null;
        }
    }
}
```

### 사용 예시

```csharp
// .osz 파일 선택
string oszPath = "/path/to/beatmap.osz";

// 압축 해제
string extractedFolder = OszExtractor.ExtractOsz(oszPath);

if (!string.IsNullOrEmpty(extractedFolder))
{
    // 압축 해제된 폴더에서 .osu 파일 찾기
    string[] osuFiles = Directory.GetFiles(extractedFolder, "*.osu");

    foreach (string osuFile in osuFiles)
    {
        Debug.Log($"발견: {Path.GetFileName(osuFile)}");
    }
}
```

---

## 권장 적용 순서

### 단계 1: 기본 테스트 (5분)
✅ **방법 1** 사용 - 파일 경로만 .osu로 변경하여 즉시 테스트

### 단계 2: 폴더 스캔 (30분)
✅ **방법 3** 구현 - OsuBeatmapScanner로 맵 목록 관리

### 단계 3: UI 통합 (1시간)
✅ **방법 4** 구현 - 기존 곡 선택 UI에 osu 맵 통합

### 단계 4: 고급 기능 (선택사항)
- **방법 2**: 파일 브라우저
- **방법 5**: .osz 자동 압축 해제

---

## 폴더 구조 예시

```
Synth/Assets/StreamingAssets/
└── OsuMaps/
    ├── Freedom Dive/
    │   ├── audio.mp3
    │   ├── bg.jpg
    │   ├── Freedom Dive [4K Easy].osu
    │   ├── Freedom Dive [4K Hard].osu
    │   └── Freedom Dive [4K Insane].osu
    │
    ├── Galaxy Collapse/
    │   ├── audio.mp3
    │   ├── Galaxy Collapse [7K Normal].osu
    │   └── Galaxy Collapse [7K Expert].osu
    │
    └── ...
```

---

## 문제 해결

### Q: osu 맵이 로드되지 않습니다

**A:** 체크리스트:
1. 파일이 osu!mania 모드인지 확인 (Mode: 3)
2. CircleSize가 설정되어 있는지 확인
3. TimingPoints에 BPM 정보가 있는지 확인
4. 파일 경로가 올바른지 확인

### Q: 오디오가 재생되지 않습니다

**A:** .osu 파일과 오디오 파일이 같은 폴더에 있는지 확인:
```
AudioFilename: audio.mp3  ← .osu 파일 내용
```

### Q: 여러 난이도를 어떻게 구분하나요?

**A:** 같은 곡의 여러 난이도는 Version 필드로 구분됩니다:
```
Version: 4K Easy
Version: 4K Hard
Version: 4K Insane
```

---

## 다음 단계

1. **프리뷰 재생**: 선택 화면에서 곡 미리듣기
2. **배경 이미지**: .osu의 배경 이미지 로드
3. **랭킹 시스템**: osu 맵 플레이 기록 저장
4. **패턴 난이도**: osu 맵 패턴 자동 분석

---

**작성일**: 2025-11-02
**버전**: 1.0
