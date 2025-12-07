using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineCamera Cinemachine;

    [Header("Simple Follow Camera")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -5);
    public float smoothSpeed = 5f;

    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    [Header("Camera Rotation")]
    public float fixedXRotation = 45f;
    public float delayBeforeRotation = 0.1f;  // 회전 시작까지 대기 시간
    public float rotationSpeed = 2f;        // 회전 속도
    public float sameDirectionThreshold = 60f;  // 같은 방향으로 판단하는 각도 범위
    public float behindAngleThreshold = 30f;    // 이 각도 이상 차이나면 회전 대상
    public float rotationCooldown = 0.5f;       // 회전 후 쿨다운

    private float currentYRotation = 0f;
    private float targetYRotation = 0f;
    private float lastTargetYRotation = 0f;
    private float stationaryTimer = 0f;
    private float cooldownTimer = 0f;

    private void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        // 카메라 위치 따라가기
        Vector3 desiredPosition = target.position + RotateOffset(offset, currentYRotation);
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // 쿨다운 처리
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 캐릭터 Y 회전 추적
        float characterYRotation = target.eulerAngles.y;
        float angleDiffFromCamera = Mathf.Abs(Mathf.DeltaAngle(characterYRotation, currentYRotation));

        // 캐릭터가 카메라 뒤쪽을 보고 있는지 체크
        bool isBehind = angleDiffFromCamera >= behindAngleThreshold;

        // 쿨다운 아닐 때만 회전 체크
        if (cooldownTimer <= 0f)
        {
            // 뒤쪽을 보고 있고, 같은 방향 유지 중일 때만 회전
            if (isBehind && Mathf.Abs(Mathf.DeltaAngle(characterYRotation, lastTargetYRotation)) < sameDirectionThreshold)
            {
                stationaryTimer += Time.deltaTime;
                if (stationaryTimer >= delayBeforeRotation)
                {
                    targetYRotation = characterYRotation;
                    cooldownTimer = rotationCooldown;  // 쿨다운 시작
                    stationaryTimer = 0f;
                }
            }
            else
            {
                stationaryTimer = 0f;
                lastTargetYRotation = characterYRotation;
            }
        }

        // 카메라 Y 회전 부드럽게 따라가기
        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, rotationSpeed * Time.deltaTime);
        mainCamera.transform.rotation = Quaternion.Euler(fixedXRotation, currentYRotation, 0f);
    }

    private Vector3 RotateOffset(Vector3 offset, float yAngle)
    {
        return Quaternion.Euler(0, yAngle, 0) * offset;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetCinemachineTarget(GameObject gameObject)
    {
        if (GameManager.Instance.isTest == false) return;
        if (gameObject == null)
        {
            Cinemachine.Follow = null;
            Cinemachine.gameObject.SetActive(false);
            return;
        }
        Cinemachine.Follow = gameObject.transform;
        Cinemachine.gameObject.SetActive(true);
    }
}
