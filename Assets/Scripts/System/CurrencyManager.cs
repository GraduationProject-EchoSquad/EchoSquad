using UnityEngine;

// 게임 내 재화(점수, 돈)를 관리하는 매니저
public class CurrencyManager : Singleton<CurrencyManager>
{
    [Header("Currency Settings")]
    [SerializeField] private int startingMoney = 1000;  // 시작 돈

    private int currentScore;
    private int currentMoney;

    public int Score => currentScore;
    public int Money => currentMoney;

    private void Start()
    {
        // 초기화
        currentScore = 0;
        currentMoney = startingMoney;

        // 적 처치 이벤트 구독
        PubSubManager.Instance.Subscribe<OnEnemyDeathData>(PubSubEvent.OnEnemyDeath, OnEnemyKilled);

        // 초기 UI 업데이트
        PublishMoneyChanged();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (PubSubManager.Instance != null)
        {
            PubSubManager.Instance.Unsubscribe<OnEnemyDeathData>(PubSubEvent.OnEnemyDeath, OnEnemyKilled);
        }
    }

    // 적이 처치되었을 때 호출
    private void OnEnemyKilled(OnEnemyDeathData data)
    {
        // 적 처치 시 돈과 점수 지급
        AddMoney(data.MoneyReward);
        AddScore(data.ScoreReward);

        Debug.Log($"[CurrencyManager] {data.EnemyType} 처치! +{data.MoneyReward}원, +{data.ScoreReward}점");
    }

    // 점수 추가
    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        currentScore += amount;

        // 점수 변경 이벤트 발행
        PubSubManager.Instance.Publish<OnScoreUpdatedData>(PubSubEvent.OnScoreUpdated, data =>
        {
            data.Score = currentScore;
        });
    }

    // 돈 추가
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;
        PublishMoneyChanged();
    }

    // 돈 사용 (구매 등). 충분한 돈이 있어서 성공했으면 true, 실패하면 false
    public bool SpendMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[CurrencyManager] 잘못된 금액입니다.");
            return false;
        }

        if (currentMoney < amount)
        {
            Debug.Log($"[CurrencyManager] 돈이 부족합니다. 필요: {amount}, 보유: {currentMoney}");
            return false;
        }

        currentMoney -= amount;
        PublishMoneyChanged();
        return true;
    }

    // 돈이 충분한지 확인
    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    // 돈 변경 이벤트 발행
    private void PublishMoneyChanged()
    {
        PubSubManager.Instance.Publish<OnMoneyChangedData>(PubSubEvent.OnMoneyChanged, data =>
        {
            data.Money = currentMoney;
        });
    }

    // 재화 초기화 (게임 재시작 시)
    public void ResetCurrency()
    {
        currentScore = 0;
        currentMoney = startingMoney;

        PublishMoneyChanged();

        PubSubManager.Instance.Publish<OnScoreUpdatedData>(PubSubEvent.OnScoreUpdated, data =>
        {
            data.Score = currentScore;
        });
    }
}
