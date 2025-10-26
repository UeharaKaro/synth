using UnityEngine;

[CreateAssetMenu(fileName = "GearSettings", menuName = "RhythmGame/GearSettings")]
public class GearSettings : ScriptableObject
{
    [Header("Line Configuration")]
    [Range(4, 10)]
    public int lineCount = 4;
    
    [Header("Gear Dimensions")]
    [Range(0.1f, 2f)]
    public float leftRightRatio = 1f; // 좌우 비율
    [Range(1f, 10f)]
    public float gearLength = 5f; // 기어 길이
    [Range(0.5f, 2f)]
    public float lineWidth = 1f; // 라인 간 너비
    [Range(0.1f, 1f)]
    public float noteSize = 0.5f; // 노트 크기
    [Range(0f, 1f)]
    public float lineSpacing = 0.1f; // 라인 간격 (deprecated, lineWidth 사용)
    [Range(1f, 20f)]
    public float gearHeight = 8f; // 기어 높이 (deprecated, gearLength 사용)
    
    [Header("Note Settings")]
    [Range(1f, 20f)]
    public float noteSpeed = 5f; // 노트 속도
    
    [Header("Camera Settings")]
    [Range(-90f, 90f)]
    public float cameraAngle = 45f; // 카메라 각도
    [Range(1f, 20f)]
    public float cameraHeight = 10f; // 카메라 높이
    
    [Header("Judgment Line")]
    [Range(0f, 5f)]
    public float judgmentLineHeight = 1f; // 판정선 높이
    [Range(-10f, 0f)]
    public float judgmentLineY = -3f; // 판정선 Y 위치
    
    [Header("HP Bar")]
    [Range(0.5f, 2f)]
    public float hpBarWidth = 0.8f;
    [Range(0.1f, 1f)]
    public float hpBarMargin = 0.3f;
    
    [Header("Visual Settings")]
    public Color gearBackgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
    public Color lineColor = new Color(0.3f, 0.3f, 0.5f, 0.5f);
    public Color judgmentLineColor = new Color(1f, 0.2f, 0.8f, 1f);
    public Color hpBarFullColor = new Color(0.2f, 1f, 0.3f, 1f);
    public Color hpBarEmptyColor = new Color(1f, 0.2f, 0.2f, 1f);
    
    [Header("Judgment Display")]
    [Range(0, 200)]
    public int judgmentDisplayRangeMs = 100; // 표시할 판정 범위 (ms)
    public bool showSPerfect = true;
    public bool showPerfect = true;
    public bool showGreat = true;
    public bool showGood = true;
    public bool showBad = true;
    public bool showMiss = true;
    
    // 라인 개수에 따른 기본 너비 계산
    public float GetBaseWidth()
    {
        switch (lineCount)
        {
            case 4: return 4f;
            case 5: return 5f;
            case 6: return 6f;
            case 7: return 7f;
            case 8: return 8f;
            case 10: return 10f;
            default: return 4f;
        }
    }
    
    public float GetTotalWidth()
    {
        return GetBaseWidth() * lineWidth * leftRightRatio;
    }
}