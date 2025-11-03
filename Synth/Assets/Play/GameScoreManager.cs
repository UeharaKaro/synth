using UnityEngine;

/// <summary>
/// 점수, 콤보, 퍼센트 계산을 담당하는 매니저
/// GameEvents를 통해 이벤트 기반으로 동작
/// </summary>
public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    [SerializeField] private int totalNotes = 1000; // 곡의 전체 노트 수 (차트 로드 시 업데이트)
    
    [Header("Combo Multiplier")]
    [SerializeField] private int comboThreshold1 = 50;  // 콤보 50 이상: x2
    [SerializeField] private int comboThreshold2 = 100; // 콤보 100 이상: x3
    [SerializeField] private int comboThreshold3 = 200; // 콤보 200 이상: x4
    
    // 현재 상태
    private long currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int hitNotes = 0;
    
    // 판정별 카운트 (통계용)
    private int[] judgmentCounts = new int[6]; // S_Perfect, Perfect, Great, Good, Bad, Miss
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnEnable()
    {
        // 이벤트 구독
        GameEvents.OnNoteHit += HandleNoteHit;
        GameEvents.OnNoteMiss += HandleMiss;
        GameEvents.OnSongStart += ResetScore;
    }
    
    private void OnDisable()
    {
        // 이벤트 구독 해제
        GameEvents.OnNoteHit -= HandleNoteHit;
        GameEvents.OnNoteMiss -= HandleMiss;
        GameEvents.OnSongStart -= ResetScore;
    }
    
    /// <summary>
    /// 노트 히트 처리
    /// </summary>
    private void HandleNoteHit(JudgmentType judgment, float timeDifferenceMs)
    {
        // Miss는 HandleMiss에서 처리
        if (judgment == JudgmentType.Miss)
        {
            HandleMiss();
            return;
        }
        
        // 판정 카운트 증가
        judgmentCounts[(int)judgment]++;
        
        // 기본 점수
        int basePoints = judgment.BasePoints();
        
        // 콤보 처리
        if (judgment.BreaksCombo())
        {
            // Bad 이상: 콤보 break
            currentCombo = 0;
        }
        else
        {
            // Good 이하: 콤보 증가
            currentCombo++;
            hitNotes++;
            
            // 최대 콤보 갱신
            if (currentCombo > maxCombo)
            {
                maxCombo = currentCombo;
            }
        }
        
        // 콤보 배율 적용
        int multiplier = GetComboMultiplier();
        int finalPoints = basePoints * multiplier;
        
        // 점수 증가
        currentScore += finalPoints;
        
        // UI 업데이트 (이벤트 발생)
        GameEvents.RaiseScoreChanged(currentScore);
        GameEvents.RaiseComboChanged(currentCombo);
        GameEvents.RaisePercentChanged(CalculatePercent());
        
        Debug.Log($"Hit: {judgment} | Score: +{finalPoints} (x{multiplier}) | Combo: {currentCombo} | Total: {currentScore}");
    }
    
    /// <summary>
    /// 미스 처리
    /// </summary>
    private void HandleMiss()
    {
        // 판정 카운트 증가
        judgmentCounts[(int)JudgmentType.Miss]++;
        
        // 콤보 리셋
        currentCombo = 0;
        
        // UI 업데이트
        GameEvents.RaiseComboChanged(0);
        
        Debug.Log("MISS! Combo reset.");
    }
    
    /// <summary>
    /// 콤보 배율 계산
    /// </summary>
    private int GetComboMultiplier()
    {
        if (currentCombo >= comboThreshold3) return 4;
        if (currentCombo >= comboThreshold2) return 3;
        if (currentCombo >= comboThreshold1) return 2;
        return 1;
    }
    
    /// <summary>
    /// 진행률 계산 (%)
    /// </summary>
    private float CalculatePercent()
    {
        if (totalNotes == 0) return 0f;
        return (float)hitNotes / (float)totalNotes * 100f;
    }
    
    /// <summary>
    /// 점수 초기화
    /// </summary>
    private void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        hitNotes = 0;
        
        // 판정 카운트 초기화
        for (int i = 0; i < judgmentCounts.Length; i++)
        {
            judgmentCounts[i] = 0;
        }
        
        // UI 초기화
        GameEvents.RaiseScoreChanged(0);
        GameEvents.RaiseComboChanged(0);
        GameEvents.RaisePercentChanged(0f);
        
        Debug.Log("Score Reset");
    }
    
    /// <summary>
    /// 전체 노트 수 설정 (차트 로드 시 호출)
    /// </summary>
    public void SetTotalNotes(int total)
    {
        totalNotes = total;
        Debug.Log($"Total notes set: {totalNotes}");
    }
    
    /// <summary>
    /// 현재 점수 가져오기
    /// </summary>
    public long GetCurrentScore() => currentScore;
    
    /// <summary>
    /// 현재 콤보 가져오기
    /// </summary>
    public int GetCurrentCombo() => currentCombo;
    
    /// <summary>
    /// 최대 콤보 가져오기
    /// </summary>
    public int GetMaxCombo() => maxCombo;
    
    /// <summary>
    /// 판정별 카운트 가져오기
    /// </summary>
    public int GetJudgmentCount(JudgmentType judgment)
    {
        return judgmentCounts[(int)judgment];
    }
    
    /// <summary>
    /// 정확도 계산 (%)
    /// </summary>
    public float CalculateAccuracy()
    {
        int totalHits = 0;
        float weightedScore = 0f;
        
        for (int i = 0; i < judgmentCounts.Length; i++)
        {
            totalHits += judgmentCounts[i];
            
            // 가중치: S_Perfect=1.0, Perfect=0.9, Great=0.7, Good=0.4, Bad=0.1, Miss=0
            float weight = 0f;
            switch (i)
            {
                case 0: weight = 1.0f; break; // S_Perfect
                case 1: weight = 0.9f; break; // Perfect
                case 2: weight = 0.7f; break; // Great
                case 3: weight = 0.4f; break; // Good
                case 4: weight = 0.1f; break; // Bad
                case 5: weight = 0f; break;   // Miss
            }
            
            weightedScore += judgmentCounts[i] * weight;
        }
        
        if (totalHits == 0) return 0f;
        return (weightedScore / totalHits) * 100f;
    }
}
