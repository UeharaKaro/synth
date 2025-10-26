# NoteManager 추가 설정 가이드

**날짜**: 2025-10-26  
**목적**: GameScene에 NoteManager 추가 및 설정  
**예상 시간**: 10분

---

## 📋 목차

1. [Note Prefab 자동 생성](#1-note-prefab-자동-생성)
2. [NoteManager GameObject 생성](#2-notemanager-gameobject-생성)
3. [NoteManager 설정](#3-notemanager-설정)
4. [GameManager 연결](#4-gamemanager-연결)
5. [테스트](#5-테스트)
6. [문제 해결](#6-문제-해결)

---

## 1. Note Prefab 자동 생성

### 방법 A: 자동 생성 도구 사용 (추천 ⭐)

**Editor 스크립트 생성됨**: `Assets/Editor/NotePrefabCreator.cs`

#### 1단계: Unity Editor 대기
```
Unity 하단 상태바:
"Compiling..." → "Compilation Complete"
```

#### 2단계: 자동 생성 실행
```
Unity 상단 메뉴:
Tools → Create Note Prefab 클릭

결과:
✅ Assets/Prefabs/Note.prefab 생성됨
✅ Project 창에서 자동 선택됨
✅ Console에 "✅ Note Prefab 생성 완료" 메시지
```

#### 3단계 (선택): Long Note Prefab 생성
```
Unity 상단 메뉴:
Tools → Create Long Note Prefab 클릭

결과:
✅ Assets/Prefabs/LongNote.prefab 생성됨
```

---

### 방법 B: 수동 생성

만약 자동 도구가 작동하지 않으면:

#### 1단계: GameObject 생성
```
Hierarchy 우클릭 → Create Empty
이름: "Note"
```

#### 2단계: 컴포넌트 추가
```
Inspector:
1. Add Component → Sprite Renderer
   - Color: 하늘색 (R:0.2, G:0.8, B:1.0)
   - Sorting Order: 10

2. Add Component → Box Collider 2D
   - Size: (1, 1)
   - Is Trigger: ✅

3. Add Component → NoteController

Transform:
- Scale: (0.9, 0.2, 1)
```

#### 3단계: Prefab으로 저장
```
1. Project 창에서 Assets 우클릭 → Create → Folder
   이름: "Prefabs"

2. Hierarchy에서 Note를 Prefabs 폴더로 드래그

3. Hierarchy에서 Note GameObject 삭제
```

---

## 2. NoteManager GameObject 생성

### 1단계: GameObject 생성
```
GameScene Hierarchy에서:
1. 우클릭 → Create Empty
2. 이름: "NoteManager"
3. Position: (0, 0, 0)
```

### 2단계: NoteManager 컴포넌트 추가
```
Inspector:
1. Add Component 클릭
2. "NoteManager" 검색
3. NoteManager (Script) 선택
```

### 3단계: Hierarchy 정리
```
최종 구조:
GameScene
├── Main Camera
├── EventSystem
├── GameManager
├── GameSettingManager
├── AudioManager
├── RhythmManager
├── ChartLoader
├── GearController
├── NoteSpawner
├── NoteManager ✨ 신규
├── InputManager
├── HPSystem
└── Canvas
```

---

## 3. NoteManager 설정

### Inspector 설정

```
NoteManager (Script)
├── [References]
│   ├── Gear Controller: [GearController 드래그] ✅
│   ├── Settings: [GearSettings 드래그] (선택사항)
│
├── [Note Prefab]
│   └── Note Prefab: [Assets/Prefabs/Note.prefab 드래그] ✅
│
└── [Note Settings]
    ├── Note Speed: 5
    └── Spawn Height: 10
```

### 설정 체크리스트
```
[ ] Gear Controller 연결
    → Hierarchy에서 GearController를 드래그

[ ] Note Prefab 연결
    → Project 창 Prefabs/Note.prefab을 드래그

[ ] Note Speed 확인 (기본값: 5)

[ ] Spawn Height 확인 (기본값: 10)
```

---

## 4. GameManager 연결

### Inspector 설정

```
GameManager 선택 → Inspector:

[시스템 참조]
├── Chart Loader: [ChartLoader]
├── Note Spawner: [NoteSpawner]
├── Note Manager: [NoteManager] ✨ 드래그
├── Audio Manager: [AudioManager]
├── HP System: [HPSystem]
├── Rhythm Manager: [RhythmManager]
├── Gear Controller: [GearController]
└── Input Manager: [InputManager]

[UI 참조]
├── Progress Display: [ProgressDisplay]
├── Score Display: [ScoreDisplay]
├── Combo Judgment Display: [ComboJudgmentDisplay]
├── Judgment Offset Display: [JudgmentOffsetDisplay]
└── Pause Menu UI: [PauseMenuUI]

[게임 상태]
├── ☐ Auto Start (false)
├── ☑ Use Sample Chart (true)
└── ☐ Use Note Spawner (false) ✨ 체크 해제!
```

### ⚠️ 중요: Use Note Spawner 설정
```
Use Note Spawner:
├── ☑ true  → NoteSpawner 사용
└── ☐ false → NoteManager 사용 ✨

NoteManager를 사용하려면:
Use Note Spawner를 false로 설정!
```

---

## 5. 테스트

### 테스트 체크리스트

```
[ ] 1. 씬 저장
    Ctrl+S (File → Save Scene)

[ ] 2. Console 창 열기
    Ctrl+Shift+C (Window → General → Console)

[ ] 3. Play 버튼 클릭
    Unity 상단 ▶ 버튼

[ ] 4. Console 메시지 확인
    정상:
    ✅ "NoteManager: 초기화 완료"
    ✅ "GameManager: 게임 시작"
    ✅ "NoteManager: 노트 스폰 시작"
    
    오류:
    ❌ "NullReferenceException" → 참조 연결 확인
    ❌ "Note Prefab is null" → Note Prefab 연결 확인

[ ] 5. 화면 확인
    ✅ 노트가 위에서 아래로 떨어짐
    ✅ 진행도 바 업데이트
    ✅ 점수 표시
    ✅ HP 바 작동

[ ] 6. 키 입력 테스트
    A, S, D, F 키 입력
    ✅ 노트 판정 발생
    ✅ 점수 증가
    ✅ 콤보 표시
```

---

## 6. 문제 해결

### 문제 1: "Note Prefab is null"

**원인**: Note Prefab이 연결되지 않음

**해결**:
```
1. NoteManager 선택
2. Inspector에서 Note Prefab 필드 확인
3. None으로 되어있으면:
   → Project 창에서 Prefabs/Note.prefab 찾기
   → Note Prefab 필드로 드래그
```

---

### 문제 2: "NoteController not found"

**원인**: Note Prefab에 NoteController 컴포넌트가 없음

**해결**:
```
1. Project 창에서 Prefabs/Note.prefab 더블클릭
2. Inspector에서 NoteController 컴포넌트 확인
3. 없으면:
   → Add Component → NoteController
   → Ctrl+S (저장)
```

---

### 문제 3: 노트가 스폰되지 않음

**원인**: GameManager 설정 오류

**해결**:
```
GameManager Inspector 확인:
1. Use Note Spawner: ☐ false 확인
2. Note Manager: [NoteManager] 연결 확인
3. Use Sample Chart: ☑ true 확인
```

---

### 문제 4: 노트가 보이지 않음

**원인**: Sprite Renderer 설정 문제

**해결**:
```
1. Play 모드 진입
2. Hierarchy에서 Note(Clone) 찾기
3. Inspector 확인:
   - Sprite Renderer가 있는지
   - Color Alpha가 0이 아닌지 (255)
   - Sprite가 할당되어 있는지
```

---

### 문제 5: 노트가 너무 빠르거나 느림

**해결**:
```
NoteManager Inspector:
- Note Speed 조정
  - 너무 빠르면: 3~4
  - 보통: 5 (기본값)
  - 느리면: 6~8
```

---

## 📊 NoteSpawner vs NoteManager 비교

### 현재 설정 (NoteManager 사용)

```
✅ 장점:
- 오브젝트 풀링으로 메모리 효율적
- 고성능 (많은 노트 처리)
- 노트 재사용으로 GC 부하 감소

⚠️ 단점:
- Note Prefab 필수
- 설정이 조금 복잡
- 초기 풀 생성 시간 필요
```

### 이전 설정 (NoteSpawner 사용)

```
✅ 장점:
- 설정 간단
- Prefab 불필요
- 즉시 테스트 가능

⚠️ 단점:
- 노트 생성/삭제 오버헤드
- 메모리 효율 낮음
- GC 발생 가능
```

---

## 🎯 다음 단계

NoteManager 설정이 완료되면:

### 1. 성능 비교 테스트
```
1. NoteManager 사용 (현재)
   - Play 모드 → FPS 확인
   
2. NoteSpawner 사용
   - Use Note Spawner: true
   - Play 모드 → FPS 확인
   
3. 더 나은 쪽 선택
```

### 2. 고급 설정
```
[ ] Long Note Prefab 추가
[ ] Note Pool Size 조정
[ ] 키 사운드 통합
[ ] 노트 이펙트 추가
```

### 3. 차트 에디터 테스트
```
[ ] 에디터에서 차트 생성
[ ] NoteManager로 재생
[ ] 정상 동작 확인
```

---

## ✅ 완료 체크리스트

모든 단계를 완료했는지 확인하세요:

```
[ ] Assets/Editor/NotePrefabCreator.cs 생성
[ ] Tools → Create Note Prefab 실행
[ ] Assets/Prefabs/Note.prefab 생성 확인
[ ] NoteManager GameObject 생성
[ ] NoteManager 컴포넌트 추가
[ ] Note Prefab 연결
[ ] Gear Controller 연결
[ ] GameManager에 NoteManager 연결
[ ] Use Note Spawner: false 설정
[ ] 씬 저장 (Ctrl+S)
[ ] Play 모드 테스트 성공
[ ] 노트 스폰 확인
[ ] 키 입력 판정 확인
```

---

## 🎉 완료!

NoteManager 설정이 완료되었습니다!

**이제 다음을 확인하세요**:
- ✅ 노트가 정상적으로 스폰됨
- ✅ 오브젝트 풀링으로 효율적 동작
- ✅ 메모리 사용량 감소
- ✅ FPS 안정화

**다음 작업 추천**:
1. 옵션 메뉴 UI 구현 (4시간)
2. 보안 시스템 구현 (10시간)
3. 차트 에디터 완성 (8시간)

---

**작성**: Claude Code  
**버전**: 1.0  
**상태**: 완료 ✅
