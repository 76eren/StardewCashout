using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace StardewGambling;

public sealed class ModEntry : Mod
{
    private const string CashOutItemId = "Eren.StardewGambling_CashOut250";

    private const string QualifiedCashOutItemId =
        "(O)Eren.StardewGambling_CashOut250";

    private const int QiCoinCost = 1000;
    private const int GoldReward = 250;

    public override void Entry(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
        Monitor.Log(
            "Mod loaded. You can now buy the Cash Out item from the Casino shop.",
            LogLevel.Info
        );
    }

    private void OnAssetRequested(
        object? sender,
        AssetRequestedEventArgs e
    )
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(asset =>
            {
                var objects = asset
                    .AsDictionary<string, ObjectData>()
                    .Data;

                objects[CashOutItemId] = new ObjectData
                {
                    Name = "Casino Cash Out",
                    DisplayName = "Cash Out 250g",

                    Description =
                        "Exchange 1,000 Qi Coins for 250g. Never said it would be a fair deal.",

                    Type = "Basic",
                    Category = 0,
                    Price = 0,

                    // I don't have a sprite (yet), so I'll use a vanilla one.
                    // 336 is the Gold Bar sprite.
                    Texture = "Maps/springobjects",
                    SpriteIndex = 336,

                    Edibility = -300,
                    ExcludeFromShippingCollection = true,
                    ExcludeFromFishingCollection = true,
                    ExcludeFromRandomSale = true,

                    CanBeGivenAsGift = false
                };
            });
        }

        // Add the item to the Casino shop.
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var shops = asset
                    .AsDictionary<string, ShopData>()
                    .Data;

                if (!shops.TryGetValue("Casino", out ShopData? casino))
                {
                    Monitor.Log(
                        "Couldn't find the Casino shop.",
                        LogLevel.Error
                    );

                    return;
                }

                // Avoid adding it twice if the asset gets edited again.
                if (casino.Items.Any(
                        item => item.Id == CashOutItemId
                    ))
                {
                    return;
                }

                casino.Items.Insert(
                    0,
                    new ShopItemData
                    {
                        Id = CashOutItemId,
                        ItemId = QualifiedCashOutItemId,
                        Price = QiCoinCost
                    }
                );
            });
        }
    }

    // This event is triggered when the player's inventory changes, in this case when they buy the Cash Out item from the Casino shop.
    // Maybe this is a stupid way of doing it but I wasn't sure how else to do it
    private void OnInventoryChanged(
        object? sender,
        InventoryChangedEventArgs e
    )
    {
        if (!e.IsLocalPlayer)
        {
            return;
        }

        foreach (Item item in e.Added.ToArray())
        {
            if (item.QualifiedItemId != QualifiedCashOutItemId)
                continue;

            int amount = item.Stack;

            e.Player.Items.Remove(item);

            int goldToGive = GoldReward * amount;

            e.Player.Money += goldToGive;

            Game1.addHUDMessage(
                new HUDMessage(
                    $"+{goldToGive}g",
                    HUDMessage.newQuest_type
                )
            );

            Monitor.Log(
                $"Player exchanged {QiCoinCost * amount} Qi Coins " +
                $"for {goldToGive}g.",
                LogLevel.Info
            );
        }
    }
}
