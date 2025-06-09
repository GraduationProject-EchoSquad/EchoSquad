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

    private UnitController unit;
    private UnitController aimTargetUnit;

    [Header("Visibility Setting")]
    [SerializeField] float viewDistance = 10f;      // 최대 탐지 거리
    [SerializeField] float viewAngle = 120f;      // 시야각(°)
    [SerializeField] float eyeHeight;  //  Raycast 시작 높이

    protected bool hasEnoughDistance =>
        !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y, gun.fireTransform.position,
            ~excludeTarget);

    protected virtual void Start()
    {
        unitAnimator = GetComponent<Animator>();
        unit = GetComponent<UnitController>();

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
        //Debug.Log($"[Shoot] 단발 모드 진입 → hasEnoughDistance={hasEnoughDistance}");

        // linedUp 체크 제거하고, 사거리(장애물)만 확인
        if (aimTargetUnit != null && hasEnoughDistance)
        {
            //Debug.Log("[Shoot] hasEnoughDistance == true → gun.Fire 호출");
            bool fired = gun.Fire(aimTargetUnit.transform.position);
            //Debug.Log($"[Shoot] gun.Fire 리턴값 = {fired}");
            if (fired)
            {
                //Debug.Log("[Shoot] 발사 성공 → 애니메이터 트리거");
                unitAnimator.SetTrigger("Shoot");
                if (gun.magAmmo <= 0)
                {
                    Reload();
                }
            }
        }
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
                aimTargetUnit = null;
            }
            else
            {
                return;
            }
        }

        //aimTargetUnit = UnitManager.Instance.GetNearestEnemyUnit(unit, 10f);
        aimTargetUnit = UnitManager.Instance
        .GetVisibleEnemies(unit, viewDistance, viewAngle, eyeHeight, excludeTarget)
        .OrderBy(e => (e.transform.position - transform.position).sqrMagnitude)
        .FirstOrDefault();
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
}