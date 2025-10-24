using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임의 모든 곡 정보를 관리하는 데이터베이스
/// ScriptableObject로 구현되어 Unity Inspector에서 쉽게 관리 가능
/// </summary>
[CreateAssetMenu(fileName = "SongDatabase", menuName = "Rhythm Game/Song Database", order = 1)]
public class SongDatabase : ScriptableObject
{
    [Header("곡 목록")]
    [Tooltip("게임에 포함된 모든 곡 목록")]
    public List<SongData> songs = new List<SongData>();

    [Header("설정")]
    [Tooltip("기본 난이도")]
    public string defaultDifficulty = "Normal";

    [Tooltip("기본 키 개수")]
    public int defaultKeyCount = 4;

    /// <summary>
    /// 곡 ID로 곡 데이터를 가져옵니다.
    /// </summary>
    public SongData GetSongById(string songId)
    {
        return songs.Find(s => s.songId == songId);
    }

    /// <summary>
    /// 인덱스로 곡 데이터를 가져옵니다.
    /// </summary>
    public SongData GetSongByIndex(int index)
    {
        if (index >= 0 && index < songs.Count)
            return songs[index];
        return null;
    }

    /// <summary>
    /// 총 곡 개수를 반환합니다.
    /// </summary>
    public int GetSongCount()
    {
        return songs.Count;
    }

    /// <summary>
    /// 잠금 해제된 곡 목록만 가져옵니다.
    /// </summary>
    public List<SongData> GetUnlockedSongs()
    {
        return songs.Where(s => !s.isLocked).ToList();
    }

    /// <summary>
    /// 특정 장르의 곡 목록을 가져옵니다.
    /// </summary>
    public List<SongData> GetSongsByGenre(string genre)
    {
        return songs.Where(s => s.genre == genre).ToList();
    }

    /// <summary>
    /// 특정 아티스트의 곡 목록을 가져옵니다.
    /// </summary>
    public List<SongData> GetSongsByArtist(string artist)
    {
        return songs.Where(s => s.artist == artist).ToList();
    }

    /// <summary>
    /// 특정 키 개수를 지원하는 곡 목록을 가져옵니다.
    /// </summary>
    public List<SongData> GetSongsByKeyCount(int keyCount)
    {
        return songs.Where(s => s.SupportsKeyCount(keyCount)).ToList();
    }

    /// <summary>
    /// 곡 제목으로 검색합니다.
    /// </summary>
    public List<SongData> SearchByTitle(string searchText)
    {
        string lowerSearch = searchText.ToLower();
        return songs.Where(s => s.title.ToLower().Contains(lowerSearch)).ToList();
    }

    /// <summary>
    /// 곡을 추가합니다. (런타임에서 사용)
    /// </summary>
    public void AddSong(SongData song)
    {
        if (!songs.Contains(song))
        {
            songs.Add(song);
        }
    }

    /// <summary>
    /// 곡을 제거합니다. (런타임에서 사용)
    /// </summary>
    public void RemoveSong(SongData song)
    {
        songs.Remove(song);
    }

    /// <summary>
    /// 곡 ID로 곡을 제거합니다.
    /// </summary>
    public void RemoveSongById(string songId)
    {
        SongData song = GetSongById(songId);
        if (song != null)
        {
            songs.Remove(song);
        }
    }

    /// <summary>
    /// 모든 곡을 정렬합니다.
    /// </summary>
    public void SortSongs(SongSortType sortType)
    {
        switch (sortType)
        {
            case SongSortType.Title:
                songs = songs.OrderBy(s => s.title).ToList();
                break;
            case SongSortType.Artist:
                songs = songs.OrderBy(s => s.artist).ToList();
                break;
            case SongSortType.BPM:
                songs = songs.OrderBy(s => s.bpm).ToList();
                break;
            case SongSortType.Genre:
                songs = songs.OrderBy(s => s.genre).ToList();
                break;
        }
    }

    /// <summary>
    /// 데이터베이스 검증 (Editor에서 사용)
    /// </summary>
    public void ValidateDatabase()
    {
        for (int i = 0; i < songs.Count; i++)
        {
            SongData song = songs[i];

            // ID가 비어있으면 자동 생성
            if (string.IsNullOrEmpty(song.songId))
            {
                song.songId = $"song_{i:D3}";
                Debug.LogWarning($"곡 '{song.title}'에 ID가 없어 자동 생성: {song.songId}");
            }

            // 중복 ID 체크
            var duplicates = songs.Where(s => s.songId == song.songId).ToList();
            if (duplicates.Count > 1)
            {
                Debug.LogError($"중복된 곡 ID 발견: {song.songId}");
            }

            // 난이도 정보가 없으면 경고
            if (song.difficulties.Count == 0)
            {
                Debug.LogWarning($"곡 '{song.title}'에 난이도 정보가 없습니다.");
            }

            // 지원하는 키 개수가 없으면 경고
            if (song.supportedKeyCounts.Count == 0)
            {
                Debug.LogWarning($"곡 '{song.title}'에 지원하는 키 개수가 설정되지 않았습니다.");
            }
        }

        Debug.Log($"데이터베이스 검증 완료. 총 {songs.Count}곡");
    }
}

/// <summary>
/// 곡 정렬 타입
/// </summary>
public enum SongSortType
{
    Title,
    Artist,
    BPM,
    Genre
}
