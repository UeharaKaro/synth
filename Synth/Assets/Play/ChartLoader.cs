using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// 차트 파일 로딩 및 관리 클래스
/// JSON 형식의 차트 파일을 읽고 파싱하여 게임플레이에 사용
/// </summary>
public class ChartLoader : MonoBehaviour
{
    [Header("차트 경로 설정")]
    [SerializeField] private string chartsFolderPath = "Charts"; // Resources 폴더 기준 상대 경로
    [SerializeField] private bool useStreamingAssets = false; // StreamingAssets 사용 여부

    [Header("현재 로드된 차트")]
    [SerializeField] private ChartData currentChart;

    // Singleton 패턴
    public static ChartLoader Instance { get; private set; }

    // 이벤트
    public delegate void ChartLoadedHandler(ChartData chart);
    public event ChartLoadedHandler OnChartLoaded;

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

    /// <summary>
    /// 차트 파일 로드 (Resources 폴더에서)
    /// </summary>
    /// <param name="chartFileName">차트 파일 이름 (확장자 제외)</param>
    public ChartData LoadChartFromResources(string chartFileName)
    {
        string path = $"{chartsFolderPath}/{chartFileName}";
        TextAsset chartFile = Resources.Load<TextAsset>(path);

        if (chartFile == null)
        {
            Debug.LogError($"ChartLoader: 차트 파일을 찾을 수 없습니다 - {path}");
            return null;
        }

        return ParseChartJson(chartFile.text);
    }

    /// <summary>
    /// 차트 파일 로드 (StreamingAssets 또는 외부 경로에서)
    /// </summary>
    /// <param name="filePath">차트 파일 전체 경로</param>
    public ChartData LoadChartFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"ChartLoader: 차트 파일이 존재하지 않습니다 - {filePath}");
            return null;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            return ParseChartJson(jsonContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: 차트 파일 읽기 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// JSON 문자열 파싱하여 ChartData 생성
    /// </summary>
    private ChartData ParseChartJson(string jsonContent)
    {
        try
        {
            ChartData chart = JsonUtility.FromJson<ChartData>(jsonContent);

            if (chart == null)
            {
                Debug.LogError("ChartLoader: JSON 파싱 실패");
                return null;
            }

            // 차트 유효성 검사
            if (!chart.IsValid())
            {
                Debug.LogError("ChartLoader: 유효하지 않은 차트 데이터");
                return null;
            }

            // 노트 정렬
            chart.SortNotesByTime();

            Debug.Log($"ChartLoader: 차트 로드 성공 - {chart.songName} ({chart.notes.Count}개 노트)");

            currentChart = chart;
            OnChartLoaded?.Invoke(chart);

            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: JSON 파싱 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 노래 선택 데이터로부터 차트 로드
    /// </summary>
    /// <param name="songName">노래 이름</param>
    /// <param name="difficulty">난이도</param>
    /// <param name="keyCount">키 개수</param>
    public ChartData LoadChart(string songName, string difficulty, int keyCount)
    {
        // 파일 이름 형식: SongName_Difficulty_KeyCount.json
        // 예: Synthesis_Hard_6K.json
        string fileName = $"{songName}_{difficulty}_{keyCount}K";

        ChartData chart;
        if (useStreamingAssets)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, chartsFolderPath, $"{fileName}.json");
            chart = LoadChartFromFile(filePath);
        }
        else
        {
            chart = LoadChartFromResources(fileName);
        }

        return chart;
    }

    /// <summary>
    /// 현재 로드된 차트 반환
    /// </summary>
    public ChartData GetCurrentChart()
    {
        return currentChart;
    }

    /// <summary>
    /// 차트 저장 (에디터용 또는 런타임 생성 차트)
    /// </summary>
    public void SaveChart(ChartData chart, string fileName)
    {
        if (chart == null)
        {
            Debug.LogError("ChartLoader: 저장할 차트가 없습니다");
            return;
        }

        try
        {
            string json = chart.ToJson();
            string savePath;

            if (useStreamingAssets)
            {
                savePath = Path.Combine(Application.streamingAssetsPath, chartsFolderPath, $"{fileName}.json");
            }
            else
            {
                // Resources 폴더는 런타임에 쓰기 불가하므로 persistentDataPath 사용
                string chartDir = Path.Combine(Application.persistentDataPath, chartsFolderPath);
                if (!Directory.Exists(chartDir))
                {
                    Directory.CreateDirectory(chartDir);
                }
                savePath = Path.Combine(chartDir, $"{fileName}.json");
            }

            File.WriteAllText(savePath, json);
            Debug.Log($"ChartLoader: 차트 저장 성공 - {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: 차트 저장 실패 - {e.Message}");
        }
    }

    /// <summary>
    /// 샘플 차트 생성 (테스트용)
    /// </summary>
    public ChartData CreateSampleChart()
    {
        ChartData chart = new ChartData
        {
            songName = "Sample Song",
            artistName = "Sample Artist",
            audioFileName = "sample_audio.mp3",
            bpm = 120f,
            offset = 0f,
            difficulty = "Normal",
            keyCount = 4,
            level = 5
        };

        // 간단한 테스트 노트 생성 (4비트마다 노트 배치)
        float beatInterval = 60f / chart.bpm; // 1비트 = 0.5초 (120 BPM 기준)

        for (int i = 0; i < 32; i++) // 8마디 (4/4박자)
        {
            int track = i % 4; // 4개 트랙에 순환 배치
            double timing = i * beatInterval + 2.0; // 2초 후부터 시작

            NoteData note = new NoteData(
                timing,
                track,
                KeySoundType.None,
                false,
                0
            );

            chart.AddNote(note);
        }

        // 롱노트 몇 개 추가
        chart.AddNote(new NoteData(16.0, 1, KeySoundType.None, true, 18.0));
        chart.AddNote(new NoteData(20.0, 2, KeySoundType.None, true, 22.0));

        Debug.Log($"ChartLoader: 샘플 차트 생성 - {chart.GetNoteCount()}개 노트");

        currentChart = chart;
        return chart;
    }
}
