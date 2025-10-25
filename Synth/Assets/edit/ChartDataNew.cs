using UnityEngine;
using System.Collections.Generic;

namespace ChartSystem
{
    /// <summary>
    /// 마디선 오버라이드 설정 (특정 마디 범위에서 다른 박자 수 적용)
    /// </summary>
    [System.Serializable]
    public class MeasureLineOverride
    {
        public int startMeasure;      // 시작 마디 번호 (1부터 시작)
        public int endMeasure;        // 끝 마디 번호 (포함)
        public int beatsPerMeasure;   // 이 구간의 마디당 박자 수

        public MeasureLineOverride(int start, int end, int beats)
        {
            startMeasure = start;
            endMeasure = end;
            beatsPerMeasure = beats;
        }
    }

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

        [Header("마디선 설정 (플레이 시 표시)")]
        public int defaultBeatsPerMeasure = 4;  // 기본 마디당 박자 수
        public List<MeasureLineOverride> measureLineOverrides = new List<MeasureLineOverride>();

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
            defaultBeatsPerMeasure = 4;
            measureLineOverrides.Clear();
        }
        
        public int GetNoteCount()
        {
            return notes.Count;
        }
        
        public float GetChartDuration()
        {
            if (notes.Count == 0) return 0f;

            double maxTime = 0.0;
            foreach (var note in notes)
            {
                double noteEndTime = note.isLongNote ? note.longNoteEndTiming : note.timing;
                if (noteEndTime > maxTime)
                    maxTime = noteEndTime;
            }
            return (float)maxTime;
        }
    }

    // NoteData 클래스는 Assets/Play/NoteData.cs (Global namespace)에 정의되어 있습니다.
    // ChartSystem namespace에서도 global NoteData를 사용하여 두 시스템 간 호환성을 유지합니다.
    //
    // 참고: ChartEditorNew에서 사용하는 NoteData는 다음과 같은 생성자를 지원합니다:
    //   - new NoteData(double timing, int track, KeySoundType keySoundType, bool isLongNote, double endTiming)
    //
    // KeySoundType, SFXType enum은 Assets/GameEnums.cs에 정의되어 있습니다.
}