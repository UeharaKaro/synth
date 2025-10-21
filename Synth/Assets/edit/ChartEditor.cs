using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 필요
using System.IO;
using UnityEngine.Networking;

[System.Serializable]
public class ChartData
{
    public string audioFileName;
    
    public float bpm; // 곡의 BPM 정보 추가
    public List<NoteData> notes = new List<NoteData>();
}

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour
{
    // UI 요소 연결
    [Header("UI 요소")] public InputField audioPathInputField; // 오디오 파일 경로 입력 필드
    public Slider timelineSlider; // 노래 진행 상태를 보여줄 슬라이더
    public Text currentTimeText; // 현재 시간 표시 텍스트
    public Text totalTimeText; // 전체 시간 표시 텍스트

    // --- 내부 변수 ---
    private AudioSource audioSource;
    private ChartData currentChart;
    private string audioFilePath;
} // 임시 저장용 중괄호 , 편집시 해제할것

/*
void Start()
{
    audioSource = GetComponent<AudioSource>();
    currentChart = new ChartData();

    // 슬라이더 값 변경 시 오디오 위치 이동
    timelineSlider.onValueChanged.AddListener(SeekAudio);
}

void Update()
{
    // 오디오가 재생 중일 때 슬라이더와 시간 텍스트 업데이트
    if (audioSource.isPlaying)
    {
        timelineSlider.value = audioSource.time;
        currentTimeText.text = FormatTime(audioSource.time);
    }
}
}
*/ // 임시 저장용 주석, 편집시 해제할것