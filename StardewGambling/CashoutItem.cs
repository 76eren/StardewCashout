using StardewValley.GameData.Objects;

namespace StardewGambling;

public class CashoutItem
{
    public CashoutItem(
        string cashOutItemId,
        string qualifiedCashOutItemId,
        int qiCoinCost,
        int goldReward,
        ObjectData objectData
    )
    {
        CashOutItemId = cashOutItemId;
        QualifiedCashOutItemId = qualifiedCashOutItemId;
        QiCoinCost = qiCoinCost;
        GoldReward = goldReward;
        ObjectData = objectData;
    }

    public string CashOutItemId { get; init; }
    public string QualifiedCashOutItemId { get; init; }
    public int QiCoinCost { get; init; }
    public int GoldReward { get; init; }
    public ObjectData ObjectData { get; init; }
}
