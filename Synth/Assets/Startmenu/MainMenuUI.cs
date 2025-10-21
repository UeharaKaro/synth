using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 메인 메뉴 UI 관리 스크립트
/// PLAY, OPTION, EXIT 버튼으로 구성된 시작 화면을 관리합니다.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("메뉴 버튼")]
    [Tooltip("플레이 버튼 - 곡 선택 화면으로 이동")]
    public Button playButton;

    [Tooltip("옵션 버튼 - 설정 화면으로 이동")]
    public Button optionButton;

    [Tooltip("종료 버튼 - 게임 종료")]
    public Button exitButton;

    [Header("씬 이름 설정")]
    [Tooltip("곡 선택 씬 이름")]
    public string songSelectionSceneName = "SongSelectionScene";

    [Tooltip("옵션 씬 이름")]
    public string optionSceneName = "OptionScene";

    [Header("키보드 네비게이션 설정")]
    [Tooltip("메뉴 버튼들 (순서대로)")]
    public List<Button> menuButtons;

    [Header("선택 시 시각적 효과")]
    [Tooltip("선택된 버튼의 색상")]
    public Color selectedColor = Color.yellow;

    [Tooltip("기본 버튼 색상")]
    public Color normalColor = Color.white;

    private int currentButtonIndex = 0; // 현재 선택된 버튼 인덱스

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (optionButton != null)
        {
            optionButton.onClick.AddListener(OnOptionButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        // 메뉴 버튼 리스트가 비어있으면 자동으로 채우기
        if (menuButtons == null || menuButtons.Count == 0)
        {
            menuButtons = new List<Button>();
            if (playButton != null) menuButtons.Add(playButton);
            if (optionButton != null) menuButtons.Add(optionButton);
            if (exitButton != null) menuButtons.Add(exitButton);
        }

        // 첫 번째 버튼 선택
        if (menuButtons.Count > 0)
        {
            SelectButton(0);
        }
    }

    void Update()
    {
        // 키보드 네비게이션
        HandleKeyboardNavigation();
    }

    /// <summary>
    /// 키보드 입력을 처리하여 메뉴를 네비게이션합니다.
    /// </summary>
    private void HandleKeyboardNavigation()
    {
        if (menuButtons == null || menuButtons.Count == 0) return;

        // 아래 방향키: 다음 버튼으로 이동
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentButtonIndex++;
            if (currentButtonIndex >= menuButtons.Count)
            {
                currentButtonIndex = 0; // 순환
            }
            SelectButton(currentButtonIndex);
        }
        // 위 방향키: 이전 버튼으로 이동
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

    /// <summary>
    /// PLAY 버튼 클릭 시 호출됩니다.
    /// 곡 선택 화면으로 이동합니다.
    /// </summary>
    public void OnPlayButtonClicked()
    {
        Debug.Log("PLAY 버튼 클릭 - 곡 선택 화면으로 이동");

        // 곡 선택 씬으로 전환
        if (!string.IsNullOrEmpty(songSelectionSceneName))
        {
            SceneManager.LoadScene(songSelectionSceneName);
        }
        else
        {
            Debug.LogWarning("곡 선택 씬 이름이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// OPTION 버튼 클릭 시 호출됩니다.
    /// 설정 조절 화면으로 이동합니다.
    /// </summary>
    public void OnOptionButtonClicked()
    {
        Debug.Log("OPTION 버튼 클릭 - 설정 화면으로 이동");

        // 옵션 씬으로 전환
        if (!string.IsNullOrEmpty(optionSceneName))
        {
            SceneManager.LoadScene(optionSceneName);
        }
        else
        {
            Debug.LogWarning("옵션 씬 이름이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// EXIT 버튼 클릭 시 호출됩니다.
    /// 게임을 종료합니다.
    /// </summary>
    public void OnExitButtonClicked()
    {
        Debug.Log("EXIT 버튼 클릭 - 게임 종료");

        // 에디터에서 실행 중일 때
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 빌드된 게임에서 실행 중일 때
        Application.Quit();
        #endif
    }
}
