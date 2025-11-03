using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// SongDatabase를 자동으로 생성하는 Unity Editor 도구
/// 
/// 사용법:
/// 1. Unity 메뉴: Tools → Synth → Create Sample SongDatabase
/// 2. 샘플 곡 3개가 자동으로 추가된 SongDatabase 생성
/// 
/// 생성 위치: Assets/Resources/SongDatabase.asset
/// </summary>
public class SongDatabaseBuilder : EditorWindow
{
    private int numberOfSongs = 3;
    private bool includeAlbumArt = false;
    private bool includeMultipleDifficulties = true;
    private bool includeMultipleKeyCounts = true;

    [MenuItem("Tools/Synth/Create Sample SongDatabase")]
    public static void ShowWindow()
    {
        SongDatabaseBuilder window = GetWindow<SongDatabaseBuilder>("SongDatabase Builder");
        window.minSize = new Vector2(450, 600);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("SongDatabase 자동 생성", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "샘플 SongDatabase를 자동으로 생성합니다.\n" +
            "테스트용 곡 데이터가 포함됩니다.",
            MessageType.Info
        );
        EditorGUILayout.Space();

        // 옵션 설정
        GUILayout.Label("생성 옵션:", EditorStyles.boldLabel);
        numberOfSongs = EditorGUILayout.IntSlider("곡 개수", numberOfSongs, 1, 10);
        includeAlbumArt = EditorGUILayout.Toggle("앨범 아트 포함", includeAlbumArt);
        includeMultipleDifficulties = EditorGUILayout.Toggle("다중 난이도 (Easy/Normal/Hard)", includeMultipleDifficulties);
        includeMultipleKeyCounts = EditorGUILayout.Toggle("다중 키 모드 (4K/6K/8K)", includeMultipleKeyCounts);

        EditorGUILayout.Space();

        // 생성될 내용 미리보기
        GUILayout.Label("생성될 내용:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            $"• 곡 개수: {numberOfSongs}개\n" +
            $"• 난이도: {(includeMultipleDifficulties ? "Easy, Normal, Hard (3개)" : "Normal (1개)")}\n" +
            $"• 키 모드: {(includeMultipleKeyCounts ? "4K, 6K, 8K (3개)" : "4K (1개)")}\n" +
            $"• 앨범 아트: {(includeAlbumArt ? "포함" : "미포함")}\n\n" +
            $"저장 위치: Assets/Resources/SongDatabase.asset",
            MessageType.None
        );

        EditorGUILayout.Space();

        // 생성 버튼
        if (GUILayout.Button("🚀 SongDatabase 생성", GUILayout.Height(40)))
        {
            CreateSongDatabase();
        }

        EditorGUILayout.Space();

        // 기존 데이터베이스 열기 버튼
        if (GUILayout.Button("📂 기존 SongDatabase 열기", GUILayout.Height(30)))
        {
            OpenExistingSongDatabase();
        }

        EditorGUILayout.Space();

        // 도움말
        EditorGUILayout.HelpBox(
            "생성 후 할 일:\n" +
            "1. Project 창에서 Assets/Resources/SongDatabase 확인\n" +
            "2. Inspector에서 곡 정보 수정\n" +
            "3. SongSelectionScene에서 SongDatabase 참조 연결",
            MessageType.Info
        );
    }

    private void CreateSongDatabase()
    {
        if (!EditorUtility.DisplayDialog(
            "SongDatabase 생성",
            $"샘플 SongDatabase를 생성하시겠습니까?\n\n" +
            $"곡 개수: {numberOfSongs}개\n" +
            $"저장 위치: Assets/Resources/SongDatabase.asset\n\n" +
            $"기존 파일이 있다면 덮어씁니다.",
            "생성",
            "취소"))
        {
            return;
        }

        try
        {
            // Resources 폴더 확인/생성
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                Debug.Log("✓ Resources 폴더 생성");
            }

            // SongDatabase 생성
            SongDatabase database = ScriptableObject.CreateInstance<SongDatabase>();

            // 기본 설정
            database.defaultDifficulty = "Normal";
            database.defaultKeyCount = 4;
            database.songs = new List<SongData>();

            // 샘플 곡 추가
            for (int i = 0; i < numberOfSongs; i++)
            {
                SongData song = CreateSampleSong(i);
                database.songs.Add(song);
            }

            // 에셋으로 저장
            string assetPath = "Assets/Resources/SongDatabase.asset";
            
            // 기존 파일 삭제
            if (AssetDatabase.LoadAssetAtPath<SongDatabase>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
                Debug.Log("✓ 기존 SongDatabase 삭제");
            }

            AssetDatabase.CreateAsset(database, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 생성된 에셋 선택
            EditorGUIUtility.PingObject(database);
            Selection.activeObject = database;

            Debug.Log($"=== SongDatabase 생성 완료! ===");
            Debug.Log($"✓ 곡 {numberOfSongs}개 추가");
            Debug.Log($"✓ 위치: {assetPath}");

            EditorUtility.DisplayDialog(
                "완료!",
                $"SongDatabase가 생성되었습니다!\n\n" +
                $"곡 개수: {numberOfSongs}개\n" +
                $"위치: {assetPath}\n\n" +
                $"Project 창에서 확인하세요.",
                "확인"
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SongDatabase 생성 중 오류: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("오류", $"생성 중 오류 발생:\n{e.Message}", "확인");
        }
    }

    private SongData CreateSampleSong(int index)
    {
        SongData song = new SongData();

        // 샘플 곡 이름 목록
        string[] sampleTitles = {
            "Synthesis", "Neon Dreams", "Digital Storm", "Cyber Pulse",
            "Electric Sky", "Data Flow", "Binary Star", "Circuit Breaker",
            "Quantum Leap", "Pixel Paradise"
        };

        string[] sampleArtists = {
            "Unknown Artist", "DJ Synth", "Electronic Dreams", "Digital Composer",
            "Neon Producer", "Cyber Artist", "Rhythm Master", "Beat Creator"
        };

        string[] genres = {
            "Electronic", "EDM", "Dubstep", "Drum & Bass", "Trance", "House", "Techno"
        };

        float[] bpmValues = { 120, 140, 150, 160, 180, 200 };

        // 기본 정보
        song.songId = $"song_{index:D3}";
        song.title = sampleTitles[index % sampleTitles.Length];
        song.artist = sampleArtists[index % sampleArtists.Length];

        // 음악 파일
        song.audioPath = "Audio/Songs/";
        song.audioFileName = $"{song.title.Replace(" ", "")}.ogg";
        song.previewStartTime = 30f + (index * 5f); // 30, 35, 40...

        // 비주얼 (선택사항)
        if (includeAlbumArt)
        {
            // 기본 Unity 스프라이트 사용
            song.albumArt = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        // 곡 정보
        song.bpm = bpmValues[index % bpmValues.Length];
        song.songLength = 120f + (index * 30f); // 120, 150, 180...
        song.genre = genres[index % genres.Length];
        song.description = $"A {song.genre} track with {song.bpm} BPM.";
        song.isLocked = false;

        // 지원하는 키 모드
        song.supportedKeyCounts = new List<int>();
        if (includeMultipleKeyCounts)
        {
            song.supportedKeyCounts.Add(4);
            song.supportedKeyCounts.Add(6);
            song.supportedKeyCounts.Add(8);
        }
        else
        {
            song.supportedKeyCounts.Add(4);
        }

        // 난이도 정보
        song.difficulties = new List<DifficultyInfo>();

        if (includeMultipleDifficulties)
        {
            // Easy
            song.difficulties.Add(CreateDifficulty(
                "Easy", 
                3f + index, 
                4, 
                300 + (index * 50), 
                new Color(0.3f, 1f, 0.3f), // 초록색
                $"{song.title.Replace(" ", "")}_Easy_4K.json"
            ));

            // Normal
            song.difficulties.Add(CreateDifficulty(
                "Normal", 
                6f + index, 
                4, 
                500 + (index * 100), 
                new Color(0.3f, 0.6f, 1f), // 파란색
                $"{song.title.Replace(" ", "")}_Normal_4K.json"
            ));

            // Hard
            song.difficulties.Add(CreateDifficulty(
                "Hard", 
                9f + index, 
                4, 
                800 + (index * 150), 
                new Color(1f, 0.3f, 0.3f), // 빨간색
                $"{song.title.Replace(" ", "")}_Hard_4K.json"
            ));
        }
        else
        {
            // Normal만
            song.difficulties.Add(CreateDifficulty(
                "Normal", 
                5f + index, 
                4, 
                500 + (index * 100), 
                new Color(0.3f, 0.6f, 1f),
                $"{song.title.Replace(" ", "")}_Normal_4K.json"
            ));
        }

        return song;
    }

    private DifficultyInfo CreateDifficulty(
        string name, 
        float level, 
        int keyCount, 
        int totalNotes, 
        Color color, 
        string chartFileName)
    {
        DifficultyInfo difficulty = new DifficultyInfo();
        difficulty.difficultyName = name;
        difficulty.level = level;
        difficulty.keyCount = keyCount;
        difficulty.totalNotes = totalNotes;
        difficulty.difficultyColor = color;
        difficulty.chartFileName = chartFileName;
        difficulty.chartPaths = new List<ChartPathInfo>();

        return difficulty;
    }

    private void OpenExistingSongDatabase()
    {
        string assetPath = "Assets/Resources/SongDatabase.asset";
        SongDatabase database = AssetDatabase.LoadAssetAtPath<SongDatabase>(assetPath);

        if (database != null)
        {
            EditorGUIUtility.PingObject(database);
            Selection.activeObject = database;
            Debug.Log($"✓ SongDatabase 열기: {assetPath}");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "파일 없음",
                $"SongDatabase를 찾을 수 없습니다.\n\n" +
                $"위치: {assetPath}\n\n" +
                $"먼저 생성 버튼을 눌러 생성하세요.",
                "확인"
            );
        }
    }
}

/// <summary>
/// SongDatabase Inspector 커스텀 에디터
/// </summary>
[CustomEditor(typeof(SongDatabase))]
public class SongDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SongDatabase database = (SongDatabase)target;

        // 기본 Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("데이터베이스 정보", EditorStyles.boldLabel);

        // 통계 표시
        EditorGUILayout.HelpBox(
            $"총 곡 수: {database.GetSongCount()}개\n" +
            $"잠금 해제된 곡: {database.GetUnlockedSongs().Count}개\n" +
            $"기본 난이도: {database.defaultDifficulty}\n" +
            $"기본 키 개수: {database.defaultKeyCount}K",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // 검증 버튼
        if (GUILayout.Button("🔍 데이터베이스 검증", GUILayout.Height(30)))
        {
            database.ValidateDatabase();
        }

        // 정렬 버튼들
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("정렬", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("제목순"))
        {
            database.SortSongs(SongSortType.Title);
            EditorUtility.SetDirty(database);
            Debug.Log("✓ 제목순 정렬 완료");
        }
        if (GUILayout.Button("아티스트순"))
        {
            database.SortSongs(SongSortType.Artist);
            EditorUtility.SetDirty(database);
            Debug.Log("✓ 아티스트순 정렬 완료");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("BPM순"))
        {
            database.SortSongs(SongSortType.BPM);
            EditorUtility.SetDirty(database);
            Debug.Log("✓ BPM순 정렬 완료");
        }
        if (GUILayout.Button("장르순"))
        {
            database.SortSongs(SongSortType.Genre);
            EditorUtility.SetDirty(database);
            Debug.Log("✓ 장르순 정렬 완료");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 곡 목록 미리보기
        if (database.songs.Count > 0)
        {
            EditorGUILayout.LabelField("곡 목록 미리보기", EditorStyles.boldLabel);
            
            for (int i = 0; i < Mathf.Min(5, database.songs.Count); i++)
            {
                SongData song = database.songs[i];
                EditorGUILayout.LabelField(
                    $"[{i}] {song.title}", 
                    $"{song.artist} - {song.bpm} BPM",
                    EditorStyles.helpBox
                );
            }

            if (database.songs.Count > 5)
            {
                EditorGUILayout.LabelField($"... 외 {database.songs.Count - 5}곡", EditorStyles.miniLabel);
            }
        }
    }
}
