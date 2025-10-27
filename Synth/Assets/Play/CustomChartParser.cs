using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 커스텀 .synth 파일 형식을 파싱하는 클래스
/// </summary>
public class CustomChartParser
{
    /// <summary>
    /// .synth 파일을 읽어 ChartData로 변환합니다
    /// </summary>
    public static ChartSystem.ChartDataNew ParseFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"CustomChartParser: 파일이 존재하지 않습니다 - {filePath}");
            return null;
        }

        try
        {
            string content = File.ReadAllText(filePath);
            return ParseFromString(content);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CustomChartParser: 파일 읽기 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// .synth 형식의 문자열을 ChartData로 변환합니다
    /// </summary>
    public static ChartSystem.ChartDataNew ParseFromString(string content)
    {
        ChartSystem.ChartDataNew chart = new ChartSystem.ChartDataNew();

        try
        {
            string[] lines = content.Split('\n');
            string currentSection = "";
            List<string> sectionLines = new List<string>();

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                // 빈 줄이나 주석 무시
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                // 섹션 시작
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // 이전 섹션 처리
                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        ProcessSection(chart, currentSection, sectionLines);
                    }

                    currentSection = line.Substring(1, line.Length - 2); // [ ] 제거
                    sectionLines.Clear();
                }
                else
                {
                    sectionLines.Add(line);
                }
            }

            // 마지막 섹션 처리
            if (!string.IsNullOrEmpty(currentSection))
            {
                ProcessSection(chart, currentSection, sectionLines);
            }

            // 차트 통계 자동 계산
            chart.UpdateStatistics();

            Debug.Log($"CustomChartParser: 차트 파싱 성공 - {chart.songName} ({chart.noteCount}개 노트)");
            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CustomChartParser: 파싱 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 각 섹션을 처리합니다
    /// </summary>
    private static void ProcessSection(ChartSystem.ChartDataNew chart, string sectionName, List<string> lines)
    {
        switch (sectionName)
        {
            case "METADATA":
                ParseMetadata(chart, lines);
                break;
            case "MUSIC_INFO":
                ParseMusicInfo(chart, lines);
                break;
            case "DIFFICULTY":
                ParseDifficulty(chart, lines);
                break;
            case "CHART_INFO":
                ParseChartInfo(chart, lines);
                break;
            case "STATISTICS":
                // 통계는 자동 계산되므로 파싱하지 않음 (선택적으로 검증용으로 사용 가능)
                break;
            case "VISUALS":
                ParseVisuals(chart, lines);
                break;
            case "META":
                ParseMeta(chart, lines);
                break;
            case "PATTERN_DIFFICULTY":
                chart.patternDifficulty = ChartSystem.PatternDifficulty.ParseFromSynthFormat(lines.ToArray());
                break;
            case "MEASURE_LINES":
                ParseMeasureLines(chart, lines);
                break;
            case "NOTES":
                ParseNotes(chart, lines);
                break;
            default:
                Debug.LogWarning($"CustomChartParser: 알 수 없는 섹션 - {sectionName}");
                break;
        }
    }

    #region Section Parsers

    private static void ParseMetadata(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "Title": chart.songName = UnescapeValue(kvp.Value); break;
                case "Artist": chart.artistName = UnescapeValue(kvp.Value); break;
                case "Audio": chart.audioFileName = UnescapeValue(kvp.Value); break;
                case "Cover": chart.coverImageFileName = UnescapeValue(kvp.Value); break;
                case "BPM": float.TryParse(kvp.Value, out chart.bpm); break;
                case "Offset": float.TryParse(kvp.Value, out chart.offset); break;
            }
        }
    }

    private static void ParseMusicInfo(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "Duration": float.TryParse(kvp.Value, out chart.duration); break;
                case "PreviewStart": float.TryParse(kvp.Value, out chart.previewStart); break;
                case "PreviewDuration": float.TryParse(kvp.Value, out chart.previewDuration); break;
                case "Composer": chart.composer = UnescapeValue(kvp.Value); break;
                case "Arranger": chart.arranger = UnescapeValue(kvp.Value); break;
            }
        }
    }

    private static void ParseDifficulty(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "Name": chart.difficulty = kvp.Value; break;
                case "Keys": int.TryParse(kvp.Value, out chart.keyCount); break;
                case "Level": int.TryParse(kvp.Value, out chart.level); break;
            }
        }
    }

    private static void ParseChartInfo(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "Author": chart.chartAuthor = UnescapeValue(kvp.Value); break;
                case "Created": chart.createdDate = UnescapeValue(kvp.Value); break;
                case "Modified": chart.modifiedDate = UnescapeValue(kvp.Value); break;
                case "Version": chart.version = UnescapeValue(kvp.Value); break;
                case "Description": chart.description = UnescapeValue(kvp.Value); break;
            }
        }
    }

    private static void ParseVisuals(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "BackgroundImage": chart.backgroundImage = UnescapeValue(kvp.Value); break;
                case "BackgroundVideo": chart.backgroundVideo = UnescapeValue(kvp.Value); break;
                case "StoryboardFile": chart.storyboardFile = UnescapeValue(kvp.Value); break;
                case "SkinOverride": chart.skinOverride = UnescapeValue(kvp.Value); break;
            }
        }
    }

    private static void ParseMeta(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "Tags": chart.tags = UnescapeValue(kvp.Value); break;
                case "Source": chart.source = UnescapeValue(kvp.Value); break;
                case "Copyright": chart.copyright = UnescapeValue(kvp.Value); break;
                case "BeatmapID": chart.beatmapId = UnescapeValue(kvp.Value); break;
            }
        }
    }

    private static void ParseMeasureLines(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        chart.measureLineOverrides.Clear();

        foreach (string line in lines)
        {
            if (line.StartsWith("DefaultBeatsPerMeasure:"))
            {
                string value = line.Substring("DefaultBeatsPerMeasure:".Length).Trim();
                int.TryParse(value, out chart.defaultBeatsPerMeasure);
            }
            else if (line.StartsWith("Override:"))
            {
                string value = line.Substring("Override:".Length).Trim();
                string[] parts = value.Split(',');
                if (parts.Length >= 3)
                {
                    int start, end, beats;
                    if (int.TryParse(parts[0].Trim(), out start) &&
                        int.TryParse(parts[1].Trim(), out end) &&
                        int.TryParse(parts[2].Trim(), out beats))
                    {
                        chart.measureLineOverrides.Add(new ChartSystem.MeasureLineOverride(start, end, beats));
                    }
                }
            }
        }
    }

    private static void ParseNotes(ChartSystem.ChartDataNew chart, List<string> lines)
    {
        chart.notes.Clear();

        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length < 3)
                continue;

            double timing;
            int track;
            KeySoundType keySoundType;

            if (!double.TryParse(parts[0].Trim(), out timing))
                continue;
            if (!int.TryParse(parts[1].Trim(), out track))
                continue;
            if (!System.Enum.TryParse(parts[2].Trim(), out keySoundType))
                keySoundType = KeySoundType.None;

            // 롱노트 체크
            if (parts.Length >= 4)
            {
                double endTime;
                if (double.TryParse(parts[3].Trim(), out endTime))
                {
                    // 롱노트
                    NoteData note = new NoteData(timing, track, keySoundType, true, endTime);
                    note.CalculateBeatTiming(chart.bpm);
                    chart.AddNote(note);
                }
                else
                {
                    // 일반 노트
                    NoteData note = new NoteData(timing, track, keySoundType);
                    note.CalculateBeatTiming(chart.bpm);
                    chart.AddNote(note);
                }
            }
            else
            {
                // 일반 노트
                NoteData note = new NoteData(timing, track, keySoundType);
                note.CalculateBeatTiming(chart.bpm);
                chart.AddNote(note);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// "Key: Value" 형식의 줄을 파싱합니다
    /// </summary>
    private static KeyValuePair<string, string> ParseKeyValue(string line)
    {
        int colonIndex = line.IndexOf(':');
        if (colonIndex > 0)
        {
            string key = line.Substring(0, colonIndex).Trim();
            string value = line.Substring(colonIndex + 1).Trim();
            return new KeyValuePair<string, string>(key, value);
        }
        return new KeyValuePair<string, string>("", "");
    }

    /// <summary>
    /// 이스케이프 처리된 값을 원래대로 복원합니다
    /// </summary>
    private static string UnescapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return value.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
    }

    #endregion
}
