using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 설정 데이터 구조
///
/// Phase 2-B-5 업데이트 (2025-10-27):
/// - SFX 볼륨 추가
/// - 게임플레이 설정 추가 (판정 모드, 표시 옵션)
///
/// 키 바인딩 업데이트 (2025-10-28):
/// - 각 키 모드별 커스텀 키 바인딩 지원
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

    [Header("Key Bindings")]
    // 키 모드별 커스텀 키 바인딩 리스트 (JsonUtility가 Dictionary를 지원하지 않음)
    public List<KeyBindingEntry> customKeyBindings = new List<KeyBindingEntry>();

    // 키 바인딩 프리셋 (최대 5개)
    public List<KeyBindingPreset> keyBindingPresets = new List<KeyBindingPreset>();

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
        customKeyBindings.Clear();
    }

    /// <summary>
    /// 특정 키 모드의 키 바인딩을 가져옵니다
    /// </summary>
    public KeyCode[] GetKeyBindings(int lineCount)
    {
        foreach (var entry in customKeyBindings)
        {
            if (entry.lineCount == lineCount)
            {
                return ParseKeyBindings(entry.keyBindings);
            }
        }
        return null; // 커스텀 설정이 없으면 null 반환 (기본값 사용)
    }

    /// <summary>
    /// 특정 키 모드의 키 바인딩을 설정합니다
    /// </summary>
    public void SetKeyBindings(int lineCount, KeyCode[] keys)
    {
        // 기존 항목 제거
        customKeyBindings.RemoveAll(e => e.lineCount == lineCount);

        // 새 항목 추가
        customKeyBindings.Add(new KeyBindingEntry
        {
            lineCount = lineCount,
            keyBindings = SerializeKeyBindings(keys)
        });
    }

    /// <summary>
    /// 특정 키 모드의 키 바인딩을 기본값으로 리셋합니다
    /// </summary>
    public void ResetKeyBindings(int lineCount)
    {
        customKeyBindings.RemoveAll(e => e.lineCount == lineCount);
    }

    /// <summary>
    /// KeyCode 배열을 문자열로 직렬화
    /// </summary>
    private string SerializeKeyBindings(KeyCode[] keys)
    {
        List<string> keyNames = new List<string>();
        foreach (KeyCode key in keys)
        {
            keyNames.Add(((int)key).ToString());
        }
        return string.Join(",", keyNames);
    }

    /// <summary>
    /// 문자열을 KeyCode 배열로 역직렬화
    /// </summary>
    private KeyCode[] ParseKeyBindings(string serialized)
    {
        if (string.IsNullOrEmpty(serialized))
            return new KeyCode[0];

        string[] parts = serialized.Split(',');
        List<KeyCode> keys = new List<KeyCode>();
        foreach (string part in parts)
        {
            if (int.TryParse(part.Trim(), out int keyInt))
            {
                keys.Add((KeyCode)keyInt);
            }
        }
        return keys.ToArray();
    }

    /// <summary>
    /// 프리셋 저장
    /// </summary>
    public void SavePreset(string presetName, int lineCount, KeyCode[] keys)
    {
        // 같은 이름의 프리셋 제거
        keyBindingPresets.RemoveAll(p => p.presetName == presetName && p.lineCount == lineCount);

        // 새 프리셋 추가
        keyBindingPresets.Add(new KeyBindingPreset
        {
            presetName = presetName,
            lineCount = lineCount,
            keyBindings = SerializeKeyBindings(keys)
        });
    }

    /// <summary>
    /// 프리셋 로드
    /// </summary>
    public KeyCode[] LoadPreset(string presetName, int lineCount)
    {
        var preset = keyBindingPresets.Find(p => p.presetName == presetName && p.lineCount == lineCount);
        if (preset != null)
        {
            return ParseKeyBindings(preset.keyBindings);
        }
        return null;
    }

    /// <summary>
    /// 특정 키 모드의 모든 프리셋 가져오기
    /// </summary>
    public List<KeyBindingPreset> GetPresetsForMode(int lineCount)
    {
        return keyBindingPresets.FindAll(p => p.lineCount == lineCount);
    }

    /// <summary>
    /// 프리셋 삭제
    /// </summary>
    public void DeletePreset(string presetName, int lineCount)
    {
        keyBindingPresets.RemoveAll(p => p.presetName == presetName && p.lineCount == lineCount);
    }
}

/// <summary>
/// 키 바인딩 항목 (JsonUtility 직렬화를 위해 별도 클래스 사용)
/// </summary>
[System.Serializable]
public class KeyBindingEntry
{
    public int lineCount; // 4, 5, -5, 6, 7, 8, 10
    public string keyBindings; // "100,102,106,107" (KeyCode enum 값들의 문자열)
}

/// <summary>
/// 키 바인딩 프리셋
/// </summary>
[System.Serializable]
public class KeyBindingPreset
{
    public string presetName; // 프리셋 이름
    public int lineCount; // 키 모드
    public string keyBindings; // 키 바인딩
}