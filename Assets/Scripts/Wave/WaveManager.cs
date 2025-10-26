using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;             // TMP 용
using EasyTextEffects;   // TextEffect가 이 네임스페이스에 있다고 가정

[System.Serializable]
public class Wave
{
    [Header("Normal Wave Settings")]
    public UnitController enemyPrefab;
    public int count;
    public float spawnInterval;
 
    [Header("Boss Wave Settings")]
    public bool isBossWave;
    public UnitController bossPrefab;
 
    [Header("Common Settings")]
    public Transform[] spawnPoints;
}

public class WaveManager : Singleton<WaveManager>
{
    [Header("Waves")]
    public Wave[] waves;

    [Header("Time Between Waves (s)")]
    public float timeBetweenWaves = 5f;

    private int currentWaveIndex;
    private int MaxWaveIndex = 1;
    public int enemiesRemaining;

    private UniTaskCompletionSource<bool> waveCompletionSource;

    protected override void Awake()
    {
        base.Awake();

        // Wave 배열 길이로 MaxWaveIndex 초기화
        MaxWaveIndex = waves.Length;

        // 몬스터 사망 이벤트 구독
        PubSubManager.Instance.Subscribe<OnEnemyDeathData>(PubSubEvent.OnEnemyDeath, OnEnemyDeath);
        PubSubManager.Instance.Subscribe<OnPlayerDeathData>(PubSubEvent.OnPlayerDeath, HandlePlayerDeathDuringWave);
    }

    private void Start()
    {

    }

    // GameManager에서 호출하는 웨이브 시작 메서드
    public async UniTask<bool> StartWaves()
    {
        while (currentWaveIndex < MaxWaveIndex)
        {
            // --- 웨이브 시작 ---
            GameManager.Instance.SetGameState(GameManager.GameState.Wave);
            CountdownUI countdownUI = await UIManager.Instance.GetUI<CountdownUI>(UIManager.EUIData.Countdown);
            countdownUI.gameObject.SetActive(true);
            await countdownUI.PlayCountdownText(currentWaveIndex);
            await RunWave();

            // 플레이어가 모든 생명을 잃었는지 확인 (PlayerController가 EndGame 호출)
            if (GameManager.Instance.CurrentGameState == GameManager.GameState.End)
            {
                return false;
                //break; // 게임 루프 종료
            }

            currentWaveIndex++;
            
            // --- 휴식 시간 시작 ---
            if (currentWaveIndex < MaxWaveIndex)
            {
                GameManager.Instance.SetGameState(GameManager.GameState.Break);
                await BreakTime();
            }
        }

        //승리
        return true;
    }

    private async UniTask RunWave()
    {
        waveCompletionSource = new UniTaskCompletionSource<bool>();
        

        Wave wave = waves[currentWaveIndex];
        SetEnemiesRemaining(wave.count);
        
        PubSubManager.Instance.Publish<OnWaveStartData>(PubSubEvent.OnWaveStart, data =>
        {
            data.WaveIndex = currentWaveIndex + 1;
        });

        // 몬스터 스폰을 백그라운드에서 진행
        if (wave.isBossWave)
        {
            SpawnBoss(wave);
        }
        SpawnEnemiesAsync(wave).Forget();
        

        // 웨이브 종료 조건 (모든 몬스터 사망 또는 플레이어 사망)을 기다림
        bool isSuccess = await waveCompletionSource.Task;

        if (isSuccess == false)
        {
            GameManager.Instance.EndGame(false);
        }

        // 다음 웨이브를 위해 이벤트 구독 해제
        //PubSubManager.Instance.Unsubscribe<OnPlayerDeathData>(PubSubEvent.OnPlayerDeath, HandlePlayerDeathDuringWave);
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
        Debug.Log("end breakTime!");
    }

    private void SpawnEnemy(Wave wave)
    {
        Transform spawnPoint = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Length)];
        UnitManager.Instance.SpawnUnit(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation, UnitController.EUnitTeamType.Enemy);
        
        PubSubManager.Instance.Publish(PubSubEvent.OnEnemySpawn);
    }

    private void SpawnBoss(Wave wave)
    {
        if (wave.bossPrefab == null)
        {
            Debug.LogError($"Boss wave {currentWaveIndex} has no boss prefab assigned!");
            waveCompletionSource.TrySetResult(true); // 오류가 있지만 웨이브를 강제로 종료하여 멈춤 방지
            return;
        }
        // 보스는 보통 정해진 위치에 소환되므로, 첫 번째 스폰 포인트를 사용하거나 별도의 보스 스폰 포인트를 지정할 수 있습니다.
        Transform spawnPoint = wave.spawnPoints[0];
        UnitManager.Instance.SpawnUnit(wave.bossPrefab, spawnPoint.position, spawnPoint.rotation, UnitController.EUnitTeamType.Enemy);
    }

    // 몬스터가 죽을 때마다 호출
    private void OnEnemyDeath(OnEnemyDeathData data)
    {
        SetEnemiesRemaining(enemiesRemaining - 1);

        // 남은 몬스터가 없고, 웨이브가 아직 진행 중이라면 웨이브 종료 처리
        if (enemiesRemaining <= 0 && !waveCompletionSource.Task.Status.IsCompleted())
        {
            waveCompletionSource.TrySetResult(true); // 웨이브 성공으로 종료
        }
    }

    // 웨이브 진행 중 플레이어가 죽었을 때 호출
    private void SetEnemiesRemaining(int count)
    {
        enemiesRemaining = Mathf.Max(0, count);
        PubSubManager.Instance.Publish<OnRemainEnemyCountChangeData>(PubSubEvent.OnRemainEnemyCountChange, data =>
        {
            data.remainEnemyCount = enemiesRemaining;
        });
    }

    // 웨이브 진행 중 플레이어가 죽었을 때 호출
    private void HandlePlayerDeathDuringWave(OnPlayerDeathData data)
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