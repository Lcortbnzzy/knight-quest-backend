using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Godot;
using Logger = Game.Utils.Logger;

namespace Game.Autoloads;

[GlobalClass]
public partial class ShopManager : Autoload<ShopManager>
{
    [Signal] public delegate void CoinsChangedEventHandler(int coins);
    [Signal] public delegate void ItemBoughtEventHandler(Item item);

    public static int Stars
    {
        get => SaveManager.Data.Shop.Stars;
        set => SaveManager.Data.Shop.Stars = value;
    }

    private static readonly Dictionary<Type, List<Item>> shopItems = new()
    {
        { typeof(Consumable), [] },
    };

    public override void _Ready()
    {
        LoadShopItems();
    }

    private void LoadShopItems()
    {
        shopItems[typeof(Consumable)] = ItemRegistry.GetItemsByType<Consumable>().Select(Item (item) => item).ToList();
        Logger.Debug($"Consumables loaded: {shopItems[typeof(Consumable)].Count}");

        if (!OS.IsDebugBuild() || shopItems[typeof(Consumable)].Count <= 0) return;

        var firstConsumable = shopItems[typeof(Consumable)][0];
        InventoryManager.Instance.AddItem(firstConsumable);
        Logger.Debug($"Added {firstConsumable.Name} to inventory.");
    }

    public static void AddCoins(int amount)
    {
        Stars += amount;
        Instance.EmitSignalCoinsChanged(Stars);
    }

    public static void SpendCoins(int amount)
    {
        if (Stars < amount) return;

        Stars -= amount;
        Instance.EmitSignalCoinsChanged(Stars);
    }

    public static List<Item> GetItemsByType<T>() where T : Item =>
        shopItems.TryGetValue(typeof(T), out var items) ? items : [];

    public static void BuyItem(Item item)
    {
        if (item is null) return;
        if (Stars < item.Cost) return;

        var typeKey = typeof(Consumable);

        if (!shopItems.TryGetValue(typeKey, out var items))
            return;

        var existingItem = items.Find(i => i.ResourcePath == item.ResourcePath);
        if (existingItem is null) return;

        SpendCoins(item.Cost);

        if (SaveManager.Data is not null)
            SaveManager.Data.Shop.Stars = Stars;

        InventoryManager.Instance.AddItem(item);

        Instance.EmitSignalItemBought(item);

        Logger.Info($"Bought {item.Name}");

        Logger.Info(InventoryManager.Instance.Items.TryGetValue(item, out var itemGroup)
            ? $"{item.Name} Quantity: {itemGroup.Quantity}"
            : $"{item.Name} not found in inventory.");

        SaveManager.SaveInventory();
    }
}