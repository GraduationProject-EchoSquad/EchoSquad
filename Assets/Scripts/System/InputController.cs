using Cysharp.Threading.Tasks;
using UnityEngine;

public class InputController : Singleton<InputController>
{
    public string exitButtonName = "Cancel"; // 발사를 위한 입력 버튼 이름
    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown(exitButtonName))
        {
            if (UIManager.Instance.HasPopupUI())
            {
                UIManager.Instance.HidePopupUI();
            }
            else
            {
                UIManager.Instance.Show<ExitUI>(UIManager.EUIData.Exit).Forget();
            }
        }
        
        if (GameManager.Instance == null
            || GameManager.Instance.IsGameControllable() == false)
        {
            return;
        }

        // C 키로 CommandUI 토글
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCommandUI();
        }
    }

    private void ToggleCommandUI()
    {
        var commandUI = UIManager.Instance.Get<CommandUI>(UIManager.EUIData.Command);
        if (commandUI != null && commandUI.gameObject.activeInHierarchy)
        {
            UIManager.Instance.Hide(UIManager.EUIData.Command);
        }
        else
        {
            UIManager.Instance.Show<CommandUI>(UIManager.EUIData.Command).Forget();
        }
    }
}
