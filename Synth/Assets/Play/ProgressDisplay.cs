using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 게임 진행도 표시 UI
/// - 현재 시간 / 전체 시간
/// - 진행률 바
/// - BPM 표시
/// </summary>
public class ProgressDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioManager audioManager;
    
    [Header("UI Position")]
    [SerializeField] private Vector3 barPosition = new Vector3(0, 4f, -0.1f);
    [SerializeField] private float barWidth = 12f;
    [SerializeField] private float barHeight = 0.3f;
    
    [Header("Display Settings")]
    [SerializeField] private bool showTimeText = true;
    [SerializeField] private bool showBPM = true;
    [SerializeField] private bool showPercentage = true;
    
    [Header("Colors")]
    [SerializeField] private Color barBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color barFillColor = new Color(0.2f, 0.8f, 1f, 0.9f);
    [SerializeField] private Color barBorderColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    // UI Elements
    private GameObject barContainer;
    private GameObject barBackground;
    private GameObject barFill;
    private GameObject barBorder;
    private TextMeshPro timeText;
    private TextMeshPro bpmText;
    private TextMeshPro percentageText;
    
    // Runtime Data
    private float totalSongLength = 0f;
    private float currentBPM = 120f;
    
    void Start()
    {
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        
        CreateProgressBar();
        CreateTextDisplays();
    }
    
    void Update()
    {
        UpdateProgressBar();
        UpdateTextDisplays();
    }
    
    void CreateProgressBar()
    {
        // Container
        barContainer = new GameObject("ProgressBarContainer");
        barContainer.transform.SetParent(transform);
        barContainer.transform.localPosition = barPosition;
        
        // Background
        barBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        barBackground.name = "ProgressBackground";
        barBackground.transform.SetParent(barContainer.transform);
        barBackground.transform.localPosition = Vector3.zero;
        barBackground.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = barBackgroundColor;
        barBackground.GetComponent<Renderer>().material = bgMat;
        
        // Fill
        barFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        barFill.name = "ProgressFill";
        barFill.transform.SetParent(barContainer.transform);
        barFill.transform.localPosition = new Vector3(0, 0, -0.01f);
        barFill.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        
        Material fillMat = new Material(Shader.Find("Sprites/Default"));
        fillMat.color = barFillColor;
        barFill.GetComponent<Renderer>().material = fillMat;
        
        // Border
        CreateBorder();
    }
    
    void CreateBorder()
    {
        barBorder = new GameObject("ProgressBorder");
        barBorder.transform.SetParent(barContainer.transform);
        barBorder.transform.localPosition = new Vector3(0, 0, -0.02f);
        
        // 테두리를 라인으로 생성
        CreateBorderLine(new Vector3(-barWidth/2, barHeight/2, 0), new Vector3(barWidth/2, barHeight/2, 0)); // Top
        CreateBorderLine(new Vector3(-barWidth/2, -barHeight/2, 0), new Vector3(barWidth/2, -barHeight/2, 0)); // Bottom
        CreateBorderLine(new Vector3(-barWidth/2, -barHeight/2, 0), new Vector3(-barWidth/2, barHeight/2, 0)); // Left
        CreateBorderLine(new Vector3(barWidth/2, -barHeight/2, 0), new Vector3(barWidth/2, barHeight/2, 0)); // Right
    }
    
    void CreateBorderLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("BorderLine");
        line.transform.SetParent(barBorder.transform);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = barBorderColor;
        lr.endColor = barBorderColor;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.sortingOrder = 10;
    }
    
    void CreateTextDisplays()
    {
        // Time Text (Left)
        if (showTimeText)
        {
            GameObject timeObj = new GameObject("TimeText");
            timeObj.transform.SetParent(barContainer.transform);
            timeObj.transform.localPosition = new Vector3(-barWidth/2 - 1f, 0, -0.02f);
            
            timeText = timeObj.AddComponent<TextMeshPro>();
            timeText.text = "0:00 / 0:00";
            timeText.fontSize = 3f;
            timeText.alignment = TextAlignmentOptions.MidlineRight;
            timeText.color = Color.white;
        }
        
        // BPM Text (Right)
        if (showBPM)
        {
            GameObject bpmObj = new GameObject("BPMText");
            bpmObj.transform.SetParent(barContainer.transform);
            bpmObj.transform.localPosition = new Vector3(barWidth/2 + 1f, 0, -0.02f);
            
            bpmText = bpmObj.AddComponent<TextMeshPro>();
            bpmText.text = "BPM: 120";
            bpmText.fontSize = 3f;
            bpmText.alignment = TextAlignmentOptions.MidlineLeft;
            bpmText.color = Color.white;
        }
        
        // Percentage Text (Center)
        if (showPercentage)
        {
            GameObject percentObj = new GameObject("PercentageText");
            percentObj.transform.SetParent(barContainer.transform);
            percentObj.transform.localPosition = new Vector3(0, 0, -0.03f);
            
            percentageText = percentObj.AddComponent<TextMeshPro>();
            percentageText.text = "0%";
            percentageText.fontSize = 2.5f;
            percentageText.alignment = TextAlignmentOptions.Center;
            percentageText.color = new Color(1f, 1f, 1f, 0.9f);
            percentageText.fontStyle = FontStyles.Bold;
        }
    }
    
    void UpdateProgressBar()
    {
        if (audioManager == null || !audioManager.IsPlaying)
            return;
        
        float currentTime = audioManager.GetMusicTime();
        float progress = totalSongLength > 0 ? Mathf.Clamp01(currentTime / totalSongLength) : 0f;
        
        // Update fill bar scale and position
        Vector3 fillScale = barFill.transform.localScale;
        fillScale.x = barWidth * progress;
        barFill.transform.localScale = fillScale;
        
        // Anchor to left
        float xOffset = -(barWidth - fillScale.x) / 2f;
        barFill.transform.localPosition = new Vector3(xOffset, 0, -0.01f);
    }
    
    void UpdateTextDisplays()
    {
        if (audioManager == null || !audioManager.IsPlaying)
            return;
        
        float currentTime = audioManager.GetMusicTime();
        float progress = totalSongLength > 0 ? Mathf.Clamp01(currentTime / totalSongLength) : 0f;
        
        // Time Text
        if (showTimeText && timeText != null)
        {
            string currentTimeStr = FormatTime(currentTime);
            string totalTimeStr = FormatTime(totalSongLength);
            timeText.text = $"{currentTimeStr} / {totalTimeStr}";
        }
        
        // BPM Text
        if (showBPM && bpmText != null)
        {
            bpmText.text = $"BPM: {currentBPM:F0}";
        }
        
        // Percentage Text
        if (showPercentage && percentageText != null)
        {
            percentageText.text = $"{progress * 100f:F1}%";
        }
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes}:{seconds:D2}";
    }
    
    /// <summary>
    /// 총 곡 길이 설정
    /// </summary>
    public void SetSongLength(float length)
    {
        totalSongLength = length;
    }
    
    /// <summary>
    /// BPM 설정
    /// </summary>
    public void SetBPM(float bpm)
    {
        currentBPM = bpm;
    }
    
    /// <summary>
    /// 진행도 바 색상 변경
    /// </summary>
    public void SetBarColor(Color color)
    {
        if (barFill != null)
        {
            Material fillMat = barFill.GetComponent<Renderer>().material;
            fillMat.color = color;
        }
    }
}
