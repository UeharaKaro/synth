# 고급 곡 선택 UI 설정 가이드 (SongSelectionUIAdvanced)

> **작성일**: 2025-10-26  
> **버전**: 1.0  
> **대상**: Unity 6.0 이상  

---

## 📋 목차

1. [개요](#개요)
2. [기본 vs 고급 버전 비교](#기본-vs-고급-버전-비교)
3. [UI 계층 구조](#ui-계층-구조)
4. [단계별 설정](#단계별-설정)
5. [기능 상세 설명](#기능-상세-설명)
6. [커스터마이징](#커스터마이징)
7. [트러블슈팅](#트러블슈팅)

---

## 개요

**SongSelectionUIAdvanced**는 기본 곡 선택 UI를 대폭 확장한 고급 버전입니다.

### 주요 기능

✅ **스크롤 뷰 곡 목록** - 모든 곡을 한눈에 볼 수 있는 스크롤 가능한 리스트  
✅ **정렬 시스템** - 제목, 아티스트, BPM, 레벨, 플레이 횟수, 최고 점수 등으로 정렬  
✅ **필터링 시스템** - 난이도, 키 모드, 레벨 범위로 필터링  
✅ **검색 기능** - 곡 제목/아티스트/장르 검색  
✅ **즐겨찾기** - 좋아하는 곡을 즐겨찾기에 추가  
✅ **최고 점수 표시** - 난이도별 최고 점수, 등급, 플레이 횟수 표시  
✅ **클리어 상태** - 클리어한 곡 표시 및 필터링  
✅ **선택 애니메이션** - 부드러운 스크롤 및 선택 효과  

---

## 기본 vs 고급 버전 비교

| 기능 | 기본 (SongSelectionUI) | 고급 (SongSelectionUIAdvanced) |
|------|----------------------|-------------------------------|
| 곡 탐색 방식 | 순환 방식 (한 곡씩) | 스크롤 뷰 (전체 목록) |
| 정렬 | ❌ 없음 | ✅ 7가지 옵션 |
| 필터링 | ❌ 없음 | ✅ 난이도/키모드/레벨 |
| 검색 | ❌ 없음 | ✅ 실시간 검색 |
| 즐겨찾기 | ❌ 없음 | ✅ 지원 |
| 최고 점수 | ❌ 없음 | ✅ 표시 |
| 클리어 상태 | ❌ 없음 | ✅ 뱃지 표시 |
| NEW 뱃지 | ❌ 없음 | ✅ 최근 7일 표시 |
| 썸네일 | ❌ 없음 | ✅ 목록에 표시 |

**권장 사용**:
- **기본 버전**: 프로토타입, 10곡 이하 프로젝트
- **고급 버전**: 정식 출시, 20곡 이상 프로젝트

---

## UI 계층 구조

### 전체 구조

```
SongSelectionCanvas (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── TitleText
│   └── BackButton
├── MainContainer (Horizontal Layout)
│   ├── LeftPanel (곡 목록)
│   │   ├── FilterPanel
│   │   │   ├── SearchBar
│   │   │   │   ├── SearchInputField
│   │   │   │   ├── SearchButton
│   │   │   │   └── ClearSearchButton
│   │   │   ├── SortDropdown
│   │   │   ├── SortOrderToggle
│   │   │   ├── DifficultyFilterDropdown
│   │   │   ├── KeyModeFilterDropdown
│   │   │   ├── LevelRangeSliders
│   │   │   │   ├── MinLevelSlider
│   │   │   │   ├── MaxLevelSlider
│   │   │   │   ├── MinLevelText
│   │   │   │   └── MaxLevelText
│   │   │   ├── FavoritesOnlyToggle
│   │   │   └── ClearedOnlyToggle
│   │   └── SongListScrollView (Scroll View)
│   │       ├── Viewport
│   │       └── Content (Vertical Layout Group)
│   │           ├── SongListItem (프리팹)
│   │           ├── SongListItem (프리팹)
│   │           └── ... (동적 생성)
│   └── RightPanel (곡 상세 정보)
│       ├── AlbumArtPanel
│       │   ├── AlbumArtImage
│       │   └── FavoriteToggleButton
│       ├── SongInfoPanel
│       │   ├── SongTitleText
│       │   ├── ArtistText
│       │   ├── BPMText
│       │   ├── SongLengthText
│       │   ├── GenreText
│       │   └── DescriptionText
│       ├── DifficultyPanel
│       │   ├── DifficultyText
│       │   ├── DifficultyLevelText
│       │   ├── TotalNotesText
│       │   ├── KeyCountText
│       │   ├── DifficultyIndicatorImage
│       │   ├── PreviousDifficultyButton
│       │   └── NextDifficultyButton
│       ├── KeyModePanel
│       │   ├── PreviousKeyCountButton
│       │   └── NextKeyCountButton
│       ├── HighScorePanel
│       │   ├── HighScoreText
│       │   ├── HighRankText
│       │   ├── PlayCountText
│       │   └── ClearStatusText
│       └── ActionPanel
│           ├── SelectSongButton
│           ├── PreviewButton
│           └── BackButton
└── LockedPanel (곡 잠금 표시)
    ├── LockedIndicator
    └── LockedMessageText
```

### SongListItem 프리팹 구조

```
SongListItem (Prefab)
├── Background (Image)
├── SelectionIndicator (Image)
├── ThumbnailImage (Image)
├── InfoContainer (Vertical Layout)
│   ├── TitleText (TMP)
│   ├── ArtistText (TMP)
│   └── BPMText (TMP)
├── LevelRangeText (TMP)
├── IconsContainer (Horizontal Layout)
│   ├── FavoriteIcon (Image)
│   ├── LockIcon (Image)
│   ├── ClearedBadge (Image)
│   └── NewBadge (Image)
└── Button (Component)
```

---

## 단계별 설정

### Phase 1: 기본 Canvas 생성 (10분)

1. **Canvas 생성**
   ```
   Hierarchy 우클릭 → UI → Canvas
   이름: "SongSelectionCanvas"
   ```

2. **Canvas 설정**
   - Canvas Scaler 컴포넌트:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Match: `0.5` (Width/Height 중간)

3. **배경 이미지 추가**
   ```
   SongSelectionCanvas 우클릭 → UI → Image
   이름: "Background"
   ```
   - Inspector:
     - Color: 검은색 또는 원하는 배경색
     - Raycast Target: 체크 해제

---

### Phase 2: 메인 레이아웃 구성 (15분)

#### 1. Header Panel (상단)
```
SongSelectionCanvas 우클릭 → UI → Panel
이름: "HeaderPanel"
```
- RectTransform:
  - Anchor: `Top Stretch`
  - Height: `100`
  - Pos Y: `0`

**TitleText 추가:**
```
HeaderPanel 우클릭 → UI → Text - TextMeshPro
이름: "TitleText"
```
- Text: "곡 선택"
- Font Size: `48`
- Alignment: 중앙 정렬

**BackButton 추가:**
```
HeaderPanel 우클릭 → UI → Button - TextMeshPro
이름: "BackButton"
```
- Text: "← 뒤로"
- 위치: 왼쪽 상단

#### 2. Main Container (좌우 분할)
```
SongSelectionCanvas 우클릭 → UI → Panel
이름: "MainContainer"
```
- RectTransform:
  - Anchor: `Stretch Stretch`
  - Top: `-100` (Header 높이)
  - Bottom: `0`
- Horizontal Layout Group 추가:
  - Spacing: `20`
  - Child Force Expand: Width ✅, Height ✅

---

### Phase 3: 왼쪽 패널 (곡 목록) 구성 (30분)

#### 1. Left Panel 생성
```
MainContainer 우클릭 → UI → Panel
이름: "LeftPanel"
```
- Layout Element 추가:
  - Preferred Width: `800`

#### 2. Filter Panel 생성
```
LeftPanel 우클릭 → UI → Panel
이름: "FilterPanel"
```
- RectTransform:
  - Anchor: `Top Stretch`
  - Height: `250`
- Vertical Layout Group 추가:
  - Spacing: `10`
  - Padding: 좌/우/상/하 `10`

**검색 바:**
```
FilterPanel 우클릭 → UI → Input Field - TextMeshPro
이름: "SearchInputField"
```
- Placeholder Text: "곡 제목, 아티스트, 장르 검색..."
- Height: `40`

**검색 버튼 컨테이너:**
```
FilterPanel 우클릭 → UI → Panel
이름: "SearchButtonsContainer"
```
- Horizontal Layout Group:
  - Spacing: `10`

```
SearchButtonsContainer 우클릭 → UI → Button (x2)
이름: "SearchButton", "ClearSearchButton"
```

**정렬 드롭다운:**
```
FilterPanel 우클릭 → UI → Dropdown - TextMeshPro
이름: "SortDropdown"
```
- Options: "제목", "아티스트", "BPM", "레벨", "플레이 횟수", "최고 점수", "추가 날짜"

**정렬 순서 토글:**
```
FilterPanel 우클릭 → UI → Toggle
이름: "SortOrderToggle"
```
- Label: "오름차순/내림차순"

**필터 드롭다운들:**
```
FilterPanel 우클릭 → UI → Dropdown - TextMeshPro (x2)
이름: "DifficultyFilterDropdown", "KeyModeFilterDropdown"
```

**레벨 슬라이더:**
```
FilterPanel 우클릭 → UI → Slider (x2)
이름: "MinLevelSlider", "MaxLevelSlider"
```
- Min Value: `1`
- Max Value: `20`
- Whole Numbers: ✅

**필터 토글들:**
```
FilterPanel 우클릭 → UI → Toggle (x2)
이름: "FavoritesOnlyToggle", "ClearedOnlyToggle"
```
- Label: "즐겨찾기만", "클리어한 곡만"

#### 3. 곡 목록 스크롤 뷰
```
LeftPanel 우클릭 → UI → Scroll View
이름: "SongListScrollView"
```
- RectTransform:
  - Anchor: `Stretch Stretch`
  - Top: `-250` (Filter Panel 높이)
  - Bottom: `0`
- Scroll Rect 설정:
  - Vertical: ✅
  - Horizontal: ❌
  - Movement Type: `Clamped`
  - Scroll Sensitivity: `30`

**Content 설정:**
- Content 선택
- Vertical Layout Group 추가:
  - Spacing: `5`
  - Child Force Expand: Width ✅, Height ❌
  - Child Control Size: Width ✅, Height ✅
- Content Size Fitter 추가:
  - Vertical Fit: `Preferred Size`

---

### Phase 4: SongListItem 프리팹 생성 (20분)

1. **빈 GameObject 생성**
   ```
   Hierarchy 우클릭 → Create Empty
   이름: "SongListItem"
   ```

2. **RectTransform 추가 및 설정**
   - Height: `100`
   - Layout Element 추가:
     - Min Height: `100`
     - Preferred Height: `100`

3. **Background Image**
   ```
   SongListItem 우클릭 → UI → Image
   이름: "Background"
   ```
   - Anchor: `Stretch Stretch`
   - Color: 반투명 흰색 `(1, 1, 1, 0.1)`

4. **Selection Indicator**
   ```
   SongListItem 우클릭 → UI → Image
   이름: "SelectionIndicator"
   ```
   - Anchor: `Left Stretch`
   - Width: `5`
   - Color: 노란색
   - Enabled: ❌ (스크립트에서 제어)

5. **Thumbnail Image**
   ```
   SongListItem 우클릭 → UI → Image
   이름: "ThumbnailImage"
   ```
   - 위치: 왼쪽
   - Size: `80 x 80`
   - Preserve Aspect: ✅

6. **Info Container**
   ```
   SongListItem 우클릭 → UI → Panel
   이름: "InfoContainer"
   ```
   - Vertical Layout Group:
     - Spacing: `5`
     - Child Alignment: `Upper Left`
   - 3개의 TextMeshPro 추가:
     - TitleText (Font Size: 20, Bold)
     - ArtistText (Font Size: 16)
     - BPMText (Font Size: 14, Gray)

7. **Level Range Text**
   ```
   SongListItem 우클릭 → UI → Text - TextMeshPro
   이름: "LevelRangeText"
   ```
   - 위치: 오른쪽 상단
   - Text: "Lv. 1~10"

8. **Icons Container**
   ```
   SongListItem 우클릭 → UI → Panel
   이름: "IconsContainer"
   ```
   - Horizontal Layout Group:
     - Spacing: `5`
   - 4개의 Image 추가:
     - FavoriteIcon (별 아이콘)
     - LockIcon (자물쇠 아이콘)
     - ClearedBadge ("CLEAR" 텍스트)
     - NewBadge ("NEW" 텍스트)

9. **Button 컴포넌트 추가**
   - SongListItem에 Button 컴포넌트 추가
   - Navigation: `None`

10. **SongListItem 스크립트 추가**
    - SongListItem에 `SongListItem.cs` 스크립트 추가
    - Inspector에서 모든 UI 요소 연결

11. **프리팹으로 저장**
    ```
    SongListItem을 Assets/songselect/ 폴더로 드래그
    → Prefab 생성
    Hierarchy에서 원본 삭제
    ```

---

### Phase 5: 오른쪽 패널 (곡 상세 정보) 구성 (20분)

#### 1. Right Panel 생성
```
MainContainer 우클릭 → UI → Panel
이름: "RightPanel"
```
- Layout Element 추가:
  - Flexible Width: `1` (나머지 공간 차지)

#### 2. Album Art Panel
```
RightPanel 우클릭 → UI → Panel
이름: "AlbumArtPanel"
```
- 위치: 상단
- Size: `400 x 400`

**AlbumArtImage:**
```
AlbumArtPanel 우클릭 → UI → Image
이름: "AlbumArtImage"
```
- Anchor: `Center`
- Size: `380 x 380`
- Preserve Aspect: ✅

**FavoriteToggleButton:**
```
AlbumArtPanel 우클릭 → UI → Button
이름: "FavoriteToggleButton"
```
- 위치: 오른쪽 상단
- Text: "★"

#### 3. Song Info Panel
```
RightPanel 우클릭 → UI → Panel
이름: "SongInfoPanel"
```
- Vertical Layout Group
- 6개의 TextMeshPro 추가:
  - SongTitleText (크기: 32)
  - ArtistText (크기: 24)
  - BPMText (크기: 18)
  - SongLengthText (크기: 18)
  - GenreText (크기: 16)
  - DescriptionText (크기: 14)

#### 4. Difficulty Panel
```
RightPanel 우클릭 → UI → Panel
이름: "DifficultyPanel"
```
- Horizontal Layout Group
- 요소 추가:
  - PreviousDifficultyButton ("◀")
  - DifficultyText (현재 난이도)
  - NextDifficultyButton ("▶")
  - DifficultyLevelText ("Lv. X")
  - TotalNotesText ("XXX Notes")
  - DifficultyIndicatorImage (색상 표시)

#### 5. Key Mode Panel
```
RightPanel 우클릭 → UI → Panel
이름: "KeyModePanel"
```
- Horizontal Layout Group
- 요소 추가:
  - PreviousKeyCountButton ("◀")
  - KeyCountText ("XK")
  - NextKeyCountButton ("▶")

#### 6. High Score Panel
```
RightPanel 우클릭 → UI → Panel
이름: "HighScorePanel"
```
- Vertical Layout Group
- 4개의 TextMeshPro 추가:
  - HighScoreText ("최고 점수: XXXXXXX")
  - HighRankText ("등급: SS")
  - PlayCountText ("플레이: XX회")
  - ClearStatusText ("CLEARED" / "NOT CLEARED")

#### 7. Action Panel
```
RightPanel 우클릭 → UI → Panel
이름: "ActionPanel"
```
- Horizontal Layout Group
- 3개의 Button 추가:
  - SelectSongButton ("게임 시작")
  - PreviewButton ("미리듣기")
  - BackButton ("뒤로 가기")

---

### Phase 6: 스크립트 연결 (15분)

1. **SongSelectionCanvas에 SongSelectionUIAdvanced 추가**
   ```
   SongSelectionCanvas 선택
   → Add Component
   → SongSelectionUIAdvanced
   ```

2. **Inspector에서 모든 필드 연결**

   **데이터베이스:**
   - Song Database: SongDatabase 에셋 연결

   **스크롤 뷰:**
   - Song List Scroll View: SongListScrollView
   - Song List Content: SongListScrollView/Viewport/Content
   - Song List Item Prefab: SongListItem 프리팹

   **곡 정보 UI:**
   - Song Title Text
   - Artist Text
   - BPM Text
   - Song Length Text
   - Genre Text
   - Description Text
   - Album Art Image
   - Background Image

   **난이도 UI:**
   - Difficulty Text
   - Difficulty Level Text
   - Total Notes Text
   - Difficulty Indicator Image
   - Key Count Text

   **최고 점수 UI:**
   - High Score Text
   - High Rank Text
   - Play Count Text
   - Clear Status Text

   **검색 UI:**
   - Search Input Field
   - Search Button
   - Clear Search Button

   **정렬 UI:**
   - Sort Dropdown
   - Sort Order Toggle
   - Sort Order Text

   **필터 UI:**
   - Difficulty Filter Dropdown
   - Key Mode Filter Dropdown
   - Min Level Slider
   - Max Level Slider
   - Min Level Text
   - Max Level Text
   - Favorites Only Toggle
   - Cleared Only Toggle

   **버튼:**
   - Previous Difficulty Button
   - Next Difficulty Button
   - Previous Key Count Button
   - Next Key Count Button
   - Select Song Button
   - Back Button
   - Preview Button
   - Favorite Toggle Button

   **잠금 UI:**
   - Locked Indicator
   - Locked Message Text

   **씬 설정:**
   - Game Scene Name: "GameScene"
   - Main Menu Scene Name: "MainMenuScene"

   **오디오:**
   - Preview Audio Source: (선택사항)

   **애니메이션 설정:**
   - Enable Selection Animation: ✅
   - Selection Scale: `1.1`
   - Animation Duration: `0.2`
   - Scroll Speed: `5`

---

## 기능 상세 설명

### 1. 스크롤 뷰 곡 목록

**작동 방식:**
- 모든 곡이 SongListItem 프리팹으로 동적 생성
- 스크롤로 전체 목록 탐색
- 선택된 곡은 하이라이트 표시
- 키보드 (↑↓)로 곡 선택 시 자동 스크롤

**성능 최적화:**
- 썸네일 비동기 로딩
- CoverArtLoader 자동 캐싱
- 스크롤 애니메이션 부드러움

### 2. 정렬 시스템

**정렬 옵션:**
1. **제목** (Title): 곡 제목 알파벳순
2. **아티스트** (Artist): 아티스트명 알파벳순
3. **BPM**: 템포 순서
4. **레벨** (Level): 난이도 레벨 순서
5. **플레이 횟수** (Play Count): 플레이 많은 순
6. **최고 점수** (High Score): 점수 높은 순
7. **추가 날짜** (Date Added): 최근 추가 순

**정렬 순서:**
- 오름차순 ▲ / 내림차순 ▼ 토글

### 3. 필터링 시스템

**필터 종류:**
- **난이도**: Easy, Normal, Hard, Expert, Master, Special
- **키 모드**: 4K, 5K, 6K, 7K, 8K, 10K
- **레벨 범위**: 슬라이더로 1~20 사이 조절
- **즐겨찾기만**: 즐겨찾기 곡만 표시
- **클리어한 곡만**: 클리어한 곡만 표시

**복합 필터:**
- 여러 필터 동시 적용 가능
- 필터 조건에 맞는 곡만 표시

### 4. 검색 기능

**검색 대상:**
- 곡 제목
- 아티스트명
- 장르

**검색 방식:**
- 부분 일치 검색 (contains)
- 대소문자 구분 없음
- 실시간 검색 (검색 버튼 클릭 시 적용)

### 5. 즐겨찾기

**추가/제거:**
- 앨범 커버 옆 별 버튼 클릭
- 키보드 `F` 키

**저장:**
- PlayerPrefs에 자동 저장
- 게임 종료 후에도 유지

**표시:**
- 곡 목록에 별 아이콘 표시
- 즐겨찾기 필터로 모아보기

### 6. 최고 점수 표시

**표시 정보:**
- 최고 점수 (High Score)
- 최고 등급 (Rank: SSS~F)
- 플레이 횟수 (Play Count)
- 클리어 여부 (CLEARED / NOT CLEARED)

**데이터 저장:**
- PlayerPrefs 사용
- 난이도별, 키 모드별 개별 저장

### 7. 곡 아이템 뱃지

**뱃지 종류:**
- ⭐ **즐겨찾기**: 즐겨찾기 추가된 곡
- 🔒 **잠금**: 잠금 해제 필요한 곡
- ✅ **클리어**: 한 번 이상 클리어한 곡
- 🆕 **NEW**: 최근 7일 이내 추가된 곡

---

## 커스터마이징

### 색상 테마 변경

**SongSelectionUIAdvanced.cs 수정:**
```csharp
[Header("색상 설정")]
public Color primaryColor = new Color(1f, 0.8f, 0f); // 메인 색상
public Color secondaryColor = new Color(0.2f, 0.2f, 0.2f); // 보조 색상
public Color accentColor = new Color(0f, 0.8f, 1f); // 강조 색상
```

### 정렬 옵션 추가

**SortOption enum에 추가:**
```csharp
public enum SortOption
{
    Title,
    Artist,
    BPM,
    Level,
    PlayCount,
    HighScore,
    DateAdded,
    MyCustomSort // 새로운 정렬
}
```

**SortSongs() 메서드에 케이스 추가:**
```csharp
case SortOption.MyCustomSort:
    sorted = sortAscending ? songs.OrderBy(s => /* 정렬 기준 */) : songs.OrderByDescending(s => /* 정렬 기준 */);
    break;
```

### 필터 옵션 추가

**InitializeUI() 메서드에 드롭다운 옵션 추가:**
```csharp
List<string> myFilterOptions = new List<string> { "전체", "옵션1", "옵션2" };
myFilterDropdown.AddOptions(myFilterOptions);
```

### NEW 뱃지 기간 변경

**SongListItem.cs의 IsNewSong() 메서드 수정:**
```csharp
return diff.TotalDays <= 14; // 7일 → 14일
```

---

## 트러블슈팅

### Q: 곡 목록이 표시되지 않습니다

**A: 체크리스트**
1. ✅ SongDatabase가 할당되어 있는가?
2. ✅ SongDatabase에 곡이 추가되어 있는가?
3. ✅ SongListItem 프리팹이 할당되어 있는가?
4. ✅ songListContent (Content) RectTransform이 할당되어 있는가?
5. ✅ Console 창에 에러가 있는가?

**디버그 방법:**
```csharp
// SongSelectionUIAdvanced.cs의 Start() 메서드에 추가
Debug.Log($"총 곡 수: {allSongs.Count}");
Debug.Log($"필터된 곡 수: {filteredSongs.Count}");
Debug.Log($"생성된 아이템 수: {songListItems.Count}");
```

### Q: 썸네일 이미지가 로드되지 않습니다

**A: 확인 사항**
1. ✅ CoverArtLoader.Instance가 씬에 존재하는가?
2. ✅ StreamingAssets/CoverArt/ 폴더에 이미지가 있는가?
3. ✅ 이미지 파일명이 오디오 파일명과 일치하는가?
4. ✅ 이미지 형식이 PNG 또는 JPG인가?

### Q: 정렬이 작동하지 않습니다

**A: 확인 사항**
1. ✅ SortDropdown이 할당되어 있는가?
2. ✅ sortDropdown.onValueChanged 리스너가 등록되어 있는가?
3. ✅ ApplyFiltersAndSort() 메서드가 호출되는가?

**디버그:**
```csharp
Debug.Log($"현재 정렬 옵션: {currentSortOption}, 오름차순: {sortAscending}");
```

### Q: 필터가 작동하지 않습니다

**A: 확인 사항**
1. ✅ 필터 드롭다운/슬라이더/토글이 모두 할당되어 있는가?
2. ✅ 리스너가 등록되어 있는가? (InitializeUI에서)
3. ✅ PassesFilters() 메서드 로직이 올바른가?

**디버그:**
```csharp
Debug.Log($"필터: 난이도={currentDifficultyFilter}, 키모드={currentKeyModeFilter}, 레벨={minLevelFilter}~{maxLevelFilter}");
```

### Q: 검색이 작동하지 않습니다

**A: 확인 사항**
1. ✅ SearchInputField가 할당되어 있는가?
2. ✅ onValueChanged 리스너가 등록되어 있는가?
3. ✅ 검색 버튼 클릭 시 ApplyFiltersAndSort() 호출되는가?

### Q: 즐겨찾기가 저장되지 않습니다

**A: 확인 사항**
1. ✅ SaveFavorites() 메서드가 호출되는가?
2. ✅ PlayerPrefs.Save()가 호출되는가?
3. ✅ 앱 종료 시 OnApplicationQuit()에서 저장하는가?

**강제 저장:**
```csharp
void OnApplicationQuit()
{
    SaveFavorites();
}
```

### Q: 최고 점수가 표시되지 않습니다

**A: 원인**
- 아직 플레이 기록이 없음
- GameResultManager가 점수를 저장하지 않음

**해결:**
1. 한 번 플레이하여 점수 기록
2. GameResultManager.SaveResult() 메서드 확인

### Q: 키보드 네비게이션이 작동하지 않습니다

**A: 확인 사항**
1. ✅ enableKeyboardNavigation이 true인가?
2. ✅ Update() 메서드에서 HandleKeyboardInput() 호출되는가?
3. ✅ Input System이 올바르게 설정되어 있는가?

### Q: 스크롤이 부드럽지 않습니다

**A: 개선 방법**
1. `scrollSpeed` 값 조정 (기본 5.0)
2. `animationDuration` 값 조정 (기본 0.2초)
3. `enableSelectionAnimation` 끄기

---

## 성능 최적화

### 권장 설정

**100곡 이하:**
- 모든 기능 사용 가능
- 썸네일 전체 로딩

**100~300곡:**
- 썸네일 비동기 로딩 (기본 동작)
- 스크롤 시에만 로딩 (Lazy Loading 구현 권장)

**300곡 이상:**
- Virtual Scrolling 구현 권장
- 뷰포트에 보이는 아이템만 생성

### Virtual Scrolling 구현 (선택사항)

**고급 최적화:**
```csharp
// 화면에 보이는 아이템만 생성
// Unity UI Extensions의 Recyclable Scroll Rect 사용 권장
```

---

## 테스트 체크리스트

### 기능 테스트
- [ ] 곡 목록이 정상적으로 표시되는가?
- [ ] 스크롤이 부드러운가?
- [ ] 곡 선택이 정상적으로 작동하는가?
- [ ] 썸네일이 로드되는가?
- [ ] 정렬이 각 옵션별로 정상 작동하는가?
- [ ] 필터가 정상적으로 적용되는가?
- [ ] 검색이 정확한 결과를 반환하는가?
- [ ] 즐겨찾기 추가/제거가 작동하는가?
- [ ] 최고 점수가 표시되는가?
- [ ] 키보드 네비게이션이 작동하는가?

### 성능 테스트
- [ ] 100곡 로드 시간 < 1초
- [ ] 스크롤 프레임레이트 60fps 유지
- [ ] 메모리 사용량 적절한가?

---

## 참고 자료

**관련 파일:**
- `SongSelectionUIAdvanced.cs` - 메인 스크립트
- `SongListItem.cs` - 곡 아이템 스크립트
- `SongSelectionUI.cs` - 기본 버전 (참고용)
- `SongDatabase.cs` - 곡 데이터베이스
- `SongData.cs` - 곡 데이터 구조

**외부 문서:**
- `README_SongSelection.md` - 기본 사용 가이드
- `SONG_SELECTION_SUMMARY.md` - 구현 완료 보고서
- `DEVELOPMENT_TODO.md` - 전체 개발 계획

---

**작성자**: Claude Code  
**버전**: 1.0  
**최종 업데이트**: 2025-10-26
