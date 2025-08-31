using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingPanel;  // SettingUI 패널 오브젝트
    public Button startButton;       // Start 버튼
    public Button settingButton;     // Setting 버튼
    public Button exitButton;        // Exit 버튼

    void Start()
    {
        // 버튼 클릭 이벤트 연결
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

    // 게임 종료
    void ExitGame()
    {
        EditorApplication.isPlaying = false;  // 유니티 에디터 실행 종료

        //Application.Quit();
    }
}