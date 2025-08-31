using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingPanel;  // SettingUI �г� ������Ʈ
    public Button startButton;       // Start ��ư
    public Button settingButton;     // Setting ��ư
    public Button exitButton;        // Exit ��ư

    void Start()
    {
        // ��ư Ŭ�� �̺�Ʈ ����
        if (settingButton != null)
            settingButton.onClick.AddListener(OpenSettingPanel);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    void OpenSettingPanel()
    {
        if (settingPanel != null)
            settingPanel.SetActive(true);
    }

    // ���� ����
    void ExitGame()
    {
        EditorApplication.isPlaying = false;  // ����Ƽ ������ ���� ����

        //Application.Quit();
    }
}