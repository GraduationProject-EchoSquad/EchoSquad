using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Canvas")] [SerializeField] private Canvas mainCanvas; // Main UI Canvas

    public enum EUIData
    {
        None,

        
        //Panel
        Title,
        HUD,
        TeamVoiceSetUp,
        GameOver,

        //Popup

        //Object
        Countdown
    }

    public enum EUIType
    {
        None,

        Panel,

        Popup
    }

    public static Dictionary<EUIData, string> UIMetaData = new Dictionary<EUIData, string>()
    {
        { EUIData.Title, "Prefabs/UI/TitleUI.prefab" },
        { EUIData.HUD, "Prefabs/UI/HUD.prefab" },
        { EUIData.TeamVoiceSetUp, "Prefabs/UI/AllyStatChoiceUI.prefab" },
        { EUIData.GameOver, "Prefabs/UI/GameoverUI.prefab" }, // GameOverUI 프리팹 경로 추가
        
        { EUIData.Countdown, "Prefabs/UI/CountdownText.prefab" }, // GameOverUI 프리팹 경로 추가
    };

    public static Dictionary<EUIData, UIBase> UIDict = new Dictionary<EUIData, UIBase>();

    private async UniTask<T> LoadUI<T>(EUIData UIData) where T : UIBase
    {
        T step = await ResourceController.Instance.GetAsync<T>(UIMetaData[UIData], mainCanvas.transform);
        //await UniTask.NextFrame();

        //step.gameObject.SetActive(true);

        return step;
    }
    
    public async UniTask<T> GetUI<T>(EUIData UIData) where T : UIBase
    {
        if (UIDict.ContainsKey(UIData) == false)
        {
            T loadedUI = await LoadUI<T>(UIData);
            UIDict.Add(UIData, loadedUI);
        }

        //UIDict[UIData].gameObject.SetActive(true);

        return (T)UIDict[UIData];
    }


    private void Awake()
    {
        // Shift UI를 위한 전역 "UI Audio" AudioSource 생성
        CreateUIAudioIfNeeded();
        
        // 게임 시작 시 필요한 UI 미리 로드
        PreloadGameUI().Forget();
    }

    private async UniTaskVoid PreloadGameUI()
    {
        // GameOverUI를 미리 로드하고 비활성화 상태로 둡니다.
        (await GetUI<TitleUI>(EUIData.Title)).gameObject.SetActive(true);
        //await GetUI<GameOverUI>(EUIData.GameOver);
    }

    // Shift UI의 UIElementSound가 사용할 전역 "UI Audio" AudioSource 생성
    private void CreateUIAudioIfNeeded()
    {
        // 이미 씬에 존재하는지 확인
        GameObject existingUIAudio = GameObject.Find("UI Audio");

        if (existingUIAudio == null)
        {
            GameObject uiAudio = new GameObject("UI Audio");
            AudioSource audioSource = uiAudio.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D 사운드
            audioSource.volume = 1f;

            // 씬 전환 시에도 유지 (선택사항)
            // DontDestroyOnLoad(uiAudio);

            Debug.Log("[UIManager] Created global UI Audio AudioSource");
        }
    }

    // 메인 Canvas 반환 (다른 Manager가 UI를 생성할 때 사용)
    public Canvas GetMainCanvas()
    {
        return mainCanvas;
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}