# 곡 선택 시스템 사용 가이드

Unity 리듬게임의 곡 선택 화면을 구현하는 스크립트들입니다.

## 📋 파일 구성

1. **SongData.cs** - 개별 곡의 정보를 저장하는 데이터 클래스
2. **SongDatabase.cs** - 모든 곡을 관리하는 ScriptableObject 데이터베이스
3. **SongSelectionUI.cs** - 곡 선택 화면 UI를 관리하는 메인 스크립트
4. **SongListLoader.cs** - 차트 자동 스캔 시스템 (NEW - 2025-10-26)
5. **SampleChartGenerator.cs** - 샘플 차트 생성 유틸리티 (NEW - 2025-10-26)

**연동 시스템**:
- `GameResultManager` (Assets/playresult/) - 곡 정보 및 결과 데이터 관리

## 🚀 빠른 시작

### 방법 1: 자동 스캔 (권장)

1. **SongDatabase 생성**: Create → Rhythm Game → Song Database
2. **SongListLoader 추가**:
   - 빈 GameObject 생성 → `SongListLoader` 컴포넌트 추가
   - `Song Database` 필드에 데이터베이스 연결
   - `Scan On Start` 체크
3. 완료! 씬 시작 시 `StreamingAssets/Charts/` 자동 스캔

### 방법 2: 수동 설정

기존 방식대로 SongDatabase Inspector에서 수동으로 곡 추가

## 🎵 주요 기능 (2025-10-26 업데이트)

### 새로운 기능
- ✅ **자동 차트 스캔**: `StreamingAssets/Charts/` 폴더 자동 스캔
- ✅ **AudioManager 통합**: FMOD 기반 미리듣기
- ✅ **커버 아트 자동 로딩**: `CoverArtLoader` 통합
- ✅ **GameResultManager**: 씬 간 데이터 전달

### 키보드 조작
- **↑/↓**: 이전/다음 곡 선택
- **←/→**: 이전/다음 난이도 선택
- **Left Shift/Right Shift**: 이전/다음 키 개수 선택
- **Enter**: 곡 선택 및 게임 시작
- **Space**: 미리듣기 재생/중지
- **ESC**: 뒤로 가기

## Unity 설정 방법

### 1. SongDatabase 생성

1. Project 창에서 우클릭 → Create → Rhythm Game → Song Database
2. 생성된 SongDatabase asset의 이름을 "MainSongDatabase"로 변경
3. Inspector에서 곡 정보 추가:

```
Songs (목록):
  - Element 0:
    - Song Id: "song_001"
    - Title: "첫 번째 곡"
    - Artist: "아티스트 이름"
    - Audio Path: "Audio/song001"
    - BPM: 120
    - Song Length: 180

    Difficulties:
      - Element 0:
        - Difficulty Name: "Easy"
        - Level: 1
        - Total Notes: 150
        - Difficulty Color: 초록색
        - Chart Paths:
          - Element 0:
            - Key Count: 4
            - Chart Path: "Charts/song001_easy_4k"

      - Element 1:
        - Difficulty Name: "Hard"
        - Level: 5
        - Total Notes: 350
        - Difficulty Color: 빨간색
        - Chart Paths:
          - Element 0:
            - Key Count: 4
            - Chart Path: "Charts/song001_hard_4k"

    Supported Key Counts: 4, 6
```

### 2. 곡 선택 씬 설정

```
Hierarchy:
  - Canvas
    - SongInfoPanel
      - AlbumArt (Image)
      - SongTitle (TextMeshProUGUI)
      - ArtistName (TextMeshProUGUI)
      - BPM (TextMeshProUGUI)
      - SongLength (TextMeshProUGUI)
      - Genre (TextMeshProUGUI)
      - Description (TextMeshProUGUI)

    - DifficultyPanel
      - DifficultyText (TextMeshProUGUI)
      - DifficultyLevel (TextMeshProUGUI)
      - TotalNotes (TextMeshProUGUI)
      - DifficultyIndicator (Image) - 색상 표시용

    - KeyModePanel
      - KeyCountText (TextMeshProUGUI)

    - NavigationPanel
      - PreviousSongButton (Button) - ↑
      - NextSongButton (Button) - ↓
      - PreviousDifficultyButton (Button) - ←
      - NextDifficultyButton (Button) - →
      - PreviousKeyCountButton (Button)
      - NextKeyCountButton (Button)

    - ActionPanel
      - SelectSongButton (Button) - "Start"
      - PreviewButton (Button) - "Preview"
      - BackButton (Button) - "Back"

    - SongIndexText (TextMeshProUGUI) - "1 / 10"

    - LockedIndicator (GameObject) - 잠금 표시
      - LockedMessage (TextMeshProUGUI)

    - Background (Image) - 배경 이미지

  - AudioSource (미리듣기용)

  - SongSelectionManager (GameObject)
    - SongSelectionUI (Component)
```

### 3. SongSelectionUI Inspector 설정

**곡 데이터베이스:**
- Song Database: MainSongDatabase asset 연결

**곡 정보 UI:**
- Song Title Text: SongTitle 연결
- Artist Text: ArtistName 연결
- BPM Text: BPM 연결
- Song Length Text: SongLength 연결
- Genre Text: Genre 연결
- Description Text: Description 연결
- Album Art Image: AlbumArt 연결
- Background Image: Background 연결

**난이도 UI:**
- Difficulty Text: DifficultyText 연결
- Difficulty Level Text: DifficultyLevel 연결
- Total Notes Text: TotalNotes 연결
- Difficulty Indicator Image: DifficultyIndicator 연결

**키 모드 UI:**
- Key Count Text: KeyCountText 연결

**곡 목록 UI:**
- Song Index Text: SongIndexText 연결
- Previous Song Button: PreviousSongButton 연결
- Next Song Button: NextSongButton 연결

**난이도/키 모드 변경 버튼:**
- Previous Difficulty Button: PreviousDifficultyButton 연결
- Next Difficulty Button: NextDifficultyButton 연결
- Previous Key Count Button: PreviousKeyCountButton 연결
- Next Key Count Button: NextKeyCountButton 연결

**메인 버튼:**
- Select Song Button: SelectSongButton 연결
- Back Button: BackButton 연결
- Preview Button: PreviewButton 연결

**잠금 UI:**
- Locked Indicator: LockedIndicator 연결
- Locked Message Text: LockedMessage 연결

**씬 설정:**
- Game Scene Name: "GameScene"
- Main Menu Scene Name: "MainMenuScene"

**오디오 설정:**
- Preview Audio Source: AudioSource 연결

## 키보드 조작

- **↑/↓**: 이전/다음 곡 선택
- **←/→**: 이전/다음 난이도 선택
- **Left Shift/Right Shift**: 이전/다음 키 개수 선택
- **Enter**: 곡 선택 및 게임 시작
- **Space**: 미리듣기
- **ESC**: 뒤로 가기

## 코드 사용 예제

### 곡 선택 정보 가져오기 (게임 씬에서)

```csharp
void Start()
{
    // GameResultManager에서 가져오기 (권장)
    if (GameResultManager.Instance != null)
    {
        string songTitle, artist, difficulty;
        int keyCount;
        GameResultManager.Instance.GetCurrentSongInfo(
            out songTitle, out artist, out difficulty, out keyCount
        );

        Debug.Log($"선택된 곡: {songTitle} - {artist} [{difficulty}] {keyCount}K");
    }

    // PlayerPrefs에서 가져오기 (하위 호환)
    string songId = PlayerPrefs.GetString("SelectedSongId");
    string chartPath = PlayerPrefs.GetString("SelectedChartPath");
    int selectedKeyCount = PlayerPrefs.GetInt("SelectedKeyCount");
}
```

### 프로그래밍 방식으로 곡 로드

```csharp
public SongSelectionUI songSelectionUI;

void LoadSpecificSong()
{
    // 인덱스로 곡 로드
    songSelectionUI.LoadSong(2); // 3번째 곡 로드
}
```

### 곡 데이터베이스 검증

Unity Editor에서 다음 코드를 실행하여 데이터베이스를 검증할 수 있습니다:

```csharp
[MenuItem("Tools/Validate Song Database")]
static void ValidateDatabase()
{
    SongDatabase db = AssetDatabase.LoadAssetAtPath<SongDatabase>("Assets/Data/MainSongDatabase.asset");
    if (db != null)
    {
        db.ValidateDatabase();
    }
}
```

## 곡 데이터 구조

### SongData
- **songId**: 곡 고유 ID (중복 불가)
- **title**: 곡 제목
- **artist**: 아티스트 이름
- **audioPath**: 음악 파일 경로
- **albumArt**: 앨범 아트 이미지
- **bpm**: BPM
- **difficulties**: 난이도 목록
- **supportedKeyCounts**: 지원하는 키 개수

### DifficultyInfo
- **difficultyName**: 난이도 이름 (Easy, Normal, Hard 등)
- **level**: 난이도 레벨 (숫자)
- **totalNotes**: 총 노트 수
- **difficultyColor**: 난이도 표시 색상
- **chartPaths**: 키 개수별 차트 파일 경로

## 확장 방법

### 곡 검색 기능 추가

```csharp
public TMP_InputField searchInputField;

void OnSearchTextChanged(string searchText)
{
    var results = songDatabase.SearchByTitle(searchText);
    // 검색 결과를 UI에 표시
}
```

### 곡 정렬 기능 추가

```csharp
public void SortByTitle()
{
    songDatabase.SortSongs(SongSortType.Title);
    LoadSong(0); // 첫 번째 곡으로 이동
}

public void SortByBPM()
{
    songDatabase.SortSongs(SongSortType.BPM);
    LoadSong(0);
}
```

### 즐겨찾기 기능 추가

SongData에 필드 추가:
```csharp
public bool isFavorite = false;
```

SongDatabase에 메서드 추가:
```csharp
public List<SongData> GetFavoriteSongs()
{
    return songs.Where(s => s.isFavorite).ToList();
}
```

### 곡 잠금 해제 시스템

```csharp
void UnlockSong(string songId)
{
    SongData song = songDatabase.GetSongById(songId);
    if (song != null)
    {
        song.isLocked = false;
        PlayerPrefs.SetInt($"Song_{songId}_Unlocked", 1);
        PlayerPrefs.Save();
    }
}

void LoadUnlockStatus()
{
    foreach (var song in songDatabase.songs)
    {
        bool isUnlocked = PlayerPrefs.GetInt($"Song_{song.songId}_Unlocked", 0) == 1;
        song.isLocked = !isUnlocked;
    }
}
```

## 주의사항

1. **SongDatabase는 ScriptableObject**입니다. Unity Editor에서 생성하고 Inspector에서 편집해야 합니다.
2. **곡 ID는 고유**해야 합니다. 중복 시 ValidateDatabase()를 호출하면 경고가 표시됩니다.
3. **차트 경로**는 실제 파일 경로와 일치해야 합니다.
4. **앨범 아트와 배경 이미지**는 Sprite로 설정해야 합니다.
5. 버튼 이벤트는 스크립트에서 자동으로 등록되므로 Inspector에서 OnClick 설정이 불필요합니다.

## 미리듣기 기능 (AudioManager 통합)

미리듣기 기능이 이제 AudioManager와 완전히 통합되었습니다!

```csharp
// SongSelectionUI.cs - 자동 구현됨
private void PlayPreview()
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.LoadBGM(currentSong.audioPath);
        AudioManager.Instance.PlayBGM();
        // 미리듣기 시작 시간으로 이동
        StartCoroutine(SeekToPreviewTime(currentSong.previewStartTime));
    }
}

private void StopPreview()
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.StopBGM();
    }
}
```

**필요 조건**:
- AudioManager 인스턴스가 씬에 존재
- 오디오 파일이 `StreamingAssets/Audio/BGM/` 폴더에 존재
- FMOD가 올바르게 초기화됨

## 커버 아트 자동 로딩 (NEW)

`CoverArtLoader`를 통해 커버 이미지를 자동으로 로드합니다:

```csharp
// SongSelectionUI.cs - 자동 구현됨
private IEnumerator LoadCoverArtAsync(string audioFileName)
{
    var loadCoroutine = CoverArtLoader.LoadCoverArtAsync(audioFileName, (sprite) =>
    {
        if (sprite != null && albumArtImage != null)
        {
            albumArtImage.sprite = sprite;
        }
    });
    yield return StartCoroutine(loadCoroutine);
}
```

**커버 이미지 설정**:
1. `StreamingAssets/CoverArt/` 폴더에 이미지 배치
2. 파일명을 오디오 파일과 동일하게 설정:
   ```
   sample_audio.wav → sample_audio.png (또는 .jpg)
   ```

## 디버그 팁

Inspector에서 SongDatabase를 선택하고 다음을 확인하세요:
- 곡 개수가 0이 아닌지
- 각 곡의 ID가 설정되어 있는지
- 각 곡에 최소 1개 이상의 난이도가 있는지
- 각 곡에 지원하는 키 개수가 설정되어 있는지

문제가 있다면 Console 창에서 ValidateDatabase() 결과를 확인하세요.
