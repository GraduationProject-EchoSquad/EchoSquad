using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;             // TMP 용
using EasyTextEffects;   // TextEffect가 이 네임스페이스에 있다고 가정

[System.Serializable]
public class Wave
{
    public UnitController enemyPrefab;
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

    private int currentWaveIndex;
    private const int MaxWaveIndex = 1;
    private int enemiesRemaining;
    private UniTaskCompletionSource<bool> waveCompletionSource;

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
        
        // 몬스터 사망 이벤트 구독
        PubSubManager.Instance.Subscribe(PubSubEvent.OnEnemyDeath, (data) => OnEnemyDeath());
    }

    private void Start()
    {
        
    }
    private void HandleIntroFinished()
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Wave);
        // 인트로 끝나면 게임 루프 시작
        GameLoop().Forget();
    }

    private void UpdateUI()
    {
        // 현재 웨이브와 남은 적의 수 표시
        UIManager.Instance.UpdateWaveText(currentWaveIndex, enemiesRemaining);
    }

    // 게임의 전체 흐름을 관리하는 메인 루프
    private async UniTaskVoid GameLoop()
    {
        UnitManager.Instance.InitSpawnUnit();

        while (currentWaveIndex < MaxWaveIndex)
        {
            // --- 웨이브 시작 ---
            GameManager.Instance.SetGameState(GameManager.GameState.Wave);
            await PlayCountdownText();
            await RunWave();

            // 플레이어가 모든 생명을 잃었는지 확인 (PlayerController가 EndGame 호출)
            if (GameManager.Instance.CurrentGameState == GameManager.GameState.End)
            {
                break; // 게임 루프 종료
            }

            // --- 휴식 시간 시작 ---
            GameManager.Instance.SetGameState(GameManager.GameState.Break);
            await BreakTime();

            currentWaveIndex++;
        }

        // 모든 웨이브를 클리어했거나 게임 오버 상태
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.End)
        {
            GameManager.Instance.EndGame(); // "You Win" 시나리오
        }
    }

    private async UniTask RunWave()
    {
        waveCompletionSource = new UniTaskCompletionSource<bool>();
        PubSubManager.Instance.Subscribe(PubSubEvent.OnPlayerDeath, HandlePlayerDeathDuringWave);

        Wave wave = waves[currentWaveIndex];
        enemiesRemaining = wave.count;
        UpdateUI();
        
        PubSubManager.Instance.Publish(PubSubEvent.OnWaveStart);

        // 몬스터 스폰을 백그라운드에서 진행
        SpawnEnemiesAsync(wave).Forget();

        // 웨이브 종료 조건 (모든 몬스터 사망 또는 플레이어 사망)을 기다림
        await waveCompletionSource.Task;

        // 다음 웨이브를 위해 이벤트 구독 해제
        PubSubManager.Instance.Unsubscribe(PubSubEvent.OnPlayerDeath, HandlePlayerDeathDuringWave);
    }

    private async UniTask SpawnEnemiesAsync(Wave wave)
    {
        for (int i = 0; i < wave.count; i++)
        {
            // 웨이브가 이미 종료되었다면 스폰 중지
            if (waveCompletionSource.Task.Status.IsCompleted()) break;
            
            SpawnEnemy(wave);
            await UniTask.WaitForSeconds(wave.spawnInterval);
        }
    }

    private async UniTask BreakTime()
    {
        // TODO: 휴식 시간 UI 표시 (예: "Next wave in...")
        await UniTask.WaitForSeconds(timeBetweenWaves);
    }

    // 숫자 → "Wave {n}" → "Fight!" 순으로 TMP 텍스트 교체
    private async UniTask PlayCountdownText()
    {
        if (countdownText == null)
        {
            return;
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
            await UniTask.WaitForSeconds(countdownInterval);
        }

        // (2) "Wave {번호}" 표시 (currentWaveIndex가 0부터 시작하므로 +1)
        int waveNumber = currentWaveIndex + 1;
        countdownText.text = $"<link=wave+fadein+movein>Wave {waveNumber}</link>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        await UniTask.WaitForSeconds(countdownInterval);

        // (3) "Fight!" 표시
        countdownText.text = $"<b><size=250px><link=gradient+wave+rotate+scale>Start!</link></size></b>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        await UniTask.WaitForSeconds(countdownInterval);

        // (4) 텍스트 숨김
        countdownText.text = string.Empty;
        if (countdownText.gameObject.activeSelf)
            countdownText.gameObject.SetActive(false);
       
    }

    private void SpawnEnemy(Wave wave)
    {
        Transform spawnPoint = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Length)];
        UnitManager.Instance.SpawnUnit(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation, UnitController.EUnitTeamType.Enemy);
        
        PubSubManager.Instance.Publish(PubSubEvent.OnEnemySpawn);
    }

    // 몬스터가 죽을 때마다 호출
    private void OnEnemyDeath()
    {
        enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        UpdateUI();

        // 남은 몬스터가 없고, 웨이브가 아직 진행 중이라면 웨이브 종료 처리
        if (enemiesRemaining <= 0 && !waveCompletionSource.Task.Status.IsCompleted())
        {
            waveCompletionSource.TrySetResult(true); // 웨이브 성공으로 종료
        }
    }

    // 웨이브 진행 중 플레이어가 죽었을 때 호출
    private void HandlePlayerDeathDuringWave(PubSubDataBase data)
    {
        // 웨이브가 아직 진행 중이라면 웨이브 종료 처리
        if (!waveCompletionSource.Task.Status.IsCompleted())
            waveCompletionSource.TrySetResult(false); // 웨이브 실패로 종료
    }

    // 디버그용: 현재 웨이브를 강제로 종료시킵니다.
    public void ForceEndWave(bool success)
    {
        if (waveCompletionSource != null && !waveCompletionSource.Task.Status.IsCompleted())
        {
            waveCompletionSource.TrySetResult(success);
        }
    }
}