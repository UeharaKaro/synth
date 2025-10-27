using UnityEngine;
using System;

/// <summary>
/// 개별 플레이 기록
/// 한 곡을 플레이한 결과를 저장
/// </summary>
[System.Serializable]
public class PlayRecord
{
    [Header("곡 정보")]
    public string songName;
    public string difficulty;
    public int keyCount;
    public string beatmapId; // 차트 고유 ID

    [Header("점수 정보")]
    public int score;
    public float accuracy; // 정확도 (%)
    public int maxCombo;
    public int perfect;
    public int great;
    public int good;
    public int bad;
    public int miss;

    [Header("패턴 난이도 (차트의 패턴 난이도)")]
    public float trill = 0f;
    public float stairs = 0f;
    public float chord = 0f;
    public float denim = 0f;
    public float jacks = 0f;
    public float longNoteHybrid = 0f;
    public float burst = 0f;
    public float offbeat = 0f;

    [Header("플레이 정보")]
    public string playDate; // YYYY-MM-DD HH:mm:ss
    public bool isCleared; // 클리어 여부
    public bool isFullCombo; // 풀콤보 여부
    public bool isAllPerfect; // 올퍼펙 여부

    /// <summary>
    /// 패턴 난이도를 ChartSystem.PatternDifficulty에서 복사
    /// </summary>
    public void SetPatternDifficultyFromChart(ChartSystem.PatternDifficulty pattern)
    {
        if (pattern == null) return;

        trill = pattern.trill;
        stairs = pattern.stairs;
        chord = pattern.chord;
        denim = pattern.denim;
        jacks = pattern.jacks;
        longNoteHybrid = pattern.longNoteHybrid;
        burst = pattern.burst;
        offbeat = pattern.offbeat;
    }

    /// <summary>
    /// ChartSystem.PatternDifficulty 객체 반환
    /// </summary>
    public ChartSystem.PatternDifficulty GetPatternDifficulty()
    {
        return new ChartSystem.PatternDifficulty
        {
            trill = this.trill,
            stairs = this.stairs,
            chord = this.chord,
            denim = this.denim,
            jacks = this.jacks,
            longNoteHybrid = this.longNoteHybrid,
            burst = this.burst,
            offbeat = this.offbeat
        };
    }

    /// <summary>
    /// 플레이 레이팅 계산 (패턴 난이도 * 정확도)
    /// 레이더 차트에서 가중치로 사용
    /// </summary>
    public float GetRating()
    {
        // 정확도를 0~1 범위로 정규화
        float normalizedAccuracy = Mathf.Clamp01(accuracy / 100f);

        // 클리어하지 못했으면 레이팅에 페널티
        if (!isCleared)
            normalizedAccuracy *= 0.5f;

        return normalizedAccuracy;
    }
}
