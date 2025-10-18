using System;
using System.Linq;
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
    [SerializeField] private Text enemyText;

    private void Start()
    {
        PubSubManager.Instance.Subscribe<OnPlayerDeathData>(PubSubEvent.OnPlayerDeath,
            data => UpdateLifeText(data.liveCount));
        PubSubManager.Instance.Subscribe<OnWaveStartData>(PubSubEvent.OnWaveStart,
            data => UpdateWaveText(data.waveIndex));
        /*PubSubManager.Instance.Subscribe(PubSubEvent.OnEnemyDeath,
            () => UpdateEnemyCountText(UnitManager.Instance.GetAliveEnemies(UnitManager.Instance.GetPlayerUnit())
                .Count()));*/
    }

    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score : " + newScore;
    }

    public void UpdateWaveText(int waves)
    {
        waveText.text = "Wave : " + waves;
    }

    public void UpdateEnemyCountText(int count)
    {
        enemyText.text = "Enemy Left : " + count;
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