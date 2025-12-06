using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;             // TMP 용
using EasyTextEffects;   // TextEffect가 이 네임스페이스에 있다고 가정

[System.Serializable]
public class EnemySpawnInfo
{
    public UnitController enemyPrefab;
    public int count;
}

[System.Serializable]
public class Wave
{
    [Header("Normal Wave Settings")]
    [Tooltip("이 웨이브에서 스폰할 적 타입들")]
    public EnemySpawnInfo[] enemyTypes;
    public float spawnInterval;

    [Header("Boss Wave Settings")]
    public bool isBossWave;
    public UnitController bossPrefab;

    [Header("Common Settings")]
    public Transform[] spawnPoints;

    // 총 적의 수 계산 (내부 사용)
    public int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var enemyInfo in enemyTypes)
        {
            total += enemyInfo.count;
        }

        if (isBossWave)
        {
            total += 1;
        }
        
        return total;
    }
}

public class WaveManager : Singleton<WaveManager>
{
    [Header("Waves")]
    public Wave[] waves;

    [Header("Time Between Waves (s)")]
    public float timeBetweenWaves = 5f;

    public int currentWaveIndex { get; private set; }
    private int MaxWaveIndex = 2;
    public int enemiesRemaining;

    private UniTaskCompletionSource<bool> waveCompletionSource;

    protected override void Awake()
    {
        base.Awake();

        // Wave 배열 길이로 MaxWaveIndex 초기화
        MaxWaveIndex = waves.Length;

        // 몬스터 사망 이벤트 구독
        PubSubManager.Instance.Subscribe<OnEnemyDeathData>(PubSubEvent.OnEnemyDeath, OnEnemyDeath);
        //PubSubManager.Instance.Subscribe<OnPlayerDeathData>(PubSubEvent.OnPlayerDeath, HandlePlayerDeathDuringWave);
        PubSubManager.Instance.Subscribe<OnUnitDeathData>(PubSubEvent.OnUnitDeath, HandlePlayerDeathDuringWave);
    }

    private void Start()
    {

    }

    // GameManager에서 호출하는 웨이브 시작 메서드
    public async UniTask<bool> StartWaves()
    {
        currentWaveIndex = 1;
        
        while (currentWaveIndex < MaxWaveIndex)
        {
            // --- 웨이브 시작 ---
            GameManager.Instance.SetGameState(GameManager.GameState.Wave);
            CountdownUI countdownUI = await UIManager.Instance.Show<CountdownUI>(UIManager.EUIData.Countdown);
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


        Wave wave = waves[currentWaveIndex - 1];
        SetEnemiesRemaining(wave.GetTotalEnemyCount());

        PubSubManager.Instance.Publish<OnWaveStartData>(PubSubEvent.OnWaveStart, data =>
        {
            data.WaveIndex = currentWaveIndex;
        });

        // 일반 몬스터 스폰 후 보스 스폰
        await SpawnEnemiesAsync(wave);

        if (wave.isBossWave)
        {
            SpawnBoss(wave);
        }

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
        // 스폰할 적들의 리스트 생성
        List<UnitController> spawnQueue = new List<UnitController>();

        foreach (var enemyInfo in wave.enemyTypes)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                spawnQueue.Add(enemyInfo.enemyPrefab);
            }
        }

        // 리스트를 섞어서 랜덤한 순서로 스폰
        ShuffleList(spawnQueue);

        // 모든 적을 스폰
        for (int i = 0; i < spawnQueue.Count; i++)
        {
            // 웨이브가 이미 종료되었다면 스폰 중지
            if (waveCompletionSource.Task.Status.IsCompleted()) break;

            SpawnEnemy(wave, spawnQueue[i]);
            await UniTask.WaitForSeconds(wave.spawnInterval);
        }
    }

    // Fisher-Yates 셔플 알고리즘
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private async UniTask BreakTime()
    {
        // TODO: 휴식 시간 UI 표시 (예: "Next wave in...")
        await UniTask.WaitForSeconds(timeBetweenWaves);
        Debug.Log("end breakTime!");
    }

    private void SpawnEnemy(Wave wave, UnitController enemyPrefab)
    {
        Transform spawnPoint = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Length)];
        UnitManager.Instance.SpawnUnit(enemyPrefab, spawnPoint.position, spawnPoint.rotation, UnitController.EUnitTeamType.Enemy);

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
    private void HandlePlayerDeathDuringWave(OnUnitDeathData data)
    {
        if (data.DeathController.GetUnitTeamType() == UnitController.EUnitTeamType.Allay)
        {
            foreach (var allayUnit in  UnitManager.Instance.GetUnitTeamTypeList(UnitController.EUnitTeamType.Allay))
            {
                if (allayUnit.IsDead() == false)
                {
                    return;
                }
            }
            
            // 웨이브가 아직 진행 중이라면 웨이브 종료 처리
            if (!waveCompletionSource.Task.Status.IsCompleted())
                waveCompletionSource.TrySetResult(false); // 웨이브 실패로 종료
        }
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