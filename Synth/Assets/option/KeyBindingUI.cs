using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 키 바인딩 설정 UI
/// 각 키 모드별로 커스텀 키를 설정할 수 있습니다.
/// ESC를 눌러 키 할당 모드를 종료합니다.
/// </summary>
public class KeyBindingUI : MonoBehaviour
{
    [Header("UI References")]
    public Dropdown keyModeDropdown; // 4K, 5K Standard, 5+1K 등 선택
    public Transform keyButtonContainer; // 키 버튼들이 생성될 컨테이너
    public GameObject keyButtonPrefab; // 키 버튼 프리팹
    public Button resetButton; // 기본값으로 리셋 버튼
    public Button resetAllButton; // 모든 키 바인딩 리셋 버튼
    public Text statusText; // 상태 메시지 표시

    [Header("Key Configuration")]
    private int currentKeyMode = 4; // 현재 선택된 키 모드
    private List<KeyCode> currentKeys = new List<KeyCode>(); // 현재 모드의 키 목록
    private List<Button> keyButtons = new List<Button>(); // 생성된 키 버튼들

    // 키 할당 상태
    private bool isWaitingForKey = false; // 키 입력 대기 중인지
    private int waitingKeyIndex = -1; // 어떤 키를 기다리는지
    private Button waitingButton = null; // 대기 중인 버튼

    // 금지된 키 (플레이용으로 사용 불가)
    private static readonly HashSet<KeyCode> ForbiddenKeys = new HashSet<KeyCode>
    {
        KeyCode.Escape, // ESC는 키 할당 모드 종료용
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.LeftCommand,
        KeyCode.RightCommand,
        KeyCode.LeftWindows,
        KeyCode.RightWindows,
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.Mouse2
    };

    // 키 모드 정보
    private static readonly Dictionary<int, string> KeyModeNames = new Dictionary<int, string>
    {
        { 4, "4K" },
        { 5, "5K Standard" },
        { -5, "5+1K" },
        { 6, "6K" },
        { 7, "7K" },
        { 8, "8K" },
        { 10, "10K" }
    };

    void Start()
    {
        SetupUI();
        LoadCurrentKeyMode();
    }

    void Update()
    {
        if (isWaitingForKey)
        {
            HandleKeyInput();
        }
    }

    void SetupUI()
    {
        // 드롭다운 설정
        if (keyModeDropdown != null)
        {
            keyModeDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var kvp in KeyModeNames)
            {
                options.Add(kvp.Value);
            }
            keyModeDropdown.AddOptions(options);
            keyModeDropdown.onValueChanged.AddListener(OnKeyModeChanged);
        }

        // 리셋 버튼
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        if (resetAllButton != null)
        {
            resetAllButton.onClick.AddListener(OnResetAllClicked);
        }
    }

    void OnKeyModeChanged(int index)
    {
        // 드롭다운 인덱스를 키 모드로 변환
        List<int> keyModes = new List<int>(KeyModeNames.Keys);
        keyModes.Sort();
        currentKeyMode = keyModes[index];

        LoadCurrentKeyMode();
    }

    void LoadCurrentKeyMode()
    {
        // SettingsManager에서 커스텀 키 바인딩 로드
        KeyCode[] customKeys = null;
        if (SettingsManager.Instance != null)
        {
            customKeys = SettingsManager.Instance.GetKeyBindings(currentKeyMode);
        }

        // 커스텀 키가 없으면 기본값 사용
        if (customKeys == null || customKeys.Length == 0)
        {
            currentKeys = GetDefaultKeys(currentKeyMode);
        }
        else
        {
            currentKeys = new List<KeyCode>(customKeys);
        }

        RefreshUI();
    }

    List<KeyCode> GetDefaultKeys(int lineCount)
    {
        switch (lineCount)
        {
            case 4:
                return new List<KeyCode> { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
            case 5:
                return new List<KeyCode> { KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K };
            case -5: // 5+1K
                return new List<KeyCode> { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
            case 6:
                return new List<KeyCode> { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
            case 7:
                return new List<KeyCode> { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K, KeyCode.L };
            case 8:
                return new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
            case 10:
                return new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
            default:
                return new List<KeyCode> { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
        }
    }

    void RefreshUI()
    {
        // 기존 버튼 제거
        foreach (Button btn in keyButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        keyButtons.Clear();

        if (keyButtonContainer == null || keyButtonPrefab == null)
        {
            Debug.LogWarning("KeyBindingUI: keyButtonContainer 또는 keyButtonPrefab이 null입니다.");
            return;
        }

        // 새 버튼 생성
        for (int i = 0; i < currentKeys.Count; i++)
        {
            GameObject btnObj = Instantiate(keyButtonPrefab, keyButtonContainer);
            Button btn = btnObj.GetComponent<Button>();
            Text btnText = btnObj.GetComponentInChildren<Text>();

            if (btn != null)
            {
                int index = i; // 클로저를 위한 로컬 변수
                btn.onClick.AddListener(() => OnKeyButtonClicked(index));
                keyButtons.Add(btn);

                if (btnText != null)
                {
                    // 5+1K 모드에서 중앙 트랙 표시
                    if (currentKeyMode == -5 && index == 2)
                    {
                        btnText.text = $"트랙 {index + 1}: {currentKeys[index]} / {currentKeys[index + 1]}";
                        // 3번 인덱스는 건너뛰기 (F/J가 같은 트랙)
                    }
                    else if (currentKeyMode == -5 && index == 3)
                    {
                        continue; // 이미 2번에서 표시됨
                    }
                    else
                    {
                        int trackNumber = GetTrackNumber(index);
                        btnText.text = $"트랙 {trackNumber}: {currentKeys[index]}";
                    }
                }
            }
        }

        ShowStatus($"{KeyModeNames[currentKeyMode]} 키 바인딩 로드 완료");
    }

    int GetTrackNumber(int index)
    {
        if (currentKeyMode == -5)
        {
            // 5+1K: 0=트랙1, 1=트랙2, 2/3=트랙3, 4=트랙4, 5=트랙5
            if (index <= 1) return index + 1;
            if (index <= 3) return 3;
            return index;
        }
        return index + 1;
    }

    void OnKeyButtonClicked(int index)
    {
        if (isWaitingForKey)
        {
            ShowStatus("이미 키 입력을 기다리고 있습니다. ESC를 눌러 취소하세요.");
            return;
        }

        isWaitingForKey = true;
        waitingKeyIndex = index;
        waitingButton = keyButtons[index];

        // 버튼 색상 변경 (대기 중 표시)
        ColorBlock colors = waitingButton.colors;
        colors.normalColor = Color.yellow;
        waitingButton.colors = colors;

        ShowStatus($"트랙 {GetTrackNumber(index)}에 할당할 키를 누르세요 (ESC로 취소)");
    }

    void HandleKeyInput()
    {
        // ESC로 키 할당 모드 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelKeyWaiting();
            return;
        }

        // 모든 키 체크
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                // 금지된 키 체크
                if (ForbiddenKeys.Contains(key))
                {
                    ShowStatus($"{key}는 사용할 수 없는 키입니다.");
                    continue;
                }

                // 중복 키 체크
                if (currentKeys.Contains(key) && currentKeys[waitingKeyIndex] != key)
                {
                    ShowStatus($"{key}는 이미 사용 중인 키입니다.");
                    continue;
                }

                // 키 할당
                currentKeys[waitingKeyIndex] = key;

                // 5+1K 모드에서 중앙 트랙 처리 (인덱스 2, 3)
                if (currentKeyMode == -5 && waitingKeyIndex == 2)
                {
                    // 키 2개를 동시에 설정해야 함
                    ShowStatus($"중앙 트랙의 첫 번째 키: {key}. 두 번째 키를 눌러주세요.");
                    waitingKeyIndex = 3; // 다음 키 대기
                    return;
                }

                // 설정 저장
                SaveKeyBindings();
                RefreshUI();
                CancelKeyWaiting();

                ShowStatus($"트랙 {GetTrackNumber(waitingKeyIndex)}에 {key} 할당 완료");
                break;
            }
        }
    }

    void CancelKeyWaiting()
    {
        if (waitingButton != null)
        {
            // 버튼 색상 복원
            ColorBlock colors = waitingButton.colors;
            colors.normalColor = Color.white;
            waitingButton.colors = colors;
        }

        isWaitingForKey = false;
        waitingKeyIndex = -1;
        waitingButton = null;

        ShowStatus("키 할당이 취소되었습니다.");
    }

    void SaveKeyBindings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetKeyBindings(currentKeyMode, currentKeys.ToArray());
            ShowStatus("키 바인딩이 저장되었습니다.");
        }
    }

    void OnResetClicked()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetKeyBindings(currentKeyMode);
        }

        LoadCurrentKeyMode();
        ShowStatus($"{KeyModeNames[currentKeyMode]} 키 바인딩이 기본값으로 리셋되었습니다.");
    }

    void OnResetAllClicked()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetAllKeyBindings();
        }

        LoadCurrentKeyMode();
        ShowStatus("모든 키 바인딩이 기본값으로 리셋되었습니다.");
    }

    void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[KeyBindingUI] {message}");
    }

    void OnDestroy()
    {
        // 이벤트 리스너 정리
        if (keyModeDropdown != null)
        {
            keyModeDropdown.onValueChanged.RemoveAllListeners();
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
        }

        if (resetAllButton != null)
        {
            resetAllButton.onClick.RemoveAllListeners();
        }
    }
}
