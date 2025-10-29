# 긴급 수정 사항 (Critical Issues)

> **최근 업데이트**: 2025-10-27
> **우선순위**: 최상

[← 메인 TODO로 돌아가기](DEVELOPMENT_TODO.md)

---

## 🚨 긴급 수정 사항 (Critical)

### ~~1. 게임 시작 씬 설정~~ (완료: 2025-10-28)
**파일**: Unity Build Settings, Scene Management

**구현 완료**:
```
[X] Build Settings 씬 순서 설정
    [X] MainMenu.unity를 첫 번째 씬 [0]으로 설정
    [X] 씬 순서 구성 완료

[X] MainMenu 씬 구성 완료
    [X] Background 이미지 설정 (전체 화면)
    [X] 4개 버튼 생성 (PLAY, COURSE, OPTION, EXIT)
    [X] 타이틀 텍스트 추가
    [X] MainMenuManager 스크립트 연결
    [X] 버튼 동작 테스트 완료
```

**테스트 완료**:
```
[X] Unity Editor에서 버튼 클릭 정상 작동
[X] MainMenuManager 스크립트 연결 확인
[X] Build Settings 씬 순서 설정 완료
```

---

### ~~2. 중복된 점수 시스템 해결~~ (완료: 2025-10-25)
**파일**: `ScoreSystem.cs` vs `JudgmentResult.cs`

**문제**:
- 두 파일에 거의 동일한 `RhythmScoreSystem` 클래스 존재 (각 395줄)
- `ScoreSystem.cs`의 전체 클래스가 주석처리되어 있음
- `TimingAccuracy`, `ScoreType` enum 중복 정의
- 어느 것이 실제 사용되는지 불명확

**해결 완료**:
```
[X] ScoreSystem.cs 또는 JudgmentResult.cs 중 하나 선택 - JudgmentResult.cs 유지
[X] 사용하지 않는 버전 완전 삭제 - ScoreSystem.cs 삭제 완료
[X] 점수 계산 로직 통합 및 테스트 - JudgmentResult.cs가 ChartSystem 네임스페이스에서 활성화
[X] 모든 클래스가 단일 시스템 참조하도록 수정 - 중복 제거됨
```

### ~~3. HP/게이지 시스템 구현~~ (완료: 2025-10-25)
**관련 파일**: `HPSystem.cs`, `NoteController.cs`, `GearController.cs`

**문제**:
- `UpdateHP()` 메서드가 여러 곳에서 호출되지만 실제 구현 없음
- HP 증감 로직 없음
- 게이지 UI 표시 없음
- 클리어 조건 (Normal 70% 등) 코드에만 있고 실제 적용 안됨

**구현 완료**:
```
[X] HPSystem 클래스 생성 - Assets/Play/HPSystem.cs
[X] 판정별 HP 증감량 정의
    - S_Perfect: +2, Perfect: +1.5, Great: +1, Good: 0, Bad: -2, Miss: -5
[X] 난이도별 HP 감소율 차별화
    - Normal: 1.0x, Hard: 1.5x, Super: 2.0x
[X] HP 게이지 UI 연동 (HPBarAnimator와 연결)
[X] HP 0일 때 게임오버 처리 (UnityEvent)
[X] 클리어/실패 조건 판정 (CheckClearCondition)
[X] NoteController와 RhythmManager 통합
[X] JudgmentResult enum 제거, JudgmentType 통일
[X] GearController.ProcessJudgment() 추가
```


### ~~4. 오디오 시스템 통합~~ (완료: 2025-10-26)
**파일**: `AudioManager.cs` (797줄)

**구현 완료**:
```
[X] 노래 선택 시 오디오 파일 로드 - GameManager.StartGame()에서 LoadBGM() 호출
[X] AudioManager와 NoteManager/NoteSpawner 연동
[X] Time.time 대신 AudioManager.GetMusicTime() 사용
[X] 키 사운드 재생 시스템 구현
[X] 판정 효과음 추가 (Hit, Miss)
[X] 오디오 오프셋 설정 적용
[X] 일시정지/재개/게임오버 시 오디오 정지 처리
[X] StreamingAssets 폴더 구조 생성
[X] CoverArtLoader 통합 - 커버 이미지 동적 로딩
```


### ~~5. NoteData 생성자 버그~~ (완료: 2025-01-26)
**파일**: `Assets/Play/NoteData.cs`, `Assets/songselect/SampleChartGenerator.cs`

**해결 완료**:
```
[X] NoteData.cs에 기본 생성자 추가
[X] 기존 생성자는 그대로 유지 (하위 호환성)
[X] 두 가지 생성 방식 모두 지원
```

### ~~6. 차트 로딩 시스템 구현~~ (완료: 2025-10-25)
**구현 완료**:
```
[X] 차트 파일 형식 정의 (JSON) - NoteData 구조 활용
[X] ChartData 클래스 생성 - Assets/Play/ChartData.cs
[X] ChartLoader 클래스 생성 - Assets/Play/ChartLoader.cs
[X] 선택한 난이도/키 수에 맞는 차트 로드
[X] 차트 메타데이터 파싱 (BPM, offset, 아티스트 등)
[X] 노트 데이터 → NoteManager.SpawnNote() 변환
[X] 타이밍에 맞춰 노트 스폰 코루틴 구현
[X] GameManager 통합 - 두 시스템(NoteSpawner/NoteManager) 모두 지원
[X] 샘플 차트 생성 기능 (테스트용)
```

---

## 🎉 모든 긴급 수정 사항 완료!

**완료 날짜**: 2025-10-28

모든 Critical Issues가 해결되었습니다:
- ✅ 중복 점수 시스템 통합
- ✅ HP/게이지 시스템 구현
- ✅ 오디오 시스템 통합  
- ✅ NoteData 생성자 버그 수정
- ✅ 차트 로딩 시스템 구현
- ✅ **게임 시작 씬 설정 완료 (MainMenu)**

**다음 우선순위**: 
1. SongSelection 씬 구성
2. GameScene 통합 테스트
3. Result 씬 구현
