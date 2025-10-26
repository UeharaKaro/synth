using UnityEngine;
using System.Collections;
using TMPro;

public class HPBarAnimator : MonoBehaviour
{
    [Header("HP Bar Settings")]
    private Transform hpFillTransform;
    private GearSettings settings;
    private Material hpMaterial;
    
    [Header("Clear Line Settings")]
    [SerializeField] private bool showClearLine = true;
    [SerializeField] private float clearThreshold = 70f; // Normal 모드 기준
    [SerializeField] private Color clearLineColor = new Color(0f, 1f, 0f, 0.8f);
    
    [Header("HP Text Settings")]
    [SerializeField] private bool showHPPercentage = true;
    [SerializeField] private Color hpTextColor = Color.white;
    
    private float currentHP = 100f;
    private float targetHP = 100f;
    private float bpm = 120f;
    private float beatInterval;
    
    private Vector3 originalScale;
    private Color originalColor;
    
    private bool isAnimating = false;
    private float poppingIntensity = 0.1f;
    private float glowIntensity = 0.3f;
    
    // UI Elements
    private GameObject clearLine;
    private TextMeshPro hpText;
    
    // Danger zone effect
    private bool isInDangerZone = false;
    private Coroutine dangerPulseCoroutine;
    
    public void Initialize(Transform fillTransform, GearSettings gearSettings)
    {
        hpFillTransform = fillTransform;
        settings = gearSettings;
        hpMaterial = hpFillTransform.GetComponent<Renderer>().material;
        originalScale = hpFillTransform.localScale;
        originalColor = settings.hpBarFullColor;
        
        SetBPM(120f); // 기본 BPM
        StartCoroutine(BeatAnimation());
        
        // Create UI elements
        if (showClearLine)
            CreateClearLine();
        
        if (showHPPercentage)
            CreateHPText();
    }
    
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        beatInterval = 60f / bpm;
    }
    
    public void SetHP(float hp)
    {
        targetHP = Mathf.Clamp(hp, 0, 100);
        if (!isAnimating)
            StartCoroutine(UpdateHPBar());
    }
    
    IEnumerator UpdateHPBar()
    {
        isAnimating = true;
        
        while (Mathf.Abs(currentHP - targetHP) > 0.1f)
        {
            currentHP = Mathf.Lerp(currentHP, targetHP, Time.deltaTime * 5f);
            
            // HP에 따른 크기 조정
            float hpRatio = currentHP / 100f;
            Vector3 newScale = originalScale;
            newScale.y = originalScale.y * hpRatio;
            
            // 하단 기준으로 스케일 조정
            hpFillTransform.localScale = newScale;
            float yOffset = (1f - hpRatio) * originalScale.y * 0.5f;
            hpFillTransform.localPosition = new Vector3(0, -yOffset, -0.01f);
            
            // HP에 따른 색상 변경
            Color targetColor = Color.Lerp(settings.hpBarEmptyColor, settings.hpBarFullColor, hpRatio);
            hpMaterial.color = targetColor;
            
            // Update HP text
            if (showHPPercentage && hpText != null)
            {
                hpText.text = $"{currentHP:F0}%";
            }
            
            // Danger zone check
            CheckDangerZone(hpRatio);
            
            yield return null;
        }
        
        currentHP = targetHP;
        isAnimating = false;
    }
    
    IEnumerator BeatAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(beatInterval);
            
            // BPM에 맞춘 발광 효과
            StartCoroutine(GlowEffect());
            
            // HP가 100% 미만일 때 팝핑 모션
            if (currentHP < 100f)
            {
                StartCoroutine(PoppingMotion());
            }
        }
    }
    
    IEnumerator GlowEffect()
    {
        float duration = beatInterval * 0.3f;
        float elapsed = 0f;
        Color baseColor = hpMaterial.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 발광 효과를 위한 밝기 조절
            float glowAmount = Mathf.Sin(t * Mathf.PI) * glowIntensity;
            Color glowColor = baseColor * (1f + glowAmount);
            glowColor.a = baseColor.a;
            
            hpMaterial.color = glowColor;
            
            // 엣지 글로우 효과
            CreateGlowPulse(glowAmount);
            
            yield return null;
        }
        
        hpMaterial.color = baseColor;
    }
    
    IEnumerator PoppingMotion()
    {
        float duration = beatInterval * 0.2f;
        float elapsed = 0f;
        Vector3 currentScale = hpFillTransform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 위로 솟아오르는 팝핑 애니메이션
            float popAmount = Mathf.Sin(t * Mathf.PI) * poppingIntensity;
            Vector3 popScale = currentScale;
            popScale.y = currentScale.y * (1f + popAmount);
            popScale.x = currentScale.x * (1f - popAmount * 0.3f); // 약간의 스퀴시 효과
            
            hpFillTransform.localScale = popScale;
            
            yield return null;
        }
        
        hpFillTransform.localScale = currentScale;
    }
    
    void CreateGlowPulse(float intensity)
    {
        // 엣지 글로우를 위한 추가 비주얼 효과
        if (hpFillTransform.childCount > 0)
        {
            Transform glowEdge = hpFillTransform.GetChild(0);
            if (glowEdge != null)
            {
                Material edgeMat = glowEdge.GetComponent<Renderer>().material;
                if (edgeMat != null)
                {
                    Color edgeColor = originalColor;
                    edgeColor.a = 0.5f * intensity;
                    edgeMat.color = edgeColor;
                }
            }
        }
    }
    
    void OnEnable()
    {
        CreateEdgeGlow();
    }
    
    void CreateEdgeGlow()
    {
        // HP바 엣지 글로우 오브젝트 생성
        GameObject edgeGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        edgeGlow.name = "EdgeGlow";
        edgeGlow.transform.SetParent(hpFillTransform);
        edgeGlow.transform.localPosition = Vector3.zero;
        edgeGlow.transform.localScale = new Vector3(1.3f, 1.05f, 1f);
        
        Material edgeMat = new Material(Shader.Find("Sprites/Default"));
        edgeMat.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        edgeGlow.GetComponent<Renderer>().material = edgeMat;
    }
    
    void CreateClearLine()
    {
        clearLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
        clearLine.name = "ClearLine";
        clearLine.transform.SetParent(hpFillTransform.parent);
        
        // Position at clear threshold
        float clearY = (clearThreshold / 100f - 0.5f) * originalScale.y;
        clearLine.transform.localPosition = new Vector3(0, clearY, -0.02f);
        clearLine.transform.localScale = new Vector3(originalScale.x * 1.2f, 0.05f, 1f);
        
        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = clearLineColor;
        clearLine.GetComponent<Renderer>().material = lineMat;
        
        // Create clear line text
        GameObject textObj = new GameObject("ClearLineText");
        textObj.transform.SetParent(clearLine.transform);
        textObj.transform.localPosition = new Vector3(originalScale.x * 0.7f, 0, -0.01f);
        
        TextMeshPro clearText = textObj.AddComponent<TextMeshPro>();
        clearText.text = "CLEAR";
        clearText.fontSize = 1.5f;
        clearText.alignment = TextAlignmentOptions.Center;
        clearText.color = clearLineColor;
        clearText.fontStyle = FontStyles.Bold;
    }
    
    void CreateHPText()
    {
        GameObject textObj = new GameObject("HPPercentageText");
        textObj.transform.SetParent(hpFillTransform.parent);
        textObj.transform.localPosition = new Vector3(0, originalScale.y * 0.6f, -0.02f);
        
        hpText = textObj.AddComponent<TextMeshPro>();
        hpText.text = "100%";
        hpText.fontSize = 3f;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = hpTextColor;
        hpText.fontStyle = FontStyles.Bold;
    }
    
    void CheckDangerZone(float hpRatio)
    {
        bool wasDanger = isInDangerZone;
        isInDangerZone = hpRatio < (clearThreshold / 100f);
        
        if (isInDangerZone && !wasDanger)
        {
            // Enter danger zone
            if (dangerPulseCoroutine != null)
                StopCoroutine(dangerPulseCoroutine);
            dangerPulseCoroutine = StartCoroutine(DangerPulseEffect());
        }
        else if (!isInDangerZone && wasDanger)
        {
            // Exit danger zone
            if (dangerPulseCoroutine != null)
            {
                StopCoroutine(dangerPulseCoroutine);
                dangerPulseCoroutine = null;
            }
        }
    }
    
    IEnumerator DangerPulseEffect()
    {
        while (isInDangerZone)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Pulse effect on HP bar
                float pulseAmount = Mathf.Sin(t * Mathf.PI * 2f) * 0.2f;
                Color dangerColor = settings.hpBarEmptyColor;
                dangerColor.r = Mathf.Clamp01(dangerColor.r + pulseAmount);
                hpMaterial.color = Color.Lerp(hpMaterial.color, dangerColor, Time.deltaTime * 5f);
                
                // Pulse effect on HP text
                if (hpText != null)
                {
                    Color textColor = hpTextColor;
                    textColor.r = 1f;
                    textColor.a = 0.7f + Mathf.Abs(pulseAmount);
                    hpText.color = textColor;
                }
                
                yield return null;
            }
        }
        
        // Reset colors
        if (hpText != null)
            hpText.color = hpTextColor;
    }
    
    /// <summary>
    /// 클리어 라인 임계값 설정
    /// </summary>
    public void SetClearThreshold(float threshold)
    {
        clearThreshold = threshold;
        
        if (clearLine != null)
        {
            float clearY = (clearThreshold / 100f - 0.5f) * originalScale.y;
            clearLine.transform.localPosition = new Vector3(0, clearY, -0.02f);
        }
    }
}