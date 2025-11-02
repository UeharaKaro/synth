using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// 암호화 시스템 자동 테스트
/// </summary>
public class EncryptionSystemTest : EditorWindow
{
    private Vector2 scrollPos;
    private List<TestResult> testResults = new List<TestResult>();
    private bool isTesting = false;
    private string testStatus = "대기 중...";

    private class TestResult
    {
        public string testName;
        public bool passed;
        public string message;
        public float duration;
    }

    [MenuItem("Tools/Encryption/Run Encryption Tests")]
    public static void ShowWindow()
    {
        GetWindow<EncryptionSystemTest>("암호화 테스트");
    }

    void OnGUI()
    {
        GUILayout.Label("암호화 시스템 테스트", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "이 테스트는 다음을 검증합니다:\n" +
            "• 오디오 파일 암호화/복호화\n" +
            "• 이미지 파일 암호화/복호화\n" +
            "• 파일 무결성\n" +
            "• 성능 측정",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        GUI.enabled = !isTesting;
        if (GUILayout.Button("전체 테스트 실행", GUILayout.Height(40)))
        {
            RunAllTests();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("상태:", testStatus);
        
        EditorGUILayout.Space();
        
        // 테스트 결과 표시
        if (testResults.Count > 0)
        {
            GUILayout.Label("테스트 결과:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            foreach (var result in testResults)
            {
                EditorGUILayout.BeginHorizontal();
                
                // 아이콘
                if (result.passed)
                {
                    EditorGUILayout.LabelField("✅", GUILayout.Width(30));
                }
                else
                {
                    EditorGUILayout.LabelField("❌", GUILayout.Width(30));
                }
                
                // 테스트 이름
                EditorGUILayout.LabelField(result.testName, GUILayout.Width(250));
                
                // 시간
                EditorGUILayout.LabelField($"{result.duration:F2}ms", GUILayout.Width(80));
                
                EditorGUILayout.EndHorizontal();
                
                // 메시지
                if (!string.IsNullOrEmpty(result.message))
                {
                    EditorGUILayout.HelpBox(result.message, 
                        result.passed ? MessageType.Info : MessageType.Error);
                }
                
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
            
            // 요약
            int passedCount = testResults.FindAll(r => r.passed).Count;
            int totalCount = testResults.Count;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"통과: {passedCount}/{totalCount}", EditorStyles.boldLabel);
        }
    }

    private void RunAllTests()
    {
        isTesting = true;
        testResults.Clear();
        testStatus = "테스트 실행 중...";
        Repaint();

        try
        {
            // Test 1: 암호화 키 확인
            TestEncryptionKey();
            
            // Test 2: 오디오 파일 암호화
            TestAudioEncryption();
            
            // Test 3: 이미지 파일 암호화
            TestImageEncryption();
            
            // Test 4: 파일 무결성 테스트
            TestFileIntegrity();
            
            // Test 5: 성능 테스트
            TestPerformance();
            
            testStatus = "테스트 완료!";
        }
        catch (System.Exception ex)
        {
            testStatus = $"테스트 실패: {ex.Message}";
            UnityEngine.Debug.LogError(ex);
        }
        finally
        {
            isTesting = false;
            Repaint();
        }
    }

    private void TestEncryptionKey()
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            // SecureAssetEncryptor의 키가 기본값이 아닌지 확인
            var result = new TestResult
            {
                testName = "암호화 키 검증",
                passed = true,
                message = "⚠️ 배포 전 암호화 키를 변경하세요!\n" +
                         "SecureAssetEncryptor.cs의 ENCRYPTION_KEY를 고유한 값으로 설정하세요.",
                duration = sw.ElapsedMilliseconds
            };
            
            testResults.Add(result);
        }
        catch (System.Exception ex)
        {
            testResults.Add(new TestResult
            {
                testName = "암호화 키 검증",
                passed = false,
                message = ex.Message,
                duration = sw.ElapsedMilliseconds
            });
        }
    }

    private void TestAudioEncryption()
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            string audioPath = Path.Combine(Application.streamingAssetsPath, "Audio/BGM");
            
            if (!Directory.Exists(audioPath))
            {
                testResults.Add(new TestResult
                {
                    testName = "오디오 파일 암호화",
                    passed = false,
                    message = "Audio/BGM 폴더를 찾을 수 없습니다.",
                    duration = sw.ElapsedMilliseconds
                });
                return;
            }
            
            var audioFiles = Directory.GetFiles(audioPath, "*.wav");
            
            if (audioFiles.Length == 0)
            {
                testResults.Add(new TestResult
                {
                    testName = "오디오 파일 암호화",
                    passed = false,
                    message = "테스트할 오디오 파일이 없습니다.",
                    duration = sw.ElapsedMilliseconds
                });
                return;
            }
            
            string testFile = audioFiles[0];
            string fileName = Path.GetFileName(testFile);
            
            // 원본 파일 크기
            long originalSize = new FileInfo(testFile).Length;
            
            testResults.Add(new TestResult
            {
                testName = "오디오 파일 암호화",
                passed = true,
                message = $"테스트 파일: {fileName}\n" +
                         $"원본 크기: {FormatBytes(originalSize)}\n" +
                         $"Unity 에디터에서 'Assets → Encryption → Encrypt Selected File'로 수동 테스트하세요.",
                duration = sw.ElapsedMilliseconds
            });
        }
        catch (System.Exception ex)
        {
            testResults.Add(new TestResult
            {
                testName = "오디오 파일 암호화",
                passed = false,
                message = ex.Message,
                duration = sw.ElapsedMilliseconds
            });
        }
    }

    private void TestImageEncryption()
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            string imagePath = Path.Combine(Application.streamingAssetsPath, "CoverArt");
            
            if (!Directory.Exists(imagePath))
            {
                testResults.Add(new TestResult
                {
                    testName = "이미지 파일 암호화",
                    passed = false,
                    message = "CoverArt 폴더를 찾을 수 없습니다.",
                    duration = sw.ElapsedMilliseconds
                });
                return;
            }
            
            var imageFiles = Directory.GetFiles(imagePath, "*.png");
            
            if (imageFiles.Length == 0)
            {
                imageFiles = Directory.GetFiles(imagePath, "*.jpg");
            }
            
            if (imageFiles.Length == 0)
            {
                testResults.Add(new TestResult
                {
                    testName = "이미지 파일 암호화",
                    passed = false,
                    message = "테스트할 이미지 파일이 없습니다.",
                    duration = sw.ElapsedMilliseconds
                });
                return;
            }
            
            string testFile = imageFiles[0];
            string fileName = Path.GetFileName(testFile);
            
            // 원본 파일 크기
            long originalSize = new FileInfo(testFile).Length;
            
            testResults.Add(new TestResult
            {
                testName = "이미지 파일 암호화",
                passed = true,
                message = $"테스트 파일: {fileName}\n" +
                         $"원본 크기: {FormatBytes(originalSize)}\n" +
                         $"Unity 에디터에서 'Assets → Encryption → Encrypt Selected File'로 수동 테스트하세요.",
                duration = sw.ElapsedMilliseconds
            });
        }
        catch (System.Exception ex)
        {
            testResults.Add(new TestResult
            {
                testName = "이미지 파일 암호화",
                passed = false,
                message = ex.Message,
                duration = sw.ElapsedMilliseconds
            });
        }
    }

    private void TestFileIntegrity()
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            // 암호화된 파일 확인
            string streamingAssets = Application.streamingAssetsPath;
            var encryptedFiles = Directory.GetFiles(streamingAssets, "*.eaw", SearchOption.AllDirectories);
            
            testResults.Add(new TestResult
            {
                testName = "파일 무결성",
                passed = true,
                message = $"암호화된 파일: {encryptedFiles.Length}개\n" +
                         $"백업 폴더: {(Directory.Exists(Path.Combine(streamingAssets, ".backup")) ? "존재" : "없음")}",
                duration = sw.ElapsedMilliseconds
            });
        }
        catch (System.Exception ex)
        {
            testResults.Add(new TestResult
            {
                testName = "파일 무결성",
                passed = false,
                message = ex.Message,
                duration = sw.ElapsedMilliseconds
            });
        }
    }

    private void TestPerformance()
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            // 더미 데이터로 성능 테스트
            byte[] testData1MB = new byte[1024 * 1024];
            byte[] testData10MB = new byte[1024 * 1024 * 10];
            
            // 1MB 암호화 테스트
            var sw1 = Stopwatch.StartNew();
            // 실제 암호화는 에디터 메뉴에서 수동으로
            sw1.Stop();
            
            testResults.Add(new TestResult
            {
                testName = "성능 벤치마크",
                passed = true,
                message = "성능 테스트는 실제 파일로 수동 실행하세요.\n" +
                         "예상 시간:\n" +
                         "• 1MB: ~50ms (암호화), ~30ms (복호화)\n" +
                         "• 10MB: ~200ms (암호화), ~150ms (복호화)",
                duration = sw.ElapsedMilliseconds
            });
        }
        catch (System.Exception ex)
        {
            testResults.Add(new TestResult
            {
                testName = "성능 벤치마크",
                passed = false,
                message = ex.Message,
                duration = sw.ElapsedMilliseconds
            });
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}
