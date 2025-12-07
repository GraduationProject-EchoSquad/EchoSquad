using UnityEngine;
using UnityEngine.UI;

public class ExitUI : UIBase
{
    [SerializeField] private Button exitGameButton;      // 게임 종료
    [SerializeField] private Button toSystemButton;      // 시스템(타이틀)으로
    [SerializeField] private Button closeButton;         // 닫기 (게임으로 돌아가기)

    private void Start()
    {
        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(OnExitGameClicked);

        if (toSystemButton != null)
            toSystemButton.onClick.AddListener(OnToSystemClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        if (exitGameButton != null)
            exitGameButton.onClick.RemoveListener(OnExitGameClicked);

        if (toSystemButton != null)
            toSystemButton.onClick.RemoveListener(OnToSystemClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnExitGameClicked()
    {
        Debug.Log("[ExitUI] Exit Game clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnToSystemClicked()
    {
        Debug.Log("[ExitUI] To System clicked");
        // 타이틀 씬으로 이동
        UIManager.Instance.GameRestart();
    }

    private void OnCloseClicked()
    {
        Debug.Log("[ExitUI] Close clicked");
        UIManager.Instance.Hide(UIManager.EUIData.Exit);
    }
}
