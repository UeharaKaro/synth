# StreamingAssets 폴더 구조

이 폴더는 게임 런타임에 동적으로 로드되는 에셋들을 저장합니다.

## 📁 폴더 구조

```
StreamingAssets/
├── Audio/
│   ├── BGM/              # 배경음악 파일
│   │   └── *.wav         # 차트별 BGM (예: sample_audio.wav)
│   └── KeySounds/        # 키사운드 파일
│       ├── Kick.wav
│       ├── Snare.wav
│       ├── Hihat.wav
│       ├── Vocal1.wav
│       ├── Vocal2.wav
│       ├── Synth1.wav
│       ├── Synth2.wav
│       ├── Bass.wav
│       ├── Piano.wav
│       └── Guitar.wav
├── Charts/               # 차트 JSON 파일
│   └── *.json            # 예: SampleSong_Normal_4K.json
└── CoverArt/             # 커버 이미지 (앨범 아트)
    └── *.png/jpg         # 예: sample_audio.png
```

## 🎵 오디오 파일 형식

### BGM (배경음악)
- **위치**: `StreamingAssets/Audio/BGM/`
- **형식**: WAV (권장), MP3, OGG
- **설정**:
  - 샘플레이트: 44100 Hz 이상
  - 비트레이트: 320kbps 이상 (고품질)
  - 채널: Stereo
- **네이밍**: 차트의 `audioFileName` 필드와 일치해야 함
  - 예: `ChartData.audioFileName = "sample_audio.wav"` 
  - → `StreamingAssets/Audio/BGM/sample_audio.wav`

### 효과음 (SFX)
- **위치**: `StreamingAssets/Audio/`
- **파일**:
  - `Metronome.wav` - 메트로놈 사운드
  - `Hit.wav` - 노트 히트 효과음
  - `Miss.wav` - 미스 효과음

### 키사운드 (Key Sounds)
- **위치**: `StreamingAssets/Audio/KeySounds/`
- **형식**: WAV (권장)
- **타입**: 
  - `None` - 사운드 없음
  - `Kick.wav` - 킥 드럼
  - `Snare.wav` - 스네어 드럼
  - `Hihat.wav` - 하이햇
  - `Vocal1.wav`, `Vocal2.wav` - 보컬 샘플
  - `Synth1.wav`, `Synth2.wav` - 신스 사운드
  - `Bass.wav` - 베이스
  - `Piano.wav` - 피아노
  - `Guitar.wav` - 기타

### 커버 이미지 (Cover Art / 앨범 아트)
- **위치**: `StreamingAssets/CoverArt/`
- **형식**: PNG (권장), JPG
- **권장 크기**: 
  - 최소: 512x512px
  - 권장: 1024x1024px
  - 최대: 2048x2048px (메모리 주의)
- **네이밍**: 오디오 파일명과 동일하게
  - 예: `sample_audio.wav` → `sample_audio.png`
  - 또는 차트에서 명시: `coverImageFileName: "my_custom_cover.png"`

## 📄 차트 파일 형식

### 파일 위치
- **위치**: `StreamingAssets/Charts/`
- **형식**: JSON
- **네이밍 규칙**: `{SongName}_{Difficulty}_{KeyCount}K.json`
  - 예: `Synthesis_Hard_6K.json`

### JSON 구조 예시
```json
{
  "songName": "Sample Song",
  "artistName": "Sample Artist",
  "audioFileName": "sample_audio.wav",
  "coverImageFileName": "sample_audio.png",
  "bpm": 120.0,
  "offset": 0.0,
  "difficulty": "Normal",
  "keyCount": 4,
  "level": 5,
  "notes": [
    {
      "timing": 2.0,
      "track": 0,
      "keySoundType": 1,
      "isLongNote": false,
      "longNoteEndTiming": 0.0
    }
  ]
}
```

## 🚀 테스트용 샘플 차트

게임은 오디오 파일이 없어도 테스트 가능합니다:
- `GameManager.useSampleChart = true` 설정 시 샘플 차트 자동 생성
- 단, 오디오 재생은 실제 파일이 필요합니다

### 최소 테스트 구성
1. BGM 하나만 추가: `StreamingAssets/Audio/BGM/sample_audio.wav`
2. 샘플 차트 사용: Unity에서 GameManager의 `useSampleChart = true`

## ⚙️ 개발 중 주의사항

### ChartLoader 설정
```csharp
// ChartLoader에서 StreamingAssets 사용 설정
[SerializeField] private bool useStreamingAssets = true; // StreamingAssets 폴더 사용
```

### AudioManager 경로
- AudioManager는 자동으로 `Application.streamingAssetsPath + "/Audio/"` 경로 사용
- 빌드 후에도 동일한 경로 유지

### Unity 에디터에서 테스트
- 파일 추가 후 Unity를 재시작하거나 Reimport 필요
- `Assets → Refresh` 또는 `Ctrl+R`

## 📦 빌드 시 포함 여부

StreamingAssets 폴더의 모든 파일은:
- ✅ 빌드에 자동으로 포함됨
- ✅ 압축되지 않고 그대로 복사됨
- ✅ 런타임에 동적 로딩 가능
- ❌ Unity의 Import Settings 적용 안됨 (원본 그대로)

## 🔧 문제 해결

### 오디오 파일이 로드되지 않을 때
1. 파일 경로 확인: `StreamingAssets/Audio/BGM/{fileName}`
2. 파일 이름 대소문자 확인 (case-sensitive)
3. 지원되는 형식인지 확인 (WAV, MP3, OGG)
4. Unity 콘솔에서 FMOD 에러 메시지 확인

### 차트 파일이 로드되지 않을 때
1. JSON 형식 유효성 검증 (온라인 JSON Validator 사용)
2. 파일 인코딩 확인 (UTF-8 권장)
3. ChartLoader의 `useStreamingAssets` 설정 확인

---

**마지막 업데이트**: 2025-10-26  
**작성**: 오디오 시스템 통합 작업 중
