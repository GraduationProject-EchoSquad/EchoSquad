using System.Collections.Generic;
using UnityEngine;

// 상점 시스템 관리 매니저
// 아이템 목록 관리 및 구매 처리
public class ShopManager : Singleton<ShopManager>
{
    [Header("상점 아이템 목록")]
    [Tooltip("판매할 아이템 리스트 (ScriptableObject)")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("구매 제한 설정")]
    [Tooltip("각 아이템별 구매 횟수 추적")]
    private Dictionary<string, int> itemPurchaseCount = new Dictionary<string, int>();

    private PlayerController currentPlayer;
    private List<UnitController> aiAllies = new List<UnitController>();

    private void Start()
    {
        // 플레이어 참조 가져오기
        currentPlayer = FindObjectOfType<PlayerController>();

        if (currentPlayer == null)
        {
            Debug.LogWarning("[ShopManager] PlayerController를 찾을 수 없습니다!");
        }

        // AI 동료 참조 가져오기
        RefreshAIAllies();

        // 구매 횟수 초기화
        InitializePurchaseCount();
    }

    // AI 동료 목록 새로고침
    private void RefreshAIAllies()
    {
        aiAllies.Clear();

        if (UnitManager.Instance != null)
        {
            // Allay 팀의 유닛들 가져오기 (플레이어 제외)
            var allyUnits = UnitManager.Instance.GetUnitTeamTypeList(UnitController.EUnitTeamType.Allay);

            foreach (var unit in allyUnits)
            {
                // PlayerController가 아닌 유닛만 AI 동료로 간주
                if (unit != null && !(unit is PlayerController))
                {
                    aiAllies.Add(unit);
                }
            }

            Debug.Log($"[ShopManager] AI 동료 {aiAllies.Count}명 발견");
        }
    }
    // 구매 횟수 초기화
    private void InitializePurchaseCount()
    {
        itemPurchaseCount.Clear();

        foreach (var item in shopItems)
        {
            if (item != null)
            {
                itemPurchaseCount[item.itemId] = 0;
            }
        }
    }
    // 상점 아이템 목록 가져오기
    public List<ShopItem> GetShopItems()
    {
        return shopItems;
    }
    // 아이템 구매 시도
    // returns:구매 성공 여부
    public bool PurchaseItem(ShopItem item)
    {
        if (item == null)
        {
            PublishPurchaseFailed("Not valid Item");
            return false;
        }

        // 1. 구매 가능 여부 확인 (반복 구매 제한)
        if (!CanPurchaseItem(item))
        {
            PublishPurchaseFailed($"'{item.itemName}' can't no more buy");
            return false;
        }

        // 2. 돈 확인
        if (!CurrencyManager.Instance.HasEnoughMoney(item.price))
        {
            PublishPurchaseFailed($"Not Enough Money ({item.price})");
            return false;
        }

        // 3. 대상 유닛 확인 및 찾기
        UnitController targetUnit = null;

        if (item.target == ShopItemTarget.Player)
        {
            // 플레이어에게 적용
            if (currentPlayer == null)
            {
                currentPlayer = FindObjectOfType<PlayerController>();

                if (currentPlayer == null)
                {
                    Debug.LogError("[ShopManager] PlayerController를 찾을 수 없습니다!");
                    return false;
                }
            }

            targetUnit = currentPlayer;
        }
        else // AI 동료에게 적용
        {
            // AI 동료 목록 새로고침
            RefreshAIAllies();

            if (aiAllies.Count == 0)
            {
                Debug.LogWarning("[ShopManager] AI 동료를 찾을 수 없습니다!");
                return false;
            }

            // 모든 AI 동료에게 효과 적용 (첫 번째 동료만 적용하려면 aiAllies[0] 사용)
            // 여기서는 첫 번째 AI 동료에게만 적용
            targetUnit = aiAllies[0];
        }

        // 4. 돈 차감
        if (!CurrencyManager.Instance.SpendMoney(item.price))
        {
            PublishPurchaseFailed($"Not Enough Money ({item.price})");
            return false;
        }

        // 5. 아이템 효과 적용
        if (item.target == ShopItemTarget.AI)
        {
            // 모든 AI 동료에게 효과 적용
            foreach (var ally in aiAllies)
            {
                if (ally != null)
                {
                    item.ApplyEffect(ally);
                }
            }
        }
        else
        {
            // 플레이어에게만 효과 적용
            item.ApplyEffect(targetUnit);
        }

        // 6. 구매 횟수 증가
        if (itemPurchaseCount.ContainsKey(item.itemId))
        {
            itemPurchaseCount[item.itemId]++;
        }
        else
        {
            itemPurchaseCount[item.itemId] = 1;
        }

        // 7. 구매 성공 이벤트 발행
        PublishItemPurchased(item);

        Debug.Log($"[ShopManager] '{item.itemName}' 구매 성공! 남은 돈: {CurrencyManager.Instance.Money}");
        return true;
    }
    // 아이템을 구매할 수 있는지 확인 (반복 구매 제한 체크)
    private bool CanPurchaseItem(ShopItem item)
    {
        // 반복 구매 가능한 아이템은 항상 구매 가능
        if (item.isRepeatable)
        {
            // 최대 스택 제한이 있는 경우
            if (item.maxStackCount > 0)
            {
                int currentCount = itemPurchaseCount.ContainsKey(item.itemId) ? itemPurchaseCount[item.itemId] : 0;
                return currentCount < item.maxStackCount;
            }

            // 무제한 구매 가능
            return true;
        }

        // 반복 구매 불가능한 아이템은 한 번만 구매 가능
        int purchaseCount = itemPurchaseCount.ContainsKey(item.itemId) ? itemPurchaseCount[item.itemId] : 0;
        return purchaseCount == 0;
    }
    // 아이템 구매 가능 횟수 가져오기
    public int GetRemainingPurchaseCount(ShopItem item)
    {
        if (item.isRepeatable)
        {
            if (item.maxStackCount > 0)
            {
                int currentCount = itemPurchaseCount.ContainsKey(item.itemId) ? itemPurchaseCount[item.itemId] : 0;
                return item.maxStackCount - currentCount;
            }

            return int.MaxValue; // 무제한
        }

        int purchaseCount = itemPurchaseCount.ContainsKey(item.itemId) ? itemPurchaseCount[item.itemId] : 0;
        return purchaseCount == 0 ? 1 : 0;
    }
    // 아이템 구매 성공 이벤트 발행
    private void PublishItemPurchased(ShopItem item)
    {
        PubSubManager.Instance.Publish<OnItemPurchasedData>(PubSubEvent.OnItemPurchased, data =>
        {
            data.ItemId = item.itemId;
            data.ItemName = item.itemName;
        });
    }
    // 구매 실패 이벤트 발행
    private void PublishPurchaseFailed(string reason)
    {
        PubSubManager.Instance.Publish<OnPurchaseFailedData>(PubSubEvent.OnPurchaseFailed, data =>
        {
            data.Reason = reason;
        });
    }
    // 상점 초기화 (게임 재시작 시)
    public void ResetShop()
    {
        InitializePurchaseCount();
        Debug.Log("[ShopManager] 상점이 초기화되었습니다.");
    }
}
