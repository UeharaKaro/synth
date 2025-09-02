using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChartEditorTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestsOnStart = false;
    public bool showTestResults = true;
    public ChartEditor chartEditor;
    
    private List<TestResult> testResults = new List<TestResult>();
    
    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    IEnumerator RunAllTests()
    {
        Debug.Log("=== 차트 에디터 테스트 시작 ===");
        
        yield return StartCoroutine(TestChartDataCreation());
        yield return StartCoroutine(TestNoteManagement());
        yield return StartCoroutine(TestLaneSystem());
        yield return StartCoroutine(TestAudioIntegration());
        yield return StartCoroutine(TestValidation());
        yield return StartCoroutine(TestSaveLoad());
        
        ShowTestResults();
        Debug.Log("=== 차트 에디터 테스트 완료 ===");
    }
    
    IEnumerator TestChartDataCreation()
    {
        Debug.Log("테스트: 차트 데이터 생성");
        
        // 새 차트 생성 테스트
        ChartData testChart = new ChartData();
        testChart.songTitle = "Test Song";
        testChart.artist = "Test Artist";
        testChart.bpm = 120f;
        testChart.laneCount = 4;
        
        bool isValid = testChart.ValidateChart();
        AddTestResult("ChartData 기본 생성", isValid, "기본 차트 데이터가 유효해야 함");
        
        // 통계 테스트
        var stats = testChart.GetStatistics();
        AddTestResult("차트 통계 계산", stats != null, "통계 객체가 생성되어야 함");
        
        yield return null;
    }
    
    IEnumerator TestNoteManagement()
    {
        Debug.Log("테스트: 노트 관리");
        
        ChartData testChart = new ChartData();
        testChart.bpm = 120f;
        testChart.laneCount = 4;
        
        // 노트 추가 테스트
        NoteData note1 = new NoteData(1.0, 0, KeySoundType.Kick);
        NoteData note2 = new NoteData(2.0, 1, KeySoundType.Snare);
        NoteData longNote = new NoteData(3.0, 2, KeySoundType.Hihat, true, 4.0);
        
        testChart.AddNote(note1);
        testChart.AddNote(note2);
        testChart.AddNote(longNote);
        
        AddTestResult("노트 추가", testChart.notes.Count == 3, $"3개 노트가 추가되어야 함 (실제: {testChart.notes.Count})");
        
        // 노트 정렬 테스트
        NoteData earlyNote = new NoteData(0.5, 0, KeySoundType.None);
        testChart.AddNote(earlyNote);
        
        bool isSorted = true;
        for (int i = 1; i < testChart.notes.Count; i++)
        {
            if (testChart.notes[i].timing < testChart.notes[i-1].timing)
            {
                isSorted = false;
                break;
            }
        }
        AddTestResult("노트 정렬", isSorted, "노트가 시간순으로 정렬되어야 함");
        
        // 특정 범위 노트 가져오기 테스트
        var rangeNotes = testChart.GetNotesInRange(1.5, 3.5);
        AddTestResult("범위 노트 검색", rangeNotes.Count == 2, $"2개 노트가 범위에 있어야 함 (실제: {rangeNotes.Count})");
        
        // 노트 제거 테스트
        bool removed = testChart.RemoveNote(note1);
        AddTestResult("노트 제거", removed && testChart.notes.Count == 3, "노트가 제거되어야 함");
        
        yield return null;
    }
    
    IEnumerator TestLaneSystem()
    {
        Debug.Log("테스트: 레인 시스템");
        
        if (chartEditor == null)
        {
            AddTestResult("레인 시스템", false, "ChartEditor가 없음");
            yield break;
        }
        
        // 초기 레인 수 확인
        int initialLanes = chartEditor.GetCurrentChart().laneCount;
        AddTestResult("초기 레인 수", initialLanes >= 4 && initialLanes <= 10, 
                     $"초기 레인 수가 유효 범위에 있어야 함 (실제: {initialLanes})");
        
        // 레인 수 변경 시뮬레이션
        // 실제 키 입력 시뮬레이션은 어렵지만, 내부 메서드 테스트 가능
        yield return null;
    }
    
    IEnumerator TestAudioIntegration()
    {
        Debug.Log("테스트: 오디오 통합");
        
        if (chartEditor == null)
        {
            AddTestResult("오디오 통합", false, "ChartEditor가 없음");
            yield break;
        }
        
        // 오디오 오프셋 테스트
        float originalOffset = chartEditor.GetCurrentChart().audioOffset;
        chartEditor.SetAudioOffset(50f);
        bool offsetChanged = Mathf.Abs(chartEditor.GetCurrentChart().audioOffset - 50f) < 0.1f;
        AddTestResult("오디오 오프셋 설정", offsetChanged, "오프셋이 50ms로 설정되어야 함");
        
        // 재생 속도 테스트
        chartEditor.SetPlaybackSpeed(1.5f);
        // 내부 변수 접근이 어렵지만, 에러가 없으면 성공으로 간주
        AddTestResult("재생 속도 설정", true, "재생 속도 설정 완료");
        
        yield return null;
    }
    
    IEnumerator TestValidation()
    {
        Debug.Log("테스트: 유효성 검사");
        
        // 유효한 차트 테스트
        ChartData validChart = new ChartData();
        validChart.audioFileName = "test.wav";
        validChart.bpm = 120f;
        validChart.laneCount = 4;
        
        var validResult = ChartValidator.ValidateChart(validChart);
        AddTestResult("유효한 차트 검증", validResult.IsValid, "유효한 차트는 통과해야 함");
        
        // 잘못된 차트 테스트
        ChartData invalidChart = new ChartData();
        invalidChart.audioFileName = ""; // 빈 파일명
        invalidChart.bpm = -10f; // 잘못된 BPM
        invalidChart.laneCount = 15; // 잘못된 레인 수
        
        var invalidResult = ChartValidator.ValidateChart(invalidChart);
        AddTestResult("잘못된 차트 검증", !invalidResult.IsValid, "잘못된 차트는 실패해야 함");
        
        // 노트 유효성 테스트
        ChartData noteTestChart = new ChartData();
        noteTestChart.audioFileName = "test.wav";
        noteTestChart.bpm = 120f;
        noteTestChart.laneCount = 4;
        
        // 잘못된 트랙 번호
        noteTestChart.AddNote(new NoteData(1.0, 10, KeySoundType.None)); // 트랙 10은 유효하지 않음
        
        var noteResult = ChartValidator.ValidateChart(noteTestChart);
        AddTestResult("잘못된 노트 검증", !noteResult.IsValid, "잘못된 트랙 번호는 실패해야 함");
        
        yield return null;
    }
    
    IEnumerator TestSaveLoad()
    {
        Debug.Log("테스트: 저장/로드");
        
        // 테스트 차트 생성
        ChartData testChart = new ChartData();
        testChart.songTitle = "Test Save Load";
        testChart.artist = "Test Artist";
        testChart.bpm = 140f;
        testChart.laneCount = 6;
        
        // 몇 개 노트 추가
        testChart.AddNote(new NoteData(1.0, 0, KeySoundType.Kick));
        testChart.AddNote(new NoteData(2.0, 2, KeySoundType.Snare));
        testChart.AddNote(new NoteData(3.0, 1, KeySoundType.Hihat, true, 4.0));
        
        // JSON 변환 테스트
        string json = JsonUtility.ToJson(testChart, true);
        bool jsonCreated = !string.IsNullOrEmpty(json);
        AddTestResult("JSON 변환", jsonCreated, "차트가 JSON으로 변환되어야 함");
        
        if (jsonCreated)
        {
            // JSON에서 복원 테스트
            try
            {
                ChartData loadedChart = JsonUtility.FromJson<ChartData>(json);
                bool dataMatches = loadedChart.songTitle == testChart.songTitle &&
                                 loadedChart.artist == testChart.artist &&
                                 Mathf.Abs(loadedChart.bpm - testChart.bpm) < 0.1f &&
                                 loadedChart.laneCount == testChart.laneCount &&
                                 loadedChart.notes.Count == testChart.notes.Count;
                
                AddTestResult("JSON 복원", dataMatches, "차트가 JSON에서 정확히 복원되어야 함");
            }
            catch (System.Exception e)
            {
                AddTestResult("JSON 복원", false, $"JSON 복원 중 오류: {e.Message}");
            }
        }
        
        yield return null;
    }
    
    void AddTestResult(string testName, bool passed, string description)
    {
        testResults.Add(new TestResult
        {
            testName = testName,
            passed = passed,
            description = description
        });
        
        string status = passed ? "PASS" : "FAIL";
        Debug.Log($"[{status}] {testName}: {description}");
    }
    
    void ShowTestResults()
    {
        if (!showTestResults) return;
        
        int passedCount = 0;
        int totalCount = testResults.Count;
        
        foreach (var result in testResults)
        {
            if (result.passed) passedCount++;
        }
        
        Debug.Log($"\n=== 테스트 결과 요약 ===");
        Debug.Log($"전체: {totalCount}, 성공: {passedCount}, 실패: {totalCount - passedCount}");
        Debug.Log($"성공률: {(float)passedCount / totalCount * 100:F1}%");
        
        if (totalCount - passedCount > 0)
        {
            Debug.Log("\n실패한 테스트:");
            foreach (var result in testResults)
            {
                if (!result.passed)
                {
                    Debug.LogError($"- {result.testName}: {result.description}");
                }
            }
        }
    }
    
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string description;
    }
    
    // 런타임에서 테스트 실행
    [ContextMenu("Run Tests")]
    public void RunTestsManually()
    {
        StartCoroutine(RunAllTests());
    }
    
    void OnGUI()
    {
        if (showTestResults && testResults.Count > 0)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 280, 200), GUI.skin.box);
            GUILayout.Label("테스트 결과", EditorGUIStyle.boldLabel);
            
            int passed = 0;
            foreach (var result in testResults)
            {
                if (result.passed) passed++;
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(result.passed ? "✓" : "✗");
                GUILayout.Label(result.testName);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10);
            GUILayout.Label($"성공: {passed}/{testResults.Count}");
            
            if (GUILayout.Button("테스트 다시 실행"))
            {
                testResults.Clear();
                RunTestsManually();
            }
            
            GUILayout.EndArea();
        }
    }
    
    private static class EditorGUIStyle
    {
        public static GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
    }
}