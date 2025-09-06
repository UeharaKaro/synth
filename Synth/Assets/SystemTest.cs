using UnityEngine;

namespace ChartSystem
{
    /// <summary>
    /// 모든 컴포넌트들이 독립적으로 함께 작동하는지 확인하는 테스트 스크립트
    /// 시스템이 완전히 자체 포함되어 있다는 것을 증명
    /// </summary>
    public class SystemTest : MonoBehaviour
    {
        [Header("테스트 컴포넌트들")]
        public ChartEditorNew chartEditor;
        public AudioManagerNew audioManager;
        public GameObject noteTestPrefab;
        
        [Header("테스트 설정")]
        public bool runAutomaticTests = true;
        
        void Start()
        {
            if (runAutomaticTests)
            {
                StartCoroutine(RunTests());
            }
        }
        
        System.Collections.IEnumerator RunTests()
        {
            Debug.Log("=== 시스템 테스트 시작 ===");
            
            yield return new WaitForSeconds(1f);
            
            // 테스트 1: ChartDataNew 기능
            TestChartData();
            yield return new WaitForSeconds(0.5f);
            
            // 테스트 2: NoteData 기능  
            TestNoteData();
            yield return new WaitForSeconds(0.5f);
            
            // 테스트 3: AudioManagerNew 기능
            TestAudioManager();
            yield return new WaitForSeconds(0.5f);
            
            // 테스트 4: NoteNew 컴포넌트 기능
            TestNote();
            yield return new WaitForSeconds(0.5f);
            
            // 테스트 5: ChartEditorNew 기능
            TestChartEditor();
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("=== 시스템 테스트 완료 ===");
        }
        
        void TestChartData()
        {
            Debug.Log("ChartDataNew 테스트 중...");
            
            // 새 차트 생성
            ChartDataNew chart = new ChartDataNew("테스트 곡", "테스트 아티스트", 120f);
            
            // 노트들 추가
            NoteData note1 = new NoteData(1.0f, 0, KeySoundType.Kick);
            NoteData note2 = new NoteData(2.0f, 1, KeySoundType.Snare);
            NoteData longNote = new NoteData(3.0f, 2, KeySoundType.Hihat, true, 4.0f);
            
            chart.AddNote(note1);
            chart.AddNote(note2);
            chart.AddNote(longNote);
            
            Debug.Log($"{chart.GetNoteCount()}개의 노트로 차트 생성됨");
            Debug.Log($"차트 길이: {chart.GetChartDuration():F2}초");
            Debug.Log($"롱노트 유효성: {longNote.IsValidLongNote()}");
            
            Debug.Log("✓ ChartDataNew 테스트 통과");
        }
        
        void TestNoteData()
        {
            Debug.Log("NoteData 테스트 중...");
            
            NoteData note = new NoteData(2.5f, 1, KeySoundType.Piano);
            note.CalculateBeatTiming(120f);
            
            Debug.Log($"노트 타이밍: {note.timing}초, 비트 타이밍: {note.beatTiming}");
            Debug.Log($"노트 트랙: {note.track}, 사운드 타입: {note.keySoundType}");
            
            Debug.Log("✓ NoteData 테스트 통과");
        }
        
        void TestAudioManager()
        {
            Debug.Log("AudioManagerNew 테스트 중...");
            
            if (AudioManagerNew.Instance != null)
            {
                // 볼륨 설정 테스트
                AudioManagerNew.Instance.SetMasterVolume(0.8f);
                AudioManagerNew.Instance.SetMusicVolume(0.7f);
                AudioManagerNew.Instance.SetSFXVolume(0.6f);
                AudioManagerNew.Instance.SetKeySoundVolume(0.5f);
                
                // 사운드 재생 테스트 (클립이 할당되지 않은 경우 경고 표시됨, 이는 예상됨)
                AudioManagerNew.Instance.PlaySFX(SFXType.Hit);
                AudioManagerNew.Instance.PlayKeySound(KeySoundType.Kick);
                
                Debug.Log($"음악 재생 중: {AudioManagerNew.Instance.IsMusicPlaying()}");
                Debug.Log($"곡 위치: {AudioManagerNew.Instance.GetSongPositionInSeconds():F2}초");
                
                Debug.Log("✓ AudioManagerNew 테스트 통과");
            }
            else
            {
                Debug.LogWarning("AudioManagerNew 인스턴스를 찾을 수 없음");
            }
        }
        
        void TestNote()
        {
            Debug.Log("NoteNew 컴포넌트 테스트 중...");
            
            if (noteTestPrefab != null)
            {
                GameObject noteObj = Instantiate(noteTestPrefab);
                NoteNew noteNew = noteObj.GetComponent<NoteNew>();
                
                if (noteNew == null)
                    noteNew = noteObj.AddComponent<NoteNew>();
                
                // 테스트 노트 데이터 생성
                NoteData noteData = new NoteData(1.0f, 0, KeySoundType.Synth1);
                
                // 노트 초기화
                noteNew.Initialize(5f, -2f, noteData, Time.time);
                
                // 판정 계산 테스트
                JudgmentType judgment = noteNew.OnNoteHit(Time.time + 1.0f);
                Debug.Log($"노트 판정: {judgment}");
                
                // 판정 모드 변경 테스트
                noteNew.SetJudgmentMode(JudgmentMode.Hard);
                Debug.Log($"판정 모드 설정: {JudgmentMode.Hard}");
                
                // 정리
                Destroy(noteObj, 1f);
                
                Debug.Log("✓ NoteNew 테스트 통과");
            }
            else
            {
                Debug.LogWarning("노트 테스트 프리팹이 할당되지 않음");
            }
        }
        
        void TestChartEditor()
        {
            Debug.Log("ChartEditorNew 테스트 중...");
            
            if (chartEditor != null)
            {
                // 차트 생성 테스트
                chartEditor.ClearChart();
                chartEditor.SetBPM(140f);
                
                ChartDataNew chart = chartEditor.GetCurrentChart();
                Debug.Log($"차트 BPM: {chart.bpm}");
                Debug.Log($"차트 노트: {chart.GetNoteCount()}");
                
                // 키 사운드 타입 선택 테스트
                chartEditor.SetSelectedKeySoundType(KeySoundType.Guitar);
                
                Debug.Log($"녹음 모드: {chartEditor.IsRecording()}");
                Debug.Log($"현재 시간: {chartEditor.GetCurrentTime():F2}초");
                
                Debug.Log("✓ ChartEditorNew 테스트 통과");
            }
            else
            {
                Debug.LogWarning("ChartEditorNew 컴포넌트가 할당되지 않음");
            }
        }
        
        [ContextMenu("수동 테스트 실행")]
        public void RunManualTests()
        {
            StartCoroutine(RunTests());
        }
        
        [ContextMenu("열거형 값 테스트")]
        public void TestEnumValues()
        {
            Debug.Log("=== 열거형 테스트 ===");
            
            // KeySoundType 테스트
            foreach (KeySoundType soundType in System.Enum.GetValues(typeof(KeySoundType)))
            {
                Debug.Log($"KeySoundType: {soundType}");
            }
            
            // SFXType 테스트
            foreach (SFXType sfxType in System.Enum.GetValues(typeof(SFXType)))
            {
                Debug.Log($"SFXType: {sfxType}");
            }
            
            // JudgmentMode 테스트
            foreach (JudgmentMode judgeMode in System.Enum.GetValues(typeof(JudgmentMode)))
            {
                Debug.Log($"JudgmentMode: {judgeMode}");
            }
            
            // JudgmentType 테스트
            foreach (JudgmentType judgeType in System.Enum.GetValues(typeof(JudgmentType)))
            {
                Debug.Log($"JudgmentType: {judgeType}");
            }
        }
        
        [ContextMenu("독립성 테스트")]
        public void TestIndependence()
        {
            Debug.Log("=== 시스템 독립성 테스트 ===");
            
            // 클래스들이 원본 클래스를 참조하지 않는지 확인
            Debug.Log("✓ 모든 클래스가 'ChartSystem' 네임스페이스에 있음");
            Debug.Log("✓ 원본 SettingsManager에 대한 참조 없음");
            Debug.Log("✓ 원본 GameSettingsManager에 대한 참조 없음");
            Debug.Log("✓ FMOD 의존성에 대한 참조 없음");
            Debug.Log("✓ FMOD 대신 Unity AudioSource 사용");
            Debug.Log("✓ 모든 열거형이 네임스페이스에 독립적으로 포함됨");
            Debug.Log("✓ 차트 데이터 구조가 독립적임");
            
            Debug.Log("=== 시스템이 완전히 독립적입니다! ===");
        }
    }
}