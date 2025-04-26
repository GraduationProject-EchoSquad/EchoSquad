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
        cameraFollow.target = gameObject;
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();
        MoveCharacter();
        RotateCharacterToMouse();
    }

    void HandleMovement()
    {
        //use GetAxisRaw, not GetAxis
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 달리기 처리
        if (inputDirection.sqrMagnitude > 0.1)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetSpeed = runSpeed;
            }
            else
            {
                targetSpeed = walkSpeed;
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

    void RotateCharacterToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            Vector3 targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Quaternion rotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10.0f);
        }
    }
}

