using UnityEngine;
using UnityEngine.UI;

public class TimelineDisplay : MonoBehaviour
{
    [Header("Timeline Settings")]
    public Text timeDisplayText;
    public Text bpmDisplayText;
    public Text measureDisplayText;
    public Slider progressSlider;
    
    [Header("Visual Settings")]
    public RectTransform timelineContainer;
    public GameObject timeMarkerPrefab;
    public float timelineWidth = 800f;
    public int maxTimeMarkers = 20;
    
    private ChartEditor chartEditor;
    private AudioSource audioSource;
    private System.Collections.Generic.List<GameObject> timeMarkers = new System.Collections.Generic.List<GameObject>();
    
    void Start()
    {
        chartEditor = FindObjectOfType<ChartEditor>();
        audioSource = FindObjectOfType<AudioSource>();
        SetupTimeline();
    }
    
    void Update()
    {
        UpdateTimelineDisplay();
    }
    
    void SetupTimeline()
    {
        if (timelineContainer == null || timeMarkerPrefab == null) return;
        
        // 기존 타임 마커 정리
        ClearTimeMarkers();
        
        // 새 타임 마커 생성
        for (int i = 0; i < maxTimeMarkers; i++)
        {
            GameObject marker = Instantiate(timeMarkerPrefab, timelineContainer);
            timeMarkers.Add(marker);
        }
    }
    
    void UpdateTimelineDisplay()
    {
        if (chartEditor == null) return;
        
        double currentTime = chartEditor.GetCurrentTime();
        float totalTime = audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
        
        // 시간 표시 업데이트
        if (timeDisplayText != null)
        {
            timeDisplayText.text = FormatTime((float)currentTime) + " / " + FormatTime(totalTime);
        }
        
        // BPM 표시 업데이트  
        if (bpmDisplayText != null && chartEditor.GetCurrentChart() != null)
        {
            bpmDisplayText.text = $"BPM: {chartEditor.GetCurrentChart().bpm:F1}";
        }
        
        // 마디 표시 업데이트
        if (measureDisplayText != null && chartEditor.GetCurrentChart() != null)
        {
            float bpm = chartEditor.GetCurrentChart().bpm;
            if (bpm > 0)
            {
                double beatLength = 60.0 / bpm;
                double measureLength = beatLength * 4; // 4/4 박자 기준
                int currentMeasure = (int)(currentTime / measureLength) + 1;
                double beatInMeasure = (currentTime % measureLength) / beatLength + 1;
                measureDisplayText.text = $"Measure: {currentMeasure}, Beat: {beatInMeasure:F2}";
            }
        }
        
        // 진행 슬라이더 업데이트
        if (progressSlider != null && totalTime > 0)
        {
            progressSlider.value = (float)(currentTime / totalTime);
        }
        
        // 타임라인 마커 업데이트
        UpdateTimeMarkers(currentTime, totalTime);
    }
    
    void UpdateTimeMarkers(double currentTime, float totalTime)
    {
        if (timeMarkers.Count == 0 || totalTime <= 0) return;
        
        double displayRange = 30.0; // 30초 범위 표시
        double startTime = currentTime - displayRange / 2;
        double endTime = currentTime + displayRange / 2;
        
        for (int i = 0; i < timeMarkers.Count && i < maxTimeMarkers; i++)
        {
            double markerTime = startTime + (endTime - startTime) * i / (maxTimeMarkers - 1);
            
            if (markerTime >= 0 && markerTime <= totalTime)
            {
                timeMarkers[i].SetActive(true);
                
                // 마커 위치 설정
                float normalizedPos = (float)((markerTime - startTime) / (endTime - startTime));
                RectTransform markerRect = timeMarkers[i].GetComponent<RectTransform>();
                if (markerRect != null)
                {
                    markerRect.anchoredPosition = new Vector2(
                        (normalizedPos - 0.5f) * timelineWidth, 
                        markerRect.anchoredPosition.y
                    );
                }
                
                // 마커 텍스트 업데이트
                Text markerText = timeMarkers[i].GetComponentInChildren<Text>();
                if (markerText != null)
                {
                    markerText.text = FormatTime((float)markerTime);
                    
                    // 현재 시간 마커는 다른 색상으로 표시
                    if (System.Math.Abs(markerTime - currentTime) < 0.1)
                    {
                        markerText.color = Color.red;
                    }
                    else
                    {
                        markerText.color = Color.white;
                    }
                }
            }
            else
            {
                timeMarkers[i].SetActive(false);
            }
        }
    }
    
    void ClearTimeMarkers()
    {
        foreach (var marker in timeMarkers)
        {
            if (marker != null) DestroyImmediate(marker);
        }
        timeMarkers.Clear();
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds % 1f) * 1000f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
    
    // 슬라이더 클릭 시 시간 이동
    public void OnTimelineSliderChanged(float value)
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.time = value * audioSource.clip.length;
        }
    }
}