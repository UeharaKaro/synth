using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ChartData
{
    [Header("곡 정보")]
    public string songTitle = "";
    public string artist = "";
    public string audioFileName = "";
    public float bpm = 120f;
    public float audioOffset = 0f; // ms 단위 오디오 딜레이
    
    [Header("차트 설정")]
    public int laneCount = 4;
    public float scrollSpeed = 8f;
    public int beatDivision = 4; // 1/4, 1/8, 1/16 등 (분모)
    
    [Header("노트 데이터")]
    public List<NoteData> notes = new List<NoteData>();
    
    [Header("메타데이터")]
    public string chartVersion = "1.0";
    public DateTime createdDate = DateTime.Now;
    public DateTime lastModified = DateTime.Now;
    
    public ChartData()
    {
        notes = new List<NoteData>();
    }
    
    // 노트 추가
    public void AddNote(NoteData note)
    {
        notes.Add(note);
        SortNotesByTiming();
        lastModified = DateTime.Now;
    }
    
    // 노트 제거
    public bool RemoveNote(NoteData note)
    {
        bool removed = notes.Remove(note);
        if (removed)
        {
            lastModified = DateTime.Now;
        }
        return removed;
    }
    
    // 특정 위치의 노트 제거
    public bool RemoveNoteAt(double timing, int track)
    {
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            if (Mathf.Approximately((float)notes[i].timing, (float)timing) && notes[i].track == track)
            {
                notes.RemoveAt(i);
                lastModified = DateTime.Now;
                return true;
            }
        }
        return false;
    }
    
    // 타이밍순으로 노트 정렬
    public void SortNotesByTiming()
    {
        notes.Sort((a, b) => a.timing.CompareTo(b.timing));
    }
    
    // 특정 시간 범위의 노트들 가져오기
    public List<NoteData> GetNotesInRange(double startTime, double endTime)
    {
        List<NoteData> result = new List<NoteData>();
        foreach (var note in notes)
        {
            if (note.timing >= startTime && note.timing <= endTime)
            {
                result.Add(note);
            }
        }
        return result;
    }
    
    // 특정 트랙의 노트들 가져오기
    public List<NoteData> GetNotesInTrack(int track)
    {
        List<NoteData> result = new List<NoteData>();
        foreach (var note in notes)
        {
            if (note.track == track)
            {
                result.Add(note);
            }
        }
        return result;
    }
    
    // 차트 데이터 유효성 검사
    public bool ValidateChart()
    {
        if (string.IsNullOrEmpty(audioFileName)) return false;
        if (bpm <= 0) return false;
        if (laneCount < 4 || laneCount > 10) return false;
        
        // 노트 위치 유효성 검사
        foreach (var note in notes)
        {
            if (note.track < 0 || note.track >= laneCount) return false;
            if (note.timing < 0) return false;
            if (note.isLongNote && note.longNoteEndTiming <= note.timing) return false;
        }
        
        return true;
    }
    
    // 차트 통계 정보 가져오기
    public ChartStatistics GetStatistics()
    {
        ChartStatistics stats = new ChartStatistics();
        stats.totalNotes = notes.Count;
        stats.normalNotes = 0;
        stats.longNotes = 0;
        
        double totalDuration = 0;
        foreach (var note in notes)
        {
            if (note.isLongNote)
            {
                stats.longNotes++;
                totalDuration = Math.Max(totalDuration, note.longNoteEndTiming);
            }
            else
            {
                stats.normalNotes++;
                totalDuration = Math.Max(totalDuration, note.timing);
            }
        }
        
        stats.chartDuration = totalDuration;
        stats.averageNPS = stats.totalNotes / Math.Max(totalDuration, 1.0); // Notes Per Second
        
        return stats;
    }
    
    // 차트 데이터 복사
    public ChartData Clone()
    {
        ChartData clone = new ChartData();
        clone.songTitle = songTitle;
        clone.artist = artist;
        clone.audioFileName = audioFileName;
        clone.bpm = bpm;
        clone.audioOffset = audioOffset;
        clone.laneCount = laneCount;
        clone.scrollSpeed = scrollSpeed;
        clone.beatDivision = beatDivision;
        clone.chartVersion = chartVersion;
        clone.createdDate = createdDate;
        clone.lastModified = DateTime.Now;
        
        foreach (var note in notes)
        {
            clone.notes.Add(new NoteData(note.timing, note.track, note.keySoundType, note.isLongNote, note.longNoteEndTiming));
        }
        
        return clone;
    }
}

[System.Serializable]
public class ChartStatistics
{
    public int totalNotes;
    public int normalNotes;
    public int longNotes;
    public double chartDuration; // 초 단위
    public double averageNPS; // Notes Per Second
}