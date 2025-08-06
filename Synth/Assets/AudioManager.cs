using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMOD; // FMOD 라이브러리를 사용하기 위해 필요

// SFX(효과음)을 정의하는 enum(열거형), 숫자대신 의미있는 이름 사용가능
public enum SFXType
{
    Metronome, // 메트로놈 소리(쉼표 유무는 선택)
    Hit, // 일반적인 히트 사운드
    Miss // 노트를 놓쳤을 때 나는 소리
}
// 키사운드 타입을 위한 enum (각 노트의 개별 키음)
public enum KeySoundType
{
    Kick,       // 킥드럼 소리
    Snare,      // 스네어 드럼 소리
    Hihat,      // 하이햇 소리
    Vocal1,     // 보컬 소리1
    Vocal2,     // 보컬 소리2
    Synth1,     // 신디사이저 소리 1
    Synth2,     // 신디사이저 소리 2
    Bass,       // 베이스 소리
    Piano,      // 피아노 소리
    Guitar,     // 기타 소리
    None,       // 소리 없음
}

public class AudioManager : MonoBehaviour // AudioManager 클래스는 FMOD를 사용하여 오디오를 관리하는 역할을 함
{
    
}
