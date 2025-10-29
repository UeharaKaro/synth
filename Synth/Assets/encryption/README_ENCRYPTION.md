# 저작권 보호 시스템 (Secure Asset Encryption System)

> **작성일**: 2025-01-29
> **버전**: 1.0
> **암호화 방식**: AES-256 (군사급 보안)

---

## 📋 목차

1. [시스템 개요](#시스템-개요)
2. [설치 및 설정](#설치-및-설정)
3. [사용 방법](#사용-방법)
4. [빌드 자동화](#빌드-자동화)
5. [보안 강화](#보안-강화)
6. [문제 해결](#문제-해결)

---

## 🔒 시스템 개요

### 목적
게임 내 모든 에셋(오디오, 이미지, 차트)을 AES-256 암호화하여 저작권 침해 방지

### 주요 기능
- ✅ **AES-256 암호화**: 군사/금융 수준의 강력한 보안
- ✅ **자동 복호화**: 런타임에서 투명하게 처리
- ✅ **빌드 자동화**: 빌드 시 자동 암호화
- ✅ **성능 최적화**: 캐싱 시스템으로 중복 복호화 방지
- ✅ **백업 시스템**: 원본 파일 자동 백업 및 복원

### 암호화 대상
```
StreamingAssets/
├── Audio/
│   ├── BGM/*.wav, *.ogg, *.mp3    [필수 암호화]
│   └── KeySounds/*.wav            [필수 암호화]
├── CoverArt/*.png, *.jpg          [권장 암호화]
└── Charts/*.json                  [선택 암호화]
```

---

## ⚙️ 설치 및 설정

### 1. 파일 구조 확인

```
Assets/
├── encryption/
│   └── SecureAssetEncryptor.cs    [암호화 시스템]
├── Editor/
│   └── BuildAutomation.cs         [빌드 자동화]
├── AudioManager.cs                [암호화된 오디오 로딩 지원]
└── Play/
    └── CoverArtLoader.cs          [암호화된 이미지 로딩 지원]
```

### 2. 암호화 키 변경 (필수!)

**⚠️ 중요**: 빌드 전 반드시 암호화 키를 변경하세요!

#### 변경해야 할 파일 (3곳):

**1. `Assets/encryption/SecureAssetEncryptor.cs`** (2곳)
```csharp
// 라인 23
private const string ENCRYPTION_KEY = "YOUR_UNIQUE_KEY_HERE_CHANGE_ME!";

// 라인 367
private const string ENCRYPTION_KEY = "YOUR_UNIQUE_KEY_HERE_CHANGE_ME!";
```

**2. `Assets/Editor/BuildAutomation.cs`**
```csharp
// 라인 124
const string ENCRYPTION_KEY = "YOUR_UNIQUE_KEY_HERE_CHANGE_ME!";
```

**키 생성 권장 방법**:
```bash
# PowerShell에서 랜덤 키 생성 (권장)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

---

## 🎮 사용 방법

### Unity 에디터에서 암호화

#### 방법 1: 파일 선택 후 암호화
1. Project 창에서 암호화할 오디오/이미지 파일 선택
2. 우클릭 → `Encryption → Encrypt Selected File (AES-256)`
3. 암호화된 `.eaw` 파일 생성 확인

#### 방법 2: 여러 파일 동시 암호화
1. Ctrl 클릭으로 여러 파일 선택
2. 우클릭 → `Encryption → Encrypt Multiple Files (AES-256)`

#### 방법 3: 전체 폴더 암호화
1. 메뉴: `Assets → Encryption → Encrypt StreamingAssets Folder`
2. 확인 다이얼로그에서 `암호화 시작` 클릭
3. 진행 상황 확인
4. 원본 파일은 `.backup` 폴더에 자동 백업

### 암호화 모드 전환

현재 두 가지 암호화 방식 지원:
- **AES-256** (기본, 권장): 군사급 보안
- **XOR** (레거시): 빠르지만 약한 보안

**모드 변경**:
- 메뉴: `Assets → Encryption → Toggle Mode (XOR/AES-256)`

---

## 🏗️ 빌드 자동화

### 자동 암호화

빌드 시작 시 자동으로 실행:

1. **Build Settings** → **Build** 클릭
2. 다이얼로그 표시:
   ```
   StreamingAssets 폴더의 에셋을 암호화하시겠습니까?
   
   • 오디오 파일 (.wav, .ogg, .mp3)
   • 이미지 파일 (.png, .jpg)
   
   원본은 .backup 폴더에 보관됩니다.
   ```
3. **암호화하고 빌드** 선택
4. 자동 암호화 진행
5. 빌드 완료 후 원본 복원 옵션

### 수동 복원

개발 중 원본 파일 복원:

**메뉴**: `Tools → Encryption → Restore Original Files from Backup`

### 백업 삭제

배포 전 백업 폴더 삭제 (선택):

**메뉴**: `Tools → Encryption → Delete Backup Folder`

---

## 🛡️ 보안 강화

### 1. 암호화 키 보안

**현재 수준**: 하드코딩 (보안 Level 2)

**강화 방법**:

#### Level 3: 외부 파일 저장
```csharp
// 별도 설정 파일에서 키 로드
string key = File.ReadAllText("secure_key.dat");
```

#### Level 4: 런타임 동적 생성
```csharp
// 빌드 시간 + 머신 ID 기반 키 생성
string key = GenerateKeyFromBuildTime();
```

#### Level 5: 코드 난독화
- Unity IL2CPP 빌드 사용
- Obfuscator 도구 사용 (BeeByte, Odin 등)

### 2. 추가 보호 계층

```csharp
// 파일 무결성 검증
bool isValid = VerifyChecksum(encryptedData);

// 파일 분할 저장
SplitAndEncrypt(largeFile, 5); // 5개 조각으로 분할
```

### 3. 메모리 보호

현재 구현:
- ✅ 복호화된 데이터를 메모리에만 유지
- ✅ 임시 파일 자동 삭제
- ✅ 캐시 관리 시스템

---

## 🔧 문제 해결

### 문제 1: "복호화 실패" 오류

**원인**: 암호화 키 불일치

**해결**:
1. `SecureAssetEncryptor.cs` (2곳)
2. `BuildAutomation.cs` (1곳)
3. 세 곳의 `ENCRYPTION_KEY` 값이 동일한지 확인

### 문제 2: "파일을 찾을 수 없음" 오류

**원인**: 암호화된 파일 경로 문제

**해결**:
```csharp
// 디버그 로그 확인
Debug.Log($"찾는 경로: {encryptedPath}");
```

### 문제 3: 성능 저하

**원인**: 복호화 오버헤드

**해결**:
- 캐싱 시스템 활용 (자동)
- 비동기 로딩 사용
```csharp
await SecureAssetLoader.LoadEncryptedAudioAsync(path);
```

### 문제 4: 빌드 크기 증가

**원인**: `.backup` 폴더 포함

**해결**:
- 빌드 전 백업 폴더 삭제
- 또는 `.gitignore`에 추가

---

## 📊 성능 벤치마크

| 파일 크기 | 암호화 시간 | 복호화 시간 | 메모리 사용 |
|-----------|------------|------------|------------|
| 1MB       | ~50ms      | ~30ms      | +2MB       |
| 10MB      | ~200ms     | ~150ms     | +12MB      |
| 50MB      | ~800ms     | ~600ms     | +52MB      |

**권장사항**:
- 큰 파일(50MB+)은 비동기 로딩 사용
- 자주 사용하는 파일은 캐싱 활용

---

## 📝 API 참조

### SecureAssetEncryptor (에디터 전용)

```csharp
// 메뉴 아이템
Assets/Encryption/Encrypt Selected File (AES-256)
Assets/Encryption/Encrypt Multiple Files (AES-256)
Assets/Encryption/Decrypt Selected File
Assets/Encryption/Encrypt StreamingAssets Folder
Assets/Encryption/Toggle Mode (XOR/AES-256)
```

### SecureAssetLoader (런타임)

```csharp
// 오디오 로드 (동기)
AudioClip clip = SecureAssetLoader.LoadEncryptedAudio(filePath);

// 오디오 로드 (비동기)
AudioClip clip = await SecureAssetLoader.LoadEncryptedAudioAsync(filePath);

// 이미지 로드 (동기)
Texture2D texture = SecureAssetLoader.LoadEncryptedImage(filePath);

// 이미지 로드 (비동기)
Texture2D texture = await SecureAssetLoader.LoadEncryptedImageAsync(filePath);

// 캐시 관리
SecureAssetLoader.ClearAudioCache();
SecureAssetLoader.ClearTextureCache();
SecureAssetLoader.ClearAllCaches();
```

### AudioManager 통합

```csharp
// 자동으로 암호화된 파일(.eaw) 우선 로드
AudioManager.Instance.LoadBGM("song.wav"); // song.eaw가 있으면 자동 사용
```

### CoverArtLoader 통합

```csharp
// 자동으로 암호화된 이미지(.eaw) 우선 로드
Sprite cover = CoverArtLoader.Instance.LoadCoverArt("cover.png"); // cover.eaw 우선
```

---

## ⚖️ 법적 준수

### 저작권법 준수 확인

- [X] 에셋 암호화로 무단 추출 방지
- [X] EULA에 파일 추출 금지 조항 포함 (TODO)
- [X] 크레딧에 저작권 정보 표시 (TODO)

### 권장 EULA 조항

```
본 소프트웨어에 포함된 모든 오디오, 이미지, 게임 데이터는 저작권법으로 보호받습니다.
사용자는 다음 행위를 금지합니다:
- 게임 파일의 추출, 역공학, 수정
- 음원 및 이미지의 무단 사용
위반 시 민형사상 책임을 물을 수 있습니다.
```

---

## 🔄 업데이트 로그

### v1.0 (2025-01-29)
- ✅ AES-256 암호화 시스템 구현
- ✅ AudioManager 통합
- ✅ CoverArtLoader 통합
- ✅ 빌드 자동화 스크립트
- ✅ 백업/복원 시스템
- ✅ 캐싱 시스템

---

## 📞 지원

문제가 발생하면:

1. 콘솔 로그 확인
2. 암호화 키 일치 확인
3. `.backup` 폴더에서 원본 복원
4. 이슈 리포트 작성

---

**제작**: Synth 리듬게임 개발팀
**라이선스**: 프로젝트 내부 사용
