using UnityEngine;

[System.Serializable]
public class GameSettings
{
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;
    
    [Range(-200f, 200f)]
    public float volumeOffset = 0f; // 밀리초
    
    [Range(-200f, 200f)]
    public float judgmentOffset = 0f; // 밀리초
    
    public int audioBuffer = 512; // 오디오 버퍼 예시 (64, 128, 256, 512, 1024, 2048)
    
    [Header("Visual Settings")]
    [Range(0.5f, 3f)]
    public float noteSize = 1f;
    
    [Range(5f, 30f)]
    public float trackHeight = 15f;
    
    [Range(-45f, 45f)]
    public float trackAngle = 0f; // degrees
    
    [Range(0.1f, 1f)]
    public float trackOpacity = 0.8f;
    
    [Range(1f, 20f)]
    public float noteScrollSpeed = 8f;
    
    // 기본값으로 초기화하는 메서드
    public void ResetToDefault()
    {
        musicVolume = 0.8f;
        volumeOffset = 0f;
        judgmentOffset = 0f;
        audioBuffer = 512;
        noteSize = 1f;
        trackHeight = 15f;
        trackAngle = 0f;
        trackOpacity = 0.8f;
        noteScrollSpeed = 8f;
    }
}