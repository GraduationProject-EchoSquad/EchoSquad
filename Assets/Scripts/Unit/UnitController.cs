using System;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    protected LivingEntity LivingEntity;
    protected Animator animator;

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        LivingEntity = GetComponent<LivingEntity>();
        LivingEntity.OnDeath += HandleDeath;
    }

    protected virtual void HandleDeath()
    {
    }
}