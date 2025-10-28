using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 곡 목록 스크롤 뷰의 개별 아이템 컴포넌트
/// </summary>
public class SongListItem : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("곡 제목 텍스트")]
    public TextMeshProUGUI titleText;

    [Tooltip("아티스트 텍스트")]
    public TextMeshProUGUI artistText;

    [Tooltip("BPM 텍스트")]
    public TextMeshProUGUI bpmText;

    [Tooltip("난이도 범위 텍스트 (Lv. 1~10)")]
    public TextMeshProUGUI levelRangeText;

    [Tooltip("앨범 커버 이미지 (작은 썸네일)")]
    public Image thumbnailImage;

    [Tooltip("선택 표시 이미지")]
    public Image selectionIndicator;

    [Tooltip("즐겨찾기 아이콘")]
    public GameObject favoriteIcon;

    [Tooltip("잠금 아이콘")]
    public GameObject lockIcon;

    [Tooltip("클리어 뱃지")]
    public GameObject clearedBadge;

    [Tooltip("NEW 뱃지")]
    public GameObject newBadge;

    [Header("색상 설정")]
    [Tooltip("선택되었을 때 배경 색상")]
    public Color selectedColor = new Color(1f, 1f, 0f, 0.3f);

    [Tooltip("선택되지 않았을 때 배경 색상")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.1f);

    // 데이터
    private SongData songData;
    private int itemIndex;
    private SongSelectionUIAdvanced parentUI;
    private bool isSelected = false;
    private bool isFavorite = false;

    // 컴포넌트
    private Image backgroundImage;

    void Awake()
    {
        // 배경 이미지 컴포넌트 가져오기
        backgroundImage = GetComponent<Image>();
    }

    /// <summary>
    /// 곡 아이템을 설정합니다.
    /// </summary>
    public void Setup(SongData song, int index, SongSelectionUIAdvanced parent)
    {
        songData = song;
        itemIndex = index;
        parentUI = parent;

        UpdateDisplay();
    }

    /// <summary>
    /// 화면 표시를 업데이트합니다.
    /// </summary>
    private void UpdateDisplay()
    {
        if (songData == null) return;

        // 텍스트 업데이트
        if (titleText != null)
            titleText.text = songData.title;

        if (artistText != null)
            artistText.text = songData.artist;

        if (bpmText != null)
            bpmText.text = $"BPM: {songData.bpm:F0}";

        // 난이도 범위 계산
        if (levelRangeText != null && songData.difficulties != null && songData.difficulties.Count > 0)
        {
            float minLevel = float.MaxValue;
            float maxLevel = float.MinValue;

            foreach (var diff in songData.difficulties)
            {
                if (diff.level < minLevel) minLevel = diff.level;
                if (diff.level > maxLevel) maxLevel = diff.level;
            }

            if (Mathf.Approximately(minLevel, maxLevel))
            {
                levelRangeText.text = $"Lv. {minLevel:F1}";
            }
            else
            {
                levelRangeText.text = $"Lv. {minLevel:F1}~{maxLevel:F1}";
            }
        }

        // 썸네일 로드
        LoadThumbnail();

        // 아이콘 업데이트
        UpdateIcons();
    }

    /// <summary>
    /// 썸네일 이미지를 로드합니다.
    /// </summary>
    private void LoadThumbnail()
    {
        if (thumbnailImage == null || songData == null) return;

        // CoverArtLoader 사용
        if (CoverArtLoader.Instance != null)
        {
            StartCoroutine(LoadThumbnailCoroutine());
        }
    }

    /// <summary>
    /// 썸네일 로딩 코루틴
    /// </summary>
    private System.Collections.IEnumerator LoadThumbnailCoroutine()
    {
        yield return CoverArtLoader.Instance.LoadCoverArtCoroutine(songData, (texture) =>
        {
            if (texture != null && thumbnailImage != null)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                thumbnailImage.sprite = sprite;
            }
        });
    }

    /// <summary>
    /// 아이콘들을 업데이트합니다.
    /// </summary>
    private void UpdateIcons()
    {
        // 즐겨찾기 아이콘
        if (favoriteIcon != null)
        {
            favoriteIcon.SetActive(isFavorite);
        }

        // 잠금 아이콘
        if (lockIcon != null)
        {
            lockIcon.SetActive(songData != null && songData.isLocked);
        }

        // 클리어 뱃지
        if (clearedBadge != null)
        {
            bool cleared = HasCleared();
            clearedBadge.SetActive(cleared);
        }

        // NEW 뱃지 (최근 추가된 곡)
        if (newBadge != null)
        {
            bool isNew = IsNewSong();
            newBadge.SetActive(isNew);
        }
    }

    /// <summary>
    /// 선택 상태를 설정합니다.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // 배경 색상 변경
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }

        // 선택 표시 이미지
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = selected;
        }

        // 선택 애니메이션 (선택사항)
        if (selected)
        {
            transform.localScale = Vector3.one * 1.05f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 즐겨찾기 상태를 설정합니다.
    /// </summary>
    public void SetFavorite(bool favorite)
    {
        isFavorite = favorite;

        if (favoriteIcon != null)
        {
            favoriteIcon.SetActive(favorite);
        }
    }

    /// <summary>
    /// 곡을 클리어했는지 확인합니다.
    /// </summary>
    private bool HasCleared()
    {
        if (songData == null || songData.difficulties == null) return false;

        // 하나라도 클리어했으면 true
        foreach (var diff in songData.difficulties)
        {
            string key = $"Cleared_{songData.title}_{diff.difficultyName}_{diff.keyCount}K";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 새로운 곡인지 확인합니다 (최근 7일 이내 추가).
    /// </summary>
    private bool IsNewSong()
    {
        if (songData == null) return false;

        string key = $"SongAddedDate_{songData.title}";
        string dateStr = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(dateStr))
        {
            // 날짜가 없으면 지금 저장
            PlayerPrefs.SetString(key, System.DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            return true;
        }

        // 날짜 비교
        if (System.DateTime.TryParse(dateStr, out System.DateTime addedDate))
        {
            System.TimeSpan diff = System.DateTime.Now - addedDate;
            return diff.TotalDays <= 7;
        }

        return false;
    }

    /// <summary>
    /// 아이템이 클릭되었을 때 호출됩니다.
    /// </summary>
    public void OnItemClicked()
    {
        if (parentUI != null)
        {
            parentUI.SelectSongByIndex(itemIndex);
        }
    }
}
