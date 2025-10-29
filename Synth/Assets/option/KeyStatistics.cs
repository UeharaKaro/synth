using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 키 입력 통계 추적 시스템
/// 각 키별로 정확도, 사용 빈도, 평균 타이밍 등을 기록합니다.
/// </summary>
[System.Serializable]
public class KeyStatistics
{
    [System.Serializable]
    public class KeyStat
    {
        public KeyCode key;
        public int totalHits = 0;           // 총 입력 횟수
        public int perfectHits = 0;         // Perfect 판정
        public int greatHits = 0;           // Great 판정
        public int goodHits = 0;            // Good 판정
        public int badHits = 0;             // Bad 판정
        public int missHits = 0;            // Miss 판정
        public float totalTimingOffset = 0f; // 누적 타이밍 오프셋 (ms)
        public float lastUsedTime = 0f;     // 마지막 사용 시간

        public float GetAccuracy()
        {
            if (totalHits == 0) return 0f;
            return (float)(perfectHits + greatHits) / totalHits * 100f;
        }

        public float GetAverageOffset()
        {
            if (totalHits == 0) return 0f;
            return totalTimingOffset / totalHits;
        }

        public int GetScore()
        {
            return perfectHits * 100 + greatHits * 50 + goodHits * 20 + badHits * 5;
        }
    }

    [Header("Statistics Data")]
    public List<KeyStat> keyStats = new List<KeyStat>();

    /// <summary>
    /// 키 입력 기록
    /// </summary>
    public void RecordKeyHit(KeyCode key, JudgmentType judgment, float timingOffset)
    {
        KeyStat stat = GetOrCreateKeyStat(key);

        stat.totalHits++;
        stat.totalTimingOffset += timingOffset;
        stat.lastUsedTime = Time.time;

        switch (judgment)
        {
            case JudgmentType.SPerfect:
            case JudgmentType.Perfect:
                stat.perfectHits++;
                break;
            case JudgmentType.Great:
                stat.greatHits++;
                break;
            case JudgmentType.Good:
                stat.goodHits++;
                break;
            case JudgmentType.Bad:
                stat.badHits++;
                break;
            case JudgmentType.Miss:
                stat.missHits++;
                break;
        }
    }

    /// <summary>
    /// 키 통계 가져오기 또는 생성
    /// </summary>
    private KeyStat GetOrCreateKeyStat(KeyCode key)
    {
        KeyStat stat = keyStats.Find(s => s.key == key);
        if (stat == null)
        {
            stat = new KeyStat { key = key };
            keyStats.Add(stat);
        }
        return stat;
    }

    /// <summary>
    /// 특정 키의 통계 가져오기
    /// </summary>
    public KeyStat GetKeyStat(KeyCode key)
    {
        return keyStats.Find(s => s.key == key);
    }

    /// <summary>
    /// 모든 통계 초기화
    /// </summary>
    public void ResetAllStats()
    {
        keyStats.Clear();
    }

    /// <summary>
    /// 가장 많이 사용된 키 Top N
    /// </summary>
    public List<KeyStat> GetTopUsedKeys(int count)
    {
        List<KeyStat> sorted = new List<KeyStat>(keyStats);
        sorted.Sort((a, b) => b.totalHits.CompareTo(a.totalHits));
        return sorted.GetRange(0, Mathf.Min(count, sorted.Count));
    }

    /// <summary>
    /// 정확도가 가장 높은 키 Top N
    /// </summary>
    public List<KeyStat> GetTopAccurateKeys(int count)
    {
        List<KeyStat> sorted = new List<KeyStat>(keyStats);
        sorted.Sort((a, b) => b.GetAccuracy().CompareTo(a.GetAccuracy()));
        return sorted.GetRange(0, Mathf.Min(count, sorted.Count));
    }

    /// <summary>
    /// 정확도가 가장 낮은 키 Top N
    /// </summary>
    public List<KeyStat> GetWorstAccurateKeys(int count)
    {
        List<KeyStat> sorted = new List<KeyStat>(keyStats);
        sorted.Sort((a, b) => a.GetAccuracy().CompareTo(b.GetAccuracy()));

        // totalHits가 10 이상인 것만 (통계적 유의성)
        sorted.RemoveAll(s => s.totalHits < 10);

        return sorted.GetRange(0, Mathf.Min(count, sorted.Count));
    }

    /// <summary>
    /// JSON으로 직렬화
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    /// <summary>
    /// JSON에서 역직렬화
    /// </summary>
    public static KeyStatistics FromJson(string json)
    {
        return JsonUtility.FromJson<KeyStatistics>(json);
    }
}
