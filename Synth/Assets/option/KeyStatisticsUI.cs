using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 키 통계 및 히트맵 UI
/// 각 키별 정확도, 사용 빈도를 시각적으로 표시합니다.
/// </summary>
public class KeyStatisticsUI : MonoBehaviour
{
    [Header("UI References")]
    public Text totalHitsText;
    public Text overallAccuracyText;
    public Transform statsListContainer;
    public GameObject statsItemPrefab;

    [Header("Heatmap")]
    public Transform heatmapContainer;
    public GameObject heatmapKeyPrefab;

    [Header("Top Lists")]
    public Transform topUsedContainer;
    public Transform topAccurateContainer;
    public Transform worstAccurateContainer;
    public GameObject topItemPrefab;

    [Header("Buttons")]
    public Button refreshButton;
    public Button resetButton;
    public Button exportButton;

    private KeyStatistics statistics;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        LoadStatistics();
        SetupButtons();
        RefreshUI();
    }

    void SetupButtons()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshUI);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetStatistics);

        if (exportButton != null)
            exportButton.onClick.AddListener(ExportStatistics);
    }

    void LoadStatistics()
    {
        string json = PlayerPrefs.GetString("KeyStatistics", "");
        if (!string.IsNullOrEmpty(json))
        {
            statistics = KeyStatistics.FromJson(json);
        }
        else
        {
            statistics = new KeyStatistics();
        }
    }

    void SaveStatistics()
    {
        string json = statistics.ToJson();
        PlayerPrefs.SetString("KeyStatistics", json);
        PlayerPrefs.Save();
    }

    public void RefreshUI()
    {
        LoadStatistics();
        ClearSpawnedObjects();
        UpdateOverallStats();
        UpdateStatsList();
        UpdateHeatmap();
        UpdateTopLists();
    }

    void ClearSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    void UpdateOverallStats()
    {
        int totalHits = 0;
        int totalPerfect = 0;
        int totalGreat = 0;

        foreach (var stat in statistics.keyStats)
        {
            totalHits += stat.totalHits;
            totalPerfect += stat.perfectHits;
            totalGreat += stat.greatHits;
        }

        if (totalHitsText != null)
        {
            totalHitsText.text = $"총 입력 횟수: {totalHits:N0}";
        }

        if (overallAccuracyText != null)
        {
            float accuracy = totalHits > 0 ? (float)(totalPerfect + totalGreat) / totalHits * 100f : 0f;
            overallAccuracyText.text = $"전체 정확도: {accuracy:F2}%";
        }
    }

    void UpdateStatsList()
    {
        if (statsListContainer == null || statsItemPrefab == null) return;

        foreach (var stat in statistics.keyStats)
        {
            GameObject itemObj = Instantiate(statsItemPrefab, statsListContainer);
            spawnedObjects.Add(itemObj);

            Text[] texts = itemObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 4)
            {
                texts[0].text = stat.key.ToString();
                texts[1].text = $"{stat.totalHits:N0}";
                texts[2].text = $"{stat.GetAccuracy():F1}%";
                texts[3].text = $"{stat.GetAverageOffset():F1}ms";
            }
        }
    }

    void UpdateHeatmap()
    {
        if (heatmapContainer == null || heatmapKeyPrefab == null) return;

        // 최대값 찾기 (정규화용)
        int maxHits = 0;
        foreach (var stat in statistics.keyStats)
        {
            if (stat.totalHits > maxHits) maxHits = stat.totalHits;
        }

        if (maxHits == 0) return;

        // 키보드 레이아웃 (QWERTY)
        string[] rows = new string[]
        {
            "1234567890",
            "QWERTYUIOP",
            "ASDFGHJKL;",
            "ZXCVBNM"
        };

        float yOffset = 0f;
        foreach (string row in rows)
        {
            float xOffset = 0f;
            foreach (char c in row)
            {
                KeyCode key = CharToKeyCode(c);
                var stat = statistics.GetKeyStat(key);

                GameObject keyObj = Instantiate(heatmapKeyPrefab, heatmapContainer);
                RectTransform rt = keyObj.GetComponent<RectTransform>();

                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(xOffset, -yOffset);
                }

                Image img = keyObj.GetComponent<Image>();
                Text txt = keyObj.GetComponentInChildren<Text>();

                if (txt != null)
                {
                    txt.text = c.ToString();
                }

                if (img != null && stat != null)
                {
                    // 히트 수에 따라 색상 강도 조절
                    float intensity = (float)stat.totalHits / maxHits;

                    // 정확도에 따라 색상 선택
                    float accuracy = stat.GetAccuracy();
                    Color color;

                    if (accuracy >= 90f)
                        color = Color.Lerp(Color.white, Color.green, intensity);
                    else if (accuracy >= 70f)
                        color = Color.Lerp(Color.white, Color.yellow, intensity);
                    else
                        color = Color.Lerp(Color.white, Color.red, intensity);

                    img.color = color;
                }

                spawnedObjects.Add(keyObj);
                xOffset += 60f; // 키 간격
            }
            yOffset += 60f; // 행 간격
        }
    }

    void UpdateTopLists()
    {
        // 가장 많이 사용된 키
        if (topUsedContainer != null && topItemPrefab != null)
        {
            var topUsed = statistics.GetTopUsedKeys(5);
            foreach (var stat in topUsed)
            {
                GameObject itemObj = Instantiate(topItemPrefab, topUsedContainer);
                spawnedObjects.Add(itemObj);

                Text txt = itemObj.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = $"{stat.key}: {stat.totalHits:N0}회 ({stat.GetAccuracy():F1}%)";
                }
            }
        }

        // 가장 정확한 키
        if (topAccurateContainer != null && topItemPrefab != null)
        {
            var topAccurate = statistics.GetTopAccurateKeys(5);
            foreach (var stat in topAccurate)
            {
                GameObject itemObj = Instantiate(topItemPrefab, topAccurateContainer);
                spawnedObjects.Add(itemObj);

                Text txt = itemObj.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = $"{stat.key}: {stat.GetAccuracy():F1}% ({stat.totalHits:N0}회)";
                }
            }
        }

        // 가장 부정확한 키
        if (worstAccurateContainer != null && topItemPrefab != null)
        {
            var worstAccurate = statistics.GetWorstAccurateKeys(5);
            foreach (var stat in worstAccurate)
            {
                GameObject itemObj = Instantiate(topItemPrefab, worstAccurateContainer);
                spawnedObjects.Add(itemObj);

                Text txt = itemObj.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = $"{stat.key}: {stat.GetAccuracy():F1}% ({stat.totalHits:N0}회)";
                }
            }
        }
    }

    KeyCode CharToKeyCode(char c)
    {
        switch (c)
        {
            case '1': return KeyCode.Alpha1;
            case '2': return KeyCode.Alpha2;
            case '3': return KeyCode.Alpha3;
            case '4': return KeyCode.Alpha4;
            case '5': return KeyCode.Alpha5;
            case '6': return KeyCode.Alpha6;
            case '7': return KeyCode.Alpha7;
            case '8': return KeyCode.Alpha8;
            case '9': return KeyCode.Alpha9;
            case '0': return KeyCode.Alpha0;
            case ';': return KeyCode.Semicolon;
            default: return (KeyCode)System.Enum.Parse(typeof(KeyCode), c.ToString());
        }
    }

    void ResetStatistics()
    {
        statistics.ResetAllStats();
        SaveStatistics();
        RefreshUI();
        Debug.Log("통계가 초기화되었습니다.");
    }

    void ExportStatistics()
    {
        string json = statistics.ToJson();
        string filePath = Application.persistentDataPath + "/key_statistics.json";
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"통계가 내보내기되었습니다: {filePath}");
    }

    void OnDestroy()
    {
        if (refreshButton != null) refreshButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (exportButton != null) exportButton.onClick.RemoveAllListeners();
    }
}
