using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요
using System.Collections.Generic; // List 사용을 위해 필요
using UnityEngine.SceneManagement; // 씬 잔환을 위해 필요

public class MainMenuManager : MonoBehaviour
{
    [Header("메뉴 버튼 리스트")] [Tooltip("상호작용할 버튼들을 순서대로 여기에 등록합니다")]
    public List<Button> menuButtons; // 메뉴 버튼들을 저장할 리스트

    [Header("선택 시 시각적 효과")] [Tooltip("선택된 버튼의 색상")]
    public Color selectedColor = Color.yellow; // 선택된 버튼의 색상

    [Tooltip("기본 버튼 색상")] public Color normalColor = Color.white; // 기본 버튼 색상

    private int currentSelectionIndex = 0; // 현재 선택된 버튼 인덱스

    void Start()
    {
        // 게임 시작 시 첫 번째 버튼을 선택된 상태로 만듬
        SelectButton(currentButtonIndex);
    }

    void Update()
    {
        // 방향키 입력 처리
        // 아래 방향키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // 다음 버튼으로 이동
            currentSelectionIndex++;
            // 만약 마지막 버튼을 넘어갔다면 첫 번째 버튼으로 순환
            if (currentSelectionIndex >= menuButtons.Count)
            {
                currentSelectionIndex = 0;
            }

            SelectButton(currentSelectionIndex);
        }
        // 위 방향키를 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 이전 버튼으로 이동
            currentSelectionIndex--;
            // 만약 첫 번째 버튼을 넘어갔다면 마지막 버튼으로 순환
            if (currentSelectionIndex < 0)
            {
                currentSelectionIndex = menuButtons.Count - 1;
            }

            SelectButton(currentSelectionIndex);
        }

        // 선택 및 실행 처리
        // Enter 키 or 마우스 왼쪽 클릭을 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            // 현재 선택된 버튼의 기능을 실행
            menuButtons[currentSelectionIndex].onClick.Invoke();
        }
    }

    // 특정 인덱스의 버튼을 선택하고 시각적으로 표시하는 함수
    void SelectButton(int index)
    {
        // 모든 버튼을 기본 색상으로 설정
        for (int i = 0; i < menuButtons.Count; i++)
        {
            // Selectable 컴포넌트를 통헤 색상 제어
            var colors = menuButtons[i].colors;
            colors.normalColor = normalColor; // 기본 색상으로 설정
            menuButtons[i].colors = colors;
        }

        // 선택된 버튼만 강조 색상으로 변경
        var selectedButton = menuButtons[index].colors;
        selectedButton.normalColor = selectedColor; // 선택된 색상으로 설정
        menuButtons[index].colors = selectedColor;

        currentButtonIndex = index;
    }
    // 각 버튼에 연결될 함수들
    // 이 함수들은 Unity Editor에서 버튼의 OnClick 이벤트에 연결할 예정

    public void OnNOrmalModeClicked()
    {
        Debug.Log("Normal 모드 선택됨");
        GameSettingsManager.Instance.CurrentMode = JudgmentMode.JudgmentMode_Normal;
        SceneManager.LoadScene("SongSelectionScene"); // 곡선택 씬으로 전환
    }

    public void OnHardModeClicked()
    {
        Debug.Log("Hard 모드 선택됨");
        GameSettingsManager.Instance.CurrentMode = JudgmentMode.JudgmentMode_Hard;
        SceneManager.LoadScene("SongSelectionScene"); // 곡 선택 씬으로 전환
    }

    /* public void OnSuperModeClicked()
    {
        Debug.Log("Super 모드 선택됨");
        GameSettingsManager.Instance.CurrentMode = JudgmentMode.JudgmentMode_Super;
        SceneManager.LoadScene("SongSelectionScene"); // 곡 선택 씬으로 전환
    } */ // 추후에 Super 모드 출시시 활성화
    public void OnCourseModeClicked()
    {
        Debug.Log("Course 모드 선택됨");
        // GameSettingsManager.Instance.CurrentMode = JudgmentMode.JudgmentMode_Course; // Course 모드가 추가되면  Course 전용 판정 추가예쩡
        SceneManager.LoadScene("SongSelectionScene"); // 코스 모드 씬으로 전환
    }

    public void OnOptionsClicked()
    {
        Debug.Log("Options 메뉴 선택됨");
        SceneManager.LoadScene("OptionsScene"); // 옵션 씬으로 전환
    }
}