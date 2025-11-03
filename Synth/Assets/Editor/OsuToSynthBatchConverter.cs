using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// osu! 맵 파일(.osu)을 Synth 맵 파일(.synth)로 일괄 변환하는 도구
/// Unity 메뉴: Tools → Convert All Osu Maps to Synth
/// 
/// 생성일: 2025-11-03
/// </summary>
public class OsuToSynthBatchConverter : MonoBehaviour
{
    [Header("변환 설정")]
    [Tooltip("osu 맵이 있는 폴더 경로 (StreamingAssets 기준)")]
    public string osuMapsFolder = "Ousmaps";
    
    [Tooltip("변환된 synth 맵을 저장할 폴더 (StreamingAssets 기준)")]
    public string synthMapsFolder = "SynthMaps";
    
    [Tooltip("변환 후 osu 파일 삭제 여부")]
    public bool deleteOsuFilesAfterConversion = false;
    
    [Header("변환 옵션")]
    [Tooltip("이미 존재하는 synth 파일 덮어쓰기")]
    public bool overwriteExisting = false;
    
    [Tooltip("변환 중 상세 로그 출력")]
    public bool verboseLogging = true;
    
#if UNITY_EDITOR
    [MenuItem("Tools/Convert All Osu Maps to Synth")]
    public static void ConvertAllOsuMapsMenu()
    {
        // 직접 실행 (static 메서드로 변경)
        ConvertAllOsuMapsStatic();
    }
    
    /// <summary>
    /// Static 메서드로 변환 실행
    /// </summary>
    private static void ConvertAllOsuMapsStatic()
    {
        string osuFolderPath = Path.Combine(Application.streamingAssetsPath, "Ousmaps");
        string synthFolderPath = Path.Combine(Application.streamingAssetsPath, "SynthMaps");
        
        Debug.Log($"=== osu → synth 일괄 변환 시작 ===");
        Debug.Log($"osu 폴더: {osuFolderPath}");
        Debug.Log($"synth 폴더: {synthFolderPath}");
        Debug.Log($"");
        
        // 폴더 존재 확인
        if (!Directory.Exists(osuFolderPath))
        {
            Debug.LogError($"❌ osu 맵 폴더가 존재하지 않습니다: {osuFolderPath}");
            return;
        }
        
        // synth 폴더 생성
        if (!Directory.Exists(synthFolderPath))
        {
            Directory.CreateDirectory(synthFolderPath);
            Debug.Log($"✅ synth 폴더 생성: {synthFolderPath}");
        }
        
        // 모든 .osu 파일 찾기
        string[] osuFiles = Directory.GetFiles(osuFolderPath, "*.osu", SearchOption.AllDirectories);
        
        if (osuFiles.Length == 0)
        {
            Debug.LogWarning($"⚠️ osu 파일을 찾을 수 없습니다.");
            return;
        }
        
        Debug.Log($"📁 발견된 osu 파일: {osuFiles.Length}개");
        Debug.Log($"");
        
        // 통계
        int successCount = 0;
        int skipCount = 0;
        int failCount = 0;
        List<string> failedFiles = new List<string>();
        
        // 각 파일 변환
        foreach (string osuFilePath in osuFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(osuFilePath);
            string synthFilePath = Path.Combine(synthFolderPath, fileName + ".synth");
            
            // 이미 존재하는 파일 체크
            if (File.Exists(synthFilePath))
            {
                Debug.Log($"⏭️ 건너뜀 (이미 존재): {fileName}");
                skipCount++;
                continue;
            }
            
            // 변환 시도
            bool success = ConvertSingleFileStatic(osuFilePath, synthFilePath);
            
            if (success)
            {
                successCount++;
            }
            else
            {
                failCount++;
                failedFiles.Add(fileName);
            }
        }
        
        // 최종 결과
        Debug.Log($"");
        Debug.Log($"=== 변환 완료 ===");
        Debug.Log($"✅ 성공: {successCount}개");
        Debug.Log($"⏭️ 건너뜀: {skipCount}개");
        Debug.Log($"❌ 실패: {failCount}개");
        
        if (failCount > 0)
        {
            Debug.LogWarning($"실패한 파일 목록:");
            foreach (string failedFile in failedFiles)
            {
                Debug.LogWarning($"  - {failedFile}");
            }
        }
        
        Debug.Log($"===================");
        
        // Unity Editor에서 Asset 갱신
        AssetDatabase.Refresh();
    }
    
    /// <summary>
    /// Static 버전 - 단일 파일 변환
    /// </summary>
    private static bool ConvertSingleFileStatic(string osuFilePath, string synthFilePath)
    {
        string fileName = Path.GetFileName(osuFilePath);
        
        try
        {
            Debug.Log($"🔄 변환 중: {fileName}");
            
            // 1. osu 파일 파싱
            ChartData osuChart = OsuManiaParser.ParseFromFile(osuFilePath);
            
            if (osuChart == null)
            {
                Debug.LogError($"❌ 파싱 실패: {fileName}");
                return false;
            }
            
            // 2. synth 파일 형식으로 저장
            bool saved = SaveChartDataToSynthStatic(osuChart, synthFilePath);
            
            if (saved)
            {
                Debug.Log($"✅ 변환 성공: {fileName} → {Path.GetFileName(synthFilePath)}");
                return true;
            }
            else
            {
                Debug.LogError($"❌ 저장 실패: {fileName}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 변환 오류: {fileName} - {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// Static 버전 - synth 파일 저장
    /// </summary>
    private static bool SaveChartDataToSynthStatic(ChartData chart, string filePath)
    {
        try
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // 헤더
            sb.AppendLine("# Synth Chart Format v1.0");
            sb.AppendLine($"# Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Converted from osu! mania beatmap");
            sb.AppendLine();
            
            // 메타데이터
            sb.AppendLine("[METADATA]");
            sb.AppendLine($"Title: {chart.songName}");
            sb.AppendLine($"Artist: {chart.artistName}");
            sb.AppendLine($"Audio: {chart.audioFileName}");
            sb.AppendLine($"Cover: {chart.coverImageFileName}");
            sb.AppendLine($"BPM: {chart.bpm}");
            sb.AppendLine($"Offset: {chart.offset}");
            sb.AppendLine();
            
            // 난이도 정보
            sb.AppendLine("[DIFFICULTY]");
            sb.AppendLine($"Name: {chart.difficulty}");
            sb.AppendLine($"Keys: {chart.keyCount}");
            sb.AppendLine($"Level: {chart.level:F1}");
            sb.AppendLine();
            
            // 차트 정보
            sb.AppendLine("[CHART_INFO]");
            sb.AppendLine($"Author: {chart.chartAuthor}");
            sb.AppendLine($"Created: {chart.createdDate}");
            sb.AppendLine($"Modified: {System.DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine($"Source: {chart.source}");
            sb.AppendLine($"Tags: {chart.tags}");
            sb.AppendLine();
            
            // 통계
            sb.AppendLine("[STATISTICS]");
            sb.AppendLine($"NoteCount: {chart.noteCount}");
            sb.AppendLine($"LongNoteCount: {chart.longNoteCount}");
            sb.AppendLine($"MaxCombo: {chart.maxCombo}");
            sb.AppendLine($"Density: {chart.density:F2}");
            sb.AppendLine();
            
            // 노트 데이터
            sb.AppendLine("[NOTES]");
            sb.AppendLine("# Format: timing, track, keysound, endtime(if long note)");
            
            if (chart.notes != null)
            {
                foreach (var note in chart.notes)
                {
                    if (note.isLongNote)
                    {
                        sb.AppendLine($"{note.timing:F3}, {note.track}, {note.keySoundType}, {note.longNoteEndTiming:F3}");
                    }
                    else
                    {
                        sb.AppendLine($"{note.timing:F3}, {note.track}, {note.keySoundType}");
                    }
                }
            }
            
            // 파일 저장
            File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveChartDataToSynth 오류: {e.Message}");
            return false;
        }
    }
#endif
    
    /// <summary>
    /// 모든 osu 맵을 synth 형식으로 변환
    /// </summary>
    [ContextMenu("Convert All Osu Maps")]
    public void ConvertAllOsuMaps()
    {
        string osuFolderPath = Path.Combine(Application.streamingAssetsPath, osuMapsFolder);
        string synthFolderPath = Path.Combine(Application.streamingAssetsPath, synthMapsFolder);
        
        Debug.Log($"=== osu → synth 일괄 변환 시작 ===");
        Debug.Log($"osu 폴더: {osuFolderPath}");
        Debug.Log($"synth 폴더: {synthFolderPath}");
        Debug.Log($"");
        
        // 폴더 존재 확인
        if (!Directory.Exists(osuFolderPath))
        {
            Debug.LogError($"❌ osu 맵 폴더가 존재하지 않습니다: {osuFolderPath}");
            return;
        }
        
        // synth 폴더 생성
        if (!Directory.Exists(synthFolderPath))
        {
            Directory.CreateDirectory(synthFolderPath);
            Debug.Log($"✅ synth 폴더 생성: {synthFolderPath}");
        }
        
        // 모든 .osu 파일 찾기
        string[] osuFiles = Directory.GetFiles(osuFolderPath, "*.osu", SearchOption.AllDirectories);
        
        if (osuFiles.Length == 0)
        {
            Debug.LogWarning($"⚠️ osu 파일을 찾을 수 없습니다.");
            return;
        }
        
        Debug.Log($"📁 발견된 osu 파일: {osuFiles.Length}개");
        Debug.Log($"");
        
        // 통계
        int successCount = 0;
        int skipCount = 0;
        int failCount = 0;
        List<string> failedFiles = new List<string>();
        
        // 각 파일 변환
        foreach (string osuFilePath in osuFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(osuFilePath);
            string synthFilePath = Path.Combine(synthFolderPath, fileName + ".synth");
            
            // 이미 존재하는 파일 체크
            if (File.Exists(synthFilePath) && !overwriteExisting)
            {
                if (verboseLogging)
                    Debug.Log($"⏭️ 건너뜀 (이미 존재): {fileName}");
                skipCount++;
                continue;
            }
            
            // 변환 시도
            bool success = ConvertSingleFile(osuFilePath, synthFilePath);
            
            if (success)
            {
                successCount++;
                
                // 원본 삭제 옵션
                if (deleteOsuFilesAfterConversion)
                {
                    try
                    {
                        File.Delete(osuFilePath);
                        if (verboseLogging)
                            Debug.Log($"🗑️ 원본 삭제: {fileName}.osu");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"⚠️ 원본 삭제 실패: {fileName}.osu - {e.Message}");
                    }
                }
            }
            else
            {
                failCount++;
                failedFiles.Add(fileName);
            }
        }
        
        // 최종 결과
        Debug.Log($"");
        Debug.Log($"=== 변환 완료 ===");
        Debug.Log($"✅ 성공: {successCount}개");
        Debug.Log($"⏭️ 건너뜀: {skipCount}개");
        Debug.Log($"❌ 실패: {failCount}개");
        
        if (failCount > 0)
        {
            Debug.LogWarning($"실패한 파일 목록:");
            foreach (string failedFile in failedFiles)
            {
                Debug.LogWarning($"  - {failedFile}");
            }
        }
        
        Debug.Log($"===================");
        
#if UNITY_EDITOR
        // Unity Editor에서 Asset 갱신
        AssetDatabase.Refresh();
#endif
    }
    
    /// <summary>
    /// 단일 osu 파일을 synth 파일로 변환
    /// </summary>
    private bool ConvertSingleFile(string osuFilePath, string synthFilePath)
    {
        string fileName = Path.GetFileName(osuFilePath);
        
        try
        {
            if (verboseLogging)
                Debug.Log($"🔄 변환 중: {fileName}");
            
            // 1. osu 파일 파싱
            ChartData osuChart = OsuManiaParser.ParseFromFile(osuFilePath);
            
            if (osuChart == null)
            {
                Debug.LogError($"❌ 파싱 실패: {fileName}");
                return false;
            }
            
            // 2. synth 파일 형식으로 저장
            bool saved = SaveChartDataToSynth(osuChart, synthFilePath);
            
            if (saved)
            {
                if (verboseLogging)
                    Debug.Log($"✅ 변환 성공: {fileName} → {Path.GetFileName(synthFilePath)}");
                return true;
            }
            else
            {
                Debug.LogError($"❌ 저장 실패: {fileName}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 변환 오류: {fileName} - {e.Message}");
            if (verboseLogging)
                Debug.LogError($"스택 트레이스: {e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// ChartData를 .synth 파일로 저장
    /// </summary>
    private bool SaveChartDataToSynth(ChartData chart, string filePath)
    {
        try
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // 헤더
            sb.AppendLine("# Synth Chart Format v1.0");
            sb.AppendLine($"# Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Converted from osu! mania beatmap");
            sb.AppendLine();
            
            // 메타데이터
            sb.AppendLine("[METADATA]");
            sb.AppendLine($"Title: {chart.songName}");
            sb.AppendLine($"Artist: {chart.artistName}");
            sb.AppendLine($"Audio: {chart.audioFileName}");
            sb.AppendLine($"Cover: {chart.coverImageFileName}");
            sb.AppendLine($"BPM: {chart.bpm}");
            sb.AppendLine($"Offset: {chart.offset}");
            sb.AppendLine();
            
            // 난이도 정보
            sb.AppendLine("[DIFFICULTY]");
            sb.AppendLine($"Name: {chart.difficulty}");
            sb.AppendLine($"Keys: {chart.keyCount}");
            sb.AppendLine($"Level: {chart.level:F1}");
            sb.AppendLine();
            
            // 차트 정보
            sb.AppendLine("[CHART_INFO]");
            sb.AppendLine($"Author: {chart.chartAuthor}");
            sb.AppendLine($"Created: {chart.createdDate}");
            sb.AppendLine($"Modified: {System.DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine($"Source: {chart.source}");
            sb.AppendLine($"Tags: {chart.tags}");
            sb.AppendLine();
            
            // 통계
            sb.AppendLine("[STATISTICS]");
            sb.AppendLine($"NoteCount: {chart.noteCount}");
            sb.AppendLine($"LongNoteCount: {chart.longNoteCount}");
            sb.AppendLine($"MaxCombo: {chart.maxCombo}");
            sb.AppendLine($"Density: {chart.density:F2}");
            sb.AppendLine();
            
            // 노트 데이터
            sb.AppendLine("[NOTES]");
            sb.AppendLine("# Format: timing, track, keysound, endtime(if long note)");
            
            foreach (var note in chart.notes)
            {
                if (note.isLongNote)
                {
                    sb.AppendLine($"{note.timing:F3}, {note.track}, {note.keySoundType}, {note.longNoteEndTiming:F3}");
                }
                else
                {
                    sb.AppendLine($"{note.timing:F3}, {note.track}, {note.keySoundType}");
                }
            }
            
            // 파일 저장
            File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveChartDataToSynth 오류: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// ChartData를 검증 (사용하지 않음, 호환성을 위해 유지)
    /// </summary>
    private ChartData ConvertToNewFormat(ChartData oldChart)
    {
        return oldChart;
    }
}
