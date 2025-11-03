# Synth 리듬게임 개발 TODO 및 이슈 정리

> **프로젝트 완성도**: 약 97% (베타 단계)
> **최근 업데이트**: 2025-11-03 (세션 5회)
> **예상 완료 시간**: 9.5시간 (약 87시간 진행, 8.5시간 완료)
> **✅ 완료**: ResultScene 자동 빌더 생성

---

## 📋 문서 구조

이 문서는 가독성을 위해 여러 파일로 분리되었습니다:

1. **[01_CRITICAL_ISSUES.md](01_CRITICAL_ISSUES.md)** - 긴급 수정 사항
2. **[02_SCENE_SETUP.md](02_SCENE_SETUP.md)** - Unity 씬 설정 가이드
3. **[03_COPYRIGHT_PROTECTION.md](03_COPYRIGHT_PROTECTION.md)** - 저작권 보호 시스템
4. **[04_MISSING_FEATURES.md](04_MISSING_FEATURES.md)** - 구현 필요 기능
5. **[05_BUGS_AND_ISSUES.md](05_BUGS_AND_ISSUES.md)** - 버그 및 코드 이슈
6. **[06_UI_UX_IMPROVEMENTS.md](06_UI_UX_IMPROVEMENTS.md)** - UI/UX 개선 사항
7. **[07_PERFORMANCE.md](07_PERFORMANCE.md)** - 성능 최적화
8. **[08_REFACTORING.md](08_REFACTORING.md)** - 코드 리팩토링
9. **[09_DEVELOPMENT_PLAN.md](09_DEVELOPMENT_PLAN.md)** - 우선순위별 개발 계획

---

## 📊 프로젝트 현황 요약

### 최근 완료 작업 (2025-11-03, 세션 #5)

**ResultScene 자동 빌더 생성 ✅**:
- **파일**: `Assets/Editor/ResultSceneBuilder.cs` (550줄)
- **기능**:
  - Unity 메뉴: Tools → Build Result Scene
  - 클릭 한 번으로 완전한 ResultScene 자동 생성
  - 60개 이상 UI 요소 자동 생성 및 연결
  - 곡 정보, 결과 정보, 판정 통계, 특수 표시, 버튼 패널
  - GameResultManager, ResultSceneLoader 자동 추가
  - 생성 시간: 약 5초
- **가이드**: `RESULTSCENE_AUTO_BUILD.md` 생성 (350줄)

**GameEvents 매개변수 수정 ✅**:
- **문제**: NoteController에서 `RaiseNoteHit(result, timeDifferenceMs)` 2개 매개변수로 호출하지만 GameEvents는 1개만 받음
- **해결**:
  - `GameEvents.OnNoteHit`: `Action<JudgmentType>` → `Action<JudgmentType, float>`
  - `GameEvents.RaiseNoteHit`: 2번째 매개변수 추가 (`float timeDifferenceMs = 0f`)
  - `GameplayUIManager.OnNoteHit`: 매개변수 추가
  - `GameScoreManager.HandleNoteHit`: 매개변수 추가
- **효과**: UI 시스템에서 타이밍 오프셋 정보 활용 가능 (Early/Late 표시 등)

**OptionsScene 자동 빌더 생성 ✅**:
- **파일**: `Assets/Editor/OptionsSceneBuilder.cs` (750줄)
- **기능**:
  - Unity 메뉴: Tools → Build Options Scene
  - 클릭 한 번으로 완전한 OptionsScene 자동 생성
  - Canvas, SettingsManager, OptionMenuUI + 50개 이상 UI 요소 생성
  - 3개 탭 (오디오/비주얼/게임플레이)
  - 9개 슬라이더, 1개 드롭다운, 2개 토글, 3개 버튼
  - 모든 참조 자동 연결
  - 생성 시간: 약 5초
- **가이드**: `OPTIONSSCENE_AUTO_BUILD.md` 생성

### 이전 완료 작업 (2025-11-02, 세션 #4)

**코드 중복 제거 및 컴파일 오류 수정 ✅**:
- **JudgmentExtensions 중복 제거**
  - `Assets/Play/JudgmentExtensions.cs` 삭제
  - `GameEnums.cs`의 JudgmentExtensions 클래스만 사용 (GetColor 메서드 포함)
  - Unity 캐시 및 .csproj 파일 정리

- **CreateSongListItemPrefab.cs 오류 수정**
  - `backgroundImage` private 필드 접근 오류 해결 (Awake에서 자동 할당)
  - `selectionIndicator` 타입 불일치 수정 (GameObject → Image)
  - 코드 정리 및 주석 추가

### 이전 완료 작업 (2025-01-29, 세션 #3)

**UI 애니메이션 시스템 구현 완료 ✅**:
- **이벤트 시스템** (`GameEvents.cs` - 90줄)
  - 게임 전역 이벤트 관리
  - OnNoteHit, OnNoteMiss, OnSongStart 등
  - 콤보, 점수, 퍼센트 변경 이벤트
  
- **점수/콤보 관리자** (`GameScoreManager.cs` - 235줄)
  - 콤보 배율 시스템 (50→x2, 100→x3, 200→x4)
  - 판정별 통계 추적
  - 정확도 계산 (가중치 기반)
  
- **UI 애니메이션 통합** (`GameplayUIManager.cs` - 355줄)
  - 트랙 슬라이드 인/아웃 (0.8초 애니메이션)
  - 콤보 펄스 (1.35x 스케일)
  - 판정 텍스트 (Punch + Fade)
  - 점수/퍼센트 실시간 갱신
  
- **히트 이펙트 풀링** (`HitEffectPool.cs`, `HitEffect.cs`)
  - 오브젝트 풀링으로 성능 최적화
  - 판정별 색상/애니메이션
  - 스케일 + 페이드 효과
  
- **판정 확장 메서드** (`GameEnums.cs` 확장)
  - BasePoints(), BreaksCombo(), ToDisplayString(), GetColor()
  
- **완벽한 구현 가이드** (`UI_ANIMATION_IMPLEMENTATION_GUIDE.md` - 650줄)
  - Unity 세팅 단계별 가이드
  - 코드 통합 방법
  - 애니메이션 커스터마이징
  - 체크리스트

### 이전 완료 작업 (2025-01-29, 세션 #2)

**HPBar 및 Track 수정 완료 ✅**:
- **Track 폭 조정**: 1.0 → 0.6 (40% 얇게)
- **Track 간격 조정**: 1.1 → 0.8 (더 좁게)
- **HPBar 세로형 명확화**: 폭 0.4, 높이 7.0
- **GameSceneBuilder.cs 수정 완료**
- **수정 가이드 문서 작성** (`HPBAR_TRACK_FIX_GUIDE.md`)

### 이전 완료 작업 (2025-01-29, 세션 #1)

**GameScene 설정 가이드 작성 완료 ✅**:
- **GAMESCENE_SETUP_INSTRUCTIONS.md 생성**
  - 3가지 설정 방법 제공 (자동/수동/체크리스트)
  - 방법 1: GameSceneBuilder 사용 (5분)
  - 방법 2: 부분 업데이트 (30분)
  - 방법 3: 체크리스트 검증 (10분)
  - 테스트 실행 가이드 포함
  - 문제 해결 섹션 포함

### 이전 완료 작업 (2025-01-29)

**저작권 보호 시스템 구현 완료 ✅** (Phase 4-A ~ 4-D):
- **AES-256 암호화 시스템**
  - SecureAssetEncryptor.cs 생성 (669줄)
  - 군사급 보안 수준 (AES-256-CBC)
  - XOR/AES 모드 전환 지원
  - 암호화 헤더 자동 감지
  
- **런타임 복호화 시스템**
  - SecureAssetLoader 클래스 구현
  - AudioManager 통합 완료
  - CoverArtLoader 통합 완료
  - 캐싱 시스템으로 성능 최적화
  
- **빌드 자동화**
  - BuildAutomation.cs 생성 (359줄)
  - 빌드 전 자동 암호화
  - 백업/복원 시스템
  - 수동 복원 메뉴 추가
  
- **문서화**
  - README_ENCRYPTION.md (상세 사용 가이드)
  - API 참조 문서
  - 문제 해결 가이드

**이전 작업 (2025-10-28)**
- **Background 이미지 설정**
  - RectTransform Anchor 설정 (Stretch-Stretch)
  - 전체 화면 커버 확인
  - Canvas Scaler 설정 완료
  
- **UI 구성 완료**
  - 4개 버튼 생성 (PLAY, COURSE, OPTION, EXIT)
  - 타이틀 텍스트 추가
  - MainMenuManager 스크립트 연결
  - 버튼 동작 테스트 완료
  
- **Build Settings 구성**
  - MainMenu.unity를 첫 번째 씬 [0]으로 설정
  - 씬 순서 정의 완료
  - 게임 시작 지점 설정 완료

**이전 작업 (2025-10-27)**

**Phase 2-B-5 완료 ✅**:
- **옵션 메뉴 UI 시스템** (OptionMenuUI.cs, 630줄)
  - 탭 시스템 구현 (오디오/비주얼/게임플레이)
  - SFX 볼륨 슬라이더 추가
  - 판정 모드 드롭다운 (Normal/Hard/Super)
  - 실시간 AudioManager 연동 (볼륨 프리뷰)
  - SettingsManager 통합 (JSON 저장/로드)

**Phase 2-B-7 완료 ✅**:
- **Unity 씬 설정 가이드** (상세 문서 작성)
- **GameManager 참조 시스템 개선**
- **NoteManager 완전 구현**
- **버그 수정** (NullReferenceException 7곳 해결)

**Phase 2-B-6 완료 ✅**:
- **진행도 표시 시스템** (ProgressDisplay.cs)
- **점수 표시 시스템** (ScoreDisplay.cs)
- **HP 게이지 개선** (HPBarAnimator.cs)

**Phase 2-B-4 완료 ✅**:
- 스크롤 뷰 곡 목록 시스템
- 7가지 정렬 옵션, 5가지 필터 시스템
- 실시간 검색 기능
- 즐겨찾기 시스템

---

### 시스템별 완성도

| 시스템 | 완성도 | 상태 | 남은 시간 |
|-------|--------|------|----------|
| 게임플레이 | 98% | 🟢 완료 | 0.5시간 |
| **UI 애니메이션** | **95%** | **✅ 완료** | **1.5시간** |
| 노래 선택 | 95% | ✅ 완료 | 1시간 |
| 메인 메뉴 | 95% | ✅ 완료 | 0.5시간 |
| 일시정지 시스템 | 100% | ✅ 완료 | - |
| 결과 화면 | 90% | 100% | ✅ 완료 (Unity 설정 포함) | - |
| 차트 에디터 | 75% | 🟢 진행중 | 6시간 |
| 옵션 메뉴 | 100% | ✅ 완료 (Unity 설정 포함) | - |
| 오디오 시스템 | 100% | ✅ 완료 | - |
| 에셋 관리 | 90% | 🟢 완료 | 1시간 |
| 보안 시스템 | 90% | ✅ 완료 | 1시간 |
| Unity 씬 설정 | 92% | 🟢 완료 | 1시간 |
| **전체** | **96%** | **🟢 Beta** | **12시간** |

---

## 🎯 다음 우선순위 작업

### 1️⃣ Unity Editor에서 UI 애니메이션 시스템 구현 - ✅ 자동화 완료!

**🚀 자동 생성 스크립트 (1분)** ⭐ 가장 추천!
- [UIAnimationSystemBuilder.cs](Assets/Editor/UIAnimationSystemBuilder.cs) - 자동 생성 스크립트
- [UI_ANIMATION_AUTO_GENERATOR_GUIDE.md](UI_ANIMATION_AUTO_GENERATOR_GUIDE.md) - 사용 가이드

**사용법**:
```
1. Unity 열기
2. GameScene 열기
3. Tools → Synth → Create UI Animation System
4. "🚀 자동 생성 시작" 버튼 클릭
5. 완료! (1분)
```

**📚 수동 구현 가이드** (학습용):
- [UI_ANIMATION_CHECKLIST.md](UI_ANIMATION_CHECKLIST.md) - 단계별 체크리스트 (1.5시간)
- [UI_ANIMATION_QUICKSTART.md](UI_ANIMATION_QUICKSTART.md) - 빠른 시작 가이드 (30분)
- [UI_ANIMATION_IMPLEMENTATION_GUIDE.md](UI_ANIMATION_IMPLEMENTATION_GUIDE.md) - 전체 상세 가이드

**✅ 코드 작업 완료 (2025-11-03)**:
```
[X] GameEvents.cs - 이미 존재
[X] GameScoreManager.cs - 이미 존재
[X] GameplayUIManager.cs - 이미 존재
[X] HitEffectPool.cs - 이미 존재
[X] HitEffect.cs - 이미 존재
[X] GameEnums.cs 확장 메서드 - 이미 존재
[X] NoteController.cs에 GameEvents 호출 추가
[X] GameManager.cs에 게임 시작 이벤트 추가
[X] UI_ANIMATION_CHECKLIST.md 생성 (체크리스트)
[X] UI_ANIMATION_QUICKSTART.md 생성 (빠른 가이드)
[X] UIAnimationSystemBuilder.cs 생성 (자동 생성 스크립트) ⚡
[X] UI_ANIMATION_AUTO_GENERATOR_GUIDE.md 생성 (사용 가이드)
```

**🎮 Unity Editor 작업 (자동화됨)**:
```
[✓] Canvas 및 UI 오브젝트 생성 → 자동 생성 ⚡
[✓] 매니저 오브젝트 생성 → 자동 생성 ⚡
[✓] HitEffect Prefab 생성 → 자동 생성 ⚡
[✓] 참조 연결 → 자동 연결 ⚡
[ ] Play 모드 테스트 (1분)
    [ ] 트랙 슬라이드 인 확인
    [ ] 콤보/점수 애니메이션 확인
    [ ] 판정 텍스트 확인
    [ ] 히트 이펙트 확인
```

### 2️⃣ SongDatabase 자동 생성 - ✅ 완료 (2025-11-03) ⚡
**파일**: 
- [SongDatabaseBuilder.cs](Assets/Editor/SongDatabaseBuilder.cs) - 자동 생성 스크립트
- [SONGDATABASE_AUTO_BUILD_GUIDE.md](SONGDATABASE_AUTO_BUILD_GUIDE.md) - 사용 가이드

**사용법**:
```
1. Unity 메뉴: Tools → Synth → Create Sample SongDatabase
2. 옵션 선택 (곡 개수, 난이도 등)
3. "🚀 SongDatabase 생성" 클릭
4. 완료! (30초)
```

**✅ 완료 (2025-11-03)**:
```
[X] SongDatabaseBuilder.cs 생성 (420+ 줄)
[X] 샘플 곡 자동 생성 (1~10개 선택 가능)
[X] 다중 난이도 지원 (Easy/Normal/Hard)
[X] 다중 키 모드 지원 (4K/6K/8K)
[X] Inspector 커스텀 에디터 (정렬, 검증 기능)
[X] SONGDATABASE_AUTO_BUILD_GUIDE.md 작성

[ ] Unity에서 실행 테스트 (30초)
[ ] SongSelectionScene에 연결
```

### 3️⃣ OptionsScene 자동 생성 - ✅ 완료 (2025-11-03)
**파일**: [OPTIONSSCENE_AUTO_BUILD.md](OPTIONSSCENE_AUTO_BUILD.md)
```
[X] OptionsSceneBuilder.cs 생성 - 완료: 2025-11-03
    [X] Unity 메뉴 추가 (Tools → Build Options Scene)
    [X] Canvas, SettingsManager, OptionMenuUI 자동 생성
    [X] 50개 이상 UI 요소 자동 연결
    [X] 사용 가이드 문서 작성
    
[X] Unity Editor에서 OptionsScene 생성 - 완료: 2025-11-03
[X] Play 모드 테스트 완료
```

### 4️⃣ Unity 씬 설정 완료 (1시간)
**파일**: [02_SCENE_SETUP.md](02_SCENE_SETUP.md)
```
[X] GameScene 설정 가이드 - 완료: 2025-01-29
    [X] GAMESCENE_SETUP_INSTRUCTIONS.md 작성
    [X] 3가지 방법 제공 (자동/수동/체크리스트)
    [X] GameSceneBuilder 확인 (이미 존재)
    [X] HPBar 및 Track 수정 완료
    
[ ] Unity Editor에서 GameScene 실행 (5분)
[ ] SongSelection 씬 - 새 UI 컴포넌트 추가 (30분)
[ ] ResultScene 씬 - 애니메이션 확인 (15분)
[ ] 전체 플로우 테스트 (10분)
```

### 5️⃣ 저작권 보호 시스템 테스트 (1시간)
**파일**: [03_COPYRIGHT_PROTECTION.md](03_COPYRIGHT_PROTECTION.md)
```
[X] Phase 4-A: 현재 시스템 개선 (2시간) - 완료: 2025-01-29
    [X] SecureAssetEncryptor.cs 생성 (AES-256 암호화)
    [X] 암호화 키 관리 시스템
    [X] 에디터 메뉴 통합

[X] Phase 4-B: AES 암호화 마이그레이션 (3시간) - 완료: 2025-01-29
    [X] AES-256 암호화 구현
    [X] 암호화 헤더 시스템 (자동 감지)
    [X] XOR/AES 모드 전환 기능

[X] Phase 4-C: 시스템 통합 (2시간) - 완료: 2025-01-29
    [X] AudioManager 복호화 통합
    [X] CoverArtLoader 복호화 통합
    [X] SecureAssetLoader 런타임 시스템

[X] Phase 4-D: 빌드 자동화 (1시간) - 완료: 2025-01-29
    [X] BuildPreprocessor 구현
    [X] BuildPostprocessor 구현
    [X] 백업/복원 시스템
    [X] 수동 복원 메뉴 추가

[X] 문서화 (1시간) - 완료: 2025-01-29
    [X] README_ENCRYPTION.md 작성
    [X] 사용법 가이드
    [X] API 문서
    [X] 문제 해결 가이드

[ ] 테스트 및 검증 (1시간)
    [ ] 암호화/복호화 기능 테스트
    [ ] 빌드 자동화 테스트
    [ ] 성능 벤치마크
```

### 6️⃣ 차트 에디터 고급 기능 (6시간)
**파일**: [04_MISSING_FEATURES.md](04_MISSING_FEATURES.md)
```
[ ] 마우스 입력 개선
[ ] BPM 변속 시스템
[ ] 플레이테스트 기능
[ ] 분석 및 시각화 도구
```

---

## 🚨 긴급 이슈

~~모든 긴급 이슈가 해결되었습니다.~~ (완료: 2025-11-02)

**최근 해결 (2025-11-02)**:
- ~~JudgmentExtensions 중복 정의 오류~~ ✅ 해결
- ~~CreateSongListItemPrefab.cs 컴파일 오류~~ ✅ 해결
- ~~Unity 캐시 및 프로젝트 파일 정리~~ ✅ 완료

세부 사항은 [01_CRITICAL_ISSUES.md](01_CRITICAL_ISSUES.md)를 참조하세요.

---

## 📖 개발 가이드

### 새로운 기능 추가 시
1. 해당 카테고리의 마크다운 파일 확인
2. 체크리스트에 항목 추가
3. 완료 후 `[X]` 또는 `✅`로 표시
4. 완료 날짜 기록

### 버그 발견 시
1. [05_BUGS_AND_ISSUES.md](05_BUGS_AND_ISSUES.md)에 추가
2. 우선순위 설정 (긴급/중간/낮음)
3. 해결 방안 작성
4. 완료 후 ~~취소선~~ 처리

### 성능 최적화
[07_PERFORMANCE.md](07_PERFORMANCE.md)에서 최적화 체크리스트 확인

### 코드 리팩토링
[08_REFACTORING.md](08_REFACTORING.md)에서 개선 사항 확인

---

## 📝 프로젝트 정보

- **Unity 버전**: 2021.3+ (6000.1.4f1)
- **오디오**: FMOD 사용
- **UI**: Unity UI + TextMeshPro
- **언어**: C# 9.0
- **빌드 타겟**: Windows (기본)

---

## ✅ Phase별 완료 현황

- [X] **Phase 1**: 핵심 시스템 완성 (완료: 2025-10-26)
- [X] **Phase 2-A**: 노래 선택 시스템 (완료: 2025-10-26)
- [🔄] **Phase 2-B**: UI 완성 (98% 완료, UI 애니메이션 시스템 구현)
- [🔲] **Phase 3**: 차트 에디터 완성 (75% 완료)
- [🔄] **Phase 4**: 보안 시스템 & 폴리싱 (90% 완료, 테스트 필요)

상세 계획은 [09_DEVELOPMENT_PLAN.md](09_DEVELOPMENT_PLAN.md) 참조

---

## 🔗 관련 문서

- **DEVELOPMENT_ARCHIVE.md** - 완료된 작업의 상세 로그
- **SESSION_SUMMARY/** - 세션별 작업 기록
  - **SESSION_SUMMARY_2025-01-29_UI_Animation.md** - UI 애니메이션 시스템 (최신)
  - **SESSION_SUMMARY_2025-01-29_GameScene_Setup.md** - GameScene 가이드
  - **SESSION_SUMMARY_2025-10-28_MainMenu_Setup.md** - MainMenu 설정
- **ChartEditorBeta_Documentation.md** - 차트 에디터 사용 가이드
- **CLAUDE.md** - Claude Code 개발 가이드
- **UI_ANIMATION_IMPLEMENTATION_GUIDE.md** - UI 애니메이션 구현 가이드 (신규)
- **HPBAR_TRACK_FIX_GUIDE.md** - HPBar/Track 수정 가이드
- **GAMESCENE_SETUP_INSTRUCTIONS.md** - GameScene 설정 가이드
- **ChartEditorBeta_Documentation.md** - 차트 에디터 사용 가이드
- **CLAUDE.md** - Claude Code 개발 가이드

---

**마지막 업데이트**: 2025-11-02 (세션 4회 완료)
**문서 버전**: 3.4 (코드 중복 제거 및 컴파일 오류 수정)
**다음 단계**: Unity Editor에서 UI 애니메이션 시스템 구현 (1.5시간)
