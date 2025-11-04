using System;
using System.Collections.Generic;
using Game.Data;
using Game.Utils;
using Godot;

using Logger = Game.Utils.Logger;

namespace Game.Autoloads;

public partial class InventoryManager : Autoload<InventoryManager>
{
    [Signal] public delegate void ItemAddedEventHandler(ItemGroup group);
    [Signal] public delegate void UpdatedEventHandler(ItemGroup group);
    [Signal] public delegate void ItemRemovedEventHandler(ItemGroup group);
    [Signal] public delegate void InventoryUpdatedEventHandler();

    private readonly Dictionary<Item, ItemGroup> items = new();
    public IReadOnlyDictionary<Item, ItemGroup> Items => items;

    public void AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (items.TryGetValue(item, out var itemGroup))
        {
            itemGroup.Quantity++;
            items[item] = itemGroup;
        }
        else
        {
            itemGroup = new ItemGroup { Item = item, Quantity = 1 };
            items[item] = itemGroup;
        }

        EmitSignalItemAdded(itemGroup);
    }

    public void RemoveItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!items.TryGetValue(item, out var itemGroup)) return;
    
        if (itemGroup.Quantity > 1)
        {
            itemGroup.Quantity--;
            items[item] = itemGroup;
            EmitSignalUpdated(itemGroup);
        }
        else
        {
            itemGroup.Quantity--;
            items[item] = itemGroup;
            EmitSignalUpdated(itemGroup);
            items.Remove(item);
            EmitSignalItemRemoved(itemGroup);
        }
    }

    public void UseItem(Item item, Entity target)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(target);

        if (!items.TryGetValue(item, out var itemGroup)) return;

        if (item is Consumable consumable)
        {
            consumable.Use(target);
            RemoveItem(item);
            SaveManager.SaveInventory();
        }
        else
        {
            Logger.Warn($"Item {item.Name} is not consumable and cannot be used.");
        }
    }
    public void ClearInventory()
    {
        items.Clear();
        EmitSignal(SignalName.InventoryUpdated); 
    }

}