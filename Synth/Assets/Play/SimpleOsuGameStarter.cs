using UnityEngine;
using System.IO;

/// <summary>
/// osu! 맵으로 게임을 간단하게 시작하는 예제 스크립트
/// GameManager와 ChartLoader가 Scene에 있어야 합니다.
/// </summary>
public class SimpleOsuGameStarter : MonoBehaviour
{
    [Header("osu 파일 경로 설정")]
    [Tooltip("테스트할 .osu 파일 경로")]
    [SerializeField] private string osuFilePath = "sample_mania.osu";

    [Tooltip("프로젝트 루트 기준 경로 사용")]
    [SerializeField] private bool useProjectRoot = true;

    [Tooltip("StreamingAssets 기준 경로 사용")]
    [SerializeField] private bool useStreamingAssets = false;

    [Header("자동 실행")]
    [SerializeField] private bool autoStartOnPlay = true;

    void Start()
    {
        if (autoStartOnPlay)
        {
            StartGameWithOsuFile();
        }
    }

    /// <summary>
    /// osu 파일로 게임 시작
    /// </summary>
    [ContextMenu("Start Game with osu File")]
    public void StartGameWithOsuFile()
    {
        // 전체 경로 생성
        string fullPath = GetFullPath(osuFilePath);

        Debug.Log($"=== osu! Game Start ===");
        Debug.Log($"파일 경로: {fullPath}");

        // 파일 존재 확인
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"❌ 파일이 존재하지 않습니다: {fullPath}");
            Debug.LogError("Inspector에서 osuFilePath를 확인하세요.");
            return;
        }

        // mania 모드 확인
        if (!OsuManiaParser.IsManiaMode(fullPath))
        {
            Debug.LogWarning($"⚠️ 이 파일은 osu!mania가 아닙니다!");
        }

        // ChartLoader 확인
        if (ChartLoader.Instance == null)
        {
            Debug.LogError("❌ ChartLoader가 Scene에 없습니다!");
            Debug.LogError("Scene에 ChartLoader 컴포넌트를 추가하세요.");
            return;
        }

        // 차트 로드
        Debug.Log("차트 로드 중...");
        ChartData chart = ChartLoader.Instance.LoadChartFromFile(fullPath);

        if (chart == null)
        {
            Debug.LogError("❌ 차트 로드 실패!");
            return;
        }

        // 차트 정보 출력
        Debug.Log($"✅ 차트 로드 성공!");
        Debug.Log($"   제목: {chart.songName}");
        Debug.Log($"   아티스트: {chart.artistName}");
        Debug.Log($"   난이도: {chart.difficulty} ({chart.keyCount}K)");
        Debug.Log($"   노트: {chart.noteCount}개");
        Debug.Log($"   BPM: {chart.bpm:F2}");

        // GameManager 확인
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameManager가 없습니다.");
            Debug.LogWarning("게임을 시작하려면 GameManager.Instance.StartGame(chart)를 호출하세요.");
            return;
        }

        // 게임 시작
        Debug.Log("게임 시작!");
        // GameManager.Instance.StartGame(chart); // ← GameManager에 public StartGame이 있으면 활성화

        Debug.Log($"===================\n");
    }

    /// <summary>
    /// 전체 파일 경로 반환
    /// </summary>
    private string GetFullPath(string path)
    {
        // 절대 경로면 그대로 반환
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        // StreamingAssets 기준
        if (useStreamingAssets)
        {
            return Path.Combine(Application.streamingAssetsPath, path);
        }

        // 프로젝트 루트 기준
        if (useProjectRoot)
        {
            string projectRoot = Path.GetFullPath(Application.dataPath + "/..");
            return Path.Combine(projectRoot, path);
        }

        // 상대 경로 그대로
        return path;
    }

    /// <summary>
    /// osu 파일 경로 설정 (스크립트에서 호출)
    /// </summary>
    public void SetOsuFilePath(string path)
    {
        osuFilePath = path;
    }
}
