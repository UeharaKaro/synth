using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 패턴 레이더 차트 시각화
/// 사운드 볼텍스 이펙터 레이더 스타일
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class PatternRadarChart : Graphic
{
    [Header("레이더 차트 설정")]
    [Tooltip("상위 몇 곡을 레이더 계산에 포함할지")]
    [Range(10, 100)]
    public int topSongsCount = 50;

    [Tooltip("차트 크기 (반지름)")]
    public float chartRadius = 150f;

    [Tooltip("최대 값 (패턴 난이도 최대값)")]
    public float maxValue = 20f;

    [Header("색상 설정")]
    public Color fillColor = new Color(0.2f, 0.6f, 1f, 0.3f); // 반투명 파란색
    public Color outlineColor = new Color(0.2f, 0.6f, 1f, 1f); // 진한 파란색
    public float outlineWidth = 2f;

    [Header("격자선 설정")]
    public Color gridColor = new Color(1f, 1f, 1f, 0.2f);
    public int gridLevels = 4; // 격자선 레벨 (0%, 25%, 50%, 75%, 100%)
    public bool showGridLines = true;

    [Header("라벨 설정")]
    public GameObject labelPrefab; // 텍스트 라벨 프리팹
    public float labelDistance = 180f; // 라벨과 차트 중심 간 거리
    public bool showLabels = true;

    private PatternRadarData currentData;
    private List<GameObject> labels = new List<GameObject>();

    protected override void Start()
    {
        base.Start();
        UpdateRadarChart();
    }

    /// <summary>
    /// 레이더 차트 업데이트
    /// </summary>
    public void UpdateRadarChart()
    {
        // PlayerProfile에서 레이더 데이터 가져오기
        currentData = PlayerProfile.Instance.CalculateRadarData(topSongsCount);

        // 라벨 생성/업데이트
        if (showLabels)
        {
            UpdateLabels();
        }

        // 차트 다시 그리기
        SetVerticesDirty();
    }

    /// <summary>
    /// 수동으로 데이터 설정
    /// </summary>
    public void SetData(PatternRadarData data)
    {
        currentData = data;
        SetVerticesDirty();
    }

    /// <summary>
    /// UI 그리기
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (currentData == null)
        {
            currentData = new PatternRadarData();
        }

        // 격자선 그리기
        if (showGridLines)
        {
            DrawGrid(vh);
        }

        // 데이터 다각형 그리기
        DrawDataPolygon(vh);
    }

    /// <summary>
    /// 격자선 그리기
    /// </summary>
    private void DrawGrid(VertexHelper vh)
    {
        int patternCount = PatternRadarData.GetPatternCount();
        float angleStep = 360f / patternCount;

        // 격자 레벨별로 그리기
        for (int level = 1; level <= gridLevels; level++)
        {
            float ratio = (float)level / gridLevels;
            float radius = chartRadius * ratio;

            // 다각형 격자선
            for (int i = 0; i < patternCount; i++)
            {
                float angle1 = angleStep * i - 90f; // -90도로 위쪽부터 시작
                float angle2 = angleStep * (i + 1) - 90f;

                Vector2 point1 = GetPointOnCircle(angle1, radius);
                Vector2 point2 = GetPointOnCircle(angle2, radius);

                DrawLine(vh, point1, point2, gridColor, 1f);
            }
        }

        // 중심에서 각 꼭지점으로 선 그리기
        for (int i = 0; i < patternCount; i++)
        {
            float angle = angleStep * i - 90f;
            Vector2 point = GetPointOnCircle(angle, chartRadius);

            DrawLine(vh, Vector2.zero, point, gridColor, 1f);
        }
    }

    /// <summary>
    /// 데이터 다각형 그리기
    /// </summary>
    private void DrawDataPolygon(VertexHelper vh)
    {
        float[] values = currentData.ToArray();
        int patternCount = values.Length;
        float angleStep = 360f / patternCount;

        List<Vector2> points = new List<Vector2>();

        // 각 패턴 값에 따라 점 계산
        for (int i = 0; i < patternCount; i++)
        {
            float angle = angleStep * i - 90f; // -90도로 위쪽부터 시작
            float normalizedValue = Mathf.Clamp01(values[i] / maxValue);
            float radius = chartRadius * normalizedValue;

            Vector2 point = GetPointOnCircle(angle, radius);
            points.Add(point);
        }

        // 채우기 (삼각형 팬 방식)
        int centerIndex = vh.currentVertCount;
        vh.AddVert(Vector3.zero, fillColor, Vector2.zero);

        for (int i = 0; i < patternCount; i++)
        {
            vh.AddVert(points[i], fillColor, Vector2.zero);
        }

        // 삼각형 생성
        for (int i = 0; i < patternCount; i++)
        {
            int next = (i + 1) % patternCount;
            vh.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + next + 1);
        }

        // 외곽선 그리기
        for (int i = 0; i < patternCount; i++)
        {
            int next = (i + 1) % patternCount;
            DrawLine(vh, points[i], points[next], outlineColor, outlineWidth);
        }
    }

    /// <summary>
    /// 원 위의 점 계산
    /// </summary>
    private Vector2 GetPointOnCircle(float angleDegrees, float radius)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(
            Mathf.Cos(angleRadians) * radius,
            Mathf.Sin(angleRadians) * radius
        );
    }

    /// <summary>
    /// 선 그리기
    /// </summary>
    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * width * 0.5f;

        int startIndex = vh.currentVertCount;

        vh.AddVert(start + perpendicular, color, Vector2.zero);
        vh.AddVert(start - perpendicular, color, Vector2.zero);
        vh.AddVert(end + perpendicular, color, Vector2.zero);
        vh.AddVert(end - perpendicular, color, Vector2.zero);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 1, startIndex + 3, startIndex + 2);
    }

    /// <summary>
    /// 라벨 생성/업데이트
    /// </summary>
    private void UpdateLabels()
    {
        // 기존 라벨 제거
        foreach (GameObject label in labels)
        {
            if (label != null)
                Destroy(label);
        }
        labels.Clear();

        if (labelPrefab == null)
        {
            Debug.LogWarning("PatternRadarChart: labelPrefab이 설정되지 않았습니다.");
            return;
        }

        string[] patternNames = PatternRadarData.GetPatternNames();
        int patternCount = patternNames.Length;
        float angleStep = 360f / patternCount;

        for (int i = 0; i < patternCount; i++)
        {
            float angle = angleStep * i - 90f;
            Vector2 position = GetPointOnCircle(angle, labelDistance);

            GameObject labelObj = Instantiate(labelPrefab, transform);
            RectTransform rectTransform = labelObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }

            Text textComponent = labelObj.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = patternNames[i];
                textComponent.alignment = TextAnchor.MiddleCenter;
            }

            labels.Add(labelObj);
        }
    }

    /// <summary>
    /// 에디터에서 값 변경 시 업데이트
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 테스트용 더미 데이터 생성
    /// </summary>
    [ContextMenu("Generate Test Data")]
    public void GenerateTestData()
    {
        currentData = new PatternRadarData
        {
            trill = Random.Range(5f, 20f),
            stairs = Random.Range(5f, 20f),
            chord = Random.Range(5f, 20f),
            denim = Random.Range(5f, 20f),
            jacks = Random.Range(5f, 20f),
            longNoteHybrid = Random.Range(5f, 20f),
            burst = Random.Range(5f, 20f),
            offbeat = Random.Range(5f, 20f)
        };

        SetVerticesDirty();
        Debug.Log("테스트 데이터 생성 완료");
    }
#endif
}
