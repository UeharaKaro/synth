using System.Collections;
using System.Collections.Generic;       // Dictionary를 사용하기 위해 추가
using UnityEngine;
using FMOD;
using FMOD.Studio;
using Debug = UnityEngine.Debug;

/*
public class RhythmGameManger : MonoBehaviour
{
    [Header("게임설정")]
    public string bgmFileName = "testSong.wav";        // 재생할 음악 파일명
    public MetronomeSystem metronomeSystem;
        
    [Header("노트설정")]
    public GameObject notePrefab;            // 노트 프리팹 (프리팹의 정의는 https://docs.unity3d.com/kr/2023.2/Manual/Prefabs.html )
    public Transform[] trackSpawnsPoints;    // 각 트랙의 노트 생성 위치들
    public KeyCode[] trackKeys = { KeyCode.A, KeyCode.S,KeyCode.D, KeyCode.F }; // 각 트랙의 입력 키
        
    [Header("게임 데이터")]
    public List<NoteData> noteDataList = new List<NoteData>(); // 노트 데이터 리스트
        
    [Header("UI 컨트롤")]
    public UnityEngine.UI.Button playButton; // 게임 시작 버튼
    public UnityEngine.UI.Button stopButton; // 게임 정지 버튼   
    public UnityEngine.UI.Slider scrollSpeedSlider; // 스크롤 속도 조절 슬라이더
    public UnityEngine.UI.Text scrollSpeedText; // 현재 스크롤 속도를 보여주는 텍스트
    public UnityEngine.UI.Slider bgmVolumeSlider; // BGM 볼륨 슬라이더
        
        
    // 게임 진행 관련 변수들
    private bool isGamingPlaying = false;       // 게임 진행중인지 여부 
    private int currentNoteIndex = 0;       // 현재 처리 중인 노트 인덱스
    private List<Note> activeNotes = new List<Note>(); // 화면에 있는 활성 노트들
    private Queue<GameObject> notePool = new Queue<GameObject>(); // 노트 오브젝트 풀
    
    // 게임 시작 시 한 번 실행되는 함수
    void Start()
    {
        // UI 버튼들에 함수 연결 (버튼을 클릭하면 해당 함수가 실행됨)
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);       // 플레이 버튼 클릭시 PlayGame 함수 실행
        
        if (stopButton != null)
            stopButton.onClick.AddListener(StopGame);       // 정지 버튼 클릭 시 StopGame 함수 실행
        
        // 스크록 속도 슬라이더 설정
        if (scrollSpeedSlider != null)
        {
            // 슬라이더 값이 변경될 때마다 OnScrollSpeedChanged 함수 실행
            scrollSpeedSlider.onValueChanged.AddListener(OnScrollSpeedChanged);
            // 슬라이더 초기값을 현재 메트로놈 시스템의 스크롤 속도로 설정
            scrollSpeedSlider.value = metronomeSystem.scrollSpeed;
        }
    */

// SFX(효과음)을 정의하는 enum(열거형), 숫자대신 의미있는 이름 사용가능
public enum SFXType
{
    Metronome, // 메트로놈 소리(쉼표 유무는 선택)
    Hit, // 일반적인 히트 사운드
    Miss // 노트를 놓쳤을 때 나는 소리
}
// 키사운드 타입을 위한 enum (각 노트의 개별 키음)
public enum KeySoundType
{
    Kick,       // 킥드럼 소리
    Snare,      // 스네어 드럼 소리
    Hihat,      // 하이햇 소리
    Vocal1,     // 보컬 소리1
    Vocal2,     // 보컬 소리2
    Synth1,     // 신디사이저 소리 1
    Synth2,     // 신디사이저 소리 2
    Bass,       // 베이스 소리
    Piano,      // 피아노 소리
    Guitar,     // 기타 소리
    None,       // 소리 없음
}

// 판정 모드를 나타내는 열거형 

public enum JudgmentMode
{   
    JudgmentMode_Normal,       // 일반 게이머들 추천
    JudgmentMode_Hard,         // 심화 유저들 추천, 추후 랭크전 도입시 변별용
    JudgmentMode_Super,        // 현재 추가 계획 없음
}
// 판정 등급을 나타내는 열거형
public enum JudgmentType
{
    S_Perfect,      //세부판정
    Perfect,
    Great,
    Good,
    Bad,
    Miss
}   

public class RhythmManger : MonoBehaviour
{
    // 판정 기준 설정
    // 각 판정 등급의 최대 허용 시간(ms)를 저장하는 구조체
    public struct JudgmentTimings
    {
        public readonly float S_Perfect; // S_Perfect 허용 시간
        public readonly float Perfect; // Perfect 허용 시간
        public readonly float Great; // Great 허용 시간
        public readonly float Good; // Good 허용 시간
        public readonly float Bad; // Bad 허용 시간, Miss 는 허용시간 x. 이후 공푸어(긴접미스) 추가 여부는 미지수

        // 생성자: 각 모드별 판정 기준을 쉽게 초기화 하기 위함
        public JudgmentTimings(float s_Perfect, float perfect, float great, float good, float bad)
        {
            S_Perfect = s_Perfect;
            Perfect = perfect;
            Great = great;
            Good = good;
            Bad = bad;
        }
    }

    // 각 판정 모드에 따른 시간 기준을 저장하는 Dictionary
    private Dictionary<JudgmentMode, JudgmentTimings> modeTimings = new Dictionary<JudgmentMode, JudgmentTimings>()
    {
        // Normal 모드: S_Perfect 없음(0으로 설정), Perfect 판정이 다른 모드 대비 가장 널널함, 곡 종료시 게이지 70%이상시 클리어
        { JudgmentMode.JudgmentMode_Normal, new JudgmentTimings(0f, 41.66f, 83.33f, 120f, 150f) },

        // Hard 모드: S_perfect 등장, 전체적인 허용 시간 범위 감소,게이지 감소량 증가
        { JudgmentMode.JudgmentMode_Hard, new JudgmentTimings(16.67f, 31.25f, 62.49f, 88.33f, 120f) },

        // Super 모드: S_Perfect 허용 시간 범위 대폭 감소,게이지 감소량 증가, Bad 판정 삭제
        { JudgmentMode.JudgmentMode_Super, new JudgmentTimings(4.17f, 12.50f, 25.00f, 62.49f, 0f) }
    };

    // 현재 게임의 판정 모드 (Inspector 창에서 변경 가능)
    [Header("게임 난이도 설정")] [Tooltip("현재 적용할 판정 모드를 선택 하세요.")]
    public JudgmentMode currentMode = JudgmentMode.JudgmentMode_Normal;

    /// <summary>
    /// 시간 차이와 현재 모드를 기반으로 판정을 구함
    /// </summary>
    /// <param name="timeDifferenceMs">시간 차이(밀리초)</param>
    /// <returns> 계산된 판정 등급 <returns>
    public JudgmentType GetJudgment(float timeDifferenceMs)
    {
        // 현재 모드에 맞는 판정 기준을 가져온다
        JudgmentTimings timings = modeTimings[currentMode];

        float absTimeDiff = Mathf.Abs(timeDifferenceMs);

        // S_Perfect 판정 확인 (해당 모드에서 S_Perfect 기준이 0보다 클 떄만)
        if (timings.S_Perfect > 0 && absTimeDiff <= timings.S_Perfect) // S_Perfect 부터 Good 까지 공통 판정 로직
        {
            return JudgmentType.S_Perfect;
        }

        if (absTimeDiff <= timings.Perfect)
        {
            return JudgmentType.Perfect;
        }

        if (absTimeDiff <= timings.Great)
        {
            return JudgmentType.Great;
        }

        if (absTimeDiff <= timings.Good)
        {
            return JudgmentType.Good;
        }

        // *Super 모드일 경우, Bad 판정을 건너뛰고 바로 Miss 처리*
        if (currentMode == JudgmentMode.JudgmentMode_Super)
        {
            // Good 판정 범위를 벗어나면 무조건 Miss
            return JudgmentType.Miss;
        }

        // Super 모드가 아닐 경우, 기존처럼 Bad 판정을 확인
        if (absTimeDiff <= timings.Bad)
        {
            return JudgmentType.Bad;
        }

        // 모든 판정 범위를 벗어나면 Miss
        return JudgmentType.Miss;
    }

    // 예제 사용법(임시)
    void Update()
    {
        // 판정 모드 실시간 변경 (키보드 1, 2, 3)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentMode = JudgmentMode.JudgmentMode_Normal;
            Debug.Log("<color=green>판정 모드가 Normal로 변경 되었습니다.</color>");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentMode = JudgmentMode.JudgmentMode_Hard;
            Debug.Log("<color=orange>판정 모드가 Hard로 변경되었습니다.</color>");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
         {
             currentMode = JudgmentMode.JudgmentMode_Super;
             Debug.Log("<color=red>판정 모드가 Super로 변경되었습니다,</color>");
         }               

            // 현재 모드로 판정 테스트 진행
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 160ms 부터 160ms 사이의 랜덤한 시간 차이 생성
                float randomTimeDifference = Random.Range(-160f, 160f);

                // 생성된 시간 차이로 판정 함수 호출
                JudgmentType currentJudgment = GetJudgment(randomTimeDifference);

                // 결과를 Unity 콘솔에 출력
                Debug.Log(
                    $"모드: {currentMode} | 시간 오차: {randomTimeDifference:F2}ms -> 판정:<color=yellow>{currentJudgment}</color>");
            }
        }
    }
