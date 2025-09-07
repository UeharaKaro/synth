using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class JudgmentOffsetDisplay : MonoBehaviour
{
    private GearSettings settings;
    private GameObject centerLine;
    private List<OffsetLine> activeLines = new List<OffsetLine>();
    private Queue<OffsetLine> pooledLines = new Queue<OffsetLine>();

    private float displayWidth = 4f;
    private float displayHeight = 0.5f;
    private float maxOffsetMs = 100f;

    private class OffsetLine
    {
        public GameObject gameObject;
        public Renderer renderer;
        public float createTime;
        public float offsetMs;
        public JudgmentType judgment;
    }

    public void Initialize(GearSettings gearSettings)
    {
        settings = gearSettings;
        CreateDisplay();
        CreateLinePool();
    }

    void CreateDisplay()
    {
        // 배경 생성
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "OffsetDisplayBG";
        bg.transform.SetParent(transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(displayWidth, displayHeight, 1);

        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0, 0, 0, 0.3f);
        bg.GetComponent<Renderer>().material = bgMat;

        // 중심선 (0ms) 생성
        centerLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
        centerLine.name = "CenterLine";
        centerLine.transform.SetParent(transform);
        centerLine.transform.localPosition = new Vector3(0, 0, -0.01f);
        centerLine.transform.localScale = new Vector3(0.02f, displayHeight * 0.8f, 1);

        Material centerMat = new Material(Shader.Find("Sprites/Default"));
        centerMat.color = new Color(1f, 1f, 1f, 0.5f);
        centerLine.GetComponent<Renderer>().material = centerMat;

        // 구분선 생성 (±50ms)
        CreateGuideLine(-displayWidth * 0.25f, 0.2f);
        CreateGuideLine(displayWidth * 0.25f, 0.2f);
    }

    void CreateGuideLine(float xPos, float alpha)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
        line.name = "GuideLine";
        line.transform.SetParent(transform);
        line.transform.localPosition = new Vector3(xPos, 0, -0.005f);
        line.transform.localScale = new Vector3(0.01f, displayHeight * 0.5f, 1);

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.5f, 0.5f, 0.5f, alpha);
        line.GetComponent<Renderer>().material = mat;
    }

    void CreateLinePool()
    {
        // 오브젝트 풀 생성
        for (int i = 0; i < 20; i++)
        {
            OffsetLine line = CreateOffsetLine();
            line.gameObject.SetActive(false);
            pooledLines.Enqueue(line);
        }
    }

    OffsetLine CreateOffsetLine()
    {
        GameObject lineObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        lineObj.name = "OffsetLine";
        lineObj.transform.SetParent(transform);
        lineObj.transform.localScale = new Vector3(0.05f, displayHeight * 0.6f, 1);

        Material mat = new Material(Shader.Find("Sprites/Default"));
        Renderer rend = lineObj.GetComponent<Renderer>();
        rend.material = mat;

        OffsetLine line = new OffsetLine
        {
            gameObject = lineObj,
            renderer = rend
        };

        return line;
    }

    public void ShowOffset(JudgmentType judgment, float offsetMs)
    {
        // 판정 범위 체크
        if (Mathf.Abs(offsetMs) > settings.judgmentDisplayRangeMs)
            return;

        // 판정 타입별 표시 여부 체크
        if (!ShouldShowJudgment(judgment))
            return;

        // 풀에서 라인 가져오기
        OffsetLine line = GetPooledLine();
        if (line == null)
            return;

        // 라인 설정
        line.judgment = judgment;
        line.offsetMs = offsetMs;
        line.createTime = Time.time;

        // 위치 설정 (오프셋에 따라 좌우 배치)
        float xPos = (offsetMs / maxOffsetMs) * (displayWidth * 0.4f);
        line.gameObject.transform.localPosition = new Vector3(xPos, 0, -0.02f);

        // 색상 설정
        Color lineColor = GetJudgmentColor(judgment);
        line.renderer.material.color = lineColor;

        // 활성화
        line.gameObject.SetActive(true);
        activeLines.Add(line);

        // 페이드 아웃 시작
        StartCoroutine(FadeOutLine(line));
    }

    bool ShouldShowJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return settings.showSPerfect;
            case JudgmentType.Perfect: return settings.showPerfect;
            case JudgmentType.Great: return settings.showGreat;
            case JudgmentType.Good: return settings.showGood;
            case JudgmentType.Bad: return settings.showBad;
            case JudgmentType.Miss: return settings.showMiss;
            default: return false;
        }
    }

    Color GetJudgmentColor(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect:
                return new Color(0.2f, 0.4f, 1f, 0.8f); // 파란색
            case JudgmentType.Perfect:
                return new Color(0.2f, 1f, 0.3f, 0.8f); // 초록색
            case JudgmentType.Great:
            case JudgmentType.Good:
            case JudgmentType.Bad:
                return new Color(1f, 1f, 0.2f, 0.8f); // 노란색
            case JudgmentType.Miss:
                return new Color(1f, 0.2f, 0.2f, 0.8f); // 빨간색
            default:
                return Color.white;
        }
    }

    OffsetLine GetPooledLine()
    {
        if (pooledLines.Count > 0)
        {
            return pooledLines.Dequeue();
        }

        // 풀이 비어있으면 새로 생성
        return CreateOffsetLine();
    }

    IEnumerator FadeOutLine(OffsetLine line)
    {
        float fadeDuration = 1f;
        float elapsed = 0f;
        Color originalColor = line.renderer.material.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // 페이드 아웃
            Color currentColor = originalColor;
            currentColor.a = Mathf.Lerp(originalColor.a, 0f, t);
            line.renderer.material.color = currentColor;

            // 위로 살짝 이동
            Vector3 pos = line.gameObject.transform.localPosition;
            pos.y = Mathf.Lerp(0, displayHeight * 0.2f, t);
            line.gameObject.transform.localPosition = pos;

            yield return null;
        }

        // 라인 반환
        ReturnLine(line);
    }

    void ReturnLine(OffsetLine line)
    {
        line.gameObject.SetActive(false);
        activeLines.Remove(line);
        pooledLines.Enqueue(line);
    }

    void Update()
    {
        // 오래된 라인 자동 제거
        for (int i = activeLines.Count - 1; i >= 0; i--)
        {
            if (Time.time - activeLines[i].createTime > 2f)
            {
                ReturnLine(activeLines[i]);
            }
        }
    }
}