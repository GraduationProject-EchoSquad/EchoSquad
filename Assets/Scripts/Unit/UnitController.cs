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
    
    /*public enum EUnitType
    {
        Player,
        Teammate,
        Zombie,
    }*/
    
    protected LivingEntity LivingEntity;
    protected Animator animator;
    protected EUnitTeamType unitTeamType;

    protected virtual void Start()
    {
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
}