using UnityEngine;

/// <summary>
/// 게임 전역에서 사용되는 공통 Enum 정의
/// ChartSystem namespace와 Global namespace 모두에서 공유
/// </summary>

/// <summary>
/// 키 사운드 타입 정의
/// 노트로 트리거할 수 있는 다양한 사운드 타입들
/// </summary>
public enum KeySoundType
{
    None,       // 사운드 없음
    Kick,       // 킥 드럼 사운드
    Snare,      // 스네어 드럼 사운드
    Hihat,      // 하이햇 사운드
    Vocal1,     // 보컬 사운드 1
    Vocal2,     // 보컬 사운드 2
    Synth1,     // 신디사이저 사운드 1
    Synth2,     // 신디사이저 사운드 2
    Bass,       // 베이스 사운드
    Piano,      // 피아노 사운드
    Guitar      // 기타 사운드
}

/// <summary>
/// 효과음 타입 정의
/// </summary>
public enum SFXType
{
    Metronome,  // 메트로놈 사운드
    Hit,        // 히트 효과음
    Miss        // 미스 효과음
}

/// <summary>
/// 판정 등급 열거형
/// 타이밍 정확도에 따른 판정 등급
/// </summary>
public enum JudgmentType
{
    S_Perfect,  // 최고 판정 (Super 모드: 4.17ms, Hard 모드: 16.67ms, Normal: 없음)
    Perfect,    // 완벽 (Normal: 41.66ms, Hard: 32.25ms, Super: 12.50ms)
    Great,      // 좋음 (Normal: 83.33ms, Hard: 62.49ms, Super: 25.00ms)
    Good,       // 보통 (Normal: 120ms, Hard: 88.33ms, Super: 62.49ms)
    Bad,        // 나쁨 (Normal: 150ms, Hard: 120ms, Super: 없음 - Miss로 처리)
    Miss        // 놓침
}

/// <summary>
/// 판정 모드 열거형
/// 게임의 난이도에 따른 판정 기준
/// </summary>
public enum JudgmentMode
{
    Normal,     // 일반 모드 - 가장 관대한 타이밍, S_Perfect 없음
    Hard,       // 하드 모드 - 중간 난이도, S_Perfect 포함
    Super,      // 슈퍼 모드 - 가장 엄격한 타이밍, Bad 없음 (Good 실패 시 바로 Miss)

    // 하위 호환성을 위한 별칭
    JudgmentMode_Normal = Normal,
    JudgmentMode_Hard = Hard,
    JudgmentMode_Super = Super
}
