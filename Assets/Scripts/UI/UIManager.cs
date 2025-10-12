using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;

    private void Start()
    {
        PubSubManager.Instance.Subscribe(PubSubEvent.OnPlayerDeath, UpdateLifeText);
    }

    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score : " + newScore;
    }
    
    public void UpdateWaveText(int waves, int count)
    {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    private void UpdateLifeText(PubSubDataBase data)
    {
        if (data is OnPlayerDeathData onPlayerDeathData)
        {
            UpdateLifeText(onPlayerDeathData.liveCount);
        }
    }
    public void UpdateLifeText(int count)
    {
        lifeText.text = "Life : " + count;
    }
    
    public void SetActiveGameoverUI(bool active)
    {
        gameoverUI.SetActive(active);
    }
    
    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}