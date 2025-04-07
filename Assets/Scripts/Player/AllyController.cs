using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyController : PlayerBase
{
    // AI 동료의 동작을 처리하는 로직을 추가합니다.
    void Update()
    {
        // AI 동료의 이동 및 행동 로직
        ApplyGravity();
        MoveCharacter();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        moveDirection = direction * currentSpeed;
    }

    public void FireAtTarget(Vector3 targetPosition)
    {
        // 무기 발사 로직 추가
    }
}

