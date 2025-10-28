using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 곡 선택 화면 UI를 관리하는 메인 스크립트
/// </summary>
public class SongSelectionUI : MonoBehaviour
{
    [Header("곡 데이터베이스")]
    [Tooltip("곡 목록이 저장된 데이터베이스")]
    public SongDatabase songDatabase;

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

    [Header("곡 목록 UI")]
    [Tooltip("곡 인덱스 표시 텍스트 (예: 1 / 10)")]
    public TextMeshProUGUI songIndexText;

    [Tooltip("이전 곡 버튼")]
    public Button previousSongButton;

    [Tooltip("다음 곡 버튼")]
    public Button nextSongButton;

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

    // 현재 선택 상태
    private int currentSongIndex = 0;
    private int currentDifficultyIndex = 0;
    private int currentKeyCountIndex = 0;
    private SongData currentSong;
    private DifficultyInfo currentDifficulty;
    private int currentKeyCount;

    // 사용 가능한 난이도 및 키 개수 목록
    private List<string> availableDifficulties = new List<string>();
    private List<int> availableKeyCounts = new List<int>();

    // 미리듣기 상태
    private bool isPreviewPlaying = false;

    void Start()
    {
        // 데이터베이스 확인
        if (songDatabase == null || songDatabase.GetSongCount() == 0)
        {
            Debug.LogError("SongDatabase가 설정되지 않았거나 곡이 없습니다!");
            return;
        }

        // 버튼 이벤트 등록
        RegisterButtonEvents();

        // 첫 번째 곡 로드
        LoadSong(0);
    }

    void Update()
    {
        if (enableKeyboardNavigation)
        {
            HandleKeyboardInput();
        }
    }

    /// <summary>
    /// 버튼 이벤트를 등록합니다.
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (previousSongButton != null)
            previousSongButton.onClick.AddListener(OnPreviousSongClicked);

        if (nextSongButton != null)
            nextSongButton.onClick.AddListener(OnNextSongClicked);

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
    }

    /// <summary>
    /// 키보드 입력을 처리합니다.
    /// </summary>
    private void HandleKeyboardInput()
    {
        // 곡 선택 (위/아래 방향키)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnPreviousSongClicked();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnNextSongClicked();
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
    }

    /// <summary>
    /// 지정된 인덱스의 곡을 로드합니다.
    /// </summary>
    public void LoadSong(int index)
    {
        // 인덱스 범위 확인
        if (index < 0 || index >= songDatabase.GetSongCount())
        {
            Debug.LogWarning($"잘못된 곡 인덱스: {index}");
            return;
        }

        currentSongIndex = index;
        currentSong = songDatabase.GetSongByIndex(index);

        if (currentSong == null)
        {
            Debug.LogError($"곡을 로드할 수 없습니다: Index {index}");
            return;
        }

        // 사용 가능한 난이도 및 키 개수 목록 업데이트
        UpdateAvailableDifficulties();
        UpdateAvailableKeyCounts();

        // 기본 난이도 및 키 개수 설정
        currentDifficultyIndex = 0;
        currentKeyCountIndex = 0;

        // UI 업데이트
        UpdateUI();

        // 미리듣기 중지
        StopPreview();

        Debug.Log($"곡 로드: {currentSong.title} - {currentSong.artist}");
    }

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
                availableDifficulties.Add(diff.difficultyName);
            }
        }

        // 난이도가 없으면 기본값 추가
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

        if (currentSong != null && currentSong.supportedKeyCounts != null)
        {
            availableKeyCounts.AddRange(currentSong.supportedKeyCounts);
        }

        // 키 개수가 없으면 기본값 추가
        if (availableKeyCounts.Count == 0)
        {
            availableKeyCounts.Add(songDatabase.defaultKeyCount);
        }

        // 정렬
        availableKeyCounts.Sort();
    }

    /// <summary>
    /// 모든 UI를 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        if (currentSong == null) return;

        // 곡 정보 업데이트
        UpdateSongInfo();

        // 난이도 정보 업데이트
        UpdateDifficultyInfo();

        // 키 개수 정보 업데이트
        UpdateKeyCountInfo();

        // 잠금 상태 업데이트
        UpdateLockStatus();

        // 곡 인덱스 업데이트
        UpdateSongIndex();
    }

    /// <summary>
    /// 곡 정보 UI를 업데이트합니다.
    /// </summary>
    private void UpdateSongInfo()
    {
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

        // 앨범 아트 로드 (CoverArtLoader 사용)
        if (albumArtImage != null)
        {
            // 먼저 SongData의 albumArt 스프라이트 사용 시도
            if (currentSong.albumArt != null)
            {
                albumArtImage.sprite = currentSong.albumArt;
            }
            else if (!string.IsNullOrEmpty(currentSong.audioPath))
            {
                // audioPath로부터 커버 이미지 자동 로드
                StartCoroutine(LoadCoverArtAsync(currentSong.audioPath));
            }
        }

        if (backgroundImage != null && currentSong.backgroundImage != null)
            backgroundImage.sprite = currentSong.backgroundImage;
    }

    /// <summary>
    /// 커버 아트를 비동기로 로드합니다.
    /// </summary>
    private System.Collections.IEnumerator LoadCoverArtAsync(string audioFileName)
    {
        // CoverArtLoader 인스턴스 확인
        if (CoverArtLoader.Instance == null)
        {
            Debug.LogWarning("CoverArtLoader 인스턴스가 없습니다!");
            yield break;
        }

        // CoverArtLoader를 통해 이미지 로드
        var loadCoroutine = CoverArtLoader.Instance.LoadCoverArtAsync(audioFileName, (sprite) =>
        {
            if (sprite != null && albumArtImage != null)
            {
                albumArtImage.sprite = sprite;
                Debug.Log($"커버 아트 로드 성공: {audioFileName}");
            }
            else
            {
                Debug.LogWarning($"커버 아트를 찾을 수 없습니다: {audioFileName}");
            }
        });

        yield return StartCoroutine(loadCoroutine);
    }

    /// <summary>
    /// 난이도 정보 UI를 업데이트합니다.
    /// </summary>
    private void UpdateDifficultyInfo()
    {
        if (currentDifficultyIndex >= 0 && currentDifficultyIndex < availableDifficulties.Count)
        {
            string difficultyName = availableDifficulties[currentDifficultyIndex];
            currentDifficulty = currentSong.GetDifficulty(difficultyName);

            if (difficultyText != null)
                difficultyText.text = difficultyName;

            if (currentDifficulty != null)
            {
                if (difficultyLevelText != null)
                    difficultyLevelText.text = $"Lv.{currentDifficulty.level:F1}";

                if (totalNotesText != null)
                    totalNotesText.text = $"Notes: {currentDifficulty.totalNotes}";

                if (difficultyIndicatorImage != null)
                    difficultyIndicatorImage.color = currentDifficulty.difficultyColor;
            }
        }
    }

    /// <summary>
    /// 키 개수 정보 UI를 업데이트합니다.
    /// </summary>
    private void UpdateKeyCountInfo()
    {
        if (currentKeyCountIndex >= 0 && currentKeyCountIndex < availableKeyCounts.Count)
        {
            currentKeyCount = availableKeyCounts[currentKeyCountIndex];

            if (keyCountText != null)
                keyCountText.text = $"{currentKeyCount}K";
        }
    }

    /// <summary>
    /// 잠금 상태 UI를 업데이트합니다.
    /// </summary>
    private void UpdateLockStatus()
    {
        bool isLocked = currentSong.isLocked;

        if (lockedIndicator != null)
            lockedIndicator.SetActive(isLocked);

        if (selectSongButton != null)
            selectSongButton.interactable = !isLocked;

        if (previewButton != null)
            previewButton.interactable = !isLocked;
    }

    /// <summary>
    /// 곡 인덱스 표시를 업데이트합니다.
    /// </summary>
    private void UpdateSongIndex()
    {
        if (songIndexText != null)
        {
            songIndexText.text = $"{currentSongIndex + 1} / {songDatabase.GetSongCount()}";
        }
    }

    // ===== 버튼 이벤트 핸들러 =====

    public void OnPreviousSongClicked()
    {
        int newIndex = currentSongIndex - 1;
        if (newIndex < 0)
            newIndex = songDatabase.GetSongCount() - 1; // 순환
        LoadSong(newIndex);
    }

    public void OnNextSongClicked()
    {
        int newIndex = currentSongIndex + 1;
        if (newIndex >= songDatabase.GetSongCount())
            newIndex = 0; // 순환
        LoadSong(newIndex);
    }

    public void OnPreviousDifficultyClicked()
    {
        currentDifficultyIndex--;
        if (currentDifficultyIndex < 0)
            currentDifficultyIndex = availableDifficulties.Count - 1;
        UpdateDifficultyInfo();
    }

    public void OnNextDifficultyClicked()
    {
        currentDifficultyIndex++;
        if (currentDifficultyIndex >= availableDifficulties.Count)
            currentDifficultyIndex = 0;
        UpdateDifficultyInfo();
    }

    public void OnPreviousKeyCountClicked()
    {
        currentKeyCountIndex--;
        if (currentKeyCountIndex < 0)
            currentKeyCountIndex = availableKeyCounts.Count - 1;
        UpdateKeyCountInfo();
    }

    public void OnNextKeyCountClicked()
    {
        currentKeyCountIndex++;
        if (currentKeyCountIndex >= availableKeyCounts.Count)
            currentKeyCountIndex = 0;
        UpdateKeyCountInfo();
    }

    public void OnSelectSongClicked()
    {
        if (currentSong == null || currentSong.isLocked)
        {
            Debug.Log("곡이 잠겨있습니다!");
            return;
        }

        Debug.Log($"곡 선택: {currentSong.title} [{availableDifficulties[currentDifficultyIndex]}] {currentKeyCount}K");

        // 선택한 곡 정보를 GameResultManager에 저장
        if (GameResultManager.Instance != null)
        {
            GameResultManager.Instance.SetCurrentSongInfo(
                currentSong.title,
                currentSong.artist,
                availableDifficulties[currentDifficultyIndex],
                currentKeyCount
            );
        }

        // PlayerPrefs에도 저장 (하위 호환성)
        PlayerPrefs.SetString("SelectedSongId", currentSong.songId);
        PlayerPrefs.SetString("SelectedSongTitle", currentSong.title);
        PlayerPrefs.SetString("SelectedArtist", currentSong.artist);
        PlayerPrefs.SetString("SelectedDifficulty", availableDifficulties[currentDifficultyIndex]);
        PlayerPrefs.SetInt("SelectedKeyCount", currentKeyCount);
        PlayerPrefs.SetString("SelectedChartPath", currentSong.GetChartPath(availableDifficulties[currentDifficultyIndex], currentKeyCount));
        PlayerPrefs.Save();

        // 게임 씬으로 전환
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            StopPreview();
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("게임 씬 이름이 설정되지 않았습니다!");
        }
    }

    public void OnBackClicked()
    {
        Debug.Log("뒤로 가기");
        StopPreview();

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("메인 메뉴 씬 이름이 설정되지 않았습니다!");
        }
    }

    public void OnPreviewClicked()
    {
        if (currentSong == null || currentSong.isLocked)
            return;

        if (isPreviewPlaying)
        {
            StopPreview();
        }
        else
        {
            PlayPreview();
        }
    }

    /// <summary>
    /// 곡 미리듣기를 시작합니다.
    /// </summary>
    private void PlayPreview()
    {
        if (string.IsNullOrEmpty(currentSong.audioPath))
        {
            Debug.LogWarning("오디오 파일 경로가 설정되지 않았습니다!");
            return;
        }

        // AudioManager를 통한 미리듣기 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.LoadBGM(currentSong.audioPath);
            AudioManager.Instance.PlayBGM();
            
            // 미리듣기 시작 시간으로 이동 (코루틴 시작)
            StartCoroutine(SeekToPreviewTime(currentSong.previewStartTime));
            
            isPreviewPlaying = true;

            if (previewButton != null)
            {
                var buttonText = previewButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = "Stop";
            }

            Debug.Log($"미리듣기 시작: {currentSong.title} (시작 시간: {currentSong.previewStartTime}초)");
        }
        else
        {
            Debug.LogWarning("AudioManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 미리듣기 시작 시간으로 이동하는 코루틴
    /// </summary>
    private System.Collections.IEnumerator SeekToPreviewTime(float startTime)
    {
        // BGM이 시작될 때까지 짧은 대기
        yield return new WaitForSeconds(0.1f);
        
        // 미리듣기 시간 설정 (FMOD 채널 위치 조정)
        // Note: FMOD Channel의 setPosition을 사용하여 시간 이동
        // 구현 방법은 AudioManager에 SetBGMPosition 메서드 추가 필요
        // 임시로 여기서는 로그만 출력
        Debug.Log($"미리듣기 위치 설정: {startTime}초");
    }

    /// <summary>
    /// 곡 미리듣기를 중지합니다.
    /// </summary>
    private void StopPreview()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        isPreviewPlaying = false;

        if (previewButton != null)
        {
            var buttonText = previewButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Preview";
        }

        Debug.Log("미리듣기 중지");
    }

    void OnDestroy()
    {
        // 미리듣기 중지
        StopPreview();

        // 버튼 이벤트 해제
        if (previousSongButton != null)
            previousSongButton.onClick.RemoveListener(OnPreviousSongClicked);

        if (nextSongButton != null)
            nextSongButton.onClick.RemoveListener(OnNextSongClicked);

        if (previousDifficultyButton != null)
            previousDifficultyButton.onClick.RemoveListener(OnPreviousDifficultyClicked);

        if (nextDifficultyButton != null)
            nextDifficultyButton.onClick.RemoveListener(OnNextDifficultyClicked);

        if (previousKeyCountButton != null)
            previousKeyCountButton.onClick.RemoveListener(OnPreviousKeyCountClicked);

        if (nextKeyCountButton != null)
            nextKeyCountButton.onClick.RemoveListener(OnNextKeyCountClicked);

        if (selectSongButton != null)
            selectSongButton.onClick.RemoveListener(OnSelectSongClicked);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);

        if (previewButton != null)
            previewButton.onClick.RemoveListener(OnPreviewClicked);
    }
}
