using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    protected CharacterController characterController;
    protected CharacterAnimator characterAnimator;
    //should be normalized
    protected Vector3 moveDirection;
    protected float currentSpeed;
    protected float targetSpeed;
    protected float verticalVelocity;

    [Header("Movement Settings")]
    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;
    public float gravity = -15f;
    public float jumpForce = 7.0f;

    [Header("Air Control")]
    public float airControlFactor = 0.7f;
    public float airControlLerp = 2f;

    protected virtual void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        currentSpeed = walkSpeed;
    }

    protected void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    protected void MoveCharacter()
    {
        Vector3 finalMove = moveDirection * currentSpeed + Vector3.up * verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }

    protected void Jump()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = jumpForce;
            characterAnimator?.SetTrigger("Jump");
        }
    }
}
