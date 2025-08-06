using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    // 다른 스크립트에서 GameSettingsManager.Instance 로 쉽게 접근할 수 있도록 설정
    public static GameSettingsManager Instance { get; private set; }

    // 현재 선택된 판정 모드. 기본값은 Normal
    public JudgmentMode CurrentMode { get; set; } = JudgmentMode.JudgmentMode_Normal;

    // Awkae는 씬이 로드될 때 가장 먼저 실행되는 함수 중 하나
    private void Awake()
    {
        // 싱글톤 패턴 구현
        // 만약 이미 Instance가 존재하고, 그게 이 객체가 아니라면
        if (Instance != null && Instance != this)
        {
            // 이 객체를 파괴하고, 기존의 것을 계속 사용
            // 이렇게 하면 씬을 다시 로드해도 설정 유지 가능
            Destroy(gameObject);
            return; 
        }

        // Instance가 없다면, 이 객체를 Instance로 지정
        Instance = this;

        // 다른 씬으로 넘억도 이 게임 오브젝트가 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }
}   