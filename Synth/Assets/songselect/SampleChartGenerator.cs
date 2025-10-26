using UnityEngine;
using System.IO;

/// <summary>
/// 테스트용 샘플 차트를 생성하는 유틸리티
/// Unity 메뉴: Tools → Create Sample Charts
/// </summary>
public class SampleChartGenerator
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Create Sample Charts")]
    public static void CreateSampleCharts()
    {
        string chartsPath = Path.Combine(Application.streamingAssetsPath, "Charts");
        
        // Charts 폴더 생성
        if (!Directory.Exists(chartsPath))
        {
            Directory.CreateDirectory(chartsPath);
            Debug.Log($"Charts 폴더 생성: {chartsPath}");
        }

        // 샘플 곡 3개 생성
        CreateSampleSong("Sample Song 1", "Artist A", "sample_song_1.wav", 120f, chartsPath);
        CreateSampleSong("Sample Song 2", "Artist B", "sample_song_2.wav", 140f, chartsPath);
        CreateSampleSong("Sample Song 3", "Artist C", "sample_song_3.wav", 160f, chartsPath);

        Debug.Log("샘플 차트 생성 완료! StreamingAssets/Charts/ 폴더를 확인하세요.");
        UnityEditor.AssetDatabase.Refresh();
    }

    private static void CreateSampleSong(string songName, string artistName, string audioFileName, float bpm, string chartsPath)
    {
        // 각 곡에 대해 여러 난이도와 키 모드 생성
        string[] difficulties = { "Easy", "Normal", "Hard" };
        int[] keyCounts = { 4, 6 };
        int[] levels = { 1, 3, 5 };

        for (int i = 0; i < difficulties.Length; i++)
        {
            string difficulty = difficulties[i];
            int level = levels[i];

            foreach (int keyCount in keyCounts)
            {
                CreateChart(songName, artistName, audioFileName, bpm, difficulty, keyCount, level, chartsPath);
            }
        }
    }

    private static void CreateChart(string songName, string artistName, string audioFileName, 
        float bpm, string difficulty, int keyCount, int level, string chartsPath)
    {
        ChartData chart = new ChartData
        {
            songName = songName,
            artistName = artistName,
            audioFileName = audioFileName,
            coverImageFileName = audioFileName.Replace(".wav", ".png"),
            bpm = bpm,
            offset = 0f,
            difficulty = difficulty,
            keyCount = keyCount,
            level = level
        };

        // 간단한 노트 패턴 생성 (테스트용)
        int noteCount = level * 30; // 난이도에 따라 노트 수 증가
        double currentTime = 2.0; // 2초부터 시작
        double beatInterval = 60.0 / bpm; // 1비트 간격 (초)

        for (int i = 0; i < noteCount; i++)
        {
            NoteData note = new NoteData
            {
                timing = currentTime,
                track = Random.Range(0, keyCount),
                isLongNote = Random.value < 0.2f, // 20% 확률로 롱노트
                keySoundType = KeySoundType.None
            };

            if (note.isLongNote)
            {
                note.longNoteEndTiming = currentTime + beatInterval * Random.Range(2, 5);
            }

            chart.notes.Add(note);

            // 다음 노트 타이밍 (난이도에 따라 간격 조정)
            double interval = beatInterval / (level * 0.5);
            currentTime += interval;
        }

        chart.SortNotesByTime();

        // 파일명 생성 (예: sample_song_1_easy_4k.json)
        string fileName = $"{songName.Replace(" ", "_").ToLower()}_{difficulty.ToLower()}_{keyCount}k.json";
        string filePath = Path.Combine(chartsPath, fileName);

        // JSON 저장
        string json = chart.ToJson();
        File.WriteAllText(filePath, json);

        Debug.Log($"차트 생성: {fileName} ({chart.notes.Count}개 노트)");
    }
#endif
}
