using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
// NoteData 클래스는 노트의 데이터를 저장하는 용도로 사용
public class NoteData
{
    public double timing; // 노트가 나타나야 하는 시간 (초 단위, DSP 시간 기준)
    public float beatTiming; // BPM 기준 박자 타이밍
    public int track; // 트랙(라인) 번호 (0~3, 0~6 등)
    public KeySoundType keySoundType; // 이 노트에 할당된 키사운드 타입
    public bool isLongNote; // 롱노트 여부
    public double longNoteEndTiming; // 롱노트의 끝나는 시간 (초 단위, DSP 시간 기준)

    // 생성자 함수  
    /// <summary>
    /// NoteData 클래스의 생성자입니다.
    /// 노트의 시간, 트랙, 키사운드 타입, 롱노트 여부, 롱노트 종료 시간을 초기화합니다.
    /// </summary>
    /// <param name="timing">노트가 등장해야 하는 시간 (초 단위, DSP 기준)</param>
    /// <param name="track">노트가 속한 트랙(라인) 번호</param>
    /// <param name="keySoundType">노트에 할당된 키사운드 타입</param>
    /// <param name="isLongNote">롱노트 여부 (기본값: false)</param>
    /// <param name="endTiming">롱노트의 끝나는 시간 (초 단위, 기본값: 0)</param>
    public NoteData(double timing, int track, KeySoundType keySoundType, bool isLongNote = false, double endTiming = 0)
    {
        this.timing = timing;
        this.track = track;
        this.keySoundType = keySoundType;
        this.isLongNote = isLongNote;
        this.longNoteEndTiming = endTiming;
    }

    // BPM 기준으로 박자 타이밍 계산
    public void CaculateBeatTiming(float bpm)
    {
        beatTiming = (float)(timing * bpm / 60.0f); // BPM을 초 단위로 변환하여 박자 타이밍 계산
    }
}

// 개별 오브젝트를 제어하는 클래스 
public class Note : MonoBehaviour
{
    [Header("노트 설정")] public KeySoundType keySoundType = KeySoundType.None; // 이 노트에 할당된 키사운드 타입을 저장, 기본값은 None
    public int track = 0; // 노트가 속한 트랙(라인) 번호, 기본값은 0
    public double timing = 0.0; // 노트가 등장해야 하는 시간 (초 단위, DSP 기준)
    public bool isLongNote = false; // 롱노트 여부 

    [Header("시각적 설정")] public SpriteRenderer noteRenderer; // 노트 스프라이트 렌더러
    public Color evenTrackColor = Color.white; // 짝수 트랙 노트 색상 (0,2,4,6 번 트랙 등)
    public Color oddTrackColor = Color.cyan; // 홀수 트랙 노트 색상 (1,3,5,7 번 트랙 등)

    [Header("판정 설정")] public JudgmentMode judgmentMode = JudgmentMode.JudgmentMode_Normal; // 기본 판정 모드

    [Header("Normal 모드 판정 임계값/기준(ms)")]
    public float normalSperfectThreshold = 0f; // Normal 모드 S_Perfect 판정 기준 (없음, 0으로 설정) 

    public float normalPerfectThreshold = 41.66f; // Normal 모드 Perfect 판정 기준 (ms)
    public float normalGreatThreshold = 83.33f; // Normal 모드 Great 판정 기준 (ms)
    public float normalGoodThreshold = 120f; // Normal 모드 Good 판정 기준 (ms)
    public float normalBadThreshold = 150f; // Normal 모드 Bad 판정 기준 (ms)

    [Header("Hard 모드 판정 임계값/기준(ms)")] public float hardSPerfectThreshold = 16.67f; // Hard 모드 S_Perfect 판정 기준 (ms)
    public float hardPerfectThreshold = 32.25f; // Hard 모드 Perfect 판정 기준 (ms)
    public float hardGreatThreshold = 62.49f; // Hard 모드 Great 판정 기준 (ms)
    public float hardGoodThreshold = 88.33f; // Hard 모드 Good 판정 기준 (ms)
    public float hardBadThreshold = 120f; // Hard 모드 Bad 판정 기준 (ms) 

    [Header("Super 모드 판정 임계값/기준(ms)")] public float superSPerfectThreshold = 4.17f; // Super 모드 S_Perfect 판정 기준 (ms)
    public float superPerfectThreshold = 12.50f; // Super 모드 Perfect 판정 기준 (ms)
    public float superGreatThreshold = 25.00f; // Super 모드 Great 판정 기준 (ms)
    public float superGoodThreshold = 62.49f; // Super 모드 Good 판정 기준 (ms)
    public float superBadThreshold = 0f; // Super 모드 Bad 판정 기준 (없음, 0으로 설정)

    private double moveSpeed; // 노트 이동 속도 (DSP 시간기준)
    private double targetY; // 노트가 이동해야 하는 목표 Y 위치 (판정선)
    private bool initialized = false; // 초기화되었는지 여부
    private bool isHit = false; // 노트가 쳐졌는지 여부
    private double spwanTime; // 노트가 생성된 DSP 시간
    private Vector3 startPosition; // 노트의 시작 위치
    private Vector3 targetPosition; // 노트의 목표 위치

    // 롱노트 관련 변수들
    private double longNoteEndTiming; // 롱노트가 끝나는 시간
    private bool isLongNoteHeld = false; // 롱노트가 활성화되었는지 여부
    private LineRenderer longNoteTrail; // 롱노트 시각적 표현용

    // 노트를 초기화 하는 함수
    public void Initialize(double speed, double target, NoteData noteData, double currentTime)
    {
        moveSpeed = speed; // 노트 이동 속도 설정
        targetY = target; // 목표 Y 위치 설정
        keySoundType = noteData.keySoundType; // 노트의 키사운드 타입 설정
        track = noteData.track; // 노트의 트랙 설정
        timing = noteData.timing; // 노트의 등장 시간 설정
        isLongNote = noteData.isLongNote; // 롱노트 여부 설정
        longNoteEndTiming = noteData.longNoteEndTiming; // 롱노트 종료 시간 설정
        spwanTime = currentTime; // 현재 DSP 시간 저장
        initialized = true; // 초기화 완료 플래그 설정
        isHit = false; // 노트가 쳐지지 않은 상태로 초기화
        isLongNoteHeld = false; // 롱노트 활성화 상태 초기화

        startPosition = transform.position; // 노트의 시작 위치 저장
        targetPosition = new Vector3(startPosition.x, (float)targetY, startPosition.z);

        // 노트 색상 설정
        SetNoteAppearance();

        // 롱노트인경우 시각적 트레일 설정
        if (isLongNote)
        {
            SetupLongNoteVisual();
        }
    }

    // 노트 외형 설정
    void SetNoteAppearance()
    {
        if (noteRenderer != null)
        {
            // 트랙 번호에 따른 색상 설정 (짝수/홀수 구분)'
            Color baseColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
            noteRenderer.color = baseColor;

            // 롱노트인 경우 약간 투명도 조정으로 구분 (변경 가능성있음)
            if (isLongNote)
            {
                Color longColor = baseColor;
                longColor.a = 0.8f; // 약간 투명하게 (투명도 20% 정도)
                noteRenderer.color = longColor;
            }
        }
    }

    // 롱노트 시각적 설정
    void SetupLongNoteVisual()
    {
        longNoteTrail = GetComponent<LineRenderer>();
        if (longNoteTrail == null)
        {
            longNoteTrail = gameObject.AddComponent<LineRenderer>(); // LineRenderer가 없으면 추가
        }

        longNoteTrail.material = new Material(Shader.Find("Sprites/Default")); // 기본 셰이더 사용

        // 트랙에 따른 색상으로 롱노트 트레일 설정
        Color trailColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
        longNoteTrail.startColor = trailColor;
        longNoteTrail.endColor = trailColor; // 시작과 끝 색상 동일
        // longNoteTrail.endColor = Color.Lerp(trailColor, Color.white, 0.3f); -> 끝부분을 살짝 하얗게 그라데이션

        longNoteTrail.startWidth = 0.1f; // 트레일 시작 너비
        longNoteTrail.endWidth = 0.1f; // 트레일 끝 너비
        longNoteTrail.positionCount = 2; // 시작과 끝 위치 설정
    }

    // 매 프레임마다 실행되는 함수 - 노트를 DSP 시간에 따라 정확히 이동
    void Update()
    {
        if (!initialized) return;

        // DSP 시간 기준으로 정확한 위치 계산
        // AudioManager.Instance.GetSongPositionInSeconds() 대신 임시로 Time.time을 사용
        // 실제 게임에서는 AudioManager를 통해 현재 재생 중인 곡의 DSP 시간을 가져와야 함
        double currentSongTime = GetCurrentSongTime();
        double timeUntilHit = timing - currentSongTime;

        // 시간 비율을 이용한 정확한 위치 계산 (프레임률에 무관)
        double totalTime = timing = spwanTime; // 노트가 등장해야 하는 시간
        double progress = 1.0 - (timeUntilHit / totalTime); // 현재 시간 기준으로 노트가 얼마나 진행되었는지 비율로 계산
        progress = System.Math.Max(0.0, System.Math.Min(1.0, progress)); // 0~1 사이로 제한 

        // 보간을 이용한 부드러운 이동
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, (float)progress); // 시작 위치에서 목표 위치까지 비율에 따라 보간
        transform.position = currentPos; // 현재 위치 업데이트

        // 롱노트 트레일 업데이트
        if (isLongNote && longNoteTrail != null)
        {
            UpdateLongNoteTrail();
        }
    }

    // 현재 곡 재생 시간을 가져오는 함수(임시구현)
    // 실제로는 AudioManager를 통해 현재 재생 중인 곡의 DSP 시간을 가져와야 함
    private double GetCurrentSongTime()
    {
        // 임시로 Time.time을 사용
        // 실제 게임에서는 AudioManager.Instance.GetSongPositionInSeconds()를 사용해야 함
        return Time.time; // 현재 시간 반환 (초 단위)
    }

    // 롱노트 트레일 업데이트 함수
    private void UpdateLongNoteTrail()
    {
        if (isLongNoteHeld)
        {
            // 롱노트가 활성 상태일 때 트레일을 판정선까지 연장
            longNoteTrail.SetPosition(0, startPosition); // 시작 위치
            longNoteTrail.SetPosition(1,
                new Vector3(transform.position.x, (float)targetY, transform.position.z)); // 판정선 위치
        }
        else
        {
            // 롱노트가 비활성 상태일 때 트레일 숨김
            longNoteTrail.positionCount = 0; // 트레일을 비활성화
        }
    }

    // 노트가 정확히 쳐졌을때 호출되는 함수
    public JudgmentType OnNoteHit(double hitTime)
    {
        if (isHit) return JudgmentType.Miss; // 이미 쳐진 노트는 무시

        isHit = true; // 노트가 쳐졌음을 표시

        double timeDifference = System.Math.Abs(hitTime - timing) * 1000.0;
        JudgmentType judgment = CalculateJudgment(timeDifference);

        // 키사운드 재생 (실제 구현에서는 AudioManager 사용)
        if (keySoundType != KeySoundType.None)
        {
            PlayKeySound(keySoundType);
        }

        // 롱노트인 경우 홀드 상태로 전환
        if (isLongNote)
        {
            isLongNoteHeld = true;
            // 롱노트 시각적 효과 시작
            if (longNoteTrail != null)
            {
                longNoteTrail.positionCount = 2;
            }

            return judgment; // 롱노트는 여기서 바로 반환하지 않고 홀드 상태 유지
        }

        // 일반 노트는 즉시 비활성화
        gameObject.SetActive(false);

        return judgment;
    }

    // 롱노트 릴리즈 처리
    public JudgmentType OnLongNoteRelease(double releaseTime)
    {
        if (!isLongNote || !isLongNoteHeld) return JudgmentType.Miss;

        isLongNoteHeld = false;

        // 릴리즈 타이밍 판정
        double timeDifference = System.Math.Abs(releaseTime - longNoteEndTiming) * 1000.0;
        JudgmentType judgment = CalculateJudgment(timeDifference);

        // 롱노트 릴리즈 시에도 키사운드 재생
        if (keySoundType != KeySoundType.None)
        {
            PlayKeySound(keySoundType);
        }

        // 롱노트 트레일 숨김
        if (longNoteTrail != null)
        {
            longNoteTrail.positionCount = 0;
        }

        gameObject.SetActive(false);

        return judgment;
    }

    // 타이밍 차이를 기반으로 판정 등급 계산
    JudgmentType CalculateJudgment(double timeDifferenceMs)
    {
        switch (judgmentMode)
        {
            case JudgmentMode.JudgmentMode_Normal:
                return CalculateNormalJudgment(timeDifferenceMs);

            case JudgmentMode.JudgmentMode_Hard:
                return CalculateHardJudgment(timeDifferenceMs);

            case JudgmentMode.JudgmentMode_Super:
                return CalculateSuperJudgment(timeDifferenceMs);

            default:
                return CalculateNormalJudgment(timeDifferenceMs);
        }
    }

    // Normal 모드 판정 (S_Perfect 없음)
    JudgmentType CalculateNormalJudgment(double timeDifferenceMs)
    {
        if (timeDifferenceMs <= normalPerfectThreshold) return JudgmentType.Perfect;
        else if (timeDifferenceMs <= normalGreatThreshold) return JudgmentType.Great;
        else if (timeDifferenceMs <= normalGoodThreshold) return JudgmentType.Good;
        else if (timeDifferenceMs <= normalBadThreshold) return JudgmentType.Bad;
        else return JudgmentType.Miss;
    }

    // Hard 모드 판정 (S_Perfect 포함)
    JudgmentType CalculateHardJudgment(double timeDifferenceMs)
    {
        if (timeDifferenceMs <= hardSPerfectThreshold) return JudgmentType.S_Perfect;
        else if (timeDifferenceMs <= hardPerfectThreshold) return JudgmentType.Perfect;
        else if (timeDifferenceMs <= hardGreatThreshold) return JudgmentType.Great;
        else if (timeDifferenceMs <= hardGoodThreshold) return JudgmentType.Good;
        else if (timeDifferenceMs <= hardBadThreshold) return JudgmentType.Bad;
        else return JudgmentType.Miss;
    }

    // Super 모드 판정 (S_Perfect 포함, Bad 없음)
    JudgmentType CalculateSuperJudgment(double timeDifferenceMs)
    {
        if (timeDifferenceMs <= superSPerfectThreshold) return JudgmentType.S_Perfect;
        else if (timeDifferenceMs <= superPerfectThreshold) return JudgmentType.Perfect;
        else if (timeDifferenceMs <= superGreatThreshold) return JudgmentType.Great;
        else if (timeDifferenceMs <= superGoodThreshold) return JudgmentType.Good;
        else return JudgmentType.Miss; // Bad 판정 없음, 바로 Miss
    }

    // 키사운드 재생 함수 (임시 구현)
    void PlayKeySound(KeySoundType soundType)
    {
        // 실제로는 AudioManager.Instance.PlayKeySound(soundType) 사용
        Debug.Log($"키사운드 재생: {soundType}");
    }

    // 노트를 놓쳤을 때 호출되는 함수
    public void OnNoteMiss()
    {
        if (isHit) return; // 이미 쳐진 노트는 무시

        // Miss 효과음 재생 (실제로는 AudioManager 사용)
        Debug.Log("Miss!");

        gameObject.SetActive(false);
    }

    // 이 노트를 제거해야 하는지 판단하는 함수
    public bool ShouldDestroy()
    {
        double currentTime = GetCurrentSongTime();

        // 현재 판정 모드에 따른 최대 판정 범위 계산
        float maxThreshold = GetMaxThresholdForCurrentMode();

        // 판정 시간을 최대 판정 한계를 지나면 자동으로 Miss 처리
        if (currentTime > timing + (maxThreshold / 1000.0) && !isHit)
        {
            OnNoteMiss();
            return true;
        }

        // 화면 완전히 벗어나면 제거
        return transform.position.y < targetY - 3f;
    }

    // 판정 범위 내에 있는지 확인하는 함수
    public bool IsInJudgmentRange()
    {
        double currentTime = GetCurrentSongTime();
        double timeDifference = System.Math.Abs(currentTime - timing);

        float maxThreshold = GetMaxThresholdForCurrentMode();
        return timeDifference <= (maxThreshold / 1000.0);
    }

    // 현재 판정 모드에 따른 최대 임계값 반환
    float GetMaxThresholdForCurrentMode()
    {
        switch (judgmentMode)
        {
            case JudgmentMode.JudgmentMode_Normal:
                return normalBadThreshold; // Normal 모드는 S_Perfect 없음
            case JudgmentMode.JudgmentMode_Hard:
                return hardBadThreshold;
            case JudgmentMode.JudgmentMode_Super:
                return superGoodThreshold; // Super 모드는 Good이 최대
            default:
                return normalBadThreshold;
        }
    }

    // 롱노트가 현재 홀드 중인지 학인
    public bool IsLongNoteHeld()
    {
        return isLongNote && isLongNoteHeld; // 롱노트가 활성화되어 있고 홀드 중인지 확인
    }

    // 판정 임계값을 설정하는 함수들 (외부에서 호출 가능)
    public void SetJudgmentMode(JudgmentMode mode)
    {
        judgmentMode = mode;
    }

    public void SetNormalThresholds(float perfect, float great, float good, float bad)
    {
        normalPerfectThreshold = perfect;
        normalGreatThreshold = great;
        normalGoodThreshold = good;
        normalBadThreshold = bad;
    }

    public void SetHardThresholds(float sPerfect, float perfect, float great, float good, float bad)
    {
        hardSPerfectThreshold = sPerfect;
        hardPerfectThreshold = perfect;
        hardGreatThreshold = great;
        hardGoodThreshold = good;
        hardBadThreshold = bad;
    }

    public void SetSuperThresholds(float sPerfect, float perfect, float great, float good) //
    {
        superSPerfectThreshold = sPerfect;
        superPerfectThreshold = perfect;
        superGreatThreshold = great;
        superGoodThreshold = good;
    }

    // 트랙별 색상 설정 함수들 (외부에서 호출 가능)
    public void SetTrackColors(Color evenColor, Color oddColor)
    {
        evenTrackColor = evenColor;
        oddTrackColor = oddColor;

        // 색상 변경 후 즉시 외형 업데이트
        SetNoteAppearance();

        // 롱노트 트레일 색상도 업데이트
        if (isLongNote && longNoteTrail != null)
        {
            Color trailColor = (track % 2 == 0) ? evenTrackColor : oddTrackColor;
            longNoteTrail.startColor = trailColor;
            longNoteTrail.endColor = trailColor; // 시작과 끝 색상 동일
        }
    }

    // 현재 노트의 색상 반환 
    public Color GetNoteColor()
    {
        return (track % 2 == 0) ? evenTrackColor : oddTrackColor;

    }

    // 트랙이 짝수인지 홀수인지 확인
    public bool IsEvenTrack()
    {
        return track % 2 == 0; // 짝수 트랙이면 true, 홀수 트랙이면 false 반환
    }

    // 노트의 현재 상태를 가져오는 함수들
    public double GetTiming() => timing; // 노트의 등장 시간 반환
    public double GetLongNoteEndTiming() => longNoteEndTiming; // 롱노트의 종료 시간 반환 
    public int GetTrack() => track; // 노트의 트랙 번호 반환
    public bool IsHit() => isHit; // 노트가 쳐졌는지 여부 반환
}