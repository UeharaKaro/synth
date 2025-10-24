# Synth - Unity 리듬 게임

<div align="center">

![Unity](https://img.shields.io/badge/Unity-2021.3+-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Mac%20%7C%20Linux-lightgrey.svg)

**고급 게임플레이 메커니즘과 포괄적인 UI 시스템을 갖춘 Unity 기반 모던 리듬 게임**

[기능](#-주요-기능) • [설치](#-설치-방법) • [사용법](#-사용-방법) • [문서](#-문서) • [기여](#-기여하기)

</div>

---

## 📖 개요

Synth는 Unity로 개발된 풍부한 기능을 갖춘 리듬 게임으로, 다양한 키 모드(4K-10K), 여러 난이도, 그리고 곡 선택, 게임플레이, 결과 화면을 포함한 완전한 게임 시스템을 제공합니다.

### 주요 특징

- 🎵 다중 키 모드 지원 (4K, 5K, 6K, 7K, 8K, 10K)
- 🎯 다양한 난이도의 고급 판정 시스템 (Normal, Hard, Super)
- 📊 정확도 계산을 포함한 포괄적인 스코어링 시스템
- 🎨 부드러운 애니메이션의 모던 UI
- 🎼 커스텀 비트맵 제작을 위한 차트 에디터
- 🔊 FMOD 오디오 통합
- 📈 상세한 통계와 랭킹이 있는 결과 화면

## ✨ 주요 기능

### 게임플레이 시스템

- **다중 키 지원**: 4키부터 10키까지 플레이 가능
- **판정 모드**:
  - Normal: 캐주얼 친화적인 타이밍 범위
  - Hard: 숙련된 플레이어를 위한 경쟁적 타이밍
  - Super: 전문가 수준의 정밀도 (개발 예정)
- **노트 타입**:
  - 일반 노트
  - 롱노트 (홀드)
  - 슬라이드 노트 (개발 예정)

### UI 시스템

#### 메인 메뉴
- 깔끔하고 직관적인 인터페이스
- 키보드 네비게이션 지원
- 모든 게임 모드에 쉽게 접근

#### 곡 선택
- ScriptableObject 아키텍처로 구현된 포괄적인 곡 데이터베이스
- 곡당 다중 난이도 지원
- 앨범 아트 및 배경 이미지 표시
- 곡 미리듣기 기능
- 장르, 아티스트, 키 개수별 필터링
- 진행도를 위한 곡 잠금 시스템

#### 결과 화면
- 상세한 플레이 통계
- 판정 분석 (S Perfect, Perfect, Great, Good, Bad, Miss)
- 정확도 계산
- 랭킹 시스템 (SSS부터 F까지)
- 풀 콤보 및 퍼펙트 플레이 표시
- 부드러운 애니메이션과 전환 효과

### 차트 에디터 (베타)

- 비트맵 제작을 위한 비주얼 차트 에디터
- BPM 및 오프셋 설정
- 노트 배치 및 타이밍 조정
- 커스텀 차트 형식으로 내보내기
- 자세한 내용은 [ChartEditorBeta_Documentation_Kr.md](ChartEditorBeta_Documentation_Kr.md) 참조

## 🚀 설치 방법

### 필수 요구사항

- **Unity**: 2021.3 이상 권장
- **Git**: 레포지토리 클론을 위해 필요
- **운영체제**: Windows, macOS, 또는 Linux

### 설치 단계

1. **레포지토리 클론**
   ```bash
   git clone https://github.com/UeharaKaro/synth.git
   cd synth
   ```

2. **Unity에서 열기**
   - Unity Hub 실행
   - "Add" 클릭 후 `Synth` 폴더 선택
   - Unity 2021.3 이상으로 프로젝트 열기

3. **의존성 설치**
   - FMOD는 이미 프로젝트에 포함되어 있음
   - TextMeshPro는 프로젝트 열 때 자동으로 설치됨

4. **곡 데이터베이스 설정**
   - `Assets/songselect/`로 이동
   - 우클릭 → Create → Rhythm Game → Song Database
   - 데이터베이스에 곡 추가
   - [곡 선택 README](Synth/Assets/songselect/README_SongSelection.md) 참조

## 📚 사용 방법

### 게임 플레이

1. **게임 시작**: Main Menu 씬에서 시작
2. **곡 선택**: Song Selection 화면에서 곡 선택
3. **난이도 및 키 모드 선택**
4. **플레이**: 음악에 맞춰 노트 치기
5. **결과 확인**: 곡 완료 후 결과 화면 보기

### 차트 제작

1. Chart Editor 씬 열기
2. 오디오 파일 로드
3. BPM 및 오프셋 설정
4. 타임라인에 노트 배치
5. 차트 내보내기
6. [차트 에디터 문서](ChartEditorBeta_Documentation_Kr.md) 참조

### 키보드 조작

**곡 선택 화면:**
- ↑/↓: 곡 탐색
- ←/→: 난이도 변경
- Shift: 키 모드 변경
- Enter: 곡 선택
- Space: 곡 미리듣기
- ESC: 메뉴로 돌아가기

**게임플레이:**
- 기본: D, F, J, K (4K 모드)
- 게임 설정에서 변경 가능

## 📁 프로젝트 구조

```
synth/
├── Synth/                      # Unity 프로젝트 폴더
│   └── Assets/
│       ├── AudioManager.cs     # 오디오 관리 시스템
│       ├── Play/               # 게임플레이 스크립트
│       │   ├── NoteManager.cs
│       │   ├── ScoreSystem.cs
│       │   ├── GearController.cs
│       │   └── ...
│       ├── Startmenu/          # 메인 메뉴 UI
│       │   ├── MainMenuUI.cs
│       │   └── ...
│       ├── songselect/         # 곡 선택 시스템
│       │   ├── SongSelectionUI.cs
│       │   ├── SongDatabase.cs
│       │   └── SongData.cs
│       ├── playresult/         # 결과 화면 시스템
│       │   ├── PlayResultUI.cs
│       │   ├── PlayResultData.cs
│       │   └── GameResultManager.cs
│       ├── option/             # 설정/옵션
│       │   ├── OptionMenuUI.cs
│       │   └── GameSettings.cs
│       ├── edit/               # 차트 에디터
│       │   └── ChartEditorNew.cs
│       └── Plugins/            # 서드파티 플러그인
│           └── FMOD/           # FMOD 오디오 엔진
├── .github/                    # GitHub 설정
│   └── workflows/              # CI/CD 워크플로우
├── README.md                   # 영문 README
├── README.ko.md                # 한국어 README (이 파일)
├── LICENSE                     # MIT 라이선스
└── .gitignore                  # Git 무시 규칙
```

## 📖 문서

각 시스템에 대한 상세 문서:

- **[곡 선택 시스템](Synth/Assets/songselect/README_SongSelection.md)**: 곡 데이터베이스, UI 설정, 키보드 조작
- **[플레이 결과 시스템](Synth/Assets/playresult/README_PlayResult.md)**: 결과 화면, 랭킹, 통계
- **[차트 에디터](ChartEditorBeta_Documentation_Kr.md)**: 비트맵 생성 및 편집
- **[차트 에디터 (영문)](ChartEditorBeta_Documentation.md)**: 차트 에디터 영문 가이드

## 🎮 게임 시스템

### 스코어링 시스템

게임은 다음을 포함하는 포괄적인 스코어링 시스템을 갖추고 있습니다:
- **점수 계산**: 판정 정확도 기반
- **콤보 시스템**: 연속 히트
- **정확도 백분율** 계산
- **랭크 결정**: (SSS, SS, S, A, B, C, D, F)

### 판정 시스템

다양한 스킬 레벨을 위한 여러 타이밍 범위:

**Normal 모드:**
- Perfect: ±41.66ms
- Great: ±83.33ms
- Good: ±120ms
- Bad: ±150ms

**Hard 모드:**
- S Perfect: ±16.67ms
- Perfect: ±32.25ms
- Great: ±62.49ms
- Good: ±88.33ms
- Bad: ±120ms

## 🛠️ 기술 스택

- **엔진**: Unity 2021.3+
- **언어**: C#
- **오디오**: FMOD
- **UI**: Unity UI + TextMeshPro
- **데이터 관리**: ScriptableObjects
- **버전 관리**: Git

## 🤝 기여하기

기여를 환영합니다! 가이드라인은 [CONTRIBUTING.md](CONTRIBUTING.md)를 참조하세요.

### 기여 방법

1. 레포지토리를 Fork
2. 기능 브랜치 생성 (`git checkout -b feature/AmazingFeature`)
3. 변경사항 커밋 (`git commit -m 'Add some AmazingFeature'`)
4. 브랜치에 Push (`git push origin feature/AmazingFeature`)
5. Pull Request 열기

## 📝 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다 - 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

## 👥 제작자

- **UeharaKaro** - *초기 작업* - [GitHub](https://github.com/UeharaKaro)

## 🙏 감사의 말

- 오디오 엔진을 제공한 FMOD
- 게임 엔진을 제공한 Unity Technologies
- 모든 기여자와 테스터

## 📮 연락처

프로젝트 링크: [https://github.com/UeharaKaro/synth](https://github.com/UeharaKaro/synth)

## 🌐 다른 언어

- [English](README.md) - 영어

---

<div align="center">
Unity로 ❤️를 담아 제작
</div>
