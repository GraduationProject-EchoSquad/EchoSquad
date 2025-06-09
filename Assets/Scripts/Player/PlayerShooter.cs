using UnityEngine;

// 주어진 Gun 오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총에 위치하도록 조정
public class PlayerShooter : UnitShooter
{
    
    private PlayerInput playerInput;
    private Camera playerCamera;
    
    void Awake()
    {
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
        {
            excludeTarget |= 1 << gameObject.layer;
        }
    }

    protected override void Start()
    {
        base.Start();
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
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
        unitAnimator.SetFloat("Angle", animAngle);
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

    public override void Shoot()
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
                unitAnimator.SetTrigger("Shoot");
            }
        }
        else
        {
            Debug.Log("[Shoot] 사정거리/장애물 조건 불만족");
        }
    }

    protected override void UpdateAimTarget()
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
}