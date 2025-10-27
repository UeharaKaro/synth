using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 옵션 메뉴 UI 관리 스크립트 (통합 버전)
/// SettingsManager와 연동하여 게임 설정을 조절합니다.
///
/// Phase 2-B-5 구현 (2025-10-27)
/// - 탭 시스템 (오디오/비주얼/게임플레이)
/// - SFX 볼륨 추가
/// - 판정 모드 드롭다운
/// - 실시간 AudioManager 연동
/// - SettingsManager 통합
/// </summary>
public class OptionMenuUI : MonoBehaviour
{
    [Header("탭 시스템")]
    [Tooltip("오디오 설정 패널")]
    public GameObject audioPanel;

    [Tooltip("비주얼 설정 패널")]
    public GameObject visualPanel;

    [Tooltip("게임플레이 설정 패널")]
    public GameObject gameplayPanel;

    [Tooltip("탭 버튼들")]
    public Button audioTabButton;
    public Button visualTabButton;
    public Button gameplayTabButton;

    [Header("오디오 설정")]
    [Tooltip("음악 볼륨 슬라이더 (0.0 ~ 1.0)")]
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    [Tooltip("효과음 볼륨 슬라이더 (0.0 ~ 1.0)")]
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;

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

    [Header("게임플레이 설정")]
    [Tooltip("판정 모드 드롭다운 (Normal/Hard/Super)")]
    public TMP_Dropdown judgmentModeDropdown;

    [Tooltip("판정 표시 토글")]
    public Toggle showJudgmentToggle;

    [Tooltip("오프셋 표시 토글")]
    public Toggle showOffsetToggle;

    [Header("버튼")]
    [Tooltip("설정 적용 버튼")]
    public Button applyButton;

    [Tooltip("기본값으로 리셋 버튼")]
    public Button resetButton;

    [Tooltip("뒤로 가기 버튼")]
    public Button backButton;

    [Header("씬 설정")]
    [Tooltip("뒤로 가기 시 돌아갈 씬 이름")]
    public string previousSceneName = "MainMenu";

    [Header("실시간 프리뷰")]
    [Tooltip("슬라이더 변경 시 즉시 적용 (AudioManager 연동)")]
    public bool enableRealtimePreview = true;

    private SettingsManager settingsManager;
    private AudioManager audioManager;
    private int currentTabIndex = 0; // 0: Audio, 1: Visual, 2: Gameplay

    void Start()
    {
        // SettingsManager 찾기
        settingsManager = SettingsManager.Instance;
        if (settingsManager == null)
        {
            Debug.LogWarning("SettingsManager를 찾을 수 없습니다! GameObject에 SettingsManager 추가 필요");
        }

        // AudioManager 찾기 (실시간 프리뷰용)
        audioManager = AudioManager.Instance;
        if (audioManager == null && enableRealtimePreview)
        {
            Debug.LogWarning("AudioManager를 찾을 수 없습니다. 실시간 오디오 프리뷰가 비활성화됩니다.");
            enableRealtimePreview = false;
        }

        // UI 초기화
        InitializeUI();
        InitializeTabs();

        // 버튼 이벤트 등록
        RegisterButtonEvents();

        // 슬라이더 값 변경 이벤트 등록
        RegisterSliderEvents();

        // 첫 번째 탭 활성화
        ShowTab(0);
    }

    /// <summary>
    /// 탭 시스템 초기화
    /// </summary>
    private void InitializeTabs()
    {
        if (audioTabButton != null)
            audioTabButton.onClick.AddListener(() => ShowTab(0));

        if (visualTabButton != null)
            visualTabButton.onClick.AddListener(() => ShowTab(1));

        if (gameplayTabButton != null)
            gameplayTabButton.onClick.AddListener(() => ShowTab(2));
    }

    /// <summary>
    /// 탭 전환
    /// </summary>
    private void ShowTab(int tabIndex)
    {
        currentTabIndex = tabIndex;

        // 모든 패널 비활성화
        if (audioPanel != null)
            audioPanel.SetActive(tabIndex == 0);

        if (visualPanel != null)
            visualPanel.SetActive(tabIndex == 1);

        if (gameplayPanel != null)
            gameplayPanel.SetActive(tabIndex == 2);

        // 탭 버튼 색상 변경 (선택된 탭 강조)
        UpdateTabButtonColors();
    }

    /// <summary>
    /// 탭 버튼 색상 업데이트
    /// </summary>
    private void UpdateTabButtonColors()
    {
        Color selectedColor = new Color(1f, 1f, 1f, 1f);
        Color normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        if (audioTabButton != null)
        {
            var colors = audioTabButton.colors;
            colors.normalColor = currentTabIndex == 0 ? selectedColor : normalColor;
            audioTabButton.colors = colors;
        }

        if (visualTabButton != null)
        {
            var colors = visualTabButton.colors;
            colors.normalColor = currentTabIndex == 1 ? selectedColor : normalColor;
            visualTabButton.colors = colors;
        }

        if (gameplayTabButton != null)
        {
            var colors = gameplayTabButton.colors;
            colors.normalColor = currentTabIndex == 2 ? selectedColor : normalColor;
            gameplayTabButton.colors = colors;
        }
    }

    /// <summary>
    /// UI를 현재 설정값으로 초기화합니다.
    /// </summary>
    private void InitializeUI()
    {
        if (settingsManager == null)
        {
            Debug.LogError("SettingsManager가 없어 UI를 초기화할 수 없습니다!");
            return;
        }

        GameSettings settings = settingsManager.Settings;

        // 오디오 설정
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = settings.musicVolume;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.value = settings.sfxVolume;
        }

        if (volumeOffsetSlider != null)
        {
            volumeOffsetSlider.minValue = -200f;
            volumeOffsetSlider.maxValue = 200f;
            volumeOffsetSlider.value = settings.volumeOffset;
        }

        if (judgmentOffsetSlider != null)
        {
            judgmentOffsetSlider.minValue = -200f;
            judgmentOffsetSlider.maxValue = 200f;
            judgmentOffsetSlider.value = settings.judgmentOffset;
        }

        // 비주얼 설정
        if (noteSizeSlider != null)
        {
            noteSizeSlider.minValue = 0.5f;
            noteSizeSlider.maxValue = 3f;
            noteSizeSlider.value = settings.noteSize;
        }

        if (trackHeightSlider != null)
        {
            trackHeightSlider.minValue = 5f;
            trackHeightSlider.maxValue = 30f;
            trackHeightSlider.value = settings.trackHeight;
        }

        if (trackAngleSlider != null)
        {
            trackAngleSlider.minValue = -45f;
            trackAngleSlider.maxValue = 45f;
            trackAngleSlider.value = settings.trackAngle;
        }

        if (trackOpacitySlider != null)
        {
            trackOpacitySlider.minValue = 0.1f;
            trackOpacitySlider.maxValue = 1f;
            trackOpacitySlider.value = settings.trackOpacity;
        }

        if (noteScrollSpeedSlider != null)
        {
            noteScrollSpeedSlider.minValue = 1f;
            noteScrollSpeedSlider.maxValue = 20f;
            noteScrollSpeedSlider.value = settings.noteScrollSpeed;
        }

        // 게임플레이 설정
        if (judgmentModeDropdown != null)
        {
            judgmentModeDropdown.ClearOptions();
            judgmentModeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Normal (일반)",
                "Hard (어려움)",
                "Super (최고난이도)"
            });
            judgmentModeDropdown.value = settings.defaultJudgmentMode;
        }

        if (showJudgmentToggle != null)
            showJudgmentToggle.isOn = settings.showJudgmentText;

        if (showOffsetToggle != null)
            showOffsetToggle.isOn = settings.showOffsetText;

        // 텍스트 업데이트
        UpdateAllTexts();
    }

    /// <summary>
    /// 버튼 이벤트 등록
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyButtonClicked);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    /// <summary>
    /// 슬라이더 값 변경 이벤트를 등록합니다.
    /// </summary>
    private void RegisterSliderEvents()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

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

        // 게임플레이 설정
        if (judgmentModeDropdown != null)
            judgmentModeDropdown.onValueChanged.AddListener(OnJudgmentModeChanged);

        if (showJudgmentToggle != null)
            showJudgmentToggle.onValueChanged.AddListener(OnShowJudgmentToggled);

        if (showOffsetToggle != null)
            showOffsetToggle.onValueChanged.AddListener(OnShowOffsetToggled);
    }

    /// <summary>
    /// 모든 텍스트를 현재 값으로 업데이트합니다.
    /// </summary>
    private void UpdateAllTexts()
    {
        if (settingsManager == null) return;

        GameSettings settings = settingsManager.Settings;

        if (musicVolumeText != null)
            musicVolumeText.text = $"{settings.musicVolume:F2}";

        if (sfxVolumeText != null)
            sfxVolumeText.text = $"{settings.sfxVolume:F2}";

        if (volumeOffsetText != null)
            volumeOffsetText.text = $"{settings.volumeOffset:F0}ms";

        if (judgmentOffsetText != null)
            judgmentOffsetText.text = $"{settings.judgmentOffset:F0}ms";

        if (noteSizeText != null)
            noteSizeText.text = $"{settings.noteSize:F2}";

        if (trackHeightText != null)
            trackHeightText.text = $"{settings.trackHeight:F1}";

        if (trackAngleText != null)
            trackAngleText.text = $"{settings.trackAngle:F1}°";

        if (trackOpacityText != null)
            trackOpacityText.text = $"{settings.trackOpacity:F2}";

        if (noteScrollSpeedText != null)
            noteScrollSpeedText.text = $"{settings.noteScrollSpeed:F1}";
    }

    // ============================================
    // 슬라이더 값 변경 핸들러들
    // ============================================

    private void OnMusicVolumeChanged(float value)
    {
        if (settingsManager != null)
        {
            settingsManager.SetMusicVolume(value);

            // 실시간 프리뷰 - 기존 SetBGMVolume 메서드 사용
            if (enableRealtimePreview && audioManager != null)
            {
                audioManager.SetBGMVolume(value);
            }
        }

        if (musicVolumeText != null)
            musicVolumeText.text = $"{value:F2}";
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (settingsManager != null)
        {
            settingsManager.SetSFXVolume(value);

            // 실시간 프리뷰
            if (enableRealtimePreview && audioManager != null)
            {
                audioManager.SetSFXVolume(value);
            }
        }

        if (sfxVolumeText != null)
            sfxVolumeText.text = $"{value:F2}";
    }

    private void OnVolumeOffsetChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetVolumeOffset(value);

        if (volumeOffsetText != null)
            volumeOffsetText.text = $"{value:F0}ms";
    }

    private void OnJudgmentOffsetChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetJudgmentOffset(value);

        if (judgmentOffsetText != null)
            judgmentOffsetText.text = $"{value:F0}ms";
    }

    private void OnNoteSizeChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetNoteSize(value);

        if (noteSizeText != null)
            noteSizeText.text = $"{value:F2}";
    }

    private void OnTrackHeightChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetTrackHeight(value);

        if (trackHeightText != null)
            trackHeightText.text = $"{value:F1}";
    }

    private void OnTrackAngleChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetTrackAngle(value);

        if (trackAngleText != null)
            trackAngleText.text = $"{value:F1}°";
    }

    private void OnTrackOpacityChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetTrackOpacity(value);

        if (trackOpacityText != null)
            trackOpacityText.text = $"{value:F2}";
    }

    private void OnNoteScrollSpeedChanged(float value)
    {
        if (settingsManager != null)
            settingsManager.SetNoteScrollSpeed(value);

        if (noteScrollSpeedText != null)
            noteScrollSpeedText.text = $"{value:F1}";
    }

    private void OnJudgmentModeChanged(int index)
    {
        if (settingsManager != null)
            settingsManager.SetDefaultJudgmentMode(index);
    }

    private void OnShowJudgmentToggled(bool isOn)
    {
        if (settingsManager != null)
            settingsManager.SetShowJudgmentText(isOn);
    }

    private void OnShowOffsetToggled(bool isOn)
    {
        if (settingsManager != null)
            settingsManager.SetShowOffsetText(isOn);
    }

    // ============================================
    // 버튼 핸들러들
    // ============================================

    /// <summary>
    /// 적용 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnApplyButtonClicked()
    {
        if (settingsManager != null)
        {
            settingsManager.SaveSettings();
            Debug.Log("설정이 저장되었습니다.");
        }
        else
        {
            Debug.LogError("SettingsManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 리셋 버튼 클릭 시 호출됩니다.
    /// 모든 설정을 기본값으로 되돌립니다.
    /// </summary>
    public void OnResetButtonClicked()
    {
        if (settingsManager != null)
        {
            settingsManager.ResetToDefault();
            InitializeUI();
            Debug.Log("설정이 기본값으로 리셋되었습니다.");
        }
        else
        {
            Debug.LogError("SettingsManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 뒤로 가기 버튼 클릭 시 호출됩니다.
    /// 이전 씬으로 돌아갑니다.
    /// </summary>
    public void OnBackButtonClicked()
    {
        Debug.Log($"이전 씬({previousSceneName})으로 돌아갑니다.");

        if (!string.IsNullOrEmpty(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            Debug.LogWarning("이전 씬 이름이 설정되지 않았습니다!");
        }
    }

    void OnDestroy()
    {
        // 슬라이더 이벤트 해제
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

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

        if (judgmentModeDropdown != null)
            judgmentModeDropdown.onValueChanged.RemoveListener(OnJudgmentModeChanged);

        if (showJudgmentToggle != null)
            showJudgmentToggle.onValueChanged.RemoveListener(OnShowJudgmentToggled);

        if (showOffsetToggle != null)
            showOffsetToggle.onValueChanged.RemoveListener(OnShowOffsetToggled);

        // 버튼 이벤트 해제
        if (applyButton != null)
            applyButton.onClick.RemoveListener(OnApplyButtonClicked);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetButtonClicked);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);

        if (audioTabButton != null)
            audioTabButton.onClick.RemoveAllListeners();

        if (visualTabButton != null)
            visualTabButton.onClick.RemoveAllListeners();

        if (gameplayTabButton != null)
            gameplayTabButton.onClick.RemoveAllListeners();
    }
}
