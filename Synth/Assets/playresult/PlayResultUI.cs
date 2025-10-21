using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// 플레이 결과 화면 UI 관리 스크립트
/// 곡 종료 후 플레이 결과를 표시하고 다음 행동을 선택할 수 있게 합니다.
/// </summary>
public class PlayResultUI : MonoBehaviour
{
    [Header("곡 정보 UI")]
    [Tooltip("곡 제목 텍스트")]
    public TextMeshProUGUI songTitleText;

    [Tooltip("아티스트 이름 텍스트")]
    public TextMeshProUGUI artistNameText;

    [Tooltip("난이도 텍스트")]
    public TextMeshProUGUI difficultyText;

    [Tooltip("키 개수 텍스트")]
    public TextMeshProUGUI keyCountText;

    [Header("결과 정보 UI")]
    [Tooltip("최종 점수 텍스트")]
    public TextMeshProUGUI scoreText;

    [Tooltip("정확도 텍스트")]
    public TextMeshProUGUI accuracyText;

    [Tooltip("최대 콤보 텍스트")]
    public TextMeshProUGUI maxComboText;

    [Tooltip("랭크 텍스트 (S, A, B, C, D, F)")]
    public TextMeshProUGUI rankText;

    [Header("판정 카운트 UI")]
    [Tooltip("S Perfect 카운트 텍스트")]
    public TextMeshProUGUI sPerfectCountText;

    [Tooltip("Perfect 카운트 텍스트")]
    public TextMeshProUGUI perfectCountText;

    [Tooltip("Great 카운트 텍스트")]
    public TextMeshProUGUI greatCountText;

    [Tooltip("Good 카운트 텍스트")]
    public TextMeshProUGUI goodCountText;

    [Tooltip("Bad 카운트 텍스트")]
    public TextMeshProUGUI badCountText;

    [Tooltip("Miss 카운트 텍스트")]
    public TextMeshProUGUI missCountText;

    [Header("특수 표시 UI")]
    [Tooltip("Full Combo 표시 오브젝트")]
    public GameObject fullComboIndicator;

    [Tooltip("Perfect Play 표시 오브젝트")]
    public GameObject perfectPlayIndicator;

    [Header("버튼")]
    [Tooltip("재시작 버튼")]
    public Button retryButton;

    [Tooltip("곡 선택 화면으로 돌아가기 버튼")]
    public Button backToSongSelectButton;

    [Tooltip("메인 메뉴로 돌아가기 버튼")]
    public Button backToMainMenuButton;

    [Header("씬 설정")]
    [Tooltip("현재 플레이한 곡의 씬 이름 (재시작용)")]
    public string currentGameSceneName = "GameScene";

    [Tooltip("곡 선택 씬 이름")]
    public string songSelectionSceneName = "SongSelectionScene";

    [Tooltip("메인 메뉴 씬 이름")]
    public string mainMenuSceneName = "MainMenuScene";

    [Header("애니메이션 설정")]
    [Tooltip("결과 표시 애니메이션 지속 시간")]
    public float animationDuration = 0.5f;

    [Tooltip("숫자 카운트 애니메이션 지속 시간")]
    public float countAnimationDuration = 1.5f;

    private PlayResultData resultData;

    void Start()
    {
        // 버튼 이벤트 등록
        RegisterButtonEvents();

        // 테스트용 데이터로 초기화 (실제로는 게임 씬에서 전달받아야 함)
        // LoadTestData();
    }

    /// <summary>
    /// 버튼 클릭 이벤트를 등록합니다.
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClicked);

        if (backToSongSelectButton != null)
            backToSongSelectButton.onClick.AddListener(OnBackToSongSelectButtonClicked);

        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuButtonClicked);
    }

    /// <summary>
    /// 플레이 결과 데이터를 설정하고 UI를 업데이트합니다.
    /// </summary>
    public void SetResultData(PlayResultData data)
    {
        resultData = data;
        StartCoroutine(DisplayResultsWithAnimation());
    }

    /// <summary>
    /// 플레이 결과를 애니메이션과 함께 표시합니다.
    /// </summary>
    private IEnumerator DisplayResultsWithAnimation()
    {
        // 초기 상태: 모든 UI 숨김
        HideAllUI();

        yield return new WaitForSeconds(0.5f);

        // 1. 곡 정보 표시
        UpdateSongInfo();
        yield return new WaitForSeconds(animationDuration);

        // 2. 랭크 표시
        UpdateRank();
        yield return new WaitForSeconds(animationDuration);

        // 3. 점수, 정확도, 콤보 애니메이션
        yield return StartCoroutine(AnimateScoreDisplay());

        // 4. 판정 카운트 표시
        UpdateJudgmentCounts();
        yield return new WaitForSeconds(animationDuration);

        // 5. 특수 표시 (Full Combo, Perfect Play)
        UpdateSpecialIndicators();
    }

    /// <summary>
    /// 모든 UI를 초기 상태로 숨깁니다.
    /// </summary>
    private void HideAllUI()
    {
        if (scoreText != null) scoreText.text = "";
        if (accuracyText != null) accuracyText.text = "";
        if (maxComboText != null) maxComboText.text = "";
        if (rankText != null) rankText.text = "";

        if (fullComboIndicator != null) fullComboIndicator.SetActive(false);
        if (perfectPlayIndicator != null) perfectPlayIndicator.SetActive(false);
    }

    /// <summary>
    /// 곡 정보를 업데이트합니다.
    /// </summary>
    private void UpdateSongInfo()
    {
        if (songTitleText != null)
            songTitleText.text = resultData.songTitle;

        if (artistNameText != null)
            artistNameText.text = resultData.artistName;

        if (difficultyText != null)
            difficultyText.text = resultData.difficulty;

        if (keyCountText != null)
            keyCountText.text = $"{resultData.keyCount}K";
    }

    /// <summary>
    /// 랭크를 업데이트합니다.
    /// </summary>
    private void UpdateRank()
    {
        if (rankText != null)
        {
            rankText.text = resultData.rank;
            rankText.color = resultData.GetRankColor();
        }
    }

    /// <summary>
    /// 점수, 정확도, 콤보를 애니메이션으로 표시합니다.
    /// </summary>
    private IEnumerator AnimateScoreDisplay()
    {
        float elapsed = 0f;
        int startScore = 0;
        int targetScore = resultData.score;
        float startAccuracy = 0f;
        float targetAccuracy = resultData.accuracy;
        int startCombo = 0;
        int targetCombo = resultData.maxCombo;

        while (elapsed < countAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countAnimationDuration);

            // Ease-out 효과
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            // 점수 애니메이션
            if (scoreText != null)
            {
                int currentScore = (int)Mathf.Lerp(startScore, targetScore, easedT);
                scoreText.text = currentScore.ToString("N0");
            }

            // 정확도 애니메이션
            if (accuracyText != null)
            {
                float currentAccuracy = Mathf.Lerp(startAccuracy, targetAccuracy, easedT);
                accuracyText.text = $"{currentAccuracy:F2}%";
            }

            // 콤보 애니메이션
            if (maxComboText != null)
            {
                int currentCombo = (int)Mathf.Lerp(startCombo, targetCombo, easedT);
                maxComboText.text = $"{currentCombo}";
            }

            yield return null;
        }

        // 최종 값 설정
        if (scoreText != null)
            scoreText.text = targetScore.ToString("N0");

        if (accuracyText != null)
            accuracyText.text = $"{targetAccuracy:F2}%";

        if (maxComboText != null)
            maxComboText.text = $"{targetCombo}";
    }

    /// <summary>
    /// 판정 카운트를 업데이트합니다.
    /// </summary>
    private void UpdateJudgmentCounts()
    {
        if (sPerfectCountText != null)
            sPerfectCountText.text = resultData.sPerfectCount.ToString();

        if (perfectCountText != null)
            perfectCountText.text = resultData.perfectCount.ToString();

        if (greatCountText != null)
            greatCountText.text = resultData.greatCount.ToString();

        if (goodCountText != null)
            goodCountText.text = resultData.goodCount.ToString();

        if (badCountText != null)
            badCountText.text = resultData.badCount.ToString();

        if (missCountText != null)
            missCountText.text = resultData.missCount.ToString();
    }

    /// <summary>
    /// 특수 표시 (Full Combo, Perfect Play)를 업데이트합니다.
    /// </summary>
    private void UpdateSpecialIndicators()
    {
        if (fullComboIndicator != null)
            fullComboIndicator.SetActive(resultData.isFullCombo);

        if (perfectPlayIndicator != null)
            perfectPlayIndicator.SetActive(resultData.isPerfectPlay);
    }

    /// <summary>
    /// 재시작 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnRetryButtonClicked()
    {
        Debug.Log("재시작 버튼 클릭 - 곡 다시 플레이");

        // 현재 씬 이름 가져오기 (저장된 씬 이름이 없으면 PlayerPrefs에서 가져오기)
        string sceneToLoad = currentGameSceneName;
        if (PlayerPrefs.HasKey("LastPlayedScene"))
        {
            sceneToLoad = PlayerPrefs.GetString("LastPlayedScene");
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("재시작할 씬 이름이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 곡 선택 화면으로 돌아가기 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnBackToSongSelectButtonClicked()
    {
        Debug.Log("곡 선택 화면으로 돌아가기");

        if (!string.IsNullOrEmpty(songSelectionSceneName))
        {
            SceneManager.LoadScene(songSelectionSceneName);
        }
        else
        {
            Debug.LogWarning("곡 선택 씬 이름이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnBackToMainMenuButtonClicked()
    {
        Debug.Log("메인 메뉴로 돌아가기");

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("메인 메뉴 씬 이름이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 테스트용 데이터를 로드합니다. (디버그 전용)
    /// </summary>
    public void LoadTestData()
    {
        PlayResultData testData = new PlayResultData
        {
            songTitle = "Test Song",
            artistName = "Test Artist",
            difficulty = "Hard",
            keyCount = 4,
            score = 950000,
            accuracy = 98.5f,
            maxCombo = 512,
            sPerfectCount = 450,
            perfectCount = 50,
            greatCount = 10,
            goodCount = 2,
            badCount = 0,
            missCount = 0
        };

        testData.CalculatePlayStats();
        SetResultData(testData);
    }

    /// <summary>
    /// 외부에서 GameResult를 사용해 결과를 설정합니다.
    /// </summary>
    public void SetResultFromGameResult(GameResult gameResult, string songTitle = "Unknown Song",
        string artistName = "Unknown Artist", string difficulty = "Normal", int keyCount = 4)
    {
        PlayResultData data = PlayResultData.FromGameResult(gameResult, songTitle, artistName, difficulty, keyCount);
        SetResultData(data);
    }

    void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryButtonClicked);

        if (backToSongSelectButton != null)
            backToSongSelectButton.onClick.RemoveListener(OnBackToSongSelectButtonClicked);

        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.RemoveListener(OnBackToMainMenuButtonClicked);
    }
}
