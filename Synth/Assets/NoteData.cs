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
    [Header("노트 설정")]
    public KeySoundType keySoundType = KeySoundType.None; // 이 노트에 할당된 키사운드 타입을 저장, 기본값은 None
    public int track = 0; // 노트가 속한 트랙(라인) 번호, 기본값은 0
    public double timing = 0.0; // 노트가 등장해야 하는 시간 (초 단위, DSP 기준)
    public bool isLongNote = false; // 롱노트 여부 

    [Header("시각적 설정")]
    public SpriteRenderer noteRenderer; // 노트 스프라이트 렌더러
    public Color evenTrackColor = Color.white;  // 짝수 트랙 노트 색상 (0,2,4,6 번 트랙 등)
    public Color oddTrackColor = Color.cyan; // 홀수 트랙 노트 색상 (1,3,5,7 번 트랙 등)
    
    [Header("판정 설정")]
    public JudgmentMode JudgmentMode = JudgmentMode.JudgmentMode_Normal // 기본 판정 모드
        
    [Header("Normal 모드 판정 임계값/기준(ms)")]
    public float normalPerfectThreshold = 41.66f; // Normal 모드 Perfect 판정 기준 (ms)
    public float normalGreatThreshold = 83.33f; // Normal 모드 Great 판정 기준 (ms)
    public float normalGoodThreshold = 120f; // Normal 모드 Good 판정 기준 (ms)
    public float normalBadThreshold = 150f; // Normal 모드 Bad 판정 기준 (ms)
    
    [Header("Hard 모드 판정 임계값/기준(ms)")]
    public float hardSPerfectThreshold = 16.67f; // Hard 모드 S_Perfect 판정 기준 (ms)
    public float hardPerfectThreshold = 32.25f; // Hard 모드 Perfect 판정 기준 (ms)
    public float hardGreatThreshold = 62.49f; // Hard 모드 Great 판정 기준 (ms)
    public float hardGoodThreshold = 88.33f; // Hard 모드 Good 판정 기준 (ms)
    public float hardBadThreshold = 120f; // Hard 모드 Bad 판정 기준 (ms) 