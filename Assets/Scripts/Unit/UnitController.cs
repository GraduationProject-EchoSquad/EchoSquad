using System;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    protected LivingEntity health;
    protected Animator animator;

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<LivingEntity>();
        health.OnDeath += HandleDeath;
    }

    protected virtual void HandleDeath()
    {
    }
}