using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 프로필 UI 관리자
/// 플레이어 프로필 정보와 레이더 차트를 표시
/// </summary>
public class ProfileUIManager : MonoBehaviour
{
    [Header("UI 레퍼런스")]
    public Text playerNameText;
    public Text totalPlaysText;
    public Text totalClearsText;
    public Text totalFullCombosText;
    public Text totalAllPerfectsText;
    public PatternRadarChart radarChart;

    [Header("설정")]
    [Tooltip("레이더 차트 업데이트 주기 (초)")]
    public float updateInterval = 1f;

    private float lastUpdateTime;

    void Start()
    {
        UpdateProfileUI();
    }

    void Update()
    {
        // 주기적으로 UI 업데이트 (플레이 후 자동 반영)
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateProfileUI();
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 프로필 UI 업데이트
    /// </summary>
    public void UpdateProfileUI()
    {
        PlayerProfile profile = PlayerProfile.Instance;

        // 플레이어 정보
        if (playerNameText != null)
            playerNameText.text = profile.playerName;

        if (totalPlaysText != null)
            totalPlaysText.text = $"총 플레이: {profile.totalPlays}회";

        if (totalClearsText != null)
        {
            float clearRate = profile.totalPlays > 0 ? (float)profile.totalClears / profile.totalPlays * 100f : 0f;
            totalClearsText.text = $"클리어: {profile.totalClears}회 ({clearRate:F1}%)";
        }

        if (totalFullCombosText != null)
            totalFullCombosText.text = $"풀콤보: {profile.totalFullCombos}회";

        if (totalAllPerfectsText != null)
            totalAllPerfectsText.text = $"올퍼펙: {profile.totalAllPerfects}회";

        // 레이더 차트 업데이트
        if (radarChart != null)
        {
            radarChart.UpdateRadarChart();
        }
    }

    /// <summary>
    /// 플레이어 이름 변경
    /// </summary>
    public void ChangePlayerName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
            return;

        PlayerProfile.Instance.playerName = newName;
        PlayerProfile.Instance.Save();
        UpdateProfileUI();
    }

    /// <summary>
    /// 프로필 초기화 (주의!)
    /// </summary>
    public void ResetProfile()
    {
        PlayerProfile.Instance.Reset();
        UpdateProfileUI();
        Debug.Log("프로필이 초기화되었습니다.");
    }

    /// <summary>
    /// 레이더 차트 수동 새로고침
    /// </summary>
    public void RefreshRadarChart()
    {
        if (radarChart != null)
        {
            radarChart.UpdateRadarChart();
        }
    }
}
