using Cysharp.Threading.Tasks;
using UnityEngine;

public class InputController : Singleton<InputController>
{
    public string exitButtonName = "Cancel"; // 발사를 위한 입력 버튼 이름
    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance == null
            || GameManager.Instance.IsGameControllable() == false)
        {
            return;
        }

        if (Input.GetButtonDown(exitButtonName))
        {
            UIManager.Instance.Show<ExitUI>(UIManager.EUIData.Exit).Forget();
        }
    }
}
