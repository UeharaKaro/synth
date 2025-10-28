using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// 플레이어 프로필
/// 플레이 기록, 통계, 레이더 차트 데이터 관리
/// </summary>
[System.Serializable]
public class PlayerProfile
{
    [Header("플레이어 정보")]
    public string playerName = "Player";
    public string playerId = ""; // 고유 ID

    [Header("플레이 기록")]
    public List<PlayRecord> playRecords = new List<PlayRecord>();

    [Header("통계")]
    public int totalPlays = 0;
    public int totalClears = 0;
    public int totalFullCombos = 0;
    public int totalAllPerfects = 0;

    // Singleton 패턴
    private static PlayerProfile instance;
    public static PlayerProfile Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Load();
                if (instance == null)
                {
                    instance = new PlayerProfile();
                    instance.playerId = System.Guid.NewGuid().ToString();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// 플레이 기록 추가
    /// </summary>
    public void AddPlayRecord(PlayRecord record)
    {
        if (record == null) return;

        playRecords.Add(record);
        totalPlays++;

        if (record.isCleared) totalClears++;
        if (record.isFullCombo) totalFullCombos++;
        if (record.isAllPerfect) totalAllPerfects++;

        // 자동 저장
        Save();

        Debug.Log($"PlayRecord 추가: {record.songName} - {record.difficulty} ({record.score}점)");
    }

    /// <summary>
    /// 특정 곡의 최고 기록 가져오기
    /// </summary>
    public PlayRecord GetBestRecord(string songName, string difficulty, int keyCount)
    {
        var records = playRecords.Where(r =>
            r.songName == songName &&
            r.difficulty == difficulty &&
            r.keyCount == keyCount
        ).OrderByDescending(r => r.score);

        return records.FirstOrDefault();
    }

    /// <summary>
    /// 레이더 차트 데이터 계산
    /// </summary>
    /// <param name="topCount">각 패턴별 상위 몇 곡을 포함할지 (기본: 50)</param>
    public PatternRadarData CalculateRadarData(int topCount = 50)
    {
        PatternRadarData radarData = new PatternRadarData();

        // 각 패턴별로 가장 높은 점수의 곡들을 선택
        radarData.trill = CalculatePatternScore("trill", topCount);
        radarData.stairs = CalculatePatternScore("stairs", topCount);
        radarData.chord = CalculatePatternScore("chord", topCount);
        radarData.denim = CalculatePatternScore("denim", topCount);
        radarData.jacks = CalculatePatternScore("jacks", topCount);
        radarData.longNoteHybrid = CalculatePatternScore("longNoteHybrid", topCount);
        radarData.burst = CalculatePatternScore("burst", topCount);
        radarData.offbeat = CalculatePatternScore("offbeat", topCount);

        return radarData;
    }

    /// <summary>
    /// 특정 패턴의 레이더 점수 계산
    /// </summary>
    private float CalculatePatternScore(string patternName, int topCount)
    {
        // 해당 패턴의 난이도가 높은 곡들만 필터링 (최소 5 이상)
        var validRecords = playRecords.Where(r => GetPatternValue(r, patternName) >= 5).ToList();

        if (validRecords.Count == 0)
            return 0f;

        // 패턴 난이도 * 정확도로 가중 점수 계산
        var scoredRecords = validRecords.Select(r => new
        {
            Record = r,
            WeightedScore = GetPatternValue(r, patternName) * r.GetRating()
        }).OrderByDescending(x => x.WeightedScore).Take(topCount);

        // 상위 N곡의 평균 계산
        float totalScore = 0f;
        int count = 0;

        foreach (var item in scoredRecords)
        {
            totalScore += item.WeightedScore;
            count++;
        }

        if (count == 0)
            return 0f;

        // 평균 점수 반환
        return totalScore / count;
    }

    /// <summary>
    /// PlayRecord에서 특정 패턴의 값 가져오기
    /// </summary>
    private float GetPatternValue(PlayRecord record, string patternName)
    {
        switch (patternName)
        {
            case "trill": return record.trill;
            case "stairs": return record.stairs;
            case "chord": return record.chord;
            case "denim": return record.denim;
            case "jacks": return record.jacks;
            case "longNoteHybrid": return record.longNoteHybrid;
            case "burst": return record.burst;
            case "offbeat": return record.offbeat;
            default: return 0f;
        }
    }

    /// <summary>
    /// 프로필 저장
    /// </summary>
    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(this, true);
            string path = GetSavePath();

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, json);
            Debug.Log($"PlayerProfile 저장 완료: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerProfile 저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 프로필 로드
    /// </summary>
    public static PlayerProfile Load()
    {
        try
        {
            string path = GetSavePath();

            if (!File.Exists(path))
            {
                Debug.Log("PlayerProfile 파일이 존재하지 않습니다. 새로 생성합니다.");
                return null;
            }

            string json = File.ReadAllText(path);
            PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(json);

            Debug.Log($"PlayerProfile 로드 완료: {profile.playerName} (플레이 {profile.totalPlays}회)");
            return profile;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerProfile 로드 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 저장 경로 반환
    /// </summary>
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "player_profile.json");
    }

    /// <summary>
    /// 프로필 초기화
    /// </summary>
    public void Reset()
    {
        playRecords.Clear();
        totalPlays = 0;
        totalClears = 0;
        totalFullCombos = 0;
        totalAllPerfects = 0;
        Save();

        Debug.Log("PlayerProfile 초기화 완료");
    }
}

/// <summary>
/// 레이더 차트 데이터
/// 각 패턴별 레이더 점수 (0~20 스케일)
/// </summary>
[System.Serializable]
public class PatternRadarData
{
    public float trill = 0f;
    public float stairs = 0f;
    public float chord = 0f;
    public float denim = 0f;
    public float jacks = 0f;
    public float longNoteHybrid = 0f;
    public float burst = 0f;
    public float offbeat = 0f;

    /// <summary>
    /// 배열로 변환 (시각화에 편리)
    /// </summary>
    public float[] ToArray()
    {
        return new float[] { trill, stairs, chord, denim, jacks, longNoteHybrid, burst, offbeat };
    }

    /// <summary>
    /// 패턴 이름 배열 반환
    /// </summary>
    public static string[] GetPatternNames()
    {
        return new string[] { "트릴", "계단", "동치", "데님", "따닥이", "롱잡", "폭타", "즈레" };
    }

    /// <summary>
    /// 패턴 개수 반환
    /// </summary>
    public static int GetPatternCount()
    {
        return 8;
    }
}
