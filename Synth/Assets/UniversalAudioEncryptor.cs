using UnityEditor;
using System.IO; // 파일 입출력 네임스페이스 (File, Path 등)
using System.Text; // 문자열 인코딩(UTF8) 네임스페이스
using UnityEngine;
using System.Threading.Tasks; // 비동기 작업 네임스페이스(Task)
using UnityEngine.Networking; // Unity의 네트워킹(웹 요청) 기능을 위한 네임스페이스

#region Editor-Only Encryption Tool

/// <summary>
/// Unity 에디터 내에서 에셋(주로 오디오 파일) 암호화 기능 제공 클래스
/// 이 클래스의 코드는 Unity 에디터에서만 실행되며, 최종 게임 빌드에는 포함되지 않음
/// </summary>
public class AssetEncryptor
{
    // [중요] 암호화 및 복호화에 사용될 비밀 키
    private const string EncryptionKey = "YourSecretKey";
    private const string MenuItemEncrypt = "Assets/Encrypt Audio File";

    [MenuItem(MenuItemEncrypt)]
    private static void EncryptSelectedAudioFile()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null || !(selectedObject is AudioClip))
        {
            EditorUtility.DisplayDialog("오류", "암호화할 오디오 파일(.wav, .ogg)을 선택해주세요.", "확인");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        string fullPath = Path.GetFullPath(assetPath);
        byte[] fileBytes = File.ReadAllBytes(fullPath);
        byte[] encryptedBytes = ProcessData(fileBytes);
        string newPath = Path.ChangeExtension(assetPath, ".bytes");
        File.WriteAllBytes(newPath, encryptedBytes);
        AssetDatabase.Refresh();

        Debug.Log($"암호화 완료: '{assetPath}' -> '{newPath}'");
        EditorUtility.DisplayDialog("성공", $"파일이 성공적으로 암호화되었습니다.\n원본: {assetPath}\n결과: {newPath}", "확인");
    }

    [MenuItem(MenuItemEncrypt, true)]
    private static bool ValidateEncryptAudioFile()
    {
        return Selection.activeObject is AudioClip;
    }

    private static byte[] ProcessData(byte[] data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return result;
    }
}

#endregion

#region Runtime Loader

/// <summary>
/// 게임 실행 중(런타임)에 암호화된 오디오 파일을 불러와서(복호화) 재생 가능한 형태로 만드는 헬퍼 클래스
/// 이 클래스는 최종 게임 빌드에 포함됨
/// </summary>
public static class RuntimeAudioLoader
{
    // [중요] 에디터의 AssetEncryptor 클래스에 정의된 EncryptionKey와 반드시 동일한 값이어야 함
    private const string EncryptionKey = "YourSecretKey";

    // --- Method 1: Synchronous loader for .WAV files ---
    /// <summary>
    /// [WAV 전용] 암호화된 TextAsset을 AudioClip으로 동기 변환
    /// 내부적으로 WavUtility 사용, 비압축 WAV 파일에 가장 빠르고 효율적
    /// </summary>
    public static AudioClip LoadEncryptedAudio(TextAsset encryptedAudioAsset)
    {
        if (encryptedAudioAsset == null)
        {
            Debug.LogError("암호화된 오디오 에셋이 null입니다.");
            return null;
        }
        byte[] decryptedBytes = ProcessData(encryptedAudioAsset.bytes);
        return WavUtility.ToAudioClip(decryptedBytes);
    }

    // --- Method 2: Asynchronous loader for compressed audio (FLAC, MP3, OGG) ---
    /// <summary>
    /// [FLAC, MP3, OGG 등] 암호화된 TextAsset을 AudioClip으로 비동기 변환
    /// 복호화된 데이터를 임시 파일로 저장 후 로드하므로 대부분의 압축 오디오 형식 지원
    /// </summary>
    public static async Task<AudioClip> LoadEncryptedAudioAsync(TextAsset encryptedAudioAsset, string tempFileName)
    {
        if (encryptedAudioAsset == null)
        {
            Debug.LogError("암호화된 오디오 에셋이 null입니다.");
            return null;
        }

        byte[] decryptedBytes = ProcessData(encryptedAudioAsset.bytes);
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
        return audioClip;
    }

    /// <summary>
    /// 암호화/복호화 핵심 로직
    /// </summary>
    private static byte[] ProcessData(byte[] data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return result;
    }
}

#endregion

#region Utilities

/// <summary>
/// .wav 파일의 바이트 배열을 Unity의 AudioClip 객체로 변환하는 유틸리티 클래스
/// </summary>
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] fileBytes)
    {
        int channels = fileBytes[22];
        int sampleRate = System.BitConverter.ToInt32(fileBytes, 24);
        int bitDepth = System.BitConverter.ToInt16(fileBytes, 34);

        int dataChunkPos = 12;
        while (!(fileBytes[dataChunkPos] == 'd' && fileBytes[dataChunkPos + 1] == 'a' &&
                 fileBytes[dataChunkPos + 2] == 't' && fileBytes[dataChunkPos + 3] == 'a'))
        {
            dataChunkPos += 4;
            int chunkSize = System.BitConverter.ToInt32(fileBytes, dataChunkPos);
            dataChunkPos += 4 + chunkSize;
        }
        
        int dataSize = System.BitConverter.ToInt32(fileBytes, dataChunkPos + 4);
        int dataStart = dataChunkPos + 8;

        float[] data = new float[dataSize / (bitDepth / 8)];
        for (int i = 0; i < data.Length; i++)
        {
            int sampleIndex = dataStart + i * (bitDepth / 8);
            if (bitDepth == 16)
            {
                short sample = System.BitConverter.ToInt16(fileBytes, sampleIndex);
                data[i] = sample / 32768f;
            }
            else if (bitDepth == 8)
            {
                data[i] = (fileBytes[sampleIndex] - 128) / 128f;
            }
        }

        AudioClip audioClip = AudioClip.Create("DecryptedWav", data.Length / channels, channels, sampleRate, false);
        audioClip.SetData(data, 0);
        return audioClip;
    }
}

#endregion

#region Test Player Component

/// <summary>
/// 암호화된 오디오 파일을 테스트하기 위한 통합 플레이어 컴포넌트
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

    [Tooltip(".bytes 확장자를 가진 암호화 에셋을 여기에 할당")]
    public TextAsset EncryptedAudioFile;

    [Tooltip("압축 오디오의 경우, 생성될 임시 파일 이름 지정 (예: temp.flac)")]
    public string TempFileName = "temp.audio";

    private AudioSource audioSource;

    // Start 메서드를 async void로 변경하여 동기/비동기 코드 모두 처리
    async void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (EncryptedAudioFile == null)
        {
            Debug.LogError("EncryptedAudioFile이 할당되지 않았습니다!");
            return;
        }

        AudioClip clip = null;

        // Inspector에서 선택한 로드 방식에 따라 다른 메서드 호출
        switch (AudioType)
        {
            case LoadMethod.WAV:
                Debug.Log("WAV 로더(동기)를 사용하여 로드를 시작합니다...");
                clip = RuntimeAudioLoader.LoadEncryptedAudio(EncryptedAudioFile);
                break;

            case LoadMethod.Compressed:
                Debug.Log("압축 오디오 로더(비동기)를 사용하여 로드를 시작합니다...");
                clip = await RuntimeAudioLoader.LoadEncryptedAudioAsync(EncryptedAudioFile, TempFileName);
                break;
        }

        // 최종적으로 로드된 클립 재생
        if (clip != null)
        {
            Debug.Log("오디오 클립 로드 성공! 재생을 시작합니다.");
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("오디오 클립 로드에 실패했습니다.");
        }
    }
}

#endregion
