using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : UIBase
{
    [SerializeField] private Text resultText;
    [SerializeField] private Button TitleButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TitleButton.onClick.AddListener(() => OnClickTitle());
    }
    
    private void ShowGameOverUI(OnGameEndData data)
    {
        gameObject.SetActive(true);
 
        if (data.IsWin)
        {
            resultText.text = "YOU WIN!";
        }
        else
        {
            resultText.text = "GAME OVER";
        }
    }

    private async UniTaskVoid OnClickTitle()
    {
        await SceneController.Instance.LoadSceneAsync(SceneController.ESceneData.Title,LoadSceneMode.Single);
        gameObject.SetActive(false);
    }
}
