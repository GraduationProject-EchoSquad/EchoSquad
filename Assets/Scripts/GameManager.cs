using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 점수와 게임 오버 여부, 게임 UI를 관리하는 게임 매니저
// 게임의 전체 흐름을 총괄
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Start,
        Preparation,  // 동료 능력치 조절 단계
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

    public void HandleIntroFinished()
    {
        Debug.Log("[GameManager] Intro 완료 - 게임 흐름 시작");
        GameFlowController().Forget();
    }

    // 게임의 전체 흐름 총괄: Intro → Preparation → Wave → End
    private async UniTaskVoid GameFlowController()
    {
        // 1. Preparation 단계: 동료 음성 설정
        SetGameState(GameState.Preparation);
        Debug.Log("[GameManager] Preparation 단계 시작");

        await TeammateVoiceSetupManager.Instance.ShowAndWaitForCompletion();

        // HUD Panel 숨김
        HUDUI hudUI = await UIManager.Instance.GetUI<HUDUI>(UIManager.EUIData.HUD);
        hudUI.gameObject.SetActive(true);

        Debug.Log("[GameManager] Preparation 단계 완료 - 유닛 스폰");
        UnitManager.Instance.InitSpawnUnit();

        // VoiceProfile 적용 (유닛 생성 후)
        TeammateVoiceSetupManager.Instance.ApplyVoiceProfilesToTeammates();

        // 2. Wave 단계: WaveManager에게 웨이브 진행 위임
        SetGameState(GameState.Wave);
        Debug.Log("[GameManager] Wave 단계 시작 - WaveManager에 위임");

        bool isWin = await WaveManager.Instance.StartWaves();

        // 3. 모든 웨이브 완료 또는 게임 오버
        if (CurrentGameState != GameState.End)
        {
            Debug.Log("[GameManager] 모든 웨이브 클리어!");
            EndGame(isWin);
        }
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
            PubSubManager.Instance.Publish<OnScoreUpdatedData>(PubSubEvent.OnScoreUpdated, data => data.Score = score);
            //UIManager.Instance.UpdateScoreText(score);
        }
    }

    // 게임 오버 처리
    public async UniTaskVoid EndGame(bool isWin)
    {
        // 게임 오버 상태를 참으로 변경
        SetGameState(GameState.End);
        
        PubSubManager.Instance.Publish<OnGameEndData>(PubSubEvent.OnGameEnd, data => data.IsWin = isWin);
        if (isWin)
        {
            GameOverUI GameOverUI = await UIManager.Instance.GetUI<GameOverUI>(UIManager.EUIData.EndingClear);
            GameOverUI.gameObject.SetActive(true); //.ShowGameOverUI(isWin);    
        }
        else
        {
            GameOverUI GameOverUI = await UIManager.Instance.GetUI<GameOverUI>(UIManager.EUIData.EndingFail);
            GameOverUI.gameObject.SetActive(true); //.ShowGameOverUI(isWin); 
        }
    }

    public bool IsGameControllable()
    {
        return CurrentGameState == GameState.Wave || CurrentGameState == GameState.Break;
    }
}