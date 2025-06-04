using UnityEngine;

// 주어진 Gun 오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총에 위치하도록 조정
public class PlayerShooter : MonoBehaviour
{
    public enum AimState
    {
        Idle,
        HipFire
    }

    public AimState aimState { get; private set; }

    public Gun gun; // 사용할 총
    public LayerMask excludeTarget;
    
    private PlayerInput playerInput;
    private Animator playerAnimator; // 애니메이터 컴포넌트
    private Camera playerCamera;
    
    private float waitingTimeForReleasingAim = 2.5f;
    private float lastFireInputTime; 
    
    private Vector3 aimPoint;
    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y,gun.fireTransform.position, ~excludeTarget);
    
    void Awake()
    {
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
        {
            excludeTarget |= 1 << gameObject.layer;
        }
    }

    private void Start()
    {
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
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

    private void FixedUpdate()
    {
        if (playerInput.fire)
        {
            Debug.Log("[FixedUpdate] playerInput.fire == true, Shoot() 호출 시도");
            lastFireInputTime = Time.time;
            Shoot();
        }
        else if (playerInput.reload)
        {
            Debug.Log("[FixedUpdate] playerInput.reload == true, Reload() 호출");
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();

        // 1) 카메라 계산 부분을 무시하고 상체 ‘들기’에 대응되는 최소값(예: 0.8f)만 넘겨주기
        float fixedAngle = 0.5f; // 1에 가까울수록 더 완전하게 상체를 든 상태
        float camY = playerCamera.transform.position.y;
        float animAngle = Mathf.InverseLerp(0f, 20f, camY);
        // camY=6 → animAngle=0  /  camY=8 → animAngle=0.5  /  camY=10 → animAngle=1
        playerAnimator.SetFloat("Angle", animAngle);
        //playerAnimator.SetFloat("Angle", fixedAngle);

        if (!playerInput.fire && Time.time >= lastFireInputTime + waitingTimeForReleasingAim)
        {
            aimState = AimState.Idle;
        }
        
        UpdateUI();
    }

    //public void Shoot()
    //{
    //    Debug.Log($"[Shoot] 진입. aimState={aimState}, linedUp={linedUp}, hasEnoughDistance={hasEnoughDistance}");

    //    if (aimState == AimState.Idle)
    //    {
    //        Debug.Log("[Shoot] 현재 상태: Idle");
    //        if (linedUp)
    //        {
    //            Debug.Log("[Shoot] linedUp == true → aimState를 HipFire로 변경");
    //            aimState = AimState.HipFire;
    //        }
    //        else
    //        {
    //            Debug.Log("[Shoot] linedUp == false → 아무 동작 안 함");
    //        }
    //    }
    //    else if (aimState == AimState.HipFire)
    //    {
    //        Debug.Log("[Shoot] 현재 상태: HipFire");
    //        if (hasEnoughDistance)
    //        {
    //            Debug.Log("[Shoot] hasEnoughDistance == true → gun.Fire() 호출 시도");
    //            if (gun.Fire(aimPoint))
    //            {
    //                Debug.Log("[Shoot] gun.Fire() 성공 → 애니메이터 트리거 발동");
    //                playerAnimator.SetTrigger("Shoot");
    //            }
    //            else
    //            {
    //                Debug.Log("[Shoot] gun.Fire() 실패 (남은 탄약 없음 등)");
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("[Shoot] hasEnoughDistance == false → aimState를 Idle로 변경");
    //            aimState = AimState.Idle;
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log($"[Shoot] 정의되지 않은 상태: {aimState}");
    //    }
    //}

    public void Shoot()
    {
        Debug.Log($"[Shoot] 단발 모드 진입 → hasEnoughDistance={hasEnoughDistance}");

        // linedUp 체크 제거하고, 사거리(장애물)만 확인
        if (hasEnoughDistance)
        {
            Debug.Log("[Shoot] hasEnoughDistance == true → gun.Fire 호출");
            bool fired = gun.Fire(aimPoint);
            Debug.Log($"[Shoot] gun.Fire 리턴값 = {fired}");
            if (fired)
            {
                Debug.Log("[Shoot] 발사 성공 → 애니메이터 트리거");
                playerAnimator.SetTrigger("Shoot");
            }
        }
        else
        {
            Debug.Log("[Shoot] 사정거리/장애물 조건 불만족");
        }
    }



    public void Reload()
    {
        // 재장전 입력 감지시 재장전
        if(gun.Reload()) playerAnimator.SetTrigger("Reload");
    }

    private void UpdateAimTarget()
    {
        RaycastHit hit;
        
        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));

        if (Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget))
        {
            aimPoint = hit.point;

            if (Physics.Linecast(gun.fireTransform.position, hit.point, out hit, ~excludeTarget))
            {
                aimPoint = hit.point;
            }
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
        }
    }

    // 탄약 UI 갱신
    private void UpdateUI()
    {
        if (gun == null || UIManager.Instance == null) return;
        
        // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
    
    }

    // 애니메이터의 IK 갱신
    private void OnAnimatorIK(int layerIndex)
    {
        if (gun == null || gun.state == Gun.State.Reloading) return;

        // IK를 사용하여 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춘다
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand,
            gun.leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand,
            gun.leftHandMount.rotation);
    }
}