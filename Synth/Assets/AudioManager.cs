using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMOD; // FMOD 라이브러리를 사용하기 위해 필요

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

public class AudioManager : MonoBehaviour // AudioManager 클래스는 FMOD를 사용하여 오디오를 관리하는 역할을 함
{
    [Header("FMOD 볼륨 설정")]
    public float mastervolume = 1.0f;    // 전체 볼륨 (0~ 1.0f 범위, 1.0f가 최대 볼륨)
    public float sfxVolume = 1.0f;       // 효과음 볼륨
    public float bgmVolume = 1.0f;      // 배경음악 볼륨
    public float keySoundVolume = 1.0f; // 키사운드 볼륨
    
    [Header("오디오 파일 경로")]
    public string audiopath = "Assets/Audio/"; // 오디오 파일이 저장된 경로
    
    [Header("채널 설정")] 
    public int maxChannels = 512; // 최대 채널 수, FMOD에서 동시에 재생할 수 있는 오디오 채널의 수
    
    // FMOD에서 사용하는 핵심 구성요소들
    private FMOD.System system; // FMOD 전체 시스템 (오디오 엔진의 뇌 역할)
    private FMOD.ChannelGroup sfxChannelGroup; // 효과음 채널 그룹
    private FMOD.ChannelGroup bgmChannelGroup; // 배경음악 채널 그룹
    private FMOD.ChannelGroup keySoundChannelGroup; // 키사운드를 관리하는 그룹
    private FMOD.ChannelGroup masterChannelGroup; // 전체 볼륨을 관리하는 그룹
    
    // 효과음들을 저장하고 관리하는 딕셔너리 (키-값 쌍으로 저장)
    private Dictionary<SFXType, FMOD.Sound> sfxs; // 효과음 파일들을 저장하는 딕셔너리
    private Dictionary<SFXType, FMOD.Channel> sfxChannels; // 효과음 채널들을 저장하는 딕셔너리
    
    // 키사운드들을 저장하고 관리하는 딕셔너리 (각 노트의 개별 소리)
    private Dictionary<KeySoundType, FMOD.Sound> keySounds; // 키사운드 파일들을 저장하는 딕셔너리

    // 배경음악 관련 변수들
    private FMOD.Sound bgmSound; // 현재 재생 중인 배경음악 (키사운드가 없을 때 재생되는 보정 BGM)
    private FMOD.Channel bgmChannel; // 배경음악 재생 채널
    
    // DSP 시간 기반 정확한 타이밍을 위한 변수들
    private double dspSongTime = 0.0; // 곡이 시작된 DSP 시간
    private bool isSongStarted = false; // 곡이 시작되었는지 여부

    // 사용 중인 채널들을 추적하기 위한 리스트 (메모리 누수 방지)
    private List<FMOD.Channel> activeChannels = new List<FMOD.Channel>(); // 현재 활성화된 채널들을 저장하는 리스트
    
    // 싱글톤 패턴 구현 - 게임 전체에서 AudioManager는 하나만 존재
    // 어디서든 AudioManager.Instance로 접근 가능
    private  static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            // 만약 instance가 null이라면, 현재 씬에서 AudioManager를 찾거나 새로 생성
            if (instance == null)
                instance = FindObjectOfType<AudioManager>();
            return instance;
        }
    }
    
    // 게임 오브젝트가 생성될 때 제일 먼저 실행되는 함수
    void Awake()
    {
        // 이미 AuidoManager가 존재한다면 새로 생성된 것을 삭제 (중복 방지)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 AudioManager는 유지
            InitializeFMOD(); // FMOD 초기화 함수 호출
        }
        else
        {
            Destroy(gameObject); // 중복된 AudioManager는 삭제
        }
    }
    
    // FMOD 시스템 초기화 함수
    void InitializeFMOD()
    {
        // FMOD 시스템 생성 (오디오 엔진 시작)
        // maxChannels로 충분한 채널 수 확보 (동시 재생 가능한 사운드 수)
        FMOD.Factory.System_Create(out system);
        system.init(maxChannels, FMOD.INITFLAGS.NORMAL, System.IntPtr.Zero); // FMOD 시스템 초기화
        
        // 마스터 채널 그룹 생성 (모든 오디오 최상위 그룹)
        system.getMasterChannelGroup(out masterChannelGroup);
        
        // 채널 그룹 생성 - 오디오를 종류별로 분류해서 관리 
        system.createChannelGroup("SFX", out sfxChannelGroup); // 효과음 채널 그룹
        system.createChannelGroup("BGM", out bgmChannelGroup); // 배경음악 채널 그룹
        system.createChannelGroup("KeySound", out keySoundChannelGroup); // 키사운드 채널 그룹
        
        // 각 채널 그룹을 마스터 그룹의 하위로 설정 
        masterChannelGroup.addGroup(sfxChannelGroup);
        masterChannelGroup.addGroup(bgmChannelGroup);
        masterChannelGroup.addGroup(keySoundChannelGroup);
        
        // 효과음과 키사운드를 저장할 딕셔너리 초기화
        sfxs = new Dictionary<SFXType, FMOD.Sound>();
        sfxChannels = new Dictionary<SFXType, FMOD.Channel>();
        
        // 키 사운드를 저장할 딕셔너리 초기화
        keySounds = new Dictionary<KeySoundType, FMOD.Sound>();
        
        LoadSFXs(); // 효과음 파일들을 로드
        LoadKeySounds(); // 키사운드 파일들을 로드
        
        UnityEngine.Debug.Log($"FMOD 초기화 완료 - 최대 채널: {maxChannels}");
    }
    // 효과음 파일들을 메모리에 미리 로드하는 함수
    void LoadSFXs()
    {   
        // SFXType 열거형에 정의된 모든 효과음에 대해 반복
        foreach (SFXType sfxType in System.Enum.GetValues(typeof(SFXType)))
        {
            // 파일 이름 생성: ex) Metronome-> "Metronome.wav"
            string fileName = sfxType.ToString() + ".wav";
            // 전체 파일 경로 생성
            string filePath = Application.streamingAssetsPath + "/Audio/" + fileName;
            
            // FMOD에서 사운드 파일을 메모리에 로드
            FMOD.Sound sound;
            var result = system.createSound(filePath, FMOD.MODE.DEFAULT, out sound);
            
            if (result == FMOD.RESULT.OK)
            {
                sfxs[sfxType] = sound; // 성공적으로 로드되면 딕셔너리에 추가
                UnityEngine.Debug.Log($"효과음 로드 성공: {fileName}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"효과음 파일을 찾을 수 없습니다: {filePath} (결과: {result})");
            }
        }
    }
    
    // 모든 키사운드 파일을 메모리에 미리 로드하는 함수
    void LoadKeySounds()
    {
        // KeySoundType 열거형에 정의된 모든 키사운드에 대해 반복
        foreach (KeySoundType keySoundType in System.Enum.GetValues(typeof(KeySoundType)))
        {
            // None 타입은 제외 (소리 없음)
            if (keySoundType == KeySoundType.None) continue;
            
            // 파일 이름 생성: ex) Kick-> "Kick.wav"
            string fileName = keySoundType.ToString() + ".wav";
            // 전체 파일 경로 생성 (KeySounds 폴더에 저장)
            string filePath = Application.streamingAssetsPath + "/Audio/KeySounds/" + fileName;
            
            // FMOD에서 사운드 파일을 메모리에 로드
            FMOD.Sound sound;
            var result = system.createSound(filePath, FMOD.MODE.DEFAULT, out sound);
            
            // 파일 로드에 성공한 경우에만 딕셔너리에 저장
            if (result == FMOD.RESULT.OK)
            {
                keySounds[keySoundType] = sound; // 성공적으로 로드되면 딕셔너리에 추가
                UnityEngine.Debug.Log($"키사운드 로드 성공: {fileName}");
            }
            else
            {
                // 파일 로드 실패 시 디버그 메세지 출력
                UnityEngine.Debug.LogWarning($"키사운드 파일을 찾을 수 없습니다: {filePath} (결과: {result})");
            }
            
        }
    }
    // 배경음악 파일을 로드하는 함수
    public void LoadBGM(string fileName)
    {
        // 기존 BGM이 있으면 해제
        if (bgmSound.hasHandle())
        {
            bgmSound.release();
        }
        
        // 배경음악 파일의 전체 경로 생성
        string filePath = Application.streamingAssetsPath + "/Audio/BGM/" + fileName;
        // FMOD에서 사운드 파일을 메모리에 로드
        var result = system.createSound(filePath, FMOD.MODE.DEFAULT, out bgmSound);
        
        if (result == FMOD.RESULT.OK)
        {
            UnityEngine.Debug.Log($"BGM 로드 완료: {fileName}");
        }
        else
        {
            UnityEngine.Debug.LogError($"BGM 로드 실패: {filePath} (결과: {result})");
        }
    }
    
    // 배경음악을 재생하는 함수 (DSP 시간 기반으로 정확한 타이밍)
    public void PlayBGM()
    {
        // 배경음악이 로드되어 있는지 확인
        if (bgmSound.hasHandle())
        {
           // DSP 시간을 기록하여 정확한 곡 시작 지점 추적
           dspSongTime = AudioSettings.dspTime;
           isSongStarted = true; // 곡이 시작되었음을 표시
           
           // 배경음악 재생 시작
           var result = system.playSound(bgmSound, bgmChannelGroup, false, out bgmChannel);
           if (result == FMOD.RESULT.OK)
           {    
                bgmChannel.setVolume(bgmVolume); // 배경음악 볼륨 설정
                UnityEngine.Debug.Log($"BGM 재생 시작 - DSP 시간: {dspSongTime}");
           }
           else
           {
               UnityEngine.Debug.LogError($"BGM 재생 실패: {result}");
           }
        }