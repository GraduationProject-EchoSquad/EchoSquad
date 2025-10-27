using UnityEngine;

// 이 스크립트는 Animator가 붙어있는 '자식' 오브젝트에 붙입니다.
public class EnemyAnimationEventProxy : MonoBehaviour
{
    private EnemyController parentController;

    void Awake()
    {
        // 내 부모 중에 있는 EnemyController를 찾아서 저장
        parentController = GetComponentInParent<EnemyController>();

        if (parentController == null)
        {
            Debug.LogError("부모에게서 EnemyController를 찾을 수 없습니다!", gameObject);
        }
    }

    // 1. 애니메이션 이벤트가 이 함수를 호출
    public void ApplyDamageToTarget()
    {
        // 2. 이 함수는 부모의 진짜 함수를 호출
        if (parentController != null)
        {
            parentController.ApplyDamageToTarget();
        }
    }

    // 1. 애니메이션 이벤트가 이 함수를 호출
    public void OnAttackAnimationEnd()
    {
        // 2. 이 함수는 부모의 진짜 함수를 호출
        if (parentController != null)
        {
            parentController.OnAttackAnimationEnd();
        }
    }
}