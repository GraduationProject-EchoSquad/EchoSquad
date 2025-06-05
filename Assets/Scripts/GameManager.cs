using UnityEngine;

// 점수와 게임 오버 여부, 게임 UI를 관리하는 게임 매니저
public class GameManager : Singleton<GameManager>
{

    private int score; // 현재 게임 점수
    public bool isGameover { get; private set; } // 게임 오버 상태

    protected override void Awake()
    {
        base.Awake();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }


    // 점수를 추가하고 UI 갱신
    public void AddScore(int newScore)
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (!isGameover)
        {
            // 점수 추가
            score += newScore;
            // 점수 UI 텍스트 갱신
            UIManager.Instance.UpdateScoreText(score);
        }
    }

    // 게임 오버 처리
    public void EndGame()
    {
        // 게임 오버 상태를 참으로 변경
        isGameover = true;
        // 게임 오버 UI를 활성화
        UIManager.Instance.SetActiveGameoverUI(true);
    }
}