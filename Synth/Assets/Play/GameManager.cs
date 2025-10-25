using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 흐름을 관리하는 메인 매니저
/// 차트 로딩, 오디오 재생, 노트 스폰 등을 통합 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("시스템 참조")]
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private NoteSpawner noteSpawner; // 새 노트 스폰 시스템
    [SerializeField] private NoteManager noteManager; // 기존 노트 관리 시스템
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private HPSystem hpSystem;
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private GearController gearController;

    [Header("게임 상태")]
    [SerializeField] private bool autoStart = false; // 자동 시작 (테스트용)
    [SerializeField] private bool useSampleChart = true; // 샘플 차트 사용 (테스트용)
    [SerializeField] private bool useNoteSpawner = true; // true: NoteSpawner 사용, false: NoteManager 사용

    // 현재 게임 상태
    private enum GameState
    {
        Idle,
        Loading,
        Playing,
        Paused,
        Finished
    }

    private GameState currentState = GameState.Idle;
    private ChartData currentChart;

    // Singleton 패턴
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 시스템 참조 자동 찾기
        FindSystemReferences();
    }

    void Start()
    {
        // HP 시스템 이벤트 구독
        if (hpSystem != null)
        {
            hpSystem.OnGameOver.AddListener(HandleGameOver);
            hpSystem.OnGameClear.AddListener(HandleGameClear);
        }

        // 자동 시작 (테스트용)
        if (autoStart)
        {
            if (useSampleChart)
            {
                StartGameWithSampleChart();
            }
            else
            {
                LoadChartFromSelection();
            }
        }
    }

    /// <summary>
    /// 시스템 참조 찾기
    /// </summary>
    private void FindSystemReferences()
    {
        if (chartLoader == null)
        {
            chartLoader = FindObjectOfType<ChartLoader>();
            if (chartLoader == null)
            {
                GameObject loaderObj = new GameObject("ChartLoader");
                chartLoader = loaderObj.AddComponent<ChartLoader>();
            }
        }

        if (noteSpawner == null)
        {
            noteSpawner = FindObjectOfType<NoteSpawner>();
            if (noteSpawner == null)
            {
                GameObject spawnerObj = new GameObject("NoteSpawner");
                noteSpawner = spawnerObj.AddComponent<NoteSpawner>();
            }
        }

        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();

        if (hpSystem == null)
            hpSystem = HPSystem.Instance;

        if (rhythmManager == null)
            rhythmManager = FindObjectOfType<RhythmManager>();

        if (gearController == null)
            gearController = FindObjectOfType<GearController>();

        if (noteManager == null)
            noteManager = FindObjectOfType<NoteManager>();
    }

    /// <summary>
    /// 샘플 차트로 게임 시작 (테스트용)
    /// </summary>
    public void StartGameWithSampleChart()
    {
        Debug.Log("GameManager: 샘플 차트로 게임 시작");

        currentState = GameState.Loading;

        // 샘플 차트 생성
        ChartData sampleChart = chartLoader.CreateSampleChart();

        if (sampleChart != null)
        {
            StartGame(sampleChart);
        }
        else
        {
            Debug.LogError("GameManager: 샘플 차트 생성 실패");
            currentState = GameState.Idle;
        }
    }

    /// <summary>
    /// 노래 선택 데이터로부터 차트 로드
    /// </summary>
    public void LoadChartFromSelection()
    {
        currentState = GameState.Loading;

        // PlayerPrefs에서 선택된 노래 정보 가져오기
        int keyCount = PlayerPrefs.GetInt("SelectedKeyCount", 4);
        string difficulty = PlayerPrefs.GetString("SelectedDifficulty", "Normal");
        string songName = PlayerPrefs.GetString("SelectedSongName", "SampleSong");

        Debug.Log($"GameManager: 차트 로드 - {songName} ({difficulty}, {keyCount}K)");

        // 차트 로드
        ChartData chart = chartLoader.LoadChart(songName, difficulty, keyCount);

        if (chart != null)
        {
            StartGame(chart);
        }
        else
        {
            Debug.LogError($"GameManager: 차트 로드 실패 - {songName}_{difficulty}_{keyCount}K");

            // 샘플 차트로 대체
            Debug.LogWarning("GameManager: 샘플 차트로 대체합니다");
            StartGameWithSampleChart();
        }
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame(ChartData chart)
    {
        if (chart == null)
        {
            Debug.LogError("GameManager: 차트가 null입니다!");
            return;
        }

        currentChart = chart;
        currentState = GameState.Playing;

        // HP 시스템 초기화
        if (hpSystem != null)
        {
            hpSystem.InitializeHP();
        }

        // 오디오 재생 시작
        if (audioManager != null && !string.IsNullOrEmpty(chart.audioFileName))
        {
            audioManager.LoadBGM(chart.audioFileName);
            audioManager.PlayBGM();
            Debug.Log($"GameManager: 오디오 재생 - {chart.audioFileName}");
        }
        else
        {
            Debug.LogWarning("GameManager: AudioManager가 없거나 오디오 파일명이 비어있습니다!");
        }

        // 노트 스폰 시작 (선택된 시스템에 따라)
        if (useNoteSpawner && noteSpawner != null)
        {
            // 새 NoteSpawner 시스템 사용
            noteSpawner.LoadAndStartChart(chart);
        }
        else if (!useNoteSpawner && noteManager != null)
        {
            // 기존 NoteManager 시스템 사용
            noteManager.LoadFromChartData(chart);
        }
        else
        {
            Debug.LogWarning("GameManager: 노트 스폰 시스템이 설정되지 않았습니다!");
        }

        Debug.Log($"GameManager: 게임 시작 - {chart.songName}");
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;

        // 오디오 일시정지
        if (audioManager != null)
        {
            audioManager.PauseBGM();
        }

        Debug.Log("GameManager: 게임 일시정지");
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        currentState = GameState.Playing;
        Time.timeScale = 1f;

        // 오디오 재개
        if (audioManager != null)
        {
            audioManager.ResumeBGM();
        }

        Debug.Log("GameManager: 게임 재개");
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;

        // 현재 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 게임오버 처리
    /// </summary>
    private void HandleGameOver()
    {
        currentState = GameState.Finished;

        Debug.Log("GameManager: 게임오버");

        // 노트 스폰 중지
        if (useNoteSpawner && noteSpawner != null)
        {
            noteSpawner.StopSpawning();
        }
        else if (!useNoteSpawner && noteManager != null)
        {
            noteManager.ClearAllNotes();
        }

        // 오디오 중지
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

        // TODO: 게임오버 UI 표시 또는 결과 화면으로 전환
        // 잠시 후 결과 화면으로 전환
        Invoke(nameof(GoToResultScreen), 2f);
    }

    /// <summary>
    /// 클리어 처리
    /// </summary>
    private void HandleGameClear()
    {
        currentState = GameState.Finished;

        Debug.Log("GameManager: 곡 클리어!");

        // 클리어 판정
        bool isCleared = hpSystem.CheckClearCondition();

        // TODO: 결과 데이터 수집 및 저장

        // 결과 화면으로 전환
        Invoke(nameof(GoToResultScreen), 2f);
    }

    /// <summary>
    /// 결과 화면으로 전환
    /// </summary>
    private void GoToResultScreen()
    {
        // TODO: 결과 데이터 전달
        SceneManager.LoadScene(SceneNames.RESULT);
    }

    /// <summary>
    /// 노래 선택으로 돌아가기
    /// </summary>
    public void BackToSongSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneNames.SONG_SELECTION);
    }

    /// <summary>
    /// 현재 게임 상태 확인
    /// </summary>
    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    void Update()
    {
        // ESC 키로 일시정지/재개
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        // R 키로 재시작 (테스트용)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (hpSystem != null)
        {
            hpSystem.OnGameOver.RemoveListener(HandleGameOver);
            hpSystem.OnGameClear.RemoveListener(HandleGameClear);
        }
    }
}
