using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : UIBase
{
    [SerializeField] private Button TitleButton;
    [SerializeField] private TextMeshProUGUI WaveText;
    [SerializeField] private TextMeshProUGUI PlayTimeText;
    [SerializeField] private TextMeshProUGUI ScoreText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TitleButton.onClick.AddListener(() => OnClickTitle());
    }

    void OnEnable()
    {
        WaveText.text = WaveManager.Instance.currentWaveIndex.ToString();
        PlayTimeText.text = PlayTimeManager.Instance.GetPlayTimeDisplay();
        ScoreText.text = CurrencyManager.Instance.Score.ToString();
    }

    /*public void ShowGameOverUI(bool isWin)
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
    }*/

    private async UniTaskVoid OnClickTitle()
    {
        UIManager.Instance.HideAllPopupUI();
        await SceneController.Instance.LoadSceneAsync(SceneController.ESceneData.Start,LoadSceneMode.Single);
        await SceneController.Instance.LoadTitle();
        gameObject.SetActive(false);
    }
}
