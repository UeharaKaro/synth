using UnityEngine;

/// <summary>
/// 게임 결과 기록 헬퍼
/// 플레이가 끝나면 자동으로 PlayRecord를 생성하고 저장
/// </summary>
public class GameResultRecorder : MonoBehaviour
{
    /// <summary>
    /// 게임 결과를 기록합니다
    /// 게임 종료 시 호출하세요
    /// </summary>
    /// <param name="chart">플레이한 차트 데이터</param>
    /// <param name="score">획득 점수</param>
    /// <param name="accuracy">정확도 (%)</param>
    /// <param name="maxCombo">최대 콤보</param>
    /// <param name="perfect">Perfect 판정 수</param>
    /// <param name="great">Great 판정 수</param>
    /// <param name="good">Good 판정 수</param>
    /// <param name="bad">Bad 판정 수</param>
    /// <param name="miss">Miss 판정 수</param>
    /// <param name="isCleared">클리어 여부</param>
    public static void RecordGameResult(
        ChartSystem.ChartDataNew chart,
        int score,
        float accuracy,
        int maxCombo,
        int perfect,
        int great,
        int good,
        int bad,
        int miss,
        bool isCleared)
    {
        if (chart == null)
        {
            Debug.LogError("GameResultRecorder: chart가 null입니다.");
            return;
        }

        // PlayRecord 생성
        PlayRecord record = new PlayRecord
        {
            songName = chart.songName,
            difficulty = chart.difficulty,
            keyCount = chart.keyCount,
            beatmapId = chart.beatmapId,

            score = score,
            accuracy = accuracy,
            maxCombo = maxCombo,
            perfect = perfect,
            great = great,
            good = good,
            bad = bad,
            miss = miss,

            playDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            isCleared = isCleared,
            isFullCombo = (miss == 0 && bad == 0),
            isAllPerfect = (great == 0 && good == 0 && bad == 0 && miss == 0)
        };

        // 차트의 패턴 난이도 복사
        record.SetPatternDifficultyFromChart(chart.patternDifficulty);

        // PlayerProfile에 추가
        PlayerProfile.Instance.AddPlayRecord(record);

        Debug.Log($"게임 결과 기록 완료: {chart.songName} - {score}점 ({accuracy:F2}%)");
    }

    /// <summary>
    /// ChartData (Play용) 버전
    /// </summary>
    public static void RecordGameResult(
        ChartData chart,
        int score,
        float accuracy,
        int maxCombo,
        int perfect,
        int great,
        int good,
        int bad,
        int miss,
        bool isCleared)
    {
        if (chart == null)
        {
            Debug.LogError("GameResultRecorder: chart가 null입니다.");
            return;
        }

        // PlayRecord 생성
        PlayRecord record = new PlayRecord
        {
            songName = chart.songName,
            difficulty = chart.difficulty,
            keyCount = chart.keyCount,
            beatmapId = chart.beatmapId,

            score = score,
            accuracy = accuracy,
            maxCombo = maxCombo,
            perfect = perfect,
            great = great,
            good = good,
            bad = bad,
            miss = miss,

            playDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            isCleared = isCleared,
            isFullCombo = (miss == 0 && bad == 0),
            isAllPerfect = (great == 0 && good == 0 && bad == 0 && miss == 0)
        };

        // ChartData는 패턴 난이도를 직접 가지지 않으므로
        // 필요하다면 ChartDataNew로 변환하거나 별도로 설정
        // 여기서는 기본값 0으로 설정 (추후 개선 필요)
        Debug.LogWarning("GameResultRecorder: ChartData는 패턴 난이도 정보가 없습니다. 0으로 설정됩니다.");

        // PlayerProfile에 추가
        PlayerProfile.Instance.AddPlayRecord(record);

        Debug.Log($"게임 결과 기록 완료: {chart.songName} - {score}점 ({accuracy:F2}%)");
    }

    /// <summary>
    /// 테스트용 더미 데이터 생성
    /// </summary>
    [ContextMenu("Generate Test Records")]
    public void GenerateTestRecords()
    {
        for (int i = 0; i < 10; i++)
        {
            ChartSystem.ChartDataNew testChart = new ChartSystem.ChartDataNew
            {
                songName = $"Test Song {i + 1}",
                difficulty = "Hard",
                keyCount = 6,
                beatmapId = System.Guid.NewGuid().ToString(),
                patternDifficulty = new ChartSystem.PatternDifficulty
                {
                    trill = Random.Range(5, 20),
                    stairs = Random.Range(5, 20),
                    chord = Random.Range(5, 20),
                    denim = Random.Range(5, 20),
                    jacks = Random.Range(5, 20),
                    longNoteHybrid = Random.Range(5, 20),
                    burst = Random.Range(5, 20),
                    offbeat = Random.Range(5, 20)
                }
            };

            RecordGameResult(
                testChart,
                score: Random.Range(800000, 1000000),
                accuracy: Random.Range(90f, 100f),
                maxCombo: Random.Range(500, 1000),
                perfect: Random.Range(400, 900),
                great: Random.Range(50, 150),
                good: Random.Range(10, 50),
                bad: Random.Range(0, 10),
                miss: Random.Range(0, 5),
                isCleared: true
            );
        }

        Debug.Log("테스트 레코드 10개 생성 완료");
    }
}
