using UnityEngine;
using System.Collections.Generic;

public static class ChartValidator
{
    public static ValidationResult ValidateChart(ChartData chart)
    {
        var result = new ValidationResult();
        
        // 기본 필드 검증
        if (string.IsNullOrEmpty(chart.audioFileName))
        {
            result.AddError("오디오 파일명이 비어있습니다.");
        }
        
        if (chart.bpm <= 0)
        {
            result.AddError("BPM이 0보다 작거나 같습니다.");
        }
        
        if (chart.laneCount < 4 || chart.laneCount > 10)
        {
            result.AddError($"레인 수가 유효하지 않습니다: {chart.laneCount} (4-10 사이여야 함)");
        }
        
        // 노트 데이터 검증
        for (int i = 0; i < chart.notes.Count; i++)
        {
            var note = chart.notes[i];
            ValidateNote(note, i, chart, result);
        }
        
        // 노트 순서 검증
        for (int i = 1; i < chart.notes.Count; i++)
        {
            if (chart.notes[i].timing < chart.notes[i-1].timing)
            {
                result.AddWarning($"노트 순서가 잘못되었습니다. 인덱스 {i}와 {i-1}");
            }
        }
        
        // 노트 밀도 검증
        CheckNoteDensity(chart, result);
        
        return result;
    }
    
    private static void ValidateNote(NoteData note, int index, ChartData chart, ValidationResult result)
    {
        // 트랙 범위 검증
        if (note.track < 0 || note.track >= chart.laneCount)
        {
            result.AddError($"노트 {index}: 트랙 번호가 유효하지 않습니다. ({note.track}, 범위: 0-{chart.laneCount-1})");
        }
        
        // 타이밍 검증
        if (note.timing < 0)
        {
            result.AddError($"노트 {index}: 타이밍이 음수입니다. ({note.timing})");
        }
        
        // 롱노트 검증
        if (note.isLongNote)
        {
            if (note.longNoteEndTiming <= note.timing)
            {
                result.AddError($"노트 {index}: 롱노트 종료 시간이 시작 시간보다 작거나 같습니다.");
            }
            
            double longNoteDuration = note.longNoteEndTiming - note.timing;
            if (longNoteDuration < 0.1) // 100ms 미만
            {
                result.AddWarning($"노트 {index}: 롱노트가 너무 짧습니다. ({longNoteDuration:F3}초)");
            }
            
            if (longNoteDuration > 30.0) // 30초 초과
            {
                result.AddWarning($"노트 {index}: 롱노트가 너무 깁니다. ({longNoteDuration:F1}초)");
            }
        }
    }
    
    private static void CheckNoteDensity(ChartData chart, ValidationResult result)
    {
        // 1초 구간별 노트 밀도 체크
        var timeSlots = new Dictionary<int, int>();
        
        foreach (var note in chart.notes)
        {
            int timeSlot = (int)note.timing;
            if (!timeSlots.ContainsKey(timeSlot))
                timeSlots[timeSlot] = 0;
            timeSlots[timeSlot]++;
        }
        
        foreach (var slot in timeSlots)
        {
            if (slot.Value > chart.laneCount * 4) // 초당 레인수 * 4개 이상
            {
                result.AddWarning($"{slot.Key}초 구간에 노트가 너무 많습니다. ({slot.Value}개)");
            }
        }
    }
}

public class ValidationResult
{
    public List<string> Errors { get; private set; } = new List<string>();
    public List<string> Warnings { get; private set; } = new List<string>();
    
    public bool IsValid => Errors.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;
    
    public void AddError(string error)
    {
        Errors.Add(error);
        Debug.LogError($"Chart Validation Error: {error}");
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
        Debug.LogWarning($"Chart Validation Warning: {warning}");
    }
    
    public string GetSummary()
    {
        string summary = "";
        
        if (Errors.Count > 0)
        {
            summary += $"오류 {Errors.Count}개:\n";
            foreach (var error in Errors)
            {
                summary += $"- {error}\n";
            }
        }
        
        if (Warnings.Count > 0)
        {
            summary += $"경고 {Warnings.Count}개:\n";
            foreach (var warning in Warnings)
            {
                summary += $"- {warning}\n";
            }
        }
        
        if (IsValid && !HasWarnings)
        {
            summary = "차트가 유효합니다.";
        }
        
        return summary;
    }
}

public static class ChartOptimizer
{
    public static ChartData OptimizeChart(ChartData originalChart)
    {
        var optimizedChart = originalChart.Clone();
        
        // 중복 노트 제거
        RemoveDuplicateNotes(optimizedChart);
        
        // 노트 정렬
        optimizedChart.SortNotesByTiming();
        
        // 불필요한 데이터 정리
        CleanupNoteData(optimizedChart);
        
        return optimizedChart;
    }
    
    private static void RemoveDuplicateNotes(ChartData chart)
    {
        var uniqueNotes = new List<NoteData>();
        var seenNotes = new HashSet<string>();
        
        foreach (var note in chart.notes)
        {
            string noteKey = $"{note.timing:F3}_{note.track}_{note.isLongNote}";
            if (!seenNotes.Contains(noteKey))
            {
                seenNotes.Add(noteKey);
                uniqueNotes.Add(note);
            }
        }
        
        int removedCount = chart.notes.Count - uniqueNotes.Count;
        if (removedCount > 0)
        {
            chart.notes = uniqueNotes;
            Debug.Log($"중복 노트 {removedCount}개 제거됨");
        }
    }
    
    private static void CleanupNoteData(ChartData chart)
    {
        foreach (var note in chart.notes)
        {
            // 타이밍 정밀도 조정 (소수점 3자리까지)
            note.timing = System.Math.Round(note.timing, 3);
            
            if (note.isLongNote)
            {
                note.longNoteEndTiming = System.Math.Round(note.longNoteEndTiming, 3);
            }
        }
    }
}