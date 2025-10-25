using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 차트 데이터를 기반으로 노트를 타이밍에 맞춰 스폰하는 클래스
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("노트 설정")]
    [SerializeField] private GameObject notePrefab; // 노트 프리팹
    [SerializeField] private float noteSpawnDistance = 10f; // 노트 스폰 거리 (판정선으로부터)
    [SerializeField] private float noteSpeed = 5f; // 노트 이동 속도

    [Header("스폰 위치")]
    [SerializeField] private Transform spawnContainer; // 노트 스폰 부모 오브젝트
    [SerializeField] private float judgmentLineY = 0f; // 판정선 Y 위치

    [Header("타이밍 설정")]
    [SerializeField] private float spawnOffset = 2f; // 노트를 미리 스폰하는 시간 (초)

    [Header("참조")]
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GearController gearController;

    // 현재 차트
    private ChartData currentChart;

    // 스폰 상태
    private bool isSpawning = false;
    private int currentNoteIndex = 0;

    // 스폰된 노트 목록
    private List<GameObject> spawnedNotes = new List<GameObject>();

    // Singleton 패턴
    public static NoteSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 참조 자동 찾기
        if (chartLoader == null)
            chartLoader = ChartLoader.Instance;

        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();

        if (gearController == null)
            gearController = FindObjectOfType<GearController>();

        // 스폰 컨테이너 생성
        if (spawnContainer == null)
        {
            GameObject container = new GameObject("NoteContainer");
            container.transform.SetParent(transform);
            spawnContainer = container.transform;
        }
    }

    void Start()
    {
        // GearController로부터 판정선 위치 가져오기
        if (gearController != null)
        {
            judgmentLineY = gearController.GetJudgmentLineY();
        }
    }

    /// <summary>
    /// 차트 로드 및 노트 스폰 시작
    /// </summary>
    public void LoadAndStartChart(ChartData chart)
    {
        if (chart == null)
        {
            Debug.LogError("NoteSpawner: 차트가 null입니다!");
            return;
        }

        currentChart = chart;
        currentNoteIndex = 0;

        // 기존 노트 제거
        ClearAllNotes();

        // 노트 스폰 시작
        StartCoroutine(SpawnNotesCoroutine());

        Debug.Log($"NoteSpawner: 차트 시작 - {chart.songName} ({chart.GetNoteCount()}개 노트)");
    }

    /// <summary>
    /// 노트 스폰 코루틴
    /// </summary>
    private IEnumerator SpawnNotesCoroutine()
    {
        isSpawning = true;

        // 오디오 시작 대기
        yield return new WaitForSeconds(currentChart.offset);

        while (currentNoteIndex < currentChart.notes.Count)
        {
            NoteData noteData = currentChart.notes[currentNoteIndex];

            // 현재 음악 재생 시간
            float currentTime = GetCurrentSongTime();

            // 노트를 스폰해야 하는 시간 계산
            // (노트 타이밍 - 스폰 오프셋) 시점에 스폰
            float spawnTime = (float)noteData.timing - spawnOffset;

            if (currentTime >= spawnTime)
            {
                SpawnNote(noteData);
                currentNoteIndex++;
            }

            yield return null; // 다음 프레임까지 대기
        }

        isSpawning = false;
        Debug.Log("NoteSpawner: 모든 노트 스폰 완료");
    }

    /// <summary>
    /// 개별 노트 스폰
    /// </summary>
    private void SpawnNote(NoteData noteData)
    {
        if (notePrefab == null)
        {
            Debug.LogError("NoteSpawner: 노트 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 트랙 위치 계산
        Vector3 spawnPosition = CalculateSpawnPosition(noteData.track);

        // 노트 생성
        GameObject noteObj = Instantiate(notePrefab, spawnPosition, Quaternion.identity, spawnContainer);
        noteObj.name = $"Note_T{noteData.track}_{noteData.timing:F2}s";

        // NoteController 설정
        NoteController controller = noteObj.GetComponent<NoteController>();
        if (controller != null)
        {
            Vector3 targetPosition = new Vector3(spawnPosition.x, judgmentLineY, spawnPosition.z);
            controller.Initialize(spawnPosition, targetPosition, (float)noteData.timing, noteData.track, noteData.keySoundType);
        }
        else
        {
            Debug.LogWarning("NoteSpawner: NoteController 컴포넌트를 찾을 수 없습니다!");
        }

        spawnedNotes.Add(noteObj);

        // TODO: 롱노트 처리
        if (noteData.isLongNote)
        {
            // 롱노트 시각적 표현 추가
        }
    }

    /// <summary>
    /// 트랙 번호에 따른 스폰 위치 계산
    /// </summary>
    private Vector3 CalculateSpawnPosition(int track)
    {
        // GearController로부터 트랙 위치 가져오기
        if (gearController != null)
        {
            Transform trackTransform = gearController.GetLine(track);
            if (trackTransform != null)
            {
                Vector3 pos = trackTransform.position;
                pos.y = judgmentLineY + noteSpawnDistance;
                return pos;
            }
        }

        // GearController가 없으면 기본 위치 계산
        float xOffset = (track - currentChart.keyCount / 2f + 0.5f) * 1.5f;
        return new Vector3(xOffset, judgmentLineY + noteSpawnDistance, 0f);
    }

    /// <summary>
    /// 현재 곡 재생 시간 가져오기
    /// </summary>
    private float GetCurrentSongTime()
    {
        if (audioManager != null && audioManager.IsPlaying)
        {
            return audioManager.GetMusicTime();
        }

        // AudioManager가 없거나 재생 중이 아니면 Time.time 사용 (테스트용)
        Debug.LogWarning("NoteSpawner: AudioManager가 없거나 재생 중이 아닙니다. Time.time 사용");
        return Time.time;
    }

    /// <summary>
    /// 모든 노트 제거
    /// </summary>
    public void ClearAllNotes()
    {
        foreach (GameObject note in spawnedNotes)
        {
            if (note != null)
            {
                Destroy(note);
            }
        }
        spawnedNotes.Clear();
        currentNoteIndex = 0;
    }

    /// <summary>
    /// 노트 스폰 중지
    /// </summary>
    public void StopSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    /// <summary>
    /// 노트 프리팹 설정
    /// </summary>
    public void SetNotePrefab(GameObject prefab)
    {
        notePrefab = prefab;
    }

    /// <summary>
    /// 스폰 거리 설정
    /// </summary>
    public void SetSpawnDistance(float distance)
    {
        noteSpawnDistance = distance;
    }

    /// <summary>
    /// 스폰 오프셋 설정
    /// </summary>
    public void SetSpawnOffset(float offset)
    {
        spawnOffset = offset;
    }

    /// <summary>
    /// 현재 차트 반환
    /// </summary>
    public ChartData GetCurrentChart()
    {
        return currentChart;
    }

    /// <summary>
    /// 스폰 중인지 확인
    /// </summary>
    public bool IsSpawning()
    {
        return isSpawning;
    }

    void OnDestroy()
    {
        ClearAllNotes();
    }
}
