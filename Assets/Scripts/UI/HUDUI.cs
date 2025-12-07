using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : UIBase
{
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text enemyText;
    [SerializeField] private Image MicBG;
    [SerializeField] private Button commandButton;

    private void Start()
    {
        if (commandButton)
        {
            commandButton.onClick.AddListener(OpenCommandUI);
        }
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
        
        PubSubManager.Instance.Subscribe(PubSubEvent.OnMicStart,
            () => ChangeMicBG(true));
        PubSubManager.Instance.Subscribe(PubSubEvent.OnMicEnd,
            () => ChangeMicBG(false));
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
    
    public void ChangeMicBG(bool isOn)
    {
        // 1. 현재 색상을 임시 변수에 복사합니다.
        Color tempColor = MicBG.color;

        if (isOn)
        {
            // 2. alpha 값을 1로 설정합니다.
            tempColor.a = 1f;
        }
        else
        {
            // 2. alpha 값을 0.1로 설정합니다.
            tempColor.a = 0.1f;
        }

        // 3. 수정된 색상을 다시 할당합니다.
        MicBG.color = tempColor;
    }

    private void OpenCommandUI()
    {
        UIManager.Instance.Show<CommandUI>(UIManager.EUIData.Command).Forget();
    }

    private void OnDestroy()
    {
        if (commandButton)
        {
            commandButton.onClick.RemoveListener(OpenCommandUI);
        }
    }
}