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
        [Header("기본 음악 정보")]
        public string songName = "";
        public string artistName = "";
        public string audioFileName = "";
        public string coverImageFileName = "";
        public float bpm = 120f;
        public float offset = 0f; // 오디오 오프셋 (초)

        [Header("음악 상세 정보")]
        public float duration = 0f; // 곡 길이 (초)
        public float previewStart = 0f; // 미리듣기 시작 시간 (초)
        public float previewDuration = 15f; // 미리듣기 길이 (초)
        public string composer = ""; // 작곡가
        public string arranger = ""; // 편곡자

        [Header("난이도 정보")]
        public string difficulty = "Normal"; // Easy, Normal, Hard, Expert, Master, Special
        public int keyCount = 4;  // 4K, 5K, 6K, 7K, 8K, 10K
        public float level = 1.0f; // 난이도 레벨 (1-20, 소수점 1자리)

        [Header("차트 제작 정보")]
        public string chartAuthor = ""; // 차트 제작자
        public string createdDate = ""; // 제작일 (YYYY-MM-DD)
        public string modifiedDate = ""; // 수정일 (YYYY-MM-DD)
        public string version = "1.0"; // 차트 버전
        public string description = ""; // 차트 설명

        [Header("차트 통계")]
        public int noteCount = 0; // 총 노트 수 (자동 계산)
        public int longNoteCount = 0; // 롱노트 수 (자동 계산)
        public int maxCombo = 0; // 최대 콤보 (자동 계산)
        public float density = 0f; // 노트 밀도 (notes per second, 자동 계산)

        [Header("비주얼 설정")]
        public string backgroundImage = ""; // 배경 이미지 파일명
        public string backgroundVideo = ""; // 배경 비디오 파일명
        public string storyboardFile = ""; // 스토리보드 파일명
        public string skinOverride = ""; // 전용 스킨 경로

        [Header("메타/분류")]
        public string tags = ""; // 태그 (쉼표로 구분: "anime,vocal,instrumental")
        public string source = ""; // 출처 (게임명, 애니메이션명 등)
        public string copyright = ""; // 저작권 정보
        public string beatmapId = ""; // 온라인 차트 ID

        [Header("패턴 난이도 (선택)")]
        public PatternDifficulty patternDifficulty = new PatternDifficulty();

        [Header("마디선 설정 (플레이 시 표시)")]
        public int defaultBeatsPerMeasure = 4;  // 기본 마디당 박자 수
        public List<MeasureLineOverride> measureLineOverrides = new List<MeasureLineOverride>();

        [Header("차트 노트들")]
        public List<NoteData> notes = new List<NoteData>();
        
        // 생성자
        public ChartDataNew()
        {
            notes = new List<NoteData>();
            measureLineOverrides = new List<MeasureLineOverride>();
            patternDifficulty = new PatternDifficulty();
            keyCount = 4;  // 기본값 4K
            previewDuration = 15f;
            version = "1.0";
            defaultBeatsPerMeasure = 4;
        }

        public ChartDataNew(string songName, string artistName, float bpm, int keyCount = 4)
        {
            this.songName = songName;
            this.artistName = artistName;
            this.bpm = bpm;
            this.keyCount = keyCount;
            this.notes = new List<NoteData>();
            this.measureLineOverrides = new List<MeasureLineOverride>();
            this.patternDifficulty = new PatternDifficulty();
            this.previewDuration = 15f;
            this.version = "1.0";
            this.defaultBeatsPerMeasure = 4;
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
            measureLineOverrides.Clear();

            // 기본 음악 정보
            songName = "";
            artistName = "";
            audioFileName = "";
            coverImageFileName = "";
            bpm = 120f;
            offset = 0f;

            // 음악 상세 정보
            duration = 0f;
            previewStart = 0f;
            previewDuration = 15f;
            composer = "";
            arranger = "";

            // 난이도 정보
            difficulty = "Normal";
            keyCount = 4;
            level = 1.0f;

            // 차트 제작 정보
            chartAuthor = "";
            createdDate = "";
            modifiedDate = "";
            version = "1.0";
            description = "";

            // 차트 통계
            noteCount = 0;
            longNoteCount = 0;
            maxCombo = 0;
            density = 0f;

            // 비주얼 설정
            backgroundImage = "";
            backgroundVideo = "";
            storyboardFile = "";
            skinOverride = "";

            // 메타/분류
            tags = "";
            source = "";
            copyright = "";
            beatmapId = "";

            // 패턴 난이도
            if (patternDifficulty != null)
                patternDifficulty.Clear();
            else
                patternDifficulty = new PatternDifficulty();

            // 마디선 설정
            defaultBeatsPerMeasure = 4;
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

        /// <summary>
        /// 차트 통계를 자동으로 계산하여 업데이트합니다
        /// </summary>
        public void UpdateStatistics()
        {
            noteCount = notes.Count;

            // 롱노트 개수 계산
            longNoteCount = 0;
            foreach (var note in notes)
            {
                if (note.isLongNote)
                    longNoteCount++;
            }

            // 최대 콤보 = 총 노트 수
            maxCombo = noteCount;

            // 노트 밀도 계산 (notes per second)
            float chartDuration = GetChartDuration();
            if (chartDuration > 0)
                density = noteCount / chartDuration;
            else
                density = 0f;
        }

        /// <summary>
        /// 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(songName)) return false;
            if (string.IsNullOrEmpty(audioFileName)) return false;
            if (bpm <= 0) return false;
            if (keyCount < 4 || keyCount > 10) return false;
            if (notes.Count == 0) return false;

            // 모든 노트의 트랙이 유효한지 확인
            foreach (var note in notes)
            {
                if (note.track < 0 || note.track >= keyCount)
                    return false;
            }

            return true;
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