using System;
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
        Move, // 정찰
        Die
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
        unitState = EUnitState.Idle;
        animator = GetComponentInChildren<Animator>();
        LivingEntity = GetComponent<LivingEntity>();
        LivingEntity.OnDeath += HandleDeath;
    }

    public void Init(EUnitTeamType newUnitTeamType)
    {
        unitTeamType = newUnitTeamType;
    }

    protected virtual void HandleDeath()
    {
        ChangeUnitState(EUnitState.Die);
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

    public bool IsDead()
    {
        return unitState == EUnitState.Die;
    }
}