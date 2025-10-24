using UnityEngine;

/// <summary>
/// 플레이 결과 데이터를 저장하는 클래스
/// 곡 종료 후 결과 화면에 표시할 정보를 담습니다.
/// </summary>
[System.Serializable]
public class PlayResultData
{
    [Header("곡 정보")]
    public string songTitle = "Unknown Song";
    public string artistName = "Unknown Artist";
    public string difficulty = "Normal";
    public int keyCount = 4;

    [Header("게임 결과")]
    public int score = 0;
    public float accuracy = 0f;
    public int maxCombo = 0;

    [Header("판정 카운트")]
    public int sPerfectCount = 0;
    public int perfectCount = 0;
    public int greatCount = 0;
    public int goodCount = 0;
    public int badCount = 0;
    public int missCount = 0;

    [Header("추가 정보")]
    public bool isFullCombo = false;
    public bool isPerfectPlay = false;
    public string rank = "F";

    /// <summary>
    /// GameResult로부터 PlayResultData를 생성합니다.
    /// </summary>
    public static PlayResultData FromGameResult(GameResult gameResult, string songTitle = "Unknown Song",
        string artistName = "Unknown Artist", string difficulty = "Normal", int keyCount = 4)
    {
        PlayResultData data = new PlayResultData
        {
            songTitle = songTitle,
            artistName = artistName,
            difficulty = difficulty,
            keyCount = keyCount,
            score = gameResult.score,
            accuracy = gameResult.accuracy,
            maxCombo = gameResult.maxCombo,
            sPerfectCount = gameResult.sPerfectCount,
            perfectCount = gameResult.perfectCount,
            greatCount = gameResult.greatCount,
            goodCount = gameResult.goodCount,
            badCount = gameResult.badCount,
            missCount = gameResult.missCount
        };

        // 플레이 통계 계산
        data.CalculatePlayStats();

        return data;
    }

    /// <summary>
    /// 총 노트 수를 계산합니다.
    /// </summary>
    public int GetTotalNotes()
    {
        return sPerfectCount + perfectCount + greatCount + goodCount + badCount + missCount;
    }

    /// <summary>
    /// 플레이 통계를 계산합니다 (풀콤보, 퍼펙트 플레이, 랭크 등).
    /// </summary>
    public void CalculatePlayStats()
    {
        // 풀 콤보 체크
        isFullCombo = (missCount == 0 && badCount == 0);

        // 퍼펙트 플레이 체크 (S Perfect + Perfect만 존재)
        isPerfectPlay = (greatCount == 0 && goodCount == 0 && badCount == 0 && missCount == 0);

        // 랭크 계산
        rank = CalculateRank();
    }

    /// <summary>
    /// 정확도를 기반으로 랭크를 계산합니다.
    /// </summary>
    private string CalculateRank()
    {
        // 퍼펙트 플레이
        if (isPerfectPlay && sPerfectCount > perfectCount)
            return "SSS";

        if (isPerfectPlay)
            return "SS";

        // 정확도 기반 랭크
        if (accuracy >= 99.0f)
            return "S";
        else if (accuracy >= 95.0f)
            return "A";
        else if (accuracy >= 90.0f)
            return "B";
        else if (accuracy >= 80.0f)
            return "C";
        else if (accuracy >= 70.0f)
            return "D";
        else
            return "F";
    }

    /// <summary>
    /// 랭크에 따른 색상을 반환합니다.
    /// </summary>
    public Color GetRankColor()
    {
        switch (rank)
        {
            case "SSS":
                return new Color(1f, 0.84f, 0f); // 금색
            case "SS":
                return new Color(0.9f, 0.9f, 0.9f); // 은색
            case "S":
                return new Color(1f, 0.5f, 0f); // 주황색
            case "A":
                return new Color(0.2f, 0.8f, 1f); // 하늘색
            case "B":
                return new Color(0.3f, 1f, 0.3f); // 초록색
            case "C":
                return new Color(1f, 1f, 0.3f); // 노란색
            case "D":
                return new Color(1f, 0.5f, 0.5f); // 연한 빨강
            default:
                return new Color(0.5f, 0.5f, 0.5f); // 회색
        }
    }
}
