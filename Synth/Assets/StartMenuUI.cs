using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요
using TMPro;
using UnityEngine.PlayerLoop; // TextMeshPro를 사용한다면 사용

public class StartMenuUI : MonoBehaviour
{
    // Inspector 창에서 연결한 UI 텍스트
    [Header("UI Elements")] [Tooltip("현재 선택된 판정 모드를 표시할 텍스트")]
    public TextMeshProUGUI modeDisplayText; // or public Text modeDisplayText;

    // 선택 가능한 모드를 배열로 관리 (Super 제외)
    private JudgmentMode[] selectableModes =
    {
        JudgmentMode.JudgmentMode_Normal, JudgmentMode.JudgmentMode_Hard /*,JudgmentMode.JudgmentMode_Super*/
    }; // 추후에 Super 모드 출시시 추가

    private int currentModeIndex = 0;

    //스크립트가 활성화 될 때 한 번 호출
    private void Start()
    {
        // 시작할때 GameSettingManager에 저장된 모드나 기본값으로 Ui를 업데이트
        currentModeIndex = (int)GameSettingsManager.Instance.CurrentMode;
        UpdateUI();
    }

    // 모드를 오른쪽(다음)으로 변경하는 함수 (+버튼에 연결)
    public void CycleModeForward()
    {
        currentModeIndex++;
        if (currentModeIndex >= selectableModes.Length)
        {
            currentModeIndex = 0; // 배열의 끝에 도달하면 처음으로 돌아감
        }

        UpdateUI();
    }

    // 모드를 왼쪽(이전)으로 변경하는 함수 (-버튼에 연결)
    public void CycleModeBackward()
    {
        currentModeIndex--;
        if (currentModeIndex < 0)
        {
            currentModeIndex = selectableModes.Length - 1; // 배열의 시작에서 뒤로 가면 끝으로 이동
        }

        UpdateUI();
    }

    // UI 텍스트와 실제 게임 설정을 업데이트 하는 함수
    private void UpdateUI()
    {
        // 현재 선택된 모드를 가져옴
        JudgmentMode selectableMode = selectableModes[currentModeIndex];

        // 1. UI 텍스트 업데이트
        if (modeDisplayText != null)
        {
            modeDisplayText.text = selectableMode.ToString(); // "Normal" 또는 "Hard" 텍스트로 표시(Super 도입시 "Super"표시)
        }

        // 2. GameSettingsManager에 지정된 모드저장
        GameSettingsManager.Instance.CurrentMode = selectableMode;

        Debug.Log($"판정모드가 {selectedMode}로 설정 되었습니다.");
    }

    // 게임 시작 버튼에 연결할 함수 
    public void OnStartGameButtonClicked()
    {
        // 여기에 게임 씬을 로드하는 코드를 추가
        /* ex): UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene";
         Debug.Log($"{GameSettingsManager.Instance.CurrentMode} 모드로 게임을 시작합니다."); */
    }
}

    
