using System.Collections;
using UnityEngine;
using TMPro;             // TMP ��
using EasyTextEffects;   // TextEffect�� �� ���ӽ����̽��� �ִٰ� ����

[System.Serializable]
public class Wave
{
    public GameObject enemyPrefab;
    public int count;
    public float spawnInterval;
    public Transform[] spawnPoints;
}

public class WaveManager : Singleton<WaveManager>
{
    [Header("Waves")]
    public Wave[] waves;

    [Header("Time Between Waves (s)")]
    public float timeBetweenWaves = 5f;

    [Header("Countdown Text")]
    public TextMeshProUGUI countdownText;   // drag in inspector
    private TextEffect countdownEffect;     // EasyTextEffects component

    [Header("Countdown Start")]
    public int countdownStart = 5;

    [Header("Countdown Interval (s)")]
    public float countdownInterval = 1f;

    private int currentWaveIndex = 0;
    private int enemiesRemaining;
    private bool isSpawning = false;

    protected override void Awake()
    {
        base.Awake();
        
        // TextEffect ������Ʈ ��������
        if (countdownText != null)
            countdownEffect = countdownText.GetComponent<TextEffect>();

        // Intro_Controller�� ã�Ƽ� �̺�Ʈ�� �ڵ鷯 ���
        Intro_Controller intro = Intro_Controller.Instance;
        if (intro != null)
        {
            intro.OnIntroFinished += HandleIntroFinished;
        }
    }

    private void Start()
    {

    }
    private void HandleIntroFinished()
    {
        // ��Ʈ�� ������ ī��Ʈ�ٿ�+���� ������ ����
        StartCoroutine(HandleWaveSequence());
    }

    private void UpdateUI()
    {
        // ���� ���̺�� ���� ���� �� ǥ��
        UIManager.Instance.UpdateWaveText(currentWaveIndex, enemiesRemaining);
    }
    private void Update()
    {
        //if (!isSpawning && enemiesRemaining == 0)
        //{
        //    if (currentWaveIndex < waves.Length)
        //    {
        //        StartCoroutine(HandleWaveSequence());
        //    }
        //    else
        //    {
        //        Debug.Log("��� ���̺� �Ϸ�!");
        //    }
        //}
    }

    // ���̺� ��ü �帧: timeBetweenWaves �� ī��Ʈ�ٿ� �� �� ����
    private IEnumerator HandleWaveSequence()
    {
        isSpawning = true;

        // (ù ���̺갡 �ƴ϶��) ���̺� �� ���
        if (currentWaveIndex > 0)
            yield return new WaitForSeconds(timeBetweenWaves);

        // ī��Ʈ�ٿ� ���
        yield return StartCoroutine(PlayCountdownText());

        // ���� ���̺� ����
        Wave wave = waves[currentWaveIndex];
        enemiesRemaining = wave.count;

        for (int i = 0; i < wave.count; i++)
        {
            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        currentWaveIndex++;
        UpdateUI();
    }

    // ���� �� "Wave {n}" �� "Fight!" ������ TMP �ؽ�Ʈ ��ü
    private IEnumerator PlayCountdownText()
    {
        if (countdownText == null)
        {
            yield break;
        }

        // (1) 5,4,3,2,1 ī��Ʈ
        for (int i = countdownStart; i > 0; i--)
        {
            // ��: "<link=wave+fadein+movein>5</link>"
            countdownText.text = $"<link=wave+fadein+movein>{i}</link>";
            // �±� �ؼ� ����
            if (countdownEffect != null)
            {
                countdownEffect.UpdateStyleInfos();
                countdownEffect.Refresh(); // �ʿ��� ��� Refresh() ȣ��
            }
            yield return new WaitForSeconds(countdownInterval);
        }

        // (2) "Wave {��ȣ}" ǥ�� (currentWaveIndex�� 0���� �����ϹǷ� +1)
        int waveNumber = currentWaveIndex + 1;
        countdownText.text = $"<link=wave+fadein+movein>Wave {waveNumber}</link>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        yield return new WaitForSeconds(countdownInterval);

        // (3) "Fight!" ǥ��
        countdownText.text = $"<b><size=250px><link=gradient+wave+rotate+scale>Start!</link></size></b>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        yield return new WaitForSeconds(countdownInterval);

        // (4) �ؽ�Ʈ ����
        countdownText.text = string.Empty;
        if (countdownText.gameObject.activeSelf)
            countdownText.gameObject.SetActive(false);
        yield return null;
    }

    private void SpawnEnemy(Wave wave)
    {
        Transform spawnPoint = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Length)];
        GameObject enemy = Instantiate(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        //EnemyController ec = enemy.GetComponent<EnemyController>();
        //if (ec != null)
        //    ec.onDeath += OnEnemyDeath;
    }

    private void OnEnemyDeath()
    {
        enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        UpdateUI();
    }
}
