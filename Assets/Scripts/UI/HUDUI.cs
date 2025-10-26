using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : UIBase
{
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text enemyText;

    private void Start()
    {
        PubSubManager.Instance.Subscribe<OnLifeChangedData>(PubSubEvent.OnLifeChanged,
            data => UpdateLifeText(data.LiveCount));
        PubSubManager.Instance.Subscribe<OnWaveStartData>(PubSubEvent.OnWaveStart,
            data => UpdateWaveText(data.WaveIndex));
        PubSubManager.Instance.Subscribe<OnScoreUpdatedData>(PubSubEvent.OnScoreUpdated,
            data => UpdateScoreText(data.Score));
        PubSubManager.Instance.Subscribe<OnRemainEnemyCountChangeData>(PubSubEvent.OnRemainEnemyCountChange,
            data => UpdateEnemyCountText(data.remainEnemyCount));
        PubSubManager.Instance.Subscribe<OnAmmoUpdatedData>(PubSubEvent.OnAmmoUpdated,
            data => UpdateAmmoText(data.MagAmmo, data.AmmoRemain));
        
        PubSubManager.Instance.Subscribe<OnGameEndData>(PubSubEvent.OnGameEnd,
            data => gameObject.SetActive(false));
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
}