using System;
using System.Collections.Generic;
using UnityEngine;

public enum PubSubEvent
{
    OnPlayerDeath,
    OnEnemySpawn,
    OnEnemyDeath,
    OnScoreUpdated,
    OnWaveStart,
}

public class PubSubDataBase
{
}

public class OnPlayerDeathData : PubSubDataBase
{
    public int liveCount { get; private set; }
}

public class PubSubManager : Singleton<PubSubManager>
{
   // 싱글톤 인스턴스
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

    // 이벤트를 저장할 딕셔너리
    // Key: PubSubEvent enum
    // Value: 해당 이벤트에 등록된 Action들 (object는 모든 타입의 파라미터를 받기 위함)
    private Dictionary<PubSubEvent, Action<PubSubDataBase>> _eventDictionary = new Dictionary<PubSubEvent, Action<PubSubDataBase>>();

    /// <summary>
    /// 이벤트 구독 (Subscribe)
    /// </summary>
    /// <param name="eventType">구독할 이벤트 타입</param>
    /// <param name="listener">실행할 메서드</param>
    public void Subscribe(PubSubEvent eventType, Action<PubSubDataBase> listener)
    {
        if (_eventDictionary.ContainsKey(eventType))
        {
            _eventDictionary[eventType] += listener;
        }
        else
        {
            _eventDictionary.Add(eventType, listener);
        }
    }

    /// <summary>
    /// 이벤트 구독 취소 (Unsubscribe)
    /// </summary>
    /// <param name="eventType">구독 취소할 이벤트 타입</param>
    /// <param name="listener">제거할 메서드</param>
    public void Unsubscribe(PubSubEvent eventType, Action<PubSubDataBase> listener)
    {
        if (_instance == null || !_eventDictionary.ContainsKey(eventType)) return;

        _eventDictionary[eventType] -= listener;

        // 더 이상 구독자가 없으면 딕셔너리에서 제거 (메모리 관리)
        if (_eventDictionary[eventType] == null)
        {
            _eventDictionary.Remove(eventType);
        }
    }

    /// <summary>
    /// 이벤트 발행 (Publish)
    /// </summary>
    /// <param name="eventType">발행할 이벤트 타입</param>
    /// <param name="data">전달할 파라미터</param>
    public void Publish(PubSubEvent eventType, PubSubDataBase data = null)
    {
        if (_eventDictionary.TryGetValue(eventType, out Action<PubSubDataBase> thisEvent))
        {
            thisEvent?.Invoke(data);
        }
    }
}
