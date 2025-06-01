using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PlayerBase
{
    private Vector3 localInputDirection;
    
    protected override void Start()
    {
        base.Start();

        // set CameraFollow target
        Camera m_MainCamera = Camera.main;
        CameraFollow cameraFollow = m_MainCamera.GetComponent<CameraFollow>();
        cameraFollow.target = transform;
    }

    void Update()
    {
        RotateCharacterToMouse();
        HandleMovement();
        ApplyGravity();
        MoveCharacter();
    }

    void HandleMovement()
    {
        //use GetAxisRaw, not GetAxis
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDirection = (camRight * h + camForward * v).normalized;

        // 달리기 처리
        if (inputDirection.sqrMagnitude > 0.1)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetSpeed = runSpeed;
                characterAnimator?.SetBool("IsRunning", true);
            }
            else
            {
                targetSpeed = walkSpeed;
                characterAnimator?.SetBool("IsRunning", false);
            }
        }
        else
        {
            targetSpeed = 0f;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);

        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
            moveDirection = (inputDirection * currentSpeed).normalized;

            characterAnimator?.SetFloat("Speed", currentSpeed);
            characterAnimator?.SetBool("IsFalling", false);

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }
        else
        {
            if (inputDirection.magnitude > 0.1f)
            {
                Vector3 targetDir = (inputDirection * currentSpeed).normalized;
                moveDirection = Vector3.Lerp(moveDirection, targetDir, Time.deltaTime * airControlLerp);
            }
            characterAnimator?.SetBool("IsFalling", true);
        }

        // Update MoveX and MoveY based on local input direction
        //use lerp
        localInputDirection = Vector3.Lerp(localInputDirection, transform.InverseTransformDirection(inputDirection), Time.deltaTime * 5f);
        characterAnimator?.SetFloat("MoveX", localInputDirection.x);
        characterAnimator?.SetFloat("MoveY", localInputDirection.z);
    }

    //void RotateCharacterToMouse()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (Physics.Raycast(ray, out RaycastHit hit, 1000))
    //    {
    //        Vector3 targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
    //        Quaternion rotation = Quaternion.LookRotation(targetPosition - transform.position);
    //        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10.0f);
    //    }
    //}
    void RotateCharacterToMouse()
    {
        // 1) 화면 정가운데(스크린 센터) 좌표 구하기
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        // 2) 현재 마우스 좌표
        Vector2 mousePos = Input.mousePosition;
        // 3) 마우스 위치와 화면 센터 간의 오프셋
        Vector2 delta = mousePos - screenCenter;

        // 4) 너무 작은 움직임(즉, 마우스가 거의 화면 중앙에 있으면)일 땐 회전하지 않음
        if (delta.sqrMagnitude < 25f) // 5px 이내라면 무시 (원하는 값으로 조정)
            return;

        // 5) 스크린 상에서 (delta.x, delta.y) 벡터를 각도로 변환 (atan2 기준: x축이 왼쪽·오른쪽, y축이 위·아래)
        //    Mathf.Atan2(delta.x, delta.y)를 사용하면, 화면 기준 위쪽(=Center→마우스)이 0°, 오른쪽이 +90°, 아래가 180°/-180°, 왼쪽이 -90°가 됩니다.
        float angleFromCenter = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;

        // 6) 현재 카메라의 Y축 회전(=캐릭터가 바라보는 기준)을 구해서 합치기
        //    즉, '카메라가 정면(앞)이라고 생각하는 방향'에, 스크린 상 delta 각도를 더한 값이 캐릭터가 최종 바라볼 yaw가 됩니다.
        float camYaw = Camera.main.transform.eulerAngles.y;
        float targetYaw = camYaw + angleFromCenter;

        // 7) 최종 회전(쿼터니언)으로 변환
        Quaternion desiredRot = Quaternion.Euler(0f, targetYaw, 0f);

        // 8) 부드럽게 회전 (원래 쓰던 Lerp 속도 그대로 사용)
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRot, Time.deltaTime * 10f);
    }

}

