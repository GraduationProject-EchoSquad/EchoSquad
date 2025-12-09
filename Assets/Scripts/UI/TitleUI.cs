using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUI : UIBase
{
    [Header("UI References")] public GameObject settingPanel; // SettingUI �г� ������Ʈ
    public GameObject startPanel; // SettingUI �г� ������Ʈ
    public Button startButton; // Start ��ư
    public Button settingButton; // Setting ��ư
    public Button exitButton; // Exit ��ư

    void Start()
    {
        // ��ư Ŭ�� �̺�Ʈ ����
        if (startButton != null)
            startButton.onClick.AddListener(() => OpenStartPanel().Forget());

        if (settingButton != null)
            settingButton.onClick.AddListener(() => OpenSettingPanel().Forget());

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    async UniTaskVoid OpenStartPanel()
    {
        startButton.interactable = false;
        await SceneController.Instance.LoadSceneAsync(SceneController.ESceneData.Main, LoadSceneMode.Single);
        gameObject.SetActive(false);
        startButton.interactable = true;

        /*gameObject.SetActive(false);
        startPanel.SetActive(true);*/
    }

    async UniTaskVoid OpenSettingPanel()
    {
        SettingController ui = await UIManager.Instance.Show<SettingController>(UIManager.EUIData
            .Setting);
        
    }

    // ���� ����
    void ExitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false; // ����Ƽ ������ ���� ����
        #endif

        Application.Quit();
    }
}