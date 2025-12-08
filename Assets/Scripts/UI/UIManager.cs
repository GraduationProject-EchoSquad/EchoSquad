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

    // UI 데이터의 종류 (이름)
    public enum EUIData
    {
        None,
    
        // Panels
        Title,
        HUD,
        TeamVoiceSetUp,

        // Popups
        EndingClear,
        EndingFail,
        Shop,
        Setting,
        Exit,
        Command,

        // Standalone Objects
        Countdown
    }

// UI의 카테고리 (종류)
    public enum EUIType
    {
        None,
        Panel,
        Popup,
        Object // 카운트다운과 같은 단일 오브젝트를 위한 타입 추가
    }

    /// <summary>
    /// 개별 UI 요소의 메타데이터를 저장하는 클래스입니다.
    /// (struct 대신 class를 사용하고, 외부에서 수정 불가능하도록 readonly 프로퍼티로 만듭니다)
    /// </summary>
    public class UIMetadata
    {
        public string Path { get; }
        public EUIType UIType { get; }

        public UIMetadata(string path, EUIType uiType)
        {
            Path = path;
            UIType = uiType;
        }
    }

    /// <summary>
    /// UI 메타데이터를 관리하는 중앙 저장소
    /// </summary>
    public static class UIDatabase
    {
        // --- 헬퍼 메서드: 가독성과 편의성을 극대화합니다 ---
        private static UIMetadata Panel(string path) => new UIMetadata(path, EUIType.Panel);
        private static UIMetadata Popup(string path) => new UIMetadata(path, EUIType.Popup);
        private static UIMetadata Object(string path) => new UIMetadata(path, EUIType.Object);

        // --- 최종 메타데이터 딕셔너리 ---
        public static readonly Dictionary<EUIData, UIMetadata> UIMetaDataDict = new Dictionary<EUIData, UIMetadata>()
        {
            // 헬퍼 메서드를 사용하여 매우 직관적으로 데이터를 등록합니다.
            { EUIData.Title,          Panel("Prefabs/UI/TitleUI.prefab") },
            { EUIData.HUD,            Panel("Prefabs/UI/HUD.prefab") },
            { EUIData.TeamVoiceSetUp, Panel("Prefabs/UI/AllyStatChoiceUI.prefab") },

            { EUIData.EndingClear,    Popup("Prefabs/UI/EndingUI_Clear.prefab") },
            { EUIData.EndingFail,     Popup("Prefabs/UI/EndingUI_Failed.prefab") },
            { EUIData.Shop,           Popup("Prefabs/UI/ShopUI.prefab") },
            { EUIData.Setting,        Popup("Prefabs/UI/SettingUI.prefab") },
            { EUIData.Exit,           Popup("Prefabs/UI/Exit.prefab") },
            { EUIData.Command,        Popup("Prefabs/UI/CommandUI.prefab") },

            { EUIData.Countdown,      Object("Prefabs/UI/CountdownText.prefab") },
        };
    }

    private Dictionary<EUIData, UIBase> UIDict = new Dictionary<EUIData, UIBase>();
    private Stack<UIBase> PopupStack = new Stack<UIBase>();
    private HashSet<EUIData> _loadingUI = new HashSet<EUIData>();  // 로딩 중인 UI 추적

    private async UniTask<T> LoadUI<T>(EUIData UIData) where T : UIBase
    {
        T step = await ResourceController.Instance.GetAsync<T>(UIDatabase.UIMetaDataDict[UIData].Path, mainCanvas.transform);
        if (step != null)
        {
            step.uiData = UIData;
            step.gameObject.SetActive(false);
        }
        return step;
    }
    
    public async UniTask<T> GetUI<T>(EUIData UIData) where T : UIBase
    {
        if (UIDict.ContainsKey(UIData) == false)
        {
            T loadedUI = await LoadUI<T>(UIData);
            UIDict.Add(UIData, loadedUI);
        }
        return (T)UIDict[UIData];
    }

    /// <summary>
    /// UI를 표시합니다 (async)
    /// </summary>
    public async UniTask<T> Show<T>(EUIData UIData) where T : UIBase
    {
        // 이미 로딩 중이면 중복 호출 방지
        if (_loadingUI.Contains(UIData))
        {
            return null;
        }

        _loadingUI.Add(UIData);
        try
        {
            T ui = await GetUI<T>(UIData);
            if (ui != null && ui.gameObject.activeInHierarchy == false)
            {
                ui.gameObject.SetActive(true);
                if (UIDatabase.UIMetaDataDict[UIData].UIType == EUIType.Popup)
                {
                    PopupStack.Push(ui);
                }
            }
            return ui;
        }
        finally
        {
            _loadingUI.Remove(UIData);
        }
    }

    /// <summary>
    /// UI를 숨깁니다 (async)
    /// </summary>
    public void Hide(EUIData UIData)
    {
        if (UIDict.ContainsKey(UIData))
        {
            UIDict[UIData].gameObject.SetActive(false);
            if (UIDatabase.UIMetaDataDict[UIData].UIType == EUIType.Popup && PopupStack.Count > 0)
            {
                PopupStack.Pop();
            }
        }
    }
    
    /// <summary>
    /// Popui 끄기! ESC
    /// </summary>
    public void HidePopupUI()
    {
        if (PopupStack.Count > 0)
        {
            UIBase uiBase = PopupStack.Peek();
            Hide(uiBase.uiData);
        }
    }
    
    public void HideAllPopupUI()
    {
        while (PopupStack.Count != 0)
        {
            UIBase uiBase = PopupStack.Peek();
            Hide(uiBase.uiData);
        }
    }

    public bool HasPopupUI()
    {
        return PopupStack.Count > 0;
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