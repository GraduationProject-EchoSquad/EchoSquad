using UnityEngine;

// 점수와 게임 오버 여부, 게임 UI를 관리하는 게임 매니저
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Start,
        Wave,
        Break,
        End,
    } 

    private int score; // 현재 게임 점수

    public GameState CurrentGameState { get; private set; } // 현재 게임 상태

    public bool isTest { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        SetGameState(GameState.Start);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        isTest = false;
    }

    public void SetGameState(GameState NewGameState)
    {
        if (CurrentGameState == NewGameState)
        {
            return;
        }
        
        CurrentGameState = NewGameState;
        
        Debug.Log($"SetGameState : {NewGameState}");
    }


    // 점수를 추가하고 UI 갱신
    public void AddScore(int newScore)
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (CurrentGameState == GameState.Wave)
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
        SetGameState(GameState.End);
        // 게임 오버 UI를 활성화
        UIManager.Instance.SetActiveGameoverUI(true);
    }

    public bool IsGameControllable()
    {
        return CurrentGameState == GameState.Wave || CurrentGameState == GameState.Break;
    }
}