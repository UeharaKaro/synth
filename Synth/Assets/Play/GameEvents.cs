using System;

/// <summary>
/// 게임 전역 이벤트 시스템
/// 노트 히트, 미스, 게임 시작 등의 이벤트를 관리
/// </summary>
public static class GameEvents
{
    // 노트 히트 이벤트 (판정 타입 전달)
    public static Action<JudgmentType> OnNoteHit;
    
    // 노트 미스 이벤트
    public static Action OnNoteMiss;
    
    // 게임 시작 이벤트 (로딩 완료 후)
    public static Action OnSongStart;
    
    // 게임 종료 이벤트
    public static Action OnSongEnd;
    
    // 콤보 변경 이벤트
    public static Action<int> OnComboChanged;
    
    // 점수 변경 이벤트
    public static Action<long> OnScoreChanged;
    
    // 퍼센트 변경 이벤트
    public static Action<float> OnPercentChanged;
    
    // HP 변경 이벤트
    public static Action<float> OnHPChanged;
    
    // 이벤트 발생 메서드들
    public static void RaiseNoteHit(JudgmentType judgment)
    {
        OnNoteHit?.Invoke(judgment);
    }
    
    public static void RaiseNoteMiss()
    {
        OnNoteMiss?.Invoke();
    }
    
    public static void RaiseSongStart()
    {
        OnSongStart?.Invoke();
    }
    
    public static void RaiseSongEnd()
    {
        OnSongEnd?.Invoke();
    }
    
    public static void RaiseComboChanged(int combo)
    {
        OnComboChanged?.Invoke(combo);
    }
    
    public static void RaiseScoreChanged(long score)
    {
        OnScoreChanged?.Invoke(score);
    }
    
    public static void RaisePercentChanged(float percent)
    {
        OnPercentChanged?.Invoke(percent);
    }
    
    public static void RaiseHPChanged(float hp)
    {
        OnHPChanged?.Invoke(hp);
    }
    
    // 모든 이벤트 구독 해제 (씬 전환 시)
    public static void ClearAllEvents()
    {
        OnNoteHit = null;
        OnNoteMiss = null;
        OnSongStart = null;
        OnSongEnd = null;
        OnComboChanged = null;
        OnScoreChanged = null;
        OnPercentChanged = null;
        OnHPChanged = null;
    }
}
