using UnityEngine;
using UnityEngine.AI;

public class TeammateController : UnitController
{
    private const float NavSearchRadius = 100.0f;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private TeammateAI teammateAI;
    
    public UnitController followTarget = null;
    
    public float waitBeforeRelease = 3f;     // 멈춘 뒤 몇 초 후 target 제거
    private float stopTimer = 0f;

    /*//should be normalized
    protected Vector3 moveDirection;
    protected float currentSpeed;
    protected float targetSpeed;
    protected float verticalVelocity;

    [Header("Movement Settings")] public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;
    public float gravity = -15f;
    public float jumpForce = 7.0f;

    [Header("Air Control")] public float airControlFactor = 0.7f;
    public float airControlLerp = 2f;*/

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    
    void Update()
    {
        HandleMovement();
        FollowTarget();
    }
    
    void HandleMovement()
    {
        if (IsGrounded())
        {
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
            animator.SetBool("IsFalling", false);
        }
        else
        {
            animator.SetBool("IsFalling", true);
        }
        
        // characterAnimator?.SetFloat("MoveX", localInputDirection.x);
        // characterAnimator?.SetFloat("MoveY", localInputDirection.z);
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
            navMeshAgent.SetDestination(hit.position); // 보정된 위치로 이동
        }
    }

    public void MoveToUnit(UnitController unitController)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(unitController.transform.position, out hit, NavSearchRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position); // 보정된 위치로 이동
        }
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
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && navMeshAgent.velocity.sqrMagnitude < 0.01f)
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
