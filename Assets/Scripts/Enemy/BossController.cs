using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Collections.Unicode;

public class BossController : EnemyController
{
    [Header("Boss Attack Settings")]
    [SerializeField] private int normalAttackDamage = 15;
    [SerializeField] private int spellAttackDamage = 50;
    [SerializeField] private float attackAnimationDuration = 1.5f;
    [SerializeField] private float spellAnimationDuration = 2.5f;

    private float attackEndTimer = 0f;
    private bool isWaitingForAnimationEnd = false;
    private bool isRunning = false;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Boss;
    }

    protected override void Update()
    {
        base.Update();

        if (isWaitingForAnimationEnd)
        {
            attackEndTimer -= Time.deltaTime;
            if (attackEndTimer <= 0f)
            {
                OnAttackAnimationEnd();
                isWaitingForAnimationEnd = false;
            }
        }

        // Boss Run 처리
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            bool shouldRun = !isAttacking && !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool("Run", shouldRun);
        }
    }

    protected override void ExecuteAttack()
    {
        int attackType = Random.Range(0, 2);

        if (attackType == 0)
        {
            animator.SetTrigger("Attack");
            SetCurrentAttackDamage(normalAttackDamage);
            attackEndTimer = attackAnimationDuration;
            Debug.Log($"[Boss] Normal Attack - Damage: {normalAttackDamage}");
        }
        else
        {
            animator.SetTrigger("Cast Spell");
            SetCurrentAttackDamage(spellAttackDamage);
            attackEndTimer = spellAnimationDuration;
            Debug.Log($"[Boss] Cast Spell - Damage: {spellAttackDamage}");
        }

        isWaitingForAnimationEnd = true;
    }
}
