using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// HP/게이지 시스템 관리 클래스
/// 판정에 따른 HP 증감, 난이도별 차별화, 게임오버 처리 담당
/// </summary>
public class HPSystem : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float startingHP = 50f; // 시작 HP (50%에서 시작)

    [Header("Judgment HP Values")] // 임시 값 유저와 조율후 변경 예정
    [Tooltip("S_Perfect 판정 HP 증가량")]
    [SerializeField] private float sPerfectHP = 2f;

    [Tooltip("Perfect 판정 HP 증가량")]
    [SerializeField] private float perfectHP = 1.5f;

    [Tooltip("Great 판정 HP 증가량")]
    [SerializeField] private float greatHP = 1f;

    [Tooltip("Good 판정 HP 증가량")]
    [SerializeField] private float goodHP = 0f;

    [Tooltip("Bad 판정 HP 감소량 (음수로 저장)")]
    [SerializeField] private float badHP = -2f;

    [Tooltip("Miss 판정 HP 감소량 (음수로 저장)")]
    [SerializeField] private float missHP = -5f;

    [Header("Difficulty Multipliers")]
    [Tooltip("Normal 모드 HP 감소 배율")]
    [SerializeField] private float normalDrainMultiplier = 1.0f;

    [Tooltip("Hard 모드 HP 감소 배율")]
    [SerializeField] private float hardDrainMultiplier = 1.5f;

    [Tooltip("Super 모드 HP 감소 배율")]
    [SerializeField] private float superDrainMultiplier = 2.0f;

    [Header("Clear Conditions")]
    [Tooltip("Normal 모드 클리어 HP 기준 (%)")]
    [SerializeField] private float normalClearThreshold = 70f;

    [Tooltip("Hard 모드 클리어 HP 기준 (%)")] // 47-51 번줄 조정예정
    [SerializeField] private float hardClearThreshold = 80f;

    [Tooltip("Super 모드 클리어 HP 기준 (%)")]
    [SerializeField] private float superClearThreshold = 90f;

    [Header("References")]
    [SerializeField] private GearController gearController;

    [Header("Events")]
    public UnityEvent OnGameOver;
    public UnityEvent OnGameClear;

    // Private variables
    private float currentHP;
    private JudgmentMode currentMode;
    private bool isGameOver = false;

    // Singleton pattern
    public static HPSystem Instance { get; private set; }

    void Awake()
    {
        // Singleton 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("HPSystem: Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        // GearController 자동 찾기
        if (gearController == null)
        {
            gearController = FindObjectOfType<GearController>();
            if (gearController == null)
            {
                Debug.LogError("HPSystem: GearController를 찾을 수 없습니다!");
            }
        }
    }

    void Start()
    {
        InitializeHP();
        LoadGameMode();
    }

    /// <summary>
    /// HP 시스템 초기화
    /// </summary>
    public void InitializeHP()
    {
        currentHP = startingHP;
        isGameOver = false;
        UpdateHPDisplay();
    }

    /// <summary>
    /// 게임 모드 로드
    /// </summary>
    private void LoadGameMode()
    {
        if (JudgmentModeManager.Instance != null)
        {
            currentMode = JudgmentModeManager.Instance.CurrentMode;
            Debug.Log($"HPSystem: 현재 모드 - {currentMode}");
        }
        else
        {
            currentMode = JudgmentMode.JudgmentMode_Normal;
            Debug.LogWarning("HPSystem: JudgmentModeManager를 찾을 수 없어 기본 모드(Normal)로 설정합니다.");
        }
    }

    /// <summary>
    /// 판정에 따라 HP 업데이트
    /// </summary>
    /// <param name="judgment">판정 타입</param>
    public void ProcessJudgment(JudgmentType judgment)
    {
        if (isGameOver) return;

        float hpChange = GetHPChange(judgment);

        // 감소량에는 난이도 배율 적용
        if (hpChange < 0)
        {
            float drainMultiplier = GetDrainMultiplier();
            hpChange *= drainMultiplier;
        }

        // HP 적용
        currentHP = Mathf.Clamp(currentHP + hpChange, 0, maxHP);

        // UI 업데이트
        UpdateHPDisplay();

        // 게임오버 체크
        if (currentHP <= 0)
        {
            TriggerGameOver();
        }

        // 디버그 로그
        Debug.Log($"HPSystem: {judgment} 판정 → HP {hpChange:+0.0;-0.0} (현재 HP: {currentHP}/{maxHP})");
    }

    /// <summary>
    /// 판정별 HP 증감량 반환
    /// </summary>
    private float GetHPChange(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.S_Perfect:
                return sPerfectHP;

            case JudgmentType.Perfect:
                return perfectHP;

            case JudgmentType.Great:
                return greatHP;

            case JudgmentType.Good:
                return goodHP;

            case JudgmentType.Bad:
                return badHP;

            case JudgmentType.Miss:
                return missHP;

            default:
                Debug.LogWarning($"HPSystem: 알 수 없는 판정 타입 - {judgment}");
                return 0f;
        }
    }

    /// <summary>
    /// 난이도별 HP 감소 배율 반환
    /// </summary>
    private float GetDrainMultiplier()
    {
        switch (currentMode)
        {
            case JudgmentMode.JudgmentMode_Normal:
                return normalDrainMultiplier;

            case JudgmentMode.JudgmentMode_Hard:
                return hardDrainMultiplier;

            case JudgmentMode.JudgmentMode_Super:
                return superDrainMultiplier;

            default:
                return normalDrainMultiplier;
        }
    }

    /// <summary>
    /// HP 게이지 UI 업데이트
    /// </summary>
    private void UpdateHPDisplay()
    {
        if (gearController != null)
        {
            gearController.UpdateHP(currentHP);
        }
    }

    /// <summary>
    /// 게임오버 처리
    /// </summary>
    private void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("HPSystem: 게임오버! HP가 0에 도달했습니다.");

        // 게임오버 이벤트 발생
        OnGameOver?.Invoke();

        // TODO: 게임오버 씬으로 전환 또는 게임오버 UI 표시
        // SceneManager.LoadScene(SceneNames.RESULT);
    }

    /// <summary>
    /// 곡 종료 시 클리어 판정
    /// </summary>
    /// <returns>클리어 성공 여부</returns>
    public bool CheckClearCondition()
    {
        float clearThreshold = GetClearThreshold();
        bool isCleared = currentHP >= clearThreshold;

        if (isCleared)
        {
            Debug.Log($"HPSystem: 클리어 성공! (HP: {currentHP}/{clearThreshold}% 이상)");
            OnGameClear?.Invoke();
        }
        else
        {
            Debug.Log($"HPSystem: 클리어 실패 (HP: {currentHP}/{clearThreshold}% 미만)");
        }

        return isCleared;
    }

    /// <summary>
    /// 난이도별 클리어 기준 HP 반환
    /// </summary>
    private float GetClearThreshold()
    {
        switch (currentMode)
        {
            case JudgmentMode.JudgmentMode_Normal:
                return normalClearThreshold;

            case JudgmentMode.JudgmentMode_Hard:
                return hardClearThreshold;

            case JudgmentMode.JudgmentMode_Super:
                return superClearThreshold;

            default:
                return normalClearThreshold;
        }
    }

    /// <summary>
    /// 현재 HP 반환 (읽기 전용)
    /// </summary>
    public float GetCurrentHP()
    {
        return currentHP;
    }

    /// <summary>
    /// 현재 HP 비율 반환 (0~1)
    /// </summary>
    public float GetHPRatio()
    {
        return currentHP / maxHP;
    }

    /// <summary>
    /// 게임오버 상태 확인
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    /// <summary>
    /// HP 강제 설정 (테스트/디버그용)
    /// </summary>
    public void SetHP(float hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        UpdateHPDisplay();
    }
}
