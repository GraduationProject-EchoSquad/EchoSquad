using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Canvas")]
    [SerializeField] private Canvas mainCanvas;          // Main UI Canvas

    [Header("HUD Elements")]
    [SerializeField] private GameObject hudPanel;        // HUD Panel (Panel GameObject under Canvas)
    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;

    private void Awake()
    {
        // Shift UI를 위한 전역 "UI Audio" AudioSource 생성
        CreateUIAudioIfNeeded();
    }

    private void Start()
    {
        PubSubManager.Instance.Subscribe(PubSubEvent.OnPlayerDeath, UpdateLifeText);
    }

    // Shift UI의 UIElementSound가 사용할 전역 "UI Audio" AudioSource 생성
    private void CreateUIAudioIfNeeded()
    {
        // 이미 씬에 존재하는지 확인
        GameObject existingUIAudio = GameObject.Find("UI Audio");

        if (existingUIAudio == null)
        {
            GameObject uiAudio = new GameObject("UI Audio");
            AudioSource audioSource = uiAudio.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D 사운드
            audioSource.volume = 1f;

            // 씬 전환 시에도 유지 (선택사항)
            // DontDestroyOnLoad(uiAudio);

            Debug.Log("[UIManager] Created global UI Audio AudioSource");
        }
    }

    // 메인 Canvas 반환 (다른 Manager가 UI를 생성할 때 사용)
    public Canvas GetMainCanvas()
    {
        return mainCanvas;
    }

    // HUD Panel 표시/숨김
    public void SetHUDActive(bool active)
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(active);
            Debug.Log($"[UIManager] HUD Panel {(active ? "shown" : "hidden")}");
        }
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