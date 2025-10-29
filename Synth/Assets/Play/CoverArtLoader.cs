using UnityEngine;
using System.IO;
using System.Collections;

/// <summary>
/// 커버 이미지 로더 - StreamingAssets에서 커버 아트를 동적으로 로드
/// </summary>
public class CoverArtLoader : MonoBehaviour
{
    // Singleton
    public static CoverArtLoader Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private string coverArtFolder = "CoverArt"; // StreamingAssets 내 폴더명
    [SerializeField] private Sprite defaultCoverArt; // 기본 커버 이미지 (없을 때 사용)

    // 캐시
    private System.Collections.Generic.Dictionary<string, Sprite> coverCache = 
        new System.Collections.Generic.Dictionary<string, Sprite>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 커버 이미지 로드 (동기)
    /// </summary>
    /// <param name="fileName">이미지 파일명 (예: "sample_audio.png")</param>
    /// <returns>로드된 Sprite 또는 기본 이미지</returns>
    public Sprite LoadCoverArt(string fileName)
    {
        // 빈 파일명이면 기본 이미지 반환
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("CoverArtLoader: 파일명이 비어있습니다. 기본 이미지 사용");
            return defaultCoverArt;
        }

        // 캐시에 있으면 바로 반환
        if (coverCache.ContainsKey(fileName))
        {
            return coverCache[fileName];
        }

        // 파일 경로 생성
        string basePath = Path.Combine(Application.streamingAssetsPath, coverArtFolder);
        string filePath = Path.Combine(basePath, fileName);
        
        // 암호화된 파일(.eaw) 우선 확인
        string encryptedPath = Path.ChangeExtension(filePath, ".eaw");
        
        byte[] fileData = null;
        
        if (File.Exists(encryptedPath))
        {
            // 암호화된 이미지 로드
            try
            {
                Debug.Log($"CoverArtLoader: 암호화된 이미지 로드 시도 - {encryptedPath}");
                byte[] encryptedData = File.ReadAllBytes(encryptedPath);
                fileData = SecureAssetLoader.DecryptImageData(encryptedData);
                
                if (fileData == null || fileData.Length == 0)
                {
                    Debug.LogError("CoverArtLoader: 복호화된 데이터가 비어있습니다");
                    return defaultCoverArt;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CoverArtLoader: 암호화된 이미지 복호화 실패 - {e.Message}");
                return defaultCoverArt;
            }
        }
        else if (File.Exists(filePath))
        {
            // 일반 파일 로드
            Debug.Log($"CoverArtLoader: 일반 이미지 로드 시도 - {filePath}");
            fileData = File.ReadAllBytes(filePath);
        }
        else
        {
            Debug.LogWarning($"CoverArtLoader: 커버 이미지를 찾을 수 없습니다 - {fileName}");
            return defaultCoverArt;
        }

        try
        {
            // Texture2D 생성
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                // Sprite 생성
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                // 캐시에 저장
                coverCache[fileName] = sprite;

                Debug.Log($"CoverArtLoader: 커버 이미지 로드 성공 - {fileName}");
                return sprite;
            }
            else
            {
                Debug.LogError($"CoverArtLoader: 이미지 디코딩 실패 - {fileName}");
                return defaultCoverArt;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CoverArtLoader: 이미지 로드 중 오류 - {e.Message}");
            return defaultCoverArt;
        }
    }

    /// <summary>
    /// 커버 이미지 로드 (비동기 코루틴)
    /// </summary>
    public IEnumerator LoadCoverArtAsync(string fileName, System.Action<Sprite> onComplete)
    {
        // 빈 파일명이면 기본 이미지 반환
        if (string.IsNullOrEmpty(fileName))
        {
            onComplete?.Invoke(defaultCoverArt);
            yield break;
        }

        // 캐시에 있으면 바로 반환
        if (coverCache.ContainsKey(fileName))
        {
            onComplete?.Invoke(coverCache[fileName]);
            yield break;
        }

        // 파일 경로
        string filePath = Path.Combine(Application.streamingAssetsPath, coverArtFolder, fileName);

        // 파일 존재 확인
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"CoverArtLoader: 커버 이미지를 찾을 수 없습니다 - {filePath}");
            onComplete?.Invoke(defaultCoverArt);
            yield break;
        }

        // 백그라운드 로드 (프레임 드랍 방지)
        byte[] fileData = null;
        System.Threading.Tasks.Task<byte[]> loadTask = System.Threading.Tasks.Task.Run(() => File.ReadAllBytes(filePath));

        // 로딩 대기
        while (!loadTask.IsCompleted)
        {
            yield return null;
        }

        fileData = loadTask.Result;

        // Texture 생성 (메인 스레드에서 실행 필요)
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            // 캐시 저장
            coverCache[fileName] = sprite;

            Debug.Log($"CoverArtLoader: 커버 이미지 비동기 로드 성공 - {fileName}");
            onComplete?.Invoke(sprite);
        }
        else
        {
            Debug.LogError($"CoverArtLoader: 이미지 디코딩 실패 - {fileName}");
            onComplete?.Invoke(defaultCoverArt);
        }
    }

    /// <summary>
    /// ChartData로부터 커버 이미지 로드
    /// </summary>
    public Sprite LoadCoverArtFromChart(ChartData chart)
    {
        if (chart == null)
        {
            Debug.LogWarning("CoverArtLoader: ChartData가 null입니다");
            return defaultCoverArt;
        }

        // coverImageFileName이 지정되어 있으면 그걸 사용
        if (!string.IsNullOrEmpty(chart.coverImageFileName))
        {
            return LoadCoverArt(chart.coverImageFileName);
        }

        // 지정 안 되어 있으면 오디오 파일명 기반으로 추론
        // 예: "sample_audio.wav" → "sample_audio.png"
        if (!string.IsNullOrEmpty(chart.audioFileName))
        {
            string baseFileName = Path.GetFileNameWithoutExtension(chart.audioFileName);
            
            // PNG 우선 시도
            string pngFileName = baseFileName + ".png";
            if (File.Exists(Path.Combine(Application.streamingAssetsPath, coverArtFolder, pngFileName)))
            {
                return LoadCoverArt(pngFileName);
            }

            // JPG 시도
            string jpgFileName = baseFileName + ".jpg";
            if (File.Exists(Path.Combine(Application.streamingAssetsPath, coverArtFolder, jpgFileName)))
            {
                return LoadCoverArt(jpgFileName);
            }
        }

        Debug.LogWarning($"CoverArtLoader: {chart.songName}의 커버 이미지를 찾을 수 없습니다. 기본 이미지 사용");
        return defaultCoverArt;
    }

    /// <summary>
    /// SongData로부터 커버 이미지를 비동기로 로드하는 코루틴
    /// </summary>
    public IEnumerator LoadCoverArtCoroutine(SongData songData, System.Action<Texture2D> onComplete)
    {
        if (songData == null)
        {
            Debug.LogWarning("CoverArtLoader: SongData가 null입니다");
            onComplete?.Invoke(null);
            yield break;
        }

        // 파일명 결정
        string fileName = null;

        // 1. audioFileName이 있으면 그것을 기반으로
        if (!string.IsNullOrEmpty(songData.audioFileName))
        {
            string baseFileName = Path.GetFileNameWithoutExtension(songData.audioFileName);
            
            // PNG 우선 시도
            string pngFileName = baseFileName + ".png";
            string pngPath = Path.Combine(Application.streamingAssetsPath, coverArtFolder, pngFileName);
            if (File.Exists(pngPath))
            {
                fileName = pngFileName;
            }
            else
            {
                // JPG 시도
                string jpgFileName = baseFileName + ".jpg";
                string jpgPath = Path.Combine(Application.streamingAssetsPath, coverArtFolder, jpgFileName);
                if (File.Exists(jpgPath))
                {
                    fileName = jpgFileName;
                }
            }
        }

        // 2. audioPath가 있으면 시도
        if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(songData.audioPath))
        {
            string baseFileName = Path.GetFileNameWithoutExtension(songData.audioPath);
            
            string pngFileName = baseFileName + ".png";
            string pngPath = Path.Combine(Application.streamingAssetsPath, coverArtFolder, pngFileName);
            if (File.Exists(pngPath))
            {
                fileName = pngFileName;
            }
            else
            {
                string jpgFileName = baseFileName + ".jpg";
                string jpgPath = Path.Combine(Application.streamingAssetsPath, coverArtFolder, jpgFileName);
                if (File.Exists(jpgPath))
                {
                    fileName = jpgFileName;
                }
            }
        }

        // 파일명을 찾지 못했으면 null 반환
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning($"CoverArtLoader: {songData.title}의 커버 이미지를 찾을 수 없습니다");
            onComplete?.Invoke(null);
            yield break;
        }

        // 캐시 확인
        if (coverCache.ContainsKey(fileName))
        {
            Sprite cachedSprite = coverCache[fileName];
            if (cachedSprite != null && cachedSprite.texture != null)
            {
                onComplete?.Invoke(cachedSprite.texture);
                yield break;
            }
        }

        // 파일 로드
        string filePath = Path.Combine(Application.streamingAssetsPath, coverArtFolder, fileName);
        
        byte[] fileData = null;
        System.Threading.Tasks.Task<byte[]> loadTask = System.Threading.Tasks.Task.Run(() => File.ReadAllBytes(filePath));

        // 로딩 대기
        while (!loadTask.IsCompleted)
        {
            yield return null;
        }

        try
        {
            fileData = loadTask.Result;

            // Texture 생성
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                // Sprite도 캐시에 저장
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                coverCache[fileName] = sprite;

                Debug.Log($"CoverArtLoader: 커버 이미지 로드 성공 - {fileName}");
                onComplete?.Invoke(texture);
            }
            else
            {
                Debug.LogError($"CoverArtLoader: 이미지 디코딩 실패 - {fileName}");
                onComplete?.Invoke(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CoverArtLoader: 이미지 로드 중 오류 - {e.Message}");
            onComplete?.Invoke(null);
        }
    }

    /// <summary>
    /// 캐시 초기화
    /// </summary>
    public void ClearCache()
    {
        foreach (var kvp in coverCache)
        {
            if (kvp.Value != null && kvp.Value.texture != null)
            {
                Destroy(kvp.Value.texture);
            }
        }
        coverCache.Clear();
        Debug.Log("CoverArtLoader: 캐시 초기화 완료");
    }

    void OnDestroy()
    {
        ClearCache();
    }
}
