using UnityEngine;

// 주어진 Gun 오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총에 위치하도록 조정
public class PlayerShooter : UnitShooter
{
    
    private PlayerInput playerInput;
    private Camera playerCamera;
    private Vector3 aimPoint;
    
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

    protected override void InitializeEyeHeight()
    {
    }

    private void FixedUpdate()
    {
        if (playerInput.fire)
        {
            //Debug.Log("[FixedUpdate] playerInput.fire == true, Shoot() 호출 시도");
            lastFireInputTime = Time.time;
            Shoot();
        }
        else if (playerInput.reload)
        {
            //Debug.Log("[FixedUpdate] playerInput.reload == true, Reload() 호출");
            Reload();
        }
        
        if (playerInput.subFire)
        {
            gun.DrawPreviewLine();
        }
        else
        {
            gun.UnDrawPreviewLine();
        }
    }

    private void Update()
    {
        // 카메라 높이 기반 발사각 변경 기능 비활성화 - 정면으로 고정
        // float camY = playerCamera.transform.position.y;
        // float animAngle = Mathf.InverseLerp(0f, 20f, camY);

        // 정면(수평) 발사각으로 고정
        float animAngle = 0.5f;
        unitAnimator.SetFloat("Angle", animAngle);

        if (!playerInput.fire && Time.time >= lastFireInputTime + waitingTimeForReleasingAim)
        {
            aimState = AimState.Idle;
        }
    }

    public override void Shoot()
    {
        //Debug.Log($"[Shoot] 단발 모드 진입 → hasEnoughDistance={hasEnoughDistance}");

        // linedUp 체크 제거하고, 사거리(장애물)만 확인
        if (hasEnoughDistance)
        {
            //Debug.Log("[Shoot] hasEnoughDistance == true → gun.Fire 호출");
            bool fired = gun.Fire(aimPoint);
            //Debug.Log($"[Shoot] gun.Fire 리턴값 = {fired}");
            if (fired)
            {
                //Debug.Log("[Shoot] 발사 성공 → 애니메이터 트리거");
                unitAnimator.SetTrigger("Shoot");
            }
        }
        else
        {
            //Debug.Log("[Shoot] 사정거리/장애물 조건 불만족");
        }
    }

    // 탄약 UI 갱신
    private void UpdateUI()
    {
        if (gun == null || UIManager.Instance == null) return;
        
        
    
    }
}