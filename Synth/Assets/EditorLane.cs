using UnityEngine;

public class EditorLane : MonoBehaviour
{
    [Header("Lane Settings")]
    public int laneIndex = 0;
    public SpriteRenderer laneRenderer;
    public Color evenLaneColor = new Color(1f, 1f, 1f, 0.1f);
    public Color oddLaneColor = new Color(0.5f, 0.5f, 1f, 0.1f);
    public Color judgmentLineColor = Color.white;
    
    [Header("Judgment Line")]
    public GameObject judgmentLine;
    public float judgmentLineY = -8f;
    
    void Start()
    {
        UpdateLaneVisuals();
        SetupJudgmentLine();
    }
    
    public void SetLaneIndex(int index)
    {
        laneIndex = index;
        UpdateLaneVisuals();
    }
    
    void UpdateLaneVisuals()
    {
        if (laneRenderer == null) return;
        
        // 짝수/홀수 레인에 따른 색상 설정
        laneRenderer.color = (laneIndex % 2 == 0) ? evenLaneColor : oddLaneColor;
        
        // 레인 크기 설정
        laneRenderer.size = new Vector2(1f, 20f); // 세로 긴 사각형
    }
    
    void SetupJudgmentLine()
    {
        if (judgmentLine == null)
        {
            // 판정선 생성
            judgmentLine = new GameObject("JudgmentLine");
            judgmentLine.transform.SetParent(transform);
            
            SpriteRenderer lineRenderer = judgmentLine.AddComponent<SpriteRenderer>();
            lineRenderer.color = judgmentLineColor;
            lineRenderer.size = new Vector2(1.2f, 0.1f);
        }
        
        // 판정선 위치 설정
        judgmentLine.transform.localPosition = new Vector3(0, judgmentLineY, -0.1f);
    }
    
    void OnMouseDown()
    {
        // 레인 클릭 시 해당 트랙으로 포커스 (추후 구현)
        Debug.Log($"Lane {laneIndex} clicked");
    }
}