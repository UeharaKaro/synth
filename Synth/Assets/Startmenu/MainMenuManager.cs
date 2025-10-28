using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 메뉴 관리 스크립트
/// PLAY, OPTIONS, EXIT 버튼으로 구성된 시작 화면을 관리합니다.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("메뉴 버튼")]
    [Tooltip("PLAY 버튼 - 일반 플레이 모드")]
    public Button playButton;

    [Tooltip("COURSE 버튼 - 코스 모드")]
    public Button courseButton;

    [Tooltip("OPTION 버튼 - 설정 화면으로 이동")]
    public Button optionButton;

    [Tooltip("EXIT 버튼 - 게임 종료")]
    public Button exitButton;

    [Header("메뉴 버튼 리스트")]
    [Tooltip("상호작용할 버튼들을 순서대로 여기에 등록합니다 (Play, Course, Option, Exit)")]
    public List<Button> menuButtons;

    [Header("선택 시 시각적 효과")]
    [Tooltip("선택된 버튼의 색상")]
    public Color selectedColor = Color.yellow;

    [Tooltip("기본 버튼 색상")]
    public Color normalColor = Color.white;

    [Header("기본 설정")]
    [Tooltip("기본 판정 모드 (Play 버튼 클릭 시 사용)")]
    public JudgmentMode defaultJudgmentMode = JudgmentMode.Normal;

    private int currentButtonIndex = 0;

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (courseButton != null)
        {
            courseButton.onClick.AddListener(OnCourseButtonClicked);
        }

        if (optionButton != null)
        {
            optionButton.onClick.AddListener(OnOptionButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        // 메뉴 버튼 리스트가 비어있으면 자동으로 채우기 (순서: Play, Course, Option, Exit)
        if (menuButtons == null || menuButtons.Count == 0)
        {
            menuButtons = new List<Button>();
            if (playButton != null) menuButtons.Add(playButton);
            if (courseButton != null) menuButtons.Add(courseButton);
            if (optionButton != null) menuButtons.Add(optionButton);
            if (exitButton != null) menuButtons.Add(exitButton);
        }

        // 첫 번째 버튼 선택
        if (menuButtons.Count > 0)
        {
            SelectButton(currentButtonIndex);
        }
    }

    void Update()
    {
        HandleKeyboardNavigation();
    }

    /// <summary>
    /// 키보드 입력을 처리하여 메뉴를 네비게이션합니다.
    /// </summary>
    private void HandleKeyboardNavigation()
    {
        if (menuButtons == null || menuButtons.Count == 0) return;

        // 아래 방향키 또는 S키: 다음 버튼으로 이동
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentButtonIndex++;
            if (currentButtonIndex >= menuButtons.Count)
            {
                currentButtonIndex = 0; // 순환
            }
            SelectButton(currentButtonIndex);
        }
        // 위 방향키 또는 W키: 이전 버튼으로 이동
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentButtonIndex--;
            if (currentButtonIndex < 0)
            {
                currentButtonIndex = menuButtons.Count - 1; // 순환
            }
            SelectButton(currentButtonIndex);
        }

        // Enter 키 또는 스페이스바: 현재 선택된 버튼 클릭
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentButtonIndex >= 0 && currentButtonIndex < menuButtons.Count)
            {
                menuButtons[currentButtonIndex].onClick.Invoke();
            }
        }
    }

    /// <summary>
    /// 특정 인덱스의 버튼을 선택하고 시각적으로 표시합니다.
    /// </summary>
    private void SelectButton(int index)
    {
        if (menuButtons == null || index < 0 || index >= menuButtons.Count) return;

        // 모든 버튼을 기본 색상으로 설정
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (menuButtons[i] != null)
            {
                var colors = menuButtons[i].colors;
                colors.normalColor = normalColor;
                menuButtons[i].colors = colors;
            }
        }

        // 선택된 버튼만 강조 색상으로 변경
        if (menuButtons[index] != null)
        {
            var selectedColors = menuButtons[index].colors;
            selectedColors.normalColor = selectedColor;
            menuButtons[index].colors = selectedColors;
        }

        currentButtonIndex = index;
    }
    // ===========================================
    // 버튼 클릭 이벤트 핸들러
    // ===========================================

    /// <summary>
    /// PLAY 버튼 클릭 시 호출됩니다.
    /// 기본 판정 모드를 설정하고 곡 선택 화면으로 이동합니다.
    /// </summary>
    public void OnPlayButtonClicked()
    {
        Debug.Log($"PLAY 버튼 클릭 - 판정 모드: {defaultJudgmentMode}");

        // 기본 판정 모드 설정
        if (JudgmentModeManager.Instance != null)
        {
            JudgmentModeManager.Instance.CurrentMode = defaultJudgmentMode;
        }
        else
        {
            Debug.LogWarning("JudgmentModeManager가 존재하지 않습니다!");
        }

        // 곡 선택 씬으로 전환
        SceneManager.LoadScene(SceneNames.SONG_SELECTION);
    }

    /// <summary>
    /// COURSE 버튼 클릭 시 호출됩니다.
    /// 코스 모드로 곡 선택 화면으로 이동합니다.
    /// </summary>
    public void OnCourseButtonClicked()
    {
        Debug.Log("COURSE 버튼 클릭 - 코스 모드");

        // 코스 모드 설정 (추후 구현 예정)
        // 현재는 기본 판정 모드로 곡 선택 씬으로 이동
        if (JudgmentModeManager.Instance != null)
        {
            JudgmentModeManager.Instance.CurrentMode = defaultJudgmentMode;
        }

        // 곡 선택 씬으로 전환 (추후 코스 전용 씬으로 변경 가능)
        SceneManager.LoadScene(SceneNames.SONG_SELECTION);
    }

    /// <summary>
    /// OPTION 버튼 클릭 시 호출됩니다.
    /// 설정 화면으로 이동합니다.
    /// </summary>
    public void OnOptionButtonClicked()
    {
        Debug.Log("OPTION 버튼 클릭 - 설정 화면으로 이동");
        SceneManager.LoadScene(SceneNames.OPTIONS);
    }

    /// <summary>
    /// EXIT 버튼 클릭 시 호출됩니다.
    /// 게임을 종료합니다.
    /// </summary>
    public void OnExitButtonClicked()
    {
        Debug.Log("EXIT 버튼 클릭 - 게임 종료");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}