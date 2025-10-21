using UnityEngine;
using System.Collections;
using TMPro;

public class ComboJudgmentDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    public float comboDisplayY = -2f; // 판정 오차 표시 아래
    public float judgmentDisplayY = -3f; // 콤보 표시 아래
    
    [Header("Combo Display")]
    private TextMeshPro comboText;
    private TextMeshPro comboNumberText;
    private GameObject comboContainer;
    private int currentCombo = 0;
    private float comboScale = 1f;
    
    [Header("Judgment Display")]
    private TextMeshPro judgmentText;
    private GameObject judgmentContainer;
    private float judgmentDisplayDuration = 0.5f;
    
    [Header("Visual Effects")]
    private Color[] judgmentColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1f, 1f),   // S_Perfect - 파란색
        new Color(0.2f, 1f, 0.4f, 1f),   // Perfect - 초록색
        new Color(1f, 0.9f, 0.2f, 1f),   // Great - 노란색
        new Color(1f, 0.6f, 0.2f, 1f),   // Good - 주황색
        new Color(1f, 0.4f, 0.2f, 1f),   // Bad - 빨간주황색
        new Color(1f, 0.2f, 0.2f, 1f)    // Miss - 빨간색
    };
    
    private Coroutine judgmentCoroutine;
    private Coroutine comboAnimCoroutine;
    
    void Start()
    {
        CreateComboDisplay();
        CreateJudgmentDisplay();
    }
    
    void CreateComboDisplay()
    {
        // 콤보 컨테이너
        comboContainer = new GameObject("ComboContainer");
        comboContainer.transform.SetParent(transform);
        comboContainer.transform.localPosition = new Vector3(0, comboDisplayY, -0.1f);
        
        // "COMBO" 텍스트
        GameObject comboLabelObj = new GameObject("ComboLabel");
        comboLabelObj.transform.SetParent(comboContainer.transform);
        comboLabelObj.transform.localPosition = new Vector3(0, 0.3f, 0);
        
        comboText = comboLabelObj.AddComponent<TextMeshPro>();
        comboText.text = "COMBO";
        comboText.fontSize = 3f;
        comboText.alignment = TextAlignmentOptions.Center;
        comboText.color = new Color(0.8f, 0.8f, 1f, 0.8f);
        
        // 콤보 숫자
        GameObject comboNumObj = new GameObject("ComboNumber");
        comboNumObj.transform.SetParent(comboContainer.transform);
        comboNumObj.transform.localPosition = new Vector3(0, -0.3f, 0);
        
        comboNumberText = comboNumObj.AddComponent<TextMeshPro>();
        comboNumberText.text = "0";
        comboNumberText.fontSize = 8f;
        comboNumberText.alignment = TextAlignmentOptions.Center;
        comboNumberText.color = Color.white;
        comboNumberText.fontStyle = FontStyles.Bold;
        
        // 콤보 배경 효과
        CreateComboBackground();
    }
    
    void CreateComboBackground()
    {
        GameObject comboBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        comboBg.name = "ComboBackground";
        comboBg.transform.SetParent(comboContainer.transform);
        comboBg.transform.localPosition = new Vector3(0, 0, 0.1f);
        comboBg.transform.localScale = new Vector3(3f, 2f, 1f);
        
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0, 0, 0, 0.3f);
        comboBg.GetComponent<Renderer>().material = bgMat;
        
        // 글로우 효과
        GameObject comboGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        comboGlow.name = "ComboGlow";
        comboGlow.transform.SetParent(comboContainer.transform);
        comboGlow.transform.localPosition = new Vector3(0, 0, 0.05f);
        comboGlow.transform.localScale = new Vector3(3.5f, 2.5f, 1f);
        
        Material glowMat = new Material(Shader.Find("Sprites/Default"));
        glowMat.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        comboGlow.GetComponent<Renderer>().material = glowMat;
    }
    
    void CreateJudgmentDisplay()
    {
        // 판정 컨테이너
        judgmentContainer = new GameObject("JudgmentContainer");
        judgmentContainer.transform.SetParent(transform);
        judgmentContainer.transform.localPosition = new Vector3(0, judgmentDisplayY, -0.1f);
        
        // 판정 텍스트
        GameObject judgmentObj = new GameObject("JudgmentText");
        judgmentObj.transform.SetParent(judgmentContainer.transform);
        judgmentObj.transform.localPosition = Vector3.zero;
        
        judgmentText = judgmentObj.AddComponent<TextMeshPro>();
        judgmentText.text = "";
        judgmentText.fontSize = 5f;
        judgmentText.alignment = TextAlignmentOptions.Center;
        judgmentText.fontStyle = FontStyles.Bold;
        
        // 판정 배경
        CreateJudgmentBackground();
    }
    
    void CreateJudgmentBackground()
    {
        GameObject judgmentBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        judgmentBg.name = "JudgmentBackground";
        judgmentBg.transform.SetParent(judgmentContainer.transform);
        judgmentBg.transform.localPosition = new Vector3(0, 0, 0.1f);
        judgmentBg.transform.localScale = new Vector3(4f, 1f, 1f);
        
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0, 0, 0, 0.2f);
        judgmentBg.GetComponent<Renderer>().material = bgMat;
    }
    
    public void UpdateCombo(int combo, bool resetCombo = false)
    {
        if (resetCombo)
        {
            currentCombo = 0;
            ShowComboBreak();
        }
        else
        {
            currentCombo = combo;
            ShowComboIncrease();
        }
        
        UpdateComboDisplay();
    }
    
    void UpdateComboDisplay()
    {
        comboNumberText.text = currentCombo.ToString();
        
        // 콤보 수에 따른 색상 변경
        if (currentCombo >= 100)
        {
            comboNumberText.color = new Color(1f, 0.8f, 0.2f, 1f); // 골드
            comboScale = 1.3f;
        }
        else if (currentCombo >= 50)
        {
            comboNumberText.color = new Color(0.8f, 0.8f, 1f, 1f); // 은색
            comboScale = 1.2f;
        }
        else if (currentCombo >= 20)
        {
            comboNumberText.color = new Color(0.6f, 0.8f, 1f, 1f); // 하늘색
            comboScale = 1.1f;
        }
        else
        {
            comboNumberText.color = Color.white;
            comboScale = 1f;
        }
    }
    
    void ShowComboIncrease()
    {
        if (comboAnimCoroutine != null)
            StopCoroutine(comboAnimCoroutine);
        
        comboAnimCoroutine = StartCoroutine(ComboIncreaseAnimation());
    }
    
    void ShowComboBreak()
    {
        if (comboAnimCoroutine != null)
            StopCoroutine(comboAnimCoroutine);
        
        comboAnimCoroutine = StartCoroutine(ComboBreakAnimation());
    }
    
    IEnumerator ComboIncreaseAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 팝 애니메이션
            float scale = comboScale * (1f + Mathf.Sin(t * Mathf.PI) * 0.3f);
            comboNumberText.transform.localScale = Vector3.one * scale;
            
            // 회전 효과
            float rotation = Mathf.Sin(t * Mathf.PI * 2) * 5f;
            comboNumberText.transform.rotation = Quaternion.Euler(0, 0, rotation);
            
            yield return null;
        }
        
        comboNumberText.transform.localScale = Vector3.one * comboScale;
        comboNumberText.transform.rotation = Quaternion.identity;
    }
    
    IEnumerator ComboBreakAnimation()
    {
        // 콤보 브레이크 효과
        GameObject breakEffect = new GameObject("ComboBreak");
        breakEffect.transform.SetParent(comboContainer.transform);
        breakEffect.transform.localPosition = Vector3.zero;
        
        TextMeshPro breakText = breakEffect.AddComponent<TextMeshPro>();
        breakText.text = "BREAK!";
        breakText.fontSize = 6f;
        breakText.alignment = TextAlignmentOptions.Center;
        breakText.color = new Color(1f, 0.2f, 0.2f, 1f);
        breakText.fontStyle = FontStyles.Bold;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 확대 후 페이드아웃
            breakText.transform.localScale = Vector3.one * (1f + t * 0.5f);
            Color c = breakText.color;
            c.a = 1f - t;
            breakText.color = c;
            
            // 진동 효과
            float shakeX = Random.Range(-0.1f, 0.1f) * (1f - t);
            float shakeY = Random.Range(-0.1f, 0.1f) * (1f - t);
            breakText.transform.localPosition = new Vector3(shakeX, shakeY, -0.1f);
            
            yield return null;
        }
        
        Destroy(breakEffect);
    }
    
    public void ShowJudgment(JudgmentType judgment)
    {
        if (judgmentCoroutine != null)
            StopCoroutine(judgmentCoroutine);
        
        judgmentCoroutine = StartCoroutine(DisplayJudgment(judgment));
    }
    
    IEnumerator DisplayJudgment(JudgmentType judgment)
    {
        // 판정 텍스트 설정
        string judgmentString = GetJudgmentString(judgment);
        judgmentText.text = judgmentString;
        judgmentText.color = judgmentColors[(int)judgment];
        
        // 판정별 이펙트
        CreateJudgmentEffect(judgment);
        
        float elapsed = 0f;
        
        // 나타나기 애니메이션
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;
            
            judgmentText.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, t);
            judgmentText.color = new Color(judgmentText.color.r, judgmentText.color.g, 
                                          judgmentText.color.b, t);
            
            yield return null;
        }
        
        // 유지
        judgmentText.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(judgmentDisplayDuration - 0.2f);
        
        // 페이드아웃
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;
            
            judgmentText.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.8f, t);
            Color c = judgmentText.color;
            c.a = 1f - t;
            judgmentText.color = c;
            
            yield return null;
        }
        
        judgmentText.text = "";
    }
    
    string GetJudgmentString(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return "S-PERFECT!";
            case JudgmentType.Perfect: return "PERFECT!";
            case JudgmentType.Great: return "GREAT";
            case JudgmentType.Good: return "GOOD";
            case JudgmentType.Bad: return "BAD";
            case JudgmentType.Miss: return "MISS";
            default: return "";
        }
    }
    
    void CreateJudgmentEffect(JudgmentType judgment)
    {
        if (judgment <= JudgmentType.Perfect)
        {
            // Perfect 이상일 때 특수 효과
            StartCoroutine(CreatePerfectEffect());
        }
        
        if (judgment == JudgmentType.S_Perfect)
        {
            // S-Perfect일 때 추가 효과
            StartCoroutine(CreateS_PerfectRipple());
        }
    }
    
    IEnumerator CreatePerfectEffect()
    {
        // 별 파티클 효과
        for (int i = 0; i < 5; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.transform.SetParent(judgmentContainer.transform);
            star.transform.localScale = Vector3.one * 0.2f;
            
            float angle = i * 72f * Mathf.Deg2Rad;
            Vector3 startPos = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, -0.05f);
            star.transform.localPosition = startPos;
            
            Material starMat = new Material(Shader.Find("Sprites/Default"));
            starMat.color = new Color(1f, 1f, 0.5f, 1f);
            star.GetComponent<Renderer>().material = starMat;
            
            StartCoroutine(AnimateStar(star, startPos));
        }
        
        yield return null;
    }
    
    IEnumerator AnimateStar(GameObject star, Vector3 startPos)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Material mat = star.GetComponent<Renderer>().material;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 외부로 퍼지며 페이드아웃
            star.transform.localPosition = startPos * (1f + t * 2f);
            star.transform.localScale = Vector3.one * 0.2f * (1f - t * 0.5f);
            
            Color c = mat.color;
            c.a = 1f - t;
            mat.color = c;
            
            yield return null;
        }
        
        Destroy(star);
    }
    
    IEnumerator CreateS_PerfectRipple()
    {
        // S-Perfect 리플 효과
        GameObject ripple = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ripple.transform.SetParent(judgmentContainer.transform);
        ripple.transform.localPosition = new Vector3(0, 0, 0.05f);
        ripple.transform.localScale = Vector3.one;
        
        Material rippleMat = new Material(Shader.Find("Sprites/Default"));
        rippleMat.color = new Color(0.2f, 0.6f, 1f, 0.5f);
        ripple.GetComponent<Renderer>().material = rippleMat;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            ripple.transform.localScale = Vector3.one * (1f + t * 3f);
            Color c = rippleMat.color;
            c.a = 0.5f * (1f - t);
            rippleMat.color = c;
            
            yield return null;
        }
        
        Destroy(ripple);
    }
}