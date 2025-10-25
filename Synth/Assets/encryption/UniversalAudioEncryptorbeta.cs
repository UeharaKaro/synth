/*
using UnityEditor;
using System.IO; // 파일 입출력 네임스페이스 (File, Path 등)
using System.Text; // 문자열 인코딩(UTF8) 네임스페이스
using UnityEngine;
using System.Threading.Tasks; // 비동기 작업 네임스페이스(Task)
using UnityEngine.Networking; // Unity의 네트워킹(웹 요청) 기능을 위한 네임스페이스
using System.Collections.Generic; // 리스트 등 컬렉션 네임스페이스 추가 (리듬 게임에서 여러 음원 관리용)
using System.Security.Cryptography; // 더 강력한 암호화 (AES) 지원 네임스페이스 추가
using System; // 기본 시스템 네임스페이스 (예외 처리 등)

#region Editor-Only Encryption Tool

/// <summary>
/// Unity 에디터 내에서 에셋(주로 오디오 파일) 암호화 기능 제공 클래스
/// 이 클래스의 코드는 Unity 에디터에서만 실행되며, 최종 게임 빌드에는 포함되지 않음
/// 추가: 여러 파일 선택 지원, AES 암호화 옵션 추가
/// </summary>
public class UniversalAudioEncryptor
{
    // [중요] 암호화 및 복호화에 사용될 비밀 키 (리듬 게임용으로 더 긴 키 추천)
    private const string EncryptionKey = "YourSecretKeyForRhythmGame123!@#";
    private const string MenuItemEncrypt = "Assets/Encrypt Audio File";
    private const string MenuItemEncryptMultiple = "Assets/Encrypt Multiple Audio Files";
    private const string MenuItemDecrypt = "Assets/Decrypt Audio File"; // 복호화 메뉴 추가

    // 암호화 모드 선택 (기본 XOR, 옵션으로 AES)
    public enum EncryptionMode
    {
        XOR,
        AES
    }

    private static EncryptionMode currentMode = EncryptionMode.XOR; // 기본 모드

    [MenuItem(MenuItemEncrypt)]
    private static void EncryptSelectedAudioFile()
    {
        EncryptSelectedAudioFiles(new Object[] { Selection.activeObject });
    }

    [MenuItem(MenuItemEncryptMultiple)]
    private static void EncryptSelectedMultipleAudioFiles()
    {
        EncryptSelectedAudioFiles(Selection.objects);
    }

    private static void EncryptSelectedAudioFiles(Object[] selectedObjects)
    {
        List<string> encryptedPaths = new List<string>();

        foreach (Object selectedObject in selectedObjects)
        {
            if (selectedObject == null || !(selectedObject is AudioClip))
            {
                continue; // 오디오 클립이 아닌 경우 스킵
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            string fullPath = Path.GetFullPath(assetPath);
            byte[] fileBytes = File.ReadAllBytes(fullPath);
            byte[] encryptedBytes = EncryptData(fileBytes, currentMode);
            string newPath = Path.ChangeExtension(assetPath, ".bytes");
            File.WriteAllBytes(newPath, encryptedBytes);
            encryptedPaths.Add(newPath);
        }

        AssetDatabase.Refresh();

        if (encryptedPaths.Count > 0)
        {
            string message = "암호화 완료:\n" + string.Join("\n", encryptedPaths);
            Debug.Log(message);
            EditorUtility.DisplayDialog("성공", message, "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("오류", "암호화할 오디오 파일(.wav, .ogg 등)을 선택해주세요.", "확인");
        }
    }

    [MenuItem(MenuItemDecrypt)]
    private static void DecryptSelectedAudioFile()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null || !(selectedObject is TextAsset))
        {
            EditorUtility.DisplayDialog("오류", "복호화할 .bytes 파일을 선택해주세요.", "확인");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        string fullPath = Path.GetFullPath(assetPath);
        byte[] fileBytes = File.ReadAllBytes(fullPath);
        byte[] decryptedBytes = DecryptData(fileBytes, currentMode);
        string newPath = Path.ChangeExtension(assetPath, ".decrypted" + Path.GetExtension(assetPath).Replace(".bytes", ""));
        File.WriteAllBytes(newPath, decryptedBytes);
        AssetDatabase.Refresh();

        Debug.Log($"복호화 완료: '{assetPath}' -> '{newPath}'");
        EditorUtility.DisplayDialog("성공", $"파일이 성공적으로 복호화되었습니다.\n원본: {assetPath}\n결과: {newPath}", "확인");
    }

    [MenuItem(MenuItemEncrypt, true)]
    [MenuItem(MenuItemEncryptMultiple, true)]
    private static bool ValidateEncryptAudioFile()
    {
        return Selection.activeObject is AudioClip;
    }

    [MenuItem(MenuItemDecrypt, true)]
    private static bool ValidateDecryptAudioFile()
    {
        return Selection.activeObject is TextAsset && AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".bytes");
    }

    // 메뉴 아이템으로 암호화 모드 토글 추가
    [MenuItem("Assets/Toggle Encryption Mode (XOR/AES)")]
    private static void ToggleEncryptionMode()
    {
        currentMode = currentMode == EncryptionMode.XOR ? EncryptionMode.AES : EncryptionMode.XOR;
        Debug.Log($"암호화 모드 변경: {currentMode}");
        EditorUtility.DisplayDialog("모드 변경", $"현재 모드: {currentMode}", "확인");
    }

    private static byte[] EncryptData(byte[] data, EncryptionMode mode)
    {
        if (mode == EncryptionMode.AES)
        {
            return EncryptAES(data);
        }
        else
        {
            return ProcessXOR(data);
        }
    }

    private static byte[] DecryptData(byte[] data, EncryptionMode mode)
    {
        if (mode == EncryptionMode.AES)
        {
            return DecryptAES(data);
        }
        else
        {
            return ProcessXOR(data); // XOR는 대칭적이므로 동일
        }
    }

    private static byte[] ProcessXOR(byte[] data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return result;
    }

    // AES 암호화 추가 (더 안전한 옵션, 리듬 게임의 중요한 음원 보호용)
    private static byte[] EncryptAES(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32, '\0').Substring(0, 32)); // 256-bit 키
            aes.IV = new byte[16]; // 간단 IV (실제로는 랜덤 IV 사용 추천)

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }
    }

    private static byte[] DecryptAES(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32, '\0').Substring(0, 32));
            aes.IV = new byte[16];

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(data))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var resultMs = new MemoryStream())
            {
                cs.CopyTo(resultMs);
                return resultMs.ToArray();
            }
        }
    }
}

#endregion

#region Runtime Loader

/// <summary>
/// 게임 실행 중(런타임)에 암호화된 오디오 파일을 불러와서(복호화) 재생 가능한 형태로 만드는 헬퍼 클래스
/// 이 클래스는 최종 게임 빌드에 포함됨
/// 추가: AES 지원, 여러 클립 로드 캐싱 기능 (리듬 게임에서 여러 곡 로드 최적화)
/// </summary>
public static class RuntimeAudioLoader
{
    // [중요] 에디터의 UniversalAudioEncryptor 클래스에 정의된 EncryptionKey와 반드시 동일한 값이어야 함
    private const string EncryptionKey = "YourSecretKeyForRhythmGame123!@#";

    // 캐싱 딕셔너리: 이미 로드된 클립 재사용 (리듬 게임에서 반복 로드 방지)
    private static Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    // 암호화 모드 (런타임에서 에디터 모드와 동기화 필요, 기본 XOR)
    public enum EncryptionMode
    {
        XOR,
        AES
    }

    private static EncryptionMode currentMode = EncryptionMode.XOR; // 런타임 모드 (스크립터블 오브젝트나 설정으로 동기화 추천)

    // --- Method 1: Synchronous loader for .WAV files ---
    /// <summary>
    /// [WAV 전용] 암호화된 TextAsset을 AudioClip으로 동기 변환
    /// 내부적으로 WavUtility 사용, 비압축 WAV 파일에 가장 빠르고 효율적
    /// 추가: 캐싱 지원
    /// </summary>
    public static AudioClip LoadEncryptedAudio(TextAsset encryptedAudioAsset, EncryptionMode mode = EncryptionMode.XOR)
    {
        if (encryptedAudioAsset == null)
        {
            Debug.LogError("암호화된 오디오 에셋이 null입니다.");
            return null;
        }

        string assetName = encryptedAudioAsset.name;
        if (clipCache.TryGetValue(assetName, out AudioClip cachedClip))
        {
            return cachedClip;
        }

        currentMode = mode;
        byte[] decryptedBytes = DecryptData(encryptedAudioAsset.bytes);
        AudioClip clip = WavUtility.ToAudioClip(decryptedBytes);
        if (clip != null)
        {
            clipCache[assetName] = clip;
        }
        return clip;
    }

    // --- Method 2: Asynchronous loader for compressed audio (FLAC, MP3, OGG) ---
    /// <summary>
    /// [FLAC, MP3, OGG 등] 암호화된 TextAsset을 AudioClip으로 비동기 변환
    /// 복호화된 데이터를 임시 파일로 저장 후 로드하므로 대부분의 압축 오디오 형식 지원
    /// 추가: 캐싱 지원
    /// </summary>
    public static async Task<AudioClip> LoadEncryptedAudioAsync(TextAsset encryptedAudioAsset, string tempFileName, EncryptionMode mode = EncryptionMode.XOR)
    {
        if (encryptedAudioAsset == null)
        {
            Debug.LogError("암호화된 오디오 에셋이 null입니다.");
            return null;
        }

        string assetName = encryptedAudioAsset.name;
        if (clipCache.TryGetValue(assetName, out AudioClip cachedClip))
        {
            return cachedClip;
        }

        currentMode = mode;
        byte[] decryptedBytes = DecryptData(encryptedAudioAsset.bytes);
        string tempPath = Path.Combine(Application.persistentDataPath, tempFileName);
        await File.WriteAllBytesAsync(tempPath, decryptedBytes);

        AudioClip audioClip = null;
        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.UNKNOWN))
        {
            var asyncOp = www.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                audioClip = DownloadHandlerAudioClip.GetContent(www);
            }
            else
            {
                Debug.LogError($"임시 오디오 파일 로드 실패: {www.error}");
            }
        }

        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        if (audioClip != null)
        {
            clipCache[assetName] = audioClip;
        }
        return audioClip;
    }

    /// <summary>
    /// 복호화 핵심 로직
    /// </summary>
    private static byte[] DecryptData(byte[] data)
    {
        if (currentMode == EncryptionMode.AES)
        {
            return DecryptAES(data);
        }
        else
        {
            return ProcessXOR(data);
        }
    }

    private static byte[] ProcessXOR(byte[] data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return result;
    }

    // AES 복호화 (런타임 버전)
    private static byte[] DecryptAES(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32, '\0').Substring(0, 32));
            aes.IV = new byte[16];

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(data))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var resultMs = new MemoryStream())
            {
                cs.CopyTo(resultMs);
                return resultMs.ToArray();
            }
        }
    }

    // 캐시 클리어 메서드 (메모리 관리용)
    public static void ClearCache()
    {
        foreach (var clip in clipCache.Values)
        {
            if (clip != null)
            {
                Resources.UnloadAsset(clip);
            }
        }
        clipCache.Clear();
    }
}

#endregion

#region Utilities

/// <summary>
/// .wav 파일의 바이트 배열을 Unity의 AudioClip 객체로 변환하는 유틸리티 클래스
/// 추가: 오류 처리 강화
/// </summary>
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] fileBytes)
    {
        try
        {
            int channels = fileBytes[22];
            int sampleRate = BitConverter.ToInt32(fileBytes, 24);
            int bitDepth = BitConverter.ToInt16(fileBytes, 34);

            int dataChunkPos = 12;
            while (!(fileBytes[dataChunkPos] == 'd' && fileBytes[dataChunkPos + 1] == 'a' &&
                     fileBytes[dataChunkPos + 2] == 't' && fileBytes[dataChunkPos + 3] == 'a'))
            {
                dataChunkPos += 4;
                int chunkSize = BitConverter.ToInt32(fileBytes, dataChunkPos);
                dataChunkPos += 4 + chunkSize;

                if (dataChunkPos >= fileBytes.Length)
                {
                    Debug.LogError("WAV 데이터 청크를 찾을 수 없습니다.");
                    return null;
                }
            }

            int dataSize = BitConverter.ToInt32(fileBytes, dataChunkPos + 4);
            int dataStart = dataChunkPos + 8;

            if (dataStart + dataSize > fileBytes.Length)
            {
                Debug.LogError("WAV 데이터 크기가 파일 길이를 초과합니다.");
                return null;
            }

            float[] data = new float[dataSize / (bitDepth / 8)];
            for (int i = 0; i < data.Length; i++)
            {
                int sampleIndex = dataStart + i * (bitDepth / 8);
                if (bitDepth == 16)
                {
                    short sample = BitConverter.ToInt16(fileBytes, sampleIndex);
                    data[i] = sample / 32768f;
                }
                else if (bitDepth == 8)
                {
                    data[i] = (fileBytes[sampleIndex] - 128) / 128f;
                }
                else
                {
                    Debug.LogError($"지원되지 않는 비트 깊이: {bitDepth}");
                    return null;
                }
            }

            AudioClip audioClip = AudioClip.Create("DecryptedWav", data.Length / channels, channels, sampleRate, false);
            audioClip.SetData(data, 0);
            return audioClip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"WAV 변환 중 오류 발생: {ex.Message}");
            return null;
        }
    }
}

#endregion

#region Test Player Component

/// <summary>
/// 암호화된 오디오 파일을 테스트하기 위한 통합 플레이어 컴포넌트
/// 추가: 리듬 게임용으로 여러 클립 로드 및 재생 컨트롤 (플레이, 정지, 볼륨 등)
/// </summary>
public class UniversalAudioPlayer : MonoBehaviour
{
    /// <summary>
    /// 로드할 오디오의 종류 선택
    /// WAV: WavUtility를 사용하는 동기 방식 (WAV 파일 전용)
    /// Compressed: 임시 파일을 생성하는 비동기 방식 (FLAC, MP3, OGG 등 압축 파일용)
    /// </summary>
    public enum LoadMethod
    {
        WAV,
        Compressed
    }

    [Tooltip("로드할 오디오 파일의 원본 형식 선택")]
    public LoadMethod AudioType;

    [Tooltip(".bytes 확장자를 가진 암호화 에셋을 여기에 할당 (여러 개 지원)")]
    public List<TextAsset> EncryptedAudioFiles = new List<TextAsset>();

    [Tooltip("압축 오디오의 경우, 생성될 임시 파일 이름 지정 (예: temp.flac)")]
    public string TempFileNamePrefix = "temp_";

    [Tooltip("암호화 모드 선택")]
    public RuntimeAudioLoader.EncryptionMode EncryptionMode = RuntimeAudioLoader.EncryptionMode.XOR;

    private AudioSource audioSource;
    private List<AudioClip> loadedClips = new List<AudioClip>();
    private int currentClipIndex = 0;

    // Start 메서드를 async void로 변경하여 동기/비동기 코드 모두 처리
    async void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (EncryptedAudioFiles.Count == 0)
        {
            Debug.LogError("EncryptedAudioFiles가 비어 있습니다!");
            return;
        }

        // 모든 클립 로드
        foreach (var encryptedAudioFile in EncryptedAudioFiles)
        {
            AudioClip clip = null;

            switch (AudioType)
            {
                case LoadMethod.WAV:
                    Debug.Log($"WAV 로더(동기)를 사용하여 '{encryptedAudioFile.name}' 로드를 시작합니다...");
                    clip = RuntimeAudioLoader.LoadEncryptedAudio(encryptedAudioFile, EncryptionMode);
                    break;

                case LoadMethod.Compressed:
                    Debug.Log($"압축 오디오 로더(비동기)를 사용하여 '{encryptedAudioFile.name}' 로드를 시작합니다...");
                    string tempFileName = TempFileNamePrefix + encryptedAudioFile.name + ".audio";
                    clip = await RuntimeAudioLoader.LoadEncryptedAudioAsync(encryptedAudioFile, tempFileName, EncryptionMode);
                    break;
            }

            if (clip != null)
            {
                loadedClips.Add(clip);
            }
            else
            {
                Debug.LogError($"오디오 클립 '{encryptedAudioFile.name}' 로드에 실패했습니다.");
            }
        }

        // 첫 번째 클립 재생
        if (loadedClips.Count > 0)
        {
            PlayCurrentClip();
        }
    }

    private void PlayCurrentClip()
    {
        if (currentClipIndex < loadedClips.Count)
        {
            audioSource.clip = loadedClips[currentClipIndex];
            audioSource.Play();
            Debug.Log($"재생 중: {loadedClips[currentClipIndex].name}");
        }
    }

    // 리듬 게임용 추가 컨트롤 (예: 다음 곡 재생)
    public void PlayNext()
    {
        audioSource.Stop();
        currentClipIndex = (currentClipIndex + 1) % loadedClips.Count;
        PlayCurrentClip();
    }

    public void PlayPrevious()
    {
        audioSource.Stop();
        currentClipIndex = (currentClipIndex - 1 + loadedClips.Count) % loadedClips.Count;
        PlayCurrentClip();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    private void OnDestroy()
    {
        RuntimeAudioLoader.ClearCache(); // 메모리 정리
    }
}

#endregion

#region Rhythm Game Specific Manager

/// <summary>
/// 리듬 게임 전용 매니저: 여러 암호화된 음원 로드 및 재생 관리
/// 이 클래스를 게임 매니저에 붙여 사용
/// </summary>
public class RhythmGameAudioManager : MonoBehaviour
{
    [SerializeField] private List<TextAsset> songAssets = new List<TextAsset>(); // 암호화된 곡 리스트
    [SerializeField] private UniversalAudioPlayer.LoadMethod loadMethod = UniversalAudioPlayer.LoadMethod.Compressed;
    [SerializeField] private RuntimeAudioLoader.EncryptionMode encryptionMode = RuntimeAudioLoader.EncryptionMode.AES;

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> songClips = new Dictionary<string, AudioClip>();
    private string currentSongName;

    async void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 모든 곡 미리 로드 (백그라운드 로딩)
        foreach (var asset in songAssets)
        {
            AudioClip clip = null;
            if (loadMethod == UniversalAudioPlayer.LoadMethod.WAV)
            {
                clip = RuntimeAudioLoader.LoadEncryptedAudio(asset, encryptionMode);
            }
            else
            {
                string tempFileName = "rhythm_temp_" + asset.name + ".audio";
                clip = await RuntimeAudioLoader.LoadEncryptedAudioAsync(asset, tempFileName, encryptionMode);
            }

            if (clip != null)
            {
                songClips[asset.name] = clip;
            }
        }
    }

    /// <summary>
    /// 특정 곡 재생 (리듬 게임에서 곡 선택 시 호출)
    /// </summary>
    public void PlaySong(string songName)
    {
        if (songClips.TryGetValue(songName, out AudioClip clip))
        {
            audioSource.clip = clip;
            audioSource.Play();
            currentSongName = songName;
            Debug.Log($"리듬 게임 곡 재생: {songName}");
        }
        else
        {
            Debug.LogError($"곡 '{songName}'을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 현재 곡 일시정지/재개
    /// </summary>
    public void TogglePause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }

    /// <summary>
    /// 재생 시간 가져오기 (리듬 게임 노트 싱크용)
    /// </summary>
    public float GetCurrentTime()
    {
        return audioSource.time;
    }

    /// <summary>
    /// 볼륨 조절 (설정 메뉴용)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        audioSource.volume = volume;
    }
}

#endregion
