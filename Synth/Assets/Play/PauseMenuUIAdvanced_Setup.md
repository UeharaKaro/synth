# PauseMenuUIAdvanced 설정 가이드

## 🌟 새로운 고급 기능

**PauseMenuUIAdvanced.cs**는 기본 PauseMenuUI에 다음 기능을 추가합니다:
- ✨ 페이드 인/아웃 애니메이션
- ✨ 스케일 팝업 애니메이션
- 🔊 사운드 이펙트 (일시정지, 재개, 버튼 클릭, 버튼 호버)
- ⚙️ 애니메이션 곡선 커스터마이징
- 🎚️ unscaled시간 사용 (Time.timeScale=0에서도 작동)

---

## 📋 기본 설정 vs 고급 설정

| 기능 | PauseMenuUI | PauseMenuUIAdvanced |
|------|-------------|---------------------|
| 기본 일시정지 | ✅ | ✅ |
| ESC 키 토글 | ✅ | ✅ |
| GameManager 통합 | ✅ | ✅ |
| **페이드 애니메이션** | ❌ | ✅ |
| **스케일 애니메이션** | ❌ | ✅ |
| **사운드 이펙트** | ❌ | ✅ |
| **버튼 호버 사운드** | ❌ | ✅ |
| **애니메이션 곡선** | ❌ | ✅ |

---

## 🎬 Unity Editor 설정 방법

### 1단계: CanvasGroup 추가

1. Hierarchy에서 `PauseMenuPanel` 선택
2. Inspector에서 `Add Component` → `Canvas Group`
3. **Canvas Group 설정**:
   - Alpha: 1
   - Interactable: ✅
   - Block Raycasts: ✅

**Canvas Group 역할**: 페이드 애니메이션을 위한 투명도 제어

---

### 2단계: PauseMenuUIAdvanced 스크립트 교체

1. 기존 `PauseMenuManager` GameObject 선택
2. 기존 `PauseMenuUI` 스크립트 제거 (또는 비활성화)
3. `Add Component` → `PauseMenuUIAdvanced` 스크립트 추가

---

### 3단계: 기본 참조 연결

**UI 참조:**
- **Pause Menu Panel**: `PauseMenuPanel` 드래그
- **Canvas Group**: `PauseMenuPanel`의 Canvas Group 드래그
- **Panel Transform**: `PauseMenuPanel`의 Rect Transform 드래그 (자동 설정됨)
- **Resume Button**: `ResumeButton` 드래그
- **Restart Button**: `RestartButton` 드래그
- **Options Button**: `OptionsButton` 드래그
- **Main Menu Button**: `MainMenuButton` 드래그

**텍스트 참조 (선택사항):**
- Title Text, Resume Text, Restart Text, Options Text, Main Menu Text

---

### 4단계: 애니메이션 설정

#### 페이드 애니메이션
- **Enable Animations**: ✅ 체크
- **Fade In Duration**: 0.2 (초) - 메뉴 나타나는 시간
- **Fade Out Duration**: 0.15 (초) - 메뉴 사라지는 시간
- **Fade In Curve**: 기본 EaseInOut 곡선 (커스터마이징 가능)
- **Fade Out Curve**: 기본 EaseInOut 곡선

**커브 편집 방법**:
1. Fade In Curve 클릭
2. Curve Editor 열림
3. 원하는 형태로 곡선 편집 (EaseIn, EaseOut, Linear 등)

#### 스케일 애니메이션
- **Enable Scale Animation**: ✅ 체크
- **Start Scale**: (0.85, 0.85, 1) - 시작 크기 (85%)
- **Target Scale**: (1, 1, 1) - 최종 크기 (100%)
- **Scale Animation Duration**: 0.25 (초)

**추천 설정**:
- 작은 팝업: Start Scale (0.8, 0.8, 1)
- 큰 팝업: Start Scale (0.9, 0.9, 1)
- 바운스 효과: Duration 0.3초 + Overshoot 곡선

---

### 5단계: 사운드 설정

#### 사운드 클립 준비
다음 오디오 파일을 준비하세요:
- `pause_sound.wav` - 일시정지 시 재생
- `resume_sound.wav` - 재개 시 재생
- `button_click.wav` - 버튼 클릭 시 재생
- `button_hover.wav` - 버튼 호버 시 재생

#### 사운드 설정
- **Enable Sounds**: ✅ 체크
- **Pause Sound**: `pause_sound` 오디오 클립 드래그
- **Resume Sound**: `resume_sound` 오디오 클립 드래그
- **Button Click Sound**: `button_click` 오디오 클립 드래그
- **Button Hover Sound**: `button_hover` 오디오 클립 드래그
- **Sound Volume**: 0.7 (0.0 ~ 1.0)

**참고**: 사운드 파일이 없어도 작동합니다 (사운드만 재생 안됨)

---

### 6단계: 일반 설정

- **Pause Key**: Escape
- **Main Menu Scene Name**: "MainMenu"
- **Options Scene Name**: "OptionsScene"
- **Enable Only In Gameplay**: ✅ 체크
- **Block Game Input When Paused**: ✅ 체크

---

## 🎨 커스터마이징 가이드

### 애니메이션 속도 조절

**빠른 애니메이션** (반응성 중시):
```
Fade In Duration: 0.1
Fade Out Duration: 0.1
Scale Animation Duration: 0.15
```

**느린 애니메이션** (고급스러운 느낌):
```
Fade In Duration: 0.3
Fade Out Duration: 0.25
Scale Animation Duration: 0.35
```

### 애니메이션 곡선 프리셋

**Linear (선형)**:
- 시작: (0, 0)
- 끝: (1, 1)
- 일정한 속도

**EaseIn (가속)**:
- 느리게 시작 → 빠르게 끝
- 부드러운 등장

**EaseOut (감속)**:
- 빠르게 시작 → 느리게 끝
- 부드러운 정지

**EaseInOut (S-곡선)**:
- 느리게 시작 → 빠르게 → 느리게 끝
- 가장 자연스러움 (권장)

**Bounce (튕김 효과)**:
- Overshoot 사용
- 목표 지점을 넘었다가 돌아옴
- 활기찬 느낌

### 스케일 애니메이션 변형

**줌 인 효과**:
```
Start Scale: (0.5, 0.5, 1)  # 50%에서 시작
Target Scale: (1, 1, 1)
```

**줌 아웃 효과**:
```
Start Scale: (1.2, 1.2, 1)  # 120%에서 시작
Target Scale: (1, 1, 1)
```

**수평 확대**:
```
Start Scale: (0, 1, 1)  # 가로 0%
Target Scale: (1, 1, 1)
```

### 사운드 볼륨 조절

**조용한 게임**:
```
Sound Volume: 0.3 ~ 0.5
```

**보통**:
```
Sound Volume: 0.6 ~ 0.8
```

**시끄러운 게임**:
```
Sound Volume: 0.8 ~ 1.0
```

---

## 🔧 고급 기능

### 1. 애니메이션 비활성화

게임이 느린 디바이스에서는 애니메이션 끄기:
```
Enable Animations: ❌ 체크 해제
```

### 2. 스케일만 비활성화

페이드만 사용하고 스케일 비활성화:
```
Enable Scale Animation: ❌ 체크 해제
```

### 3. 사운드만 비활성화

무음 모드 또는 사운드 파일이 없을 때:
```
Enable Sounds: ❌ 체크 해제
```

### 4. 커스텀 애니메이션 곡선

1. Fade In Curve 클릭
2. Curve Editor에서 편집:
   - 우클릭 → Add Key: 키 추가
   - 키 드래그: 위치 조정
   - 키 핸들: 곡선 모양 조정

**추천 곡선**:
- 자연스러운 페이드: Ease In-Out
- 빠른 등장: Ease Out
- 부드러운 사라짐: Ease In

---

## 🎭 애니메이션 프리셋

### 프리셋 1: 클래식 (권장)
```
Fade In Duration: 0.2
Fade Out Duration: 0.15
Enable Scale Animation: ✅
Start Scale: (0.85, 0.85, 1)
Scale Duration: 0.25
Curve: EaseInOut
```

### 프리셋 2: 빠른 반응
```
Fade In Duration: 0.1
Fade Out Duration: 0.1
Enable Scale Animation: ✅
Start Scale: (0.9, 0.9, 1)
Scale Duration: 0.15
Curve: Linear
```

### 프리셋 3: 고급스러운
```
Fade In Duration: 0.3
Fade Out Duration: 0.25
Enable Scale Animation: ✅
Start Scale: (0.8, 0.8, 1)
Scale Duration: 0.35
Curve: EaseInOut
```

### 프리셋 4: 튕기는 효과
```
Fade In Duration: 0.2
Enable Scale Animation: ✅
Start Scale: (0.7, 0.7, 1)
Scale Duration: 0.4
Curve: Bounce (Overshoot)
```

---

## 🐛 트러블슈팅

### 애니메이션이 작동하지 않을 때

**1. CanvasGroup 확인**
```
PauseMenuPanel에 Canvas Group 컴포넌트가 있는지 확인
```

**2. Enable Animations 체크**
```
Inspector에서 Enable Animations가 체크되어 있는지 확인
```

**3. Duration 값 확인**
```
Duration 값이 0보다 큰지 확인 (최소 0.1 권장)
```

### 사운드가 재생되지 않을 때

**1. Enable Sounds 체크**
```
Inspector에서 Enable Sounds가 체크되어 있는지 확인
```

**2. 오디오 클립 할당**
```
Pause Sound, Resume Sound 등이 할당되어 있는지 확인
```

**3. 볼륨 확인**
```
Sound Volume이 0이 아닌지 확인 (권장: 0.7)
```

**4. Audio Source 확인**
```
GameObject에 Audio Source가 자동 생성되었는지 확인
```

### 버튼 호버 사운드가 작동하지 않을 때

**EventTrigger 자동 추가 확인**
- 스크립트가 자동으로 EventTrigger를 추가함
- 버튼에 EventTrigger 컴포넌트가 있는지 확인

### 애니메이션이 너무 빠르거나 느릴 때

**Duration 조정**:
```
빠름: 0.1 ~ 0.15초
보통: 0.2 ~ 0.25초
느림: 0.3 ~ 0.4초
```

---

## 📊 성능 고려사항

### 최적화 팁

**1. 모바일 디바이스**
- Fade In/Out Duration: 0.1초
- Scale Animation: 비활성화 고려
- 사운드: 압축된 오디오 사용

**2. 저사양 PC**
- Enable Animations: 비활성화
- 즉시 표시/숨김 사용

**3. 고사양 PC/콘솔**
- 모든 애니메이션 활성화
- Duration: 0.3초까지 가능
- 복잡한 곡선 사용 가능

---

## ✅ 테스트 체크리스트

### 기본 기능
- [ ] ESC 키로 일시정지 메뉴 열림
- [ ] ESC 키로 일시정지 메뉴 닫힘
- [ ] 재개 버튼 작동
- [ ] 재시작 버튼 작동
- [ ] 메인 메뉴 버튼 작동

### 애니메이션
- [ ] 페이드 인 애니메이션 부드러움
- [ ] 페이드 아웃 애니메이션 부드러움
- [ ] 스케일 애니메이션 자연스러움
- [ ] Time.timeScale=0에서도 애니메이션 작동

### 사운드
- [ ] 일시정지 사운드 재생
- [ ] 재개 사운드 재생
- [ ] 버튼 클릭 사운드 재생
- [ ] 버튼 호버 사운드 재생 (마우스 오버 시)

### 통합
- [ ] GameManager와 정상 연동
- [ ] 게임 시작 후에만 ESC 활성화
- [ ] 게임 종료 시 ESC 비활성화
- [ ] 오디오 자동 일시정지/재개

---

## 🎯 마이그레이션 가이드

### 기존 PauseMenuUI에서 업그레이드

**1단계**: CanvasGroup 추가
```
PauseMenuPanel 선택 → Add Component → Canvas Group
```

**2단계**: 스크립트 교체
```
PauseMenuUI 제거 → PauseMenuUIAdvanced 추가
```

**3단계**: 참조 재연결
```
모든 UI 참조 다시 드래그
```

**4단계**: 애니메이션 설정
```
Enable Animations 체크
Duration 값 설정 (권장: 0.2, 0.15)
```

**5단계**: 테스트
```
Play 버튼 클릭 → ESC 키 테스트
```

---

**작성일**: 2025-10-26  
**버전**: 2.0 (Advanced)
