using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : UIBase
{
    [SerializeField] private Text resultText;
    [SerializeField] private Button TitleButton;
    [SerializeField] private Image TitleImage;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TitleButton.onClick.AddListener(() => OnClickTitle());
    }
    
    public void ShowGameOverUI(bool isWin)
    {
        gameObject.SetActive(true);
 
        if (isWin)
        {
            TitleImage.color = new Color32(30, 98, 253, 160);
            resultText.text = "YOU WIN!";
        }
        else
        {
            TitleImage.color = new Color32(253, 30, 30, 160);
            resultText.text = "GAME OVER";
        }
    }

    private async UniTaskVoid OnClickTitle()
    {
        await SceneController.Instance.LoadSceneAsync(SceneController.ESceneData.Title,LoadSceneMode.Single);
        gameObject.SetActive(false);
    }
}
