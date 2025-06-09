using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    protected UnitShooter unitShooter;
    protected Animator animator;
    private NavMeshAgent navMeshAgent;

    public float speed = 6f;
    public float jumpVelocity = 20f;
    [Range(0.01f, 1f)] public float airControlPercent;

    public float speedSmoothTime = 0.1f;
    public float turnSmoothTime = 0.1f;

    protected float speedSmoothVelocity;
    protected float turnSmoothVelocity;

    protected float currentVelocityY;

    protected virtual float currentSpeed =>
        navMeshAgent.velocity.magnitude;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        unitShooter = GetComponent<UnitShooter>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {
        UpdateAnimation(navMeshAgent.velocity);
    }

    protected virtual void FixedUpdate()
    {
        if (unitShooter.GetAimTargetUnit() != null)
        {
            Vector3 direction = (unitShooter.GetAimTargetUnit().transform.position - transform.position).normalized;
            if (direction == Vector3.zero) return;

            float targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Rotate(targetRotation);
        }
    }

    protected void Rotate(float targetRotation)
    {
        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetRotation,
            ref turnSmoothVelocity,
            turnSmoothTime
        );

        /*transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
            ref turnSmoothVelocity, turnSmoothTime);*/
    }

    protected virtual void UpdateAnimation(Vector2 velocity)
    {
        // 속도 정규화 및 방향 추출
        float animationSpeedPercent = velocity.magnitude / speed; // speed는 최대 속도
        Vector3 localVelocity = transform.InverseTransformDirection(velocity); // 로컬 방향 기준 변환
        Vector2 moveDir = new Vector2(localVelocity.x, localVelocity.z); // z는 전진/후진

        animator.SetFloat("Horizontal Move", moveDir.x * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical Move", moveDir.y * animationSpeedPercent, 0.05f, Time.deltaTime);
    }
}