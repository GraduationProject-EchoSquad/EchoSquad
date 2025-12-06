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
    }
}
