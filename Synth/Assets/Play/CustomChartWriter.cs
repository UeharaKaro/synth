using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 커스텀 .synth 파일 형식으로 차트를 저장하는 클래스
/// </summary>
public class CustomChartWriter
{
    /// <summary>
    /// ChartData를 .synth 파일로 저장합니다
    /// </summary>
    public static bool SaveToFile(ChartSystem.ChartDataNew chart, string filePath)
    {
        if (chart == null)
        {
            Debug.LogError("CustomChartWriter: 저장할 차트가 null입니다");
            return false;
        }

        if (!chart.IsValid())
        {
            Debug.LogError("CustomChartWriter: 유효하지 않은 차트 데이터입니다");
            return false;
        }

        try
        {
            // 차트 통계 업데이트
            chart.UpdateStatistics();

            // .synth 파일 내용 생성
            string content = GenerateSynthFormat(chart);

            // 파일 저장
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);

            Debug.Log($"CustomChartWriter: 차트 저장 성공 - {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CustomChartWriter: 차트 저장 실패 - {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// ChartData를 .synth 형식의 문자열로 변환합니다
    /// </summary>
    private static string GenerateSynthFormat(ChartSystem.ChartDataNew chart)
    {
        StringBuilder sb = new StringBuilder();

        // 헤더
        sb.AppendLine("# Synth Chart Format v1.0");
        sb.AppendLine($"# Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // 기본 음악 정보
        sb.AppendLine("[METADATA]");
        sb.AppendLine($"Title: {EscapeValue(chart.songName)}");
        sb.AppendLine($"Artist: {EscapeValue(chart.artistName)}");
        sb.AppendLine($"Audio: {EscapeValue(chart.audioFileName)}");
        sb.AppendLine($"Cover: {EscapeValue(chart.coverImageFileName)}");
        sb.AppendLine($"BPM: {chart.bpm}");
        sb.AppendLine($"Offset: {chart.offset}");
        sb.AppendLine();

        // 음악 상세 정보
        sb.AppendLine("[MUSIC_INFO]");
        sb.AppendLine($"Duration: {chart.duration}");
        sb.AppendLine($"PreviewStart: {chart.previewStart}");
        sb.AppendLine($"PreviewDuration: {chart.previewDuration}");
        sb.AppendLine($"Composer: {EscapeValue(chart.composer)}");
        sb.AppendLine($"Arranger: {EscapeValue(chart.arranger)}");
        sb.AppendLine();

        // 난이도 정보
        sb.AppendLine("[DIFFICULTY]");
        sb.AppendLine($"Name: {chart.difficulty}");
        sb.AppendLine($"Keys: {chart.keyCount}");
        sb.AppendLine($"Level: {chart.level}");
        sb.AppendLine();

        // 차트 제작 정보
        sb.AppendLine("[CHART_INFO]");
        sb.AppendLine($"Author: {EscapeValue(chart.chartAuthor)}");
        sb.AppendLine($"Created: {EscapeValue(chart.createdDate)}");
        sb.AppendLine($"Modified: {EscapeValue(chart.modifiedDate)}");
        sb.AppendLine($"Version: {EscapeValue(chart.version)}");
        sb.AppendLine($"Description: {EscapeValue(chart.description)}");
        sb.AppendLine();

        // 차트 통계
        sb.AppendLine("[STATISTICS]");
        sb.AppendLine($"NoteCount: {chart.noteCount}");
        sb.AppendLine($"LongNoteCount: {chart.longNoteCount}");
        sb.AppendLine($"MaxCombo: {chart.maxCombo}");
        sb.AppendLine($"Density: {chart.density:F2}");
        sb.AppendLine();

        // 비주얼 설정
        sb.AppendLine("[VISUALS]");
        sb.AppendLine($"BackgroundImage: {EscapeValue(chart.backgroundImage)}");
        sb.AppendLine($"BackgroundVideo: {EscapeValue(chart.backgroundVideo)}");
        sb.AppendLine($"StoryboardFile: {EscapeValue(chart.storyboardFile)}");
        sb.AppendLine($"SkinOverride: {EscapeValue(chart.skinOverride)}");
        sb.AppendLine();

        // 메타/분류
        sb.AppendLine("[META]");
        sb.AppendLine($"Tags: {EscapeValue(chart.tags)}");
        sb.AppendLine($"Source: {EscapeValue(chart.source)}");
        sb.AppendLine($"Copyright: {EscapeValue(chart.copyright)}");
        sb.AppendLine($"BeatmapID: {EscapeValue(chart.beatmapId)}");
        sb.AppendLine();

        // 패턴 난이도
        if (chart.patternDifficulty != null)
        {
            sb.Append(chart.patternDifficulty.ToSynthFormat());
            sb.AppendLine();
        }

        // 마디선 설정
        sb.AppendLine("[MEASURE_LINES]");
        sb.AppendLine($"DefaultBeatsPerMeasure: {chart.defaultBeatsPerMeasure}");
        if (chart.measureLineOverrides != null && chart.measureLineOverrides.Count > 0)
        {
            sb.AppendLine("# Overrides: StartMeasure, EndMeasure, BeatsPerMeasure");
            foreach (var mOverride in chart.measureLineOverrides)
            {
                sb.AppendLine($"Override: {mOverride.startMeasure}, {mOverride.endMeasure}, {mOverride.beatsPerMeasure}");
            }
        }
        sb.AppendLine();

        // 노트 데이터
        sb.AppendLine("[NOTES]");
        sb.AppendLine("# Format: timing, track, keysound, endtime(if long note)");
        foreach (var note in chart.notes)
        {
            if (note.isLongNote)
            {
                sb.AppendLine($"{note.timing:F3}, {note.track}, {note.keySoundType}, {note.longNoteEndTiming:F3}");
            }
            else
            {
                sb.AppendLine($"{note.timing:F3}, {note.track}, {note.keySoundType}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 값을 이스케이프 처리합니다 (빈 문자열은 그대로 유지)
    /// </summary>
    private static string EscapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // 줄바꿈, 탭 등을 이스케이프
        return value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
}
