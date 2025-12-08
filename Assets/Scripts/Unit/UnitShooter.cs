using System.Linq;
using UnityEngine;

public class UnitShooter : MonoBehaviour
{
    public enum AimState
    {
        Idle,
        HipFire
    }

    protected AimState aimState;

    public Gun gun; // 사용할 총
    public LayerMask excludeTarget;
    protected Animator unitAnimator; // 애니메이터 컴포넌트

    protected float waitingTimeForReleasingAim = 2.5f;
    protected float lastFireInputTime;

    public UnitController unit { get; private set; }
    private UnitController aimTargetUnit;
    private EnemyType targetEnemyType = EnemyType.None;

    [Header("Visibility Setting")] [SerializeField]
    float viewDistance = 10f; // 최대 탐지 거리

    [SerializeField] float viewAngle = 120f; // 시야각(°)
    [SerializeField] private float autoTargetRange; // 자동 타겟팅 범위 (시야 무관)
    [SerializeField] float eyeHeight; //  Raycast 시작 높이

    protected bool hasEnoughDistance =>
        !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y, gun.fireTransform.position,
            ~excludeTarget);

    protected virtual void Start()
    {
        unitAnimator = GetComponent<Animator>();
        unit = GetComponent<UnitController>();

        autoTargetRange = 10f;
        InitializeEyeHeight();
    }

    protected virtual void InitializeEyeHeight()
    {
        var col = GetComponent<CapsuleCollider>();
        eyeHeight = col.center.y;
    }

    private void OnEnable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(true);
        gun.Setup(this);
    }

    private void OnDisable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (unit.IsDead())
        {
            return;
        }
        UpdateAimTarget();
        Shoot();
        gun.DrawPreviewLine();

        // 1) 카메라 계산 부분을 무시하고 상체 ‘들기’에 대응되는 최소값(예: 0.8f)만 넘겨주기
        float fixedAngle = 10f; // 1에 가까울수록 더 완전하게 상체를 든 상태
        float animAngle = Mathf.InverseLerp(0f, 20f, fixedAngle);
        // camY=6 → animAngle=0  /  camY=8 → animAngle=0.5  /  camY=10 → animAngle=1
        unitAnimator.SetFloat("Angle", animAngle);
        //playerAnimator.SetFloat("Angle", fixedAngle);

        if (Time.time >= lastFireInputTime + waitingTimeForReleasingAim)
        {
            aimState = AimState.Idle;
        }
    }

    public virtual void Shoot()
    {
        if (aimTargetUnit != null && IsVisibleTarget(aimTargetUnit))
        {
            bool fired = gun.Fire(aimTargetUnit.transform.position);
            if (fired)
            {
                unitAnimator.SetTrigger("Shoot");
                if (gun.magAmmo <= 0)
                {
                    Reload();
                }
            }
        }
    }

    public bool IsVisibleTarget(UnitController targetController)
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Vector3 dir = targetController.transform.position - eyePos;
        float distSqr = dir.sqrMagnitude;
        if (distSqr > viewDistance * viewDistance) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;

        if (Physics.Raycast(eyePos, dir.normalized, out var hit, Mathf.Sqrt(distSqr), ~excludeTarget))
        {
            var hitCtrl = hit.collider.GetComponentInParent<UnitController>();
            if (hitCtrl != targetController) return false;
        }

        return true;
    }

    public void Reload()
    {
        // 재장전 입력 감지시 재장전
        if (gun.Reload()) unitAnimator.SetTrigger("Reload");
    }

    protected virtual void UpdateAimTarget()
    {
        if (aimTargetUnit != null)
        {
            if (aimTargetUnit.IsDead())
            {
                SetAimTargetUnit(null);
            }
            else
            {
                return;
            }
        }
        
        SetAimTargetUnit(GetNewTargetUnit());
    }

    UnitController GetNewTargetUnit()
    {
        //aimTargetUnit = UnitManager.Instance.GetNearestEnemyUnit(unit, 10f);
        // 3. 자동 타겟팅 범위 내 적 타겟 (시야 무관)
        UnitController autoTarget = UnitManager.Instance.GetNearestEnemyUnit(unit, autoTargetRange);
        if (autoTarget != null)
        {
            return autoTarget; // 자동 타겟을 찾았으면 바로 타겟 설정
        }
        
        if (targetEnemyType == EnemyType.None)
        {
            return UnitManager.Instance
                .GetVisibleEnemies(this, viewDistance, viewAngle, eyeHeight, excludeTarget)
                .OrderBy(e => (e.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        UnitController enemyController = UnitManager.Instance
            .GetAliveEnemieTypes(unit, targetEnemyType)
            .OrderBy(e => (e.transform.position - transform.position).sqrMagnitude)
            .FirstOrDefault();
        if (enemyController == null)
        {
            targetEnemyType = EnemyType.None;
        }

        return enemyController;
    }

    // 애니메이터의 IK 갱신
    private void OnAnimatorIK(int layerIndex)
    {
        if (gun == null || gun.state == Gun.State.Reloading) return;

        // IK를 사용하여 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춘다
        unitAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        unitAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        unitAnimator.SetIKPosition(AvatarIKGoal.LeftHand,
            gun.leftHandMount.position);
        unitAnimator.SetIKRotation(AvatarIKGoal.LeftHand,
            gun.leftHandMount.rotation);
    }

    public AimState GetAimState()
    {
        return aimState;
    }

    public UnitController GetAimTargetUnit()
    {
        return aimTargetUnit;
    }

    public void SetAimTargetUnit(UnitController targetController)
    {
        aimTargetUnit = targetController;
    }

    public void SetTargetEnemyType(EnemyType newEnemyType)
    {
        if (targetEnemyType == newEnemyType)
        {
            return;
        }

        targetEnemyType = newEnemyType;
        SetAimTargetUnit(null);
    }
}