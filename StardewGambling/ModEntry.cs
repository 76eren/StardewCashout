using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace StardewGambling;

public sealed class ModEntry : Mod
{
    private List<CashoutItem> CashoutItems { get; set; } = new();

    public override void Entry(IModHelper helper)
    {
        CashoutItems = GetCashoutItems();

        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Player.InventoryChanged += OnInventoryChanged;

        Monitor.Log(
            $"Mod loaded. Added {CashoutItems.Count} cash-out item(s) to the Casino shop.",
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

                foreach (CashoutItem cashoutItem in CashoutItems)
                {
                    objects[cashoutItem.CashOutItemId] = cashoutItem.ObjectData;
                }
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

                var existingItemIds = casino.Items
                    .Select(item => item.Id)
                    .ToHashSet();

                // Takes all the CashoutItems, filters out the ones that are already in the shop so we dont add twice, and then creates a new ShopItemData for each of them.
                var shopItems = CashoutItems
                    .Where(item => !existingItemIds.Contains(item.CashOutItemId))
                    .Select(item => new ShopItemData
                    {
                        Id = item.CashOutItemId,
                        ItemId = item.QualifiedCashOutItemId,
                        Price = item.QiCoinCost
                    })
                    .ToList();

                casino.Items.InsertRange(0, shopItems);
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
            CashoutItem? cashoutItem = CashoutItems.FirstOrDefault(
                candidate => candidate.QualifiedCashOutItemId == item.QualifiedItemId
            );

            if (cashoutItem is null)
            {
                continue;
            }

            int amount = item.Stack;

            e.Player.Items.Remove(item);

            int goldToGive = cashoutItem.GoldReward * amount;

            e.Player.Money += goldToGive;

            Game1.addHUDMessage(
                new HUDMessage(
                    $"+{goldToGive}g",
                    HUDMessage.newQuest_type
                )
            );

            Monitor.Log(
                $"Player exchanged {cashoutItem.QiCoinCost * amount} Qi Coins " +
                $"for {goldToGive}g.",
                LogLevel.Info
            );
        }
    }

    private List<CashoutItem> GetCashoutItems()
    {
        return new List<CashoutItem>
        {
            new CashoutItem(
                "Eren.StardewGambling_CashOut250",
                "(O)Eren.StardewGambling_CashOut250",
                1000,
                250,
                new ObjectData
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
                }
            )
        };
    }
}
