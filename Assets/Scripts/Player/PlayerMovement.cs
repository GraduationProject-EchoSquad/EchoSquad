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
        if (currentSpeed > 0.2f || playerInput.fire || playerShooter.GetAimState() == UnitShooter.AimState.HipFire)
            Rotate(followCam.transform.eulerAngles.y);

        Move(playerInput.moveInput);

        if (playerInput.jump) Jump();
    }

    protected override void Update()
    {
        UpdateAnimation(playerInput.moveInput);
    }

    public void Move(Vector2 moveInput)
    {
        var targetSpeed = speed * moveInput.magnitude;
        var moveDirection = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x);

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