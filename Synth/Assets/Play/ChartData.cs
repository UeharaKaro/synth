using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임플레이용 차트 데이터 클래스
/// JSON 직렬화/역직렬화 지원
/// ChartDataNew와 호환 가능
/// </summary>
[System.Serializable]
public class ChartData
{
    [Header("기본 음악 정보")]
    public string songName = "";
    public string artistName = "";
    public string audioFileName = "";
    public string coverImageFileName = "";
    public float bpm = 120f;
    public float offset = 0f; // 오디오 오프셋 (초)

    [Header("음악 상세 정보 (선택)")]
    public float duration = 0f; // 곡 길이 (초)
    public float previewStart = 0f; // 미리듣기 시작 시간 (초)
    public float previewDuration = 15f; // 미리듣기 길이 (초)
    public string composer = ""; // 작곡가
    public string arranger = ""; // 편곡자

    [Header("난이도 정보")]
    public string difficulty = "Normal"; // Easy, Normal, Hard, Expert, Master, Special
    public int keyCount = 4; // 4K, 5K, 6K, 7K, 8K, 10K
    public float level = 1.0f; // 난이도 레벨 (1-20, 소수점 1자리)

    [Header("차트 제작 정보 (선택)")]
    public string chartAuthor = ""; // 차트 제작자
    public string createdDate = ""; // 제작일
    public string modifiedDate = ""; // 수정일
    public string version = "1.0"; // 차트 버전
    public string description = ""; // 차트 설명

    [Header("차트 통계 (자동 계산)")]
    public int noteCount = 0; // 총 노트 수
    public int longNoteCount = 0; // 롱노트 수
    public int maxCombo = 0; // 최대 콤보
    public float density = 0f; // 노트 밀도 (notes per second)

    [Header("비주얼 설정 (선택)")]
    public string backgroundImage = ""; // 배경 이미지 파일명
    public string backgroundVideo = ""; // 배경 비디오 파일명
    public string storyboardFile = ""; // 스토리보드 파일명
    public string skinOverride = ""; // 전용 스킨 경로

    [Header("메타/분류 (선택)")]
    public string tags = ""; // 태그 (쉼표로 구분)
    public string source = ""; // 출처
    public string copyright = ""; // 저작권 정보
    public string beatmapId = ""; // 온라인 차트 ID

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
        double chartDuration = GetChartDuration();
        if (chartDuration > 0)
            density = (float)(noteCount / chartDuration);
        else
            density = 0f;
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
