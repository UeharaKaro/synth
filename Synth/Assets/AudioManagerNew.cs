using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace ChartSystem
{
    /// <summary>
    /// Self-contained AudioManager - completely independent
    /// Simplified audio management using Unity's built-in AudioSource components
    /// No external dependencies on FMOD or SettingsManager
    /// </summary>
    public class AudioManagerNew : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource keySoundSource;
        
        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1.0f;
        [Range(0f, 1f)]
        public float musicVolume = 0.8f;
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;
        [Range(0f, 1f)]
        public float keySoundVolume = 0.8f;
        
        [Header("Audio Files")]
        public AudioClip[] sfxClips = new AudioClip[3]; // Metronome, Hit, Miss
        public AudioClip[] keySoundClips = new AudioClip[10]; // Various key sounds
        
        // Private variables
        private Dictionary<SFXType, AudioClip> sfxLibrary;
        private Dictionary<KeySoundType, AudioClip> keySoundLibrary;
        private bool isInitialized = false;
        private float songStartTime = 0f;
        private bool isSongPlaying = false;
        
        // Singleton pattern
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
            // Singleton implementation
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
            // Create audio sources if they don't exist
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
            
            // Initialize audio clip libraries
            InitializeAudioLibraries();
            
            // Apply initial volume settings
            ApplyVolumeSettings();
            
            isInitialized = true;
            Debug.Log("AudioManagerNew initialized successfully");
        }
        
        void InitializeAudioLibraries()
        {
            // Initialize SFX library
            sfxLibrary = new Dictionary<SFXType, AudioClip>();
            
            // Map SFX clips (if available)
            if (sfxClips.Length >= 3)
            {
                sfxLibrary[SFXType.Metronome] = sfxClips[0];
                sfxLibrary[SFXType.Hit] = sfxClips[1];
                sfxLibrary[SFXType.Miss] = sfxClips[2];
            }
            
            // Initialize KeySound library
            keySoundLibrary = new Dictionary<KeySoundType, AudioClip>();
            
            // Map keysound clips (if available)
            var keySoundTypes = System.Enum.GetValues(typeof(KeySoundType));
            for (int i = 0; i < keySoundClips.Length && i < keySoundTypes.Length - 1; i++) // -1 to skip 'None'
            {
                if (keySoundClips[i] != null)
                {
                    KeySoundType soundType = (KeySoundType)(i + 1); // +1 to skip 'None'
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
        /// Play background music
        /// </summary>
        public void PlayMusic(AudioClip musicClip)
        {
            if (!isInitialized || musicSource == null || musicClip == null) return;
            
            musicSource.clip = musicClip;
            musicSource.Play();
            
            songStartTime = Time.time;
            isSongPlaying = true;
            
            Debug.Log($"Playing music: {musicClip.name}");
        }
        
        /// <summary>
        /// Stop background music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                isSongPlaying = false;
                Debug.Log("Music stopped");
            }
        }
        
        /// <summary>
        /// Pause background music
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
                Debug.Log("Music paused");
            }
        }
        
        /// <summary>
        /// Resume background music
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.UnPause();
                Debug.Log("Music resumed");
            }
        }
        
        /// <summary>
        /// Play sound effect
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
                Debug.LogWarning($"SFX not found: {sfxType}");
            }
        }
        
        /// <summary>
        /// Play key sound
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
                Debug.LogWarning($"Key sound not found: {keySoundType}");
            }
        }
        
        /// <summary>
        /// Play key sound with timing adjustments
        /// </summary>
        public void PlayKeySoundAtInputTime(KeySoundType keySoundType, float actualInputTime, float expectedTime)
        {
            if (!isInitialized || keySoundSource == null || keySoundType == KeySoundType.None) return;
            
            if (keySoundLibrary.ContainsKey(keySoundType) && keySoundLibrary[keySoundType] != null)
            {
                // Calculate timing difference for potential pitch adjustment
                float timingDifference = actualInputTime - expectedTime;
                
                // Apply slight pitch variation based on timing accuracy
                float pitchAdjustment = 1.0f + (timingDifference * 0.1f);
                pitchAdjustment = Mathf.Clamp(pitchAdjustment, 0.8f, 1.2f);
                
                keySoundSource.pitch = pitchAdjustment;
                keySoundSource.PlayOneShot(keySoundLibrary[keySoundType]);
                
                // Reset pitch after playing
                StartCoroutine(ResetPitchAfterDelay(0.1f));
                
                Debug.Log($"Playing key sound: {keySoundType} with pitch: {pitchAdjustment:F2}");
            }
        }
        
        IEnumerator ResetPitchAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (keySoundSource != null)
                keySoundSource.pitch = 1.0f;
        }
        
        /// <summary>
        /// Get current song position in seconds
        /// </summary>
        public float GetSongPositionInSeconds()
        {
            if (!isSongPlaying || musicSource == null || !musicSource.isPlaying)
                return 0f;
                
            return musicSource.time;
        }
        
        /// <summary>
        /// Get song position in beats based on BPM
        /// </summary>
        public float GetSongPositionInBeats(float bpm)
        {
            float songPositionInSeconds = GetSongPositionInSeconds();
            return songPositionInSeconds * (bpm / 60.0f);
        }
        
        /// <summary>
        /// Check if music is currently playing
        /// </summary>
        public bool IsMusicPlaying()
        {
            return musicSource != null && musicSource.isPlaying;
        }
        
        /// <summary>
        /// Set master volume
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }
        
        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
        }
        
        /// <summary>
        /// Set key sound volume
        /// </summary>
        public void SetKeySoundVolume(float volume)
        {
            keySoundVolume = Mathf.Clamp01(volume);
            if (keySoundSource != null)
                keySoundSource.volume = keySoundVolume * masterVolume;
        }
        
        /// <summary>
        /// Load audio clip from Resources folder
        /// </summary>
        public AudioClip LoadAudioClip(string clipName)
        {
            AudioClip clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"Audio clip not found: {clipName}");
            }
            return clip;
        }
        
        /// <summary>
        /// Schedule key sound to play at a specific time (simplified implementation)
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
            // Update volume settings if they've changed in inspector
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