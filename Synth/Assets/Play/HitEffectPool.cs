using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 히트 이펙트 오브젝트 풀링 시스템
/// 성능 최적화를 위해 이펙트를 미리 생성하고 재사용
/// </summary>
public class HitEffectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private HitEffect effectPrefab; // 이펙트 프리팹
    [SerializeField] private int initialPoolSize = 20; // 초기 풀 크기
    [SerializeField] private bool allowGrowth = true; // 풀 부족 시 자동 확장 허용
    
    private Queue<HitEffect> pool = new Queue<HitEffect>();
    private List<HitEffect> activeEffects = new List<HitEffect>();
    
    private void Start()
    {
        InitializePool();
    }
    
    /// <summary>
    /// 풀 초기화
    /// </summary>
    private void InitializePool()
    {
        if (effectPrefab == null)
        {
            Debug.LogError("HitEffectPool: Effect prefab is not assigned!");
            return;
        }
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewEffect();
        }
        
        Debug.Log($"HitEffectPool initialized with {initialPoolSize} effects");
    }
    
    /// <summary>
    /// 새 이펙트 생성
    /// </summary>
    private HitEffect CreateNewEffect()
    {
        HitEffect effect = Instantiate(effectPrefab, transform);
        effect.gameObject.SetActive(false);
        effect.SetPool(this); // 풀 참조 설정
        pool.Enqueue(effect);
        return effect;
    }
    
    /// <summary>
    /// 풀에서 이펙트 가져오기
    /// </summary>
    public HitEffect Get()
    {
        HitEffect effect = null;
        
        // 풀에 사용 가능한 이펙트가 있으면 가져오기
        if (pool.Count > 0)
        {
            effect = pool.Dequeue();
        }
        else if (allowGrowth)
        {
            // 풀이 비었고 자동 확장이 허용되면 새로 생성
            Debug.LogWarning("HitEffectPool: Pool exhausted, creating new effect");
            effect = CreateNewEffect();
        }
        else
        {
            Debug.LogWarning("HitEffectPool: Pool exhausted and growth is disabled!");
            return null;
        }
        
        effect.gameObject.SetActive(true);
        activeEffects.Add(effect);
        
        return effect;
    }
    
    /// <summary>
    /// 풀로 이펙트 반환
    /// </summary>
    public void Return(HitEffect effect)
    {
        if (effect == null) return;
        
        effect.gameObject.SetActive(false);
        activeEffects.Remove(effect);
        pool.Enqueue(effect);
    }
    
    /// <summary>
    /// 모든 활성 이펙트 강제 반환
    /// </summary>
    public void ReturnAll()
    {
        // 리스트를 복사해서 순회 (Return() 내부에서 activeEffects 수정됨)
        var activeEffectsCopy = new List<HitEffect>(activeEffects);
        
        foreach (var effect in activeEffectsCopy)
        {
            Return(effect);
        }
    }
    
    /// <summary>
    /// 풀 상태 디버그 출력
    /// </summary>
    public void LogPoolStatus()
    {
        Debug.Log($"HitEffectPool Status - Available: {pool.Count}, Active: {activeEffects.Count}");
    }
}
