using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 고급 키 바인딩 설정 UI
///
/// 기능:
/// 1. 키 바인딩 테스트 모드
/// 2. 프리셋 저장/로드
/// 3. 내보내기/가져오기
/// </summary>
public class KeyBindingUIAdvanced : MonoBehaviour
{
    [Header("UI References - Basic")]
    public Dropdown keyModeDropdown;
    public Transform keyButtonContainer;
    public GameObject keyButtonPrefab;
    public Text statusText;

    [Header("UI References - Test Mode")]
    public GameObject testModePanel;
    public Button testModeButton;
    public Transform testTrackContainer;
    public GameObject testTrackPrefab;

    [Header("UI References - Presets")]
    public InputField presetNameInput;
    public Button savePresetButton;
    public Dropdown presetDropdown;
    public Button loadPresetButton;
    public Button deletePresetButton;

    [Header("UI References - Import/Export")]
    public Button exportButton;
    public Button importButton;
    public Button copyToClipboardButton;
    public InputField importTextField;

    [Header("UI References - Reset")]
    public Button resetButton;
    public Button resetAllButton;

    // 키 설정
    private int currentKeyMode = 4;
    private List<KeyCode> currentKeys = new List<KeyCode>();
    private List<Button> keyButtons = new List<Button>();

    // 키 할당 상태
    private bool isWaitingForKey = false;
    private int waitingKeyIndex = -1;
    private Button waitingButton = null;

    // 테스트 모드
    private bool isTestMode = false;
    private List<Image> testTracks = new List<Image>();

    // 금지된 키
    private static readonly HashSet<KeyCode> ForbiddenKeys = new HashSet<KeyCode>
    {
        KeyCode.Escape,
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
        else if (isTestMode)
        {
            HandleTestMode();
        }
    }

    void SetupUI()
    {
        // 키 모드 드롭다운
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

        // 버튼 이벤트
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        if (resetAllButton != null) resetAllButton.onClick.AddListener(OnResetAllClicked);

        // 테스트 모드
        if (testModeButton != null) testModeButton.onClick.AddListener(ToggleTestMode);

        // 프리셋
        if (savePresetButton != null) savePresetButton.onClick.AddListener(OnSavePreset);
        if (loadPresetButton != null) loadPresetButton.onClick.AddListener(OnLoadPreset);
        if (deletePresetButton != null) deletePresetButton.onClick.AddListener(OnDeletePreset);

        // 내보내기/가져오기
        if (exportButton != null) exportButton.onClick.AddListener(OnExportKeys);
        if (importButton != null) importButton.onClick.AddListener(OnImportKeys);
        if (copyToClipboardButton != null) copyToClipboardButton.onClick.AddListener(OnCopyToClipboard);

    }

    #region Basic Functions

    void OnKeyModeChanged(int index)
    {
        List<int> keyModes = new List<int>(KeyModeNames.Keys);
        keyModes.Sort();
        currentKeyMode = keyModes[index];
        LoadCurrentKeyMode();
        LoadPresetDropdown();
    }

    void LoadCurrentKeyMode()
    {
        KeyCode[] customKeys = null;
        if (SettingsManager.Instance != null)
        {
            customKeys = SettingsManager.Instance.GetKeyBindings(currentKeyMode);
        }

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
            case 4: return new List<KeyCode> { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
            case 5: return new List<KeyCode> { KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K };
            case -5: return new List<KeyCode> { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
            case 6: return new List<KeyCode> { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
            case 7: return new List<KeyCode> { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K, KeyCode.L };
            case 8: return new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
            case 10: return new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
            default: return new List<KeyCode> { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
        }
    }

    void RefreshUI()
    {
        foreach (Button btn in keyButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        keyButtons.Clear();

        if (keyButtonContainer == null || keyButtonPrefab == null) return;

        for (int i = 0; i < currentKeys.Count; i++)
        {
            GameObject btnObj = Instantiate(keyButtonPrefab, keyButtonContainer);
            Button btn = btnObj.GetComponent<Button>();
            Text btnText = btnObj.GetComponentInChildren<Text>();

            if (btn != null)
            {
                int index = i;
                btn.onClick.AddListener(() => OnKeyButtonClicked(index));
                keyButtons.Add(btn);

                if (btnText != null)
                {
                    if (currentKeyMode == -5 && index == 2)
                    {
                        btnText.text = $"트랙 {index + 1}: {currentKeys[index]} / {currentKeys[index + 1]}";
                    }
                    else if (currentKeyMode == -5 && index == 3)
                    {
                        continue;
                    }
                    else
                    {
                        btnText.text = $"트랙 {GetTrackNumber(index)}: {currentKeys[index]}";
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

        ColorBlock colors = waitingButton.colors;
        colors.normalColor = Color.yellow;
        waitingButton.colors = colors;

        ShowStatus($"트랙 {GetTrackNumber(index)}에 할당할 키를 누르세요 (ESC로 취소)");
    }

    void HandleKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelKeyWaiting();
            return;
        }

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (ForbiddenKeys.Contains(key))
                {
                    ShowStatus($"{key}는 사용할 수 없는 키입니다.");
                    continue;
                }

                if (currentKeys.Contains(key) && currentKeys[waitingKeyIndex] != key)
                {
                    ShowStatus($"{key}는 이미 사용 중인 키입니다.");
                    continue;
                }

                currentKeys[waitingKeyIndex] = key;

                if (currentKeyMode == -5 && waitingKeyIndex == 2)
                {
                    ShowStatus($"중앙 트랙의 첫 번째 키: {key}. 두 번째 키를 눌러주세요.");
                    waitingKeyIndex = 3;
                    return;
                }

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

    #endregion

    #region Test Mode

    void ToggleTestMode()
    {
        isTestMode = !isTestMode;

        if (testModePanel != null)
        {
            testModePanel.SetActive(isTestMode);
        }

        if (isTestMode)
        {
            SetupTestTracks();
            ShowStatus("테스트 모드 활성화. 키를 눌러보세요!");
        }
        else
        {
            CleanupTestTracks();
            ShowStatus("테스트 모드 비활성화");
        }
    }

    void SetupTestTracks()
    {
        CleanupTestTracks();

        if (testTrackContainer == null || testTrackPrefab == null) return;

        int trackCount = System.Math.Abs(currentKeyMode);
        for (int i = 0; i < trackCount; i++)
        {
            GameObject trackObj = Instantiate(testTrackPrefab, testTrackContainer);
            Image trackImage = trackObj.GetComponent<Image>();
            if (trackImage != null)
            {
                testTracks.Add(trackImage);
                trackImage.color = Color.gray;
            }
        }
    }

    void CleanupTestTracks()
    {
        foreach (var track in testTracks)
        {
            if (track != null) Destroy(track.gameObject);
        }
        testTracks.Clear();
    }

    void HandleTestMode()
    {
        for (int i = 0; i < currentKeys.Count; i++)
        {
            KeyCode key = currentKeys[i];
            int trackIndex = GetTestTrackIndex(i);

            if (trackIndex >= 0 && trackIndex < testTracks.Count)
            {
                if (Input.GetKeyDown(key))
                {
                    testTracks[trackIndex].color = Color.green;
                }
                else if (Input.GetKeyUp(key))
                {
                    testTracks[trackIndex].color = Color.gray;
                }
            }
        }
    }

    int GetTestTrackIndex(int keyIndex)
    {
        if (currentKeyMode == -5)
        {
            if (keyIndex <= 1) return keyIndex;
            if (keyIndex <= 3) return 2;
            return keyIndex - 1;
        }
        return keyIndex;
    }

    #endregion

    #region Presets

    void LoadPresetDropdown()
    {
        if (presetDropdown == null) return;

        presetDropdown.ClearOptions();

        if (SettingsManager.Instance != null)
        {
            var presets = SettingsManager.Instance.Settings.GetPresetsForMode(currentKeyMode);
            List<string> presetNames = new List<string> { "프리셋 선택..." };

            foreach (var preset in presets)
            {
                presetNames.Add(preset.presetName);
            }

            presetDropdown.AddOptions(presetNames);
        }
    }

    void OnSavePreset()
    {
        if (presetNameInput == null || string.IsNullOrEmpty(presetNameInput.text))
        {
            ShowStatus("프리셋 이름을 입력하세요.");
            return;
        }

        string presetName = presetNameInput.text;

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.Settings.SavePreset(presetName, currentKeyMode, currentKeys.ToArray());
            SettingsManager.Instance.SaveSettings();
            LoadPresetDropdown();
            ShowStatus($"프리셋 '{presetName}' 저장 완료");
        }
    }

    void OnLoadPreset()
    {
        if (presetDropdown == null || presetDropdown.value == 0)
        {
            ShowStatus("프리셋을 선택하세요.");
            return;
        }

        string presetName = presetDropdown.options[presetDropdown.value].text;

        if (SettingsManager.Instance != null)
        {
            KeyCode[] keys = SettingsManager.Instance.Settings.LoadPreset(presetName, currentKeyMode);
            if (keys != null && keys.Length > 0)
            {
                currentKeys = new List<KeyCode>(keys);
                SaveKeyBindings();
                RefreshUI();
                ShowStatus($"프리셋 '{presetName}' 로드 완료");
            }
            else
            {
                ShowStatus("프리셋 로드 실패");
            }
        }
    }

    void OnDeletePreset()
    {
        if (presetDropdown == null || presetDropdown.value == 0)
        {
            ShowStatus("삭제할 프리셋을 선택하세요.");
            return;
        }

        string presetName = presetDropdown.options[presetDropdown.value].text;

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.Settings.DeletePreset(presetName, currentKeyMode);
            SettingsManager.Instance.SaveSettings();
            LoadPresetDropdown();
            ShowStatus($"프리셋 '{presetName}' 삭제 완료");
        }
    }

    #endregion

    #region Import/Export

    void OnExportKeys()
    {
        string json = ExportKeysToJSON();
        string filePath = Application.persistentDataPath + $"/keybindings_{currentKeyMode}K.json";

        try
        {
            File.WriteAllText(filePath, json);
            ShowStatus($"내보내기 완료: {filePath}");
        }
        catch (System.Exception e)
        {
            ShowStatus($"내보내기 실패: {e.Message}");
        }
    }

    void OnImportKeys()
    {
        if (importTextField == null || string.IsNullOrEmpty(importTextField.text))
        {
            ShowStatus("JSON 데이터를 입력하세요.");
            return;
        }

        try
        {
            ImportKeysFromJSON(importTextField.text);
            ShowStatus("가져오기 완료");
        }
        catch (System.Exception e)
        {
            ShowStatus($"가져오기 실패: {e.Message}");
        }
    }

    void OnCopyToClipboard()
    {
        string json = ExportKeysToJSON();
        GUIUtility.systemCopyBuffer = json;
        ShowStatus("클립보드에 복사되었습니다.");
    }

    string ExportKeysToJSON()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append($"\"mode\":{currentKeyMode},");
        sb.Append("\"keys\":[");

        for (int i = 0; i < currentKeys.Count; i++)
        {
            sb.Append((int)currentKeys[i]);
            if (i < currentKeys.Count - 1) sb.Append(",");
        }

        sb.Append("]}");
        return sb.ToString();
    }

    void ImportKeysFromJSON(string json)
    {
        // 간단한 JSON 파싱
        json = json.Trim();
        if (!json.StartsWith("{") || !json.EndsWith("}"))
        {
            throw new System.Exception("잘못된 JSON 형식");
        }

        // mode 추출
        int modeStart = json.IndexOf("\"mode\":") + 7;
        int modeEnd = json.IndexOf(",", modeStart);
        int mode = int.Parse(json.Substring(modeStart, modeEnd - modeStart));

        if (mode != currentKeyMode)
        {
            ShowStatus($"경고: 현재 모드({currentKeyMode}K)와 파일 모드({mode}K)가 다릅니다.");
        }

        // keys 배열 추출
        int keysStart = json.IndexOf("[") + 1;
        int keysEnd = json.IndexOf("]");
        string keysStr = json.Substring(keysStart, keysEnd - keysStart);
        string[] keyStrs = keysStr.Split(',');

        currentKeys.Clear();
        foreach (string keyStr in keyStrs)
        {
            int keyCode = int.Parse(keyStr.Trim());
            currentKeys.Add((KeyCode)keyCode);
        }

        SaveKeyBindings();
        RefreshUI();
    }

    #endregion

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
        if (keyModeDropdown != null) keyModeDropdown.onValueChanged.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (resetAllButton != null) resetAllButton.onClick.RemoveAllListeners();
        if (testModeButton != null) testModeButton.onClick.RemoveAllListeners();
        if (savePresetButton != null) savePresetButton.onClick.RemoveAllListeners();
        if (loadPresetButton != null) loadPresetButton.onClick.RemoveAllListeners();
        if (deletePresetButton != null) deletePresetButton.onClick.RemoveAllListeners();
        if (exportButton != null) exportButton.onClick.RemoveAllListeners();
        if (importButton != null) importButton.onClick.RemoveAllListeners();
        if (copyToClipboardButton != null) copyToClipboardButton.onClick.RemoveAllListeners();
    }
}
