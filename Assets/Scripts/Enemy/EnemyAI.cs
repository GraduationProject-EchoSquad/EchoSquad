using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float attackRange = 2f;
    private Transform target;
    private NavMeshAgent agent;
    private float attackCooldown = 1f;
    private float attackTimer = 0f;

    Animator enemyAnimator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
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
        enemyAnimator.SetTrigger("Attack");
        if (target != null)
        {
            PlayerHP health = target.GetComponent<PlayerHP>();
            if (health != null)
                health.TakeDamage(10);
        }
    }
}
