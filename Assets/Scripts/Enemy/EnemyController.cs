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
    ZombieKing,
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
    private bool isAttacking = false;
    private int currentAttackDamage;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();

        // 팀 타입 설정 (Init이 호출되지 않은 경우 대비)
        unitTeamType = EUnitTeamType.Enemy;
    }

    void Update()
    {
        if (IsDead())
        {
            return;
        }

        // 공격 중에는 이동 로직 실행 안 함
        if (isAttacking)
        {
            // 공격 중에는 확실하게 멈춤
            agent.isStopped = true;
            animator.SetBool("Run", false);
            return;
        }

        UpdateAttackTarget();   // 근처 적 확인

        // 공격 대상이 있는지, 그리고 사거리 내에 있는지 확인
        float dist = Mathf.Infinity;
        bool isInAttackRange = false;

        if (attackTarget != null)
        {
            dist = Vector3.Distance(transform.position, attackTarget.position);
            isInAttackRange = (dist <= attackRange);
        }

        // 공격 사거리 내에 있을 경우
        if (isInAttackRange)
        {
            agent.isStopped = true; // 멈춤
            animator.SetBool("Run", false); // 달리기 애니메이션 정지

            // 부드러운 회전을 위해 LookAt 대신 Slerp 사용
            Vector3 lookDir = (attackTarget.position - transform.position).normalized;
            lookDir.y = 0; // Y축은 회전하지 않도록
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            // 공격 쿨다운 처리
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                Attack();
            }
        }
        else // 공격 사거리 밖에 있거나 공격 대상이 없는 경우
        {
            agent.isStopped = false; // 이동 재개
            animator.SetBool("Run", false); // 일단 false로 초기화

            // 이동 타겟 설정 (적 또는 룬)
            moveTarget = attackTarget != null ? attackTarget : FindNearestRune();

            if (moveTarget != null)
            {
                agent.SetDestination(moveTarget.position);

                // NavMeshAgent가 실제로 움직이고 있는지 확인
                bool isMoving = agent.velocity.sqrMagnitude > 0.01f;
                animator.SetBool("Run", isMoving);
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
        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("Run", false); // 확실하게 Run 애니메이션 종료

        if (enemyType == EnemyType.HandAlien)
        {
            int attackType = Random.Range(0, 2);
            if (attackType == 0)
            {
                animator.SetBool("AnkleBite", true);
                currentAttackDamage = 15;
            }
            else
            {
                animator.SetBool("CrochBite", true);
                currentAttackDamage = 35;
            }
        }
        else if (enemyType == EnemyType.Boss)
        {
            int attackType = Random.Range(0, 2);
            if (attackType == 0)
            {
                animator.SetTrigger("Attack");
                currentAttackDamage = 15;
            }
            else
            {
                animator.SetTrigger("Cast Spell");
                currentAttackDamage = 50;
            }
        }
        else
        {
            // 트리거 대신 직접 상태 전환 (Run 상태에서도 작동)
            animator.Play("Attack", 0);
            currentAttackDamage = (enemyType == EnemyType.Zombie) ? 20 : 10;
        }

        Debug.Log($"[{enemyType}] Attack started - isAttacking: {isAttacking}");
    }

    // [애니메이션 이벤트]에서 호출하는 데미지 적용 함수
    public void ApplyDamageToTarget()
    {
        if (attackTarget != null) // 타겟이 여전히 유효한지 확인
        {
            PlayerHealth health = attackTarget.GetComponent<PlayerHealth>();
            if (health != null)
            {
                DamageMessage damageMessage;
                damageMessage.damager = gameObject;
                damageMessage.amount = currentAttackDamage; // 저장된 데미지 사용
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

    // [애니메이션 이벤트]에서 호출하는 공격 종료 함수
    public void OnAttackAnimationEnd()
    {
        isAttacking = false; //  공격 종료 (Update 로직 다시 활성화)
        agent.isStopped = false; // 이동 재개 가능하도록

        // HandAlien의 경우 Bool 파라미터 리셋
        if (enemyType == EnemyType.HandAlien)
        {
            animator.SetBool("AnkleBite", false);
            animator.SetBool("CrochBite", false);
            animator.SetBool("IdleOne", true);
        }

        Debug.Log($"[{enemyType}] Attack ended - isAttacking: {isAttacking}");
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

    // 감지 범위와 공격 범위를 시각화하기 위한 Gizmo
    void OnDrawGizmosSelected()
    {
        // 감지 범위 (3f) - 노란색 원
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);

        // 공격 범위 (attackRange) - 빨간색 원
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 플레이 모드일 때만 추가 정보 표시
        if (Application.isPlaying)
        {
            // 현재 공격 대상이 있으면 선으로 연결
            if (attackTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, attackTarget.position);
            }

            // 현재 이동 목표가 있으면 녹색 선으로 연결
            if (moveTarget != null && moveTarget != attackTarget)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, moveTarget.position);
            }

            // 공격 중이면 빨간색 구체 표시
            if (isAttacking)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.3f);
            }
        }
    }
}
