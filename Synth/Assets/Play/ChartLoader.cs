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
    [SerializeField] private string chartsFolderPath = "Charts"; // StreamingAssets 기준 상대 경로
    [SerializeField] private bool useStreamingAssets = true; // StreamingAssets 사용 (기본값: true)

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
    /// JSON 또는 .synth 형식 자동 감지
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
            string extension = Path.GetExtension(filePath).ToLower();

            // .synth 파일인 경우
            if (extension == ".synth")
            {
                return LoadChartFromSynthFile(filePath);
            }
            // .osu 파일인 경우 (osu! mania)
            else if (extension == ".osu")
            {
                return LoadChartFromOsuFile(filePath);
            }
            // JSON 파일인 경우
            else if (extension == ".json")
            {
                string jsonContent = File.ReadAllText(filePath);
                return ParseChartJson(jsonContent);
            }
            // 확장자가 없거나 다른 경우, JSON으로 시도
            else
            {
                Debug.LogWarning($"ChartLoader: 알 수 없는 파일 형식 ({extension}), JSON으로 시도합니다");
                string jsonContent = File.ReadAllText(filePath);
                return ParseChartJson(jsonContent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: 차트 파일 읽기 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// .synth 파일을 로드하여 ChartData로 변환합니다
    /// </summary>
    private ChartData LoadChartFromSynthFile(string filePath)
    {
        try
        {
            // CustomChartParser를 사용하여 .synth 파일 파싱
            ChartSystem.ChartDataNew chartDataNew = CustomChartParser.ParseFromFile(filePath);

            if (chartDataNew == null)
            {
                Debug.LogError("ChartLoader: .synth 파일 파싱 실패");
                return null;
            }

            // ChartDataNew를 ChartData로 변환
            ChartData chart = ConvertFromChartDataNew(chartDataNew);

            if (chart == null || !chart.IsValid())
            {
                Debug.LogError("ChartLoader: 유효하지 않은 차트 데이터");
                return null;
            }

            chart.SortNotesByTime();

            Debug.Log($"ChartLoader: .synth 차트 로드 성공 - {chart.songName} ({chart.notes.Count}개 노트)");

            currentChart = chart;
            OnChartLoaded?.Invoke(chart);

            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: .synth 파일 로드 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// .osu 파일을 로드하여 ChartData로 변환합니다 (osu! mania)
    /// </summary>
    private ChartData LoadChartFromOsuFile(string filePath)
    {
        try
        {
            // OsuManiaParser를 사용하여 .osu 파일 파싱
            ChartData chart = OsuManiaParser.ParseFromFile(filePath);

            if (chart == null)
            {
                Debug.LogError("ChartLoader: .osu 파일 파싱 실패");
                return null;
            }

            if (!chart.IsValid())
            {
                Debug.LogError("ChartLoader: 유효하지 않은 차트 데이터");
                return null;
            }

            chart.SortNotesByTime();

            Debug.Log($"ChartLoader: .osu 차트 로드 성공 - {chart.songName} [{chart.difficulty}] ({chart.notes.Count}개 노트, {chart.keyCount}K)");

            currentChart = chart;
            OnChartLoaded?.Invoke(chart);

            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: .osu 파일 로드 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ChartDataNew를 ChartData로 변환합니다
    /// </summary>
    private ChartData ConvertFromChartDataNew(ChartSystem.ChartDataNew source)
    {
        ChartData chart = new ChartData
        {
            // 기본 음악 정보
            songName = source.songName,
            artistName = source.artistName,
            audioFileName = source.audioFileName,
            coverImageFileName = source.coverImageFileName,
            bpm = source.bpm,
            offset = source.offset,

            // 음악 상세 정보
            duration = source.duration,
            previewStart = source.previewStart,
            previewDuration = source.previewDuration,
            composer = source.composer,
            arranger = source.arranger,

            // 난이도 정보
            difficulty = source.difficulty,
            keyCount = source.keyCount,
            level = source.level,

            // 차트 제작 정보
            chartAuthor = source.chartAuthor,
            createdDate = source.createdDate,
            modifiedDate = source.modifiedDate,
            version = source.version,
            description = source.description,

            // 차트 통계
            noteCount = source.noteCount,
            longNoteCount = source.longNoteCount,
            maxCombo = source.maxCombo,
            density = source.density,

            // 비주얼 설정
            backgroundImage = source.backgroundImage,
            backgroundVideo = source.backgroundVideo,
            storyboardFile = source.storyboardFile,
            skinOverride = source.skinOverride,

            // 메타/분류
            tags = source.tags,
            source = source.source,
            copyright = source.copyright,
            beatmapId = source.beatmapId,

            // 노트 데이터
            notes = new List<NoteData>(source.notes)
        };

        return chart;
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
    /// <param name="chart">저장할 차트 데이터</param>
    /// <param name="fileName">파일 이름 (확장자 제외)</param>
    /// <param name="useSynthFormat">true면 .synth 형식, false면 .json 형식 (기본값: false)</param>
    public void SaveChart(ChartData chart, string fileName, bool useSynthFormat = false)
    {
        if (chart == null)
        {
            Debug.LogError("ChartLoader: 저장할 차트가 없습니다");
            return;
        }

        try
        {
            string extension = useSynthFormat ? ".synth" : ".json";
            string savePath;

            if (useStreamingAssets)
            {
                savePath = Path.Combine(Application.streamingAssetsPath, chartsFolderPath, $"{fileName}{extension}");
            }
            else
            {
                // Resources 폴더는 런타임에 쓰기 불가하므로 persistentDataPath 사용
                string chartDir = Path.Combine(Application.persistentDataPath, chartsFolderPath);
                if (!Directory.Exists(chartDir))
                {
                    Directory.CreateDirectory(chartDir);
                }
                savePath = Path.Combine(chartDir, $"{fileName}{extension}");
            }

            if (useSynthFormat)
            {
                // ChartData를 ChartDataNew로 변환 후 .synth 형식으로 저장
                ChartSystem.ChartDataNew chartDataNew = ConvertToChartDataNew(chart);
                CustomChartWriter.SaveToFile(chartDataNew, savePath);
            }
            else
            {
                // JSON 형식으로 저장
                string json = chart.ToJson();
                File.WriteAllText(savePath, json);
            }

            Debug.Log($"ChartLoader: 차트 저장 성공 - {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: 차트 저장 실패 - {e.Message}");
        }
    }

    /// <summary>
    /// ChartData를 ChartDataNew로 변환합니다
    /// </summary>
    private ChartSystem.ChartDataNew ConvertToChartDataNew(ChartData source)
    {
        ChartSystem.ChartDataNew chart = new ChartSystem.ChartDataNew
        {
            // 기본 음악 정보
            songName = source.songName,
            artistName = source.artistName,
            audioFileName = source.audioFileName,
            coverImageFileName = source.coverImageFileName,
            bpm = source.bpm,
            offset = source.offset,

            // 음악 상세 정보
            duration = source.duration,
            previewStart = source.previewStart,
            previewDuration = source.previewDuration,
            composer = source.composer,
            arranger = source.arranger,

            // 난이도 정보
            difficulty = source.difficulty,
            keyCount = source.keyCount,
            level = source.level,

            // 차트 제작 정보
            chartAuthor = source.chartAuthor,
            createdDate = source.createdDate,
            modifiedDate = source.modifiedDate,
            version = source.version,
            description = source.description,

            // 비주얼 설정
            backgroundImage = source.backgroundImage,
            backgroundVideo = source.backgroundVideo,
            storyboardFile = source.storyboardFile,
            skinOverride = source.skinOverride,

            // 메타/분류
            tags = source.tags,
            source = source.source,
            copyright = source.copyright,
            beatmapId = source.beatmapId,

            // 노트 데이터
            notes = new List<NoteData>(source.notes)
        };

        // 통계 자동 계산
        chart.UpdateStatistics();

        return chart;
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
            audioFileName = "sample_audio.wav", // FMOD는 wav 파일 권장
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
