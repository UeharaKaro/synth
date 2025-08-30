using UnityEngine;
using System; 
public class SettingManager : MonoBehaviour // 인게임 내 설정 메뉴
{
    public static SettingManager Instance { get; private set; }

    [SerializeField] private GameSettings gameSettings = new GameSettings();
    private const string SETTINGS_KEY = "RhythmGameSettings";
    
    public GameSettings Settings => gameSettings;
    
    // 설정 변경 이벤트
    public event Action OnSettingsChanged;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기 설정 적용
        ApplyAllSettings();
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(gameSettings, true);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
            
            Debug.Log("Settings saved successfully!");
            
            // 설정 변경 이벤트 발생
            OnSettingsChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save settings: {e.Message}");
        }
    }
    
    public void LoadSettings()
    {
        try
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                gameSettings = JsonUtility.FromJson<GameSettings>(json);
                Debug.Log("Settings loaded successfully!");
            }
            else
            {
                Debug.Log("No saved settings found. Using default values.");
                gameSettings = new GameSettings();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load settings: {e.Message}");
            gameSettings = new GameSettings(); // 기본값으로 복원
        }
    }
    
    public void ResetToDefault()
    {
        gameSettings.ResetToDefault();
        SaveSettings();
        Debug.Log("Settings reset to default values!");
    }
    
    private void ApplyAllSettings()
    {
        OnSettingsChanged?.Invoke();
    }
    
    // 개별 설정 변경 메서드들
    public void SetMusicVolume(float volume)
    {
        gameSettings.musicVolume = Mathf.Clamp01(volume);
        OnSettingsChanged?.Invoke();
    }
    
    public void SetVolumeOffset(float offset)
    {
        gameSettings.volumeOffset = Mathf.Clamp(offset, -200f, 200f);
        OnSettingsChanged?.Invoke();
    }
    
    public void SetJudgmentOffset(float offset)
    {
        gameSettings.judgmentOffset = Mathf.Clamp(offset, -200f, 200f);
        OnSettingsChanged?.Invoke();    
    }
    
    public void SetAudioBuffer(int buffer)
    {
        int[] validBuffers = { 64, 128, 256, 512, 1024, 2048 };
        if (Array.IndexOf(validBuffers, buffer) >= 0)
        {
            gameSettings.audioBuffer = buffer;
            OnSettingsChanged?.Invoke();
        }
    }
    
    public void SetNoteSize(float size)
    {
        gameSettings.noteSize = Mathf.Clamp(size, 0.5f, 3f);
        OnSettingsChanged?.Invoke();
    }
    
    public void SetTrackHeight(float height)
    {
        gameSettings.trackHeight = Mathf.Clamp(height, 5f, 30f);
        OnSettingsChanged?.Invoke();
    }
    
    public void SetTrackAngle(float angle)
    {
        gameSettings.trackAngle = Mathf.Clamp(angle, -45f, 45f);
        OnSettingsChanged?.Invoke();
    }
    
    public void SetTrackOpacity(float opacity)
    {
        gameSettings.trackOpacity = Mathf.Clamp(opacity, 0.1f, 1f);
        OnSettingsChanged?.Invoke();
    }
    
    public void SetNoteScrollSpeed(float speed)
    {
        gameSettings.noteScrollSpeed = Mathf.Clamp(speed, 1f, 20f);
        OnSettingsChanged?.Invoke();
    }
}