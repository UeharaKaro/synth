using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

/// <summary>
/// osu! mania .osu 파일 파서
/// .osu 파일을 읽어 ChartData로 변환
/// </summary>
public class OsuManiaParser
{
    /// <summary>
    /// .osu 파일을 읽어 ChartData로 변환합니다
    /// </summary>
    public static ChartData ParseFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"OsuManiaParser: 파일이 존재하지 않습니다 - {filePath}");
            return null;
        }

        try
        {
            string content = File.ReadAllText(filePath);
            return ParseFromString(content, Path.GetFileNameWithoutExtension(filePath));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OsuManiaParser: 파일 읽기 실패 - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// .osu 형식의 문자열을 ChartData로 변환합니다
    /// </summary>
    public static ChartData ParseFromString(string content, string defaultFileName = "chart")
    {
        ChartData chart = new ChartData();
        chart.notes = new List<NoteData>();

        try
        {
            string[] lines = content.Split('\n');
            string currentSection = "";
            List<string> sectionLines = new List<string>();

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                // 빈 줄 무시
                if (string.IsNullOrEmpty(line))
                    continue;

                // 섹션 시작 체크 [Section]
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // 이전 섹션 처리
                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        ProcessSection(chart, currentSection, sectionLines);
                    }

                    currentSection = line.Substring(1, line.Length - 2);
                    sectionLines.Clear();
                }
                // 주석 무시
                else if (line.StartsWith("//"))
                {
                    continue;
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

            // 파일명 설정 (audioFileName이 없으면)
            if (string.IsNullOrEmpty(chart.audioFileName))
            {
                chart.audioFileName = defaultFileName + ".mp3";
            }

            // 차트 통계 업데이트
            chart.UpdateStatistics();

            Debug.Log($"OsuManiaParser: 파싱 성공 - {chart.songName} [{chart.difficulty}] ({chart.noteCount}개 노트, {chart.keyCount}K)");
            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OsuManiaParser: 파싱 실패 - {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 각 섹션을 처리합니다
    /// </summary>
    private static void ProcessSection(ChartData chart, string sectionName, List<string> lines)
    {
        switch (sectionName)
        {
            case "General":
                ParseGeneral(chart, lines);
                break;
            case "Metadata":
                ParseMetadata(chart, lines);
                break;
            case "Difficulty":
                ParseDifficulty(chart, lines);
                break;
            case "TimingPoints":
                ParseTimingPoints(chart, lines);
                break;
            case "HitObjects":
                ParseHitObjects(chart, lines);
                break;
            default:
                // Events, Editor 등 무시
                break;
        }
    }

    #region Section Parsers

    private static void ParseGeneral(ChartData chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "AudioFilename":
                    chart.audioFileName = kvp.Value.Trim();
                    break;
                case "Mode":
                    int mode;
                    if (int.TryParse(kvp.Value, out mode) && mode != 3)
                    {
                        Debug.LogWarning($"OsuManiaParser: 이 파일은 osu!mania가 아닙니다 (Mode: {mode}). 강제로 파싱을 시도합니다.");
                    }
                    break;
            }
        }
    }

    private static void ParseMetadata(ChartData chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "Title":
                    chart.songName = kvp.Value;
                    break;
                case "TitleUnicode":
                    // 유니코드 제목이 있으면 우선 사용
                    if (!string.IsNullOrEmpty(kvp.Value))
                        chart.songName = kvp.Value;
                    break;
                case "Artist":
                    chart.artistName = kvp.Value;
                    break;
                case "ArtistUnicode":
                    // 유니코드 아티스트가 있으면 우선 사용
                    if (!string.IsNullOrEmpty(kvp.Value))
                        chart.artistName = kvp.Value;
                    break;
                case "Creator":
                    chart.chartAuthor = kvp.Value;
                    break;
                case "Version":
                    chart.difficulty = kvp.Value;
                    break;
                case "Source":
                    chart.source = kvp.Value;
                    break;
                case "Tags":
                    chart.tags = kvp.Value;
                    break;
                case "BeatmapID":
                    chart.beatmapId = kvp.Value;
                    break;
            }
        }
    }

    private static void ParseDifficulty(ChartData chart, List<string> lines)
    {
        foreach (string line in lines)
        {
            var kvp = ParseKeyValue(line);
            switch (kvp.Key)
            {
                case "CircleSize":
                    // osu!mania에서 CircleSize = 키 개수
                    float cs;
                    if (float.TryParse(kvp.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out cs))
                    {
                        chart.keyCount = Mathf.RoundToInt(cs);
                    }
                    break;
                case "OverallDifficulty":
                    // OD를 level로 매핑 (0-10 → 0-20)
                    float od;
                    if (float.TryParse(kvp.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out od))
                    {
                        chart.level = od * 2f; // 0-10을 0-20 범위로 확장
                    }
                    break;
            }
        }
    }

    private static void ParseTimingPoints(ChartData chart, List<string> lines)
    {
        // 첫 번째 uninherited timing point에서 BPM 추출
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length < 8)
                continue;

            // uninherited = 1인 것만 처리 (BPM 변경)
            int uninherited;
            if (!int.TryParse(parts[6].Trim(), out uninherited) || uninherited != 1)
                continue;

            float beatLength;
            if (float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out beatLength))
            {
                if (beatLength > 0)
                {
                    chart.bpm = 60000f / beatLength;
                    Debug.Log($"OsuManiaParser: BPM = {chart.bpm:F2} (beatLength = {beatLength})");
                    break; // 첫 번째 BPM만 사용
                }
            }
        }

        // BPM이 설정되지 않았으면 기본값
        if (chart.bpm <= 0)
        {
            chart.bpm = 120f;
            Debug.LogWarning("OsuManiaParser: BPM을 찾을 수 없어 기본값 120으로 설정합니다.");
        }
    }

    private static void ParseHitObjects(ChartData chart, List<string> lines)
    {
        if (chart.keyCount <= 0)
        {
            Debug.LogError("OsuManiaParser: keyCount가 설정되지 않았습니다. Difficulty 섹션을 먼저 파싱해야 합니다.");
            return;
        }

        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length < 5)
                continue;

            // x,y,time,type,hitSound[,endTime:hitSample]
            int x, y, time, type, hitSound;

            if (!int.TryParse(parts[0].Trim(), out x)) continue;
            if (!int.TryParse(parts[1].Trim(), out y)) continue;
            if (!int.TryParse(parts[2].Trim(), out time)) continue;
            if (!int.TryParse(parts[3].Trim(), out type)) continue;
            if (!int.TryParse(parts[4].Trim(), out hitSound)) continue;

            // x 좌표에서 트랙(컬럼) 계산
            int track = Mathf.FloorToInt(x * chart.keyCount / 512f);
            track = Mathf.Clamp(track, 0, chart.keyCount - 1);

            // 시간 변환 (밀리초 → 초)
            double timing = time / 1000.0;

            // 롱노트 체크 (type & 128)
            bool isLongNote = (type & 128) != 0;

            if (isLongNote && parts.Length >= 6)
            {
                // 롱노트: endTime:hitSample 파싱
                string[] endParams = parts[5].Split(':');
                int endTime;
                if (int.TryParse(endParams[0].Trim(), out endTime))
                {
                    double endTiming = endTime / 1000.0;
                    NoteData note = new NoteData(timing, track, KeySoundType.None, true, endTiming);
                    note.CalculateBeatTiming(chart.bpm);
                    chart.AddNote(note);
                }
            }
            else
            {
                // 일반 노트
                NoteData note = new NoteData(timing, track, KeySoundType.None);
                note.CalculateBeatTiming(chart.bpm);
                chart.AddNote(note);
            }
        }

        chart.SortNotesByTime();
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

    #endregion

    /// <summary>
    /// osu!mania 모드인지 검증
    /// </summary>
    public static bool IsManiaMode(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool inGeneral = false;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line == "[General]")
                    {
                        inGeneral = true;
                        continue;
                    }

                    if (inGeneral)
                    {
                        if (line.StartsWith("["))
                            break; // General 섹션 끝

                        if (line.StartsWith("Mode:"))
                        {
                            string[] parts = line.Split(':');
                            if (parts.Length >= 2)
                            {
                                int mode;
                                if (int.TryParse(parts[1].Trim(), out mode))
                                {
                                    return mode == 3; // 3 = osu!mania
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false; // Mode를 찾지 못하면 false
    }
}
