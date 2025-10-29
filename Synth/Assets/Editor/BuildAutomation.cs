using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 빌드 전 자동 암호화 시스템
/// </summary>
public class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("=== 빌드 전처리 시작: 에셋 암호화 ===");

        // 사용자에게 확인
        bool encrypt = EditorUtility.DisplayDialog(
            "빌드 전 암호화",
            "StreamingAssets 폴더의 에셋을 암호화하시겠습니까?\n\n" +
            "• 오디오 파일 (.wav, .ogg, .mp3)\n" +
            "• 이미지 파일 (.png, .jpg)\n\n" +
            "원본은 .backup 폴더에 보관됩니다.",
            "암호화하고 빌드",
            "암호화 없이 빌드"
        );

        if (encrypt)
        {
            EncryptStreamingAssets();
        }
        else
        {
            Debug.LogWarning("암호화를 건너뛰고 빌드를 진행합니다.");
        }
    }

    private void EncryptStreamingAssets()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        
        if (!Directory.Exists(streamingAssetsPath))
        {
            Debug.LogWarning("StreamingAssets 폴더가 존재하지 않습니다. 암호화를 건너뜁니다.");
            return;
        }

        // 암호화할 파일 찾기
        List<string> filesToEncrypt = new List<string>();
        
        // 오디오 파일
        filesToEncrypt.AddRange(Directory.GetFiles(streamingAssetsPath, "*.wav", SearchOption.AllDirectories));
        filesToEncrypt.AddRange(Directory.GetFiles(streamingAssetsPath, "*.ogg", SearchOption.AllDirectories));
        filesToEncrypt.AddRange(Directory.GetFiles(streamingAssetsPath, "*.mp3", SearchOption.AllDirectories));
        
        // 이미지 파일
        filesToEncrypt.AddRange(Directory.GetFiles(streamingAssetsPath, "*.png", SearchOption.AllDirectories));
        filesToEncrypt.AddRange(Directory.GetFiles(streamingAssetsPath, "*.jpg", SearchOption.AllDirectories));
        filesToEncrypt.AddRange(Directory.GetFiles(streamingAssetsPath, "*.jpeg", SearchOption.AllDirectories));

        if (filesToEncrypt.Count == 0)
        {
            Debug.Log("암호화할 파일이 없습니다.");
            return;
        }

        // 백업 폴더 생성
        string backupFolder = Path.Combine(streamingAssetsPath, ".backup");
        Directory.CreateDirectory(backupFolder);

        int encryptedCount = 0;
        int skippedCount = 0;

        foreach (string file in filesToEncrypt)
        {
            // 이미 암호화된 파일 건너뛰기
            string encryptedPath = Path.ChangeExtension(file, ".eaw");
            if (File.Exists(encryptedPath))
            {
                Debug.Log($"이미 암호화됨: {Path.GetFileName(file)}");
                skippedCount++;
                continue;
            }

            // 백업 폴더 내 파일은 건너뛰기
            if (file.Contains(".backup"))
            {
                skippedCount++;
                continue;
            }

            try
            {
                // 백업
                string fileName = Path.GetFileName(file);
                string backupFile = Path.Combine(backupFolder, fileName);
                File.Copy(file, backupFile, true);

                // 암호화
                byte[] originalData = File.ReadAllBytes(file);
                byte[] encryptedData = EncryptDataAES(originalData);
                File.WriteAllBytes(encryptedPath, encryptedData);

                // 원본 삭제
                File.Delete(file);

                encryptedCount++;
                Debug.Log($"암호화 완료: {fileName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"암호화 실패: {Path.GetFileName(file)}\n{ex.Message}");
            }
        }

        Debug.Log($"=== 암호화 완료 ===\n암호화: {encryptedCount}개\n건너뜀: {skippedCount}개\n백업 위치: {backupFolder}");
        
        // AssetDatabase 새로고침
        AssetDatabase.Refresh();
    }

    // AES 암호화 (SecureAssetEncryptor와 동일한 키 사용)
    private byte[] EncryptDataAES(byte[] data)
    {
        const string ENCRYPTION_KEY = "Synth_SecureKey_2025_CHANGE_THIS!";
        
        using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = GenerateKey(ENCRYPTION_KEY);
            aes.IV = GenerateIV(ENCRYPTION_KEY);
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new System.IO.MemoryStream())
            {
                // AES 헤더 추가
                byte[] header = System.Text.Encoding.ASCII.GetBytes("AES:");
                ms.Write(header, 0, header.Length);

                using (var cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }
                
                return ms.ToArray();
            }
        }
    }

    private byte[] GenerateKey(string key)
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));
        }
    }

    private byte[] GenerateIV(string key)
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key + "_IV"));
            byte[] iv = new byte[16];
            System.Array.Copy(hash, iv, 16);
            return iv;
        }
    }
}

/// <summary>
/// 빌드 후 원본 파일 복원 시스템 (옵션)
/// </summary>
public class BuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("=== 빌드 후처리 시작 ===");

        bool restore = EditorUtility.DisplayDialog(
            "빌드 완료",
            "빌드가 완료되었습니다.\n\n" +
            "StreamingAssets의 원본 파일을 복원하시겠습니까?\n" +
            "(개발 중 테스트를 위해 복원 권장)",
            "복원",
            "암호화 상태 유지"
        );

        if (restore)
        {
            RestoreOriginalFiles();
        }
    }

    private void RestoreOriginalFiles()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string backupFolder = Path.Combine(streamingAssetsPath, ".backup");

        if (!Directory.Exists(backupFolder))
        {
            Debug.LogWarning("백업 폴더가 존재하지 않습니다.");
            return;
        }

        string[] backupFiles = Directory.GetFiles(backupFolder, "*", SearchOption.AllDirectories);
        int restoredCount = 0;

        foreach (string backupFile in backupFiles)
        {
            try
            {
                string fileName = Path.GetFileName(backupFile);
                string originalPath = Path.Combine(streamingAssetsPath, 
                    backupFile.Replace(backupFolder + Path.DirectorySeparatorChar, ""));

                // 암호화된 파일 삭제
                string encryptedPath = Path.ChangeExtension(originalPath, ".eaw");
                if (File.Exists(encryptedPath))
                {
                    File.Delete(encryptedPath);
                }

                // 원본 복원
                File.Copy(backupFile, originalPath, true);
                restoredCount++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"복원 실패: {Path.GetFileName(backupFile)}\n{ex.Message}");
            }
        }

        Debug.Log($"=== 복원 완료: {restoredCount}개 파일 ===");
        AssetDatabase.Refresh();
    }
}

/// <summary>
/// 수동 백업/복원 메뉴
/// </summary>
public class EncryptionUtilities
{
    [MenuItem("Tools/Encryption/Restore Original Files from Backup")]
    public static void RestoreFromBackup()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string backupFolder = Path.Combine(streamingAssetsPath, ".backup");

        if (!Directory.Exists(backupFolder))
        {
            EditorUtility.DisplayDialog("오류", "백업 폴더가 존재하지 않습니다.", "확인");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "원본 파일 복원",
            "백업된 원본 파일을 복원하고 암호화된 파일을 삭제합니다.\n계속하시겠습니까?",
            "복원",
            "취소"
        );

        if (!confirm) return;

        string[] backupFiles = Directory.GetFiles(backupFolder, "*", SearchOption.AllDirectories);
        int restoredCount = 0;

        foreach (string backupFile in backupFiles)
        {
            try
            {
                string relativePath = backupFile.Replace(backupFolder + Path.DirectorySeparatorChar, "");
                string originalPath = Path.Combine(streamingAssetsPath, relativePath);

                // 디렉토리 생성
                Directory.CreateDirectory(Path.GetDirectoryName(originalPath));

                // 암호화된 파일 삭제
                string encryptedPath = Path.ChangeExtension(originalPath, ".eaw");
                if (File.Exists(encryptedPath))
                {
                    File.Delete(encryptedPath);
                }

                // 원본 복원
                File.Copy(backupFile, originalPath, true);
                restoredCount++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"복원 실패: {Path.GetFileName(backupFile)}\n{ex.Message}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{restoredCount}개 파일이 복원되었습니다.", "확인");
    }

    [MenuItem("Tools/Encryption/Delete Backup Folder")]
    public static void DeleteBackupFolder()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string backupFolder = Path.Combine(streamingAssetsPath, ".backup");

        if (!Directory.Exists(backupFolder))
        {
            EditorUtility.DisplayDialog("알림", "백업 폴더가 존재하지 않습니다.", "확인");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "백업 삭제",
            "백업 폴더를 삭제합니다.\n이 작업은 되돌릴 수 없습니다!\n\n" +
            "계속하시겠습니까?",
            "삭제",
            "취소"
        );

        if (!confirm) return;

        try
        {
            Directory.Delete(backupFolder, true);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "백업 폴더가 삭제되었습니다.", "확인");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("오류", $"삭제 실패: {ex.Message}", "확인");
        }
    }
}
