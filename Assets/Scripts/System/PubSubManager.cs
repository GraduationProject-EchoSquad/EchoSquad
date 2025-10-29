using System;
using System.Collections.Generic;
using UnityEngine;

public enum PubSubEvent
{
    OnPlayerDeath,
    OnLifeChanged,
    OnEnemySpawn,
    OnEnemyDeath,
    OnScoreUpdated,
    OnWaveStart,
    OnRemainEnemyCountChange,
    OnPreparationComplete,  // 동료 능력치 설정 완료
    OnAmmoUpdated,
    OnMoneyChanged,         // 돈 변경
    OnItemPurchased,        // 아이템 구매 성공
    OnPurchaseFailed,       // 구매 실패
    OnUnitDeath,

    OnGameEnd
}

public class PubSubDataBase
{
}

public class OnPlayerDeathData : PubSubDataBase
{

}

public class OnEnemyDeathData : PubSubDataBase
{
    public GameObject Enemy;
    public EnemyType EnemyType;
    public int MoneyReward;
    public int ScoreReward;
}

public class OnLifeChangedData : PubSubDataBase
{
    public int LiveCount;
}

public class OnWaveStartData : PubSubDataBase
{
    public int WaveIndex;
}

public class OnScoreUpdatedData : PubSubDataBase
{
    public int Score;
}

public class OnAmmoUpdatedData : PubSubDataBase
{
    public int MagAmmo;
    public int AmmoRemain;
}

public class OnRemainEnemyCountChangeData : PubSubDataBase
{
    public int remainEnemyCount;
}

public class OnGameEndData : PubSubDataBase
{
    public bool IsWin;
}

public class OnMoneyChangedData : PubSubDataBase
{
    public int Money;
}

public class OnItemPurchasedData : PubSubDataBase
{
    public string ItemId;
    public string ItemName;
}

public class OnPurchaseFailedData : PubSubDataBase
{
    public string Reason;
}

public class OnUnitDeathData : PubSubDataBase
{
    public UnitController DeathController;
}

public class PubSubManager
{
    #region Singleton
    private static PubSubManager _instance;
    public static PubSubManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PubSubManager();
            }
            return _instance;
        }
    }
    #endregion

    private readonly Dictionary<PubSubEvent, Delegate> _eventDictionary = new Dictionary<PubSubEvent, Delegate>();

    #region Subscribe (구독)

    /// <summary>
    /// 데이터가 있는 이벤트를 구독합니다.
    /// </summary>
    /// <typeparam name="T">전달받을 데이터의 타입 (PubSubDataBase 상속)</typeparam>
    /// <param name="eventType">구독할 이벤트 타입</param>
    /// <param name="listener">이벤트 발생 시 호출될 메서드</param>
    public void Subscribe<T>(PubSubEvent eventType, Action<T> listener) where T : PubSubDataBase
    {
        if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
        {
            // 이미 다른 타입의 리스너가 등록되어 있는지 확인 (선택적이지만 안정성을 높임)
            if (existingDelegate != null && existingDelegate.GetType() != typeof(Action<T>))
            {
                var args = existingDelegate.GetType().GetGenericArguments();
                string typeName = args.Length > 0 ? args[0].Name : existingDelegate.GetType().Name;
                Debug.LogError($"[PubSubManager] 구독 실패: 이벤트 '{eventType}'에 다른 데이터 타입({typeName})이 이미 등록되어 있습니다.");
                return;
            }
            _eventDictionary[eventType] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventDictionary[eventType] = listener;
        }
    }

    /// <summary>
    /// 데이터가 없는 이벤트를 구독합니다.
    /// </summary>
    /// <param name="eventType">구독할 이벤트 타입</param>
    /// <param name="listener">이벤트 발생 시 호출될 메서드</param>
    public void Subscribe(PubSubEvent eventType, Action listener)
    {
        if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
        {
            _eventDictionary[eventType] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventDictionary[eventType] = listener;
        }
    }

    #endregion

    #region Unsubscribe (구독 취소)

    /// <summary>
    /// 데이터가 있는 이벤트 구독을 취소합니다.
    /// </summary>
    public void Unsubscribe<T>(PubSubEvent eventType, Action<T> listener) where T : PubSubDataBase
    {
        if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, listener);
            if (newDelegate == null)
            {
                _eventDictionary.Remove(eventType);
            }
            else
            {
                _eventDictionary[eventType] = newDelegate;
            }
        }
    }

    /// <summary>
    /// 데이터가 없는 이벤트 구독을 취소합니다.
    /// </summary>
    public void Unsubscribe(PubSubEvent eventType, Action listener)
    {
        if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, listener);
            if (newDelegate == null)
            {
                _eventDictionary.Remove(eventType);
            }
            else
            {
                _eventDictionary[eventType] = newDelegate;
            }
        }
    }

    #endregion

    #region Publish (발행)
    
    public void Publish<T>(PubSubEvent eventType, Action<T> initializer) where T : PubSubDataBase, new()
    {
        // 1. 기본 생성자로 데이터 객체를 생성합니다.
        var data = new T();

        // 2. initializer 액션을 호출하여 사용자 코드에서 데이터를 설정합니다.
        initializer?.Invoke(data);

        // 3. 기존 Publish 메서드를 호출하여 최종적으로 이벤트를 발행합니다.
        Publish(eventType, data);
    }

    /// <summary>
    /// 데이터가 있는 이벤트를 발행합니다.
    /// </summary>
    public void Publish<T>(PubSubEvent eventType, T data) where T : PubSubDataBase
    {
        if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
        {
            // Action<T> 타입의 리스너만 찾아서 호출
            foreach (var handler in existingDelegate.GetInvocationList())
            {
                if (handler is Action<T> action)
                {
                    action.Invoke(data);
                }
            }
        }
    }

    /// <summary>
    /// 데이터가 없는 이벤트를 발행합니다.
    /// </summary>
    public void Publish(PubSubEvent eventType)
    {
        if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
        {
            // Action 타입의 리스너만 찾아서 호출
            foreach (var handler in existingDelegate.GetInvocationList())
            {
                if (handler is Action action)
                {
                    action.Invoke();
                }
            }
        }
    }

    #endregion
}