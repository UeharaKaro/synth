using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 고급 기능이 포함된 곡 선택 화면 UI 관리 스크립트
/// - 스크롤 뷰로 곡 목록 표시
/// - 정렬/필터링 기능
/// - 검색 기능
/// - 최고 점수 표시
/// - 즐겨찾기 시스템
/// </summary>
public class SongSelectionUIAdvanced : MonoBehaviour
{
    [Header("곡 데이터베이스")]
    [Tooltip("곡 목록이 저장된 데이터베이스")]
    public SongDatabase songDatabase;

    [Header("스크롤 뷰")]
    [Tooltip("곡 목록 스크롤뷰")]
    public ScrollRect songListScrollView;

    [Tooltip("곡 목록 컨테이너 (Scroll View의 Content)")]
    public RectTransform songListContent;

    [Tooltip("곡 아이템 프리팹 (SongListItem 컴포넌트 포함)")]
    public GameObject songListItemPrefab;

    [Tooltip("스크롤 애니메이션 속도")]
    public float scrollSpeed = 5f;

    [Header("곡 정보 UI")]
    [Tooltip("곡 제목 텍스트")]
    public TextMeshProUGUI songTitleText;

    [Tooltip("아티스트 이름 텍스트")]
    public TextMeshProUGUI artistText;

    [Tooltip("BPM 텍스트")]
    public TextMeshProUGUI bpmText;

    [Tooltip("곡 길이 텍스트")]
    public TextMeshProUGUI songLengthText;

    [Tooltip("장르 텍스트")]
    public TextMeshProUGUI genreText;

    [Tooltip("곡 설명 텍스트")]
    public TextMeshProUGUI descriptionText;

    [Tooltip("앨범아트 이미지")]
    public Image albumArtImage;

    [Tooltip("배경 이미지")]
    public Image backgroundImage;

    [Header("난이도 UI")]
    [Tooltip("현재 난이도 텍스트")]
    public TextMeshProUGUI difficultyText;

    [Tooltip("난이도 레벨 텍스트")]
    public TextMeshProUGUI difficultyLevelText;

    [Tooltip("총 노트 수 텍스트")]
    public TextMeshProUGUI totalNotesText;

    [Tooltip("난이도 표시 이미지 (색상 변경용)")]
    public Image difficultyIndicatorImage;

    [Header("키 모드 UI")]
    [Tooltip("현재 키 개수 텍스트")]
    public TextMeshProUGUI keyCountText;

    [Header("최고 점수 UI")]
    [Tooltip("최고 점수 텍스트")]
    public TextMeshProUGUI highScoreText;

    [Tooltip("최고 등급 텍스트")]
    public TextMeshProUGUI highRankText;

    [Tooltip("플레이 횟수 텍스트")]
    public TextMeshProUGUI playCountText;

    [Tooltip("클리어 여부 텍스트")]
    public TextMeshProUGUI clearStatusText;

    [Header("검색 UI")]
    [Tooltip("검색 입력 필드")]
    public TMP_InputField searchInputField;

    [Tooltip("검색 버튼")]
    public Button searchButton;

    [Tooltip("검색 초기화 버튼")]
    public Button clearSearchButton;

    [Header("정렬 UI")]
    [Tooltip("정렬 드롭다운")]
    public TMP_Dropdown sortDropdown;

    [Tooltip("정렬 순서 토글 (오름차순/내림차순)")]
    public Toggle sortOrderToggle;

    [Tooltip("정렬 순서 텍스트")]
    public TextMeshProUGUI sortOrderText;

    [Header("필터 UI")]
    [Tooltip("난이도 필터 드롭다운")]
    public TMP_Dropdown difficultyFilterDropdown;

    [Tooltip("키 모드 필터 드롭다운")]
    public TMP_Dropdown keyModeFilterDropdown;

    [Tooltip("레벨 범위 최소 슬라이더")]
    public Slider minLevelSlider;

    [Tooltip("레벨 범위 최대 슬라이더")]
    public Slider maxLevelSlider;

    [Tooltip("최소 레벨 텍스트")]
    public TextMeshProUGUI minLevelText;

    [Tooltip("최대 레벨 텍스트")]
    public TextMeshProUGUI maxLevelText;

    [Tooltip("즐겨찾기만 표시 토글")]
    public Toggle favoritesOnlyToggle;

    [Tooltip("클리어한 곡만 표시 토글")]
    public Toggle clearedOnlyToggle;

    [Header("난이도/키 모드 변경 버튼")]
    [Tooltip("이전 난이도 버튼")]
    public Button previousDifficultyButton;

    [Tooltip("다음 난이도 버튼")]
    public Button nextDifficultyButton;

    [Tooltip("이전 키 개수 버튼")]
    public Button previousKeyCountButton;

    [Tooltip("다음 키 개수 버튼")]
    public Button nextKeyCountButton;

    [Header("메인 버튼")]
    [Tooltip("곡 선택 버튼 (게임 시작)")]
    public Button selectSongButton;

    [Tooltip("뒤로 가기 버튼")]
    public Button backButton;

    [Tooltip("미리듣기 버튼")]
    public Button previewButton;

    [Tooltip("즐겨찾기 토글 버튼")]
    public Button favoriteToggleButton;

    [Header("잠금 UI")]
    [Tooltip("곡이 잠겨있을 때 표시할 오브젝트")]
    public GameObject lockedIndicator;

    [Tooltip("잠금 메시지 텍스트")]
    public TextMeshProUGUI lockedMessageText;

    [Header("씬 설정")]
    [Tooltip("게임 플레이 씬 이름")]
    public string gameSceneName = "GameScene";

    [Tooltip("메인 메뉴 씬 이름")]
    public string mainMenuSceneName = "MainMenuScene";

    [Header("오디오 설정")]
    [Tooltip("미리듣기 오디오 소스")]
    public AudioSource previewAudioSource;

    [Header("키보드 설정")]
    [Tooltip("키보드 네비게이션 활성화")]
    public bool enableKeyboardNavigation = true;

    [Header("애니메이션 설정")]
    [Tooltip("선택 애니메이션 활성화")]
    public bool enableSelectionAnimation = true;

    [Tooltip("선택 스케일")]
    public float selectionScale = 1.1f;

    [Tooltip("애니메이션 시간")]
    public float animationDuration = 0.2f;

    // 현재 선택 상태
    private SongData currentSong;
    private DifficultyInfo currentDifficulty;
    private int currentDifficultyIndex = 0;
    private int currentKeyCountIndex = 0;

    // 곡 목록
    private List<SongData> allSongs = new List<SongData>();
    private List<SongData> filteredSongs = new List<SongData>();
    private List<GameObject> songListItems = new List<GameObject>();
    private int selectedItemIndex = -1;

    // 사용 가능한 난이도 및 키 개수 목록
    private List<string> availableDifficulties = new List<string>();
    private List<int> availableKeyCounts = new List<int>();

    // 미리듣기 상태
    private bool isPreviewPlaying = false;

    // 정렬 옵션
    public enum SortOption
    {
        Title,
        Artist,
        BPM,
        Level,
        PlayCount,
        HighScore,
        DateAdded
    }

    private SortOption currentSortOption = SortOption.Title;
    private bool sortAscending = true;

    // 필터 상태
    private string currentSearchQuery = "";
    private string currentDifficultyFilter = "All";
    private int currentKeyModeFilter = -1; // -1 = All
    private int minLevelFilter = 1;
    private int maxLevelFilter = 20;
    private bool showFavoritesOnly = false;
    private bool showClearedOnly = false;

    // 즐겨찾기 데이터 (PlayerPrefs에 저장)
    private HashSet<string> favoriteSongs = new HashSet<string>();

    void Start()
    {
        // 데이터베이스 확인
        if (songDatabase == null || songDatabase.GetSongCount() == 0)
        {
            Debug.LogError("SongDatabase가 설정되지 않았거나 곡이 없습니다!");
            return;
        }

        // 즐겨찾기 데이터 로드
        LoadFavorites();

        // 모든 곡 로드
        LoadAllSongs();

        // UI 초기화
        InitializeUI();

        // 버튼 이벤트 등록
        RegisterButtonEvents();

        // 필터/정렬 적용 및 곡 목록 생성
        ApplyFiltersAndSort();
    }

    void Update()
    {
        if (enableKeyboardNavigation)
        {
            HandleKeyboardInput();
        }
    }

    void OnDestroy()
    {
        // 미리듣기 중지
        StopPreview();
    }

    #region Initialization

    /// <summary>
    /// 모든 곡을 데이터베이스에서 로드합니다.
    /// </summary>
    private void LoadAllSongs()
    {
        allSongs.Clear();
        int songCount = songDatabase.GetSongCount();

        for (int i = 0; i < songCount; i++)
        {
            SongData song = songDatabase.GetSongByIndex(i);
            if (song != null)
            {
                allSongs.Add(song);
            }
        }

        Debug.Log($"총 {allSongs.Count}개의 곡을 로드했습니다.");
    }

    /// <summary>
    /// UI 요소들을 초기화합니다.
    /// </summary>
    private void InitializeUI()
    {
        // 정렬 드롭다운 설정
        if (sortDropdown != null)
        {
            sortDropdown.ClearOptions();
            List<string> sortOptions = new List<string>
            {
                "제목", "아티스트", "BPM", "레벨", "플레이 횟수", "최고 점수", "추가 날짜"
            };
            sortDropdown.AddOptions(sortOptions);
            sortDropdown.value = 0;
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }

        // 정렬 순서 토글 설정
        if (sortOrderToggle != null)
        {
            sortOrderToggle.isOn = sortAscending;
            sortOrderToggle.onValueChanged.AddListener(OnSortOrderChanged);
            UpdateSortOrderText();
        }

        // 난이도 필터 드롭다운 설정
        if (difficultyFilterDropdown != null)
        {
            difficultyFilterDropdown.ClearOptions();
            List<string> difficultyOptions = new List<string> { "전체", "Easy", "Normal", "Hard", "Expert", "Master", "Special" };
            difficultyFilterDropdown.AddOptions(difficultyOptions);
            difficultyFilterDropdown.value = 0;
            difficultyFilterDropdown.onValueChanged.AddListener(OnDifficultyFilterChanged);
        }

        // 키 모드 필터 드롭다운 설정
        if (keyModeFilterDropdown != null)
        {
            keyModeFilterDropdown.ClearOptions();
            List<string> keyModeOptions = new List<string> { "전체", "4K", "5K", "6K", "7K", "8K", "10K" };
            keyModeFilterDropdown.AddOptions(keyModeOptions);
            keyModeFilterDropdown.value = 0;
            keyModeFilterDropdown.onValueChanged.AddListener(OnKeyModeFilterChanged);
        }

        // 레벨 슬라이더 설정
        if (minLevelSlider != null)
        {
            minLevelSlider.minValue = 1;
            minLevelSlider.maxValue = 20;
            minLevelSlider.value = 1;
            minLevelSlider.onValueChanged.AddListener(OnMinLevelChanged);
        }

        if (maxLevelSlider != null)
        {
            maxLevelSlider.minValue = 1;
            maxLevelSlider.maxValue = 20;
            maxLevelSlider.value = 20;
            maxLevelSlider.onValueChanged.AddListener(OnMaxLevelChanged);
        }

        // 토글 설정
        if (favoritesOnlyToggle != null)
        {
            favoritesOnlyToggle.isOn = false;
            favoritesOnlyToggle.onValueChanged.AddListener(OnFavoritesOnlyChanged);
        }

        if (clearedOnlyToggle != null)
        {
            clearedOnlyToggle.isOn = false;
            clearedOnlyToggle.onValueChanged.AddListener(OnClearedOnlyChanged);
        }

        // 검색 입력 필드 설정
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchTextChanged);
        }

        UpdateLevelRangeText();
    }

    /// <summary>
    /// 버튼 이벤트를 등록합니다.
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (previousDifficultyButton != null)
            previousDifficultyButton.onClick.AddListener(OnPreviousDifficultyClicked);

        if (nextDifficultyButton != null)
            nextDifficultyButton.onClick.AddListener(OnNextDifficultyClicked);

        if (previousKeyCountButton != null)
            previousKeyCountButton.onClick.AddListener(OnPreviousKeyCountClicked);

        if (nextKeyCountButton != null)
            nextKeyCountButton.onClick.AddListener(OnNextKeyCountClicked);

        if (selectSongButton != null)
            selectSongButton.onClick.AddListener(OnSelectSongClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (previewButton != null)
            previewButton.onClick.AddListener(OnPreviewClicked);

        if (favoriteToggleButton != null)
            favoriteToggleButton.onClick.AddListener(OnFavoriteToggleClicked);

        if (searchButton != null)
            searchButton.onClick.AddListener(OnSearchClicked);

        if (clearSearchButton != null)
            clearSearchButton.onClick.AddListener(OnClearSearchClicked);
    }

    #endregion

    #region Keyboard Input

    /// <summary>
    /// 키보드 입력을 처리합니다.
    /// </summary>
    private void HandleKeyboardInput()
    {
        // 곡 선택 (위/아래 방향키)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectPreviousSong();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectNextSong();
        }

        // 난이도 변경 (좌/우 방향키)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnPreviousDifficultyClicked();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnNextDifficultyClicked();
        }

        // 키 개수 변경 (좌/우 Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnPreviousKeyCountClicked();
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            OnNextKeyCountClicked();
        }

        // 곡 선택 (Enter)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSelectSongClicked();
        }

        // 뒤로 가기 (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
        }

        // 미리듣기 (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPreviewClicked();
        }

        // 즐겨찾기 (F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnFavoriteToggleClicked();
        }
    }

    #endregion

    #region Song List Management

    /// <summary>
    /// 필터와 정렬을 적용하여 곡 목록을 업데이트합니다.
    /// </summary>
    private void ApplyFiltersAndSort()
    {
        // 필터 적용
        filteredSongs = allSongs.Where(song => PassesFilters(song)).ToList();

        // 정렬 적용
        filteredSongs = SortSongs(filteredSongs);

        // 곡 목록 UI 생성
        GenerateSongList();

        // 첫 번째 곡 선택
        if (filteredSongs.Count > 0)
        {
            SelectSongByIndex(0);
        }
        else
        {
            Debug.LogWarning("필터 조건에 맞는 곡이 없습니다.");
            ClearSongInfo();
        }
    }

    /// <summary>
    /// 곡이 현재 필터를 통과하는지 확인합니다.
    /// </summary>
    private bool PassesFilters(SongData song)
    {
        // 검색어 필터
        if (!string.IsNullOrEmpty(currentSearchQuery))
        {
            string query = currentSearchQuery.ToLower();
            if (!song.title.ToLower().Contains(query) &&
                !song.artist.ToLower().Contains(query) &&
                !song.genre.ToLower().Contains(query))
            {
                return false;
            }
        }

        // 난이도 필터
        if (currentDifficultyFilter != "All" && currentDifficultyFilter != "전체")
        {
            bool hasDifficulty = song.difficulties.Any(d => d.difficultyName == currentDifficultyFilter);
            if (!hasDifficulty)
            {
                return false;
            }
        }

        // 키 모드 필터
        if (currentKeyModeFilter != -1)
        {
            bool hasKeyMode = song.difficulties.Any(d => d.keyCount == currentKeyModeFilter);
            if (!hasKeyMode)
            {
                return false;
            }
        }

        // 레벨 범위 필터
        float minLevel = song.difficulties.Min(d => d.level);
        float maxLevel = song.difficulties.Max(d => d.level);

        if (maxLevel < minLevelFilter || minLevel > maxLevelFilter)
        {
            return false;
        }

        // 즐겨찾기 필터
        if (showFavoritesOnly)
        {
            string songKey = GetSongKey(song);
            if (!favoriteSongs.Contains(songKey))
            {
                return false;
            }
        }

        // 클리어 필터
        if (showClearedOnly)
        {
            if (!HasCleared(song))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 곡 목록을 정렬합니다.
    /// </summary>
    private List<SongData> SortSongs(List<SongData> songs)
    {
        IOrderedEnumerable<SongData> sorted;

        switch (currentSortOption)
        {
            case SortOption.Title:
                sorted = sortAscending ? songs.OrderBy(s => s.title) : songs.OrderByDescending(s => s.title);
                break;

            case SortOption.Artist:
                sorted = sortAscending ? songs.OrderBy(s => s.artist) : songs.OrderByDescending(s => s.artist);
                break;

            case SortOption.BPM:
                sorted = sortAscending ? songs.OrderBy(s => s.bpm) : songs.OrderByDescending(s => s.bpm);
                break;

            case SortOption.Level:
                sorted = sortAscending ? songs.OrderBy(s => s.difficulties.Min(d => d.level)) : songs.OrderByDescending(s => s.difficulties.Max(d => d.level));
                break;

            case SortOption.PlayCount:
                sorted = sortAscending ? songs.OrderBy(s => GetPlayCount(s)) : songs.OrderByDescending(s => GetPlayCount(s));
                break;

            case SortOption.HighScore:
                sorted = sortAscending ? songs.OrderBy(s => GetHighScore(s)) : songs.OrderByDescending(s => GetHighScore(s));
                break;

            case SortOption.DateAdded:
                // 곡 인덱스를 추가 날짜로 간주
                sorted = sortAscending ? songs.OrderBy(s => allSongs.IndexOf(s)) : songs.OrderByDescending(s => allSongs.IndexOf(s));
                break;

            default:
                sorted = songs.OrderBy(s => s.title);
                break;
        }

        return sorted.ToList();
    }

    /// <summary>
    /// 곡 목록 UI를 생성합니다.
    /// </summary>
    private void GenerateSongList()
    {
        // 기존 아이템 제거
        foreach (var item in songListItems)
        {
            Destroy(item);
        }
        songListItems.Clear();

        // 프리팹이 없으면 생성하지 않음
        if (songListItemPrefab == null || songListContent == null)
        {
            Debug.LogWarning("songListItemPrefab 또는 songListContent가 설정되지 않았습니다.");
            return;
        }

        // 새 아이템 생성
        for (int i = 0; i < filteredSongs.Count; i++)
        {
            SongData song = filteredSongs[i];
            GameObject item = Instantiate(songListItemPrefab, songListContent);
            songListItems.Add(item);

            // SongListItem 컴포넌트 설정
            SongListItem listItem = item.GetComponent<SongListItem>();
            if (listItem != null)
            {
                listItem.Setup(song, i, this);
                listItem.SetFavorite(IsFavorite(song));
            }

            // 클릭 이벤트 등록
            int index = i; // 로컬 복사
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectSongByIndex(index));
            }
        }

        Debug.Log($"곡 목록 UI 생성 완료: {songListItems.Count}개 아이템");
    }

    #endregion

    #region Song Selection

    /// <summary>
    /// 인덱스로 곡을 선택합니다.
    /// </summary>
    public void SelectSongByIndex(int index)
    {
        if (index < 0 || index >= filteredSongs.Count)
        {
            Debug.LogWarning($"잘못된 곡 인덱스: {index}");
            return;
        }

        selectedItemIndex = index;
        currentSong = filteredSongs[index];

        // 사용 가능한 난이도 및 키 개수 업데이트
        UpdateAvailableDifficulties();
        UpdateAvailableKeyCounts();

        // 기본 난이도 및 키 개수 설정
        currentDifficultyIndex = 0;
        currentKeyCountIndex = 0;

        // UI 업데이트
        UpdateSongInfo();
        UpdateDifficultyInfo();
        UpdateHighScoreInfo();
        UpdateListItemSelection();

        // 스크롤 위치 조정
        ScrollToSelectedItem();

        // 미리듣기 중지
        StopPreview();

        Debug.Log($"곡 선택: {currentSong.title} - {currentSong.artist}");
    }

    /// <summary>
    /// 이전 곡을 선택합니다.
    /// </summary>
    private void SelectPreviousSong()
    {
        if (filteredSongs.Count == 0) return;

        int newIndex = selectedItemIndex - 1;
        if (newIndex < 0)
        {
            newIndex = filteredSongs.Count - 1; // 순환
        }

        SelectSongByIndex(newIndex);
    }

    /// <summary>
    /// 다음 곡을 선택합니다.
    /// </summary>
    private void SelectNextSong()
    {
        if (filteredSongs.Count == 0) return;

        int newIndex = selectedItemIndex + 1;
        if (newIndex >= filteredSongs.Count)
        {
            newIndex = 0; // 순환
        }

        SelectSongByIndex(newIndex);
    }

    /// <summary>
    /// 리스트 아이템 선택 상태를 업데이트합니다.
    /// </summary>
    private void UpdateListItemSelection()
    {
        for (int i = 0; i < songListItems.Count; i++)
        {
            SongListItem listItem = songListItems[i].GetComponent<SongListItem>();
            if (listItem != null)
            {
                listItem.SetSelected(i == selectedItemIndex);
            }
        }
    }

    /// <summary>
    /// 선택된 아이템으로 스크롤합니다.
    /// </summary>
    private void ScrollToSelectedItem()
    {
        if (songListScrollView == null || songListContent == null || selectedItemIndex < 0)
        {
            return;
        }

        // 아이템 위치 계산
        float itemHeight = songListContent.rect.height / filteredSongs.Count;
        float targetY = selectedItemIndex * itemHeight;

        // 스크롤 위치 계산 (0 = 맨 위, 1 = 맨 아래)
        float normalizedPosition = 1f - (targetY / (songListContent.rect.height - songListScrollView.viewport.rect.height));
        normalizedPosition = Mathf.Clamp01(normalizedPosition);

        // 스크롤 애니메이션
        if (enableSelectionAnimation)
        {
            StopCoroutine(nameof(SmoothScroll));
            StartCoroutine(SmoothScroll(normalizedPosition));
        }
        else
        {
            songListScrollView.verticalNormalizedPosition = normalizedPosition;
        }
    }

    /// <summary>
    /// 부드러운 스크롤 애니메이션
    /// </summary>
    private System.Collections.IEnumerator SmoothScroll(float targetPosition)
    {
        float startPosition = songListScrollView.verticalNormalizedPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            songListScrollView.verticalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        songListScrollView.verticalNormalizedPosition = targetPosition;
    }

    #endregion

    #region Difficulty & Key Count

    /// <summary>
    /// 사용 가능한 난이도 목록을 업데이트합니다.
    /// </summary>
    private void UpdateAvailableDifficulties()
    {
        availableDifficulties.Clear();

        if (currentSong != null && currentSong.difficulties != null)
        {
            foreach (var diff in currentSong.difficulties)
            {
                if (!availableDifficulties.Contains(diff.difficultyName))
                {
                    availableDifficulties.Add(diff.difficultyName);
                }
            }
        }

        if (availableDifficulties.Count == 0)
        {
            availableDifficulties.Add(songDatabase.defaultDifficulty);
        }
    }

    /// <summary>
    /// 사용 가능한 키 개수 목록을 업데이트합니다.
    /// </summary>
    private void UpdateAvailableKeyCounts()
    {
        availableKeyCounts.Clear();

        if (currentSong != null && currentSong.difficulties != null)
        {
            string currentDiffName = currentDifficultyIndex < availableDifficulties.Count
                ? availableDifficulties[currentDifficultyIndex]
                : songDatabase.defaultDifficulty;

            var difficultiesForCurrentDiff = currentSong.difficulties
                .Where(d => d.difficultyName == currentDiffName)
                .ToList();

            foreach (var diff in difficultiesForCurrentDiff)
            {
                if (!availableKeyCounts.Contains(diff.keyCount))
                {
                    availableKeyCounts.Add(diff.keyCount);
                }
            }

            availableKeyCounts.Sort();
        }

        if (availableKeyCounts.Count == 0)
        {
            availableKeyCounts.Add(songDatabase.defaultKeyCount);
        }
    }

    /// <summary>
    /// 현재 난이도 정보를 가져옵니다.
    /// </summary>
    private DifficultyInfo GetCurrentDifficultyInfo()
    {
        if (currentSong == null || currentSong.difficulties == null || currentSong.difficulties.Count == 0)
        {
            return null;
        }

        string diffName = currentDifficultyIndex < availableDifficulties.Count
            ? availableDifficulties[currentDifficultyIndex]
            : songDatabase.defaultDifficulty;

        int keyCount = currentKeyCountIndex < availableKeyCounts.Count
            ? availableKeyCounts[currentKeyCountIndex]
            : songDatabase.defaultKeyCount;

        return currentSong.difficulties
            .FirstOrDefault(d => d.difficultyName == diffName && d.keyCount == keyCount);
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// 곡 정보 UI를 업데이트합니다.
    /// </summary>
    private void UpdateSongInfo()
    {
        if (currentSong == null)
        {
            ClearSongInfo();
            return;
        }

        if (songTitleText != null)
            songTitleText.text = currentSong.title;

        if (artistText != null)
            artistText.text = currentSong.artist;

        if (bpmText != null)
            bpmText.text = $"BPM: {currentSong.bpm:F0}";

        if (songLengthText != null)
        {
            int minutes = Mathf.FloorToInt(currentSong.songLength / 60f);
            int seconds = Mathf.FloorToInt(currentSong.songLength % 60f);
            songLengthText.text = $"{minutes:D2}:{seconds:D2}";
        }

        if (genreText != null)
            genreText.text = currentSong.genre;

        if (descriptionText != null)
            descriptionText.text = currentSong.description;

        // 앨범 커버 로드
        LoadAlbumArt();

        // 잠금 UI 업데이트
        UpdateLockUI();
    }

    /// <summary>
    /// 난이도 정보 UI를 업데이트합니다.
    /// </summary>
    private void UpdateDifficultyInfo()
    {
        currentDifficulty = GetCurrentDifficultyInfo();

        if (currentDifficulty == null)
        {
            if (difficultyText != null) difficultyText.text = "N/A";
            if (difficultyLevelText != null) difficultyLevelText.text = "Lv. 0";
            if (totalNotesText != null) totalNotesText.text = "0 Notes";
            if (keyCountText != null) keyCountText.text = "0K";
            return;
        }

        if (difficultyText != null)
            difficultyText.text = currentDifficulty.difficultyName;

        if (difficultyLevelText != null)
            difficultyLevelText.text = $"Lv. {currentDifficulty.level:F1}";

        if (totalNotesText != null)
            totalNotesText.text = $"{currentDifficulty.totalNotes} Notes";

        if (keyCountText != null)
            keyCountText.text = $"{currentDifficulty.keyCount}K";

        // 난이도 색상 업데이트
        if (difficultyIndicatorImage != null)
        {
            difficultyIndicatorImage.color = currentDifficulty.color;
        }
    }

    /// <summary>
    /// 최고 점수 정보 UI를 업데이트합니다.
    /// </summary>
    private void UpdateHighScoreInfo()
    {
        if (currentSong == null || currentDifficulty == null)
        {
            ClearHighScoreInfo();
            return;
        }

        int highScore = GetHighScore(currentSong, currentDifficulty);
        string highRank = GetHighRank(currentSong, currentDifficulty);
        int playCount = GetPlayCount(currentSong, currentDifficulty);
        bool cleared = HasCleared(currentSong, currentDifficulty);

        if (highScoreText != null)
        {
            highScoreText.text = highScore > 0 ? highScore.ToString("N0") : "No Record";
        }

        if (highRankText != null)
        {
            highRankText.text = string.IsNullOrEmpty(highRank) ? "-" : highRank;
        }

        if (playCountText != null)
        {
            playCountText.text = $"Played: {playCount}";
        }

        if (clearStatusText != null)
        {
            clearStatusText.text = cleared ? "CLEARED" : "NOT CLEARED";
            clearStatusText.color = cleared ? Color.green : Color.gray;
        }
    }

    /// <summary>
    /// 잠금 UI를 업데이트합니다.
    /// </summary>
    private void UpdateLockUI()
    {
        bool isLocked = currentSong != null && currentSong.isLocked;

        if (lockedIndicator != null)
        {
            lockedIndicator.SetActive(isLocked);
        }

        if (lockedMessageText != null && isLocked)
        {
            lockedMessageText.text = currentSong.unlockCondition;
        }

        // 선택 버튼 활성화/비활성화
        if (selectSongButton != null)
        {
            selectSongButton.interactable = !isLocked;
        }
    }

    /// <summary>
    /// 곡 정보를 초기화합니다.
    /// </summary>
    private void ClearSongInfo()
    {
        if (songTitleText != null) songTitleText.text = "No Song";
        if (artistText != null) artistText.text = "";
        if (bpmText != null) bpmText.text = "BPM: 0";
        if (songLengthText != null) songLengthText.text = "00:00";
        if (genreText != null) genreText.text = "";
        if (descriptionText != null) descriptionText.text = "";

        ClearHighScoreInfo();
    }

    /// <summary>
    /// 최고 점수 정보를 초기화합니다.
    /// </summary>
    private void ClearHighScoreInfo()
    {
        if (highScoreText != null) highScoreText.text = "No Record";
        if (highRankText != null) highRankText.text = "-";
        if (playCountText != null) playCountText.text = "Played: 0";
        if (clearStatusText != null) clearStatusText.text = "NOT CLEARED";
    }

    /// <summary>
    /// 레벨 범위 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateLevelRangeText()
    {
        if (minLevelText != null)
            minLevelText.text = $"Min: {minLevelFilter}";

        if (maxLevelText != null)
            maxLevelText.text = $"Max: {maxLevelFilter}";
    }

    /// <summary>
    /// 정렬 순서 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateSortOrderText()
    {
        if (sortOrderText != null)
        {
            sortOrderText.text = sortAscending ? "오름차순" : "내림차순";
        }
    }

    #endregion

    #region Album Art & Preview

    /// <summary>
    /// 앨범 커버를 로드합니다.
    /// </summary>
    private void LoadAlbumArt()
    {
        if (currentSong == null || albumArtImage == null)
        {
            return;
        }

        // CoverArtLoader 사용
        if (CoverArtLoader.Instance != null)
        {
            StartCoroutine(LoadAlbumArtCoroutine());
        }
    }

    /// <summary>
    /// 앨범 커버 로딩 코루틴
    /// </summary>
    private System.Collections.IEnumerator LoadAlbumArtCoroutine()
    {
        yield return CoverArtLoader.Instance.LoadCoverArtCoroutine(currentSong, (texture) =>
        {
            if (texture != null && albumArtImage != null)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                albumArtImage.sprite = sprite;

                // 배경 이미지에도 적용 (선택사항)
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = sprite;
                    backgroundImage.color = new Color(1f, 1f, 1f, 0.3f); // 반투명
                }
            }
        });
    }

    /// <summary>
    /// 미리듣기를 시작합니다.
    /// </summary>
    private void StartPreview()
    {
        if (currentSong == null)
        {
            return;
        }

        // AudioManager 사용
        if (AudioManager.Instance != null)
        {
            string audioPath = currentSong.audioFileName;
            AudioManager.Instance.PlayBGM(audioPath);
            isPreviewPlaying = true;
            Debug.Log($"미리듣기 시작: {audioPath}");
        }
        else
        {
            Debug.LogWarning("AudioManager가 없습니다.");
        }
    }

    /// <summary>
    /// 미리듣기를 중지합니다.
    /// </summary>
    private void StopPreview()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            isPreviewPlaying = false;
            Debug.Log("미리듣기 중지");
        }
    }

    #endregion

    #region Favorites

    /// <summary>
    /// 즐겨찾기 데이터를 로드합니다.
    /// </summary>
    private void LoadFavorites()
    {
        favoriteSongs.Clear();
        string favoritesData = PlayerPrefs.GetString("FavoriteSongs", "");

        if (!string.IsNullOrEmpty(favoritesData))
        {
            string[] favorites = favoritesData.Split(',');
            foreach (string fav in favorites)
            {
                if (!string.IsNullOrEmpty(fav))
                {
                    favoriteSongs.Add(fav);
                }
            }
        }

        Debug.Log($"즐겨찾기 로드: {favoriteSongs.Count}개");
    }

    /// <summary>
    /// 즐겨찾기 데이터를 저장합니다.
    /// </summary>
    private void SaveFavorites()
    {
        string favoritesData = string.Join(",", favoriteSongs);
        PlayerPrefs.SetString("FavoriteSongs", favoritesData);
        PlayerPrefs.Save();
        Debug.Log("즐겨찾기 저장 완료");
    }

    /// <summary>
    /// 곡이 즐겨찾기인지 확인합니다.
    /// </summary>
    private bool IsFavorite(SongData song)
    {
        string songKey = GetSongKey(song);
        return favoriteSongs.Contains(songKey);
    }

    /// <summary>
    /// 곡 키를 생성합니다.
    /// </summary>
    private string GetSongKey(SongData song)
    {
        return $"{song.title}_{song.artist}";
    }

    /// <summary>
    /// 즐겨찾기를 토글합니다.
    /// </summary>
    private void ToggleFavorite()
    {
        if (currentSong == null)
        {
            return;
        }

        string songKey = GetSongKey(currentSong);

        if (favoriteSongs.Contains(songKey))
        {
            favoriteSongs.Remove(songKey);
            Debug.Log($"즐겨찾기 제거: {currentSong.title}");
        }
        else
        {
            favoriteSongs.Add(songKey);
            Debug.Log($"즐겨찾기 추가: {currentSong.title}");
        }

        SaveFavorites();

        // UI 업데이트
        if (selectedItemIndex >= 0 && selectedItemIndex < songListItems.Count)
        {
            SongListItem listItem = songListItems[selectedItemIndex].GetComponent<SongListItem>();
            if (listItem != null)
            {
                listItem.SetFavorite(IsFavorite(currentSong));
            }
        }

        // 필터가 즐겨찾기만 표시 중이면 목록 갱신
        if (showFavoritesOnly)
        {
            ApplyFiltersAndSort();
        }
    }

    #endregion

    #region Score Data

    /// <summary>
    /// 곡의 최고 점수를 가져옵니다.
    /// </summary>
    private int GetHighScore(SongData song)
    {
        // 모든 난이도 중 최고 점수
        int maxScore = 0;
        foreach (var diff in song.difficulties)
        {
            int score = GetHighScore(song, diff);
            if (score > maxScore)
            {
                maxScore = score;
            }
        }
        return maxScore;
    }

    /// <summary>
    /// 특정 난이도의 최고 점수를 가져옵니다.
    /// </summary>
    private int GetHighScore(SongData song, DifficultyInfo difficulty)
    {
        string key = $"HighScore_{song.title}_{difficulty.difficultyName}_{difficulty.keyCount}K";
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// 곡의 최고 등급을 가져옵니다.
    /// </summary>
    private string GetHighRank(SongData song, DifficultyInfo difficulty)
    {
        string key = $"HighRank_{song.title}_{difficulty.difficultyName}_{difficulty.keyCount}K";
        return PlayerPrefs.GetString(key, "");
    }

    /// <summary>
    /// 곡의 플레이 횟수를 가져옵니다.
    /// </summary>
    private int GetPlayCount(SongData song)
    {
        // 모든 난이도 플레이 횟수 합계
        int totalCount = 0;
        foreach (var diff in song.difficulties)
        {
            totalCount += GetPlayCount(song, diff);
        }
        return totalCount;
    }

    /// <summary>
    /// 특정 난이도의 플레이 횟수를 가져옵니다.
    /// </summary>
    private int GetPlayCount(SongData song, DifficultyInfo difficulty)
    {
        string key = $"PlayCount_{song.title}_{difficulty.difficultyName}_{difficulty.keyCount}K";
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// 곡을 클리어했는지 확인합니다.
    /// </summary>
    private bool HasCleared(SongData song)
    {
        // 하나라도 클리어했으면 true
        foreach (var diff in song.difficulties)
        {
            if (HasCleared(song, diff))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 특정 난이도를 클리어했는지 확인합니다.
    /// </summary>
    private bool HasCleared(SongData song, DifficultyInfo difficulty)
    {
        string key = $"Cleared_{song.title}_{difficulty.difficultyName}_{difficulty.keyCount}K";
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    #endregion

    #region Filter & Sort Callbacks

    private void OnSortChanged(int value)
    {
        currentSortOption = (SortOption)value;
        ApplyFiltersAndSort();
    }

    private void OnSortOrderChanged(bool ascending)
    {
        sortAscending = ascending;
        UpdateSortOrderText();
        ApplyFiltersAndSort();
    }

    private void OnDifficultyFilterChanged(int value)
    {
        if (difficultyFilterDropdown != null)
        {
            currentDifficultyFilter = difficultyFilterDropdown.options[value].text;
            ApplyFiltersAndSort();
        }
    }

    private void OnKeyModeFilterChanged(int value)
    {
        if (value == 0)
        {
            currentKeyModeFilter = -1; // 전체
        }
        else
        {
            // "4K", "5K" 등에서 숫자만 추출
            string text = keyModeFilterDropdown.options[value].text;
            string numberStr = text.Replace("K", "");
            int.TryParse(numberStr, out currentKeyModeFilter);
        }

        ApplyFiltersAndSort();
    }

    private void OnMinLevelChanged(float value)
    {
        minLevelFilter = Mathf.RoundToInt(value);

        // 최대값보다 크면 최대값으로 조정
        if (minLevelFilter > maxLevelFilter)
        {
            minLevelFilter = maxLevelFilter;
            minLevelSlider.value = minLevelFilter;
        }

        UpdateLevelRangeText();
        ApplyFiltersAndSort();
    }

    private void OnMaxLevelChanged(float value)
    {
        maxLevelFilter = Mathf.RoundToInt(value);

        // 최소값보다 작으면 최소값으로 조정
        if (maxLevelFilter < minLevelFilter)
        {
            maxLevelFilter = minLevelFilter;
            maxLevelSlider.value = maxLevelFilter;
        }

        UpdateLevelRangeText();
        ApplyFiltersAndSort();
    }

    private void OnFavoritesOnlyChanged(bool value)
    {
        showFavoritesOnly = value;
        ApplyFiltersAndSort();
    }

    private void OnClearedOnlyChanged(bool value)
    {
        showClearedOnly = value;
        ApplyFiltersAndSort();
    }

    private void OnSearchTextChanged(string text)
    {
        currentSearchQuery = text;
        // 타이핑 중에는 즉시 필터링하지 않음 (성능)
    }

    private void OnSearchClicked()
    {
        ApplyFiltersAndSort();
    }

    private void OnClearSearchClicked()
    {
        if (searchInputField != null)
        {
            searchInputField.text = "";
            currentSearchQuery = "";
            ApplyFiltersAndSort();
        }
    }

    #endregion

    #region Button Callbacks

    private void OnPreviousDifficultyClicked()
    {
        if (availableDifficulties.Count == 0) return;

        currentDifficultyIndex--;
        if (currentDifficultyIndex < 0)
        {
            currentDifficultyIndex = availableDifficulties.Count - 1;
        }

        // 키 개수 목록 갱신
        UpdateAvailableKeyCounts();
        currentKeyCountIndex = 0;

        UpdateDifficultyInfo();
        UpdateHighScoreInfo();
    }

    private void OnNextDifficultyClicked()
    {
        if (availableDifficulties.Count == 0) return;

        currentDifficultyIndex++;
        if (currentDifficultyIndex >= availableDifficulties.Count)
        {
            currentDifficultyIndex = 0;
        }

        // 키 개수 목록 갱신
        UpdateAvailableKeyCounts();
        currentKeyCountIndex = 0;

        UpdateDifficultyInfo();
        UpdateHighScoreInfo();
    }

    private void OnPreviousKeyCountClicked()
    {
        if (availableKeyCounts.Count == 0) return;

        currentKeyCountIndex--;
        if (currentKeyCountIndex < 0)
        {
            currentKeyCountIndex = availableKeyCounts.Count - 1;
        }

        UpdateDifficultyInfo();
        UpdateHighScoreInfo();
    }

    private void OnNextKeyCountClicked()
    {
        if (availableKeyCounts.Count == 0) return;

        currentKeyCountIndex++;
        if (currentKeyCountIndex >= availableKeyCounts.Count)
        {
            currentKeyCountIndex = 0;
        }

        UpdateDifficultyInfo();
        UpdateHighScoreInfo();
    }

    private void OnSelectSongClicked()
    {
        if (currentSong == null || currentSong.isLocked)
        {
            Debug.LogWarning("잠긴 곡이거나 곡이 선택되지 않았습니다.");
            return;
        }

        if (currentDifficulty == null)
        {
            Debug.LogError("난이도 정보가 없습니다.");
            return;
        }

        // 미리듣기 중지
        StopPreview();

        // 곡 정보를 GameResultManager에 저장
        if (GameResultManager.Instance != null)
        {
            GameResultManager.Instance.SetCurrentSong(
                currentSong.title,
                currentSong.artist,
                currentDifficulty.difficultyName,
                currentDifficulty.keyCount
            );
        }

        // PlayerPrefs에도 저장 (하위 호환)
        string chartPath = currentDifficulty.chartFileName;
        PlayerPrefs.SetString("SelectedChart", chartPath);
        PlayerPrefs.SetString("SelectedSongTitle", currentSong.title);
        PlayerPrefs.SetString("SelectedArtist", currentSong.artist);
        PlayerPrefs.SetString("SelectedDifficulty", currentDifficulty.difficultyName);
        PlayerPrefs.SetInt("SelectedKeyCount", currentDifficulty.keyCount);
        PlayerPrefs.Save();

        Debug.Log($"곡 선택 완료: {currentSong.title} - {currentDifficulty.difficultyName} ({currentDifficulty.keyCount}K)");
        Debug.Log($"차트 경로: {chartPath}");

        // 게임 씬 로드
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnBackClicked()
    {
        // 미리듣기 중지
        StopPreview();

        // 메인 메뉴로 돌아가기
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnPreviewClicked()
    {
        if (isPreviewPlaying)
        {
            StopPreview();
        }
        else
        {
            StartPreview();
        }
    }

    private void OnFavoriteToggleClicked()
    {
        ToggleFavorite();
    }

    #endregion
}
