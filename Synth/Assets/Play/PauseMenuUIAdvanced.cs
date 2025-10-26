using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// 일시정지 메뉴 UI 관리 (고급 버전)
/// ESC 키로 일시정지/재개 토글
/// 애니메이션, 사운드 이펙트, 입력 차단 기능 포함
/// </summary>
public class PauseMenuUIAdvanced : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private CanvasGroup canvasGroup; // 페이드 애니메이션용
    [SerializeField] private RectTransform panelTransform; // 스케일 애니메이션용
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;

    [Header("텍스트 참조 (선택)")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI resumeText;
    [SerializeField] private TextMeshProUGUI restartText;
    [SerializeField] private TextMeshProUGUI optionsText;
    [SerializeField] private TextMeshProUGUI mainMenuText;

    [Header("애니메이션 설정")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("스케일 애니메이션")]
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private Vector3 startScale = new Vector3(0.85f, 0.85f, 1f);
    [SerializeField] private Vector3 targetScale = Vector3.one;
    [SerializeField] private float scaleAnimationDuration = 0.25f;

    [Header("사운드 설정")]
    [SerializeField] private bool enableSounds = true;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.7f;

    [Header("설정")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string optionsSceneName = "OptionsScene";
    [SerializeField] private bool enableOnlyInGameplay = true;
    [SerializeField] private bool blockGameInputWhenPaused = true;

    private bool isPaused = false;
    private GameManager gameManager;
    private bool isGameplayActive = false;
    private Coroutine currentAnimation;
    private AudioSource uiAudioSource;

    void Awake()
    {
        // 오디오 소스 자동 생성
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.volume = soundVolume;
        }

        // CanvasGroup 자동 생성
        if (canvasGroup == null && pauseMenuPanel != null)
        {
            canvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
            }
        }

        // RectTransform 참조
        if (panelTransform == null && pauseMenuPanel != null)
        {
            panelTransform = pauseMenuPanel.GetComponent<RectTransform>();
        }
    }

    void Start()
    {
        // GameManager 참조
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("PauseMenuUIAdvanced: GameManager가 없습니다!");
            
            if (enableOnlyInGameplay)
            {
                isGameplayActive = false;
                enabled = false;
                return;
            }
        }
        else
        {
            isGameplayActive = true;
        }

        // 버튼 이벤트 연결
        SetupButtons();

        // 텍스트 설정
        SetupTexts();

        // 시작 시 일시정지 메뉴 숨김
        HidePauseMenuImmediate();
    }

    void Update()
    {
        if (enableOnlyInGameplay && !isGameplayActive)
            return;

        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 버튼 설정
    /// </summary>
    private void SetupButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
            AddButtonSounds(resumeButton);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            AddButtonSounds(restartButton);
        }

        if (optionsButton != null)
        {
            restartButton.onClick.AddListener(OnOptionsClicked);
            AddButtonSounds(optionsButton);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            AddButtonSounds(mainMenuButton);
        }
    }

    /// <summary>
    /// 버튼에 사운드 추가
    /// </summary>
    private void AddButtonSounds(Button button)
    {
        if (!enableSounds) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // 호버 사운드
        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => { PlaySound(buttonHoverSound); });
        trigger.triggers.Add(hoverEntry);
    }

    /// <summary>
    /// 텍스트 설정
    /// </summary>
    private void SetupTexts()
    {
        if (titleText != null)
            titleText.text = "일시정지";

        if (resumeText != null)
            resumeText.text = "재개";

        if (restartText != null)
            restartText.text = "재시작";

        if (optionsText != null)
            optionsText.text = "설정";

        if (mainMenuText != null)
            mainMenuText.text = "메인 메뉴";
    }

    /// <summary>
    /// 일시정지 활성화/비활성화
    /// </summary>
    public void SetGameplayActive(bool active)
    {
        isGameplayActive = active;
        
        if (!active && isPaused)
        {
            Resume();
        }
        
        Debug.Log($"PauseMenuUIAdvanced: 게임플레이 활성화 = {active}");
    }

    /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    /// <summary>
    /// 일시정지
    /// </summary>
    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;

        // GameManager에 일시정지 알림
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }

        // 애니메이션과 함께 메뉴 표시
        if (enableAnimations)
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(ShowPauseMenuAnimated());
        }
        else
        {
            ShowPauseMenuImmediate();
        }

        // 사운드 재생
        PlaySound(pauseSound);

        Debug.Log("PauseMenuUIAdvanced: 게임 일시정지");
    }

    /// <summary>
    /// 재개
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;

        // 애니메이션과 함께 메뉴 숨김
        if (enableAnimations)
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(HidePauseMenuAnimated());
        }
        else
        {
            HidePauseMenuImmediate();
        }

        // GameManager에 재개 알림
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
        }

        // 사운드 재생
        PlaySound(resumeSound);

        Debug.Log("PauseMenuUIAdvanced: 게임 재개");
    }

    /// <summary>
    /// 일시정지 메뉴 표시 (애니메이션)
    /// </summary>
    private IEnumerator ShowPauseMenuAnimated()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        float elapsed = 0f;
        float duration = Mathf.Max(fadeInDuration, scaleAnimationDuration);

        // 초기 상태 설정
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (panelTransform != null && enableScaleAnimation)
        {
            panelTransform.localScale = startScale;
        }

        while (elapsed < duration)
        {
            // unscaledDeltaTime 사용 (Time.timeScale=0이어도 작동)
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;

            // 페이드 인
            if (canvasGroup != null && elapsed < fadeInDuration)
            {
                float fadeProgress = elapsed / fadeInDuration;
                canvasGroup.alpha = fadeInCurve.Evaluate(fadeProgress);
            }

            // 스케일 애니메이션
            if (panelTransform != null && enableScaleAnimation && elapsed < scaleAnimationDuration)
            {
                float scaleProgress = elapsed / scaleAnimationDuration;
                panelTransform.localScale = Vector3.Lerp(startScale, targetScale, scaleProgress);
            }

            yield return null;
        }

        // 최종 상태
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        if (panelTransform != null)
        {
            panelTransform.localScale = targetScale;
        }

        currentAnimation = null;
    }

    /// <summary>
    /// 일시정지 메뉴 숨김 (애니메이션)
    /// </summary>
    private IEnumerator HidePauseMenuAnimated()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeOutDuration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = fadeOutCurve.Evaluate(progress);
            }

            yield return null;
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        currentAnimation = null;
    }

    /// <summary>
    /// 일시정지 메뉴 즉시 표시
    /// </summary>
    private void ShowPauseMenuImmediate()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (panelTransform != null)
        {
            panelTransform.localScale = targetScale;
        }
    }

    /// <summary>
    /// 일시정지 메뉴 즉시 숨김
    /// </summary>
    private void HidePauseMenuImmediate()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// 사운드 재생
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (!enableSounds || clip == null || uiAudioSource == null)
            return;

        uiAudioSource.PlayOneShot(clip, soundVolume);
    }

    /// <summary>
    /// 재개 버튼 클릭
    /// </summary>
    private void OnResumeClicked()
    {
        PlaySound(buttonClickSound);
        Resume();
    }

    /// <summary>
    /// 재시작 버튼 클릭
    /// </summary>
    private void OnRestartClicked()
    {
        PlaySound(buttonClickSound);
        
        Time.timeScale = 1f;
        isPaused = false;

        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        Debug.Log("PauseMenuUIAdvanced: 게임 재시작");
    }

    /// <summary>
    /// 설정 버튼 클릭
    /// </summary>
    private void OnOptionsClicked()
    {
        PlaySound(buttonClickSound);
        Debug.Log("PauseMenuUIAdvanced: 설정 화면 열기 (구현 필요)");
    }

    /// <summary>
    /// 메인 메뉴 버튼 클릭
    /// </summary>
    private void OnMainMenuClicked()
    {
        PlaySound(buttonClickSound);
        
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);

        Debug.Log("PauseMenuUIAdvanced: 메인 메뉴로 이동");
    }

    /// <summary>
    /// 정리
    /// </summary>
    void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OnOptionsClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

        Time.timeScale = 1f;
    }
}
