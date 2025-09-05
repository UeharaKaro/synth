using UnityEngine;
using System.Collections.Generic;

namespace ChartSystem
{
    /// <summary>
    /// Self-contained ChartData - completely independent
    /// Stores all data related to a rhythm game chart including notes and metadata
    /// </summary>
    [System.Serializable]
    public class ChartDataNew
    {
        [Header("Chart Metadata")]
        public string songName = "";
        public string artistName = "";
        public string audioFileName = "";
        public float bpm = 120f;
        public float chartDifficulty = 1.0f;
        
        [Header("Chart Notes")]
        public List<NoteData> notes = new List<NoteData>();
        
        // Constructor
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
        
        // Utility methods
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
    /// Self-contained NoteData class
    /// Contains all information about a single note
    /// </summary>
    [System.Serializable]
    public class NoteData
    {
        [Header("Note Timing")]
        public float timing = 0f;              // When the note should be hit (in seconds)
        public float beatTiming = 0f;          // Beat-based timing for BPM calculations
        
        [Header("Note Properties")]
        public int track = 0;                  // Which track/lane this note belongs to (0-based)
        public bool isLongNote = false;        // Whether this is a long note (hold note)
        public float longNoteEndTiming = 0f;   // When the long note should end (in seconds)
        
        [Header("Note Type")]
        public KeySoundType keySoundType = KeySoundType.None;
        public string noteType = "normal";     // Additional note type descriptor
        
        // Constructors
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
        
        // Utility methods
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
    /// Self-contained KeySoundType enum
    /// Defines different types of sounds that can be triggered by notes
    /// </summary>
    public enum KeySoundType
    {
        None,       // No sound
        Kick,       // Kick drum sound
        Snare,      // Snare drum sound
        Hihat,      // Hi-hat sound
        Vocal1,     // Vocal sound 1
        Vocal2,     // Vocal sound 2
        Synth1,     // Synthesizer sound 1
        Synth2,     // Synthesizer sound 2
        Bass,       // Bass sound
        Piano,      // Piano sound
        Guitar      // Guitar sound
    }

    /// <summary>
    /// SFX types for sound effects
    /// </summary>
    public enum SFXType
    {
        Metronome,  // Metronome sound
        Hit,        // Hit sound effect
        Miss        // Miss sound effect
    }
}