using UnityEngine;
using System.Collections;

/// <summary>
/// 히트 이펙트 개별 오브젝트
/// 판정에 따라 다른 비주얼/애니메이션 재생
/// </summary>
[RequireComponent(typeof(Animator))]
public class HitEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem particleSystem; // 선택사항
    
    [Header("Animation Settings")]
    [SerializeField] private float effectDuration = 0.6f; // 이펙트 지속 시간
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Visual Settings")]
    [SerializeField] private float maxScale = 1.5f;
    
    private HitEffectPool pool;
    private Coroutine effectCoroutine;
    
    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// 풀 참조 설정 (풀에서 호출)
    /// </summary>
    public void SetPool(HitEffectPool poolRef)
    {
        pool = poolRef;
    }
    
    /// <summary>
    /// 히트 이펙트 재생 (판정별)
    /// </summary>
    public void PlayAt(Vector3 position, JudgmentType judgment)
    {
        transform.position = position;
        
        // 판정별 색상 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.color = judgment.GetColor();
        }
        
        // 애니메이터가 있으면 애니메이션 재생
        if (animator != null)
        {
            string animName = GetAnimationName(judgment);
            if (!string.IsNullOrEmpty(animName))
            {
                animator.Play(animName);
            }
        }
        
        // 파티클 시스템 재생
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = judgment.GetColor();
            particleSystem.Play();
        }
        
        // 코루틴으로 이펙트 재생
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        effectCoroutine = StartCoroutine(EffectAnimation());
    }
    
    /// <summary>
    /// 미스 이펙트 재생
    /// </summary>
    public void PlayMiss(Vector3 position)
    {
        PlayAt(position, JudgmentType.Miss);
    }
    
    /// <summary>
    /// 이펙트 애니메이션 (스케일 + 페이드)
    /// </summary>
    private IEnumerator EffectAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / effectDuration);
            
            // 스케일 애니메이션
            float scaleT = scaleCurve.Evaluate(t);
            transform.localScale = Vector3.Lerp(startScale, endScale, scaleT);
            
            // 알파 페이드
            if (spriteRenderer != null)
            {
                float alphaT = alphaCurve.Evaluate(t);
                Color color = startColor;
                color.a = alphaT;
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        // 풀로 반환
        ReturnToPool();
    }
    
    /// <summary>
    /// 판정별 애니메이션 이름
    /// </summary>
    private string GetAnimationName(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return "SPerfectAnim";
            case JudgmentType.Perfect:   return "PerfectAnim";
            case JudgmentType.Great:     return "GreatAnim";
            case JudgmentType.Good:      return "GoodAnim";
            case JudgmentType.Bad:       return "BadAnim";
            case JudgmentType.Miss:      return "MissAnim";
            default: return "HitAnim"; // 기본 애니메이션
        }
    }
    
    /// <summary>
    /// 풀로 반환
    /// </summary>
    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            // 풀이 없으면 그냥 비활성화
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 강제 정지 및 반환
    /// </summary>
    public void ForceStop()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }
        
        ReturnToPool();
    }
}
