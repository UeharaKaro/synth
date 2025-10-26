using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 일시정지 메뉴 UI 관리
/// ESC 키로 일시정지/재개 토글
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject pauseMenuPanel;
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

    [Header("설정")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string optionsSceneName = "OptionsScene";
    [SerializeField] private bool enableOnlyInGameplay = true; // 게임플레이 중에만 활성화

    private bool isPaused = false;
    private GameManager gameManager;
    private bool isGameplayActive = false; // 게임플레이 활성 상태

    void Start()
    {
        // GameManager 참조
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("PauseMenuUI: GameManager가 없습니다!");
            
            // GameManager가 없으면 일시정지 비활성화
            if (enableOnlyInGameplay)
            {
                isGameplayActive = false;
                enabled = false; // Update 중단
                return;
            }
        }
        else
        {
            // GameManager가 있으면 게임플레이 활성화
            isGameplayActive = true;
        }

        // 버튼 이벤트 연결
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // 텍스트 설정
        SetupTexts();

        // 시작 시 일시정지 메뉴 숨김
        HidePauseMenu();
    }

    void Update()
    {
        // 게임플레이 활성화 체크
        if (enableOnlyInGameplay && !isGameplayActive)
            return;

        // 일시정지 중에는 Time.timeScale=0이지만 UI는 작동해야 함
        // ESC 키로 일시정지 토글
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
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
    /// 일시정지 활성화/비활성화 (외부에서 제어 가능)
    /// </summary>
    public void SetGameplayActive(bool active)
    {
        isGameplayActive = active;
        
        if (!active && isPaused)
        {
            // 게임플레이가 비활성화되면 일시정지 해제
            Resume();
        }
        
        Debug.Log($"PauseMenuUI: 게임플레이 활성화 = {active}");
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
        ShowPauseMenu();

        // GameManager에 일시정지 알림
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
        else
        {
            // GameManager가 없으면 직접 처리
            Time.timeScale = 0f;
        }

        Debug.Log("PauseMenuUI: 게임 일시정지");
    }

    /// <summary>
    /// 재개
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        HidePauseMenu();

        // GameManager에 재개 알림
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
        else
        {
            // GameManager가 없으면 직접 처리
            Time.timeScale = 1f;
        }

        Debug.Log("PauseMenuUI: 게임 재개");
    }

    /// <summary>
    /// 일시정지 메뉴 표시
    /// </summary>
    private void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 일시정지 메뉴 숨김
    /// </summary>
    private void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 재개 버튼 클릭
    /// </summary>
    private void OnResumeClicked()
    {
        Resume();
    }

    /// <summary>
    /// 재시작 버튼 클릭
    /// </summary>
    private void OnRestartClicked()
    {
        // 일시정지 해제
        Time.timeScale = 1f;
        isPaused = false;

        // GameManager를 통한 재시작
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            // GameManager가 없으면 직접 씬 재로드
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        Debug.Log("PauseMenuUI: 게임 재시작");
    }

    /// <summary>
    /// 설정 버튼 클릭
    /// </summary>
    private void OnOptionsClicked()
    {
        // 설정 씬으로 이동 (구현 필요)
        Debug.Log("PauseMenuUI: 설정 화면 열기 (구현 필요)");

        // TODO: 설정 씬이 준비되면 활성화
        // Time.timeScale = 1f;
        // SceneManager.LoadScene(optionsSceneName);
    }

    /// <summary>
    /// 메인 메뉴 버튼 클릭
    /// </summary>
    private void OnMainMenuClicked()
    {
        // 일시정지 해제
        Time.timeScale = 1f;
        isPaused = false;

        // 메인 메뉴로 이동
        SceneManager.LoadScene(mainMenuSceneName);

        Debug.Log("PauseMenuUI: 메인 메뉴로 이동");
    }

    /// <summary>
    /// 정리
    /// </summary>
    void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OnOptionsClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

        // 만약을 위한 timeScale 복원
        Time.timeScale = 1f;
    }
}
