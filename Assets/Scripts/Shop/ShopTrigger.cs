using Cysharp.Threading.Tasks;
using UnityEngine;

// 상점 상호작용 트리거
// 플레이어가 범위 내에 있을 때 상점 UI를 열 수 있게 함
[RequireComponent(typeof(Collider))]
public class ShopTrigger : MonoBehaviour
{
    [Header("상호작용 설정")]
    [Tooltip("상호작용 키 (Z키)")]
    [SerializeField] private KeyCode interactionKey = KeyCode.Z;

    [Header("UI 힌트 설정")]
    [Tooltip("상호작용 힌트 텍스트 (선택적)")]
    [SerializeField] private GameObject interactionHintUI;

    private bool playerInRange = false;
    private PlayerInput playerInput;
    private Collider triggerCollider;

    private void Awake()
    {
        // Collider를 트리거로 설정
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        // 힌트 UI 비활성화
        if (interactionHintUI != null)
        {
            interactionHintUI.SetActive(false);
        }
    }

    private void Update()
    {
        // 플레이어가 범위 내에 있고, 상점 UI가 열려있지 않을 때
        if (playerInRange && playerInput != null)
        {
            // PlayerInput에서 interact 키를 확인
            // (PlayerInput 스크립트에 interact 프로퍼티 추가 필요)
            if (Input.GetKeyDown(interactionKey))
            {
                OpenShop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 범위에 진입
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInput = other.GetComponent<PlayerInput>();

            // 힌트 UI 표시
            if (interactionHintUI != null)
            {
                interactionHintUI.SetActive(true);
            }

            Debug.Log("[ShopTrigger] 플레이어가 상점 범위에 진입했습니다. Z키를 눌러 상점을 여세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 범위에서 벗어남
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInput = null;

            // 힌트 UI 숨김
            if (interactionHintUI != null)
            {
                interactionHintUI.SetActive(false);
            }

            // 상점이 열려있으면 닫기
            if (UIManager.Instance != null)
            {
                var shopUI = UIManager.Instance.Get<ShopUI>(UIManager.EUIData.Shop);
                if (shopUI != null && shopUI.gameObject.activeSelf)
                {
                    CloseShop();
                }
            }

            Debug.Log("[ShopTrigger] 플레이어가 상점 범위를 벗어났습니다.");
        }
    }
    private async void OpenShop()
    {
        if (UIManager.Instance != null)
        {
            await UIManager.Instance.Show<ShopUI>(UIManager.EUIData.Shop);
            Debug.Log("[ShopTrigger] 상점 UI를 열었습니다.");
        }
        else
        {
            Debug.LogError("[ShopTrigger] UIManager를 찾을 수 없습니다!");
        }
    }

    private void CloseShop()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Hide(UIManager.EUIData.Shop);
            Debug.Log("[ShopTrigger] 상점 UI를 닫았습니다.");
        }
    }

    private void OnDrawGizmos()
    {
        // 에디터에서 트리거 범위 시각화
        Gizmos.color = Color.yellow;
        var col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }
    }
}
