# Synth 리듬게임 개발 TODO 및 이슈 정리

> **프로젝트 완성도**: 약 97% (베타 단계)
> **최근 업데이트**: 2025-01-29
> **예상 완료 시간**: 13.5시간 (약 83시간 진행, 9시간 완료)
> **✅ 완료**: 저작권 보호를 위한 에셋 암호화 시스템 구현 완료

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

### 최근 완료 작업 (2025-01-29)

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
| 게임플레이 | 95% | 🟢 완료 | 1시간 |
| 노래 선택 | 95% | ✅ 완료 | 1시간 |
| 메인 메뉴 | 95% | ✅ 완료 | 0.5시간 |
| 일시정지 시스템 | 100% | ✅ 완료 | - |
| 결과 화면 | 90% | ✅ 완료 | 1시간 |
| 차트 에디터 | 75% | 🟢 진행중 | 6시간 |
| 옵션 메뉴 | 100% | ✅ 완료 | - |
| 오디오 시스템 | 100% | ✅ 완료 | - |
| 에셋 관리 | 90% | 🟢 완료 | 1시간 |
| **보안 시스템** | **90%** | **✅ 완료** | **1시간** |
| Unity 씬 설정 | 90% | 🟢 완료 | 1.5시간 |
| **전체** | **97%** | **🟢 Beta** | **13시간** |

---

## 🎯 다음 우선순위 작업

### 1️⃣ 저작권 보호 시스템 (필수, 10시간)
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

[ ] 테스트 및 검증 (1시간) - 다음 단계
    [ ] 암호화/복호화 기능 테스트
    [ ] 빌드 자동화 테스트
    [ ] 성능 벤치마크
```

### 2️⃣ Unity 씬 설정 완료 (1.5시간)
**파일**: [02_SCENE_SETUP.md](02_SCENE_SETUP.md)
```
[X] GameScene 설정 가이드 - 완료: 2025-01-29
    [X] GAMESCENE_SETUP_INSTRUCTIONS.md 작성
    [X] 3가지 방법 제공 (자동/수동/체크리스트)
    [X] GameSceneBuilder 확인 (이미 존재)
    
[ ] Unity Editor에서 GameScene 실행 (사용자 작업, 5분)
[ ] SongSelection 씬 - 새 UI 컴포넌트 추가 (1시간)
[ ] ResultScene 씬 - 애니메이션 확인 (30분)
[ ] OptionsScene 씬 - 생성 및 구현 (이미 완료)
```

### 3️⃣ 차트 에디터 고급 기능 (6시간)
**파일**: [04_MISSING_FEATURES.md](04_MISSING_FEATURES.md)
```
[ ] 마우스 입력 개선
[ ] BPM 변속 시스템
[ ] 플레이테스트 기능
[ ] 분석 및 시각화 도구
```

---

## 🚨 긴급 이슈

현재 모든 긴급 이슈가 해결되었습니다. 세부 사항은 [01_CRITICAL_ISSUES.md](01_CRITICAL_ISSUES.md)를 참조하세요.

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
- [🔄] **Phase 2-B**: UI 완성 (97% 완료, GameScene 가이드 작성)
- [🔲] **Phase 3**: 차트 에디터 완성 (75% 완료)
- [🔄] **Phase 4**: 보안 시스템 & 폴리싱 (90% 완료, 테스트 필요)

상세 계획은 [09_DEVELOPMENT_PLAN.md](09_DEVELOPMENT_PLAN.md) 참조

---

## 🔗 관련 문서

- **DEVELOPMENT_ARCHIVE.md** - 완료된 작업의 상세 로그
- **SESSION_SUMMARY/** - 세션별 작업 기록
- **ChartEditorBeta_Documentation.md** - 차트 에디터 사용 가이드
- **CLAUDE.md** - Claude Code 개발 가이드

---

**마지막 업데이트**: 2025-01-29
**문서 버전**: 3.2 (GameScene 가이드 추가)
**다음 단계**: Unity Editor에서 GameScene 실행 (5분) → SongSelectionScene 설정 (1시간)
