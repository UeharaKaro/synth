using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

#region Editor-Only Encryption Tool

/// <summary>
/// 보안 강화된 에셋 암호화 시스템
/// AES-256 암호화를 사용하여 오디오, 이미지, 차트 등의 에셋 보호
/// </summary>
public class SecureAssetEncryptor
{
    // 암호화 키 - 빌드 시 반드시 변경해야 함
    private const string ENCRYPTION_KEY = "Synth_SecureKey_2025_CHANGE_THIS!";
    
    // 암호화 모드
    public enum EncryptionMode
    {
        XOR,    // 레거시 호환용 (빠르지만 약함)
        AES256  // 권장 - 군사급 보안
    }

    private static EncryptionMode currentMode = EncryptionMode.AES256;

    #region Menu Items

    [MenuItem("Assets/Encryption/Encrypt Selected File (AES-256)")]
    private static void EncryptSelectedFile()
    {
        currentMode = EncryptionMode.AES256;
        ProcessSelectedFiles(true);
    }

    [MenuItem("Assets/Encryption/Encrypt Multiple Files (AES-256)")]
    private static void EncryptMultipleFiles()
    {
        currentMode = EncryptionMode.AES256;
        ProcessSelectedFiles(true, true);
    }

    [MenuItem("Assets/Encryption/Decrypt Selected File")]
    private static void DecryptSelectedFile()
    {
        ProcessSelectedFiles(false);
    }

    [MenuItem("Assets/Encryption/Encrypt StreamingAssets Folder")]
    private static void EncryptStreamingAssets()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        if (!Directory.Exists(streamingAssetsPath))
        {
            EditorUtility.DisplayDialog("오류", "StreamingAssets 폴더가 존재하지 않습니다.", "확인");
            return;
        }

        currentMode = EncryptionMode.AES256;
        
        List<string> audioFiles = new List<string>();
        audioFiles.AddRange(Directory.GetFiles(streamingAssetsPath, "*.wav", SearchOption.AllDirectories));
        audioFiles.AddRange(Directory.GetFiles(streamingAssetsPath, "*.ogg", SearchOption.AllDirectories));
        audioFiles.AddRange(Directory.GetFiles(streamingAssetsPath, "*.mp3", SearchOption.AllDirectories));
        
        List<string> imageFiles = new List<string>();
        imageFiles.AddRange(Directory.GetFiles(streamingAssetsPath, "*.png", SearchOption.AllDirectories));
        imageFiles.AddRange(Directory.GetFiles(streamingAssetsPath, "*.jpg", SearchOption.AllDirectories));
        imageFiles.AddRange(Directory.GetFiles(streamingAssetsPath, "*.jpeg", SearchOption.AllDirectories));

        int totalFiles = audioFiles.Count + imageFiles.Count;
        
        if (totalFiles == 0)
        {
            EditorUtility.DisplayDialog("알림", "암호화할 파일이 없습니다.", "확인");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "StreamingAssets 암호화",
            $"총 {totalFiles}개 파일을 암호화합니다.\n" +
            $"오디오: {audioFiles.Count}개\n" +
            $"이미지: {imageFiles.Count}개\n\n" +
            "원본 파일은 .backup 폴더에 백업됩니다.\n계속하시겠습니까?",
            "암호화 시작",
            "취소"
        );

        if (!confirm) return;

        // 백업 폴더 생성
        string backupPath = Path.Combine(streamingAssetsPath, ".backup");
        Directory.CreateDirectory(backupPath);

        int processed = 0;
        List<string> encrypted = new List<string>();

        foreach (string file in audioFiles)
        {
            EditorUtility.DisplayProgressBar("파일 암호화 중", Path.GetFileName(file), (float)processed / totalFiles);
            
            if (EncryptFile(file, backupPath))
            {
                encrypted.Add(file);
            }
            processed++;
        }

        foreach (string file in imageFiles)
        {
            EditorUtility.DisplayProgressBar("파일 암호화 중", Path.GetFileName(file), (float)processed / totalFiles);
            
            if (EncryptFile(file, backupPath))
            {
                encrypted.Add(file);
            }
            processed++;
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        string message = $"암호화 완료!\n처리된 파일: {encrypted.Count}개\n백업 위치: {backupPath}";
        Debug.Log(message + "\n파일 목록:\n" + string.Join("\n", encrypted));
        EditorUtility.DisplayDialog("성공", message, "확인");
    }

    [MenuItem("Assets/Encryption/Toggle Mode (XOR/AES-256)")]
    private static void ToggleEncryptionMode()
    {
        currentMode = currentMode == EncryptionMode.XOR ? EncryptionMode.AES256 : EncryptionMode.XOR;
        Debug.Log($"암호화 모드 변경: {currentMode}");
        EditorUtility.DisplayDialog("모드 변경", $"현재 암호화 모드: {currentMode}", "확인");
    }

    #endregion

    #region Core Processing

    private static void ProcessSelectedFiles(bool encrypt, bool multiple = false)
    {
        UnityEngine.Object[] selectedObjects = multiple ? Selection.objects : new UnityEngine.Object[] { Selection.activeObject };
        List<string> processedPaths = new List<string>();

        foreach (UnityEngine.Object selectedObject in selectedObjects)
        {
            if (selectedObject == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(assetPath)) continue;

            // 암호화 시: 오디오 파일인지 확인
            if (encrypt && !(selectedObject is AudioClip || selectedObject is Texture2D))
            {
                continue;
            }

            // 복호화 시: .eaw 파일인지 확인
            if (!encrypt && !assetPath.EndsWith(".eaw"))
            {
                continue;
            }

            string fullPath = Path.GetFullPath(assetPath);
            
            try
            {
                if (encrypt)
                {
                    byte[] fileBytes = File.ReadAllBytes(fullPath);
                    byte[] encryptedBytes = EncryptData(fileBytes);
                    
                    // .eaw 확장자로 저장 (Encrypted Asset Wrapper)
                    string newPath = Path.ChangeExtension(assetPath, ".eaw");
                    File.WriteAllBytes(newPath, encryptedBytes);
                    
                    // 원본 파일 삭제 (보안)
                    File.Delete(fullPath);
                    
                    processedPaths.Add(newPath);
                    Debug.Log($"암호화 완료: {Path.GetFileName(fullPath)} → {Path.GetFileName(newPath)} (원본 삭제됨)");
                }
                else
                {
                    byte[] encryptedBytes = File.ReadAllBytes(fullPath);
                    byte[] decryptedBytes = DecryptData(encryptedBytes);
                    
                    // 원본 확장자로 복원
                    string originalExtension = GetOriginalExtension(fullPath);
                    string newPath = assetPath.Replace(".eaw", originalExtension);
                    File.WriteAllBytes(newPath, decryptedBytes);
                    processedPaths.Add(newPath);
                    Debug.Log($"복호화 완료: {Path.GetFileName(fullPath)} → {Path.GetFileName(newPath)}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"파일 처리 실패: {assetPath}\n오류: {ex.Message}");
            }
        }

        AssetDatabase.Refresh();

        if (processedPaths.Count > 0)
        {
            string action = encrypt ? "암호화" : "복호화";
            string message = $"{action} 완료:\n" + string.Join("\n", processedPaths);
            Debug.Log(message);
            EditorUtility.DisplayDialog("성공", $"{processedPaths.Count}개 파일 {action} 완료", "확인");
        }
        else
        {
            string action = encrypt ? "암호화" : "복호화";
            EditorUtility.DisplayDialog("알림", $"{action}할 파일을 선택해주세요.", "확인");
        }
    }

    private static bool EncryptFile(string filePath, string backupPath)
    {
        try
        {
            // 백업
            string fileName = Path.GetFileName(filePath);
            string backupFile = Path.Combine(backupPath, fileName);
            File.Copy(filePath, backupFile, true);

            // 암호화
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] encryptedBytes = EncryptData(fileBytes);

            // .eaw 확장자로 저장
            string encryptedPath = Path.ChangeExtension(filePath, ".eaw");
            File.WriteAllBytes(encryptedPath, encryptedBytes);

            // 원본 삭제
            File.Delete(filePath);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 암호화 실패: {filePath}\n{ex.Message}");
            return false;
        }
    }

    #endregion

    #region Encryption Algorithms

    private static byte[] EncryptData(byte[] data)
    {
        if (currentMode == EncryptionMode.AES256)
        {
            return EncryptAES(data);
        }
        else
        {
            return EncryptXOR(data);
        }
    }

    private static byte[] DecryptData(byte[] data)
    {
        // 파일 헤더로 암호화 방식 자동 감지
        if (data.Length > 4)
        {
            string header = Encoding.ASCII.GetString(data, 0, 4);
            if (header == "AES:")
            {
                byte[] actualData = new byte[data.Length - 4];
                Array.Copy(data, 4, actualData, 0, actualData.Length);
                return DecryptAES(actualData);
            }
        }

        // 헤더 없으면 XOR로 시도
        return EncryptXOR(data);
    }

    private static byte[] EncryptXOR(byte[] data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
        byte[] result = new byte[data.Length];
        
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        return result;
    }

    private static byte[] EncryptAES(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = GenerateKey();
            aes.IV = GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                // AES 헤더 추가
                byte[] header = Encoding.ASCII.GetBytes("AES:");
                ms.Write(header, 0, header.Length);

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
            aes.KeySize = 256;
            aes.Key = GenerateKey();
            aes.IV = GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

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

    private static byte[] GenerateKey()
    {
        using (var sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTION_KEY));
        }
    }

    private static byte[] GenerateIV()
    {
        // 고정 IV (실제로는 파일마다 다른 IV 사용 권장)
        using (var sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTION_KEY + "_IV"));
            byte[] iv = new byte[16];
            Array.Copy(hash, iv, 16);
            return iv;
        }
    }

    private static string GetOriginalExtension(string encryptedFilePath)
    {
        // 파일 헤더를 읽어서 원본 확장자 추론
        try
        {
            byte[] data = File.ReadAllBytes(encryptedFilePath);
            byte[] decrypted = DecryptData(data);
            
            // WAV 파일 시그니처: "RIFF"
            if (decrypted.Length > 4 && 
                decrypted[0] == 'R' && decrypted[1] == 'I' && 
                decrypted[2] == 'F' && decrypted[3] == 'F')
            {
                return ".wav";
            }
            
            // PNG 파일 시그니처: 89 50 4E 47
            if (decrypted.Length > 4 && 
                decrypted[0] == 0x89 && decrypted[1] == 0x50 && 
                decrypted[2] == 0x4E && decrypted[3] == 0x47)
            {
                return ".png";
            }
            
            // JPEG 파일 시그니처: FF D8 FF
            if (decrypted.Length > 3 && 
                decrypted[0] == 0xFF && decrypted[1] == 0xD8 && 
                decrypted[2] == 0xFF)
            {
                return ".jpg";
            }
            
            // OGG 파일 시그니처: "OggS"
            if (decrypted.Length > 4 && 
                decrypted[0] == 'O' && decrypted[1] == 'g' && 
                decrypted[2] == 'g' && decrypted[3] == 'S')
            {
                return ".ogg";
            }
            
            // 기본값
            return ".decrypted";
        }
        catch
        {
            return ".decrypted";
        }
    }

    #endregion
}

#endregion

#region Runtime Loader

/// <summary>
/// 런타임에서 암호화된 에셋을 로드하는 시스템
/// </summary>
public static class SecureAssetLoader
{
    private const string ENCRYPTION_KEY = "Synth_SecureKey_2025_CHANGE_THIS!";
    
    private static Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    #region Audio Loading

    public static AudioClip LoadEncryptedAudio(string filePath)
    {
        if (audioClipCache.TryGetValue(filePath, out AudioClip cached))
        {
            return cached;
        }

        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = DecryptData(encryptedData);
            
            AudioClip clip = WavUtility.ToAudioClip(decryptedData);
            if (clip != null)
            {
                audioClipCache[filePath] = clip;
            }
            
            return clip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"암호화된 오디오 로드 실패: {filePath}\n{ex.Message}");
            return null;
        }
    }

    public static async Task<AudioClip> LoadEncryptedAudioAsync(string filePath)
    {
        if (audioClipCache.TryGetValue(filePath, out AudioClip cached))
        {
            return cached;
        }

        try
        {
            byte[] encryptedData = await File.ReadAllBytesAsync(filePath);
            byte[] decryptedData = DecryptData(encryptedData);

            string tempPath = Path.Combine(Application.temporaryCachePath, Path.GetFileName(filePath) + ".temp");
            await File.WriteAllBytesAsync(tempPath, decryptedData);

            AudioClip clip = null;
            using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.UNKNOWN))
            {
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        audioClipCache[filePath] = clip;
                    }
                }
                else
                {
                    Debug.LogError($"오디오 로드 실패: {www.error}");
                }
            }

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return clip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"암호화된 오디오 비동기 로드 실패: {filePath}\n{ex.Message}");
            return null;
        }
    }

    #endregion

    #region Image Loading

    public static Texture2D LoadEncryptedImage(string filePath)
    {
        if (textureCache.TryGetValue(filePath, out Texture2D cached))
        {
            return cached;
        }

        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = DecryptData(encryptedData);

            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(decryptedData))
            {
                textureCache[filePath] = texture;
                return texture;
            }

            Debug.LogError($"이미지 로드 실패: {filePath}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"암호화된 이미지 로드 실패: {filePath}\n{ex.Message}");
            return null;
        }
    }

    public static async Task<Texture2D> LoadEncryptedImageAsync(string filePath)
    {
        if (textureCache.TryGetValue(filePath, out Texture2D cached))
        {
            return cached;
        }

        try
        {
            byte[] encryptedData = await File.ReadAllBytesAsync(filePath);
            byte[] decryptedData = DecryptData(encryptedData);

            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(decryptedData))
            {
                textureCache[filePath] = texture;
                return texture;
            }

            Debug.LogError($"이미지 로드 실패: {filePath}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"암호화된 이미지 비동기 로드 실패: {filePath}\n{ex.Message}");
            return null;
        }
    }

    #endregion

    #region Decryption

    public static byte[] DecryptAudioData(byte[] data)
    {
        return DecryptData(data);
    }

    public static byte[] DecryptImageData(byte[] data)
    {
        return DecryptData(data);
    }

    private static byte[] DecryptData(byte[] data)
    {
        // AES 헤더 확인
        if (data.Length > 4)
        {
            string header = Encoding.ASCII.GetString(data, 0, 4);
            if (header == "AES:")
            {
                byte[] actualData = new byte[data.Length - 4];
                Array.Copy(data, 4, actualData, 0, actualData.Length);
                return DecryptAES(actualData);
            }
        }

        // XOR 복호화
        return DecryptXOR(data);
    }

    private static byte[] DecryptXOR(byte[] data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
        byte[] result = new byte[data.Length];
        
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        return result;
    }

    private static byte[] DecryptAES(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = GenerateKey();
            aes.IV = GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

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

    private static byte[] GenerateKey()
    {
        using (var sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTION_KEY));
        }
    }

    private static byte[] GenerateIV()
    {
        using (var sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTION_KEY + "_IV"));
            byte[] iv = new byte[16];
            Array.Copy(hash, iv, 16);
            return iv;
        }
    }

    #endregion

    #region Cache Management

    public static void ClearAudioCache()
    {
        foreach (var clip in audioClipCache.Values)
        {
            if (clip != null)
            {
                UnityEngine.Object.Destroy(clip);
            }
        }
        audioClipCache.Clear();
    }

    public static void ClearTextureCache()
    {
        foreach (var texture in textureCache.Values)
        {
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
            }
        }
        textureCache.Clear();
    }

    public static void ClearAllCaches()
    {
        ClearAudioCache();
        ClearTextureCache();
    }

    #endregion
}

#endregion

#region Utilities

/// <summary>
/// WAV 파일 변환 유틸리티
/// </summary>
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] fileBytes)
    {
        try
        {
            if (fileBytes.Length < 44)
            {
                Debug.LogError("WAV 파일이 너무 작습니다.");
                return null;
            }

            int channels = fileBytes[22];
            int sampleRate = BitConverter.ToInt32(fileBytes, 24);
            int bitDepth = BitConverter.ToInt16(fileBytes, 34);

            int dataChunkPos = 12;
            while (dataChunkPos < fileBytes.Length - 8)
            {
                if (fileBytes[dataChunkPos] == 'd' && fileBytes[dataChunkPos + 1] == 'a' &&
                    fileBytes[dataChunkPos + 2] == 't' && fileBytes[dataChunkPos + 3] == 'a')
                {
                    break;
                }
                
                dataChunkPos += 4;
                int chunkSize = BitConverter.ToInt32(fileBytes, dataChunkPos);
                dataChunkPos += 4 + chunkSize;
            }

            if (dataChunkPos >= fileBytes.Length - 8)
            {
                Debug.LogError("WAV 데이터 청크를 찾을 수 없습니다.");
                return null;
            }

            int dataSize = BitConverter.ToInt32(fileBytes, dataChunkPos + 4);
            int dataStart = dataChunkPos + 8;

            if (dataStart + dataSize > fileBytes.Length)
            {
                Debug.LogError("WAV 데이터 크기가 파일 크기를 초과합니다.");
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
            }

            AudioClip audioClip = AudioClip.Create("DecryptedWav", data.Length / channels, channels, sampleRate, false);
            audioClip.SetData(data, 0);
            
            return audioClip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"WAV 변환 실패: {ex.Message}");
            return null;
        }
    }
}

#endregion
