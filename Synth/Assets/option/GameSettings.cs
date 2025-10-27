using UnityEngine;

/// <summary>
/// 게임 설정 데이터 구조
///
/// Phase 2-B-5 업데이트 (2025-10-27):
/// - SFX 볼륨 추가
/// - 게임플레이 설정 추가 (판정 모드, 표시 옵션)
/// </summary>
[System.Serializable]
public class GameSettings
{
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;

    [Range(0f, 1f)]
    public float sfxVolume = 0.8f; // 효과음 볼륨

    [Range(-200f, 200f)]
    public float volumeOffset = 0f; // 밀리초

    [Range(-200f, 200f)]
    public float judgmentOffset = 0f; // 밀리초

    public int audioBuffer = 512; // 오디오 버퍼 (64, 128, 256, 512, 1024, 2048)

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

    [Header("Gameplay Settings")]
    public int defaultJudgmentMode = 0; // 0: Normal, 1: Hard, 2: Super

    public bool showJudgmentText = true; // 판정 텍스트 표시 (Perfect, Great 등)

    public bool showOffsetText = true; // 타이밍 오프셋 표시 (+3ms, -5ms 등)

    // 기본값으로 초기화하는 메서드
    public void ResetToDefault()
    {
        musicVolume = 0.8f;
        sfxVolume = 0.8f;
        volumeOffset = 0f;
        judgmentOffset = 0f;
        audioBuffer = 512;
        noteSize = 1f;
        trackHeight = 15f;
        trackAngle = 0f;
        trackOpacity = 0.8f;
        noteScrollSpeed = 8f;
        defaultJudgmentMode = 0;
        showJudgmentText = true;
        showOffsetText = true;
    }
}