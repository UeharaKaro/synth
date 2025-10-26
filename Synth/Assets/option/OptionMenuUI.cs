using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 옵션 메뉴 UI 관리 스크립트
/// 게임 설정을 조절할 수 있는 화면을 관리합니다.
/// </summary>
public class OptionMenuUI : MonoBehaviour
{
    [Header("오디오 설정")]
    [Tooltip("음악 볼륨 슬라이더 (0.0 ~ 1.0)")]
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    [Tooltip("볼륨 오프셋 슬라이더 (-200ms ~ 200ms)")]
    public Slider volumeOffsetSlider;
    public TextMeshProUGUI volumeOffsetText;

    [Tooltip("판정 오프셋 슬라이더 (-200ms ~ 200ms)")]
    public Slider judgmentOffsetSlider;
    public TextMeshProUGUI judgmentOffsetText;

    [Header("비주얼 설정")]
    [Tooltip("노트 크기 슬라이더 (0.5 ~ 3.0)")]
    public Slider noteSizeSlider;
    public TextMeshProUGUI noteSizeText;

    [Tooltip("트랙 높이 슬라이더 (5 ~ 30)")]
    public Slider trackHeightSlider;
    public TextMeshProUGUI trackHeightText;

    [Tooltip("트랙 각도 슬라이더 (-45 ~ 45)")]
    public Slider trackAngleSlider;
    public TextMeshProUGUI trackAngleText;

    [Tooltip("트랙 투명도 슬라이더 (0.1 ~ 1.0)")]
    public Slider trackOpacitySlider;
    public TextMeshProUGUI trackOpacityText;

    [Tooltip("노트 스크롤 속도 슬라이더 (1 ~ 20)")]
    public Slider noteScrollSpeedSlider;
    public TextMeshProUGUI noteScrollSpeedText;

    [Header("버튼")]
    [Tooltip("설정 저장 버튼")]
    public Button saveButton;

    [Tooltip("기본값으로 리셋 버튼")]
    public Button resetButton;

    [Tooltip("뒤로 가기 버튼")]
    public Button backButton;

    [Header("씬 설정")]
    [Tooltip("뒤로 가기 시 돌아갈 씬 이름")]
    public string mainMenuSceneName = "MainMenuScene";

    private GameSettings currentSettings;

    void Start()
    {
        // JudgmentModeManager가 없으면 경고
        if (JudgmentModeManager.Instance == null)
        {
            Debug.LogWarning("JudgmentModeManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        // 현재 설정 가져오기 (없으면 새로 생성)
        currentSettings = LoadSettings();

        // UI 초기화
        InitializeUI();

        // 버튼 이벤트 등록
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClicked);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        // 슬라이더 값 변경 이벤트 등록
        RegisterSliderEvents();
    }

    /// <summary>
    /// UI를 현재 설정값으로 초기화합니다.
    /// </summary>
    private void InitializeUI()
    {
        // 오디오 설정
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = currentSettings.musicVolume;

        if (volumeOffsetSlider != null)
            volumeOffsetSlider.value = currentSettings.volumeOffset;

        if (judgmentOffsetSlider != null)
            judgmentOffsetSlider.value = currentSettings.judgmentOffset;

        // 비주얼 설정
        if (noteSizeSlider != null)
            noteSizeSlider.value = currentSettings.noteSize;

        if (trackHeightSlider != null)
            trackHeightSlider.value = currentSettings.trackHeight;

        if (trackAngleSlider != null)
            trackAngleSlider.value = currentSettings.trackAngle;

        if (trackOpacitySlider != null)
            trackOpacitySlider.value = currentSettings.trackOpacity;

        if (noteScrollSpeedSlider != null)
            noteScrollSpeedSlider.value = currentSettings.noteScrollSpeed;

        // 텍스트 업데이트
        UpdateAllTexts();
    }

    /// <summary>
    /// 슬라이더 값 변경 이벤트를 등록합니다.
    /// </summary>
    private void RegisterSliderEvents()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (volumeOffsetSlider != null)
            volumeOffsetSlider.onValueChanged.AddListener(OnVolumeOffsetChanged);

        if (judgmentOffsetSlider != null)
            judgmentOffsetSlider.onValueChanged.AddListener(OnJudgmentOffsetChanged);

        if (noteSizeSlider != null)
            noteSizeSlider.onValueChanged.AddListener(OnNoteSizeChanged);

        if (trackHeightSlider != null)
            trackHeightSlider.onValueChanged.AddListener(OnTrackHeightChanged);

        if (trackAngleSlider != null)
            trackAngleSlider.onValueChanged.AddListener(OnTrackAngleChanged);

        if (trackOpacitySlider != null)
            trackOpacitySlider.onValueChanged.AddListener(OnTrackOpacityChanged);

        if (noteScrollSpeedSlider != null)
            noteScrollSpeedSlider.onValueChanged.AddListener(OnNoteScrollSpeedChanged);
    }

    /// <summary>
    /// 모든 텍스트를 현재 값으로 업데이트합니다.
    /// </summary>
    private void UpdateAllTexts()
    {
        if (musicVolumeText != null)
            musicVolumeText.text = $"{currentSettings.musicVolume:F2}";

        if (volumeOffsetText != null)
            volumeOffsetText.text = $"{currentSettings.volumeOffset:F0}ms";

        if (judgmentOffsetText != null)
            judgmentOffsetText.text = $"{currentSettings.judgmentOffset:F0}ms";

        if (noteSizeText != null)
            noteSizeText.text = $"{currentSettings.noteSize:F2}";

        if (trackHeightText != null)
            trackHeightText.text = $"{currentSettings.trackHeight:F1}";

        if (trackAngleText != null)
            trackAngleText.text = $"{currentSettings.trackAngle:F1}°";

        if (trackOpacityText != null)
            trackOpacityText.text = $"{currentSettings.trackOpacity:F2}";

        if (noteScrollSpeedText != null)
            noteScrollSpeedText.text = $"{currentSettings.noteScrollSpeed:F1}";
    }

    // 슬라이더 값 변경 핸들러들
    private void OnMusicVolumeChanged(float value)
    {
        currentSettings.musicVolume = value;
        if (musicVolumeText != null)
            musicVolumeText.text = $"{value:F2}";
    }

    private void OnVolumeOffsetChanged(float value)
    {
        currentSettings.volumeOffset = value;
        if (volumeOffsetText != null)
            volumeOffsetText.text = $"{value:F0}ms";
    }

    private void OnJudgmentOffsetChanged(float value)
    {
        currentSettings.judgmentOffset = value;
        if (judgmentOffsetText != null)
            judgmentOffsetText.text = $"{value:F0}ms";
    }

    private void OnNoteSizeChanged(float value)
    {
        currentSettings.noteSize = value;
        if (noteSizeText != null)
            noteSizeText.text = $"{value:F2}";
    }

    private void OnTrackHeightChanged(float value)
    {
        currentSettings.trackHeight = value;
        if (trackHeightText != null)
            trackHeightText.text = $"{value:F1}";
    }

    private void OnTrackAngleChanged(float value)
    {
        currentSettings.trackAngle = value;
        if (trackAngleText != null)
            trackAngleText.text = $"{value:F1}°";
    }

    private void OnTrackOpacityChanged(float value)
    {
        currentSettings.trackOpacity = value;
        if (trackOpacityText != null)
            trackOpacityText.text = $"{value:F2}";
    }

    private void OnNoteScrollSpeedChanged(float value)
    {
        currentSettings.noteScrollSpeed = value;
        if (noteScrollSpeedText != null)
            noteScrollSpeedText.text = $"{value:F1}";
    }

    /// <summary>
    /// 저장 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnSaveButtonClicked()
    {
        SaveSettings(currentSettings);
        Debug.Log("설정이 저장되었습니다.");
    }

    /// <summary>
    /// 리셋 버튼 클릭 시 호출됩니다.
    /// 모든 설정을 기본값으로 되돌립니다.
    /// </summary>
    public void OnResetButtonClicked()
    {
        currentSettings.ResetToDefault();
        InitializeUI();
        Debug.Log("설정이 기본값으로 리셋되었습니다.");
    }

    /// <summary>
    /// 뒤로 가기 버튼 클릭 시 호출됩니다.
    /// 메인 메뉴로 돌아갑니다.
    /// </summary>
    public void OnBackButtonClicked()
    {
        // 변경사항이 있으면 저장할지 물어볼 수도 있습니다 (선택사항)
        Debug.Log("메인 메뉴로 돌아갑니다.");

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("메인 메뉴 씬 이름이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// PlayerPrefs에서 설정을 불러옵니다.
    /// </summary>
    private GameSettings LoadSettings()
    {
        GameSettings settings = new GameSettings();

        // PlayerPrefs에서 값을 불러옵니다 (없으면 기본값 사용)
        settings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        settings.volumeOffset = PlayerPrefs.GetFloat("VolumeOffset", 0f);
        settings.judgmentOffset = PlayerPrefs.GetFloat("JudgmentOffset", 0f);
        settings.audioBuffer = PlayerPrefs.GetInt("AudioBuffer", 512);
        settings.noteSize = PlayerPrefs.GetFloat("NoteSize", 1f);
        settings.trackHeight = PlayerPrefs.GetFloat("TrackHeight", 15f);
        settings.trackAngle = PlayerPrefs.GetFloat("TrackAngle", 0f);
        settings.trackOpacity = PlayerPrefs.GetFloat("TrackOpacity", 0.8f);
        settings.noteScrollSpeed = PlayerPrefs.GetFloat("NoteScrollSpeed", 8f);

        return settings;
    }

    /// <summary>
    /// PlayerPrefs에 설정을 저장합니다.
    /// </summary>
    private void SaveSettings(GameSettings settings)
    {
        PlayerPrefs.SetFloat("MusicVolume", settings.musicVolume);
        PlayerPrefs.SetFloat("VolumeOffset", settings.volumeOffset);
        PlayerPrefs.SetFloat("JudgmentOffset", settings.judgmentOffset);
        PlayerPrefs.SetInt("AudioBuffer", settings.audioBuffer);
        PlayerPrefs.SetFloat("NoteSize", settings.noteSize);
        PlayerPrefs.SetFloat("TrackHeight", settings.trackHeight);
        PlayerPrefs.SetFloat("TrackAngle", settings.trackAngle);
        PlayerPrefs.SetFloat("TrackOpacity", settings.trackOpacity);
        PlayerPrefs.SetFloat("NoteScrollSpeed", settings.noteScrollSpeed);

        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        // 슬라이더 이벤트 해제
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (volumeOffsetSlider != null)
            volumeOffsetSlider.onValueChanged.RemoveListener(OnVolumeOffsetChanged);

        if (judgmentOffsetSlider != null)
            judgmentOffsetSlider.onValueChanged.RemoveListener(OnJudgmentOffsetChanged);

        if (noteSizeSlider != null)
            noteSizeSlider.onValueChanged.RemoveListener(OnNoteSizeChanged);

        if (trackHeightSlider != null)
            trackHeightSlider.onValueChanged.RemoveListener(OnTrackHeightChanged);

        if (trackAngleSlider != null)
            trackAngleSlider.onValueChanged.RemoveListener(OnTrackAngleChanged);

        if (trackOpacitySlider != null)
            trackOpacitySlider.onValueChanged.RemoveListener(OnTrackOpacityChanged);

        if (noteScrollSpeedSlider != null)
            noteScrollSpeedSlider.onValueChanged.RemoveListener(OnNoteScrollSpeedChanged);
    }
}
