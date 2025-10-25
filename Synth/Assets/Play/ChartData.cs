using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임플레이용 차트 데이터 클래스
/// JSON 직렬화/역직렬화 지원
/// </summary>
[System.Serializable]
public class ChartData
{
    [Header("곡 메타데이터")]
    public string songName = "";
    public string artistName = "";
    public string audioFileName = "";
    public float bpm = 120f;
    public float offset = 0f; // 오디오 오프셋 (초)

    [Header("난이도 정보")]
    public string difficulty = "Normal"; // Easy, Normal, Hard, Expert, Master, Special
    public int keyCount = 4; // 4K, 5K, 6K, 7K, 8K, 10K
    public int level = 1; // 난이도 레벨 (1-20)

    [Header("차트 데이터")]
    public List<NoteData> notes = new List<NoteData>();

    /// <summary>
    /// JSON 문자열로부터 ChartData 생성
    /// </summary>
    public static ChartData FromJson(string json)
    {
        return JsonUtility.FromJson<ChartData>(json);
    }

    /// <summary>
    /// ChartData를 JSON 문자열로 변환
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    /// <summary>
    /// 노트 추가
    /// </summary>
    public void AddNote(NoteData note)
    {
        if (note != null)
        {
            notes.Add(note);
            SortNotesByTime();
        }
    }

    /// <summary>
    /// 타이밍 순으로 노트 정렬
    /// </summary>
    public void SortNotesByTime()
    {
        notes.Sort((a, b) => a.timing.CompareTo(b.timing));
    }

    /// <summary>
    /// 차트 초기화
    /// </summary>
    public void Clear()
    {
        notes.Clear();
        songName = "";
        artistName = "";
        audioFileName = "";
        bpm = 120f;
        offset = 0f;
        difficulty = "Normal";
        keyCount = 4;
        level = 1;
    }

    /// <summary>
    /// 차트의 총 노트 수 반환
    /// </summary>
    public int GetNoteCount()
    {
        return notes.Count;
    }

    /// <summary>
    /// 차트 재생 시간 계산 (초)
    /// </summary>
    public double GetChartDuration()
    {
        if (notes.Count == 0) return 0.0;

        double maxTime = 0.0;
        foreach (var note in notes)
        {
            double noteEndTime = note.isLongNote ? note.longNoteEndTiming : note.timing;
            if (noteEndTime > maxTime)
                maxTime = noteEndTime;
        }
        return maxTime;
    }

    /// <summary>
    /// 특정 트랙의 노트 수 반환
    /// </summary>
    public int GetNoteCountByTrack(int track)
    {
        int count = 0;
        foreach (var note in notes)
        {
            if (note.track == track)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 차트 유효성 검사
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(songName)) return false;
        if (string.IsNullOrEmpty(audioFileName)) return false;
        if (bpm <= 0) return false;
        if (notes.Count == 0) return false;
        if (keyCount < 4 || keyCount > 10) return false;

        // 모든 노트의 트랙이 유효한지 확인
        foreach (var note in notes)
        {
            if (note.track < 0 || note.track >= keyCount)
                return false;
        }

        return true;
    }
}
