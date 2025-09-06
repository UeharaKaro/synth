using UnityEngine;
using System.Collections.Generic;

namespace ChartSystem
{
    /// <summary>
    /// 독립적인 차트 데이터 - 완전히 자율적
    /// 노트와 메타데이터를 포함한 리듬 게임 차트 관련 모든 데이터를 저장
    /// </summary>
    [System.Serializable]
    public class ChartDataNew
    {
        [Header("차트 메타데이터")]
        public string songName = "";
        public string artistName = "";
        public string audioFileName = "";
        public float bpm = 120f;
        public float chartDifficulty = 1.0f;
        
        [Header("차트 노트들")]
        public List<NoteData> notes = new List<NoteData>();
        
        // 생성자
        public ChartDataNew()
        {
            notes = new List<NoteData>();
        }
        
        public ChartDataNew(string songName, string artistName, float bpm)
        {
            this.songName = songName;
            this.artistName = artistName;
            this.bpm = bpm;
            this.notes = new List<NoteData>();
        }
        
        // 유틸리티 메서드들
        public void AddNote(NoteData note)
        {
            if (note != null)
            {
                notes.Add(note);
                SortNotesByTime();
            }
        }
        
        public void RemoveNote(NoteData note)
        {
            notes.Remove(note);
        }
        
        public void SortNotesByTime()
        {
            notes.Sort((a, b) => a.timing.CompareTo(b.timing));
        }
        
        public void Clear()
        {
            notes.Clear();
            songName = "";
            artistName = "";
            audioFileName = "";
            bpm = 120f;
            chartDifficulty = 1.0f;
        }
        
        public int GetNoteCount()
        {
            return notes.Count;
        }
        
        public float GetChartDuration()
        {
            if (notes.Count == 0) return 0f;
            
            float maxTime = 0f;
            foreach (var note in notes)
            {
                float noteEndTime = note.isLongNote ? note.longNoteEndTiming : note.timing;
                if (noteEndTime > maxTime)
                    maxTime = noteEndTime;
            }
            return maxTime;
        }
    }

    /// <summary>
    /// 독립적인 노트 데이터 클래스
    /// 단일 노트에 관한 모든 정보를 포함
    /// </summary>
    [System.Serializable]
    public class NoteData
    {
        [Header("노트 타이밍")]
        public float timing = 0f;              // 노트를 쳐야 하는 시간 (초 단위)
        public float beatTiming = 0f;          // BPM 계산을 위한 박자 기반 타이밍
        
        [Header("노트 속성")]
        public int track = 0;                  // 이 노트가 속한 트랙/레인 (0부터 시작)
        public bool isLongNote = false;        // 롱노트(홀드 노트) 여부
        public float longNoteEndTiming = 0f;   // 롱노트가 끝나야 하는 시간 (초 단위)
        
        [Header("노트 타입")]
        public KeySoundType keySoundType = KeySoundType.None;
        public string noteType = "normal";     // 추가적인 노트 타입 설명자
        
        // 생성자들
        public NoteData()
        {
        }
        
        public NoteData(float timing, int track)
        {
            this.timing = timing;
            this.track = track;
        }
        
        public NoteData(float timing, int track, KeySoundType keySoundType, bool isLongNote = false, float endTiming = 0f)
        {
            this.timing = timing;
            this.track = track;
            this.keySoundType = keySoundType;
            this.isLongNote = isLongNote;
            this.longNoteEndTiming = endTiming;
        }
        
        // 유틸리티 메서드들
        public void CalculateBeatTiming(float bpm)
        {
            beatTiming = timing * bpm / 60.0f;
        }
        
        public float GetDuration()
        {
            return isLongNote ? (longNoteEndTiming - timing) : 0f;
        }
        
        public bool IsValidLongNote()
        {
            return isLongNote && longNoteEndTiming > timing;
        }
    }

    /// <summary>
    /// 독립적인 키 사운드 타입 열거형
    /// 노트로 트리거할 수 있는 다양한 사운드 타입들을 정의
    /// </summary>
    public enum KeySoundType
    {
        None,       // 사운드 없음
        Kick,       // 킥 드럼 사운드
        Snare,      // 스네어 드럼 사운드
        Hihat,      // 하이햇 사운드
        Vocal1,     // 보컬 사운드 1
        Vocal2,     // 보컬 사운드 2
        Synth1,     // 신디사이저 사운드 1
        Synth2,     // 신디사이저 사운드 2
        Bass,       // 베이스 사운드
        Piano,      // 피아노 사운드
        Guitar      // 기타 사운드
    }

    /// <summary>
    /// 효과음 타입들
    /// </summary>
    public enum SFXType
    {
        Metronome,  // 메트로놈 사운드
        Hit,        // 히트 효과음
        Miss        // 미스 효과음
    }
}