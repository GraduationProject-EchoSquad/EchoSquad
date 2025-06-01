using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Follow Target (Player Transform)")]
    public Transform target;

    [Header("Zoom Settings")]
    [Tooltip("Speed at which the camera moves when zooming in/out with the scroll wheel")]
    public float zoomSpeed = 2f;
    [Tooltip("Minimum distance the camera can zoom in")]
    public float minDistance = 2f;
    [Tooltip("Maximum distance the camera can zoom out")]
    public float maxDistance = 12f;

    [Header("Mouse Rotation / Damping")]
    [Tooltip("Sensitivity when rotating the camera around the player with mouse X movement")]
    public float mouseSensitivity = 5f;
    [Range(0f, 1f), Tooltip("Smoothing factor for camera movement (closer to 0 = slower follow)")]
    public float damping = 0.1f;

    // 내부 변수
    private Vector3 viewDir;         // 타겟을 기준으로 카메라가 바라보는 단위 방향 벡터
    private float currentDistance;   // 실제 화면에 사용되는 거리(부드럽게 보간됨)
    private float targetDistance;    // 스크롤 입력이 들어올 때 즉시 바뀌는 목표 거리

    // “카메라가 바라보는 Y축 회전” 값을 각도로 저장해두는 변수
    private float yaw;               // (degrees, 0~360)
    private float currentPitch;      // 사실상 pitch(위/아래 각도)는 viewDir.y에서 계산됨

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: target Transform이 할당되지 않았습니다.");
            return;
        }

        // 카메라 위치와 타겟 위치 간 초기 오프셋 계산
        Vector3 offset = transform.position - target.position;
        viewDir = offset.normalized;

        // 오프셋 길이를 초기 거리로 설정
        float fullDist = offset.magnitude;
        currentDistance = fullDist;
        targetDistance = fullDist;

        // 마우스 커서 해제 및 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // 1) 마우스 X 입력으로 카메라 방향(Y축) 회전
        float mouseX = Input.GetAxis("Mouse X");
        if (Mathf.Abs(mouseX) > Mathf.Epsilon)
        {
            float angle = mouseX * mouseSensitivity;
            viewDir = Quaternion.AngleAxis(angle, Vector3.up) * viewDir;
        }

        // 2) 마우스 휠 입력으로 줌(거리) 변경
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float proposed = targetDistance - scroll * targetDistance * 0.1f;
            targetDistance = Mathf.Clamp(proposed, minDistance, maxDistance);
        }

        // 3) 부드럽게 거리 보간
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, damping);

        // 4) 위치 보간: 타겟 위치 + 방향 * 거리
        Vector3 desiredPosition = target.position + viewDir * currentDistance;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, damping);

        // 5) 항상 타겟을 바라봄
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}