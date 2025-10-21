using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour // 트랙 높이,기울기, 투명도 관리
{
    [Header("Track References")]
    [SerializeField] private List<Transform> trackLines = new List<Transform>();
    [SerializeField] private List<LineRenderer> trackLineRenderers = new List<LineRenderer>();
    
    [Header("Track Settings")]
    [SerializeField] private Material trackMaterial;
    [SerializeField] private float baseTrackWidth = 0.1f;
    [SerializeField] private int trackCount = 4;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoCreateTracks = true;
    [SerializeField] private float trackSpacing = 2f;
    
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    
    private void Start()
    {
        InitializeTracks();
        
        // 설정 변경 이벤트 구독
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged += ApplyTrackSettings;
            ApplyTrackSettings();
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged -= ApplyTrackSettings;
        }
    }
    
    private void InitializeTracks()
    {
        // 자동으로 트랙 생성
        if (autoCreateTracks && trackLines.Count == 0)
        {
            CreateTracks();
        }
        
        // 원본 위치와 스케일 저장
        originalPositions = new Vector3[trackLines.Count];
        originalScales = new Vector3[trackLines.Count];
        
        for (int i = 0; i < trackLines.Count; i++)
        {
            if (trackLines[i] != null)
            {
                originalPositions[i] = trackLines[i].localPosition;
                originalScales[i] = trackLines[i].localScale;
            }
        }
    }
    
    private void CreateTracks()
    {
        for (int i = 0; i < trackCount; i++)
        {
            // 트랙 라인 오브젝트 생성
            GameObject trackLine = new GameObject($"TrackLine_{i}");
            trackLine.transform.SetParent(transform);
            
            // 위치 설정
            float xPos = (i - (trackCount - 1) * 0.5f) * trackSpacing;
            trackLine.transform.localPosition = new Vector3(xPos, 0f, 0f);
            
            // LineRenderer 컴포넌트 추가
            LineRenderer lr = trackLine.AddComponent<LineRenderer>();
            lr.material = trackMaterial ? trackMaterial : CreateDefaultMaterial();
            lr.startWidth = baseTrackWidth;
            lr.endWidth = baseTrackWidth;
            lr.positionCount = 2;
            
            // 라인 포지션 설정 (세로 라인)
            lr.SetPosition(0, new Vector3(0, -10f, 0));
            lr.SetPosition(1, new Vector3(0, 10f, 0));
            
            trackLines.Add(trackLine.transform);
            trackLineRenderers.Add(lr);
        }
    }
    
    private Material CreateDefaultMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        return mat;
    }
    
    private void ApplyTrackSettings()
    {
        if (SettingsManager.Instance == null) return;
        
        var settings = SettingsManager.Instance.Settings;
        
        for (int i = 0; i < trackLines.Count; i++)
        {
            if (trackLines[i] == null) continue;
            
            ApplyTrackHeight(i, settings.trackHeight);
            ApplyTrackAngle(i, settings.trackAngle);
            ApplyTrackOpacity(i, settings.trackOpacity);
        }
        
        Debug.Log($"Track settings applied - Height: {settings.trackHeight:F1}, Angle: {settings.trackAngle:F1}°, Opacity: {settings.trackOpacity:F2}");
    }
    
    private void ApplyTrackHeight(int trackIndex, float height)
    {
        if (trackIndex >= trackLines.Count || trackLines[trackIndex] == null) return;
        
        Transform track = trackLines[trackIndex];
        
        // 스케일을 통한 높이 조정
        Vector3 newScale = originalScales[trackIndex];
        newScale.y = height / 10f; // 기본 높이 10을 기준으로 스케일링
        track.localScale = newScale;
        
        // LineRenderer가 있는 경우 포지션도 조정
        if (trackIndex < trackLineRenderers.Count && trackLineRenderers[trackIndex] != null)
        {
            LineRenderer lr = trackLineRenderers[trackIndex];
            float halfHeight = height * 0.5f;
            lr.SetPosition(0, new Vector3(0, -halfHeight, 0));
            lr.SetPosition(1, new Vector3(0, halfHeight, 0));
        }
    }
    
    private void ApplyTrackAngle(int trackIndex, float angle)
    {
        if (trackIndex >= trackLines.Count || trackLines[trackIndex] == null) return;
        
        Transform track = trackLines[trackIndex];
        
        // Z축 회전 적용
        Vector3 rotation = track.localEulerAngles;
        rotation.z = angle;
        track.localEulerAngles = rotation;
    }
    
    private void ApplyTrackOpacity(int trackIndex, float opacity)
    {
        if (trackIndex >= trackLineRenderers.Count || trackLineRenderers[trackIndex] == null) return;
        
        LineRenderer lr = trackLineRenderers[trackIndex];
        
        // 머티리얼 색상의 알파값 조정
        if (lr.material != null)
        {
            Color color = lr.material.color;
            color.a = opacity;
            lr.material.color = color;
        }
    }
    
    // 트랙 위치 가져오기 (노트 생성시 사용)
    public Vector3 GetTrackPosition(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < trackLines.Count && trackLines[trackIndex] != null)
        {
            return trackLines[trackIndex].position;
        }
        return Vector3.zero;
    }
    
    // 트랙 개수 가져오기
    public int GetTrackCount()
    {
        return trackLines.Count;
    }
    
    // 트랙 추가/제거 (런타임에서)
    public void AddTrack()
    {
        if (trackLines.Count >= 8) return; // 최대 8개 트랙
        
        int newIndex = trackLines.Count;
        GameObject trackLine = new GameObject($"TrackLine_{newIndex}");
        trackLine.transform.SetParent(transform);
        
        float xPos = (newIndex - (trackLines.Count - 1) * 0.5f) * trackSpacing;
        trackLine.transform.localPosition = new Vector3(xPos, 0f, 0f);
        
        LineRenderer lr = trackLine.AddComponent<LineRenderer>();
        lr.material = trackMaterial ? trackMaterial : CreateDefaultMaterial();
        lr.startWidth = baseTrackWidth;
        lr.endWidth = baseTrackWidth;
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(0, -10f, 0));
        lr.SetPosition(1, new Vector3(0, 10f, 0));
        
        trackLines.Add(trackLine.transform);
        trackLineRenderers.Add(lr);
        
        // 원본 데이터도 업데이트
        System.Array.Resize(ref originalPositions, trackLines.Count);
        System.Array.Resize(ref originalScales, trackLines.Count);
        originalPositions[newIndex] = trackLine.transform.localPosition;
        originalScales[newIndex] = trackLine.transform.localScale;
        
        ApplyTrackSettings();
    }
    
    public void RemoveTrack()
    {
        if (trackLines.Count <= 1) return; // 최소 1개 트랙 유지
        
        int lastIndex = trackLines.Count - 1;
        if (trackLines[lastIndex] != null)
        {
            DestroyImmediate(trackLines[lastIndex].gameObject);
        }
        
        trackLines.RemoveAt(lastIndex);
        trackLineRenderers.RemoveAt(lastIndex);
        
        // 원본 데이터도 업데이트
        System.Array.Resize(ref originalPositions, trackLines.Count);
        System.Array.Resize(ref originalScales, trackLines.Count);
    }
}