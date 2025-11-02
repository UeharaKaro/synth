using UnityEngine;

/// <summary>
/// osu! mania Import 기능 테스트 스크립트
/// Unity Scene에 추가하여 실행
/// </summary>
public class OsuImportTest : MonoBehaviour
{
    [Header("테스트 설정")]
    [Tooltip("테스트할 .osu 파일 경로 (프로젝트 루트 기준)")]
    public string osuFilePath = "sample_mania.osu";

    [Header("테스트 실행")]
    [Tooltip("Start()에서 자동으로 테스트 실행")]
    public bool runOnStart = true;

    [Header("결과 (읽기 전용)")]
    [SerializeField] private ChartData loadedChart;
    [SerializeField] private bool testPassed = false;

    void Start()
    {
        if (runOnStart)
        {
            RunTest();
        }
    }

    /// <summary>
    /// 테스트 실행 (Inspector 버튼에서도 호출 가능)
    /// </summary>
    [ContextMenu("Run Test")]
    public void RunTest()
    {
        Debug.Log("===========================================");
        Debug.Log("osu! mania Import Test Started");
        Debug.Log("===========================================\n");

        testPassed = false;
        loadedChart = null;

        // 1. 파일 경로 확인
        string fullPath = GetFullPath(osuFilePath);
        Debug.Log($"[1] 파일 경로 확인");
        Debug.Log($"    입력: {osuFilePath}");
        Debug.Log($"    전체 경로: {fullPath}");

        if (!System.IO.File.Exists(fullPath))
        {
            Debug.LogError($"    ❌ 파일이 존재하지 않습니다!");
            Debug.LogError($"    프로젝트 루트에 {osuFilePath} 파일을 배치하세요.");
            return;
        }
        Debug.Log($"    ✅ 파일 존재 확인\n");

        // 2. mania 모드 검증
        Debug.Log($"[2] osu!mania 모드 검증");
        bool isMania = OsuManiaParser.IsManiaMode(fullPath);
        Debug.Log($"    Mode: {(isMania ? "osu!mania (3)" : "다른 모드")}");

        if (!isMania)
        {
            Debug.LogWarning($"    ⚠️ 이 파일은 osu!mania가 아닙니다!");
            Debug.LogWarning($"    강제로 파싱을 시도합니다...\n");
        }
        else
        {
            Debug.Log($"    ✅ osu!mania 확인\n");
        }

        // 3. 파일 로드
        Debug.Log($"[3] 차트 로드");
        ChartData chart = ChartLoader.Instance.LoadChartFromFile(fullPath);

        if (chart == null)
        {
            Debug.LogError($"    ❌ 차트 로드 실패!");
            return;
        }
        Debug.Log($"    ✅ 로드 성공\n");

        // 4. 메타데이터 출력
        Debug.Log($"[4] 메타데이터");
        Debug.Log($"    제목: {chart.songName}");
        Debug.Log($"    아티스트: {chart.artistName}");
        Debug.Log($"    제작자: {chart.chartAuthor}");
        Debug.Log($"    난이도: {chart.difficulty}");
        Debug.Log($"    출처: {(string.IsNullOrEmpty(chart.source) ? "(없음)" : chart.source)}");
        Debug.Log($"    태그: {(string.IsNullOrEmpty(chart.tags) ? "(없음)" : chart.tags)}");
        Debug.Log($"    비트맵 ID: {(string.IsNullOrEmpty(chart.beatmapId) ? "(없음)" : chart.beatmapId)}\n");

        // 5. 난이도 정보
        Debug.Log($"[5] 난이도 정보");
        Debug.Log($"    키 개수: {chart.keyCount}K");
        Debug.Log($"    레벨: {chart.level:F1}");
        Debug.Log($"    BPM: {chart.bpm:F2}\n");

        // 6. 차트 통계
        Debug.Log($"[6] 차트 통계");
        Debug.Log($"    총 노트: {chart.noteCount}");
        Debug.Log($"    롱노트: {chart.longNoteCount}");
        Debug.Log($"    일반 노트: {chart.noteCount - chart.longNoteCount}");
        Debug.Log($"    최대 콤보: {chart.maxCombo}");
        Debug.Log($"    노트 밀도: {chart.density:F2} notes/sec");
        Debug.Log($"    차트 길이: {chart.GetChartDuration():F2}초\n");

        // 7. 노트 샘플 출력
        Debug.Log($"[7] 노트 샘플 (최대 10개)");
        int sampleCount = Mathf.Min(10, chart.notes.Count);
        for (int i = 0; i < sampleCount; i++)
        {
            var note = chart.notes[i];
            string noteType = note.isLongNote ? "롱노트" : "일반  ";
            string timing = note.isLongNote
                ? $"{note.timing,6:F3}s ~ {note.longNoteEndTiming,6:F3}s ({note.longNoteEndTiming - note.timing:F3}s)"
                : $"{note.timing,6:F3}s";

            Debug.Log($"    [{i,2}] {noteType} | Track {note.track} | {timing}");
        }

        if (chart.notes.Count > sampleCount)
        {
            Debug.Log($"    ... 외 {chart.notes.Count - sampleCount}개 노트\n");
        }
        else
        {
            Debug.Log("");
        }

        // 8. 트랙별 통계
        Debug.Log($"[8] 트랙별 노트 분포");
        for (int track = 0; track < chart.keyCount; track++)
        {
            int count = chart.GetNoteCountByTrack(track);
            float percentage = (count / (float)chart.noteCount) * 100f;
            string bar = new string('█', Mathf.RoundToInt(percentage / 5f));
            Debug.Log($"    Track {track}: {count,3}개 ({percentage,5:F1}%) {bar}");
        }
        Debug.Log("");

        // 9. 유효성 검사
        Debug.Log($"[9] 유효성 검사");
        bool isValid = chart.IsValid();
        Debug.Log($"    IsValid(): {(isValid ? "✅ 통과" : "❌ 실패")}");

        if (!isValid)
        {
            Debug.LogError($"    차트가 유효하지 않습니다!");
            return;
        }
        Debug.Log("");

        // 10. 테스트 완료
        loadedChart = chart;
        testPassed = true;

        Debug.Log("===========================================");
        Debug.Log("✅ osu! mania Import Test PASSED");
        Debug.Log("===========================================");
        Debug.Log($"Inspector에서 loadedChart를 확인할 수 있습니다.\n");
    }

    /// <summary>
    /// 전체 파일 경로 반환
    /// </summary>
    private string GetFullPath(string path)
    {
        // 절대 경로면 그대로 반환
        if (System.IO.Path.IsPathRooted(path))
        {
            return path;
        }

        // 상대 경로면 프로젝트 루트 기준
        string projectRoot = Application.dataPath + "/..";
        return System.IO.Path.GetFullPath(System.IO.Path.Combine(projectRoot, path));
    }

    /// <summary>
    /// 파일 브라우저로 .osu 파일 선택 (에디터 전용)
    /// </summary>
    [ContextMenu("Select .osu File")]
    public void SelectOsuFile()
    {
#if UNITY_EDITOR
        string selectedPath = UnityEditor.EditorUtility.OpenFilePanel(
            "Select osu! mania beatmap",
            "",
            "osu"
        );

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // 프로젝트 루트 기준 상대 경로로 변환
            string projectRoot = System.IO.Path.GetFullPath(Application.dataPath + "/..");
            if (selectedPath.StartsWith(projectRoot))
            {
                osuFilePath = selectedPath.Substring(projectRoot.Length + 1);
            }
            else
            {
                osuFilePath = selectedPath; // 절대 경로
            }

            Debug.Log($"선택된 파일: {osuFilePath}");
        }
#else
        Debug.LogWarning("파일 선택은 Unity 에디터에서만 가능합니다.");
#endif
    }

    /// <summary>
    /// 로드된 차트로 게임 시작 (예시)
    /// </summary>
    [ContextMenu("Start Game with Loaded Chart")]
    public void StartGameWithLoadedChart()
    {
        if (loadedChart == null)
        {
            Debug.LogError("먼저 테스트를 실행하여 차트를 로드하세요!");
            return;
        }

        Debug.Log($"게임 시작: {loadedChart.songName} - {loadedChart.difficulty}");
        // TODO: 실제 게임 시작 로직
        // 예: GameManager.Instance.StartGame(loadedChart);
    }
}
