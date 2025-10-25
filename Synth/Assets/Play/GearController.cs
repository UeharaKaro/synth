using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GearController : MonoBehaviour
{
    [Header("Settings")]
    public GearSettings settings;
    
    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject judgmentLinePrefab;
    public GameObject notePrefab;
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public Slider hpBar;
    public Image hpBarFill;
    public Transform judgmentOffsetDisplay;
    
    [Header("Containers")]
    public Transform linesContainer;
    public Transform notesContainer;
    public Transform gearBackground;
    
    private List<Transform> lines = new List<Transform>();
    private Transform judgmentLine;
    private HPBarAnimator hpBarAnimator;
    private JudgmentOffsetDisplay offsetDisplay;
    
    private int currentScore = 0;
    private int maxCombo = 0;
    private int currentCombo = 0;
    private float currentHP = 100f;
    
    void Start()
    {
        InitializeGear();
        SetupCamera();
        CreateHPBar();
        CreateJudgmentOffsetDisplay();
    }
    
    void InitializeGear()
    {
        // 기어 배경 생성
        CreateGearBackground();
        
        // 라인 생성
        CreateLines();
        
        // 판정선 생성
        CreateJudgmentLine();
        
        // UI 초기화
        UpdateScoreDisplay();
        UpdateComboDisplay();
    }
    
    void CreateGearBackground()
    {
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "GearBackground";
        bg.transform.SetParent(gearBackground);
        bg.transform.localPosition = new Vector3(0, settings.gearLength / 2, 0.1f);
        bg.transform.localScale = new Vector3(settings.GetTotalWidth(), settings.gearLength, 1);
        
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = settings.gearBackgroundColor;
        bg.GetComponent<Renderer>().material = bgMat;
        
        // 모던한 디자인을 위한 그라데이션 효과
        AddGradientEffect(bg);
    }
    
    void CreateLines()
    {
        float totalWidth = settings.GetTotalWidth();
        float lineSpacing = totalWidth / (settings.lineCount + 1);
        float startX = -totalWidth / 2 + lineSpacing;
        
        for (int i = 0; i < settings.lineCount; i++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.name = $"Line_{i}";
            line.transform.SetParent(linesContainer);
            
            float xPos = startX + (i * lineSpacing);
            line.transform.localPosition = new Vector3(xPos, settings.gearLength / 2, 0);
            line.transform.localScale = new Vector3(0.05f, settings.gearLength, 1);
            
            Material lineMat = new Material(Shader.Find("Sprites/Default"));
            lineMat.color = settings.lineColor;
            line.GetComponent<Renderer>().material = lineMat;
            
            lines.Add(line.transform);
            
            // 라인별 히트 영역 생성
            CreateHitArea(i, xPos);
        }
    }
    
    void CreateHitArea(int lineIndex, float xPos)
    {
        GameObject hitArea = new GameObject($"HitArea_{lineIndex}");
        hitArea.transform.SetParent(linesContainer);
        hitArea.transform.localPosition = new Vector3(xPos, settings.judgmentLineHeight, -0.1f);
        
        BoxCollider2D collider = hitArea.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(settings.lineWidth * 0.8f, 0.5f);
        collider.isTrigger = true;
        
        hitArea.tag = "HitArea";
        hitArea.layer = LayerMask.NameToLayer("HitArea");
    }
    
    void CreateJudgmentLine()
    {
        GameObject judgeLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
        judgeLine.name = "JudgmentLine";
        judgeLine.transform.SetParent(linesContainer);
        judgeLine.transform.localPosition = new Vector3(0, settings.judgmentLineHeight, -0.05f);
        judgeLine.transform.localScale = new Vector3(settings.GetTotalWidth(), 0.1f, 1);
        
        Material judgeMat = new Material(Shader.Find("Sprites/Default"));
        judgeMat.color = settings.judgmentLineColor;
        judgeLine.GetComponent<Renderer>().material = judgeMat;
        
        // 판정선 발광 효과
        AddGlowEffect(judgeLine);
        
        judgmentLine = judgeLine.transform;
    }
    
    void CreateHPBar()
    {
        GameObject hpBarObj = new GameObject("HPBar");
        hpBarObj.transform.SetParent(transform);
        
        float hpBarX = settings.GetTotalWidth() / 2 + settings.hpBarWidth + settings.hpBarMargin;
        hpBarObj.transform.localPosition = new Vector3(hpBarX, settings.gearLength / 2, 0);
        
        // HP바 배경
        GameObject hpBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hpBg.name = "HPBarBackground";
        hpBg.transform.SetParent(hpBarObj.transform);
        hpBg.transform.localPosition = Vector3.zero;
        hpBg.transform.localScale = new Vector3(settings.hpBarWidth, settings.gearLength, 1);
        
        Material hpBgMat = new Material(Shader.Find("Sprites/Default"));
        hpBgMat.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        hpBg.GetComponent<Renderer>().material = hpBgMat;
        
        // HP바 필
        GameObject hpFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hpFill.name = "HPBarFill";
        hpFill.transform.SetParent(hpBarObj.transform);
        hpFill.transform.localPosition = Vector3.zero;
        hpFill.transform.localScale = new Vector3(settings.hpBarWidth * 0.9f, settings.gearLength * 0.95f, 1);
        
        Material hpFillMat = new Material(Shader.Find("Sprites/Default"));
        hpFillMat.color = settings.hpBarFullColor;
        hpFill.GetComponent<Renderer>().material = hpFillMat;
        
        // HP바 애니메이터 추가
        hpBarAnimator = hpBarObj.AddComponent<HPBarAnimator>();
        hpBarAnimator.Initialize(hpFill.transform, settings);
    }
    
    void CreateJudgmentOffsetDisplay()
    {
        GameObject offsetObj = new GameObject("JudgmentOffsetDisplay");
        offsetObj.transform.SetParent(transform);
        offsetObj.transform.localPosition = new Vector3(0, settings.gearLength * 0.7f, -0.2f);
        
        offsetDisplay = offsetObj.AddComponent<JudgmentOffsetDisplay>();
        offsetDisplay.Initialize(settings);
    }
    
    void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, settings.cameraHeight, -10);
            mainCam.transform.rotation = Quaternion.Euler(settings.cameraAngle, 0, 0);
        }
    }
    
    void AddGradientEffect(GameObject obj)
    {
        // 그라데이션 효과를 위한 추가 쿼드 생성
        GameObject gradient = GameObject.CreatePrimitive(PrimitiveType.Quad);
        gradient.name = "Gradient";
        gradient.transform.SetParent(obj.transform);
        gradient.transform.localPosition = new Vector3(0, 0, -0.01f);
        gradient.transform.localScale = Vector3.one;
        
        Material gradMat = new Material(Shader.Find("Sprites/Default"));
        gradMat.color = new Color(0, 0, 0, 0.3f);
        gradient.GetComponent<Renderer>().material = gradMat;
    }
    
    void AddGlowEffect(GameObject obj)
    {
        // 발광 효과를 위한 추가 오브젝트
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glow.name = "Glow";
        glow.transform.SetParent(obj.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(1.2f, 2f, 1);
        
        Material glowMat = new Material(Shader.Find("Sprites/Default"));
        glowMat.color = new Color(settings.judgmentLineColor.r, settings.judgmentLineColor.g, 
                                  settings.judgmentLineColor.b, 0.3f);
        glow.GetComponent<Renderer>().material = glowMat;
    }
    
    /// <summary>
    /// 판정에 따른 점수 및 콤보 처리
    /// </summary>
    public void ProcessJudgment(JudgmentType judgment)
    {
        // 점수 증가
        int scoreToAdd = GetScoreForJudgment(judgment);
        currentScore += scoreToAdd;
        UpdateScoreDisplay();

        // 콤보 처리
        if (judgment == JudgmentType.Bad || judgment == JudgmentType.Miss)
        {
            // 콤보 끊김
            currentCombo = 0;
        }
        else
        {
            // 콤보 증가
            currentCombo++;
            if (currentCombo > maxCombo)
                maxCombo = currentCombo;
        }
        UpdateComboDisplay();

        // 판정 표시 (ComboJudgmentDisplay를 통해)
        // TODO: ComboJudgmentDisplay 연동 필요 
        // 이것을 해결하기위해 : 1. ComboJudgmentDisplay 컴포넌트 참조 추가
        // 2. ProcessJudgment() 메서드에서 판정 타입(S_Perfect, Perfect, Great 등)을 ComboJudgmentDisplay에 전달하여 화면에 표시
    }

    /// <summary>
    /// 판정별 점수 반환 (기본 점수, 추후 차트 총 노트 수에 따라 조정)
    /// </summary>
    private int GetScoreForJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect:
                return 1000; // 최고 점수
            case JudgmentType.Perfect:
                return 900;
            case JudgmentType.Great:
                return 700;
            case JudgmentType.Good:
                return 400;
            case JudgmentType.Bad:
                return 100;
            case JudgmentType.Miss:
                return 0;
            default:
                return 0;
        }
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
        UpdateScoreDisplay();
    }

    public void UpdateCombo(int combo)
    {
        currentCombo = combo;
        if (combo > maxCombo)
            maxCombo = combo;
        UpdateComboDisplay();
    }

    public void UpdateHP(float hp)
    {
        currentHP = Mathf.Clamp(hp, 0, 100);
        if (hpBarAnimator != null)
            hpBarAnimator.SetHP(currentHP);
    }

    public void ShowJudgmentOffset(JudgmentType judgment, float offsetMs)
    {
        if (offsetDisplay != null)
            offsetDisplay.ShowOffset(judgment, offsetMs);
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {currentScore:D7}";
    }
    
    void UpdateComboDisplay()
    {
        if (comboText != null)
            comboText.text = $"COMBO: {currentCombo}\nMAX: {maxCombo}";
    }
    
    public Transform GetLine(int index)
    {
        if (index >= 0 && index < lines.Count)
            return lines[index];
        return null;
    }
    
    public float GetJudgmentLineY()
    {
        return settings.judgmentLineHeight;
    }
}
