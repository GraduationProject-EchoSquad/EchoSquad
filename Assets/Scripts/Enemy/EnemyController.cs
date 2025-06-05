using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : UnitController
{
    public float attackRange = 2f;
    private Transform target;
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
        UpdateTarget();

        if (target != null)
        {
            agent.SetDestination(target.position);

            float dist = Vector3.Distance(transform.position, target.position);
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

    void UpdateTarget()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] ai = GameObject.FindGameObjectsWithTag("AI");

        GameObject[] targets = player.Concat(ai).ToArray();

        float minDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject obj in targets)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = obj.transform;
            }
        }

        target = nearest;
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        if (target != null)
        {
            PlayerHP health = target.GetComponent<PlayerHP>();
            if (health != null)
                health.TakeDamage(10);
        }
    }
    
    protected override void HandleDeath()
    {
        animator.SetTrigger("Die");
        Debug.Log("Zombie died!");
        Destroy(gameObject, 2f);
    }
}
