using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 결과를 관리하고 씬 간 데이터를 전달하는 매니저 클래스
/// 싱글톤 패턴으로 구현되어 씬 전환 시에도 데이터가 유지됩니다.
/// </summary>
public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance { get; private set; }

    // 현재 플레이 결과 데이터
    private PlayResultData currentResultData;

    // 현재 게임 정보
    private string currentSongTitle = "Unknown Song";
    private string currentArtistName = "Unknown Artist";
    private string currentDifficulty = "Normal";
    private int currentKeyCount = 4;
    private string currentGameSceneName = "GameScene";

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 현재 플레이 중인 곡 정보를 설정합니다.
    /// 게임 시작 시 호출하여 곡 정보를 저장합니다.
    /// </summary>
    public void SetCurrentSongInfo(string songTitle, string artistName, string difficulty, int keyCount)
    {
        currentSongTitle = songTitle;
        currentArtistName = artistName;
        currentDifficulty = difficulty;
        currentKeyCount = keyCount;

        // 현재 씬 이름 저장 (재시작용)
        currentGameSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastPlayedScene", currentGameSceneName);
        PlayerPrefs.Save();

        Debug.Log($"곡 정보 설정: {songTitle} - {artistName} [{difficulty}] {keyCount}K");
    }

    /// <summary>
    /// 현재 플레이 중인 곡 정보를 설정합니다. (별칭 메서드)
    /// SetCurrentSongInfo의 단축 버전입니다.
    /// </summary>
    public void SetCurrentSong(string songTitle, string artistName, string difficulty, int keyCount)
    {
        SetCurrentSongInfo(songTitle, artistName, difficulty, keyCount);
    }

    /// <summary>
    /// 게임 결과를 저장하고 결과 화면으로 전환합니다.
    /// </summary>
    public void SaveResultAndShowResultScreen(GameResult gameResult, string resultSceneName = "ResultScene")
    {
        // PlayResultData 생성
        currentResultData = PlayResultData.FromGameResult(
            gameResult,
            currentSongTitle,
            currentArtistName,
            currentDifficulty,
            currentKeyCount
        );

        Debug.Log($"게임 결과 저장 완료: Score={gameResult.score}, Accuracy={gameResult.accuracy:F2}%");

        // 결과 화면 씬으로 전환
        SceneManager.LoadScene(resultSceneName);
    }

    /// <summary>
    /// 저장된 결과 데이터를 가져옵니다.
    /// </summary>
    public PlayResultData GetCurrentResultData()
    {
        return currentResultData;
    }

    /// <summary>
    /// 결과 데이터가 존재하는지 확인합니다.
    /// </summary>
    public bool HasResultData()
    {
        return currentResultData != null;
    }

    /// <summary>
    /// 결과 데이터를 초기화합니다.
    /// </summary>
    public void ClearResultData()
    {
        currentResultData = null;
        Debug.Log("게임 결과 데이터 초기화");
    }

    /// <summary>
    /// 현재 곡 정보를 가져옵니다.
    /// </summary>
    public void GetCurrentSongInfo(out string songTitle, out string artistName, out string difficulty, out int keyCount)
    {
        songTitle = currentSongTitle;
        artistName = currentArtistName;
        difficulty = currentDifficulty;
        keyCount = currentKeyCount;
    }

    /// <summary>
    /// 현재 곡 정보를 구조체로 반환합니다.
    /// </summary>
    public (string songTitle, string artistName, string difficulty, int keyCount, string sceneName) GetCurrentSongInfo()
    {
        return (currentSongTitle, currentArtistName, currentDifficulty, currentKeyCount, currentGameSceneName);
    }

    /// <summary>
    /// 마지막으로 플레이한 씬 이름을 가져옵니다.
    /// </summary>
    public string GetLastPlayedSceneName()
    {
        return currentGameSceneName;
    }
}
