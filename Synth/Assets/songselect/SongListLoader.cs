using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// StreamingAssets/Charts 폴더를 스캔하여 자동으로 곡 목록을 생성하는 시스템
/// </summary>
public class SongListLoader : MonoBehaviour
{
    [Header("데이터베이스")]
    [Tooltip("자동으로 생성된 곡 목록을 저장할 데이터베이스")]
    public SongDatabase songDatabase;

    [Header("설정")]
    [Tooltip("시작 시 자동으로 차트 스캔")]
    public bool scanOnStart = true;

    [Tooltip("차트 폴더 경로 (StreamingAssets 기준)")]
    public string chartFolderPath = "Charts";

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool enableDebugLog = true;

    void Start()
    {
        if (scanOnStart)
        {
            ScanAndLoadSongs();
        }
    }

    /// <summary>
    /// StreamingAssets/Charts 폴더를 스캔하여 곡 목록 생성
    /// </summary>
    public void ScanAndLoadSongs()
    {
        if (songDatabase == null)
        {
            Debug.LogError("SongDatabase가 설정되지 않았습니다!");
            return;
        }

        string fullPath = Path.Combine(Application.streamingAssetsPath, chartFolderPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"차트 폴더가 존재하지 않습니다: {fullPath}");
            return;
        }

        // 기존 곡 목록 초기화
        songDatabase.songs.Clear();

        // 모든 JSON 파일 검색
        string[] chartFiles = Directory.GetFiles(fullPath, "*.json", SearchOption.AllDirectories);

        if (enableDebugLog)
            Debug.Log($"총 {chartFiles.Length}개의 차트 파일을 발견했습니다.");

        // 곡별로 차트 그룹화 (songName + artistName 기준)
        Dictionary<string, List<ChartFileInfo>> songGroups = new Dictionary<string, List<ChartFileInfo>>();

        foreach (string filePath in chartFiles)
        {
            ChartData chartData = LoadChartFile(filePath);
            if (chartData != null && chartData.IsValid())
            {
                string songKey = $"{chartData.songName}_{chartData.artistName}";

                if (!songGroups.ContainsKey(songKey))
                {
                    songGroups[songKey] = new List<ChartFileInfo>();
                }

                songGroups[songKey].Add(new ChartFileInfo
                {
                    chartData = chartData,
                    filePath = GetRelativePath(filePath)
                });
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning($"유효하지 않은 차트 파일: {filePath}");
            }
        }

        // SongData 생성
        int songIndex = 0;
        foreach (var group in songGroups)
        {
            SongData songData = CreateSongDataFromCharts(group.Value, songIndex);
            if (songData != null)
            {
                songDatabase.AddSong(songData);
                songIndex++;

                if (enableDebugLog)
                    Debug.Log($"곡 추가: {songData.title} - {songData.artist} ({songData.difficulties.Count}개 난이도)");
            }
        }

        if (enableDebugLog)
            Debug.Log($"총 {songDatabase.GetSongCount()}곡이 로드되었습니다.");
    }

    /// <summary>
    /// 차트 파일을 로드합니다.
    /// </summary>
    private ChartData LoadChartFile(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            ChartData chartData = ChartData.FromJson(json);
            return chartData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"차트 파일 로드 실패: {filePath}\n{e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 차트 그룹으로부터 SongData 생성
    /// </summary>
    private SongData CreateSongDataFromCharts(List<ChartFileInfo> charts, int songIndex)
    {
        if (charts == null || charts.Count == 0)
            return null;

        // 첫 번째 차트에서 기본 정보 가져오기
        ChartData firstChart = charts[0].chartData;

        SongData songData = new SongData
        {
            songId = $"song_{songIndex:D3}",
            title = firstChart.songName,
            artist = firstChart.artistName,
            audioPath = firstChart.audioFileName,
            bpm = firstChart.bpm,
            isLocked = false
        };

        // 곡 길이 계산 (가장 긴 차트 기준)
        songData.songLength = (float)charts.Max(c => c.chartData.GetChartDuration());

        // 지원하는 키 개수 목록 생성
        HashSet<int> keyCounts = new HashSet<int>();
        foreach (var chart in charts)
        {
            keyCounts.Add(chart.chartData.keyCount);
        }
        songData.supportedKeyCounts = keyCounts.ToList();
        songData.supportedKeyCounts.Sort();

        // 난이도별로 그룹화
        Dictionary<string, List<ChartFileInfo>> difficultyGroups = new Dictionary<string, List<ChartFileInfo>>();
        foreach (var chart in charts)
        {
            string difficulty = chart.chartData.difficulty;
            if (!difficultyGroups.ContainsKey(difficulty))
            {
                difficultyGroups[difficulty] = new List<ChartFileInfo>();
            }
            difficultyGroups[difficulty].Add(chart);
        }

        // DifficultyInfo 생성
        foreach (var diffGroup in difficultyGroups)
        {
            DifficultyInfo diffInfo = new DifficultyInfo
            {
                difficultyName = diffGroup.Key,
                level = diffGroup.Value[0].chartData.level,
                totalNotes = diffGroup.Value.Max(c => c.chartData.GetNoteCount()),
                difficultyColor = GetDifficultyColor(diffGroup.Key)
            };

            // 키 개수별 차트 경로 추가
            foreach (var chart in diffGroup.Value)
            {
                diffInfo.chartPaths.Add(new ChartPathInfo
                {
                    keyCount = chart.chartData.keyCount,
                    chartPath = chart.filePath
                });
            }

            songData.difficulties.Add(diffInfo);
        }

        // 난이도 정렬 (Easy → Normal → Hard → Expert → Master → Special)
        songData.difficulties = SortDifficulties(songData.difficulties);

        return songData;
    }

    /// <summary>
    /// 난이도별 색상 반환
    /// </summary>
    private Color GetDifficultyColor(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "easy":
                return new Color(0.3f, 1f, 0.3f); // 초록
            case "normal":
                return new Color(0.3f, 0.6f, 1f); // 파랑
            case "hard":
                return new Color(1f, 0.8f, 0f); // 노랑
            case "expert":
                return new Color(1f, 0.4f, 0.4f); // 빨강
            case "master":
                return new Color(0.8f, 0.3f, 1f); // 보라
            case "special":
                return new Color(1f, 0.85f, 0.3f); // 금색
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// 난이도 정렬
    /// </summary>
    private List<DifficultyInfo> SortDifficulties(List<DifficultyInfo> difficulties)
    {
        Dictionary<string, int> difficultyOrder = new Dictionary<string, int>
        {
            { "easy", 0 },
            { "normal", 1 },
            { "hard", 2 },
            { "expert", 3 },
            { "master", 4 },
            { "special", 5 }
        };

        return difficulties.OrderBy(d =>
        {
            string lower = d.difficultyName.ToLower();
            return difficultyOrder.ContainsKey(lower) ? difficultyOrder[lower] : 999;
        }).ToList();
    }

    /// <summary>
    /// 절대 경로를 StreamingAssets 기준 상대 경로로 변환
    /// </summary>
    private string GetRelativePath(string absolutePath)
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        if (absolutePath.StartsWith(streamingAssetsPath))
        {
            return absolutePath.Substring(streamingAssetsPath.Length + 1).Replace('\\', '/');
        }
        return absolutePath;
    }

    /// <summary>
    /// 차트 파일 정보 임시 클래스
    /// </summary>
    private class ChartFileInfo
    {
        public ChartData chartData;
        public string filePath;
    }
}
