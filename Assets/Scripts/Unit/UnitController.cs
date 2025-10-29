using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    public enum EUnitTeamType
    {
        None,
        Allay,
        Neutral,
        Enemy
    }

    public enum EUnitState
    {
        None,
        Idle,
        Scout, // 정찰
        Combat,
        Supprot,
        Move, // 이동
        Die,
    }

    /*public enum EUnitType
    {
        Player,
        Teammate,
        Zombie,
    }*/

    protected LivingEntity LivingEntity;
    protected Animator animator;
    protected EUnitTeamType unitTeamType;
    protected EUnitState unitState;

    protected virtual void Start()
    {
        ChangeUnitState(EUnitState.Idle);
        animator = GetComponentInChildren<Animator>();
        LivingEntity = GetComponent<LivingEntity>();
        LivingEntity.OnDeath += () => HandleDeath().Forget();
    }

    public void Init(EUnitTeamType newUnitTeamType)
    {
        unitTeamType = newUnitTeamType;
    }

    public virtual async UniTaskVoid HandleDeath()
    {
        ChangeUnitState(EUnitState.Die);
        PubSubManager.Instance.Publish<OnUnitDeathData>(PubSubEvent.OnUnitDeath,
            data => { data.DeathController = this; });
    }

    public EUnitTeamType GetUnitTeamType()
    {
        return unitTeamType;
    }

    public EUnitTeamType GetOppositeTeamType()
    {
        switch (GetUnitTeamType())
        {
            case EUnitTeamType.Allay:
                return EUnitTeamType.Enemy;
            case EUnitTeamType.Enemy:
                return EUnitTeamType.Allay;
            default:
                return EUnitTeamType.None;
        }
    }

    protected void ChangeUnitState(EUnitState newUnitState)
    {
        unitState = newUnitState;
    }
    
    public EUnitState GetUnitState()
    {
        return unitState;
    }

    public bool IsDead()
    {
        return unitState == EUnitState.Die;
    }

    public void ApplyDamage(DamageMessage damageMessage)
    {
        LivingEntity.ApplyDamage(damageMessage);
    }
#if UNITY_EDITOR

    public void ForceDead()
    {
        LivingEntity.Die();
    }
#endif
}