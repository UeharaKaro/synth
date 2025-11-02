# 암호화 시스템 동작 명세

**날짜**: 2025-01-29  
**버전**: 1.1 (수정됨)

---

## 📋 암호화 동작

### 단일 파일 암호화
**메뉴**: `Assets → Encryption → Encrypt Selected File (AES-256)`

**동작**:
1. 원본 파일 읽기 (예: `song.wav`)
2. AES-256 암호화 수행
3. 암호화된 파일 저장 (예: `song.eaw`)
4. **원본 파일 삭제** ⚠️

**결과**:
```
Before: song.wav (10 MB)
After:  song.eaw (10 MB + 헤더)
        song.wav (삭제됨) ❌
```

**주의**: 원본 파일이 삭제되므로 **백업 필수!**

---

### 여러 파일 동시 암호화
**메뉴**: `Assets → Encryption → Encrypt Multiple Files (AES-256)`

**동작**: 단일 파일 암호화와 동일 (여러 파일 반복)

---

### 전체 폴더 암호화
**메뉴**: `Assets → Encryption → Encrypt StreamingAssets Folder`

**동작**:
1. StreamingAssets 스캔 (*.wav, *.ogg, *.mp3, *.png, *.jpg)
2. `.backup` 폴더 생성
3. 각 파일마다:
   - 원본을 `.backup` 폴더에 **백업**
   - 암호화 수행
   - `.eaw` 파일 생성
   - 원본 파일 삭제

**결과**:
```
StreamingAssets/
├── Audio/
│   └── BGM/
│       ├── song.eaw       (암호화됨)
│       └── song.wav       (삭제됨) ❌
├── CoverArt/
│   ├── cover.eaw          (암호화됨)
│   └── cover.png          (삭제됨) ❌
└── .backup/               (백업 폴더)
    ├── song.wav           (백업됨) ✅
    └── cover.png          (백업됨) ✅
```

**안전성**: `.backup` 폴더에 원본 보관

---

## 🔓 복호화 동작

### 파일 복호화
**메뉴**: `Assets → Encryption → Decrypt Selected File`

**동작**:
1. `.eaw` 파일 읽기
2. AES-256 복호화 수행
3. 파일 시그니처 분석 (자동 확장자 감지)
4. 원본 확장자로 저장

**파일 시그니처 감지**:
- WAV: `RIFF` → `.wav`
- PNG: `89 50 4E 47` → `.png`
- JPEG: `FF D8 FF` → `.jpg`
- OGG: `OggS` → `.ogg`
- 알 수 없음: `.decrypted`

**결과**:
```
Before: song.eaw
After:  song.eaw (유지됨)
        song.wav (복원됨) ✅
```

**주의**: 
- ✅ **원본 확장자로 자동 복원**
- ✅ `.eaw` 파일 유지 (삭제 안 됨)
- ❌ `.decrypted` 확장자 사용 안 함 (시그니처 감지 실패 시만)

---

## 🔄 복원 시스템

### 백업에서 복원
**메뉴**: `Tools → Encryption → Restore Original Files from Backup`

**동작**:
1. `.backup` 폴더 확인
2. 각 백업 파일마다:
   - 원본 위치로 복사
   - 대응하는 `.eaw` 파일 삭제

**결과**:
```
Before:
StreamingAssets/
├── Audio/BGM/song.eaw
└── .backup/song.wav

After:
StreamingAssets/
├── Audio/BGM/song.wav (복원됨) ✅
└── .backup/song.wav   (유지됨)
```

---

## ⚠️ 주의사항

### 1. 원본 파일 보호
```
⚠️ 암호화 시 원본 파일이 삭제됩니다!

안전한 사용 방법:
1. 전체 폴더 암호화 사용 (자동 백업)
2. 또는 수동 백업 후 단일 파일 암호화
3. Git/버전 관리 시스템 사용
```

### 2. 복호화 확장자
```
✅ 자동 감지: .wav, .png, .jpg, .ogg
❌ 수동 지정: .decrypted (감지 실패 시)

복호화된 파일이 .decrypted인 경우:
→ 파일 시그니처 확인 필요
→ 수동으로 확장자 변경
```

### 3. 백업 폴더 관리
```
.backup 폴더는:
✅ 개발 중: 유지 (원본 복원용)
❌ 배포 시: 삭제 (보안상 불필요)

삭제 방법:
Tools → Encryption → Delete Backup Folder
```

---

## 📊 파일 흐름도

### 암호화 흐름
```
[원본 파일]
    ↓
[AES-256 암호화]
    ↓
[.eaw 파일 생성]
    ↓
[원본 파일 삭제] ⚠️
```

### 전체 폴더 암호화 흐름
```
[원본 파일]
    ↓
[.backup 폴더로 백업] ✅
    ↓
[AES-256 암호화]
    ↓
[.eaw 파일 생성]
    ↓
[원본 파일 삭제]
```

### 복호화 흐름
```
[.eaw 파일]
    ↓
[AES-256 복호화]
    ↓
[파일 시그니처 감지]
    ↓
[원본 확장자로 저장] (.wav, .png, etc.)
    ↓
[.eaw 파일 유지]
```

---

## 🧪 테스트 시나리오

### Scenario 1: 단일 파일 암호화
```
1. song.wav 선택
2. 암호화 실행
3. ✅ song.eaw 생성
4. ❌ song.wav 삭제됨
5. ⚠️ 백업 없음!
```

### Scenario 2: 전체 폴더 암호화 (권장)
```
1. "Encrypt StreamingAssets Folder" 실행
2. ✅ .backup 폴더 생성
3. ✅ 모든 파일 백업
4. ✅ 모든 파일 암호화
5. ✅ 원본 파일 삭제
6. ✅ 안전하게 복원 가능
```

### Scenario 3: 복호화 및 확인
```
1. song.eaw 선택
2. 복호화 실행
3. ✅ song.wav 생성 (자동 확장자)
4. ✅ song.eaw 유지
5. ✅ 파일 비교로 무결성 확인
```

---

## 💡 Best Practices

### 개발 환경
```
1. 전체 폴더 암호화 사용 (자동 백업)
2. .backup 폴더 유지
3. Git에 .backup 폴더 커밋 (선택)
```

### 배포 빌드
```
1. 빌드 전 자동 암호화 (BuildPreprocessor)
2. 빌드 후 원본 복원 (개발 계속)
3. 최종 배포 시 .backup 폴더 제거
```

### 백업 전략
```
Option 1: Git 사용
- 원본 파일 커밋
- .eaw 파일은 .gitignore

Option 2: .backup 폴더 사용
- 자동 백업 활용
- 배포 전 삭제

Option 3: 외부 백업
- 별도 폴더에 원본 보관
- 암호화 전 수동 백업
```

---

## 🔧 문제 해결

### 문제: 원본 파일이 사라짐
**원인**: 암호화는 원본을 삭제함
**해결**:
1. `.backup` 폴더 확인
2. `Tools → Encryption → Restore Original Files from Backup`
3. 또는 Git에서 복원

### 문제: .decrypted 파일 생성
**원인**: 파일 시그니처 감지 실패
**해결**:
1. 파일 헤더 확인
2. 수동으로 확장자 변경
3. 원본 파일 타입 확인

### 문제: 백업 폴더 없음
**원인**: 단일 파일 암호화 사용
**해결**:
1. Git에서 복원
2. 또는 외부 백업 사용
3. 앞으로 전체 폴더 암호화 사용

---

## ✅ 변경 사항 (v1.1)

### 수정 내용
- ✅ 암호화 시 원본 파일 삭제 (보안 강화)
- ✅ 복호화 시 원본 확장자 자동 감지
- ✅ `.decrypted` 확장자 제거 (시그니처 감지 실패 시만 사용)
- ✅ 파일 시그니처 감지 함수 추가 (`GetOriginalExtension`)

### 이전 버전 (v1.0)
- ❌ 암호화 시 원본 유지 (보안 취약)
- ❌ 복호화 시 항상 `.decrypted` 확장자

---

**작성자**: Synth 개발팀  
**최종 수정**: 2025-01-29
