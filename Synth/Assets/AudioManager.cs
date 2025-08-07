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


// 판정 설정을 저장하는 클래스
[System.Serializable] // Unity에서 직렬화 가능하도록 설정
public class JudgmentSettings
{
    public JudgmentMode mode; // Q)여기 왜 Type이아니라 Mode가 들어가는거지..
    public float S_PerfectRange; // S_Perfect 허용 범위 (ms)
    public float PerfectRange; // Perfect 허용 범위 (ms)
    public float GreatRange; // Great 허용 범위 (ms)
    public float GoodRange; // Good 허용 범위 (ms)
    public float BadRange; // Bad 허용 범위 (ms)
    
    public JudgmentSettings(JudgmentMode mode, float s_PerfectRange, float perfectRange, float greatRange, float goodRange, float badRange)
    {
        this.mode = mode;
        this.S_PerfectRange = s_PerfectRange;
        this.PerfectRange = perfectRange;
        this.GreatRange = greatRange;
        this.GoodRange = goodRange;
        this.BadRange = badRange;
    }
    
    // 미리 정의된 판정 모드별 설정
    public static JudgmentSettings GetPreset(JudgmentMode mode)
    {
        switch (mode)
        {
            case JudgmentMode.JudgmentMode_Normal:
                return new JudgmentSettings(
                    mode,
                    0f, // S_Perfect 없음
                    41.66f, // Perfect 허용 범위
                    83.33f, // Great 허용 범위
                    120f, // Good 허용 범위
                    150f // Bad 허용 범위
                );
            case JudgmentMode.JudgmentMode_Hard:
                return new JudgmentSettings(
                    mode,
                    16.67f, // S_Perfect 허용 범위
                    31.25f, // Perfect 허용 범위
                    62.49f, // Great 허용 범위
                    88.33f, // Good 허용 범위
                    120f // Bad 허용 범위
                );
            case JudgmentMode.JudgmentMode_Super:
                return new JudgmentSettings(
                    mode,
                    4.17f, // S_Perfect 허용 범위
                    12.50f, // Perfect 허용 범위
                    25.00f, // Great 허용 범위
                    62.49f, // Good 허용 범위
                    0f // Super 모드에서는 Bad 판정 없음
                );
            default:
                // 기본값 설정 (예외 처리)
                return new JudgmentSettings(
                    JudgmentMode.JudgmentMode_Normal,
                    0f, // S_Perfect 없음
                    41.66f, // Perfect 허용 범위
                    83.33f, // Great 허용 범위
                    120f, // Good 허용 범위
                    150f // Bad 허용 범위
                );
        }
    }
}

public class AudioManager : MonoBehaviour // AudioManager 클래스는 FMOD를 사용하여 오디오를 관리하는 역할을 함
{
    [Header("FMOD 볼륨 설정")] 
    public float mastervolume = 1.0f; // 전체 볼륨 (0~ 1.0f 범위, 1.0f가 최대 볼륨)
    public float sfxVolume = 1.0f; // 효과음 볼륨
    public float bgmVolume = 1.0f; // 배경음악 볼륨
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
    private static AudioManager instance;
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
    }

    // 배경음악을 정지하는 함수
    public void StopBGM()
    {
        if (bgmChannel.hasHandle())
        {
            bgmChannel.stop();
            isSongStarted = false; // 곡이 정지되었음을 표시
            UnityEngine.Debug.Log("BGM 정지됨");
        }
    }

    // 효과음을 재생하는 함수
    public void PlaySFX(SFXType sfxType)
    {
        // 해당 효과음이 로드되어 있는지 확인
        if (sfxs.ContainsKey(sfxType))
        {
            // 새로운 채널에서 효과음 재생 (여러개 동시 재생가능)
            FMOD.Channel channel;
            var result = system.playSound(sfxs[sfxType], sfxChannelGroup, false, out channel);
            if (result == FMOD.RESULT.OK)
            {
                channel.setVolume(sfxVolume); // 효과음 볼륨 설정
                activeChannels.Add(channel); // 활성화된 채널 리스트에 추가 (메모리 관리용)
            }
        }
    }

    // 키사운드를 재생하는 함수 (노트를 정확히 칠 때 사용)
    public void PlayKeySound(KeySoundType keySoundType)
    {
        // None 타입이거나 해당 키사운드가 로드되지 않은 경우 재생하지 않음
        if (keySoundType == KeySoundType.None || !keySounds.ContainsKey(keySoundType))
            return; // 소리 없음 또는 로드되지 않은 키사운드는 무시

        // 새로운 채널에서 키사운드 재생 (여러 개 동시 재생가능)
        FMOD.Channel channel;
        // FMOD 오디오 엔진에서 특정 키사운드(keySounds[keySoundType])를 지정된 채널 그룹(keySoundChannelGroup)에서 재생하는 명령
        var result = system.playSound(keySounds[keySoundType], keySoundChannelGroup, false, out channel);
        if (result == FMOD.RESULT.OK)
        {
            channel.setVolume(keySoundVolume); // 키사운드 전용 볼륨 설정
            activeChannels.Add(channel); // 활성화된 채널 리스트에 추가 (메모리 관리용)
        }
    }
    // 키사운드를 실제 플레이어 입력 타이밍에 맞춰 재생하는 함수 *important*
    public void PlayKeySoundAtInputTime(KeySoundType keySoundType, double actualInputTime, double expectedTime)
    {
        // None 타입이거나 해당 키사운드가 로드되지 않은 경우 재생하지 않음
        if (keySoundType == KeySoundType.None || !keySounds.ContainsKey(keySoundType))
            return; // 소리 없음 또는 로드되지 않은 키사운드는 무시

        // 실제 입력 시간과 예상 시간의 차이를 계산 (초단위)
        double timingDifference = actualInputTime - expectedTime;

        // 새로운 채널에서 키사운드 재생 (여러 개 동시 재생가능)
        FMOD.Channel channel;
        var result = system.playSound(keySounds[keySoundType], keySoundChannelGroup, false, out channel);
        if (result == FMOD.RESULT.OK)
        {
            channel.setVolume(keySoundVolume); // 키사운드 전용 볼륨 설정

            // 타이밍 차이에 따른 피치(음정) 미세 조정 
            // 일찍 누르면 약간 높은 음정, 늦게 누르면 약간 낮은 음정
            float pitchShift = 1.0f + (float)(timingDifference * 0.01f); // 최대 +-10% 피치 조절 
            // ex): 입력이 0.05초 빨랐다면 피치는 1.0f + 0.0005 = 1.005f
            pitchShift = Mathf.Clamp(pitchShift, 0.8f, 1.2f); // 피치 범위 제한 (80% ~ 120%)
            channel.setPitch(pitchShift); // 피치 설정

            //  현재 판정 모드의 Great 범위를 기준으로 볼륨 감소 적용
            double timingErrorMs = System.Math.Abs(timingDifference * 1000.0); // ms 단위로 변환
            if (timingErrorMs > judgmentSettings.greatRange)
            {
                float volumeRedutuion = (float)(timingErrorMs / 100.0f); // 100ms당 볼륨 10% 감소
                float adjustedVolume = keySoundVolume * (1.0f - Mathf.Clamp(volumeRedutuion, 0.1f, 0.3f)); // 최대 30% 감소
                channel.setVolume(adjustedVolume);
            }

            // 활성화된 채널 리스트에 추가 (메모리 관리용)
            activeChannels.Add(channel);

            UnityEngine.Debug.Log($"키사운드 재생 - 타이밍 차이: {timingDifference * 1000:F1}ms, 피치: {pitchShift:F3}");
        }
    }

    // 키사운드를 실제 플레이어 입력에 맞춰 재생하는 함수 (RhythmManager에서 호출)
    public void PlayKeySoundAtInputTime(KeySoundType keySoundType, double actualInpuTime, double expectedInpuTime,
        bool enableEffects, float maxPitch, float maxVolume, JudgmentSettings judgmentSettings)
    {
        // Noene 타입이거나 해당 키사운드가 로드되지 않은 경우 재생하지 않음
        if (keySoundType == KeySoundType.None || !keySounds.ContainsKey(keySoundType))
            return; // 소리 없음 또는 로드되지 않은 키사운드는 무시

        // 실제 입력 시간과 예상 시간의 차이를 계산 (초단위)
        double timingDifference = actualInpuTime - expectedInpuTime;

        // 새로운 채널에서 키사운드 재생 (여러 개 동시 재생가능)
        FMOD.Channel channel;
        var result = system.playSound(keySounds[keySoundType], keySoundChannelGroup, false, out channel);
        if (result == FMOD.RESULT.OK)
        {
            channel.setVolume(keySoundVolume); // 키사운드 전용 볼륨 설정

            // 타이밍 기반 효과가 활성화된 경우에만 적용
            if (enableEffects)
            {
                // 타이밍 차이에 따른 피치(음정) 미세 조정 
                // 일찍 누르면 약간 높은 음정, 늦게 누르면 약간 낮은 음정
                float pitchShift = 1.0f + (float)(timingDifference * maxPitch);
                pitchShift = Mathf.Clamp(pitchShift, 1.0f - maxPitch, 1.0f + maxPitch); // 피치 범위 제한
                channel.setPitch(pitchShift); // 피치 설정

                // 현재 판정모드의 Great 범위를 기준으로 볼륨 감소 적용
                double timingErrorMs = System.Math.Abs(timingDifference * 1000.0);
                if (timingErrorMs > judgmentSettings.GreatRange) // Great 범위를 벗어난 경우
                {
                    float volumeReduction = (float)(timingErrorMs / 100.0f); // 100ms당 볼륨 10% 감소
                    float adjustedVolume = keySoundVolume * (1.0f - Mathf.Clamp(volumeReduction, 0.1f, 0f, maxVolume));
                    channel.setVolume(adjustedVolume);

                    UnityEngine.Debug.Log(
                        $"키사운드 볼륨 감소 - 판정모드: {judgmentSettings.mode}, Great범위 : {judgmentSettings.GreatRange}ms, 실제오차: {timingErrorMs:F1}ms");
                }

                UnityEngine.Debug.Log(
                    $"키사운드 재생 (효과적용) - 타이밍 차이: {timingDifference * 1000:F1}ms, 피치: {pitchShift:F3}, 판정 모드 : {judgmentSettings.mode}");
            }
            else
            {
                UnityEngine.Debug.Log($"키사운드 재생 (기본) - 타이밍 차이: {timingDifference * 1000:F1}ms");
            }

            // 활성 화된 채널 리스트에 추가 (메모리 관리용)
            activeChannels.Add(channel);
        }
    }
    
    // 미래시점에 키사운드를 예약 재생하는 함수 (레이턴시 보정용)
    public void ScheduleKeySound(KeySoundType keySoundType, double scheduledTime)
    {
        // None 타입이거나 해당 키사운드가 로드되지 않은 경우 재생하지 않음
        if (keySoundType == KeySoundType.None || !keySounds.ContainsKey(keySoundType))
            return; // 소리 없음 또는 로드되지 않은 키사운드는 무시

        // 현재는 즉시 재생하지만, FMOD의 PlayScheeduled 기능을 활용할 수 있음
        // 실제 구현시 FMOD Timeline이나 DSP 클럭 기반 스케줄링 활용
        PlayKeySound(keySoundType);
    }

    // 키사운드 볼륨을 별도로 조절하는 함수 
    public void SetKeySoundVolume(float volume)
    {
        keySoundVolume = Mathf.Clamp01(volume); // 전달받은 볼륨 값을 0~1 사이로 제한하여 keySoundVolume에 저장
        keySoundChannelGroup.setVolume(keySoundVolume); // 키사운드 채널 그룹의 볼륨 설정
    }
    
    // 배경음악(보정 BGM)의 볼륨을 조절하는 함수
    // 키사운드가 재생될 때 BGM을 줄이고, 놓쳤을 때 BGM을 원래대로 하는 용도
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume); // 전달받은 볼륨 값을 0~1 사이로 제한하여 bgmVolume에 저장
        if (bgmChannel.hasHandle())
        {
            bgmChannel.setVolume(bgmVolume); // 배경음악 채널의 볼륨 설정
        }
        bgmChannelGroup.setVolume(bgmVolume); // 배경음악 채널 그룹의 볼륨 설정
    }

    // 마스터 볼륨을 조절하는 함수
    public void SetMasterVolume(float volume)
    {
        mastervolume = Mathf.Clamp01(volume); // 전달받은 볼륨 값을 0~1 사이로 제한하여 mastervolume에 저장
        masterChannelGroup.setVolume(mastervolume); // 마스터 채널 그룹의 볼륨 설정
    }
    
    // 현재 배경음악이 얼마나 재생되었는지 DSP 시간 기준으로 반환 (초 단위)
    // 정확한 타이밍을 맞추기 위해 꼭 필요
    public double GetSongPositionInSeconds()
    {
        if (!isSongStarted) return 0.0; // 곡이 시작되지 않았다면 0초 반환
        
        // DSP 시간 기반으로 정확한 곡 재생 위치 계산
        return AudioSettings.dspTime - dspSongTime; // 현재 DSP 시간에서 곡 시작 시간을 빼서 경과 시간 계산
    }
    
    // BPM을 기준으로 현재 곡의 위치를 박자 단위로 반환
    public double GetSongPositionInBeats(double bpm)
    {
        double songPositionInSeconds = GetSongPositionInSeconds(); // 현재 곡 위치 (초 단위)
        // 1분 = 60초, BPM = 분당 박자 수이므로 초당 박자 수 = BPM/60
        return songPositionInSeconds * (bpm / 60.0); // 초 단위 곡 위치를 박자 단위로 변환
    }
    