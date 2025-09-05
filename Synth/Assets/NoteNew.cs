using UnityEngine;

namespace ChartSystem
{
    /// <summary>
    /// Self-contained Note class - completely independent
    /// Handles individual note behavior, movement, and judgment
    /// </summary>
    public class NoteNew : MonoBehaviour
    {
        [Header("Note Visual Settings")]
        public SpriteRenderer noteRenderer;
        public Color evenTrackColor = Color.white;
        public Color oddTrackColor = Color.cyan;
        
        [Header("Note Properties")]
        public KeySoundType keySoundType = KeySoundType.None;
        public int track = 0;
        public float timing = 0.0f;
        public bool isLongNote = false;
        public float longNoteEndTiming = 0.0f;
        
        [Header("Judgment Settings")]
        public JudgmentMode judgmentMode = JudgmentMode.Normal;
        
        [Header("Normal Mode Thresholds (ms)")]
        public float normalPerfectThreshold = 41.66f;
        public float normalGreatThreshold = 83.33f;
        public float normalGoodThreshold = 120f;
        public float normalBadThreshold = 150f;
        
        [Header("Hard Mode Thresholds (ms)")]
        public float hardSPerfectThreshold = 16.67f;
        public float hardPerfectThreshold = 32.25f;
        public float hardGreatThreshold = 62.49f;
        public float hardGoodThreshold = 88.33f;
        public float hardBadThreshold = 120f;
        
        [Header("Super Mode Thresholds (ms)")]
        public float superSPerfectThreshold = 4.17f;
        public float superPerfectThreshold = 12.50f;
        public float superGreatThreshold = 25.00f;
        public float superGoodThreshold = 62.49f;
        
        // Private variables
        private float moveSpeed = 5f;
        private float targetY = 0f;
        private bool initialized = false;
        private bool isHit = false;
        private float spawnTime;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        
        // Long note variables
        private bool isLongNoteHeld = false;
        private LineRenderer longNoteTrail;
        
        void Awake()
        {
            if (noteRenderer == null)
                noteRenderer = GetComponent<SpriteRenderer>();
                
            spawnTime = Time.time;
        }
        
        /// <summary>
        /// Initialize the note with the given parameters
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
            // Simple time implementation - can be replaced with more accurate timing
            return Time.time;
        }
        
        /// <summary>
        /// Handle note hit and return judgment result
        /// </summary>
        public JudgmentType OnNoteHit(float hitTime)
        {
            if (isHit) return JudgmentType.Miss;
            
            isHit = true;
            
            float timeDifference = Mathf.Abs(hitTime - timing) * 1000.0f; // Convert to ms
            JudgmentType judgment = CalculateJudgment(timeDifference);
            
            // Play key sound (simplified - just log for now)
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
        /// Handle long note release
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
            else return JudgmentType.Miss; // No Bad judgment in Super mode
        }
        
        void PlayKeySound(KeySoundType soundType)
        {
            // Simple implementation - just log the sound
            Debug.Log($"Playing key sound: {soundType}");
        }
        
        /// <summary>
        /// Check if note should be destroyed (missed)
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
            Debug.Log("Note missed!");
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
        
        // Utility methods
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
    /// Self-contained JudgmentMode enum
    /// </summary>
    public enum JudgmentMode
    {
        Normal,    // Normal difficulty - recommended for casual players
        Hard,      // Hard difficulty - recommended for experienced players
        Super      // Super difficulty - for expert players
    }

    /// <summary>
    /// Self-contained JudgmentType enum
    /// </summary>
    public enum JudgmentType
    {
        S_Perfect, // Highest accuracy judgment
        Perfect,   // High accuracy judgment
        Great,     // Good accuracy judgment
        Good,      // Acceptable accuracy judgment
        Bad,       // Poor accuracy judgment (breaks combo)
        Miss       // Complete miss
    }
}