using UnityEngine;

public class NoteController : MonoBehaviour
{
    [Header("Note Settings")]
    [SerializeField] private SpriteRenderer noteRenderer;
    [SerializeField] private Transform noteTransform;
    
    [Header("Note Data")]
    public float hitTime; // 언제 쳐야 하는 시간
    public int trackIndex; // 어느 트랙의 노트인지
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float speed;
    private bool isActive = true;
    private float creationTime;
    
    // 노트 상태
    public bool IsHit { get; private set; } = false;
    public bool IsMissed { get; private set; } = false;
    
    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (noteRenderer == null)
            noteRenderer = GetComponent<SpriteRenderer>();
        
        if (noteTransform == null)
            noteTransform = transform;
            
        creationTime = Time.time;
    }
    
    private void Start()
    {
        // 설정 변경 이벤트 구독
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged += ApplyNoteSettings;
            ApplyNoteSettings();
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged -= ApplyNoteSettings;
        }
    }
    
    public void Initialize(Vector3 startPos, Vector3 targetPos, float noteHitTime, int track)
    {
        startPosition = startPos;
        targetPosition = targetPos;
        hitTime = noteHitTime;
        trackIndex = track;
        
        transform.position = startPos;
        ApplyNoteSettings();
        
        Debug.Log($"Note initialized - Track: {trackIndex}, HitTime: {hitTime:F2}s");
    }
    
    private void ApplyNoteSettings()
    {
        if (SettingsManager.Instance == null) return;
        
        var settings = SettingsManager.Instance.Settings;
        
        // 노트 크기 적용
        ApplyNoteSize(settings.noteSize);
        
        // 스크롤 속도 적용
        speed = settings.noteScrollSpeed;
    }
    
    private void ApplyNoteSize(float size)
    {
        if (noteTransform != null)
        {
            noteTransform.localScale = Vector3.one * size;
        }
    }
    
    private void Update()
    {
        if (!isActive || IsHit || IsMissed) return;
        
        UpdateNoteMovement();
        CheckForMiss();
    }
    
    private void UpdateNoteMovement()
    {
        // 목표 지점으로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        // 목표 지점에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // 아직 치지 않았다면 Miss 처리
            if (!IsHit)
            {
                Miss();
            }
        }
    }
    
    private void CheckForMiss()
    {
        // 현재 시간이 히트 타임을 많이 지났다면 Miss
        float currentTime = GetCurrentGameTime();
        float missWindow = 0.3f; // 300ms Miss 윈도우
        
        if (currentTime > hitTime + missWindow)
        {
            Miss();
        }
    }
    
    private float GetCurrentGameTime()
    {
        // AudioManager에서 정확한 시간 가져오기
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            return audioManager.GetMusicTime();
        }
        
        // AudioManager가 없다면 Time.time 사용
        return Time.time - creationTime;
    }
    
    public JudgmentResult TryHit()
    {
        if (IsHit || IsMissed || !isActive) return JudgmentResult.Miss;
        
        float currentTime = GetCurrentGameTime();
        var settings = SettingsManager.Instance?.Settings;
        
        // 판정 오프셋 적용
        float adjustedCurrentTime = currentTime;
        if (settings != null)
        {
            adjustedCurrentTime += settings.judgmentOffset / 1000f;
        }
        
        float timeDifference = Mathf.Abs(adjustedCurrentTime - hitTime);
        JudgmentResult result = GetJudgmentResult(timeDifference);
        
        if (result != JudgmentResult.Miss)
        {
            Hit(result);
        }
        
        return result;
    }
    
    private JudgmentResult GetJudgmentResult(float timeDifference)
    {
        // 판정 기준 (초 단위)
        if (timeDifference <= 0.05f) return JudgmentResult.Perfect;
        if (timeDifference <= 0.1f) return JudgmentResult.Great;
        if (timeDifference <= 0.15f) return JudgmentResult.Good;
        if (timeDifference <= 0.2f) return JudgmentResult.Bad;
        return JudgmentResult.Miss;
    }
    
    private void Hit(JudgmentResult result)
    {
        IsHit = true;
        isActive = false;
        
        // 히트 이펙트 (색상 변경 등)
        if (noteRenderer != null)
        {
            Color hitColor = GetJudgmentColor(result);
            noteRenderer.color = hitColor;
        }
        
        // 노트 파괴 (약간의 지연 후)
        Destroy(gameObject, 0.1f);
        
        Debug.Log($"Note hit with {result} judgment!");
    }
    
    private void Miss()
    {
        IsMissed = true;
        isActive = false;
        
        // Miss 이펙트
        if (noteRenderer != null)
        {
            noteRenderer.color = Color.red;
        }
        
        // 노트 파괴
        Destroy(gameObject, 0.5f);
        
        Debug.Log("Note missed!");
    }
    
    private Color GetJudgmentColor(JudgmentResult result)
    {
        switch (result)
        {
            case JudgmentResult.Perfect: return Color.yellow;
            case JudgmentResult.Great: return Color.green;
            case JudgmentResult.Good: return Color.blue;
            case JudgmentResult.Bad: return Color.orange;
            default: return Color.red;
        }
    }
    
    // 강제로 노트 제거 (게임 리셋시 등)
    public void ForceDestroy()
    {
        isActive = false;
        Destroy(gameObject);
    }
    
    // 노트가 화면 밖으로 나갔는지 확인
    public bool IsOutOfBounds(float boundaryY = -15f)
    {
        return transform.position.y < boundaryY;
    }
}

public enum JudgmentResult
{
    Perfect,
    Great,
    Good,
    Bad,
    Miss
}