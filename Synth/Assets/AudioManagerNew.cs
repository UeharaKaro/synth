using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace ChartSystem
{
    /// <summary>
    /// 독립적인 오디오 매니저 - 완전히 자율적
    /// Unity의 내장 AudioSource 컴포넌트를 사용한 간소화된 오디오 관리
    /// FMOD나 SettingsManager에 대한 외부 의존성 없음
    /// </summary>
    public class AudioManagerNew : MonoBehaviour
    {
        [Header("오디오 소스들")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource keySoundSource;
        
        [Header("볼륨 설정")]
        [Range(0f, 1f)]
        public float masterVolume = 1.0f;
        [Range(0f, 1f)]
        public float musicVolume = 0.8f;
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;
        [Range(0f, 1f)]
        public float keySoundVolume = 0.8f;
        
        [Header("오디오 파일들")]
        public AudioClip[] sfxClips = new AudioClip[3]; // 메트로놈, 히트, 미스
        public AudioClip[] keySoundClips = new AudioClip[10]; // 다양한 키 사운드들
        
        // 개인 변수들
        private Dictionary<SFXType, AudioClip> sfxLibrary;
        private Dictionary<KeySoundType, AudioClip> keySoundLibrary;
        private bool isInitialized = false;
        private float songStartTime = 0f;
        private bool isSongPlaying = false;
        
        // 싱글톤 패턴
        private static AudioManagerNew instance;
        public static AudioManagerNew Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<AudioManagerNew>();
                return instance;
            }
        }
        
        void Awake()
        {
            // 싱글톤 구현
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void InitializeAudioManager()
        {
            // 존재하지 않는 경우 오디오 소스 생성
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("Music AudioSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
            }
            
            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFX AudioSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
            }
            
            if (keySoundSource == null)
            {
                GameObject keySoundGO = new GameObject("KeySound AudioSource");
                keySoundGO.transform.SetParent(transform);
                keySoundSource = keySoundGO.AddComponent<AudioSource>();
            }
            
            // 오디오 클립 라이브러리 초기화
            InitializeAudioLibraries();
            
            // 초기 볼륨 설정 적용
            ApplyVolumeSettings();
            
            isInitialized = true;
            Debug.Log("AudioManagerNew 성공적으로 초기화됨");
        }
        
        void InitializeAudioLibraries()
        {
            // SFX 라이브러리 초기화
            sfxLibrary = new Dictionary<SFXType, AudioClip>();
            
            // SFX 클립 매핑 (가능한 경우)
            if (sfxClips.Length >= 3)
            {
                sfxLibrary[SFXType.Metronome] = sfxClips[0];
                sfxLibrary[SFXType.Hit] = sfxClips[1];
                sfxLibrary[SFXType.Miss] = sfxClips[2];
            }
            
            // 키사운드 라이브러리 초기화
            keySoundLibrary = new Dictionary<KeySoundType, AudioClip>();
            
            // 키사운드 클립 매핑 (가능한 경우)
            var keySoundTypes = System.Enum.GetValues(typeof(KeySoundType));
            for (int i = 0; i < keySoundClips.Length && i < keySoundTypes.Length - 1; i++) // -1로 'None' 스킵
            {
                if (keySoundClips[i] != null)
                {
                    KeySoundType soundType = (KeySoundType)(i + 1); // +1로 'None' 스킵
                    keySoundLibrary[soundType] = keySoundClips[i];
                }
            }
        }
        
        void ApplyVolumeSettings()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
                
            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
                
            if (keySoundSource != null)
                keySoundSource.volume = keySoundVolume * masterVolume;
        }
        
        /// <summary>
        /// 배경 음악 재생
        /// </summary>
        public void PlayMusic(AudioClip musicClip)
        {
            if (!isInitialized || musicSource == null || musicClip == null) return;
            
            musicSource.clip = musicClip;
            musicSource.Play();
            
            songStartTime = Time.time;
            isSongPlaying = true;
            
            Debug.Log($"음악 재생: {musicClip.name}");
        }
        
        /// <summary>
        /// 배경 음악 정지
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                isSongPlaying = false;
                Debug.Log("음악 정지됨");
            }
        }
        
        /// <summary>
        /// 배경 음악 일시정지
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
                Debug.Log("음악 일시정지됨");
            }
        }
        
        /// <summary>
        /// 배경 음악 재개
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.UnPause();
                Debug.Log("음악 재개됨");
            }
        }
        
        /// <summary>
        /// 효과음 재생
        /// </summary>
        public void PlaySFX(SFXType sfxType)
        {
            if (!isInitialized || sfxSource == null) return;
            
            if (sfxLibrary.ContainsKey(sfxType) && sfxLibrary[sfxType] != null)
            {
                sfxSource.PlayOneShot(sfxLibrary[sfxType]);
            }
            else
            {
                Debug.LogWarning($"SFX를 찾을 수 없음: {sfxType}");
            }
        }
        
        /// <summary>
        /// 키 사운드 재생
        /// </summary>
        public void PlayKeySound(KeySoundType keySoundType)
        {
            if (!isInitialized || keySoundSource == null || keySoundType == KeySoundType.None) return;
            
            if (keySoundLibrary.ContainsKey(keySoundType) && keySoundLibrary[keySoundType] != null)
            {
                keySoundSource.PlayOneShot(keySoundLibrary[keySoundType]);
            }
            else
            {
                Debug.LogWarning($"키 사운드를 찾을 수 없음: {keySoundType}");
            }
        }
        
        /// <summary>
        /// 타이밍 조정과 함께 키 사운드 재생
        /// </summary>
        public void PlayKeySoundAtInputTime(KeySoundType keySoundType, float actualInputTime, float expectedTime)
        {
            if (!isInitialized || keySoundSource == null || keySoundType == KeySoundType.None) return;
            
            if (keySoundLibrary.ContainsKey(keySoundType) && keySoundLibrary[keySoundType] != null)
            {
                // 잠재적 피치 조정을 위한 타이밍 차이 계산
                float timingDifference = actualInputTime - expectedTime;
                
                // 타이밍 정확도에 따른 약간의 피치 변화 적용
                float pitchAdjustment = 1.0f + (timingDifference * 0.1f);
                pitchAdjustment = Mathf.Clamp(pitchAdjustment, 0.8f, 1.2f);
                
                keySoundSource.pitch = pitchAdjustment;
                keySoundSource.PlayOneShot(keySoundLibrary[keySoundType]);
                
                // 재생 후 피치 초기화
                StartCoroutine(ResetPitchAfterDelay(0.1f));
                
                Debug.Log($"키 사운드 재생: {keySoundType} 피치: {pitchAdjustment:F2}");
            }
        }
        
        IEnumerator ResetPitchAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (keySoundSource != null)
                keySoundSource.pitch = 1.0f;
        }
        
        /// <summary>
        /// 현재 곡 위치를 초 단위로 가져오기
        /// </summary>
        public float GetSongPositionInSeconds()
        {
            if (!isSongPlaying || musicSource == null || !musicSource.isPlaying)
                return 0f;
                
            return musicSource.time;
        }
        
        /// <summary>
        /// BPM 기반 박자로 곡 위치 가져오기
        /// </summary>
        public float GetSongPositionInBeats(float bpm)
        {
            float songPositionInSeconds = GetSongPositionInSeconds();
            return songPositionInSeconds * (bpm / 60.0f);
        }
        
        /// <summary>
        /// 음악이 현재 재생 중인지 확인
        /// </summary>
        public bool IsMusicPlaying()
        {
            return musicSource != null && musicSource.isPlaying;
        }
        
        /// <summary>
        /// 마스터 볼륨 설정
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// 음악 볼륨 설정
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }
        
        /// <summary>
        /// SFX 볼륨 설정
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
        }
        
        /// <summary>
        /// 키 사운드 볼륨 설정
        /// </summary>
        public void SetKeySoundVolume(float volume)
        {
            keySoundVolume = Mathf.Clamp01(volume);
            if (keySoundSource != null)
                keySoundSource.volume = keySoundVolume * masterVolume;
        }
        
        /// <summary>
        /// Resources 폴더에서 오디오 클립 로드
        /// </summary>
        public AudioClip LoadAudioClip(string clipName)
        {
            AudioClip clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"오디오 클립을 찾을 수 없음: {clipName}");
            }
            return clip;
        }
        
        /// <summary>
        /// 특정 시간에 키 사운드 재생 예약 (간소화된 구현)
        /// </summary>
        public void ScheduleKeySound(KeySoundType keySoundType, float scheduledTime)
        {
            if (keySoundType == KeySoundType.None) return;
            
            float delay = scheduledTime - GetSongPositionInSeconds();
            if (delay > 0)
            {
                StartCoroutine(PlayKeySoundAfterDelay(keySoundType, delay));
            }
            else
            {
                PlayKeySound(keySoundType);
            }
        }
        
        IEnumerator PlayKeySoundAfterDelay(KeySoundType keySoundType, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayKeySound(keySoundType);
        }
        
        void Update()
        {
            // 인스펙터에서 볼륨 설정이 변경된 경우 업데이트
            ApplyVolumeSettings();
        }
        
        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}