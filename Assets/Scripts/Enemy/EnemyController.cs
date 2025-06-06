using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Collections.Unicode;

public class EnemyController : UnitController
{
    public float attackRange = 1f;
    private Transform moveTarget;  
    private Transform attackTarget;
    private NavMeshAgent agent;
    private float attackCooldown = 1f;
    private float attackTimer = 0f;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        UpdateAttackTarget();   // ��ó �� Ȯ��
                                // ���� ����� ������ �װ� ����

        moveTarget = attackTarget != null ? attackTarget : FindNearestRune();

        if (moveTarget != null)
            agent.SetDestination(moveTarget.position);

        // ���� ó��
        if (attackTarget != null)
        {
            float dist = Vector3.Distance(transform.position, attackTarget.position);
            if (dist <= attackRange)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    Attack();
                }
            }
        }
    }


    void UpdateAttackTarget()
    {
        List<UnitController> units = UnitManager.Instance.GetUnitTeamTypeList(GetOppositeTeamType());

        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (UnitController unit in units)
        {
            float dist = Vector3.Distance(transform.position, unit.transform.position);
            if (dist <= 3f && dist < minDist)  // ���� �������� �˳��ϰ� Ž�� ���� ����
            {
                minDist = dist;
                nearest = unit.transform;
            }
        }

        attackTarget = nearest;
    }

    Transform FindNearestRune()
    {
        GameObject[] runes = GameObject.FindGameObjectsWithTag("Rune");
        float minDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject rune in runes)
        {
            float dist = Vector3.Distance(transform.position, rune.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = rune;
            }
        }

        return nearest?.transform;
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        if (attackTarget != null)
        {
            PlayerHP health = attackTarget.GetComponent<PlayerHP>();
            if (health != null)
                health.TakeDamage(10);

            // �� ü�� ��ũ��Ʈ �ۼ� �� �ּ� ����
            /*var rune = target.GetComponent<RuneHP>();
            if (rune != null)
            {
                rune.TakeDamage(10);
            }*/
        }
    }
    
    protected override void HandleDeath()
    {
        animator.SetTrigger("Die");
        Debug.Log("Zombie died!");
        Destroy(gameObject, 2f);
    }
}
