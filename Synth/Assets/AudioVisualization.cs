using UnityEngine;
using System.Collections;

public class AudioWaveformVisualizer : MonoBehaviour
{
    [Header("Waveform Settings")]
    public LineRenderer waveformRenderer;
    public int sampleSize = 1024;
    public float waveformScale = 100f;
    public float waveformHeight = 2f;
    public Color waveformColor = Color.green;
    
    [Header("Spectrum Settings")]
    public bool showSpectrum = false;
    public LineRenderer spectrumRenderer;
    public float spectrumScale = 50f;
    
    private AudioSource audioSource;
    private float[] waveformData;
    private float[] spectrumData;
    private ChartEditor chartEditor;
    
    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
        chartEditor = FindObjectOfType<ChartEditor>();
        
        waveformData = new float[sampleSize];
        spectrumData = new float[sampleSize];
        
        SetupRenderers();
    }
    
    void Update()
    {
        if (audioSource != null && audioSource.clip != null && audioSource.isPlaying)
        {
            UpdateWaveform();
            if (showSpectrum) UpdateSpectrum();
        }
    }
    
    void SetupRenderers()
    {
        if (waveformRenderer != null)
        {
            waveformRenderer.material = new Material(Shader.Find("Sprites/Default"));
            waveformRenderer.color = waveformColor;
            waveformRenderer.startWidth = 0.05f;
            waveformRenderer.endWidth = 0.05f;
            waveformRenderer.positionCount = sampleSize;
        }
        
        if (spectrumRenderer != null)
        {
            spectrumRenderer.material = new Material(Shader.Find("Sprites/Default"));
            spectrumRenderer.color = Color.blue;
            spectrumRenderer.startWidth = 0.05f;
            spectrumRenderer.endWidth = 0.05f;
            spectrumRenderer.positionCount = sampleSize / 2;
        }
    }
    
    void UpdateWaveform()
    {
        if (waveformRenderer == null) return;
        
        audioSource.GetOutputData(waveformData, 0);
        
        for (int i = 0; i < sampleSize; i++)
        {
            float x = (float)i / sampleSize * waveformScale;
            float y = waveformData[i] * waveformHeight;
            waveformRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
    
    void UpdateSpectrum()
    {
        if (spectrumRenderer == null) return;
        
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        
        for (int i = 0; i < sampleSize / 2; i++)
        {
            float x = (float)i / (sampleSize / 2) * spectrumScale;
            float y = Mathf.Log(spectrumData[i] + 1) * waveformHeight;
            spectrumRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
    
    public void SetWaveformVisible(bool visible)
    {
        if (waveformRenderer != null)
            waveformRenderer.enabled = visible;
    }
    
    public void SetSpectrumVisible(bool visible)
    {
        showSpectrum = visible;
        if (spectrumRenderer != null)
            spectrumRenderer.enabled = visible;
    }
}

public class BeatDetector : MonoBehaviour
{
    [Header("Beat Detection")]
    public float beatThreshold = 0.3f;
    public float minBeatInterval = 0.1f;
    
    private AudioSource audioSource;
    private float[] spectrumData = new float[512];
    private float lastBeatTime;
    private float averageEnergy;
    private float[] energyHistory = new float[43]; // 1초 분량 (43 * 1/43 ≈ 1초)
    private int historyIndex;
    
    public System.Action<float> OnBeatDetected;
    
    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
    }
    
    void Update()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            DetectBeat();
        }
    }
    
    void DetectBeat()
    {
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        
        // 낮은 주파수 에너지 계산 (베이스, 킥드럼)
        float instantEnergy = 0f;
        for (int i = 0; i < 30; i++) // 낮은 주파수만
        {
            instantEnergy += spectrumData[i];
        }
        
        // 에너지 히스토리 업데이트
        energyHistory[historyIndex] = instantEnergy;
        historyIndex = (historyIndex + 1) % energyHistory.Length;
        
        // 평균 에너지 계산
        averageEnergy = 0f;
        for (int i = 0; i < energyHistory.Length; i++)
        {
            averageEnergy += energyHistory[i];
        }
        averageEnergy /= energyHistory.Length;
        
        // 비트 감지
        if (instantEnergy > averageEnergy * (1 + beatThreshold) && 
            Time.time - lastBeatTime > minBeatInterval)
        {
            lastBeatTime = Time.time;
            OnBeatDetected?.Invoke(instantEnergy);
            
            Debug.Log($"Beat detected at {Time.time:F2}s, energy: {instantEnergy:F3}");
        }
    }
    
    public void SetBeatThreshold(float threshold)
    {
        beatThreshold = Mathf.Clamp(threshold, 0.1f, 2f);
    }
}

public class MetronomeHelper : MonoBehaviour
{
    [Header("Metronome Settings")]
    public AudioClip metronomeClip;
    public float volume = 0.5f;
    public bool playOnBeat = true;
    public bool playOnMeasure = true;
    
    private AudioSource metronomeAudioSource;
    private ChartEditor chartEditor;
    private float lastBeatTime;
    private int beatCount;
    
    void Start()
    {
        chartEditor = FindObjectOfType<ChartEditor>();
        
        // 메트로놈용 오디오 소스 생성
        GameObject metronomeObj = new GameObject("Metronome");
        metronomeObj.transform.SetParent(transform);
        metronomeAudioSource = metronomeObj.AddComponent<AudioSource>();
        metronomeAudioSource.clip = metronomeClip;
        metronomeAudioSource.volume = volume;
        metronomeAudioSource.playOnAwake = false;
    }
    
    void Update()
    {
        if (chartEditor != null && metronomeClip != null)
        {
            CheckBeatTiming();
        }
    }
    
    void CheckBeatTiming()
    {
        var chart = chartEditor.GetCurrentChart();
        if (chart == null || chart.bpm <= 0) return;
        
        double currentTime = chartEditor.GetCurrentTime();
        double beatLength = 60.0 / chart.bpm;
        
        // 현재 비트 계산
        int currentBeat = (int)(currentTime / beatLength);
        
        // 새로운 비트인지 확인
        if (currentBeat != beatCount)
        {
            beatCount = currentBeat;
            
            bool isMeasureStart = (currentBeat % 4) == 0;
            
            if ((playOnBeat && !isMeasureStart) || (playOnMeasure && isMeasureStart))
            {
                PlayMetronome(isMeasureStart);
            }
        }
    }
    
    void PlayMetronome(bool isMeasureStart)
    {
        if (metronomeAudioSource != null)
        {
            // 마디 시작은 다른 음정으로
            metronomeAudioSource.pitch = isMeasureStart ? 1.2f : 1.0f;
            metronomeAudioSource.PlayOneShot(metronomeClip, volume);
        }
    }
    
    public void SetMetronomeEnabled(bool enabled)
    {
        playOnBeat = enabled;
        playOnMeasure = enabled;
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (metronomeAudioSource != null)
            metronomeAudioSource.volume = volume;
    }
}