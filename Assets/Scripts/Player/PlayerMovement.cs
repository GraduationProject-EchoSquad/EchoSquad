using UnityEngine;

public class PlayerMovement : UnitMovement
{
    private CharacterController characterController;
    private PlayerInput playerInput;
    private PlayerShooter playerShooter;

    private Camera followCam;

    protected override float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;

    protected override void Start()
    {
        base.Start();
        playerInput = GetComponent<PlayerInput>();
        followCam = Camera.main;
        characterController = GetComponent<CharacterController>();
        playerShooter = unitShooter as PlayerShooter;
    }

    protected override void FixedUpdate()
    {
        RotateTowardsCursor();
        Move(playerInput.moveInput);
    }

    private void RotateTowardsCursor()
    {
        // 플레이어 높이의 가상 평면
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        Ray ray = followCam.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                Rotate(targetAngle);
            }
        }
    }

    protected override void Update()
    {
        UpdateAnimation(playerInput.moveInput);
        
        if (playerInput.jump) Jump();
    }

    public void Move(Vector2 moveInput)
    {
        var targetSpeed = speed * moveInput.magnitude;

        // 카메라 기준으로 이동
        Vector3 camForward = followCam.transform.forward;
        Vector3 camRight = followCam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        var moveDirection = Vector3.Normalize(camForward * moveInput.y + camRight * moveInput.x);

        var smoothTime = characterController.isGrounded ? speedSmoothTime : speedSmoothTime / airControlPercent;

        targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, smoothTime);
        currentVelocityY += Time.deltaTime * Physics.gravity.y;

        var velocity = moveDirection * targetSpeed + Vector3.up * currentVelocityY;

        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded) currentVelocityY = 0;
    }

    public void Jump()
    {
        if (!characterController.isGrounded) return;
        currentVelocityY = jumpVelocity;
    }

    protected override void UpdateAnimation(Vector2 moveInput)
    {
        var animationSpeedPercent = currentSpeed / speed;

        animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
    }
}