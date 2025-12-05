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
        EndingClear,
        EndingFail,
        Shop,

        //Popup
        Setting,
        Exit,

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
        { EUIData.EndingClear, "Prefabs/UI/EndingUI_Clear.prefab" }, 
        { EUIData.EndingFail, "Prefabs/UI/EndingUI_Failed.prefab" }, 
        { EUIData.Shop, "Prefabs/UI/ShopUI.prefab" }, // ShopUI 프리팹 경로 추가
        
        { EUIData.Setting, "Prefabs/UI/SettingUI.prefab" },
        { EUIData.Exit, "Prefabs/UI/ExitUI.prefab" },
        

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

    /// <summary>
    /// UI를 표시합니다 (async)
    /// </summary>
    public async UniTask<T> Show<T>(EUIData UIData) where T : UIBase
    {
        T ui = await GetUI<T>(UIData);
        ui.gameObject.SetActive(true);
        return ui;
    }

    /// <summary>
    /// UI를 숨깁니다 (async)
    /// </summary>
    public async UniTask Hide<T>(EUIData UIData) where T : UIBase
    {
        if (UIDict.ContainsKey(UIData))
        {
            UIDict[UIData].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// UI가 로드되어 있는지 확인하고, 로드되어 있으면 가져옵니다 (sync)
    /// </summary>
    public T Get<T>(EUIData UIData) where T : UIBase
    {
        if (UIDict.ContainsKey(UIData))
        {
            return (T)UIDict[UIData];
        }
        return null;
    }


    protected override void Awake()
    {
        base.Awake();
        // Shift UI를 위한 전역 "UI Audio" AudioSource 생성
        //CreateUIAudioIfNeeded();
        
        // 게임 시작 시 필요한 UI 미리 로드
        PreloadGameUI().Forget();
    }

    private async UniTaskVoid PreloadGameUI()
    {
        // GameOverUI를 미리 로드하고 비활성화 상태로 둡니다.
        await GetUI<TitleUI>(EUIData.Title);
        //await GetUI<GameOverUI>(EUIData.GameOver);
    }

    // Shift UI의 UIElementSound가 사용할 전역 "UI Audio" AudioSource 생성
   

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