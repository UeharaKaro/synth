using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SongSelectionScene을 자동으로 설정하는 Unity Editor 도구
/// 
/// 사용법:
/// 1. SongSelectionScene 열기
/// 2. Unity 메뉴: Tools → Synth → Setup SongSelection Scene
/// 3. SongSelectionManager 자동 생성 및 SongDatabase 연결
/// </summary>
public class SongSelectionSceneBuilder : EditorWindow
{
    private SongDatabase songDatabase;
    private bool autoLoadDatabase = true;

    [MenuItem("Tools/Synth/Setup SongSelection Scene")]
    public static void ShowWindow()
    {
        SongSelectionSceneBuilder window = GetWindow<SongSelectionSceneBuilder>("SongSelection Setup");
        window.minSize = new Vector2(400, 400);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("SongSelectionScene 자동 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "SongSelectionScene에 SongSelectionManager를 추가하고\n" +
            "SongDatabase를 자동으로 연결합니다.",
            MessageType.Info
        );
        EditorGUILayout.Space();

        // SongDatabase 선택
        GUILayout.Label("SongDatabase:", EditorStyles.boldLabel);
        songDatabase = (SongDatabase)EditorGUILayout.ObjectField(
            "Song Database", 
            songDatabase, 
            typeof(SongDatabase), 
            false
        );

        autoLoadDatabase = EditorGUILayout.Toggle("자동으로 Database 찾기", autoLoadDatabase);

        EditorGUILayout.Space();

        // 현재 상태 표시
        if (songDatabase != null)
        {
            EditorGUILayout.HelpBox(
                $"✓ SongDatabase 발견!\n" +
                $"  이름: {songDatabase.name}\n" +
                $"  곡 개수: {songDatabase.GetSongCount()}개",
                MessageType.Info
            );
        }
        else if (autoLoadDatabase)
        {
            EditorGUILayout.HelpBox(
                "⚠ SongDatabase를 찾을 수 없습니다.\n" +
                "먼저 Tools → Synth → Create Sample SongDatabase를 실행하세요.",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();

        // 설정 버튼
        GUI.enabled = songDatabase != null || autoLoadDatabase;
        if (GUILayout.Button("🚀 SongSelectionManager 설정", GUILayout.Height(40)))
        {
            SetupSongSelectionScene();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // 도움말
        EditorGUILayout.HelpBox(
            "설정 후 할 일:\n" +
            "1. Hierarchy에서 SongSelectionManager 확인\n" +
            "2. Inspector에서 Song Database 연결 확인\n" +
            "3. UI 텍스트 참조 연결 (필요시)\n" +
            "4. Play 모드 테스트",
            MessageType.Info
        );
    }

    private void SetupSongSelectionScene()
    {
        // SongDatabase 자동 로드
        if (songDatabase == null && autoLoadDatabase)
        {
            songDatabase = LoadSongDatabase();
        }

        if (songDatabase == null)
        {
            EditorUtility.DisplayDialog(
                "오류",
                "SongDatabase를 찾을 수 없습니다!\n\n" +
                "먼저 Tools → Synth → Create Sample SongDatabase를\n" +
                "실행하여 SongDatabase를 생성하세요.",
                "확인"
            );
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "SongSelectionScene 설정",
            $"SongSelectionManager를 설정하시겠습니까?\n\n" +
            $"사용할 SongDatabase: {songDatabase.name}\n" +
            $"곡 개수: {songDatabase.GetSongCount()}개",
            "설정",
            "취소"))
        {
            return;
        }

        try
        {
            Debug.Log("=== SongSelectionScene 설정 시작 ===");

            // 1. 기존 SongSelectionManager 찾기
            SongSelectionManager existingManager = FindObjectOfType<SongSelectionManager>();

            GameObject managerObj;
            SongSelectionManager manager;

            if (existingManager != null)
            {
                Debug.Log($"✓ 기존 SongSelectionManager 발견: {existingManager.name}");
                managerObj = existingManager.gameObject;
                manager = existingManager;
            }
            else
            {
                // 2. 새로 생성
                managerObj = new GameObject("SongSelectionManager");
                manager = managerObj.AddComponent<SongSelectionManager>();
                Debug.Log("✓ SongSelectionManager 생성 완료");
            }

            // 3. SongDatabase 연결
            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty databaseProp = serializedManager.FindProperty("songDatabase");
            
            if (databaseProp != null)
            {
                databaseProp.objectReferenceValue = songDatabase;
                serializedManager.ApplyModifiedProperties();
                Debug.Log($"✓ SongDatabase 연결 완료: {songDatabase.name}");
            }
            else
            {
                Debug.LogWarning("⚠ songDatabase 필드를 찾을 수 없습니다. 수동으로 연결하세요.");
            }

            // 4. UI 요소 자동 찾기 (옵션)
            AutoConnectUIElements(manager);

            // 씬 저장
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            // 생성된 오브젝트 선택
            Selection.activeGameObject = managerObj;
            EditorGUIUtility.PingObject(managerObj);

            Debug.Log("=== SongSelectionScene 설정 완료! ===");

            EditorUtility.DisplayDialog(
                "완료!",
                $"SongSelectionManager 설정 완료!\n\n" +
                $"✓ SongDatabase 연결: {songDatabase.name}\n" +
                $"✓ 곡 개수: {songDatabase.GetSongCount()}개\n\n" +
                $"Hierarchy에서 SongSelectionManager를 확인하세요.",
                "확인"
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"설정 중 오류: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("오류", $"설정 중 오류 발생:\n{e.Message}", "확인");
        }
    }

    private SongDatabase LoadSongDatabase()
    {
        // Resources 폴더에서 로드
        string[] guids = AssetDatabase.FindAssets("t:SongDatabase");
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            SongDatabase db = AssetDatabase.LoadAssetAtPath<SongDatabase>(path);
            Debug.Log($"✓ SongDatabase 자동 로드: {path}");
            return db;
        }

        Debug.LogWarning("⚠ SongDatabase를 찾을 수 없습니다.");
        return null;
    }

    private void AutoConnectUIElements(SongSelectionManager manager)
    {
        SerializedObject serializedManager = new SerializedObject(manager);

        // songTitleText
        TryConnectUIElement<TextMeshProUGUI>(
            serializedManager, 
            "songTitleText", 
            new string[] { "SongTitle", "TitleText", "Title" }
        );

        // artistText
        TryConnectUIElement<TextMeshProUGUI>(
            serializedManager, 
            "artistText", 
            new string[] { "Artist", "ArtistText", "ArtistName" }
        );

        // keyCountText
        TryConnectUIElement<TextMeshProUGUI>(
            serializedManager, 
            "keyCountText", 
            new string[] { "KeyCount", "KeyCountText", "Keys" }
        );

        // difficultyText
        TryConnectUIElement<TextMeshProUGUI>(
            serializedManager, 
            "difficultyText", 
            new string[] { "Difficulty", "DifficultyText", "Diff" }
        );

        // selectSongButton
        TryConnectUIElement<Button>(
            serializedManager, 
            "selectSongButton", 
            new string[] { "SelectButton", "SelectSong", "PlayButton", "StartButton" }
        );

        serializedManager.ApplyModifiedProperties();
    }

    private void TryConnectUIElement<T>(SerializedObject obj, string fieldName, string[] searchNames) where T : Component
    {
        SerializedProperty prop = obj.FindProperty(fieldName);
        if (prop == null || prop.objectReferenceValue != null)
            return;

        T[] allComponents = FindObjectsOfType<T>();
        foreach (string searchName in searchNames)
        {
            foreach (T component in allComponents)
            {
                if (component.name.Contains(searchName))
                {
                    prop.objectReferenceValue = component;
                    Debug.Log($"✓ UI 자동 연결: {fieldName} → {component.name}");
                    return;
                }
            }
        }

        Debug.LogWarning($"⚠ UI 요소를 찾을 수 없음: {fieldName}");
    }
}
