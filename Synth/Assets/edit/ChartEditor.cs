using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace ChartSystem
{
    /// <summary>
    /// 통합 차트 에디터
    /// DEVELOPMENT_TODO.md의 요구사항을 기반으로 한 완전한 차트 편집 도구
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ChartEditor : MonoBehaviour
    {
        #region UI References
        [Header("오디오 컨트롤 UI")]
        public InputField audioPathInputField;
        public Slider timelineSlider;
        public Text currentTimeText;
        public Text totalTimeText;
        public Button loadAudioButton;
        public Button playButton;
        public Button pauseButton;
        public Button stopButton;

        [Header("차트 정보 UI")]
        public InputField songNameInput;
        public InputField artistNameInput;
        public InputField bpmInput;
        public InputField offsetInput; // 노래 오프셋 (ms)

        [Header("에디터 상태 UI")]
        public Text modeText; // 현재 모드 표시 (Normal/Long/Slide)
        public Text gridSnapText; // 그리드 스냅 표시 (1/4, 1/8 등)
        public Text statusText; // 상태 메시지

        [Header("비주얼 타임라인")]
        public RectTransform timelineContainer; // 타임라인 컨테이너
        public GameObject noteVisualPrefab; // 노트 시각화 프리팹
        public GameObject beatLinePrefab; // 박자선 프리팹
        public GameObject measureLinePrefab; // 마디선 프리팹
        public RectTransform playheadIndicator; // 재생 헤드 표시
        public ScrollRect timelineScrollRect; // 타임라인 스크롤
        public float pixelsPerSecond = 100f; // 1초당 픽셀 수

        [Header("오디오 파형 시각화")]
        public RawImage waveformDisplay; // 파형 표시 UI
        public int waveformResolution = 1000; // 파형 해상도
        public Color waveformColor = Color.cyan; // 파형 색상
        #endregion

        #region Chart Settings
        [Header("차트 설정")]
        public string songName = "";
        public string artistName = "";
        public float bpm = 120f;
        public float offset = 0f; // 오디오 오프셋 (초 단위)

        [Header("에디터 설정")]
        public int keyCount = 4; // 4K, 5K, 6K, 7K, 8K, 10K
        public KeyCode[] trackKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
        public Transform[] noteSpawnPoints;
        public GameObject notePrefab;
        #endregion

        #region Note Input Settings
        [Header("노트 입력 설정")]
        public JudgmentMode defaultJudgmentMode = JudgmentMode.Normal;

        // 노트 타입 전환
        private enum NoteInputMode
        {
            Normal,     // N키
            Long,       // L키
            Slide       // S키 (추후 구현)
        }
        private NoteInputMode currentNoteMode = NoteInputMode.Normal;
        #endregion

        #region Grid and Measure Settings
        [Header("박자선 설정 (에디터 전용, 플레이 시 비표시)")]
        public InputField subdivisionInputField;  // subdivision 직접 입력
        public Slider subdivisionSlider;          // subdivision 슬라이더 (1~100)
        public int currentSubdivision = 16;       // 현재 박자 분할 (1~100분 음표, 기본 16)
        public bool gridSnapEnabled = true;       // 그리드 스냅 활성화

        [Header("마디선 설정 (플레이 시 표시)")]
        public InputField beatsPerMeasureInput;   // 기본 마디당 박자 수 입력
        public Button addMeasureOverrideButton;   // 마디 오버라이드 추가 버튼
        #endregion

        #region Private Variables
        // 오디오
        private AudioSource audioSource;
        private string audioFilePath;
        private bool isPlaying = false;
        private bool isPaused = false;

        // 차트 데이터
        private ChartDataNew currentChart;
        private List<GameObject> spawnedNoteVisuals = new List<GameObject>();

        // 녹음/편집 상태
        private bool isRecording = false;
        private float lastNoteTime = 0f;
        private KeySoundType selectedKeySoundType = KeySoundType.None;

        // 롱노트 입력 상태
        private bool isPlacingLongNote = false;
        private NoteData longNoteStart = null;
        private int longNoteTrack = -1;

        // Undo/Redo 시스템
        private Stack<ChartDataNew> undoStack = new Stack<ChartDataNew>();
        private Stack<ChartDataNew> redoStack = new Stack<ChartDataNew>();
        private const int MAX_UNDO_STACK = 50;

        // 변속 타입 토글 (T키)
        private enum EditScopeType
        {
            PerNote,    // 1번: 노트 1개당 개별 지정
            PerMeasure  // 2번: n번 마디 ~ N번 마디 범위 지정
        }
        private EditScopeType currentEditScope = EditScopeType.PerNote;

        // 타임라인 시각화
        private List<GameObject> timelineNoteVisuals = new List<GameObject>();
        private List<GameObject> timelineBeatLines = new List<GameObject>();
        private List<GameObject> timelineMeasureLines = new List<GameObject>();
        private bool timelineNeedsRefresh = false;

        // 마우스 편집
        private bool isDraggingNewNote = false;
        private double dragStartTime = 0;
        private int dragCurrentTrack = 0;
        private GameObject previewNoteObject = null;
        private List<NoteData> selectedNotes = new List<NoteData>();

        // 복사/붙여넣기
        private List<NoteData> clipboard = new List<NoteData>();
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            InitializeEditor();
        }

        void Start()
        {
            SetupUIEvents();
            UpdateModeDisplay();
        }

        void Update()
        {
            UpdateTimeline();
            HandleKeyboardInput();
            HandleMouseInput();
            UpdatePlayheadPosition();

            if (timelineNeedsRefresh)
            {
                RefreshTimelineVisuals();
                timelineNeedsRefresh = false;
            }
        }

        void OnDestroy()
        {
            CleanupEditor();
        }
        #endregion

        #region Initialization
        void InitializeEditor()
        {
            // 오디오 소스 초기화
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // 차트 데이터 초기화
            currentChart = new ChartDataNew();

            // 노트 프리팹이 없으면 기본 생성
            if (notePrefab == null)
            {
                notePrefab = CreateDefaultNotePrefab();
            }

            Debug.Log("ChartEditor 초기화 완료");
        }

        GameObject CreateDefaultNotePrefab()
        {
            GameObject prefab = new GameObject("Note");
            SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = Color.white;
            return prefab;
        }

        Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        void SetupUIEvents()
        {
            // 오디오 컨트롤
            if (loadAudioButton != null)
                loadAudioButton.onClick.AddListener(OnLoadAudioButtonClicked);

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);

            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopButtonClicked);

            // 타임라인
            if (timelineSlider != null)
                timelineSlider.onValueChanged.AddListener(SeekAudio);

            // 차트 정보 입력 필드
            if (songNameInput != null)
                songNameInput.onEndEdit.AddListener(value => songName = value);

            if (artistNameInput != null)
                artistNameInput.onEndEdit.AddListener(value => artistName = value);

            if (bpmInput != null)
                bpmInput.onEndEdit.AddListener(value => {
                    if (float.TryParse(value, out float newBpm))
                        SetBPM(newBpm);
                });

            if (offsetInput != null)
                offsetInput.onEndEdit.AddListener(value => {
                    if (float.TryParse(value, out float newOffset))
                        offset = newOffset / 1000f; // ms를 초로 변환
                });

            // Subdivision 입력 필드
            if (subdivisionInputField != null)
            {
                subdivisionInputField.onEndEdit.AddListener(value => {
                    if (int.TryParse(value, out int newSubdivision))
                        SetSubdivision(newSubdivision);
                });
            }

            // Subdivision 슬라이더
            if (subdivisionSlider != null)
            {
                subdivisionSlider.minValue = 1;
                subdivisionSlider.maxValue = 100;
                subdivisionSlider.wholeNumbers = true;
                subdivisionSlider.value = currentSubdivision;
                subdivisionSlider.onValueChanged.AddListener(value => SetSubdivision((int)value));
            }

            // 마디당 박자 수 입력
            if (beatsPerMeasureInput != null)
            {
                beatsPerMeasureInput.text = currentChart.defaultBeatsPerMeasure.ToString();
                beatsPerMeasureInput.onEndEdit.AddListener(value => {
                    if (int.TryParse(value, out int newBeats) && newBeats > 0)
                    {
                        currentChart.defaultBeatsPerMeasure = newBeats;
                        RequestTimelineRefresh();
                        ShowStatus($"기본 마디: {newBeats}박자");
                    }
                });
            }

            // 마디 오버라이드 추가 버튼 (TODO: 별도 UI 패널 필요)
            if (addMeasureOverrideButton != null)
            {
                addMeasureOverrideButton.onClick.AddListener(OnAddMeasureOverrideClicked);
            }
        }

        void CleanupEditor()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            ClearNoteVisuals();
        }
        #endregion

        #region Keyboard Input
        void HandleKeyboardInput()
        {
            // 노트 타입 전환
            if (Input.GetKeyDown(KeyCode.N))
            {
                currentNoteMode = NoteInputMode.Normal;
                UpdateModeDisplay();
                ShowStatus("일반 노트 모드");
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                currentNoteMode = NoteInputMode.Long;
                UpdateModeDisplay();
                ShowStatus("롱노트 모드");
            }
            else if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            {
                // Ctrl+S: 저장
                SaveChart();
            }
            else if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                // Ctrl+Z: 되돌리기
                if (Input.GetKey(KeyCode.LeftShift))
                    Redo(); // Ctrl+Shift+Z: 다시 실행
                else
                    Undo();
            }
            else if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            {
                // Ctrl+C: 복사
                CopySelectedNotes();
            }
            else if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl))
            {
                // Ctrl+V: 붙여넣기
                PasteNotes();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                // G키: 그리드 스냅 ON/OFF 토글
                ToggleGridSnap();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                // T키: 편집 범위 토글 (노트별 ↔ 마디별)
                ToggleEditScope();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                // Space: 재생/일시정지 토글
                if (isPlaying)
                    OnPauseButtonClicked();
                else
                    OnPlayButtonClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                // Delete/Backspace: 선택된 노트 삭제
                DeleteSelectedNotes();
            }

            // 녹음 중 노트 입력
            if (isRecording && audioSource.isPlaying)
            {
                HandleNoteInput();
            }
        }

        void HandleNoteInput()
        {
            for (int i = 0; i < trackKeys.Length && i < keyCount; i++)
            {
                if (Input.GetKeyDown(trackKeys[i]))
                {
                    HandleNoteInputForTrack(i);
                }
            }
        }

        void HandleNoteInputForTrack(int track)
        {
            double currentTime = audioSource.time;

            switch (currentNoteMode)
            {
                case NoteInputMode.Normal:
                    AddNormalNote(currentTime, track);
                    break;

                case NoteInputMode.Long:
                    HandleLongNoteInput(currentTime, track);
                    break;

                case NoteInputMode.Slide:
                    // TODO: 슬라이드 노트 구현
                    ShowStatus("슬라이드 노트는 아직 구현되지 않았습니다");
                    break;
            }
        }

        void AddNormalNote(double timing, int track)
        {
            // 그리드 스냅 적용
            if (gridSnapEnabled)
            {
                timing = SnapToGrid(timing);
            }

            // 너무 가까운 노트 방지
            if (timing - lastNoteTime < 0.05f)
                return;

            NoteData noteData = new NoteData(timing, track, selectedKeySoundType);
            noteData.CalculateBeatTiming(bpm);

            SaveStateForUndo();
            currentChart.AddNote(noteData);
            lastNoteTime = (float)timing;

            // 타임라인 새로고침 요청
            RequestTimelineRefresh();

            ShowStatus($"노트 추가: {timing:F2}초, 트랙 {track}");
            Debug.Log($"일반 노트 추가 - 시간: {timing:F2}초, 트랙: {track}");
        }

        void HandleLongNoteInput(double timing, int track)
        {
            if (!isPlacingLongNote)
            {
                // 롱노트 시작점 설정
                if (gridSnapEnabled)
                    timing = SnapToGrid(timing);

                longNoteStart = new NoteData(timing, track, selectedKeySoundType, true, 0);
                longNoteTrack = track;
                isPlacingLongNote = true;

                ShowStatus($"롱노트 시작: {timing:F2}초, 트랙 {track} (다시 눌러 종료 지점 설정)");
            }
            else if (longNoteTrack == track)
            {
                // 롱노트 종료점 설정
                if (gridSnapEnabled)
                    timing = SnapToGrid(timing);

                // 시작점과 종료점 중 작은 값이 시작, 큰 값이 종료
                double startTime = System.Math.Min(longNoteStart.timing, timing);
                double endTime = System.Math.Max(longNoteStart.timing, timing);

                if (endTime - startTime < 0.1) // 최소 길이 체크
                {
                    ShowStatus("롱노트가 너무 짧습니다 (최소 0.1초)");
                    isPlacingLongNote = false;
                    return;
                }

                NoteData longNote = new NoteData(startTime, track, selectedKeySoundType, true, endTime);
                longNote.CalculateBeatTiming(bpm);

                SaveStateForUndo();
                currentChart.AddNote(longNote);

                // 타임라인 새로고침 요청
                RequestTimelineRefresh();

                ShowStatus($"롱노트 추가: {startTime:F2}초 ~ {endTime:F2}초, 트랙 {track}");
                Debug.Log($"롱노트 추가 - 시작: {startTime:F2}초, 종료: {endTime:F2}초, 트랙: {track}");

                isPlacingLongNote = false;
                longNoteStart = null;
            }
            else
            {
                ShowStatus("롱노트는 같은 트랙에서 시작과 종료를 지정해야 합니다");
            }
        }

        /// <summary>
        /// 마우스 입력을 처리합니다 (타임라인에서 노트 드래그 추가)
        /// </summary>
        void HandleMouseInput()
        {
            if (timelineContainer == null || audioSource.clip == null)
                return;

            // 마우스 왼쪽 버튼을 누르기 시작
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                if (IsMouseOverTimeline(mousePos))
                {
                    // Shift 키가 눌려있으면 노트 선택 모드
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        TrySelectNoteAtPosition(mousePos);
                    }
                    else
                    {
                        // 새 노트 배치 모드
                        StartNoteDrag(mousePos);
                    }
                }
            }

            // 마우스 드래그 중
            if (Input.GetMouseButton(0) && isDraggingNewNote)
            {
                UpdateNoteDrag(Input.mousePosition);
            }

            // 마우스 버튼을 뗌 (노트 배치 완료)
            if (Input.GetMouseButtonUp(0) && isDraggingNewNote)
            {
                FinishNoteDrag(Input.mousePosition);
            }
        }

        /// <summary>
        /// 마우스가 타임라인 위에 있는지 확인합니다
        /// </summary>
        bool IsMouseOverTimeline(Vector2 mousePos)
        {
            if (timelineContainer == null)
                return false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                timelineContainer,
                mousePos,
                null,
                out Vector2 localPoint
            );

            return timelineContainer.rect.Contains(localPoint);
        }

        /// <summary>
        /// 노트 드래그를 시작합니다
        /// </summary>
        void StartNoteDrag(Vector2 mousePos)
        {
            isDraggingNewNote = true;

            // 마우스 위치를 타임라인 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                timelineContainer,
                mousePos,
                null,
                out Vector2 localPoint
            );

            // 타임과 트랙 계산
            dragStartTime = localPoint.x / pixelsPerSecond;
            dragCurrentTrack = CalculateTrackFromYPosition(localPoint.y);

            // 그리드 스냅 적용
            if (gridSnapEnabled)
            {
                dragStartTime = SnapToGrid(dragStartTime);
            }

            // 프리뷰 노트 생성
            if (noteVisualPrefab != null)
            {
                previewNoteObject = Instantiate(noteVisualPrefab, timelineContainer);
                RectTransform rt = previewNoteObject.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float xPos = (float)dragStartTime * pixelsPerSecond;
                    float yPos = CalculateNoteYPosition(dragCurrentTrack);
                    rt.anchoredPosition = new Vector2(xPos, yPos);

                    // 반투명하게 표시
                    Image img = previewNoteObject.GetComponent<Image>();
                    if (img != null)
                    {
                        Color col = GetTrackColor(dragCurrentTrack);
                        col.a = 0.5f;
                        img.color = col;
                    }
                }
            }

            ShowStatus($"노트 배치 시작: {dragStartTime:F2}초");
        }

        /// <summary>
        /// 노트 드래그를 업데이트합니다
        /// </summary>
        void UpdateNoteDrag(Vector2 mousePos)
        {
            if (previewNoteObject == null)
                return;

            // 마우스 위치를 타임라인 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                timelineContainer,
                mousePos,
                null,
                out Vector2 localPoint
            );

            // 현재 트랙 업데이트
            int newTrack = CalculateTrackFromYPosition(localPoint.y);
            if (newTrack != dragCurrentTrack)
            {
                dragCurrentTrack = newTrack;

                // 프리뷰 노트 색상 업데이트
                Image img = previewNoteObject.GetComponent<Image>();
                if (img != null)
                {
                    Color col = GetTrackColor(dragCurrentTrack);
                    col.a = 0.5f;
                    img.color = col;
                }
            }

            // 프리뷰 노트 위치 업데이트
            RectTransform rt = previewNoteObject.GetComponent<RectTransform>();
            if (rt != null)
            {
                float xPos = (float)dragStartTime * pixelsPerSecond;
                float yPos = CalculateNoteYPosition(dragCurrentTrack);
                rt.anchoredPosition = new Vector2(xPos, yPos);

                // 롱노트 모드인 경우 길이 표시
                if (currentNoteMode == NoteInputMode.Long)
                {
                    double currentTime = localPoint.x / pixelsPerSecond;
                    if (gridSnapEnabled)
                    {
                        currentTime = SnapToGrid(currentTime);
                    }

                    double duration = System.Math.Abs(currentTime - dragStartTime);
                    if (duration > 0.1)
                    {
                        rt.sizeDelta = new Vector2((float)duration * pixelsPerSecond, rt.sizeDelta.y);
                    }
                }
            }

            ShowStatus($"트랙: {dragCurrentTrack + 1}/{keyCount}");
        }

        /// <summary>
        /// 노트 드래그를 완료하고 노트를 추가합니다
        /// </summary>
        void FinishNoteDrag(Vector2 mousePos)
        {
            isDraggingNewNote = false;

            // 프리뷰 노트 제거
            if (previewNoteObject != null)
            {
                Destroy(previewNoteObject);
                previewNoteObject = null;
            }

            // 유효한 트랙인지 확인
            if (dragCurrentTrack < 0 || dragCurrentTrack >= keyCount)
            {
                ShowStatus("유효하지 않은 트랙입니다");
                return;
            }

            // 노트 추가
            if (currentNoteMode == NoteInputMode.Normal)
            {
                // 일반 노트 추가
                AddNormalNote(dragStartTime, dragCurrentTrack);
            }
            else if (currentNoteMode == NoteInputMode.Long)
            {
                // 롱노트 추가
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    timelineContainer,
                    mousePos,
                    null,
                    out Vector2 localPoint
                );

                double endTime = localPoint.x / pixelsPerSecond;
                if (gridSnapEnabled)
                {
                    endTime = SnapToGrid(endTime);
                }

                // 시작과 끝 시간 정렬
                double startTime = System.Math.Min(dragStartTime, endTime);
                double finalEndTime = System.Math.Max(dragStartTime, endTime);

                // 최소 길이 체크
                if (finalEndTime - startTime < 0.1)
                {
                    ShowStatus("롱노트가 너무 짧습니다 (최소 0.1초)");
                    return;
                }

                // 롱노트 생성
                NoteData longNote = new NoteData(startTime, dragCurrentTrack, selectedKeySoundType, true, finalEndTime);
                longNote.CalculateBeatTiming(bpm);

                SaveStateForUndo();
                currentChart.AddNote(longNote);

                // 타임라인 새로고침
                RequestTimelineRefresh();

                ShowStatus($"롱노트 추가: {startTime:F2}초 ~ {finalEndTime:F2}초, 트랙 {dragCurrentTrack}");
                Debug.Log($"롱노트 추가 (마우스) - 시작: {startTime:F2}초, 종료: {finalEndTime:F2}초, 트랙: {dragCurrentTrack}");
            }
        }

        /// <summary>
        /// Y 위치로부터 트랙 번호를 계산합니다
        /// </summary>
        int CalculateTrackFromYPosition(float yPos)
        {
            float trackHeight = 30f;
            float startY = -trackHeight * keyCount / 2f;
            int track = Mathf.FloorToInt((yPos - startY) / trackHeight);
            return Mathf.Clamp(track, 0, keyCount - 1);
        }

        /// <summary>
        /// 지정된 위치에 있는 노트를 선택하려고 시도합니다
        /// </summary>
        void TrySelectNoteAtPosition(Vector2 mousePos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                timelineContainer,
                mousePos,
                null,
                out Vector2 localPoint
            );

            double clickTime = localPoint.x / pixelsPerSecond;
            int clickTrack = CalculateTrackFromYPosition(localPoint.y);

            // 클릭 위치 근처의 노트 찾기 (0.2초 오차 범위)
            NoteData foundNote = null;
            double minDistance = 0.2;

            foreach (NoteData note in currentChart.notes)
            {
                if (note.track == clickTrack)
                {
                    double distance = System.Math.Abs(note.timing - clickTime);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        foundNote = note;
                    }
                }
            }

            if (foundNote != null)
            {
                // 이미 선택된 노트라면 선택 해제
                if (selectedNotes.Contains(foundNote))
                {
                    selectedNotes.Remove(foundNote);
                    ShowStatus($"노트 선택 해제: {foundNote.timing:F2}초");
                }
                else
                {
                    selectedNotes.Add(foundNote);
                    ShowStatus($"노트 선택: {foundNote.timing:F2}초, 트랙 {foundNote.track} (총 {selectedNotes.Count}개 선택)");
                }

                // 선택된 노트 하이라이트 업데이트
                RefreshNoteSelection();
            }
        }

        /// <summary>
        /// 선택된 노트들을 삭제합니다
        /// </summary>
        void DeleteSelectedNotes()
        {
            if (selectedNotes.Count == 0)
            {
                ShowStatus("선택된 노트가 없습니다");
                return;
            }

            SaveStateForUndo();

            int deleteCount = selectedNotes.Count;
            foreach (NoteData note in selectedNotes)
            {
                currentChart.notes.Remove(note);
            }

            selectedNotes.Clear();

            // 타임라인 새로고침
            RequestTimelineRefresh();

            ShowStatus($"{deleteCount}개 노트 삭제 완료");
            Debug.Log($"{deleteCount}개 노트 삭제");
        }

        /// <summary>
        /// 선택된 노트의 시각적 표시를 업데이트합니다
        /// </summary>
        void RefreshNoteSelection()
        {
            // 간단한 구현: 타임라인 전체 새로고침
            // 나중에 최적화 가능 (선택된 노트만 테두리 표시 등)
            RequestTimelineRefresh();
        }

        /// <summary>
        /// 선택된 노트들을 클립보드에 복사합니다
        /// </summary>
        void CopySelectedNotes()
        {
            if (selectedNotes.Count == 0)
            {
                ShowStatus("선택된 노트가 없습니다");
                return;
            }

            clipboard.Clear();

            // 선택된 노트들을 복사 (깊은 복사)
            foreach (NoteData note in selectedNotes)
            {
                NoteData noteCopy = new NoteData(
                    note.timing,
                    note.track,
                    note.keySoundType,
                    note.isLongNote,
                    note.longNoteEndTiming
                );
                // beatTiming은 CalculateBeatTiming()으로 자동 계산되므로 복사 불필요
                clipboard.Add(noteCopy);
            }

            ShowStatus($"{clipboard.Count}개 노트 복사 완료");
            Debug.Log($"{clipboard.Count}개 노트를 클립보드에 복사");
        }

        /// <summary>
        /// 클립보드의 노트들을 현재 재생 위치에 붙여넣기합니다
        /// </summary>
        void PasteNotes()
        {
            if (clipboard.Count == 0)
            {
                ShowStatus("클립보드가 비어있습니다");
                return;
            }

            SaveStateForUndo();

            // 클립보드 노트들의 최소 시간 계산
            double minTime = double.MaxValue;
            foreach (NoteData note in clipboard)
            {
                if (note.timing < minTime)
                    minTime = note.timing;
            }

            // 현재 재생 시간 (또는 0)
            double currentTime = audioSource != null ? audioSource.time : 0;

            // 그리드 스냅 적용
            if (gridSnapEnabled)
            {
                currentTime = SnapToGrid(currentTime);
            }

            // 시간 오프셋 계산
            double timeOffset = currentTime - minTime;

            // 노트들을 새 위치에 붙여넣기
            int pastedCount = 0;
            foreach (NoteData note in clipboard)
            {
                NoteData newNote = new NoteData(
                    note.timing + timeOffset,
                    note.track,
                    note.keySoundType,
                    note.isLongNote,
                    note.isLongNote ? note.longNoteEndTiming + timeOffset : 0
                );
                newNote.CalculateBeatTiming(bpm);

                currentChart.AddNote(newNote);
                pastedCount++;
            }

            // 붙여넣은 노트들을 선택
            selectedNotes.Clear();
            // 마지막에 추가된 노트들 선택 (간단히 시간 범위로 필터링)
            double pasteStartTime = currentTime;
            double pasteEndTime = currentTime + (clipboard[clipboard.Count - 1].timing - minTime) + 0.1;
            foreach (NoteData note in currentChart.notes)
            {
                if (note.timing >= pasteStartTime && note.timing <= pasteEndTime)
                {
                    selectedNotes.Add(note);
                }
            }

            // 타임라인 새로고침
            RequestTimelineRefresh();

            ShowStatus($"{pastedCount}개 노트 붙여넣기 완료 ({currentTime:F2}초)");
            Debug.Log($"{pastedCount}개 노트를 {currentTime:F2}초 위치에 붙여넣기");
        }
        #endregion

        #region Grid System
        /// <summary>
        /// G키로 그리드 스냅 토글 (ON/OFF만)
        /// </summary>
        void ToggleGridSnap()
        {
            gridSnapEnabled = !gridSnapEnabled;
            UpdateGridDisplay();
            ShowStatus(gridSnapEnabled ? $"그리드 스냅: ON (1/{currentSubdivision})" : "그리드 스냅: OFF");
        }

        /// <summary>
        /// subdivision 값을 설정합니다 (1~100)
        /// </summary>
        public void SetSubdivision(int value)
        {
            currentSubdivision = Mathf.Clamp(value, 1, 100);
            UpdateGridDisplay();
            RefreshTimelineVisuals();
            ShowStatus($"박자 분할: 1/{currentSubdivision}분 음표");
        }

        double SnapToGrid(double time)
        {
            if (!gridSnapEnabled || bpm <= 0)
                return time;

            // subdivision 범위 제한 (1~100)
            int safeSubdivision = Mathf.Clamp(currentSubdivision, 1, 100);

            // 1박자 길이 (초)
            double beatLength = 60.0 / bpm;

            // subdivision분 음표 간격 계산
            // currentSubdivision = n이면 n분음표
            double gridInterval = beatLength * (4.0 / safeSubdivision);

            // 가장 가까운 그리드에 스냅
            double snappedTime = System.Math.Round(time / gridInterval) * gridInterval;

            return snappedTime;
        }

        void UpdateGridDisplay()
        {
            if (gridSnapText != null)
            {
                if (gridSnapEnabled)
                    gridSnapText.text = $"Grid: 1/{currentSubdivision}";
                else
                    gridSnapText.text = "Grid: OFF";
            }

            // subdivision 입력 필드 업데이트
            if (subdivisionInputField != null)
            {
                subdivisionInputField.text = currentSubdivision.ToString();
            }

            // subdivision 슬라이더 업데이트
            if (subdivisionSlider != null)
            {
                subdivisionSlider.value = currentSubdivision;
            }
        }
        #endregion

        #region Audio Control
        public void OnLoadAudioButtonClicked()
        {
            string path = audioPathInputField != null ? audioPathInputField.text : "";
            if (string.IsNullOrEmpty(path))
            {
                ShowStatus("오디오 파일 경로를 입력하세요");
                return;
            }
            StartCoroutine(LoadAudio(path));
        }

        public void OnPlayButtonClicked()
        {
            if (audioSource.clip == null)
            {
                ShowStatus("먼저 오디오 파일을 로드하세요");
                return;
            }

            if (isPaused)
            {
                audioSource.UnPause();
                isPaused = false;
            }
            else
            {
                audioSource.Play();
            }

            isPlaying = true;
            isRecording = true;
            ShowStatus("재생 시작 - 녹음 모드 활성화");
        }

        public void OnPauseButtonClicked()
        {
            if (!isPlaying)
                return;

            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                isPaused = true;
                isRecording = false;
                ShowStatus("일시정지");
            }
            else if (isPaused)
            {
                audioSource.UnPause();
                isPaused = false;
                isRecording = true;
                ShowStatus("재개");
            }
        }

        public void OnStopButtonClicked()
        {
            audioSource.Stop();
            audioSource.time = 0f;
            isPlaying = false;
            isPaused = false;
            isRecording = false;

            if (timelineSlider != null)
                timelineSlider.value = 0f;

            ShowStatus("정지");
        }

        IEnumerator LoadAudio(string path)
        {
            if (!File.Exists(path))
            {
                ShowStatus($"파일을 찾을 수 없습니다: {path}");
                yield break;
            }

            string url = "file:///" + path.Replace("\\", "/");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    ShowStatus($"오디오 로드 실패: {www.error}");
                    Debug.LogError($"오디오 로드 실패: {www.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;

                    // UI 업데이트
                    if (timelineSlider != null)
                    {
                        timelineSlider.maxValue = clip.length;
                        timelineSlider.value = 0f;
                    }

                    if (totalTimeText != null)
                        totalTimeText.text = FormatTime(clip.length);

                    if (currentTimeText != null)
                        currentTimeText.text = FormatTime(0f);

                    audioFilePath = path;
                    currentChart.audioFileName = Path.GetFileName(path);

                    // 타임라인 새로고침
                    RequestTimelineRefresh();

                    // 파형 생성
                    GenerateWaveform();

                    ShowStatus($"오디오 로드 완료: {clip.name} ({clip.length:F2}초)");
                    Debug.Log($"오디오 로드 완료: {clip.name} ({clip.length:F2}초)");
                }
            }
        }

        void SeekAudio(float time)
        {
            if (audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.time = time;
                if (currentTimeText != null)
                    currentTimeText.text = FormatTime(time);
            }
        }

        void UpdateTimeline()
        {
            if (audioSource.isPlaying && timelineSlider != null)
            {
                timelineSlider.value = audioSource.time;
                if (currentTimeText != null)
                    currentTimeText.text = FormatTime(audioSource.time);
            }
        }
        #endregion

        #region Chart Management
        void SaveChart()
        {
            // 차트 메타데이터 업데이트
            currentChart.songName = songName;
            currentChart.artistName = artistName;
            currentChart.bpm = bpm;

            try
            {
                string json = JsonUtility.ToJson(currentChart, true);
                string path = Path.Combine(Application.persistentDataPath, "chart.json");
                File.WriteAllText(path, json);

                ShowStatus($"차트 저장 완료: {currentChart.GetNoteCount()}개 노트");
                Debug.Log($"차트 저장: {path} ({currentChart.GetNoteCount()}개 노트)");
            }
            catch (System.Exception e)
            {
                ShowStatus($"저장 실패: {e.Message}");
                Debug.LogError($"차트 저장 실패: {e.Message}");
            }
        }

        public void LoadChart()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "chart.json");

                if (!File.Exists(path))
                {
                    ShowStatus("차트 파일을 찾을 수 없습니다");
                    return;
                }

                string json = File.ReadAllText(path);
                currentChart = JsonUtility.FromJson<ChartDataNew>(json);

                // 에디터 설정 업데이트
                songName = currentChart.songName;
                artistName = currentChart.artistName;
                bpm = currentChart.bpm;

                // UI 업데이트
                if (songNameInput != null) songNameInput.text = songName;
                if (artistNameInput != null) artistNameInput.text = artistName;
                if (bpmInput != null) bpmInput.text = bpm.ToString();

                // 타임라인 새로고침
                RequestTimelineRefresh();

                ShowStatus($"차트 로드 완료: {currentChart.GetNoteCount()}개 노트");
                Debug.Log($"차트 로드: {currentChart.GetNoteCount()}개 노트");
            }
            catch (System.Exception e)
            {
                ShowStatus($"로드 실패: {e.Message}");
                Debug.LogError($"차트 로드 실패: {e.Message}");
                currentChart = new ChartDataNew();
            }
        }

        public void ClearChart()
        {
            if (currentChart.GetNoteCount() > 0)
            {
                SaveStateForUndo();
            }

            currentChart.Clear();
            ClearNoteVisuals();
            ShowStatus("차트 초기화 완료");
        }
        #endregion

        #region Undo/Redo System
        void SaveStateForUndo()
        {
            // 현재 상태를 JSON으로 직렬화하여 저장
            string json = JsonUtility.ToJson(currentChart);
            ChartDataNew chartCopy = JsonUtility.FromJson<ChartDataNew>(json);

            undoStack.Push(chartCopy);

            // 스택 크기 제한
            if (undoStack.Count > MAX_UNDO_STACK)
            {
                // 가장 오래된 항목 제거 (스택이므로 임시 스택 사용)
                Stack<ChartDataNew> temp = new Stack<ChartDataNew>();
                for (int i = 0; i < MAX_UNDO_STACK; i++)
                {
                    temp.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (temp.Count > 0)
                {
                    undoStack.Push(temp.Pop());
                }
            }

            // Redo 스택 클리어 (새로운 액션 후에는 Redo 불가)
            redoStack.Clear();
        }

        void Undo()
        {
            if (undoStack.Count == 0)
            {
                ShowStatus("되돌릴 작업이 없습니다");
                return;
            }

            // 현재 상태를 Redo 스택에 저장
            string currentJson = JsonUtility.ToJson(currentChart);
            ChartDataNew currentCopy = JsonUtility.FromJson<ChartDataNew>(currentJson);
            redoStack.Push(currentCopy);

            // 이전 상태 복원
            currentChart = undoStack.Pop();

            ShowStatus("되돌리기 완료");
        }

        void Redo()
        {
            if (redoStack.Count == 0)
            {
                ShowStatus("다시 실행할 작업이 없습니다");
                return;
            }

            // 현재 상태를 Undo 스택에 저장
            string currentJson = JsonUtility.ToJson(currentChart);
            ChartDataNew currentCopy = JsonUtility.FromJson<ChartDataNew>(currentJson);
            undoStack.Push(currentCopy);

            // Redo 상태 복원
            currentChart = redoStack.Pop();

            ShowStatus("다시 실행 완료");
        }
        #endregion

        #region Utility Methods
        void ToggleEditScope()
        {
            currentEditScope = (currentEditScope == EditScopeType.PerNote)
                ? EditScopeType.PerMeasure
                : EditScopeType.PerNote;

            string scopeName = (currentEditScope == EditScopeType.PerNote)
                ? "노트별"
                : "마디별";

            ShowStatus($"편집 범위: {scopeName}");
        }

        public void SetBPM(float newBPM)
        {
            bpm = Mathf.Max(60f, newBPM);
            currentChart.bpm = bpm;

            // 기존 노트들의 비트 타이밍 재계산
            foreach (var note in currentChart.notes)
            {
                note.CalculateBeatTiming(bpm);
            }

            ShowStatus($"BPM 변경: {bpm}");
        }

        public void SetKeySound(KeySoundType keySoundType)
        {
            selectedKeySoundType = keySoundType;
            ShowStatus($"키사운드 선택: {keySoundType}");
        }

        void UpdateModeDisplay()
        {
            if (modeText != null)
            {
                string modeName = currentNoteMode switch
                {
                    NoteInputMode.Normal => "일반 노트",
                    NoteInputMode.Long => "롱노트",
                    NoteInputMode.Slide => "슬라이드",
                    _ => "알 수 없음"
                };

                modeText.text = $"모드: {modeName}";
            }
        }

        /// <summary>
        /// 마디 오버라이드 추가 버튼 클릭 시 호출
        /// TODO: 별도 UI 패널로 입력 받도록 개선 필요
        /// </summary>
        void OnAddMeasureOverrideClicked()
        {
            // 임시 구현: 하드코딩된 값으로 오버라이드 추가
            // 실제로는 InputField 3개를 가진 팝업 UI가 필요함
            int startMeasure = 8;
            int endMeasure = 21;
            int beatsPerMeasure = 12;

            var newOverride = new MeasureLineOverride(startMeasure, endMeasure, beatsPerMeasure);
            currentChart.measureLineOverrides.Add(newOverride);

            RequestTimelineRefresh();
            ShowStatus($"마디 오버라이드 추가: {startMeasure}~{endMeasure}마디 = {beatsPerMeasure}박자");

            Debug.LogWarning("[ChartEditor] OnAddMeasureOverrideClicked: 임시 구현입니다. UI 패널 필요!");
        }

        void ShowStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[ChartEditor] {message}");
        }

        void ClearNoteVisuals()
        {
            foreach (GameObject visual in spawnedNoteVisuals)
            {
                if (visual != null)
                    Destroy(visual);
            }
            spawnedNoteVisuals.Clear();
        }

        string FormatTime(float time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            int milliseconds = (int)((time - (int)time) * 100);
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
        #endregion

        #region Public API
        public ChartDataNew GetCurrentChart()
        {
            return currentChart;
        }

        public void SetChart(ChartDataNew chart)
        {
            if (chart != null)
            {
                currentChart = chart;
                songName = chart.songName;
                artistName = chart.artistName;
                bpm = chart.bpm;
            }
        }

        public bool IsRecording()
        {
            return isRecording;
        }

        public float GetCurrentTime()
        {
            return audioSource != null ? audioSource.time : 0f;
        }
        #endregion

        #region Timeline Visualization
        /// <summary>
        /// 타임라인의 모든 시각 요소를 새로고침합니다
        /// </summary>
        public void RefreshTimelineVisuals()
        {
            if (timelineContainer == null || audioSource.clip == null)
                return;

            ClearTimelineVisuals();
            GenerateBeatLines();
            GenerateMeasureLines();
            GenerateNoteVisuals();
        }

        /// <summary>
        /// 타임라인의 모든 시각 요소를 제거합니다
        /// </summary>
        void ClearTimelineVisuals()
        {
            foreach (GameObject obj in timelineNoteVisuals)
            {
                if (obj != null) Destroy(obj);
            }
            timelineNoteVisuals.Clear();

            foreach (GameObject obj in timelineBeatLines)
            {
                if (obj != null) Destroy(obj);
            }
            timelineBeatLines.Clear();

            foreach (GameObject obj in timelineMeasureLines)
            {
                if (obj != null) Destroy(obj);
            }
            timelineMeasureLines.Clear();
        }

        /// <summary>
        /// BPM 기반 박자선을 생성합니다 (에디터 전용, 플레이 시 비표시)
        /// subdivision: 1~100분 음표
        /// </summary>
        void GenerateBeatLines()
        {
            if (beatLinePrefab == null || bpm <= 0 || audioSource.clip == null)
                return;

            // subdivision 범위 제한 (1~100)
            int safeSubdivision = Mathf.Clamp(currentSubdivision, 1, 100);

            float songLength = audioSource.clip.length;
            float beatInterval = 60f / bpm; // 1박자 길이 (초)

            // subdivision분 음표 간격 계산
            // 1/4박자(quarter note) = 4분음표
            // currentSubdivision = n이면 n분음표
            float subdivisionInterval = beatInterval * (4f / safeSubdivision);

            for (float time = 0; time < songLength; time += subdivisionInterval)
            {
                GameObject beatLine = Instantiate(beatLinePrefab, timelineContainer);
                RectTransform rt = beatLine.GetComponent<RectTransform>();

                if (rt != null)
                {
                    float xPos = time * pixelsPerSecond;
                    rt.anchoredPosition = new Vector2(xPos, 0);

                    // 1박자마다 더 굵게 표시
                    bool isMainBeat = Mathf.Approximately(time % beatInterval, 0);
                    Image img = beatLine.GetComponent<Image>();
                    if (img != null)
                    {
                        // 박자선은 반투명하게 (에디터에서만 보임)
                        img.color = isMainBeat ? new Color(0.5f, 0.5f, 1f, 0.4f) : new Color(0.5f, 0.5f, 1f, 0.15f);
                    }
                }

                timelineBeatLines.Add(beatLine);
            }
        }

        /// <summary>
        /// 마디선을 생성합니다 (플레이 시 표시, 오버라이드 지원)
        /// </summary>
        void GenerateMeasureLines()
        {
            if (measureLinePrefab == null || bpm <= 0 || audioSource.clip == null || currentChart == null)
                return;

            float songLength = audioSource.clip.length;
            float beatLength = 60f / bpm; // 1박자 길이 (초)

            int defaultBeats = Mathf.Max(1, currentChart.defaultBeatsPerMeasure); // 기본 마디당 박자 수
            float currentTime = 0f;
            int measureNumber = 1;

            while (currentTime < songLength)
            {
                // 현재 마디에 적용할 박자 수 결정
                int beatsForThisMeasure = GetBeatsPerMeasureAtMeasureNumber(measureNumber, defaultBeats);

                // 마디선 생성
                GameObject measureLine = Instantiate(measureLinePrefab, timelineContainer);
                RectTransform rt = measureLine.GetComponent<RectTransform>();

                if (rt != null)
                {
                    float xPos = currentTime * pixelsPerSecond;
                    rt.anchoredPosition = new Vector2(xPos, 0);

                    // 마디 번호 텍스트 추가
                    Text measureText = measureLine.GetComponentInChildren<Text>();
                    if (measureText != null)
                    {
                        measureText.text = $"M{measureNumber}\n({beatsForThisMeasure}박자)";
                    }

                    // 마디선은 굵고 진하게 (플레이 시 표시)
                    Image img = measureLine.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(1f, 1f, 1f, 0.8f); // 흰색, 불투명
                    }
                }

                timelineMeasureLines.Add(measureLine);

                // 다음 마디 시작 시간 계산
                currentTime += beatLength * beatsForThisMeasure;
                measureNumber++;
            }
        }

        /// <summary>
        /// 특정 마디 번호에서 적용할 박자 수를 반환합니다 (오버라이드 고려)
        /// </summary>
        int GetBeatsPerMeasureAtMeasureNumber(int measureNum, int defaultBeats)
        {
            if (currentChart == null || currentChart.measureLineOverrides == null)
                return defaultBeats;

            // 오버라이드 리스트에서 해당하는 범위 찾기
            foreach (var mOverride in currentChart.measureLineOverrides)
            {
                if (measureNum >= mOverride.startMeasure && measureNum <= mOverride.endMeasure)
                {
                    return Mathf.Max(1, mOverride.beatsPerMeasure);
                }
            }

            return defaultBeats;
        }

        /// <summary>
        /// 차트의 모든 노트를 타임라인에 시각화합니다
        /// </summary>
        void GenerateNoteVisuals()
        {
            if (noteVisualPrefab == null || currentChart == null)
                return;

            foreach (NoteData note in currentChart.notes)
            {
                GameObject noteVisual = Instantiate(noteVisualPrefab, timelineContainer);
                RectTransform rt = noteVisual.GetComponent<RectTransform>();

                if (rt != null)
                {
                    float xPos = (float)note.timing * pixelsPerSecond;
                    float yPos = CalculateNoteYPosition(note.track);
                    rt.anchoredPosition = new Vector2(xPos, yPos);

                    // 롱노트인 경우 길이 표시
                    if (note.isLongNote && note.longNoteEndTiming > note.timing)
                    {
                        float duration = (float)(note.longNoteEndTiming - note.timing);
                        rt.sizeDelta = new Vector2(duration * pixelsPerSecond, rt.sizeDelta.y);
                    }

                    // 트랙별 색상 구분
                    Image img = noteVisual.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = GetTrackColor(note.track);
                    }
                }

                timelineNoteVisuals.Add(noteVisual);
            }
        }

        /// <summary>
        /// 트랙 인덱스에 따라 Y 포지션을 계산합니다
        /// </summary>
        float CalculateNoteYPosition(int track)
        {
            float trackHeight = 30f; // 트랙당 높이
            float startY = -trackHeight * keyCount / 2f;
            return startY + (track * trackHeight);
        }

        /// <summary>
        /// 트랙별 색상을 반환합니다
        /// </summary>
        Color GetTrackColor(int track)
        {
            Color[] trackColors = new Color[]
            {
                new Color(1f, 0.3f, 0.3f),   // 빨강
                new Color(0.3f, 0.6f, 1f),   // 파랑
                new Color(0.3f, 1f, 0.3f),   // 초록
                new Color(1f, 1f, 0.3f),     // 노랑
                new Color(1f, 0.3f, 1f),     // 핑크
                new Color(0.3f, 1f, 1f),     // 청록
                new Color(1f, 0.6f, 0.3f),   // 주황
                new Color(0.6f, 0.3f, 1f),   // 보라
                new Color(1f, 1f, 1f),       // 흰색
                new Color(0.8f, 0.8f, 0.8f)  // 회색
            };

            return trackColors[track % trackColors.Length];
        }

        /// <summary>
        /// 재생 헤드의 위치를 업데이트합니다
        /// </summary>
        void UpdatePlayheadPosition()
        {
            if (playheadIndicator == null || audioSource == null)
                return;

            float currentTime = audioSource.time;
            float xPos = currentTime * pixelsPerSecond;
            playheadIndicator.anchoredPosition = new Vector2(xPos, playheadIndicator.anchoredPosition.y);

            // 자동 스크롤 (재생 중일 때)
            if (isPlaying && timelineScrollRect != null && audioSource.clip != null)
            {
                float scrollPercentage = currentTime / audioSource.clip.length;
                timelineScrollRect.horizontalNormalizedPosition = scrollPercentage;
            }
        }

        /// <summary>
        /// 타임라인 새로고침을 요청합니다 (다음 프레임에 실행)
        /// </summary>
        public void RequestTimelineRefresh()
        {
            timelineNeedsRefresh = true;
        }

        /// <summary>
        /// 오디오 클립의 파형을 생성하고 표시합니다
        /// </summary>
        public void GenerateWaveform()
        {
            if (waveformDisplay == null || audioSource.clip == null)
                return;

            AudioClip clip = audioSource.clip;
            int sampleCount = clip.samples * clip.channels;
            float[] samples = new float[sampleCount];
            clip.GetData(samples, 0);

            int width = waveformResolution;
            int height = 200;
            Texture2D texture = new Texture2D(width, height);

            // 배경을 투명하게
            Color[] clearColors = new Color[width * height];
            for (int i = 0; i < clearColors.Length; i++)
                clearColors[i] = new Color(0, 0, 0, 0);
            texture.SetPixels(clearColors);

            // 파형 그리기
            int samplesPerPixel = sampleCount / width;
            for (int x = 0; x < width; x++)
            {
                int sampleIndex = x * samplesPerPixel;
                if (sampleIndex >= sampleCount)
                    break;

                // 구간별 최대/최소값 계산
                float min = 1f;
                float max = -1f;
                for (int i = 0; i < samplesPerPixel && (sampleIndex + i) < sampleCount; i++)
                {
                    float sample = samples[sampleIndex + i];
                    if (sample < min) min = sample;
                    if (sample > max) max = sample;
                }

                // 정규화 (-1~1 → 0~height)
                int yMin = Mathf.Clamp((int)((min + 1f) * height * 0.5f), 0, height - 1);
                int yMax = Mathf.Clamp((int)((max + 1f) * height * 0.5f), 0, height - 1);

                // 세로선 그리기
                for (int y = yMin; y <= yMax; y++)
                {
                    texture.SetPixel(x, y, waveformColor);
                }
            }

            texture.Apply();
            waveformDisplay.texture = texture;

            Debug.Log($"파형 생성 완료: {width}x{height}, 샘플 수: {sampleCount}");
        }
        #endregion

        #region TODO: Advanced Features
        // TODO: 변속 시스템 (BPM Change)
        // - 노트별/마디별 BPM 변경
        // - 변속 구간 시각화

        // TODO: 플레이테스트 기능
        // - 에디터 내에서 차트 테스트 플레이
        // - Left Click으로 시작 노트 선택

        // TODO: 판정 조정 시스템
        // - Normal/Hard/Super 모드별 커스텀 판정 ms
        // - 노트별/마디별 판정 설정

        // TODO: HP 감소율 조정
        // - 노트별/마디별 HP 감소율 설정
        // - HP 밸런싱 시뮬레이션

        // TODO: 노트 밀도 그래프
        // - 1초당 노트 개수 시각화
        // - 난이도 곡선 표시

        // TODO: 고급 키사운드 기능
        // - 키음 드래그 앤 드롭
        // - 노트별 키사운드 볼륨 조절

        // TODO: 비주얼 타임라인
        // - 노트 시각화
        // - 마디선/박자선 표시
        // - 오디오 파형 표시

        // TODO: 노트 편집 고급 기능
        // - 다중 선택 (Shift+클릭, 드래그 박스)
        // - 복사/붙여넣기
        // - 노트 이동 (드래그)
        // - 노트 삭제 (Delete 키)

        // TODO: 차트 유효성 검사
        // - 노트 겹침 검사
        // - 타이밍 오류 검사
        // - 난이도 레벨 자동 계산

        // TODO: Export/Import
        // - 다양한 형식으로 차트 내보내기
        // - 외부 차트 파일 가져오기
        #endregion
    }
}
