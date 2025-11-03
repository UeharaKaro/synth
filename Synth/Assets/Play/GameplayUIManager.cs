using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 게임플레이 UI 통합 관리자
/// - 트랙 슬라이드 인 애니메이션
/// - 콤보/점수/퍼센트 표시 및 애니메이션
/// - 판정 이펙트 재생
/// </summary>
public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance { get; private set; }
    
    [Header("Track Animation")]
    [SerializeField] private RectTransform trackContainer; // 트랙이 들어올 RectTransform
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-2000, 0); // 화면 밖 (왼쪽)
    [SerializeField] private Vector2 shownPosition = Vector2.zero; // 보여질 위치
    [SerializeField] private float slideDuration = 0.8f; // 슬라이드 시간
    [SerializeField] private AnimationCurve slideEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private TextMeshProUGUI judgmentText; // 판정 표시 ("PERFECT!" 등)
    
    [Header("Combo Animation")]
    [SerializeField] private float comboPulseScale = 1.35f; // 콤보 증가 시 스케일
    [SerializeField] private float comboPulseDuration = 0.18f;
    [SerializeField] private AnimationCurve comboPulseCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Judgment Animation")]
    [SerializeField] private float judgmentDisplayDuration = 0.6f; // 판정 표시 시간
    [SerializeField] private float judgmentFadeOutDuration = 0.2f;
    [SerializeField] private Vector3 judgmentPunchScale = new Vector3(1.5f, 1.5f, 1f);
    
    [Header("Hit Effect Pool")]
    [SerializeField] private HitEffectPool hitEffectPool;
    [SerializeField] private Transform hitEffectSpawnPoint; // 이펙트 생성 위치
    
    private Coroutine comboPulseCoroutine;
    private Coroutine judgmentDisplayCoroutine;
    private bool isInitialized = false;
    
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
        GameEvents.OnSongStart += OnSongStart;
        GameEvents.OnNoteHit += OnNoteHit;
        GameEvents.OnNoteMiss += OnNoteMiss;
        GameEvents.OnComboChanged += UpdateCombo;
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnPercentChanged += UpdatePercent;
    }
    
    private void OnDisable()
    {
        // 이벤트 구독 해제
        GameEvents.OnSongStart -= OnSongStart;
        GameEvents.OnNoteHit -= OnNoteHit;
        GameEvents.OnNoteMiss -= OnNoteMiss;
        GameEvents.OnComboChanged -= UpdateCombo;
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnPercentChanged -= UpdatePercent;
    }
    
    private void Start()
    {
        InitializeUI();
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        if (isInitialized) return;
        
        // 트랙을 화면 밖으로 숨김
        if (trackContainer != null)
        {
            trackContainer.anchoredPosition = hiddenPosition;
        }
        
        // 텍스트 초기화
        if (comboText != null) comboText.gameObject.SetActive(false);
        if (scoreText != null) scoreText.text = "0";
        if (percentText != null) percentText.text = "0.0%";
        if (judgmentText != null) judgmentText.gameObject.SetActive(false);
        
        isInitialized = true;
        Debug.Log("GameplayUIManager initialized");
    }
    
    /// <summary>
    /// 게임 시작 시 호출 - 트랙 슬라이드 인
    /// </summary>
    private void OnSongStart()
    {
        if (trackContainer != null)
        {
            StartCoroutine(SlideTrackIn());
        }
    }
    
    /// <summary>
    /// 트랙 슬라이드 인 애니메이션
    /// </summary>
    private IEnumerator SlideTrackIn()
    {
        float elapsed = 0f;
        Vector2 startPos = hiddenPosition;
        Vector2 endPos = shownPosition;
        
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float easedT = slideEasing.Evaluate(t);
            
            trackContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            
            yield return null;
        }
        
        trackContainer.anchoredPosition = endPos;
        Debug.Log("Track slide-in complete");
    }
    
    /// <summary>
    /// 노트 히트 시 호출
    /// </summary>
    private void OnNoteHit(JudgmentType judgment, float timeDifferenceMs)
    {
        // 판정 텍스트 표시
        ShowJudgment(judgment);
        
        // 히트 이펙트 재생
        PlayHitEffect(judgment);
    }
    
    /// <summary>
    /// 미스 시 호출
    /// </summary>
    private void OnNoteMiss()
    {
        // 미스 판정 표시
        ShowJudgment(JudgmentType.Miss);
        
        // 미스 이펙트 재생
        PlayMissEffect();
    }
    
    /// <summary>
    /// 판정 텍스트 표시
    /// </summary>
    private void ShowJudgment(JudgmentType judgment)
    {
        if (judgmentText == null) return;
        
        // 기존 코루틴 정지
        if (judgmentDisplayCoroutine != null)
        {
            StopCoroutine(judgmentDisplayCoroutine);
        }
        
        judgmentDisplayCoroutine = StartCoroutine(JudgmentDisplayAnimation(judgment));
    }
    
    /// <summary>
    /// 판정 텍스트 애니메이션
    /// </summary>
    private IEnumerator JudgmentDisplayAnimation(JudgmentType judgment)
    {
        // 텍스트 설정
        judgmentText.text = judgment.ToDisplayString();
        judgmentText.color = judgment.GetColor();
        judgmentText.gameObject.SetActive(true);
        
        // Punch 애니메이션 (크게 → 작게)
        float punchDuration = 0.15f;
        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;
        
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            judgmentText.transform.localScale = Vector3.Lerp(judgmentPunchScale, originalScale, t);
            yield return null;
        }
        
        judgmentText.transform.localScale = originalScale;
        
        // 표시 시간 대기
        yield return new WaitForSeconds(judgmentDisplayDuration - punchDuration);
        
        // Fade Out
        Color startColor = judgmentText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        elapsed = 0f;
        
        while (elapsed < judgmentFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / judgmentFadeOutDuration;
            judgmentText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        judgmentText.gameObject.SetActive(false);
        judgmentText.color = startColor; // 원래 색상 복원
    }
    
    /// <summary>
    /// 콤보 업데이트
    /// </summary>
    private void UpdateCombo(int combo)
    {
        if (comboText == null) return;
        
        if (combo <= 0)
        {
            // 콤보 0 또는 리셋 시 숨김
            comboText.gameObject.SetActive(false);
            return;
        }
        
        // 콤보 텍스트 표시
        comboText.gameObject.SetActive(true);
        comboText.text = $"{combo}\nCOMBO";
        
        // 펄스 애니메이션
        if (comboPulseCoroutine != null)
        {
            StopCoroutine(comboPulseCoroutine);
        }
        comboPulseCoroutine = StartCoroutine(ComboPulseAnimation());
    }
    
    /// <summary>
    /// 콤보 펄스 애니메이션
    /// </summary>
    private IEnumerator ComboPulseAnimation()
    {
        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * comboPulseScale;
        
        while (elapsed < comboPulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / comboPulseDuration;
            float curveValue = comboPulseCurve.Evaluate(t);
            
            comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            
            yield return null;
        }
        
        comboText.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 점수 업데이트
    /// </summary>
    private void UpdateScore(long score)
    {
        if (scoreText == null) return;
        
        // 천 단위 구분 쉼표 추가
        scoreText.text = score.ToString("N0");
    }
    
    /// <summary>
    /// 퍼센트 업데이트
    /// </summary>
    private void UpdatePercent(float percent)
    {
        if (percentText == null) return;
        
        // 소수점 1자리
        percentText.text = percent.ToString("F1") + "%";
    }
    
    /// <summary>
    /// 히트 이펙트 재생
    /// </summary>
    private void PlayHitEffect(JudgmentType judgment)
    {
        if (hitEffectPool == null || hitEffectSpawnPoint == null) return;
        
        HitEffect effect = hitEffectPool.Get();
        if (effect != null)
        {
            effect.PlayAt(hitEffectSpawnPoint.position, judgment);
        }
    }
    
    /// <summary>
    /// 미스 이펙트 재생
    /// </summary>
    private void PlayMissEffect()
    {
        if (hitEffectPool == null || hitEffectSpawnPoint == null) return;
        
        HitEffect effect = hitEffectPool.Get();
        if (effect != null)
        {
            effect.PlayMiss(hitEffectSpawnPoint.position);
        }
    }
    
    /// <summary>
    /// 트랙 슬라이드 아웃 (게임 종료 시)
    /// </summary>
    public IEnumerator SlideTrackOut()
    {
        if (trackContainer == null) yield break;
        
        float elapsed = 0f;
        Vector2 startPos = trackContainer.anchoredPosition;
        Vector2 endPos = hiddenPosition;
        
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float easedT = slideEasing.Evaluate(t);
            
            trackContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            
            yield return null;
        }
        
        trackContainer.anchoredPosition = endPos;
    }
}
