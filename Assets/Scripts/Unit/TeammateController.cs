using GLTFast.Schema;
using UnityEngine;
using UnityEngine.AI;

public class TeammateController : UnitController
{
    private const float NavSearchRadius = 100.0f;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private TeammateAI teammateAI;
    private UnitShooter unitShooter;

    private float waitBeforeRelease = 3f; // 멈춘 뒤 몇 초 후 target 제거
    private float stopTimer = 0f;
    private UnitController followTarget = null;

    public float speed = 6f;
    [SerializeField] float turnSpeed = 120f;
    [SerializeField] float accel = 8f;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();
        unitShooter = GetComponent<UnitShooter>();
        navMeshAgent.speed = speed;
        navMeshAgent.angularSpeed = turnSpeed;
        navMeshAgent.acceleration = accel;
    }

    void Update()
    {
        HandleMovement();
        FollowTarget();
        
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                navMeshAgent.ResetPath(); // 목적지 초기화
                Debug.Log("도달 완료. 경로 초기화됨.");
            }
        }
    }

    void HandleMovement()
    {
        /*if (IsGrounded())
        {
            animator.SetBool("IsFalling", false);
        }
        else
        {
            animator.SetBool("IsFalling", true);
        }*/

        float speed = navMeshAgent.velocity.magnitude;
        Vector3 worldVel = navMeshAgent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        Vector2 moveInput = new Vector2(localVel.x, localVel.z).normalized;
        animator.SetFloat("Horizontal Move", moveInput.x * speed, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical Move", moveInput.y * speed, 0.05f, Time.deltaTime);
    }

    bool IsGrounded()
    {
        float rayDistance = 1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, rayDistance);
    }


    public void MoveToObject(GameObject gameObject)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(gameObject.transform.position, out hit, NavSearchRadius, NavMesh.AllAreas))
        {
            followTarget = null;
            navMeshAgent.SetDestination(hit.position); // 보정된 위치로 이동
        }
    }

    public void MoveToUnit(UnitController unitController)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(unitController.transform.position, out hit, NavSearchRadius, NavMesh.AllAreas))
        {
            followTarget = null;
            navMeshAgent.SetDestination(hit.position); // 보정된 위치로 이동
        }
    }

    public void SetFollowUnit(UnitController followUnitController)
    {
        followTarget = followUnitController;
    }

    private void FollowTarget()
    {
        if (followTarget == null)
        {
            return;
        }

        var targetPosition = followTarget.transform.position;
        navMeshAgent.SetDestination(targetPosition);

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= navMeshAgent.stoppingDistance)
        {
            // agent가 실제로 멈췄는지 (목표 도착 + 속도 거의 없음)
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
                navMeshAgent.velocity.sqrMagnitude < 0.01f)
            {
                stopTimer += Time.deltaTime;

                if (stopTimer >= waitBeforeRelease)
                {
                    Debug.Log("타겟 도착 후 3초 경과 → 타겟 해제");
                    followTarget = null;
                    stopTimer = 0f;
                }
            }
            else
            {
                // 아직 멈춘 상태가 아니므로 타이머 초기화
                stopTimer = 0f;
            }
        }
        else
        {
            // 아직 도착하지 않음
            stopTimer = 0f;
        }
    }

    public TeammateAI GetTeammateAI()
    {
        return teammateAI;
    }
}