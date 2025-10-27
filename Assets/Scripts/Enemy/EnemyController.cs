using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Collections.Unicode;

public enum EnemyType
{
    None,
    Zombie,
    Boss,
    HandAlien,
    BrainLeg,
    WhiteLeg,
    BlackLeg
}

public class EnemyController : UnitController
{
    public EnemyType enemyType;
    public int moneyReward = 50;  // 처치 시 보상 금액
    public int scoreReward = 100; // 처치 시 보상 점수
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
        if (IsDead())
        {
            return;
        }

        UpdateAttackTarget();   // 근처 적 확인
                                // 공격 대상이 있으면 그걸 추적

        moveTarget = attackTarget != null ? attackTarget : FindNearestRune();

        if (moveTarget != null)
            agent.SetDestination(moveTarget.position);


        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("Run", isMoving);

        // 공격 처리
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
            if (dist <= 3f && dist < minDist)  // 공격 범위보다 넉넉하게 탐색 범위 설정
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
        int damage;

        // HandAlien 계열은 다양한 공격 패턴 사용
        if (enemyType == EnemyType.HandAlien)
        {
            int attackType = Random.Range(0, 2);
            if (attackType == 0)
            {
                animator.SetTrigger("AnkleBiteTrigger");
                damage = 15; // 발목 물기 - 약한 공격
            }
            else
            {
                animator.SetTrigger("CrochBiteTrigger");
                damage = 25; // 사타구니 물기 - 강한 공격!
            }
        }
        else
        {
            // 일반 적들은 기본 공격
            animator.SetTrigger("Attack");
            damage = (enemyType == EnemyType.Zombie) ? 20 : 10;
        }

        if (attackTarget != null)
        {
            PlayerHealth health = attackTarget.GetComponent<PlayerHealth>();
            if (health != null)
            {
                DamageMessage damageMessage;

                damageMessage.damager = gameObject;
                damageMessage.amount = damage;
                damageMessage.hitPoint = attackTarget.transform.position;
                damageMessage.hitNormal = Vector3.up;
                health.ApplyDamage(damageMessage);
            }

            // 룬 체력 스크립트 작성 후 주석 해제
            /*var rune = target.GetComponent<RuneHP>();
            if (rune != null)
            {
                rune.TakeDamage(10);
            }*/
        }
    }
    
    public override async UniTaskVoid HandleDeath()
    {
        base.HandleDeath();
        agent.enabled = false;
        animator.SetTrigger("Die");

        // 몬스터 정보와 함께 죽음 이벤트 발행
        PubSubManager.Instance.Publish<OnEnemyDeathData>(PubSubEvent.OnEnemyDeath, data =>
        {
            data.Enemy = gameObject;
            data.EnemyType = enemyType;
            data.MoneyReward = moneyReward;
            data.ScoreReward = scoreReward;
        });

        Debug.Log("Enemy died!");
        //Destroy(gameObject, 2f);

        await UniTask.WaitForSeconds(2f);

        UnitManager.Instance.DeleteUnit(this);
    }
}
