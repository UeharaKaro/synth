using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 곡 선택 화면에서 ESC 키로 표시되는 메뉴
/// 메인 메뉴로 돌아가기, 옵션, 게임 종료 등의 기능 제공
/// </summary>
public class SongSelectionESCMenu : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject escMenuPanel;
    [SerializeField] private Button backToMainMenuButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private Button cancelButton;

    [Header("텍스트 참조 (선택)")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI backText;
    [SerializeField] private TextMeshProUGUI optionsText;
    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private TextMeshProUGUI cancelText;

    [Header("설정")]
    [SerializeField] private KeyCode escKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string optionsSceneName = "OptionsScene";
    [SerializeField] private bool showConfirmDialog = true; // 메인 메뉴로 돌아갈 때 확인 다이얼로그 표시

    private bool isMenuOpen = false;

    void Start()
    {
        // 버튼 이벤트 연결
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(OnQuitGameClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        // 텍스트 설정
        SetupTexts();

        // 시작 시 메뉴 숨김
        HideMenu();
    }

    void Update()
    {
        // ESC 키로 메뉴 토글
        if (Input.GetKeyDown(escKey))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// 텍스트 설정
    /// </summary>
    private void SetupTexts()
    {
        if (titleText != null)
            titleText.text = "메뉴";

        if (backText != null)
            backText.text = "메인 메뉴로";

        if (optionsText != null)
            optionsText.text = "설정";

        if (quitText != null)
            quitText.text = "게임 종료";

        if (cancelText != null)
            cancelText.text = "취소";
    }

    /// <summary>
    /// 메뉴 토글
    /// </summary>
    public void ToggleMenu()
    {
        if (isMenuOpen)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    /// <summary>
    /// 메뉴 표시
    /// </summary>
    public void ShowMenu()
    {
        if (isMenuOpen) return;

        isMenuOpen = true;

        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(true);
        }

        // 미리듣기 일시정지 (AudioManager가 있다면)
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PauseBGM();
        }

        Debug.Log("SongSelectionESCMenu: 메뉴 열림");
    }

    /// <summary>
    /// 메뉴 숨김
    /// </summary>
    public void HideMenu()
    {
        if (!isMenuOpen) return;

        isMenuOpen = false;

        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(false);
        }

        // 미리듣기 재개 (AudioManager가 있다면)
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.ResumeBGM();
        }

        Debug.Log("SongSelectionESCMenu: 메뉴 닫힘");
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기 버튼 클릭
    /// </summary>
    private void OnBackToMainMenuClicked()
    {
        Debug.Log("SongSelectionESCMenu: 메인 메뉴로 이동");

        // 확인 다이얼로그 표시 옵션
        if (showConfirmDialog)
        {
            // TODO: 확인 다이얼로그 구현
            // "정말 메인 메뉴로 돌아가시겠습니까?"
            // 지금은 바로 이동
        }

        // AudioManager 정리
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

        // 메인 메뉴로 이동
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// 설정 버튼 클릭
    /// </summary>
    private void OnOptionsClicked()
    {
        Debug.Log("SongSelectionESCMenu: 설정 화면 열기");

        // TODO: 설정 씬이 준비되면 활성화
        // SceneManager.LoadScene(optionsSceneName);

        // 또는 오버레이 방식으로 설정 패널 표시
        // OptionsPanel.SetActive(true);
    }

    /// <summary>
    /// 게임 종료 버튼 클릭
    /// </summary>
    private void OnQuitGameClicked()
    {
        Debug.Log("SongSelectionESCMenu: 게임 종료");

        // AudioManager 정리
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

#if UNITY_EDITOR
        // 에디터에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드에서는 애플리케이션 종료
        Application.Quit();
#endif
    }

    /// <summary>
    /// 취소 버튼 클릭
    /// </summary>
    private void OnCancelClicked()
    {
        HideMenu();
    }

    /// <summary>
    /// 정리
    /// </summary>
    void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.RemoveListener(OnBackToMainMenuClicked);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OnOptionsClicked);

        if (quitGameButton != null)
            quitGameButton.onClick.RemoveListener(OnQuitGameClicked);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancelClicked);
    }
}
