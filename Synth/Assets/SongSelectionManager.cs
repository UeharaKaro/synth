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
		// Special 난이도가 아닌 경우에만 키 개수 변경 허용
		

}
