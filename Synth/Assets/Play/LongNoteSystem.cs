using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LongNoteSystem : MonoBehaviour
{
    [Header("References")]
    public GearController gearController;
    public ComboJudgmentDisplay comboJudgmentDisplay;
    public GearSettings settings;
    
    [Header("Long Note Settings")]
    public float tickInterval = 0.125f; // BPM 기준 8분음표
    public int tickScore = 100; // 틱당 점수
    public float tickHPGain = 0.5f; // 틱당 HP 회복
    
    private Dictionary<int, LongNoteData> activeLongNotes = new Dictionary<int, LongNoteData>();
    private float currentBPM = 120f;
    private int totalScore = 0;
    private int currentCombo = 0;
    
    public class LongNoteData
    {
        public GameObject noteObject;
        public GameObject noteBody; // 롱노트 몸통
        public GameObject noteEnd; // 롱노트 끝
        public int lineIndex;
        public float startTime;
        public float endTime;
        public float lastTickTime;
        public bool isHolding;
        public bool hasStarted;
        public Material bodyMaterial;
        public float originalLength;
        public Vector3 startPosition;
    }
    
    void Start()
    {
        UpdateTickInterval(currentBPM);
    }
    
    public void SetBPM(float bpm)
    {
        currentBPM = bpm;
        UpdateTickInterval(bpm);
    }
    
    void UpdateTickInterval(float bpm)
    {
        // BPM에 비례한 틱 간격 설정 (8분음표 기준)
        tickInterval = 60f / bpm / 2f; // 8분음표
    }
    
    public GameObject CreateLongNote(int lineIndex, float startTime, float endTime)
    {
        GameObject longNoteContainer = new GameObject($"LongNote_Line{lineIndex}");
        
        // 롱노트 시작 부분
        GameObject noteStart = CreateNoteHead(lineIndex, true);
        noteStart.transform.SetParent(longNoteContainer.transform);
        noteStart.name = "LongNoteStart";
        
        // 롱노트 몸통
        GameObject noteBody = CreateNoteBody(lineIndex, startTime, endTime);
        noteBody.transform.SetParent(longNoteContainer.transform);
        noteBody.name = "LongNoteBody";
        
        // 롱노트 끝 부분
        GameObject noteEnd = CreateNoteHead(lineIndex, false);
        noteEnd.transform.SetParent(longNoteContainer.transform);
        noteEnd.name = "LongNoteEnd";
        
        // 위치 설정
        Transform line = gearController.GetLine(lineIndex);
        if (line != null)
        {
            float noteSpeed = 5f; // NoteManager에서 가져와야 함
            float distance = (endTime - startTime) * noteSpeed;
            
            noteStart.transform.position = new Vector3(line.position.x, 10f, -0.1f);
            noteBody.transform.position = new Vector3(line.position.x, 10f + distance/2, -0.05f);
            noteEnd.transform.position = new Vector3(line.position.x, 10f + distance, -0.1f);
            
            // 몸통 크기 조정
            Vector3 bodyScale = noteBody.transform.localScale;
            bodyScale.y = distance;
            noteBody.transform.localScale = bodyScale;
        }
        
        // 롱노트 데이터 저장
        LongNoteData longNoteData = new LongNoteData
        {
            noteObject = longNoteContainer,
            noteBody = noteBody,
            noteEnd = noteEnd,
            lineIndex = lineIndex,
            startTime = startTime,
            endTime = endTime,
            lastTickTime = startTime,
            isHolding = false,
            hasStarted = false,
            bodyMaterial = noteBody.GetComponent<Renderer>().material,
            originalLength = noteBody.transform.localScale.y,
            startPosition = noteStart.transform.position
        };
        
        return longNoteContainer;
    }
    
    GameObject CreateNoteHead(int lineIndex, bool isStart)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Quad);
        
        float noteWidth = settings.lineWidth * settings.noteSize;
        head.transform.localScale = new Vector3(noteWidth, noteWidth * 0.3f, 1);
        
        Material mat = new Material(Shader.Find("Sprites/Default"));
        
        // 시작과 끝 부분 색상 구분
        if (isStart)
        {
            mat.color = new Color(0.3f, 1f, 0.8f, 1f); // 민트색
        }
        else
        {
            mat.color = new Color(1f, 0.3f, 0.8f, 1f); // 핑크색
        }
        
        head.GetComponent<Renderer>().material = mat;
        
        // 테두리 효과
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Quad);
        border.transform.SetParent(head.transform);
        border.transform.localPosition = new Vector3(0, 0, 0.01f);
        border.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        
        Material borderMat = new Material(Shader.Find("Sprites/Default"));
        borderMat.color = new Color(1f, 1f, 1f, 0.5f);
        border.GetComponent<Renderer>().material = borderMat;
        
        return head;
    }
    
    GameObject CreateNoteBody(int lineIndex, float startTime, float endTime)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Quad);
        
        float noteWidth = settings.lineWidth * settings.noteSize * 0.8f;
        body.transform.localScale = new Vector3(noteWidth, 1f, 1);
        
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.5f, 0.8f, 1f, 0.8f); // 반투명 하늘색
        body.GetComponent<Renderer>().material = mat;
        
        // 롱노트 몸통 패턴
        CreateBodyPattern(body);
        
        return body;
    }
    
    void CreateBodyPattern(GameObject body)
    {
        // 롱노트 몸통에 움직이는 패턴 추가
        GameObject pattern = GameObject.CreatePrimitive(PrimitiveType.Quad);
        pattern.transform.SetParent(body.transform);
        pattern.transform.localPosition = new Vector3(0, 0, -0.01f);
        pattern.transform.localScale = new Vector3(0.9f, 1f, 1);
        
        Material patternMat = new Material(Shader.Find("Sprites/Default"));
        patternMat.color = new Color(1f, 1f, 1f, 0.2f);
        pattern.GetComponent<Renderer>().material = patternMat;
        
        // 패턴 애니메이션 컴포넌트
        pattern.AddComponent<LongNotePattern>();
    }
    
    public void StartLongNote(int lineIndex, float currentTime)
    {
        if (!activeLongNotes.ContainsKey(lineIndex))
            return;
        
        LongNoteData longNote = activeLongNotes[lineIndex];
        
        if (!longNote.hasStarted)
        {
            // 시작 판정
            float offsetMs = (currentTime - longNote.startTime) * 1000f;
            JudgmentType startJudgment = CalculateJudgment(offsetMs);
            
            // 판정 표시
            comboJudgmentDisplay.ShowJudgment(startJudgment);
            
            if (startJudgment != JudgmentType.Miss)
            {
                longNote.hasStarted = true;
                longNote.isHolding = true;
                longNote.lastTickTime = currentTime;
                
                // 콤보 증가
                currentCombo++;
                comboJudgmentDisplay.UpdateCombo(currentCombo);
                
                // 점수 증가
                AddScore(GetScoreForJudgment(startJudgment));
                
                // 홀드 이펙트 시작
                StartCoroutine(LongNoteHoldEffect(longNote));
            }
            else
            {
                // 미스 처리
                HandleMiss();
            }
        }
    }
    
    public void ReleaseLongNote(int lineIndex, float currentTime)
    {
        if (!activeLongNotes.ContainsKey(lineIndex))
            return;
        
        LongNoteData longNote = activeLongNotes[lineIndex];
        
        if (longNote.isHolding)
        {
            longNote.isHolding = false;
            
            // 끝 판정
            float offsetMs = (currentTime - longNote.endTime) * 1000f;
            
            // 너무 일찍 놓은 경우
            if (currentTime < longNote.endTime - 0.2f) 
            {
                HandleMiss();
                comboJudgmentDisplay.ShowJudgment(JudgmentType.Miss);
            }
            else
            {
                JudgmentType endJudgment = CalculateJudgment(offsetMs);
                comboJudgmentDisplay.ShowJudgment(endJudgment);
                
                if (endJudgment != JudgmentType.Miss)
                {
                    currentCombo++;
                    comboJudgmentDisplay.UpdateCombo(currentCombo);
                    AddScore(GetScoreForJudgment(endJudgment));
                }
                else
                {
                    HandleMiss();
                }
            }
            
            // 롱노트 제거
            RemoveLongNote(lineIndex);
        }
    }
    
    void Update()
    {
        ProcessLongNoteTicks();
        UpdateLongNoteVisuals();
    }
    
    void ProcessLongNoteTicks()
    {
        float currentTime = Time.time;
        
        foreach (var kvp in activeLongNotes)
        {
            LongNoteData longNote = kvp.Value;
            
            if (longNote.isHolding && longNote.hasStarted)
            {
                // 틱 처리 (BPM에 비례)
                if (currentTime - longNote.lastTickTime >= tickInterval)
                {
                    longNote.lastTickTime = currentTime;
                    
                    // 롱노트 끝 지점 직전까지만 틱 처리
                    if (currentTime < longNote.endTime - tickInterval/2)
                    {
                        // 틱마다 S_Perfect 판정
                        ProcessTick(longNote);
                    }
                }
            }
        }
    }
    
    void ProcessTick(LongNoteData longNote)
    {
        // 틱 판정 (항상 S_Perfect)
        comboJudgmentDisplay.ShowJudgment(JudgmentType.S_Perfect);
        
        // 콤보 증가
        currentCombo++;
        comboJudgmentDisplay.UpdateCombo(currentCombo);
        
        // 점수 증가
        AddScore(tickScore);
        
        // HP 회복
        float currentHP = 80f; // 실제 HP 가져오기
        currentHP = Mathf.Min(100f, currentHP + tickHPGain);
        gearController.UpdateHP(currentHP);
        
        // 틱 이펙트
        CreateTickEffect(longNote.lineIndex);
    }
    
    void CreateTickEffect(int lineIndex)
    {
        Transform line = gearController.GetLine(lineIndex);
        if (line == null) return;
        
        GameObject tickEffect = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tickEffect.transform.position = new Vector3(line.position.x, settings.judgmentLineHeight, -0.2f);
        tickEffect.transform.localScale = new Vector3(settings.lineWidth, 0.2f, 1);
        
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.2f, 0.6f, 1f, 0.8f); // S_Perfect 색상
        tickEffect.GetComponent<Renderer>().material = mat;
        
        StartCoroutine(AnimateTickEffect(tickEffect));
    }
    
    IEnumerator AnimateTickEffect(GameObject effect)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Material mat = effect.GetComponent<Renderer>().material;
        Vector3 startPos = effect.transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 위로 올라가며 페이드아웃
            effect.transform.position = startPos + Vector3.up * t * 0.5f;
            effect.transform.localScale = new Vector3(settings.lineWidth * (1f + t), 0.2f * (1f - t * 0.5f), 1);
            
            Color c = mat.color;
            c.a = 0.8f * (1f - t);
            mat.color = c;
            
            yield return null;
        }
        
        Destroy(effect);
    }
    
    IEnumerator LongNoteHoldEffect(LongNoteData longNote)
    {
        while (longNote.isHolding)
        {
            // 홀딩 중 발광 효과
            float glow = Mathf.Sin(Time.time * 10f) * 0.2f + 0.8f;
            Color c = longNote.bodyMaterial.color;
            c.a = glow;
            longNote.bodyMaterial.color = c;
            
            yield return null;
        }
    }
    
    void UpdateLongNoteVisuals()
    {
        // 롱노트 이동 및 시각 업데이트
        foreach (var kvp in activeLongNotes)
        {
            LongNoteData longNote = kvp.Value;
            
            if (longNote.isHolding)
            {
                // 홀딩 중인 롱노트 몸통 축소
                float consumed = (Time.time - longNote.startTime) / (longNote.endTime - longNote.startTime);
                consumed = Mathf.Clamp01(consumed);
                
                Vector3 bodyScale = longNote.noteBody.transform.localScale;
                bodyScale.y = longNote.originalLength * (1f - consumed);
                longNote.noteBody.transform.localScale = bodyScale;
            }
        }
    }
    
    public void RegisterLongNote(int lineIndex, LongNoteData longNoteData)
    {
        if (!activeLongNotes.ContainsKey(lineIndex))
        {
            activeLongNotes[lineIndex] = longNoteData;
        }
    }
    
    void RemoveLongNote(int lineIndex)
    {
        if (activeLongNotes.ContainsKey(lineIndex))
        {
            LongNoteData longNote = activeLongNotes[lineIndex];
            if (longNote.noteObject != null)
            {
                Destroy(longNote.noteObject);
            }
            activeLongNotes.Remove(lineIndex);
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
    
    void AddScore(int score)
    {
        totalScore += score;
        gearController.UpdateScore(totalScore);
    }
    
    void HandleMiss()
    {
        currentCombo = 0;
        comboJudgmentDisplay.UpdateCombo(0, true);
        
        // HP 감소
        float currentHP = 80f; // 실제 HP 가져오기
        currentHP = Mathf.Max(0, currentHP - 5f);
        gearController.UpdateHP(currentHP);
    }
}

// 롱노트 패턴 애니메이션 컴포넌트
public class LongNotePattern : MonoBehaviour
{
    private Material material;
    private float scrollSpeed = 2f;
    
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }
    
    void Update()
    {
        // UV 스크롤로 패턴 움직임 효과
        if (material != null)
        {
            Vector2 offset = material.mainTextureOffset;
            offset.y += scrollSpeed * Time.deltaTime;
            material.mainTextureOffset = offset;
        }
    }
}