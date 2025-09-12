using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NoteManager : MonoBehaviour
{
    [Header("References")]
    public GearController gearController;
    public GearSettings settings;
    
    [Header("Note Prefab")]
    public GameObject notePrefab;
    
    [Header("Note Settings")]
    public float noteSpeed = 5f;
    public float spawnHeight = 10f;
    
    private Queue<Note> notePool = new Queue<Note>();
    private List<Note> activeNotes = new List<Note>();
    private Dictionary<int, Queue<NoteData>> upcomingNotes = new Dictionary<int, Queue<NoteData>>();
    
    [System.Serializable]
    public class NoteData
    {
        public int lineIndex;
        public float time;
        public NoteType type;
    }
    
    public enum NoteType
    {
        Normal,
        Long,
        Slide
    }
    
    private class Note
    {
        public GameObject gameObject;
        public Renderer renderer;
        public NoteData data;
        public float spawnTime;
        public bool isActive;
        public int lineIndex;
    }
    
    void Start()
    {
        InitializeNotePool();
        SetupNoteLanes();
    }
    
    void InitializeNotePool()
    {
        // 노트 풀 생성
        for (int i = 0; i < 100; i++)
        {
            Note note = CreateNote();
            note.gameObject.SetActive(false);
            notePool.Enqueue(note);
        }
    }
    
    void SetupNoteLanes()
    {
        // 각 라인별 노트 큐 초기화
        for (int i = 0; i < settings.lineCount; i++)
        {
            upcomingNotes[i] = new Queue<NoteData>();
        }
    }
    
    Note CreateNote()
    {
        GameObject noteObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        noteObj.name = "Note";
        noteObj.transform.SetParent(transform);
        
        // 노트 크기 설정
        float noteWidth = settings.lineWidth * settings.noteSize;
        noteObj.transform.localScale = new Vector3(noteWidth, noteWidth * 0.3f, 1);
        
        // 노트 머티리얼 설정
        Material noteMat = new Material(Shader.Find("Sprites/Default"));
        noteMat.color = GetNoteColor();
        Renderer rend = noteObj.GetComponent<Renderer>();
        rend.material = noteMat;
        
        // 노트 콜라이더 추가
        BoxCollider2D collider = noteObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        Note note = new Note
        {
            gameObject = noteObj,
            renderer = rend,
            isActive = false
        };
        
        // 노트 비주얼 효과 추가
        AddNoteEffects(noteObj);
        
        return note;
    }
    
    Color GetNoteColor()
    {
        // 라인 개수에 따른 노트 색상 (2번째 이미지 참고)
        switch (settings.lineCount)
        {
            case 4:
                return new Color(0.3f, 0.8f, 1f, 1f); // 하늘색
            case 5:
                return new Color(1f, 0.3f, 0.8f, 1f); // 핑크
            case 6:
                return new Color(0.8f, 0.3f, 1f, 1f); // 보라
            case 8:
                return new Color(0.3f, 1f, 0.8f, 1f); // 민트
            default:
                return new Color(0.5f, 0.5f, 1f, 1f); // 기본 파란색
        }
    }
    
    void AddNoteEffects(GameObject noteObj)
    {
        // 노트 테두리 효과
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Quad);
        border.name = "NoteBorder";
        border.transform.SetParent(noteObj.transform);
        border.transform.localPosition = new Vector3(0, 0, 0.01f);
        border.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        
        Material borderMat = new Material(Shader.Find("Sprites/Default"));
        borderMat.color = new Color(1f, 1f, 1f, 0.5f);
        border.GetComponent<Renderer>().material = borderMat;
        
        // 노트 글로우 효과
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glow.name = "NoteGlow";
        glow.transform.SetParent(noteObj.transform);
        glow.transform.localPosition = new Vector3(0, 0, 0.02f);
        glow.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        
        Material glowMat = new Material(Shader.Find("Sprites/Default"));
        glowMat.color = new Color(1f, 1f, 1f, 0.2f);
        glow.GetComponent<Renderer>().material = glowMat;
    }
    
    public void SpawnNote(int lineIndex, float hitTime, NoteType type = NoteType.Normal)
    {
        if (lineIndex < 0 || lineIndex >= settings.lineCount)
            return;
        
        NoteData noteData = new NoteData
        {
            lineIndex = lineIndex,
            time = hitTime,
            type = type
        };
        
        upcomingNotes[lineIndex].Enqueue(noteData);
    }
    
    void Update()
    {
        CheckAndSpawnNotes();
        UpdateActiveNotes();
    }
    
    void CheckAndSpawnNotes()
    {
        float currentTime = Time.time;
        float spawnLookAhead = spawnHeight / noteSpeed;
        
        for (int i = 0; i < settings.lineCount; i++)
        {
            if (upcomingNotes[i].Count > 0)
            {
                NoteData nextNote = upcomingNotes[i].Peek();
                
                if (currentTime >= nextNote.time - spawnLookAhead)
                {
                    upcomingNotes[i].Dequeue();
                    SpawnNoteObject(nextNote);
                }
            }
        }
    }
    
    void SpawnNoteObject(NoteData noteData)
    {
        Note note = GetPooledNote();
        if (note == null)
            return;
        
        // 노트 위치 설정
        Transform line = gearController.GetLine(noteData.lineIndex);
        if (line == null)
            return;
        
        Vector3 spawnPos = line.position;
        spawnPos.y = spawnHeight;
        spawnPos.z = -0.1f;
        
        note.gameObject.transform.position = spawnPos;
        note.data = noteData;
        note.spawnTime = Time.time;
        note.isActive = true;
        note.lineIndex = noteData.lineIndex;
        
        // 노트 타입별 비주얼 설정
        SetNoteVisual(note, noteData.type);
        
        note.gameObject.SetActive(true);
        activeNotes.Add(note);
    }
    
    void SetNoteVisual(Note note, NoteType type)
    {
        Color baseColor = GetNoteColor();
        
        switch (type)
        {
            case NoteType.Normal:
                note.renderer.material.color = baseColor;
                break;
            case NoteType.Long:
                baseColor.g += 0.2f;
                note.renderer.material.color = baseColor;
                break;
            case NoteType.Slide:
                baseColor.b += 0.2f;
                note.renderer.material.color = baseColor;
                break;
        }
    }
    
    void UpdateActiveNotes()
    {
        float judgmentY = gearController.GetJudgmentLineY();
        
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            if (!note.isActive)
                continue;
            
            // 노트 이동
            Vector3 pos = note.gameObject.transform.position;
            pos.y -= noteSpeed * Time.deltaTime;
            note.gameObject.transform.position = pos;
            
            // 판정선 통과 체크
            if (pos.y < judgmentY - 1f)
            {
                // 미스 처리
                OnNoteMiss(note);
                ReturnNote(note);
                activeNotes.RemoveAt(i);
            }
        }
    }
    
    public void ProcessNoteHit(int lineIndex, float hitTime)
    {
        Note closestNote = null;
        float closestDistance = float.MaxValue;
        float judgmentY = gearController.GetJudgmentLineY();
        
        // 해당 라인의 가장 가까운 노트 찾기
        foreach (Note note in activeNotes)
        {
            if (note.lineIndex == lineIndex && note.isActive)
            {
                float distance = Mathf.Abs(note.gameObject.transform.position.y - judgmentY);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }
        
        if (closestNote != null && closestDistance < 1f)
        {
            // 판정 계산
            float offsetMs = (closestNote.gameObject.transform.position.y - judgmentY) * 1000f / noteSpeed;
            JudgmentType judgment = CalculateJudgment(offsetMs);
            
            // 판정 결과 전달
            OnNoteHit(closestNote, judgment, offsetMs);
            
            // 노트 제거
            activeNotes.Remove(closestNote);
            ReturnNote(closestNote);
        }
    }
    
    JudgmentType CalculateJudgment(float offsetMs)
    {
        float absOffset = Mathf.Abs(offsetMs);
        
        if (absOffset <= 15f) return JudgmentType.S_Perfect;
        if (absOffset <= 30f) return JudgmentType.Perfect;
        if (absOffset <= 50f) return JudgmentType.Great;
        if (absOffset <= 80f) return JudgmentType.Good;
        if (absOffset <= 120f) return JudgmentType.Bad;
        
        return JudgmentType.Miss;
    }
    
    void OnNoteHit(Note note, JudgmentType judgment, float offsetMs)
    {
        // 판정 이펙트 생성
        CreateHitEffect(note.gameObject.transform.position, judgment);
        
        // 점수 및 콤보 업데이트
        int score = GetScoreForJudgment(judgment);
        gearController.UpdateScore(score);
        
        if (judgment != JudgmentType.Miss)
        {
            gearController.UpdateCombo(1);
        }
        else
        {
            gearController.UpdateCombo(0);
        }
        
        // 판정 오프셋 표시
        gearController.ShowJudgmentOffset(judgment, offsetMs);
        
        // HP 업데이트
        float hpChange = GetHPChangeForJudgment(judgment);
        // HP 시스템과 연동 필요
    }
    
    void OnNoteMiss(Note note)
    {
        gearController.UpdateCombo(0);
        gearController.ShowJudgmentOffset(JudgmentType.Miss, 200f);
        // HP 감소 처리
    }
    
    int GetScoreForJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return 1000;
            case JudgmentType.Perfect: return 800;
            case JudgmentType.Great: return 500;
            case JudgmentType.Good: return 300;
            case JudgmentType.Bad: return 100;
            default: return 0;
        }
    }
    
    float GetHPChangeForJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect: return 2f;
            case JudgmentType.Perfect: return 1.5f;
            case JudgmentType.Great: return 1f;
            case JudgmentType.Good: return 0.5f;
            case JudgmentType.Bad: return -1f;
            case JudgmentType.Miss: return -3f;
            default: return 0f;
        }
    }
    
    void CreateHitEffect(Vector3 position, JudgmentType judgment)
    {
        // 판정별 히트 이펙트 생성
        StartCoroutine(ShowHitEffect(position, judgment));
    }
    
    IEnumerator ShowHitEffect(Vector3 position, JudgmentType judgment)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Quad);
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * 0.5f;
        
        Material effectMat = new Material(Shader.Find("Sprites/Default"));
        
        // 판정 색상 가져오기
        Color judgmentColor = Color.white;
        switch (judgment)
        {
            case JudgmentType.S_Perfect:
                judgmentColor = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
                break;
            case JudgmentType.Perfect:
                judgmentColor = new Color(0.2f, 1f, 0.4f, 1f); // 초록색
                break;
            case JudgmentType.Great:
            case JudgmentType.Good:
            case JudgmentType.Bad:
                judgmentColor = new Color(1f, 0.9f, 0.2f, 1f); // 노란색
                break;
            case JudgmentType.Miss:
                judgmentColor = new Color(1f, 0.2f, 0.2f, 1f); // 빨간색
                break;
        }
        
        effectMat.color = judgmentColor;
        effect.GetComponent<Renderer>().material = effectMat;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            effect.transform.localScale = Vector3.one * (0.5f + t * 1.5f);
            Color c = effectMat.color;
            c.a = 1f - t;
            effectMat.color = c;
            
            yield return null;
        }
        
        Destroy(effect);
    }
    
    Note GetPooledNote()
    {
        if (notePool.Count > 0)
        {
            return notePool.Dequeue();
        }
        return CreateNote();
    }
    
    void ReturnNote(Note note)
    {
        note.gameObject.SetActive(false);
        note.isActive = false;
        notePool.Enqueue(note);
    }
}