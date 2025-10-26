using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 개별 곡의 정보를 저장하는 클래ス
/// </summary>
[System.Serializable]
public class SongData
{
    [Header("기본 정보")]
    [Tooltip("곡 고유 ID")]
    public string songId = "";

    [Tooltip("곡 제목")]
    public string title = "Unknown Song";

    [Tooltip("아티스트 이름")]
    public string artist = "Unknown Artist";

    [Header("음악 파일")]
    [Tooltip("음악 파일 경로 또는 리소스 경로")]
    public string audioPath = "";

    [Tooltip("오디오 파일 이름")]
    public string audioFileName = "";

    [Tooltip("미리듣기 시작 시간 (초)")]
    public float previewStartTime = 30f;

    [Header("비주얼")]
    [Tooltip("앨범 아트 스프라이트")]
    public Sprite albumArt;

    [Tooltip("배경 이미지 스프라이트")]
    public Sprite backgroundImage;

    [Header("곡 정보")]
    [Tooltip("BPM (Beats Per Minute)")]
    public float bpm = 120f;

    [Tooltip("곡 길이 (초)")]
    public float songLength = 180f;

    [Header("난이도 정보")]
    [Tooltip("사용 가능한 난이도 목록")]
    public List<DifficultyInfo> difficulties = new List<DifficultyInfo>();

    [Header("지원하는 키 모드")]
    [Tooltip("이 곡이 지원하는 키 개수 목록 (예: 4, 6, 8)")]
    public List<int> supportedKeyCounts = new List<int> { 4 };

    [Header("추가 정보")]
    [Tooltip("곡 설명 또는 코멘트")]
    public string description = "";

    [Tooltip("장르")]
    public string genre = "";

    [Tooltip("잠금 여부")]
    public bool isLocked = false;

    [Tooltip("잠금 해제 조건 설명")]
    public string unlockCondition = "";

    /// <summary>
    /// 특정 난이도 이름으로 난이도 정보를 가져옵니다.
    /// </summary>
    public DifficultyInfo GetDifficulty(string difficultyName)
    {
        return difficulties.Find(d => d.difficultyName == difficultyName);
    }

    /// <summary>
    /// 특정 키 개수를 지원하는지 확인합니다.
    /// </summary>
    public bool SupportsKeyCount(int keyCount)
    {
        return supportedKeyCounts.Contains(keyCount);
    }

    /// <summary>
    /// 특정 난이도와 키 개수에 대한 차트 경로를 가져옵니다.
    /// </summary>
    public string GetChartPath(string difficultyName, int keyCount)
    {
        DifficultyInfo diff = GetDifficulty(difficultyName);
        if (diff != null)
        {
            return diff.GetChartPath(keyCount);
        }
        return "";
    }
}

/// <summary>
/// 난이도별 정보를 저장하는 클래스
/// </summary>
[System.Serializable]
public class DifficultyInfo
{
    [Tooltip("난이도 이름 (Easy, Normal, Hard, Expert, Master, Special 등)")]
    public string difficultyName = "Normal";

    [Tooltip("난이도 레벨 (1~10 등)")]
    public int level = 1;

    [Tooltip("키 개수 (4, 5, 6, 7, 8, 10)")]
    public int keyCount = 4;

    [Tooltip("총 노트 수")]
    public int totalNotes = 0;

    [Tooltip("난이도 색상")]
    public Color difficultyColor = Color.white;

    [Tooltip("차트 파일 경로")]
    public string chartFileName = "";

    [Tooltip("키 개수별 차트 파일 경로 (레거시)")]
    public List<ChartPathInfo> chartPaths = new List<ChartPathInfo>();

    /// <summary>
    /// 난이도 색상 (호환성을 위한 프로퍼티)
    /// </summary>
    public Color color
    {
        get { return difficultyColor; }
        set { difficultyColor = value; }
    }

    /// <summary>
    /// 특정 키 개수에 대한 차트 경로를 가져옵니다.
    /// </summary>
    public string GetChartPath(int keyCount)
    {
        // 먼저 chartFileName이 있으면 그것을 반환
        if (!string.IsNullOrEmpty(chartFileName))
        {
            return chartFileName;
        }

        // 레거시 방식: chartPaths에서 찾기
        ChartPathInfo info = chartPaths.Find(c => c.keyCount == keyCount);
        return info != null ? info.chartPath : "";
    }
}

/// <summary>
/// 키 개수별 차트 파일 경로 정보
/// </summary>
[System.Serializable]
public class ChartPathInfo
{
    [Tooltip("키 개수 (4, 5, 6, 7, 8, 10)")]
    public int keyCount = 4;

    [Tooltip("차트 파일 경로")]
    public string chartPath = "";
}
