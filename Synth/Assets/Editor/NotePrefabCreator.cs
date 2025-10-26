using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Note Prefab 자동 생성 도구
/// Unity 메뉴: Tools → Create Note Prefab
/// </summary>
public class NotePrefabCreator
{
    [MenuItem("Tools/Create Note Prefab")]
    static void CreateNotePrefab()
    {
        // Note GameObject 생성
        GameObject note = new GameObject("Note");
        
        // Sprite Renderer 추가
        SpriteRenderer sr = note.AddComponent<SpriteRenderer>();
        
        // 기본 흰색 스프라이트 생성
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
        sr.sprite = sprite;
        sr.color = new Color(0.2f, 0.8f, 1f, 1f); // 하늘색
        sr.sortingOrder = 10;
        
        // Transform 설정
        note.transform.localScale = new Vector3(0.9f, 0.2f, 1f);
        
        // Box Collider 2D 추가
        BoxCollider2D col = note.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
        col.isTrigger = true;
        
        // NoteController 추가 (있다면)
        note.AddComponent<NoteController>();
        
        // Prefabs 폴더 생성 (없으면)
        string folderPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Prefab 저장
        string prefabPath = folderPath + "/Note.prefab";
        PrefabUtility.SaveAsPrefabAsset(note, prefabPath);
        
        // 씬에서 제거
        Object.DestroyImmediate(note);
        
        Debug.Log($"✅ Note Prefab 생성 완료: {prefabPath}");
        AssetDatabase.Refresh();
        
        // Prefab 선택
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
    
    [MenuItem("Tools/Create Long Note Prefab")]
    static void CreateLongNotePrefab()
    {
        // Long Note GameObject 생성
        GameObject longNote = new GameObject("LongNote");
        
        // Head (시작 부분)
        GameObject head = new GameObject("Head");
        head.transform.SetParent(longNote.transform);
        head.transform.localPosition = Vector3.zero;
        
        SpriteRenderer headSr = head.AddComponent<SpriteRenderer>();
        Texture2D headTexture = new Texture2D(1, 1);
        headTexture.SetPixel(0, 0, Color.white);
        headTexture.Apply();
        Sprite headSprite = Sprite.Create(headTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
        headSr.sprite = headSprite;
        headSr.color = new Color(1f, 0.8f, 0.2f, 1f); // 골드색
        headSr.sortingOrder = 11;
        head.transform.localScale = new Vector3(0.9f, 0.2f, 1f);
        
        // Body (중간 연결 부분)
        GameObject body = new GameObject("Body");
        body.transform.SetParent(longNote.transform);
        body.transform.localPosition = new Vector3(0, -0.5f, 0);
        
        SpriteRenderer bodySr = body.AddComponent<SpriteRenderer>();
        Texture2D bodyTexture = new Texture2D(1, 1);
        bodyTexture.SetPixel(0, 0, Color.white);
        bodyTexture.Apply();
        Sprite bodySprite = Sprite.Create(bodyTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
        bodySr.sprite = bodySprite;
        bodySr.color = new Color(1f, 0.9f, 0.5f, 0.6f); // 반투명 골드
        bodySr.sortingOrder = 9;
        body.transform.localScale = new Vector3(0.8f, 1f, 1f);
        
        // Tail (끝 부분)
        GameObject tail = new GameObject("Tail");
        tail.transform.SetParent(longNote.transform);
        tail.transform.localPosition = new Vector3(0, -1f, 0);
        
        SpriteRenderer tailSr = tail.AddComponent<SpriteRenderer>();
        Texture2D tailTexture = new Texture2D(1, 1);
        tailTexture.SetPixel(0, 0, Color.white);
        tailTexture.Apply();
        Sprite tailSprite = Sprite.Create(tailTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
        tailSr.sprite = tailSprite;
        tailSr.color = new Color(1f, 0.8f, 0.2f, 1f); // 골드색
        tailSr.sortingOrder = 11;
        tail.transform.localScale = new Vector3(0.9f, 0.2f, 1f);
        
        // Box Collider 2D 추가 (Head에)
        BoxCollider2D col = head.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
        col.isTrigger = true;
        
        // NoteController 추가 (메인 GameObject에)
        longNote.AddComponent<NoteController>();
        
        // Prefabs 폴더 생성 (없으면)
        string folderPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Prefab 저장
        string prefabPath = folderPath + "/LongNote.prefab";
        PrefabUtility.SaveAsPrefabAsset(longNote, prefabPath);
        
        // 씬에서 제거
        Object.DestroyImmediate(longNote);
        
        Debug.Log($"✅ Long Note Prefab 생성 완료: {prefabPath}");
        AssetDatabase.Refresh();
        
        // Prefab 선택
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
}
#endif
