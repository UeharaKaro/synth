using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 사용하기 위해 필요 


public class SongSelectionManager : MonoBehaviour   
{
    // UI 요소들
	public TextMeshProUGUI songTitleText; // 노래 제목 표시 텍스트
    public TextMeshProUGUI artistText; // 아티스트 표시 텍스트
	public TextMeshProUGUI keyCountText; // 현재 키 개수를 표현할 텍스트
	public TextMeshProUGUI difficultyText; // 현재 난이도를 표현할 텍스트
	public Button selectSongButton; // 노래 선택 버튼 (횡 슬라이더식 혹은 종(위아래))

	// 현재 키 모드(기본 4키)
	private int currentKeyCount = 4;
	private int[] availableKeyCounts = { 4, 5, 6, 7, 8, 10 }; // 지원하는 키 개수 목록
	
	// 난이도 목록 (예: Easy, Normal, Hard, Expert, Master) (추후 변경 가능)
    private string[] difficulties = { "Easy", "Normal", "Hard", "Expert", "Master", "Sepcial" };
    private int currentDifficultyIndex = 0; // 기본 난이도는 Easy

	void Start()
    {
        // 초기 UI 업데이트
		UpdateUI();

		// 버튼 클릭 이벤트 등록 (곡 선택시 게임 씬으로 이동)
		if (selectSongButton != null)
		{
			selectSongButton.onClick.AddListener(OnSelectSong); // 노래 선택 버튼 클릭시 OnSelectSong 함수 실행
		}
	}
	void Update()
	{
		// 왼쪽 Shift 키 : 키 개수 감소
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			ChangeKeyCount(-1);
		}
		
		// 오른쪽 Shift 키 : 키 개수 증가
        if (Input.GetKeyDown(KeyCode.RightShift))
		{
			ChangeKeyCount(1);
		}

		// 왼쪽 방향키 : 난이도 감소
		if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeDifficulty(-1);
        }
		
		// 오른쪽 방향키 : 난이도 증가
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeDifficulty(1);
        }
	}

	// 키 개수 변경 함수
	private void ChangeKeyCount(int delta)
	{
		// Special 난이도가 아닌 경우에만 키 개수 변경 허용 (Special 모드에서는 고정 4키/ 임시)
		if (difficulties[currentDifficultyIndex] != "Sepcial")
		{
			// 현재 키 개수의 인덱스 찾기
			int currentIndex = System.Array.IndexOf(availableKeyCounts, currentKeyCount);

			// 인덱스 변경
			currentIndex = (currentIndex + delta + availableKeyCounts.Length) % availableKeyCounts.Length;

			// 새로운 키 개수 설정
			currentKeyCount = availableKeyCounts[currentIndex];
		}

		UpdateUI();
	}
	
	// 난이도 변경 함수
	private void ChangeDifficulty(int delta)
	{
		// 인덱스 변경
		currentDifficultyIndex = (currentDifficultyIndex + delta + difficulties.Length) % difficulties.Length; 
		
		// 난이도가 Special로 변경되면 키 개수를 4키로 강제
		if (difficulties[currentDifficultyIndex] == "Sepcial")
		{
			currentKeyCount = 4; // Special 모드에서는 고정 4키 (임시)
		}
		
		UpdateUI();
	}
	
	// UI 업데이트 함수
	private void UpdateUI()
	{
		if (keyCountText != null)
		{
			keyCountText.text = "Key Count: " + currentKeyCount.ToString(); // 현재 키 개수 표시
		}

		if (difficultyText != null)
		{
			difficultyText.text = "Difficulty: " + difficulties[currentDifficultyIndex]; // 현재 난이도 표시
		}
	}
	
	// 곡 선택 시 호출되는 함수
	private void OnSelectSong()
	{
		// Special 난이도 시 키 개수 4로 고정(이미 UI에서 처리되었지만 확인)
		if (difficulties[currentDifficultyIndex] == "Special")
		{
			currentKeyCount = 4; // Special 모드에서는 고정 4키 (임시)
		}
		
		// PlayerPrefes를 사용해 keyCount와 difficulty 전달 (게임 씬에서 불러 올 수 았음)
		PlayerPrefs.SetInt("SelectedKeyCount", currentKeyCount);
		PlayerPrefs.SetString("SelectedDifficulty", difficulties[currentDifficultyIndex]);
		
		// 게임 씬 로드  (GameScene으로 가정, 실제로는 변경 필요)
		SceneManager.LoadScene("GameScene");
	}
}
