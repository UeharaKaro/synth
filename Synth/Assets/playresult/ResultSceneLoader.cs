using UnityEngine;

/// <summary>
/// 결과 화면 씬이 로드될 때 자동으로 GameResultManager에서 데이터를 가져와
/// PlayResultUI에 표시하는 헬퍼 스크립트입니다.
/// 결과 씬의 빈 GameObject에 이 스크립트를 추가하세요.
/// </summary>
public class ResultSceneLoader : MonoBehaviour
{
    [Header("UI 참조")]
    [Tooltip("PlayResultUI 컴포넌트 참조")]
    public PlayResultUI playResultUI;

    void Start()
    {
        LoadAndDisplayResult();
    }

    /// <summary>
    /// GameResultManager에서 결과 데이터를 가져와 UI에 표시합니다.
    /// </summary>
    private void LoadAndDisplayResult()
    {
        // PlayResultUI가 없으면 씬에서 찾기
        if (playResultUI == null)
        {
            playResultUI = FindObjectOfType<PlayResultUI>();
        }

        if (playResultUI == null)
        {
            Debug.LogError("PlayResultUI를 찾을 수 없습니다! 씬에 PlayResultUI 컴포넌트가 있는지 확인하세요.");
            return;
        }

        // GameResultManager에서 데이터 가져오기
        if (GameResultManager.Instance != null && GameResultManager.Instance.HasResultData())
        {
            PlayResultData resultData = GameResultManager.Instance.GetCurrentResultData();
            playResultUI.SetResultData(resultData);

            Debug.Log("게임 결과 데이터를 성공적으로 로드하여 표시했습니다.");
        }
        else
        {
            Debug.LogWarning("GameResultManager에 결과 데이터가 없습니다. 테스트 데이터를 로드합니다.");
            // 테스트 데이터 표시
            playResultUI.LoadTestData();
        }
    }
}
