using System.Collections;
using UnityEngine;
using TMPro;             // TMP 용
using EasyTextEffects;   // TextEffect가 이 네임스페이스에 있다고 가정

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
        
        // TextEffect 컴포넌트 가져오기
        if (countdownText != null)
            countdownEffect = countdownText.GetComponent<TextEffect>();

        // Intro_Controller를 찾아서 이벤트에 핸들러 등록
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
        // 인트로 끝나면 카운트다운+스폰 시퀀스 실행
        StartCoroutine(HandleWaveSequence());
    }

    private void UpdateUI()
    {
        // 현재 웨이브와 남은 적의 수 표시
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
        //        Debug.Log("모든 웨이브 완료!");
        //    }
        //}
    }

    // 웨이브 전체 흐름: timeBetweenWaves → 카운트다운 → 적 스폰
    private IEnumerator HandleWaveSequence()
    {
        isSpawning = true;

        // (첫 웨이브가 아니라면) 웨이브 간 대기
        if (currentWaveIndex > 0)
            yield return new WaitForSeconds(timeBetweenWaves);

        // 카운트다운 재생
        yield return StartCoroutine(PlayCountdownText());

        // 실제 웨이브 스폰
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

    // 숫자 → "Wave {n}" → "Fight!" 순으로 TMP 텍스트 교체
    private IEnumerator PlayCountdownText()
    {
        if (countdownText == null)
        {
            yield break;
        }

        // (1) 5,4,3,2,1 카운트
        for (int i = countdownStart; i > 0; i--)
        {
            // 예: "<link=wave+fadein+movein>5</link>"
            countdownText.text = $"<link=wave+fadein+movein>{i}</link>";
            // 태그 해석 갱신
            if (countdownEffect != null)
            {
                countdownEffect.UpdateStyleInfos();
                countdownEffect.Refresh(); // 필요한 경우 Refresh() 호출
            }
            yield return new WaitForSeconds(countdownInterval);
        }

        // (2) "Wave {번호}" 표시 (currentWaveIndex가 0부터 시작하므로 +1)
        int waveNumber = currentWaveIndex + 1;
        countdownText.text = $"<link=wave+fadein+movein>Wave {waveNumber}</link>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        yield return new WaitForSeconds(countdownInterval);

        // (3) "Fight!" 표시
        countdownText.text = $"<b><size=250px><link=gradient+wave+rotate+scale>Start!</link></size></b>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        yield return new WaitForSeconds(countdownInterval);

        // (4) 텍스트 숨김
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
