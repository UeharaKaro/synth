using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 실시간 점수 표시 UI
/// - 카운트업 애니메이션
/// - 천 단위 콤마
/// - 목표 점수 비교 (선택사항)
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [Header("UI Position")]
    [SerializeField] private Vector3 scorePosition = new Vector3(-5f, 4f, -0.1f);
    
    [Header("Display Settings")]
    [SerializeField] private bool useComma = true;
    [SerializeField] private bool showLabel = true;
    [SerializeField] private bool animateOnIncrease = true;
    [SerializeField] private float countUpSpeed = 5f; // 점수 카운트업 속도
    
    [Header("Colors")]
    [SerializeField] private Color labelColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color scoreColor = Color.white;
    [SerializeField] private Color increaseColor = new Color(1f, 0.8f, 0.2f, 1f); // 점수 증가 시 색상
    
    [Header("Visual Effects")]
    [SerializeField] private bool showScorePopup = true; // 점수 증가량 팝업 표시
    [SerializeField] private float popupDuration = 0.5f;
    
    // UI Elements
    private GameObject scoreContainer;
    private TextMeshPro scoreLabel;
    private TextMeshPro scoreText;
    
    // Runtime Data
    private int currentScore = 0;
    private int displayScore = 0;
    private int targetScore = 0;
    
    private Coroutine countUpCoroutine;
    private Coroutine flashCoroutine;
    
    void Start()
    {
        CreateScoreDisplay();
    }
    
    void Update()
    {
        UpdateScoreDisplay();
    }
    
    void CreateScoreDisplay()
    {
        // Container
        scoreContainer = new GameObject("ScoreContainer");
        scoreContainer.transform.SetParent(transform);
        scoreContainer.transform.localPosition = scorePosition;
        
        // Label (SCORE)
        if (showLabel)
        {
            GameObject labelObj = new GameObject("ScoreLabel");
            labelObj.transform.SetParent(scoreContainer.transform);
            labelObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            scoreLabel = labelObj.AddComponent<TextMeshPro>();
            scoreLabel.text = "SCORE";
            scoreLabel.fontSize = 2.5f;
            scoreLabel.alignment = TextAlignmentOptions.Center;
            scoreLabel.color = labelColor;
        }
        
        // Score Number
        GameObject scoreObj = new GameObject("ScoreNumber");
        scoreObj.transform.SetParent(scoreContainer.transform);
        scoreObj.transform.localPosition = Vector3.zero;
        
        scoreText = scoreObj.AddComponent<TextMeshPro>();
        scoreText.text = "0";
        scoreText.fontSize = 6f;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.color = scoreColor;
        scoreText.fontStyle = FontStyles.Bold;
        
        // Background
        CreateScoreBackground();
    }
    
    void CreateScoreBackground()
    {
        GameObject scoreBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        scoreBg.name = "ScoreBackground";
        scoreBg.transform.SetParent(scoreContainer.transform);
        scoreBg.transform.localPosition = new Vector3(0, 0, 0.1f);
        scoreBg.transform.localScale = new Vector3(5f, 2f, 1f);
        
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0, 0, 0, 0.3f);
        scoreBg.GetComponent<Renderer>().material = bgMat;
        
        // Glow effect
        GameObject scoreGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        scoreGlow.name = "ScoreGlow";
        scoreGlow.transform.SetParent(scoreContainer.transform);
        scoreGlow.transform.localPosition = new Vector3(0, 0, 0.05f);
        scoreGlow.transform.localScale = new Vector3(5.5f, 2.5f, 1f);
        
        Material glowMat = new Material(Shader.Find("Sprites/Default"));
        glowMat.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        scoreGlow.GetComponent<Renderer>().material = glowMat;
    }
    
    void UpdateScoreDisplay()
    {
        if (displayScore != targetScore)
        {
            // Smooth count up
            int difference = Mathf.Abs(targetScore - displayScore);
            int step = Mathf.Max(1, Mathf.CeilToInt(difference * Time.deltaTime * countUpSpeed));
            
            if (displayScore < targetScore)
                displayScore = Mathf.Min(displayScore + step, targetScore);
            else
                displayScore = Mathf.Max(displayScore - step, targetScore);
            
            UpdateScoreText();
        }
    }
    
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            if (useComma)
                scoreText.text = FormatNumberWithComma(displayScore);
            else
                scoreText.text = displayScore.ToString();
        }
    }
    
    string FormatNumberWithComma(int number)
    {
        return number.ToString("N0");
    }
    
    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int amount)
    {
        int oldScore = targetScore;
        targetScore += amount;
        currentScore = targetScore;
        
        if (animateOnIncrease)
        {
            // Flash animation
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashScoreAnimation());
        }
        
        if (showScorePopup && amount > 0)
        {
            // Show score increase popup
            ShowScoreIncreasePopup(amount);
        }
    }
    
    /// <summary>
    /// 점수 직접 설정
    /// </summary>
    public void SetScore(int score)
    {
        targetScore = score;
        currentScore = score;
    }
    
    /// <summary>
    /// 현재 점수 가져오기
    /// </summary>
    public int GetScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// 점수 초기화
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        displayScore = 0;
        targetScore = 0;
        UpdateScoreText();
    }
    
    IEnumerator FlashScoreAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Color originalColor = scoreText.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Color flash
            scoreText.color = Color.Lerp(increaseColor, originalColor, t);
            
            // Scale pulse
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
            scoreText.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        scoreText.color = originalColor;
        scoreText.transform.localScale = Vector3.one;
    }
    
    void ShowScoreIncreasePopup(int amount)
    {
        GameObject popup = new GameObject("ScorePopup");
        popup.transform.SetParent(scoreContainer.transform);
        popup.transform.localPosition = new Vector3(0, -0.8f, -0.05f);
        
        TextMeshPro popupText = popup.AddComponent<TextMeshPro>();
        popupText.text = $"+{FormatNumberWithComma(amount)}";
        popupText.fontSize = 3f;
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.color = increaseColor;
        popupText.fontStyle = FontStyles.Bold;
        
        StartCoroutine(AnimateScorePopup(popup, popupText));
    }
    
    IEnumerator AnimateScorePopup(GameObject popup, TextMeshPro text)
    {
        float elapsed = 0f;
        Vector3 startPos = popup.transform.localPosition;
        
        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;
            
            // Move up and fade out
            popup.transform.localPosition = startPos + Vector3.up * t * 1f;
            popup.transform.localScale = Vector3.one * (1f + t * 0.3f);
            
            Color c = text.color;
            c.a = 1f - t;
            text.color = c;
            
            yield return null;
        }
        
        Destroy(popup);
    }
    
    /// <summary>
    /// 점수 표시 색상 변경
    /// </summary>
    public void SetScoreColor(Color color)
    {
        if (scoreText != null)
            scoreText.color = color;
    }
}
