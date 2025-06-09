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

    // 탄약 UI 갱신
    private void UpdateUI()
    {
        if (gun == null || UIManager.Instance == null) return;
        
        // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
    
    }
}