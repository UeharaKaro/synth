using UnityEngine;
using System.Collections;

public class HPBarAnimator : MonoBehaviour
{
    private Transform hpFillTransform;
    private GearSettings settings;
    private Material hpMaterial;
    
    private float currentHP = 100f;
    private float targetHP = 100f;
    private float bpm = 120f;
    private float beatInterval;
    
    private Vector3 originalScale;
    private Color originalColor;
    
    private bool isAnimating = false;
    private float poppingIntensity = 0.1f;
    private float glowIntensity = 0.3f;
    
    public void Initialize(Transform fillTransform, GearSettings gearSettings)
    {
        hpFillTransform = fillTransform;
        settings = gearSettings;
        hpMaterial = hpFillTransform.GetComponent<Renderer>().material;
        originalScale = hpFillTransform.localScale;
        originalColor = settings.hpBarFullColor;
        
        SetBPM(120f); // 기본 BPM
        StartCoroutine(BeatAnimation());
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
}