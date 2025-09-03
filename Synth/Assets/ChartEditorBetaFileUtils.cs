using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Text;

/// <summary>
/// Utility class for ChartEditorBeta file operations and encryption
/// Handles saving/loading charts and audio file encryption/decryption
/// </summary>
public static class ChartEditorBetaFileUtils
{
    private const string CHART_FILE_EXTENSION = ".chart";
    private const string ENCRYPTED_AUDIO_EXTENSION = ".eaw"; // Encrypted Audio WAV
    private const string DEFAULT_ENCRYPTION_KEY = "ChartEditorBeta2023";

    #region Chart File Operations
    
    /// <summary>
    /// Save chart data to file with optional encryption
    /// </summary>
    public static bool SaveChart(ChartDataBeta chartData, string filePath, bool encrypt = false, string encryptionKey = null)
    {
        try
        {
            string json = JsonUtility.ToJson(chartData, true);
            
            if (encrypt)
            {
                byte[] encryptedData = EncryptString(json, encryptionKey ?? DEFAULT_ENCRYPTION_KEY);
                File.WriteAllBytes(filePath, encryptedData);
            }
            else
            {
                File.WriteAllText(filePath, json);
            }
            
            Debug.Log($"Chart saved successfully: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save chart: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Load chart data from file with automatic decryption detection
    /// </summary>
    public static ChartDataBeta LoadChart(string filePath, string encryptionKey = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Chart file not found: {filePath}");
                return null;
            }
            
            string json;
            
            // Try to determine if file is encrypted
            if (IsFileEncrypted(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                json = DecryptString(encryptedData, encryptionKey ?? DEFAULT_ENCRYPTION_KEY);
            }
            else
            {
                json = File.ReadAllText(filePath);
            }
            
            ChartDataBeta chartData = JsonUtility.FromJson<ChartDataBeta>(json);
            Debug.Log($"Chart loaded successfully: {filePath}");
            return chartData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load chart: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Export chart with metadata
    /// </summary>
    public static bool ExportChart(ChartDataBeta chartData, string filePath, string title = "", string artist = "", string charter = "")
    {
        try
        {
            var exportData = new ChartExportData
            {
                metadata = new ChartMetadata
                {
                    title = title,
                    artist = artist,
                    charter = charter,
                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    version = "1.0"
                },
                chartData = chartData
            };
            
            string json = JsonUtility.ToJson(exportData, true);
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Chart exported successfully: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export chart: {e.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Audio File Operations
    
    /// <summary>
    /// Encrypt audio file for secure distribution
    /// </summary>
    public static bool EncryptAudioFile(string inputPath, string outputPath, string encryptionKey = null)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                Debug.LogError($"Input audio file not found: {inputPath}");
                return false;
            }
            
            byte[] audioData = File.ReadAllBytes(inputPath);
            byte[] encryptedData = EncryptBytes(audioData, encryptionKey ?? DEFAULT_ENCRYPTION_KEY);
            
            File.WriteAllBytes(outputPath, encryptedData);
            Debug.Log($"Audio file encrypted successfully: {outputPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to encrypt audio file: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Decrypt audio file for playback
    /// </summary>
    public static byte[] DecryptAudioFile(string filePath, string encryptionKey = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Encrypted audio file not found: {filePath}");
                return null;
            }
            
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = DecryptBytes(encryptedData, encryptionKey ?? DEFAULT_ENCRYPTION_KEY);
            
            Debug.Log($"Audio file decrypted successfully: {filePath}");
            return decryptedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to decrypt audio file: {e.Message}");
            return null;
        }
    }
    
    #endregion
    
    #region Encryption/Decryption Core
    
    private static byte[] EncryptString(string plainText, string key)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        return EncryptBytes(plainBytes, key);
    }
    
    private static string DecryptString(byte[] encryptedData, string key)
    {
        byte[] decryptedBytes = DecryptBytes(encryptedData, key);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
    
    private static byte[] EncryptBytes(byte[] data, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GetKeyBytes(key);
            aes.GenerateIV();
            
            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
                
                // Prepend IV to encrypted data
                byte[] result = new byte[aes.IV.Length + encryptedData.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(encryptedData, 0, result, aes.IV.Length, encryptedData.Length);
                
                return result;
            }
        }
    }
    
    private static byte[] DecryptBytes(byte[] encryptedData, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GetKeyBytes(key);
            
            // Extract IV from the beginning of encrypted data
            byte[] iv = new byte[16];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
            aes.IV = iv;
            
            // Extract actual encrypted data
            byte[] actualEncryptedData = new byte[encryptedData.Length - 16];
            Buffer.BlockCopy(encryptedData, 16, actualEncryptedData, 0, actualEncryptedData.Length);
            
            using (var decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(actualEncryptedData, 0, actualEncryptedData.Length);
            }
        }
    }
    
    private static byte[] GetKeyBytes(string key)
    {
        byte[] keyBytes = new byte[32]; // 256-bit key
        byte[] sourceBytes = Encoding.UTF8.GetBytes(key);
        int copyLength = Math.Min(sourceBytes.Length, keyBytes.Length);
        Buffer.BlockCopy(sourceBytes, 0, keyBytes, 0, copyLength);
        return keyBytes;
    }
    
    private static bool IsFileEncrypted(string filePath)
    {
        try
        {
            // Try to read as JSON first
            string content = File.ReadAllText(filePath);
            JsonUtility.FromJson<ChartDataBeta>(content);
            return false; // Successfully parsed as JSON, not encrypted
        }
        catch
        {
            return true; // Failed to parse as JSON, likely encrypted
        }
    }
    
    #endregion
    
    #region File Path Utilities
    
    public static string GetChartsDirectory()
    {
        string chartsDir = Path.Combine(Application.persistentDataPath, "Charts");
        if (!Directory.Exists(chartsDir))
        {
            Directory.CreateDirectory(chartsDir);
        }
        return chartsDir;
    }
    
    public static string GetAudioDirectory()
    {
        string audioDir = Path.Combine(Application.persistentDataPath, "Audio");
        if (!Directory.Exists(audioDir))
        {
            Directory.CreateDirectory(audioDir);
        }
        return audioDir;
    }
    
    public static string GenerateChartFileName(string title, string artist)
    {
        string sanitizedTitle = SanitizeFileName(title);
        string sanitizedArtist = SanitizeFileName(artist);
        
        if (string.IsNullOrEmpty(sanitizedTitle))
            sanitizedTitle = "Untitled";
        if (string.IsNullOrEmpty(sanitizedArtist))
            sanitizedArtist = "Unknown";
            
        return $"{sanitizedArtist} - {sanitizedTitle}{CHART_FILE_EXTENSION}";
    }
    
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "";
            
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        
        return fileName.Trim();
    }
    
    public static string GetDefaultChartPath(string chartName = "NewChart")
    {
        return Path.Combine(GetChartsDirectory(), $"{SanitizeFileName(chartName)}{CHART_FILE_EXTENSION}");
    }
    
    #endregion
    
    #region Backup and Recovery
    
    public static bool CreateBackup(ChartDataBeta chartData, string originalPath)
    {
        try
        {
            string backupDir = Path.Combine(Path.GetDirectoryName(originalPath), "Backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
            
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupDir, $"{fileName}_backup_{timestamp}{CHART_FILE_EXTENSION}");
            
            return SaveChart(chartData, backupPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create backup: {e.Message}");
            return false;
        }
    }
    
    public static string[] GetBackupFiles(string originalPath)
    {
        try
        {
            string backupDir = Path.Combine(Path.GetDirectoryName(originalPath), "Backups");
            if (!Directory.Exists(backupDir))
                return new string[0];
                
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string pattern = $"{fileName}_backup_*{CHART_FILE_EXTENSION}";
            
            return Directory.GetFiles(backupDir, pattern);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get backup files: {e.Message}");
            return new string[0];
        }
    }
    
    #endregion
}

#region Data Structures

[System.Serializable]
public class ChartMetadata
{
    public string title;
    public string artist;
    public string charter;
    public string createdDate;
    public string version;
    public string description;
    public string difficulty;
    public int noteCount;
    public float duration;
}

[System.Serializable]
public class ChartExportData
{
    public ChartMetadata metadata;
    public ChartDataBeta chartData;
}

#endregion