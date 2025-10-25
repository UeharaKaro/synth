using System;
using UnityEngine;
using UnityEngine.UI;

// 타이밍 정확도 열거형
public enum TimingAccuracy
{
    None,
    Fast,
    Late
}

// 스코어 타입 열거형
public enum ScoreType
{
    Type1A, Type1B, Type1C, Type1D,
    Type2A, Type2B, Type2C, Type2D
}

// 판정 결과 데이터 클래스
[System.Serializable]
public class JudgmentResult
{
    public JudgmentType judgment;
    public TimingAccuracy timing;
    public int score;
    
    public JudgmentResult(JudgmentType judgment, TimingAccuracy timing, int score)
    {
        this.judgment = judgment;
        this.timing = timing;
        this.score = score;
    }
}

// 메인 스코어 시스템 클래스
public class RhythmScoreSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ScoreType currentScoreType = ScoreType.Type1A;
    [SerializeField] private int totalNotes = 100; // 차트의 총 노트 수
    
    [Header("Score Display")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;
    [SerializeField] private Text judgmentText;
    
    [Header("Colors")]
    [SerializeField] private Color fastColor = Color.blue;
    [SerializeField] private Color lateColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    
    private int currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    
    // 판정별 카운트
    private int sPerfectCount = 0;
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int badCount = 0;
    private int missCount = 0;
    
    void Start()
    {
        InitializeScore();
    }
    
    public void InitializeScore()
    {
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        sPerfectCount = 0;
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        badCount = 0;
        missCount = 0;
        UpdateScoreDisplay();
    }
    
    // 노트 히트 처리 메인 함수
    public void ProcessNoteHit(JudgmentType judgment, float timingDiff)
    {
        TimingAccuracy timing = DetermineTimingAccuracy(timingDiff);
        int scoreToAdd = CalculateScore(judgment);
        
        // 판정 결과 생성
        JudgmentResult result = new JudgmentResult(judgment, timing, scoreToAdd);
        
        // 점수 및 콤보 업데이트
        UpdateScore(result);
        UpdateCombo(judgment);
        
        // 판정 표시
        DisplayJudgment(result);
        
        // 판정 카운트 업데이트
        UpdateJudgmentCount(judgment);
    }
    
    // 타이밍 정확도 판단
    private TimingAccuracy DetermineTimingAccuracy(float timingDiff)
    {
        if (Mathf.Abs(timingDiff) < 0.015f) // Perfect 범위
            return TimingAccuracy.None;
        else if (timingDiff < 0)
            return TimingAccuracy.Fast;
        else
            return TimingAccuracy.Late;
    }
    
    // 점수 계산
    private int CalculateScore(JudgmentType judgment)
    {
        bool isType1 = currentScoreType.ToString().StartsWith("Type1");
        
        int baseScore;
        if (isType1)
        {
            // Type 1-x: 최대 1,000,000점
            baseScore = 1000000 / totalNotes;
        }
        else
        {
            // Type 2-x: 최대 1,000,000 + 노트수
            baseScore = 1000000 / totalNotes;
        }
        
        switch (judgment)
        {
            case JudgmentType.S_Perfect:
                return isType1 ? baseScore : (baseScore + 1);
                
            case JudgmentType.Perfect:
                return baseScore;
                
            case JudgmentType.Great:
                return (int)(baseScore * 0.8f);
                
            case JudgmentType.Good:
                return (int)(baseScore * 0.5f);
                
            case JudgmentType.Bad:
                return (int)(baseScore * 0.3f);
                
            case JudgmentType.Miss:
                return 0;
                
            default:
                return 0;
        }
    }
    
    // 점수 업데이트
    private void UpdateScore(JudgmentResult result)
    {
        currentScore += result.score;
        UpdateScoreDisplay();
    }
    
    // 콤보 업데이트
    private void UpdateCombo(JudgmentType judgment)
    {
        if (judgment == JudgmentType.Miss)
        {
            currentCombo = 0;
        }
        else
        {
            currentCombo++;
            if (currentCombo > maxCombo)
                maxCombo = currentCombo;
        }
        UpdateComboDisplay();
    }
    
    // 판정 표시
    private void DisplayJudgment(JudgmentResult result)
    {
        string displayType = currentScoreType.ToString().Substring(5); // A, B, C, or D
        string judgmentString = "";
        Color judgmentColor = normalColor;
        
        switch (displayType)
        {
            case "A": // Type N-A: 기본 표시
                judgmentString = GetBasicJudgmentString(result.judgment);
                judgmentColor = normalColor;
                break;
                
            case "B": // Type N-B: Fast/Late 텍스트 추가
                judgmentString = GetJudgmentWithTimingText(result);
                judgmentColor = GetTimingTextColor(result);
                break;
                
            case "C": // Type N-C: 색상으로 Fast/Late 구분
                judgmentString = GetBasicJudgmentString(result.judgment);
                judgmentColor = GetTimingColor(result);
                break;
                
            case "D": // Type N-D: S_Perfect/Perfect 미표시, 나머지는 Fast/Late만
                judgmentString = GetSimplifiedJudgment(result);
                judgmentColor = GetTimingColor(result);
                break;
        }
        
        if (judgmentText != null && !string.IsNullOrEmpty(judgmentString))
        {
            judgmentText.text = judgmentString;
            judgmentText.color = judgmentColor;
            
            // 애니메이션 트리거 (옵션)
            StartCoroutine(FadeOutJudgment());
        }
    }
    
    // 기본 판정 문자열
    private string GetBasicJudgmentString(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return "S Perfect";
            case JudgmentType.Perfect: return "Perfect";
            case JudgmentType.Great: return "Great";
            case JudgmentType.Good: return "Good";
            case JudgmentType.Bad: return "Bad";
            case JudgmentType.Miss: return "Miss";
            default: return "";
        }
    }
    
    // Type N-B: Fast/Late 텍스트 포함
    private string GetJudgmentWithTimingText(JudgmentResult result)
    {
        if (result.judgment == JudgmentType.S_Perfect || result.judgment == JudgmentType.Perfect)
        {
            return GetBasicJudgmentString(result.judgment);
        }
        
        if (result.judgment == JudgmentType.Miss)
        {
            return "Miss";
        }
        
        string timingText = result.timing == TimingAccuracy.Fast ? "Fast " : 
                           result.timing == TimingAccuracy.Late ? "Late " : "";
        return timingText + GetBasicJudgmentString(result.judgment);
    }
    
    // Type N-B 색상
    private Color GetTimingTextColor(JudgmentResult result)
    {
        if (result.judgment == JudgmentType.S_Perfect || result.judgment == JudgmentType.Perfect || 
            result.judgment == JudgmentType.Miss)
        {
            return normalColor;
        }
        
        return result.timing == TimingAccuracy.Fast ? fastColor : 
               result.timing == TimingAccuracy.Late ? lateColor : normalColor;
    }
    
    // Type N-C, N-D 색상
    private Color GetTimingColor(JudgmentResult result)
    {
        if (result.timing == TimingAccuracy.Fast)
            return fastColor;
        else if (result.timing == TimingAccuracy.Late)
            return lateColor;
        else
            return normalColor;
    }
    
    // Type N-D: 단순화된 판정
    private string GetSimplifiedJudgment(JudgmentResult result)
    {
        if (result.judgment == JudgmentType.S_Perfect || result.judgment == JudgmentType.Perfect)
        {
            return ""; // Perfect 계열은 표시하지 않음
        }
        
        if (result.judgment == JudgmentType.Miss)
        {
            return "Miss";
        }
        
        // Great, Good, Bad는 Fast/Late로만 표시
        return result.timing == TimingAccuracy.Fast ? "Fast" : 
               result.timing == TimingAccuracy.Late ? "Late" : "";
    }
    
    // 판정 카운트 업데이트
    private void UpdateJudgmentCount(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: sPerfectCount++; break;
            case JudgmentType.Perfect: perfectCount++; break;
            case JudgmentType.Great: greatCount++; break;
            case JudgmentType.Good: goodCount++; break;
            case JudgmentType.Bad: badCount++; break;
            case JudgmentType.Miss: missCount++; break;
        }
    }
    
    // UI 업데이트
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString("N0");
    }
    
    private void UpdateComboDisplay()
    {
        if (comboText != null)
            comboText.text = currentCombo > 0 ? $"COMBO: {currentCombo}" : "";
    }
    
    // 판정 텍스트 페이드 아웃
    private System.Collections.IEnumerator FadeOutJudgment()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = judgmentText.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            judgmentText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }
    
    // 설정 변경 메서드
    public void SetScoreType(ScoreType type)
    {
        currentScoreType = type;
    }
    
    public void SetTotalNotes(int count)
    {
        totalNotes = count;
    }
    
    // 최종 결과 반환
    public GameResult GetGameResult()
    {
        return new GameResult
        {
            score = currentScore,
            maxCombo = maxCombo,
            sPerfectCount = sPerfectCount,
            perfectCount = perfectCount,
            greatCount = greatCount,
            goodCount = goodCount,
            badCount = badCount,
            missCount = missCount,
            accuracy = CalculateAccuracy()
        };
    }
    
    // 정확도 계산
    private float CalculateAccuracy()
    {
        int totalHits = sPerfectCount + perfectCount + greatCount + goodCount + badCount + missCount;
        if (totalHits == 0) return 0f;
        
        float weightedSum = sPerfectCount * 1.0f + perfectCount * 1.0f + 
                           greatCount * 0.8f + goodCount * 0.5f + 
                           badCount * 0.3f + missCount * 0f;
        
        return (weightedSum / totalHits) * 100f;
    }
}

// 게임 결과 클래스
[System.Serializable]
public class GameResult
{
    public int score;
    public int maxCombo;
    public int sPerfectCount;
    public int perfectCount;
    public int greatCount;
    public int goodCount;
    public int badCount;
    public int missCount;
    public float accuracy;
}