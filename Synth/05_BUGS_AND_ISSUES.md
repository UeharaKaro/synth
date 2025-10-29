# 버그 및 코드 이슈 (Bugs & Code Issues)

> **최근 업데이트**: 2025-10-27
> **우선순위**: 중간

[← 메인 TODO로 돌아가기](DEVELOPMENT_TODO.md)

---

## 🐛 버그 및 코드 이슈 (Bugs & Code Issues)

### 긴급 버그 (Critical Bugs)

#### 3. InputManager.cs 빈 메서드 구현
**파일**: `Assets/Play/InputManager.cs`

**문제**:
- `ProcessInput()` 메서드 내부가 비어있음
- 입력 처리 로직 없음

**해결 방안**:
```
[ ] 키 입력 감지 구현
    [ ] Input System 또는 Input.GetKeyDown() 사용
    [ ] 4K~10K 키 레이아웃 매핑

[ ] GearController에 입력 전달
    [ ] 레인 번호와 함께 입력 이벤트 전달

[ ] 롱노트 홀드 감지
    [ ] 키 누르는 동안 지속 입력
```

#### 4. NoteController.cs 비효율적 검색
**파일**: `Assets/Play/NoteController.cs` (약 262줄)

**문제**:
- `Update()`에서 매 프레임 `FindObjectOfType<RhythmManager>()` 호출
- 성능 저하 (특히 노트가 많을 때)

**해결 방안**:
```
[ ] Singleton 패턴 활용
    private RhythmManager rhythmManager;

    void Start() {
        rhythmManager = RhythmManager.Instance;
    }

[ ] 또는 GameManager를 통한 참조
    rhythmManager = GameManager.Instance.RhythmManager;
```

---

### 잠재적 버그 (Potential Issues)

#### 1. Null Reference 위험
**여러 파일에서 발견**

**위험 지점**:
```csharp
// ❌ Null 체크 없이 접근
AudioManager.Instance.PlayBGM(audioFileName);

// ✅ 안전한 코드
if (AudioManager.Instance != null) {
    AudioManager.Instance.PlayBGM(audioFileName);
} else {
    Debug.LogError("AudioManager not found");
}
```

**수정 필요 파일**:
- `GameManager.cs`: AudioManager, ChartLoader 참조
- `NoteController.cs`: RhythmManager 참조
- `SongSelectionUI.cs`: AudioManager, CoverArtLoader 참조

#### 3. LongNoteSystem.cs 타입 캐스팅 안전성
**파일**: `Assets/Play/LongNoteSystem.cs`

**문제**: `as` 캐스팅 후 null 체크 누락

**해결**:
```csharp
NoteController noteController = note as NoteController;
if (noteController != null) {
    noteController.OnLongNoteHoldUpdate(heldTime);
}
```
