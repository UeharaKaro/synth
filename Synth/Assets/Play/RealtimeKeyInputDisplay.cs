using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 실시간 키 입력 표시
/// 게임 플레이 중 현재 누르고 있는 키를 화면에 표시
/// </summary>
public class RealtimeKeyInputDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Transform keyDisplayContainer;
    public GameObject keyDisplayPrefab;
    public bool showDisplay = true;

    [Header("Settings")]
    public Vector3 displayPosition = new Vector3(0, -300, 0);
    public float keyWidth = 60f;
    public float keyHeight = 60f;
    public float spacing = 10f;

    private Dictionary<int, GameObject> trackDisplays = new Dictionary<int, GameObject>();
    private Dictionary<int, KeyCode> trackKeys = new Dictionary<int, KeyCode>();
    private int currentKeyMode = 4;

    private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    private Color pressedColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    void Start()
    {
        LoadKeyBindings();
        CreateKeyDisplays();
    }

    void LoadKeyBindings()
    {
        // GearSettings에서 현재 키 모드 가져오기
        GearController gearController = FindObjectOfType<GearController>();
        if (gearController != null && gearController.settings != null)
        {
            currentKeyMode = gearController.settings.lineCount;
        }

        // SettingsManager에서 키 바인딩 가져오기
        if (SettingsManager.Instance != null)
        {
            KeyCode[] keys = SettingsManager.Instance.GetKeyBindings(currentKeyMode);
            if (keys != null && keys.Length > 0)
            {
                int lineCount = System.Math.Abs(currentKeyMode);
                for (int i = 0; i < keys.Length && i < lineCount; i++)
                {
                    trackKeys[i] = keys[i];
                }
            }
            else
            {
                // 기본 키 바인딩
                LoadDefaultKeyBindings();
            }
        }
        else
        {
            LoadDefaultKeyBindings();
        }
    }

    void LoadDefaultKeyBindings()
    {
        switch (currentKeyMode)
        {
            case 4:
                trackKeys[0] = KeyCode.D;
                trackKeys[1] = KeyCode.F;
                trackKeys[2] = KeyCode.J;
                trackKeys[3] = KeyCode.K;
                break;
            case 5:
                trackKeys[0] = KeyCode.D;
                trackKeys[1] = KeyCode.F;
                trackKeys[2] = KeyCode.Space;
                trackKeys[3] = KeyCode.J;
                trackKeys[4] = KeyCode.K;
                break;
            case -5:
                trackKeys[0] = KeyCode.S;
                trackKeys[1] = KeyCode.D;
                trackKeys[2] = KeyCode.F;
                trackKeys[3] = KeyCode.J;
                trackKeys[4] = KeyCode.K;
                trackKeys[5] = KeyCode.L;
                break;
            case 6:
                trackKeys[0] = KeyCode.S;
                trackKeys[1] = KeyCode.D;
                trackKeys[2] = KeyCode.F;
                trackKeys[3] = KeyCode.J;
                trackKeys[4] = KeyCode.K;
                trackKeys[5] = KeyCode.L;
                break;
            case 7:
                trackKeys[0] = KeyCode.S;
                trackKeys[1] = KeyCode.D;
                trackKeys[2] = KeyCode.F;
                trackKeys[3] = KeyCode.Space;
                trackKeys[4] = KeyCode.J;
                trackKeys[5] = KeyCode.K;
                trackKeys[6] = KeyCode.L;
                break;
            case 8:
                trackKeys[0] = KeyCode.A;
                trackKeys[1] = KeyCode.S;
                trackKeys[2] = KeyCode.D;
                trackKeys[3] = KeyCode.F;
                trackKeys[4] = KeyCode.J;
                trackKeys[5] = KeyCode.K;
                trackKeys[6] = KeyCode.L;
                trackKeys[7] = KeyCode.Semicolon;
                break;
            case 10:
                trackKeys[0] = KeyCode.A;
                trackKeys[1] = KeyCode.S;
                trackKeys[2] = KeyCode.D;
                trackKeys[3] = KeyCode.F;
                trackKeys[4] = KeyCode.G;
                trackKeys[5] = KeyCode.H;
                trackKeys[6] = KeyCode.J;
                trackKeys[7] = KeyCode.K;
                trackKeys[8] = KeyCode.L;
                trackKeys[9] = KeyCode.Semicolon;
                break;
        }
    }

    void CreateKeyDisplays()
    {
        if (keyDisplayContainer == null || keyDisplayPrefab == null)
            return;

        int lineCount = System.Math.Abs(currentKeyMode);
        float totalWidth = lineCount * keyWidth + (lineCount - 1) * spacing;
        float startX = -totalWidth / 2f + keyWidth / 2f;

        for (int i = 0; i < lineCount; i++)
        {
            GameObject displayObj = Instantiate(keyDisplayPrefab, keyDisplayContainer);
            RectTransform rectTransform = displayObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                float xPos = startX + i * (keyWidth + spacing);
                rectTransform.anchoredPosition = new Vector2(xPos, displayPosition.y);
                rectTransform.sizeDelta = new Vector2(keyWidth, keyHeight);
            }

            // 키 텍스트 표시
            Text keyText = displayObj.GetComponentInChildren<Text>();
            if (keyText != null && trackKeys.ContainsKey(i))
            {
                string keyName = GetKeyDisplayName(trackKeys[i]);
                keyText.text = keyName;
            }

            // 초기 색상 설정
            Image keyImage = displayObj.GetComponent<Image>();
            if (keyImage != null)
            {
                keyImage.color = normalColor;
            }

            trackDisplays[i] = displayObj;
        }

        // 초기 표시 여부 설정
        keyDisplayContainer.gameObject.SetActive(showDisplay);
    }

    void Update()
    {
        if (!showDisplay || trackDisplays.Count == 0)
            return;

        // 5+1K 모드 특수 처리
        if (currentKeyMode == -5)
        {
            Update5Plus1KMode();
        }
        else
        {
            UpdateNormalMode();
        }
    }

    void UpdateNormalMode()
    {
        foreach (var kvp in trackKeys)
        {
            int track = kvp.Key;
            KeyCode key = kvp.Value;

            if (trackDisplays.ContainsKey(track))
            {
                Image keyImage = trackDisplays[track].GetComponent<Image>();
                if (keyImage != null)
                {
                    if (Input.GetKey(key))
                    {
                        keyImage.color = pressedColor;
                    }
                    else
                    {
                        keyImage.color = normalColor;
                    }
                }
            }
        }
    }

    void Update5Plus1KMode()
    {
        // 5+1K 모드: 트랙 2 (index 2)가 F와 J 두 키를 받음
        for (int i = 0; i < 5; i++)
        {
            if (trackDisplays.ContainsKey(i))
            {
                Image keyImage = trackDisplays[i].GetComponent<Image>();
                if (keyImage != null)
                {
                    bool isPressed = false;

                    if (i == 2)
                    {
                        // 트랙 3 (index 2): F 또는 J 키
                        isPressed = Input.GetKey(trackKeys[2]) || Input.GetKey(trackKeys[3]);
                    }
                    else if (i > 2)
                    {
                        // index 3 이후는 trackKeys의 index + 1에 해당
                        isPressed = Input.GetKey(trackKeys[i + 1]);
                    }
                    else
                    {
                        // index 0, 1
                        isPressed = Input.GetKey(trackKeys[i]);
                    }

                    keyImage.color = isPressed ? pressedColor : normalColor;
                }
            }
        }
    }

    string GetKeyDisplayName(KeyCode key)
    {
        // 키 이름을 보기 좋게 변환
        switch (key)
        {
            case KeyCode.Space: return "Space";
            case KeyCode.LeftShift:
            case KeyCode.RightShift: return "Shift";
            case KeyCode.Return: return "Enter";
            case KeyCode.Tab: return "Tab";
            case KeyCode.Semicolon: return ";";
            case KeyCode.Quote: return "'";
            case KeyCode.Comma: return ",";
            case KeyCode.Period: return ".";
            case KeyCode.Slash: return "/";
            case KeyCode.Backslash: return "\\";
            case KeyCode.LeftBracket: return "[";
            case KeyCode.RightBracket: return "]";
            case KeyCode.Minus: return "-";
            case KeyCode.Equals: return "=";
            case KeyCode.BackQuote: return "`";
            default: return key.ToString();
        }
    }

    /// <summary>
    /// 실시간 표시 ON/OFF
    /// </summary>
    public void ToggleDisplay()
    {
        showDisplay = !showDisplay;
        if (keyDisplayContainer != null)
        {
            keyDisplayContainer.gameObject.SetActive(showDisplay);
        }
    }

    /// <summary>
    /// 외부에서 표시 여부 설정
    /// </summary>
    public void SetDisplayActive(bool active)
    {
        showDisplay = active;
        if (keyDisplayContainer != null)
        {
            keyDisplayContainer.gameObject.SetActive(active);
        }
    }
}
