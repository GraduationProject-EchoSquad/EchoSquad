using UnityEngine;

// 상점에서 판매하는 아이템 데이터 (ScriptableObject)
// Unity 에디터에서 Create > Shop > Shop Item으로 생성 가능
[CreateAssetMenu(fileName = "ShopItem", menuName = "Shop/Shop Item", order = 1)]
public class ShopItem : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("아이템 고유 ID")]
    public string itemId;

    [Tooltip("아이템 이름")]
    public string itemName;

    [Tooltip("아이템 설명")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("아이템 아이콘 (UI용)")]
    public Sprite icon;

    [Header("가격")]
    [Tooltip("아이템 가격")]
    public int price = 100;

    [Header("아이템 타입 & 효과")]
    [Tooltip("아이템 종류")]
    public ShopItemType itemType;

    [Tooltip("적용 대상 (플레이어 또는 AI 동료)")]
    public ShopItemTarget target;

    [Header("아이템별 설정값")]
    [Tooltip("FireRate: 감소시킬 발사 간격 (0.01 ~ 0.05 권장) / 총알: 채울 탄약 수 / HP 회복: 회복량 / HP 최대용량: 증가량")]
    public float effectValue;

    [Header("구매 설정")]
    [Tooltip("여러 번 구매 가능한지")]
    public bool isRepeatable = false;

    [Tooltip("반복 구매 시 최대 스택 수 (0 = 무제한)")]
    public int maxStackCount = 0;

    // 아이템 효과 적용
    public void ApplyEffect(UnitController unit)
    {
        if (unit == null)
        {
            Debug.LogError("[ShopItem] Unit이 null입니다!");
            return;
        }

        switch (itemType)
        {
            case ShopItemType.FireRate:
                ApplyFireRate(unit);
                break;

            case ShopItemType.AmmoRefill:
                ApplyAmmoRefill(unit);
                break;

            case ShopItemType.HealthRestore:
                ApplyHealthRestore(unit);
                break;

            case ShopItemType.MaxHealthIncrease:
                ApplyMaxHealthIncrease(unit);
                break;

            default:
                Debug.LogWarning($"[ShopItem] 알 수 없는 아이템 타입: {itemType}");
                break;
        }

        string targetName = target == ShopItemTarget.Player ? "플레이어" : "AI 동료";
        Debug.Log($"[ShopItem] '{itemName}' 아이템 효과 적용 완료! (대상: {targetName})");
    }

    // 발사 속도 증가 (timeBetFire 감소)
    private void ApplyFireRate(UnitController unit)
    {
        var shooter = unit.GetComponent<UnitShooter>();
        if (shooter != null && shooter.gun != null)
        {
            float decreaseAmount = effectValue;
            shooter.gun.timeBetFire = Mathf.Max(0.01f, shooter.gun.timeBetFire - decreaseAmount);
            Debug.Log($"[ShopItem] 발사 속도 증가! 새로운 발사 간격: {shooter.gun.timeBetFire:F3}초");
        }
    }

    // 총알 채우기
    private void ApplyAmmoRefill(UnitController unit)
    {
        var shooter = unit.GetComponent<UnitShooter>();
        if (shooter != null && shooter.gun != null)
        {
            int ammoToAdd = Mathf.RoundToInt(effectValue);
            shooter.gun.SetAmmoRemain(shooter.gun.ammoRemain + ammoToAdd);
            Debug.Log($"[ShopItem] 탄약 {ammoToAdd}발 보충 완료!");
        }
    }

    // HP 최대치까지 회복
    private void ApplyHealthRestore(UnitController unit)
    {
        var health = unit.GetComponent<LivingEntity>();
        if (health != null)
        {
            float healAmount = health.startingHealth - health.health;
            health.RestoreHealth(healAmount);
            Debug.Log($"[ShopItem] HP를 최대치까지 회복! ({healAmount} 회복)");
        }
    }

    // HP 최대 용량 늘리기
    private void ApplyMaxHealthIncrease(UnitController unit)
    {
        var health = unit.GetComponent<LivingEntity>();
        if (health != null)
        {
            health.IncreaseMaxHealth(effectValue);// 최대 체력 증가
            Debug.Log($"[ShopItem] 최대 HP 증가! 새로운 최대 HP: {health.startingHealth}");
        }
    }
}
