using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System;

/// <summary>
/// Enhanced Audio Manager for ChartEditorBeta with AES encryption support
/// </summary>
public class AudioManagerBeta : MonoBehaviour
{
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0.1f, 2f)]
    public float playbackSpeed = 1f;
    [Range(-200f, 200f)]
    public float audioOffset = 0f; // milliseconds

    [Header("Encryption Settings")]
    public bool useEncryption = true;
    public string encryptionKey = "DefaultKey123456"; // Should be 16, 24, or 32 characters for AES

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource metronomeSource;

    [Header("Metronome Settings")]
    public AudioClip metronomeClip;
    public bool metronomeEnabled = false;
    [Range(0.1f, 2f)]
    public float metronomeVolume = 0.5f;

    // Internal state
    private AudioClip currentMusicClip;
    private string currentAudioPath;
    private float originalMusicTime = 0f;
    private bool isPlaying = false;
    private bool isPaused = false;
    private double dspStartTime = 0.0;
    private Coroutine metronomeCoroutine;

    // Encryption
    private AesCryptoServiceProvider aesProvider;
    private byte[] encryptionKeyBytes;

    // Events
    public event System.Action<float> OnTimeChanged;
    public event System.Action<bool> OnPlayStateChanged;
    public event System.Action<AudioClip> OnAudioLoaded;

    // Singleton pattern for editor use
    private static AudioManagerBeta instance;
    public static AudioManagerBeta Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AudioManagerBeta>();
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            InitializeEncryption();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyAudioSettings();
    }

    void Update()
    {
        if (isPlaying && musicSource != null && musicSource.isPlaying)
        {
            OnTimeChanged?.Invoke(GetCurrentTime());
        }
    }

    #region Initialization
    void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }

        if (metronomeSource == null)
        {
            GameObject metronomeGO = new GameObject("MetronomeSource");
            metronomeGO.transform.SetParent(transform);
            metronomeSource = metronomeGO.AddComponent<AudioSource>();
        }

        // Configure audio sources
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        metronomeSource.playOnAwake = false;
    }

    void InitializeEncryption()
    {
        if (!useEncryption) return;

        try
        {
            aesProvider = new AesCryptoServiceProvider();
            aesProvider.Mode = CipherMode.CBC;
            aesProvider.Padding = PaddingMode.PKCS7;

            // Convert key to bytes and pad/truncate to 32 bytes (256-bit)
            encryptionKeyBytes = new byte[32];
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(encryptionKey);
            int copyLength = Mathf.Min(keyBytes.Length, encryptionKeyBytes.Length);
            Array.Copy(keyBytes, encryptionKeyBytes, copyLength);

            Debug.Log("AES encryption initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize AES encryption: {e.Message}");
            useEncryption = false;
        }
    }

    void ApplyAudioSettings()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
            musicSource.pitch = playbackSpeed;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }

        if (metronomeSource != null)
        {
            metronomeSource.volume = metronomeVolume * masterVolume;
        }
    }
    #endregion

    #region Audio Loading
    public void LoadAudioFile(string filePath)
    {
        StartCoroutine(LoadAudioCoroutine(filePath));
    }

    IEnumerator LoadAudioCoroutine(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError($"Audio file not found: {filePath}");
            yield break;
        }

        currentAudioPath = filePath;
        
        // Stop current audio if playing
        StopAudio();

        try
        {
            byte[] audioData;
            
            if (useEncryption && Path.GetExtension(filePath).ToLower() == ".wav")
            {
                // Load and decrypt encrypted audio
                audioData = LoadEncryptedAudio(filePath);
            }
            else
            {
                // Load regular audio
                audioData = File.ReadAllBytes(filePath);
            }

            if (audioData != null)
            {
                AudioClip clip = LoadAudioClipFromBytes(audioData, Path.GetFileNameWithoutExtension(filePath));
                if (clip != null)
                {
                    currentMusicClip = clip;
                    musicSource.clip = clip;
                    OnAudioLoaded?.Invoke(clip);
                    Debug.Log($"Audio loaded successfully: {filePath}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load audio file: {e.Message}");
        }

        yield return null;
    }

    byte[] LoadEncryptedAudio(string filePath)
    {
        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            return DecryptAudio(encryptedData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to decrypt audio file: {e.Message}");
            return null;
        }
    }

    byte[] DecryptAudio(byte[] encryptedData)
    {
        if (!useEncryption || aesProvider == null)
            return encryptedData;

        try
        {
            // Extract IV from the first 16 bytes
            byte[] iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);

            // Extract encrypted data (skip first 16 bytes)
            byte[] encrypted = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 16, encrypted, 0, encrypted.Length);

            using (ICryptoTransform decryptor = aesProvider.CreateDecryptor(encryptionKeyBytes, iv))
            {
                return decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Decryption failed: {e.Message}");
            return null;
        }
    }

    AudioClip LoadAudioClipFromBytes(byte[] audioData, string clipName)
    {
        try
        {
            // This is a simplified WAV loader - in practice you might want to use
            // a more robust audio loading library or Unity's WWW/UnityWebRequest
            
            // For now, we'll use a basic WAV header parser
            if (audioData.Length < 44)
            {
                Debug.LogError("Invalid WAV file: too short");
                return null;
            }

            // Parse WAV header
            int sampleRate = BitConverter.ToInt32(audioData, 24);
            int channels = BitConverter.ToInt16(audioData, 22);
            int bitsPerSample = BitConverter.ToInt16(audioData, 34);
            
            // Calculate sample count
            int dataStartIndex = 44; // Standard WAV header size
            int dataSize = audioData.Length - dataStartIndex;
            int bytesPerSample = bitsPerSample / 8;
            int sampleCount = dataSize / (bytesPerSample * channels);

            // Convert to float array
            float[] samples = new float[sampleCount * channels];
            
            if (bitsPerSample == 16)
            {
                for (int i = 0; i < sampleCount * channels; i++)
                {
                    int byteIndex = dataStartIndex + i * 2;
                    if (byteIndex + 1 < audioData.Length)
                    {
                        short sample16 = BitConverter.ToInt16(audioData, byteIndex);
                        samples[i] = sample16 / 32768f;
                    }
                }
            }
            else if (bitsPerSample == 32)
            {
                for (int i = 0; i < sampleCount * channels; i++)
                {
                    int byteIndex = dataStartIndex + i * 4;
                    if (byteIndex + 3 < audioData.Length)
                    {
                        samples[i] = BitConverter.ToSingle(audioData, byteIndex);
                    }
                }
            }

            // Create AudioClip
            AudioClip clip = AudioClip.Create(clipName, sampleCount, channels, sampleRate, false);
            clip.SetData(samples, 0);
            
            return clip;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create AudioClip from bytes: {e.Message}");
            return null;
        }
    }
    #endregion

    #region Audio Playback Control
    public void PlayAudio()
    {
        if (musicSource == null || currentMusicClip == null) return;

        if (isPaused)
        {
            musicSource.UnPause();
            isPaused = false;
        }
        else
        {
            // Apply audio offset
            float offsetSeconds = audioOffset / 1000f;
            musicSource.time = Mathf.Max(0f, offsetSeconds);
            
            musicSource.Play();
            dspStartTime = AudioSettings.dspTime;
            originalMusicTime = Time.time;
        }

        isPlaying = true;
        OnPlayStateChanged?.Invoke(true);

        // Start metronome if enabled
        if (metronomeEnabled)
        {
            StartMetronome();
        }

        Debug.Log($"Audio playback started with {audioOffset}ms offset");
    }

    public void PauseAudio()
    {
        if (musicSource == null || !isPlaying) return;

        musicSource.Pause();
        isPlaying = false;
        isPaused = true;
        OnPlayStateChanged?.Invoke(false);

        StopMetronome();
        Debug.Log("Audio playback paused");
    }

    public void StopAudio()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        isPlaying = false;
        isPaused = false;
        OnPlayStateChanged?.Invoke(false);

        StopMetronome();
        Debug.Log("Audio playback stopped");
    }

    public void SeekToTime(float time)
    {
        if (musicSource == null || currentMusicClip == null) return;

        musicSource.time = Mathf.Clamp(time, 0f, currentMusicClip.length);
        
        if (isPlaying)
        {
            dspStartTime = AudioSettings.dspTime;
            originalMusicTime = Time.time;
        }

        OnTimeChanged?.Invoke(GetCurrentTime());
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Clamp(speed, 0.1f, 2f);
        if (musicSource != null)
        {
            musicSource.pitch = playbackSpeed;
        }
        
        Debug.Log($"Playback speed set to: {playbackSpeed:F2}x");
    }

    public void SetAudioOffset(float offsetMs)
    {
        audioOffset = Mathf.Clamp(offsetMs, -200f, 200f);
        Debug.Log($"Audio offset set to: {audioOffset}ms");
    }
    #endregion

    #region Metronome
    void StartMetronome()
    {
        StopMetronome();
        if (metronomeClip != null)
        {
            metronomeCoroutine = StartCoroutine(MetronomeCoroutine());
        }
    }

    void StopMetronome()
    {
        if (metronomeCoroutine != null)
        {
            StopCoroutine(metronomeCoroutine);
            metronomeCoroutine = null;
        }
    }

    IEnumerator MetronomeCoroutine()
    {
        // This would need BPM information to work properly
        float bpm = 120f; // Default, should be provided by chart editor
        float beatInterval = 60f / bpm;

        while (isPlaying)
        {
            if (metronomeSource != null && metronomeClip != null)
            {
                metronomeSource.PlayOneShot(metronomeClip);
            }
            
            yield return new WaitForSeconds(beatInterval / playbackSpeed);
        }
    }

    public void SetMetronomeBPM(float bpm)
    {
        // Restart metronome with new BPM if it's currently running
        if (metronomeCoroutine != null && metronomeEnabled)
        {
            StartMetronome();
        }
    }
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetMetronomeVolume(float volume)
    {
        metronomeVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }
    #endregion

    #region SFX Playback
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeMultiplier);
        }
    }

    public void PlayKeySound(KeySoundType keySound)
    {
        // This would integrate with the main AudioManager's key sound system
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayKeySound(keySound);
        }
    }
    #endregion

    #region Audio Information
    public float GetCurrentTime()
    {
        if (musicSource == null || currentMusicClip == null) return 0f;

        if (isPlaying && !isPaused)
        {
            // Use DSP time for precision
            double elapsedDspTime = AudioSettings.dspTime - dspStartTime;
            return musicSource.time + (float)elapsedDspTime * playbackSpeed;
        }

        return musicSource.time;
    }

    public float GetTotalTime()
    {
        return currentMusicClip != null ? currentMusicClip.length : 0f;
    }

    public float GetProgress()
    {
        float total = GetTotalTime();
        return total > 0f ? GetCurrentTime() / total : 0f;
    }

    public bool IsPlaying => isPlaying && !isPaused;
    public bool IsPaused => isPaused;
    public bool HasAudio => currentMusicClip != null;
    public AudioClip CurrentClip => currentMusicClip;
    public string CurrentAudioPath => currentAudioPath;
    #endregion

    #region Cleanup
    void OnDestroy()
    {
        StopMetronome();
        
        if (aesProvider != null)
        {
            aesProvider.Dispose();
        }

        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    #region Public Editor Interface
    public void ToggleMetronome()
    {
        metronomeEnabled = !metronomeEnabled;
        
        if (metronomeEnabled && isPlaying)
        {
            StartMetronome();
        }
        else
        {
            StopMetronome();
        }
        
        Debug.Log($"Metronome: {(metronomeEnabled ? "ON" : "OFF")}");
    }

    public void SetEncryptionKey(string key)
    {
        encryptionKey = key;
        InitializeEncryption();
    }

    public void EnableEncryption(bool enabled)
    {
        useEncryption = enabled;
        if (enabled)
        {
            InitializeEncryption();
        }
    }
    #endregion
}