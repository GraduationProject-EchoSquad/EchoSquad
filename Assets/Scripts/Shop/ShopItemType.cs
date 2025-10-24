// 상점 아이템 타입 정의
public enum ShopItemType
{
    FireRate,           // 총알 나가는 속도 증가
    AmmoRefill,         // 총알 채우기
    HealthRestore,      // HP 최대치까지 회복
    MaxHealthIncrease   // HP 최대 용량 늘리기
}

// 아이템 적용 대상
public enum ShopItemTarget
{
    Player,     // 플레이어
    AI          // AI 동료
}
