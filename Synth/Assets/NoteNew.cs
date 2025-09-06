using UnityEngine;

namespace ChartSystem
{
    /// <summary>
    /// 독립적인 노트 클래스 - 완전히 자율적
    /// 개별 노트의 동작, 움직임, 판정을 처리
    /// </summary>
    public class NoteNew : MonoBehaviour
    {
        [Header("노트 시각 설정")]
        public SpriteRenderer noteRenderer;
        public Color evenTrackColor = Color.white;
        public Color oddTrackColor = Color.cyan;
        
        [Header("노트 속성")]
        public KeySoundType keySoundType = KeySoundType.None;
        public int track = 0;
        public float timing = 0.0f;
        public bool isLongNote = false;
        public float longNoteEndTiming = 0.0f;
        
        [Header("판정 설정")]
        public JudgmentMode judgmentMode = JudgmentMode.Normal;
        
        [Header("일반 모드 임계값 (ms)")]
        public float normalPerfectThreshold = 41.66f;
        public float normalGreatThreshold = 83.33f;
        public float normalGoodThreshold = 120f;
        public float normalBadThreshold = 150f;
        
        [Header("하드 모드 임계값 (ms)")]
        public float hardSPerfectThreshold = 16.67f;
        public float hardPerfectThreshold = 32.25f;
        public float hardGreatThreshold = 62.49f;
        public float hardGoodThreshold = 88.33f;
        public float hardBadThreshold = 120f;
        
        [Header("슈퍼 모드 임계값 (ms)")]
        public float superSPerfectThreshold = 4.17f;
        public float superPerfectThreshold = 12.50f;
        public float superGreatThreshold = 25.00f;
        public float superGoodThreshold = 62.49f;
        
        // 개인 변수들
        private float moveSpeed = 5f;
        private float targetY = 0f;
        private bool initialized = false;
        private bool isHit = false;
        private float spawnTime;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        
        // 롱노트 변수들
        private bool isLongNoteHeld = false;
        private LineRenderer longNoteTrail;
        
        void Awake()
        {
            if (noteRenderer == null)
                noteRenderer = GetComponent<SpriteRenderer>();
                
            spawnTime = Time.time;
        }
        
        /// <summary>
        /// 주어진 매개변수로 노트를 초기화
        /// </summary>
        public void Initialize(float speed, float target, NoteData noteData, float currentTime)
        {
            moveSpeed = speed;
            targetY = target;
            keySoundType = noteData.keySoundType;
            track = noteData.track;
            timing = noteData.timing;
            isLongNote = noteData.isLongNote;
            longNoteEndTiming = noteData.longNoteEndTiming;
            spawnTime = currentTime;
            initialized = true;
            isHit = false;
            isLongNoteHeld = false;
            
            startPosition = transform.position;
            targetPosition = new Vector3(startPosition.x, targetY, startPosition.z);
            
            SetNoteAppearance();
            
            if (isLongNote)
            {
                SetupLongNoteVisual();
            }
        }
        
        void SetNoteAppearance()
        {
            if (noteRenderer != null)
            {
                Color baseColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
                noteRenderer.color = baseColor;
                
                if (isLongNote)
                {
                    Color longColor = baseColor;
                    longColor.a = 0.8f;
                    noteRenderer.color = longColor;
                }
            }
        }
        
        void SetupLongNoteVisual()
        {
            longNoteTrail = GetComponent<LineRenderer>();
            if (longNoteTrail == null)
            {
                longNoteTrail = gameObject.AddComponent<LineRenderer>();
            }
            
            longNoteTrail.material = new Material(Shader.Find("Sprites/Default"));
            
            Color trailColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
            longNoteTrail.startColor = trailColor;
            longNoteTrail.endColor = trailColor;
            longNoteTrail.startWidth = 0.1f;
            longNoteTrail.endWidth = 0.1f;
            longNoteTrail.positionCount = 2;
        }
        
        void Update()
        {
            if (!initialized) return;
            
            UpdateNoteMovement();
            
            if (isLongNote && longNoteTrail != null)
            {
                UpdateLongNoteTrail();
            }
        }
        
        void UpdateNoteMovement()
        {
            float currentTime = GetCurrentTime();
            float timeUntilHit = timing - currentTime;
            
            float totalTime = timing - spawnTime;
            float progress = 1.0f - (timeUntilHit / totalTime);
            progress = Mathf.Max(0.0f, Mathf.Min(1.0f, progress));
            
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            transform.position = currentPos;
        }
        
        void UpdateLongNoteTrail()
        {
            if (isLongNoteHeld)
            {
                longNoteTrail.SetPosition(0, startPosition);
                longNoteTrail.SetPosition(1, new Vector3(transform.position.x, targetY, transform.position.z));
            }
            else
            {
                longNoteTrail.positionCount = 0;
            }
        }
        
        float GetCurrentTime()
        {
            // 간단한 시간 구현 - 더 정확한 타이밍으로 대체 가능
            return Time.time;
        }
        
        /// <summary>
        /// 노트 히트를 처리하고 판정 결과 반환
        /// </summary>
        public JudgmentType OnNoteHit(float hitTime)
        {
            if (isHit) return JudgmentType.Miss;
            
            isHit = true;
            
            float timeDifference = Mathf.Abs(hitTime - timing) * 1000.0f; // ms로 변환
            JudgmentType judgment = CalculateJudgment(timeDifference);
            
            // 키 사운드 재생 (간단히 구현 - 일단 로그만)
            if (keySoundType != KeySoundType.None)
            {
                PlayKeySound(keySoundType);
            }
            
            if (isLongNote)
            {
                isLongNoteHeld = true;
                if (longNoteTrail != null)
                {
                    longNoteTrail.positionCount = 2;
                }
                return judgment;
            }
            
            gameObject.SetActive(false);
            return judgment;
        }
        
        /// <summary>
        /// 롱노트 릴리스 처리
        /// </summary>
        public JudgmentType OnLongNoteRelease(float releaseTime)
        {
            if (!isLongNote || !isLongNoteHeld) return JudgmentType.Miss;
            
            isLongNoteHeld = false;
            
            float timeDifference = Mathf.Abs(releaseTime - longNoteEndTiming) * 1000.0f;
            JudgmentType judgment = CalculateJudgment(timeDifference);
            
            if (keySoundType != KeySoundType.None)
            {
                PlayKeySound(keySoundType);
            }
            
            if (longNoteTrail != null)
            {
                longNoteTrail.positionCount = 0;
            }
            
            gameObject.SetActive(false);
            return judgment;
        }
        
        JudgmentType CalculateJudgment(float timeDifferenceMs)
        {
            switch (judgmentMode)
            {
                case JudgmentMode.Normal:
                    return CalculateNormalJudgment(timeDifferenceMs);
                case JudgmentMode.Hard:
                    return CalculateHardJudgment(timeDifferenceMs);
                case JudgmentMode.Super:
                    return CalculateSuperJudgment(timeDifferenceMs);
                default:
                    return CalculateNormalJudgment(timeDifferenceMs);
            }
        }
        
        JudgmentType CalculateNormalJudgment(float timeDifferenceMs)
        {
            if (timeDifferenceMs <= normalPerfectThreshold) return JudgmentType.Perfect;
            else if (timeDifferenceMs <= normalGreatThreshold) return JudgmentType.Great;
            else if (timeDifferenceMs <= normalGoodThreshold) return JudgmentType.Good;
            else if (timeDifferenceMs <= normalBadThreshold) return JudgmentType.Bad;
            else return JudgmentType.Miss;
        }
        
        JudgmentType CalculateHardJudgment(float timeDifferenceMs)
        {
            if (timeDifferenceMs <= hardSPerfectThreshold) return JudgmentType.S_Perfect;
            else if (timeDifferenceMs <= hardPerfectThreshold) return JudgmentType.Perfect;
            else if (timeDifferenceMs <= hardGreatThreshold) return JudgmentType.Great;
            else if (timeDifferenceMs <= hardGoodThreshold) return JudgmentType.Good;
            else if (timeDifferenceMs <= hardBadThreshold) return JudgmentType.Bad;
            else return JudgmentType.Miss;
        }
        
        JudgmentType CalculateSuperJudgment(float timeDifferenceMs)
        {
            if (timeDifferenceMs <= superSPerfectThreshold) return JudgmentType.S_Perfect;
            else if (timeDifferenceMs <= superPerfectThreshold) return JudgmentType.Perfect;
            else if (timeDifferenceMs <= superGreatThreshold) return JudgmentType.Great;
            else if (timeDifferenceMs <= superGoodThreshold) return JudgmentType.Good;
            else return JudgmentType.Miss; // 슈퍼 모드에서는 Bad 판정 없음
        }
        
        void PlayKeySound(KeySoundType soundType)
        {
            // 간단한 구현 - 사운드만 로그
            Debug.Log($"키 사운드 재생: {soundType}");
        }
        
        /// <summary>
        /// 노트가 제거되어야 하는지 확인 (놓침)
        /// </summary>
        public bool ShouldDestroy()
        {
            float currentTime = GetCurrentTime();
            float maxThreshold = GetMaxThresholdForCurrentMode();
            
            if (currentTime > timing + (maxThreshold / 1000.0f) && !isHit)
            {
                OnNoteMiss();
                return true;
            }
            
            return transform.position.y < targetY - 3f;
        }
        
        public void OnNoteMiss()
        {
            if (isHit) return;
            Debug.Log("노트 놓침!");
            gameObject.SetActive(false);
        }
        
        float GetMaxThresholdForCurrentMode()
        {
            switch (judgmentMode)
            {
                case JudgmentMode.Normal:
                    return normalBadThreshold;
                case JudgmentMode.Hard:
                    return hardBadThreshold;
                case JudgmentMode.Super:
                    return superGoodThreshold;
                default:
                    return normalBadThreshold;
            }
        }
        
        // 유틸리티 메서드들
        public bool IsInJudgmentRange()
        {
            float currentTime = GetCurrentTime();
            float timeDifference = Mathf.Abs(currentTime - timing);
            float maxThreshold = GetMaxThresholdForCurrentMode();
            return timeDifference <= (maxThreshold / 1000.0f);
        }
        
        public bool IsLongNoteHeld()
        {
            return isLongNote && isLongNoteHeld;
        }
        
        public void SetJudgmentMode(JudgmentMode mode)
        {
            judgmentMode = mode;
        }
    }

    /// <summary>
    /// 독립적인 판정 모드 열거형
    /// </summary>
    public enum JudgmentMode
    {
        Normal,    // 일반 난이도 - 캐주얼 플레이어용 권장
        Hard,      // 하드 난이도 - 숙련된 플레이어용 권장
        Super      // 슈퍼 난이도 - 전문가용
    }

    /// <summary>
    /// 독립적인 판정 타입 열거형
    /// </summary>
    public enum JudgmentType
    {
        S_Perfect, // 최고 정확도 판정
        Perfect,   // 높은 정확도 판정
        Great,     // 좋은 정확도 판정
        Good,      // 허용 가능한 정확도 판정
        Bad,       // 낮은 정확도 판정 (콤보 끊김)
        Miss       // 완전한 놓침
    }
}