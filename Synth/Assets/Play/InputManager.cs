using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public GearController gearController;
    public NoteManager noteManager;
    public GearSettings settings;
    public LongNoteSystem longNoteSystem;
    
    [Header("Key Bindings")]
    public List<KeyCode> lineKeys = new List<KeyCode>();
    
    private Dictionary<int, KeyCode> keyBindings = new Dictionary<int, KeyCode>();
    private Dictionary<int, bool> keyPressed = new Dictionary<int, bool>();
    private Dictionary<int, float> keyPressTime = new Dictionary<int, float>();
    
    void Start()
    {
        FindReferences();
        SetupDefaultKeyBindings();
    }
    
    void FindReferences()
    {
        // GearController 찾기
        if (gearController == null)
        {
            gearController = FindObjectOfType<GearController>();
            if (gearController == null)
            {
                Debug.LogError("InputManager: GearController를 찾을 수 없습니다!");
                return;
            }
        }
        
        // GearSettings 가져오기
        if (settings == null && gearController != null)
        {
            settings = gearController.settings;
            if (settings == null)
            {
                Debug.LogError("InputManager: GearSettings를 찾을 수 없습니다!");
                return;
            }
        }
        
        // NoteManager 찾기 (선택사항)
        if (noteManager == null)
        {
            noteManager = FindObjectOfType<NoteManager>();
        }
        
        // LongNoteSystem 찾기 (선택사항)
        if (longNoteSystem == null)
        {
            longNoteSystem = FindObjectOfType<LongNoteSystem>();
        }
        
        Debug.Log($"InputManager: 초기화 완료 (Line Count: {settings.lineCount})");
    }
    
    void SetupDefaultKeyBindings()
    {
        if (settings == null)
        {
            Debug.LogWarning("InputManager: settings가 null입니다. 키 바인딩을 설정할 수 없습니다.");
            return;
        }
        
        // 라인 개수에 따른 기본 키 설정
        switch (settings.lineCount)
        {
            case 4:
                SetupKeys(new KeyCode[] { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K });
                break;
            case 5:
                SetupKeys(new KeyCode[] { KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K });
                break;
            case 6:
                SetupKeys(new KeyCode[] { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L });
                break;
            case 7:
                SetupKeys(new KeyCode[] { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.Space, KeyCode.J, KeyCode.K, KeyCode.L });
                break;
            case 8:
                SetupKeys(new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon });
                break;
            case 10:
                SetupKeys(new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, 
                                         KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon });
                break;
        }
    }
    
    void SetupKeys(KeyCode[] keys)
    {
        lineKeys.Clear();
        keyBindings.Clear();
        keyPressed.Clear();
        keyPressTime.Clear();
        
        for (int i = 0; i < keys.Length && i < settings.lineCount; i++)
        {
            lineKeys.Add(keys[i]);
            keyBindings[i] = keys[i];
            keyPressed[i] = false;
            keyPressTime[i] = 0f;
        }
    }
    
    void Update()
    {
        ProcessInput();
    }
    
    void ProcessInput()
    {
        if (settings == null)
        {
            return;
        }
        
        for (int i = 0; i < settings.lineCount; i++)
        {
            if (!keyBindings.ContainsKey(i))
                continue;
            
            KeyCode key = keyBindings[i];
            
            // 키 눌림 체크
            if (Input.GetKeyDown(key))
            {
                OnKeyPressed(i);
            }
            
            // 키 뗌 체크
            if (Input.GetKeyUp(key))
            {
                OnKeyReleased(i);
            }
            
            // 키 홀드 체크 (롱노트용)
            if (Input.GetKey(key))
            {
                OnKeyHold(i);
            }
        }
        
        // 추가 입력 처리 (ESC, 일시정지 등)
        ProcessSystemInput();
    }
    
    void OnKeyPressed(int lineIndex)
    {
        keyPressed[lineIndex] = true;
        keyPressTime[lineIndex] = Time.time;
        
        // 일반 노트 히트 처리
        noteManager.ProcessNoteHit(lineIndex, Time.time);
        
        // 롱노트 시작 처리
        if (longNoteSystem != null)
        {
            longNoteSystem.StartLongNote(lineIndex, Time.time);
        }
        
        // 키 프레스 이펙트
        ShowKeyPressEffect(lineIndex);
        
        // 사운드 피드백
        PlayHitSound(lineIndex);
    }
    
    void OnKeyReleased(int lineIndex)
    {
        keyPressed[lineIndex] = false;
        
        // 롱노트 릴리즈 처리
        ProcessLongNoteRelease(lineIndex);
        
        // 키 릴리즈 이펙트
        HideKeyPressEffect(lineIndex);
    }
    
    void OnKeyHold(int lineIndex)
    {
        // 롱노트 홀드 처리
        float holdTime = Time.time - keyPressTime[lineIndex];
        ProcessLongNoteHold(lineIndex, holdTime);
    }
    
    void ProcessLongNoteRelease(int lineIndex)
    {
        // 롱노트 종료 처리 로직
        // NoteManager와 연동하여 처리
    }
    
    void ProcessLongNoteHold(int lineIndex, float holdTime)
    {
        // 롱노트 홀드 중 처리 로직
        // HP 회복, 점수 증가 등
    }
    
    void ShowKeyPressEffect(int lineIndex)
    {
        Transform line = gearController.GetLine(lineIndex);
        if (line == null)
            return;
        
        // 라인 하이라이트 효과
        StartCoroutine(LineHighlightEffect(line));
        
        // 판정선 부근 이펙트
        CreatePressEffect(line.position.x, gearController.GetJudgmentLineY());
    }
    
    void HideKeyPressEffect(int lineIndex)
    {
        Transform line = gearController.GetLine(lineIndex);
        if (line == null)
            return;
        
        // 하이라이트 제거
        ResetLineHighlight(line);
    }
    
    System.Collections.IEnumerator LineHighlightEffect(Transform line)
    {
        Renderer lineRenderer = line.GetComponent<Renderer>();
        if (lineRenderer == null)
            yield break;
        
        Color originalColor = lineRenderer.material.color;
        Color highlightColor = new Color(originalColor.r + 0.3f, originalColor.g + 0.3f, originalColor.b + 0.5f, 1f);
        
        lineRenderer.material.color = highlightColor;
        
        float duration = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            lineRenderer.material.color = Color.Lerp(highlightColor, originalColor, t);
            yield return null;
        }
        
        lineRenderer.material.color = originalColor;
    }
    
    void ResetLineHighlight(Transform line)
    {
        Renderer lineRenderer = line.GetComponent<Renderer>();
        if (lineRenderer != null)
        {
            lineRenderer.material.color = settings.lineColor;
        }
    }
    
    void CreatePressEffect(float xPos, float yPos)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Quad);
        effect.name = "PressEffect";
        effect.transform.position = new Vector3(xPos, yPos, -0.15f);
        effect.transform.localScale = new Vector3(settings.lineWidth * 0.8f, 0.3f, 1f);
        
        Material effectMat = new Material(Shader.Find("Sprites/Default"));
        effectMat.color = new Color(1f, 1f, 1f, 0.5f);
        effect.GetComponent<Renderer>().material = effectMat;
        
        // 이펙트 애니메이션
        StartCoroutine(AnimatePressEffect(effect));
    }
    
    System.Collections.IEnumerator AnimatePressEffect(GameObject effect)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 originalScale = effect.transform.localScale;
        Material mat = effect.GetComponent<Renderer>().material;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 스케일 확대
            effect.transform.localScale = originalScale * (1f + t * 0.5f);
            
            // 페이드 아웃
            Color c = mat.color;
            c.a = 0.5f * (1f - t);
            mat.color = c;
            
            yield return null;
        }
        
        Destroy(effect);
    }
    
    void PlayHitSound(int lineIndex)
    {
        // 히트 사운드 재생
        // AudioManager와 연동 필요
    }
    
    void ProcessSystemInput()
    {
        // ESC - 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        
        // R - 리스타트
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartSong();
        }
        
        // Tab - 설정 열기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenSettings();
        }
    }
    
    void PauseGame()
    {
        Time.timeScale = 0f;
        // 일시정지 UI 표시
    }
    
    void RestartSong()
    {
        // 곡 재시작 로직
    }
    
    void OpenSettings()
    {
        // 설정 UI 열기
    }
    
    public void ChangeKeyBinding(int lineIndex, KeyCode newKey)
    {
        if (lineIndex >= 0 && lineIndex < settings.lineCount)
        {
            keyBindings[lineIndex] = newKey;
            lineKeys[lineIndex] = newKey;
        }
    }
    
    public KeyCode GetKeyBinding(int lineIndex)
    {
        if (keyBindings.ContainsKey(lineIndex))
        {
            return keyBindings[lineIndex];
        }
        return KeyCode.None;
    }
}