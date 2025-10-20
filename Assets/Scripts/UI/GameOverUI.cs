using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : UIBase
{
    [SerializeField] private Text resultText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PubSubManager.Instance.Subscribe<OnGameEndData>(PubSubEvent.OnGameEnd, ShowGameOverUI);
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
}
