using UnityEngine;

public class NoteController : MonoBehaviour
{
    [Header("Note Settings")]
    [SerializeField] private SpriteRenderer noteRenderer;
    [SerializeField] private Transform noteTransform;

    [Header("Note Data")]
    public float hitTime; // 언제 쳐야 하는 시간
    public int trackIndex; // 어느 트랙의 노트인지
    public KeySoundType keySoundType = KeySoundType.None; // 이 노트의 키 사운드

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float speed;
    private bool isActive = true;
    private float creationTime;

    // 노트 상태
    public bool IsHit { get; private set; } = false;
    public bool IsMissed { get; private set; } = false;

    // 시스템 참조
    private RhythmManager rhythmManager;
    private HPSystem hpSystem;
    private GearController gearController;
    
    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (noteRenderer == null)
            noteRenderer = GetComponent<SpriteRenderer>();

        if (noteTransform == null)
            noteTransform = transform;

        creationTime = Time.time;

        // 시스템 참조 찾기
        rhythmManager = FindObjectOfType<RhythmManager>();
        hpSystem = HPSystem.Instance;
        gearController = FindObjectOfType<GearController>();

        // 필수 참조 검증
        if (rhythmManager == null)
        {
            Debug.LogError("NoteController: RhythmManager를 찾을 수 없습니다!");
        }

        if (hpSystem == null)
        {
            Debug.LogWarning("NoteController: HPSystem을 찾을 수 없습니다. HP 변경이 적용되지 않습니다.");
        }
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
    
    public void Initialize(Vector3 startPos, Vector3 targetPos, float noteHitTime, int track, KeySoundType keySound = KeySoundType.None)
    {
        startPosition = startPos;
        targetPosition = targetPos;
        hitTime = noteHitTime;
        trackIndex = track;
        keySoundType = keySound;
        
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
        // AudioManager에서 정확한 시간 가져오기 (싱글톤 사용)
        if (AudioManager.Instance != null && AudioManager.Instance.IsPlaying)
        {
            return AudioManager.Instance.GetMusicTime();
        }

        // AudioManager가 없거나 재생 중이 아니면 Time.time 사용
        return Time.time - creationTime;
    }
    
    public JudgmentType TryHit()
    {
        if (IsHit || IsMissed || !isActive) return JudgmentType.Miss;

        if (rhythmManager == null)
        {
            Debug.LogError("NoteController: RhythmManager가 없어 판정할 수 없습니다!");
            return JudgmentType.Miss;
        }

        float currentTime = GetCurrentGameTime();
        var settings = SettingsManager.Instance?.Settings;

        // 판정 오프셋 적용
        float adjustedCurrentTime = currentTime;
        if (settings != null)
        {
            adjustedCurrentTime += settings.judgmentOffset / 1000f;
        }

        // 시간 차이 계산 (밀리초 단위로 변환)
        float timeDifferenceMs = (adjustedCurrentTime - hitTime) * 1000f;

        // RhythmManager를 통한 판정
        JudgmentType result = rhythmManager.GetJudgment(timeDifferenceMs);

        if (result != JudgmentType.Miss)
        {
            Hit(result, timeDifferenceMs);
        }

        return result;
    }
    
    private void Hit(JudgmentType result, float timeDifferenceMs)
    {
        IsHit = true;
        isActive = false;

        // 키 사운드 재생
        if (keySoundType != KeySoundType.None && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayKeySound(keySoundType);
        }

        // 판정 효과음 재생 (Good 이하는 히트 사운드)
        if (AudioManager.Instance != null)
        {
            if (result == JudgmentType.S_Perfect || result == JudgmentType.Perfect || result == JudgmentType.Great)
            {
                AudioManager.Instance.PlaySFX(SFXType.Hit);
            }
        }

        // HP 시스템 업데이트
        if (hpSystem != null)
        {
            hpSystem.ProcessJudgment(result);
        }

        // GearController에 판정 표시 (콤보, 점수 업데이트)
        if (gearController != null)
        {
            gearController.ProcessJudgment(result);
            gearController.ShowJudgmentOffset(result, timeDifferenceMs);
        }

        // 🎯 UI 애니메이션 시스템: 노트 히트 이벤트 발생
        GameEvents.RaiseNoteHit(result, timeDifferenceMs);

        // 히트 이펙트 (색상 변경 등)
        if (noteRenderer != null)
        {
            Color hitColor = GetJudgmentColor(result);
            noteRenderer.color = hitColor;
        }

        // 노트 파괴 (약간의 지연 후)
        Destroy(gameObject, 0.1f);

        Debug.Log($"Note hit with {result} judgment! (Offset: {timeDifferenceMs:F2}ms)");
    }
    
    private void Miss()
    {
        IsMissed = true;
        isActive = false;

        // Miss 효과음 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.Miss);
        }

        // HP 시스템 업데이트 (Miss)
        if (hpSystem != null)
        {
            hpSystem.ProcessJudgment(JudgmentType.Miss);
        }

        // GearController에 Miss 처리
        if (gearController != null)
        {
            gearController.ProcessJudgment(JudgmentType.Miss);
        }

        // 🎯 UI 애니메이션 시스템: 노트 미스 이벤트 발생
        GameEvents.RaiseNoteMiss();

        // Miss 이펙트
        if (noteRenderer != null)
        {
            noteRenderer.color = Color.red;
        }

        // 노트 파괴
        Destroy(gameObject, 0.5f);

        Debug.Log("Note missed!");
    }

    private Color GetJudgmentColor(JudgmentType result)
    {
        switch (result)
        {
            case JudgmentType.S_Perfect: return new Color(1f, 0.84f, 0f); // 금색
            case JudgmentType.Perfect: return Color.yellow;
            case JudgmentType.Great: return Color.green;
            case JudgmentType.Good: return Color.cyan;
            case JudgmentType.Bad: return Color.magenta;
            case JudgmentType.Miss: return Color.red;
            default: return Color.white;
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