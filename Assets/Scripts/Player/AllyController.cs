using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyController : PlayerBase
{
    // AI ������ ������ ó���ϴ� ������ �߰��մϴ�.
    void Update()
    {
        // AI ������ �̵� �� �ൿ ����
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
        // ���� �߻� ���� �߰�
    }
}

