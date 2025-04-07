using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PlayerBase
{

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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 달리기 처리
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = Mathf.Lerp(currentSpeed, runSpeed, Time.deltaTime * 5f);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, Time.deltaTime * 5f);
        }

        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
            moveDirection = inputDirection * currentSpeed;

            characterAnimator?.SetFloat("Speed", inputDirection.magnitude * currentSpeed);
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
                Vector3 targetDir = inputDirection * currentSpeed * airControlFactor;
                moveDirection = Vector3.Lerp(moveDirection, targetDir, Time.deltaTime * airControlLerp);
            }
            characterAnimator?.SetBool("IsFalling", true);
        }

        // Update MoveX and MoveY based on local input direction
        Vector3 localInputDirection = transform.InverseTransformDirection(inputDirection);
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

