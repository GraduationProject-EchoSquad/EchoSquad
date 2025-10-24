using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 UI
// 아이템 목록 표시 및 구매 처리
public class ShopUI : UIBase
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    [Header("탭 버튼")]
    [SerializeField] private Button playerTabButton;
    [SerializeField] private Button aiTabButton;

    [Header("아이템 컨테이너")]
    [SerializeField] private GameObject playerItemsContainer;
    [SerializeField] private GameObject aiItemsContainer;

    [Header("플레이어 아이템 버튼")]
    [SerializeField] private Button playerFireRateButton;
    [SerializeField] private Button playerAmmoButton;
    [SerializeField] private Button playerHealthButton;
    [SerializeField] private Button playerMaxHealthButton;

    [Header("AI 아이템 버튼")]
    [SerializeField] private Button aiFireRateButton;
    [SerializeField] private Button aiAmmoButton;
    [SerializeField] private Button aiHealthButton;
    [SerializeField] private Button aiMaxHealthButton;

    private ShopItemTarget currentTab = ShopItemTarget.Player;
    private Dictionary<Button, ShopItem> buttonItemMap = new Dictionary<Button, ShopItem>();

    private void Start()
    {
        // 닫기 버튼 이벤트
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }

        // 탭 버튼 이벤트
        if (playerTabButton != null)
        {
            playerTabButton.onClick.AddListener(() => SwitchTab(ShopItemTarget.Player));
        }
        if (aiTabButton != null)
        {
            aiTabButton.onClick.AddListener(() => SwitchTab(ShopItemTarget.AI));
        }

        // 버튼과 아이템 연결
        SetupItemButtons();

        // 돈 변경 이벤트 구독
        PubSubManager.Instance.Subscribe<OnMoneyChangedData>(PubSubEvent.OnMoneyChanged, OnMoneyChanged);

        // 구매 성공/실패 이벤트 구독
        PubSubManager.Instance.Subscribe<OnItemPurchasedData>(PubSubEvent.OnItemPurchased, OnItemPurchased);
        PubSubManager.Instance.Subscribe<OnPurchaseFailedData>(PubSubEvent.OnPurchaseFailed, OnPurchaseFailed);

        // 초기 돈 표시
        UpdateMoneyDisplay(CurrencyManager.Instance.Money);

        // 메시지 초기화
        if (messageText != null)
        {
            messageText.text = "";
        }

        // 플레이어 탭으로 시작
        SwitchTab(ShopItemTarget.Player);
    }

    // 버튼과 ShopItem 연결
    private void SetupItemButtons()
    {
        if (ShopManager.Instance == null) return;

        List<ShopItem> shopItems = ShopManager.Instance.GetShopItems();

        // 플레이어 아이템 버튼 연결
        var playerItems = shopItems.FindAll(item => item.target == ShopItemTarget.Player);
        if (playerItems.Count >= 4)
        {
            SetupButton(playerFireRateButton, playerItems.Find(i => i.itemType == ShopItemType.FireRate));
            SetupButton(playerAmmoButton, playerItems.Find(i => i.itemType == ShopItemType.AmmoRefill));
            SetupButton(playerHealthButton, playerItems.Find(i => i.itemType == ShopItemType.HealthRestore));
            SetupButton(playerMaxHealthButton, playerItems.Find(i => i.itemType == ShopItemType.MaxHealthIncrease));
        }

        // AI 아이템 버튼 연결
        var aiItems = shopItems.FindAll(item => item.target == ShopItemTarget.AI);
        if (aiItems.Count >= 4)
        {
            SetupButton(aiFireRateButton, aiItems.Find(i => i.itemType == ShopItemType.FireRate));
            SetupButton(aiAmmoButton, aiItems.Find(i => i.itemType == ShopItemType.AmmoRefill));
            SetupButton(aiHealthButton, aiItems.Find(i => i.itemType == ShopItemType.HealthRestore));
            SetupButton(aiMaxHealthButton, aiItems.Find(i => i.itemType == ShopItemType.MaxHealthIncrease));
        }
    }

    // 개별 버튼 설정
    private void SetupButton(Button button, ShopItem item)
    {
        if (button == null || item == null) return;

        buttonItemMap[button] = item;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnItemPurchaseClicked(item));
    }

    private void OnEnable()
    {
        // 현재 돈 업데이트
        if (CurrencyManager.Instance != null)
        {
            UpdateMoneyDisplay(CurrencyManager.Instance.Money);
        }

        // 현재 탭 유지
        SwitchTab(currentTab);

        // 버튼 상태 업데이트
        UpdateAllButtonStates();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (PubSubManager.Instance != null)
        {
            PubSubManager.Instance.Unsubscribe<OnMoneyChangedData>(PubSubEvent.OnMoneyChanged, OnMoneyChanged);
            PubSubManager.Instance.Unsubscribe<OnItemPurchasedData>(PubSubEvent.OnItemPurchased, OnItemPurchased);
            PubSubManager.Instance.Unsubscribe<OnPurchaseFailedData>(PubSubEvent.OnPurchaseFailed, OnPurchaseFailed);
        }

        // 버튼 이벤트 해제
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseShop);
        }
        if (playerTabButton != null)
        {
            playerTabButton.onClick.RemoveAllListeners();
        }
        if (aiTabButton != null)
        {
            aiTabButton.onClick.RemoveAllListeners();
        }

        // 아이템 버튼 이벤트 해제
        foreach (var button in buttonItemMap.Keys)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }

    // 탭 전환
    private void SwitchTab(ShopItemTarget tab)
    {
        currentTab = tab;

        // 컨테이너 전환
        if (playerItemsContainer != null)
        {
            playerItemsContainer.SetActive(tab == ShopItemTarget.Player);
        }
        if (aiItemsContainer != null)
        {
            aiItemsContainer.SetActive(tab == ShopItemTarget.AI);
        }

        // 탭 버튼 상태 업데이트 (Pressed 상태)
        if (playerTabButton != null)
        {
            playerTabButton.interactable = (tab != ShopItemTarget.Player);
        }
        if (aiTabButton != null)
        {
            aiTabButton.interactable = (tab != ShopItemTarget.AI);
        }

        // 버튼 상태 업데이트
        UpdateAllButtonStates();

        Debug.Log($"[ShopUI] 탭 전환: {(tab == ShopItemTarget.Player ? "플레이어" : "AI 동료")}");
    }

    // 모든 버튼 상태 업데이트
    private void UpdateAllButtonStates()
    {
        foreach (var kvp in buttonItemMap)
        {
            UpdateButtonState(kvp.Key, kvp.Value);
        }
    }

    // 개별 버튼 상태 업데이트
    private void UpdateButtonState(Button button, ShopItem item)
    {
        if (button == null || item == null) return;
        if (ShopManager.Instance == null || CurrencyManager.Instance == null) return;

        bool canPurchase = ShopManager.Instance.GetRemainingPurchaseCount(item) > 0
                           && CurrencyManager.Instance.HasEnoughMoney(item.price);

        button.interactable = canPurchase;
    }

    // 아이템 구매 버튼 클릭 시
    private void OnItemPurchaseClicked(ShopItem item)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.PurchaseItem(item);
        }
    }

    // 돈 변경 이벤트 처리
    private void OnMoneyChanged(OnMoneyChangedData data)
    {
        UpdateMoneyDisplay(data.Money);
        UpdateAllButtonStates(); // 돈이 바뀌면 버튼 상태도 업데이트
    }

    // 돈 표시 업데이트
    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${money}";
        }
    }

    // 아이템 구매 성공
    private void OnItemPurchased(OnItemPurchasedData data)
    {
        ShowMessage($"'{data.ItemName}' Purchase Success!", Color.green);

        // 버튼 상태 업데이트
        UpdateAllButtonStates();
    }

    // 구매 실패
    private void OnPurchaseFailed(OnPurchaseFailedData data)
    {
        ShowMessage(data.Reason, Color.red);
    }

    // 메시지 표시
    private void ShowMessage(string message, Color color)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;

            // 3초 후 메시지 지우기
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 3f);
        }
    }

    // 메시지 지우기
    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    // 상점 닫기
    private void CloseShop()
    {
        gameObject.SetActive(false);
        Debug.Log("[ShopUI] 상점을 닫았습니다.");
    }
}
