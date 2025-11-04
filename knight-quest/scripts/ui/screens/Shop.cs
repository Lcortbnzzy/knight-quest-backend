using Godot;
using System.Collections.Generic;
using System.Linq;
using Game.Autoloads;
using Game.Data;
using Game.Entities;
using Game.Utils;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game.UI;

[Scene]
public partial class Shop : CanvasLayer
{
    private PackedScene scene = GD.Load<PackedScene>("res://scenes/ui/screens/main_menu.tscn");

    [Node] private GridContainer slotContainer;
    [Node] private Label coinLabel;
    [Node] private Label selectedItemName;
    [Node] private RichTextLabel selectedItemDescription;
    [Node] private Button buyButton;
    [Node] private TextureRect itemIcon;


    [Signal]
    public delegate void ItemBoughtEventHandler();

    private List<Slot> slots;
    private Item selectedItem;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        slots = slotContainer.GetChildrenOfType<Slot>().ToList();
        ConnectSlotSignals();
        buyButton.Pressed += OnBuyButtonPress;


        ShopManager.Instance.CoinsChanged += OnCoinsChanged;

        PopulateSlots();
        UpdateCoinLabel();
        Reset();
    }

    private void ConnectSlotSignals()
    {
        foreach (var slot in slots)
        {
            slot.Pressed += (consumable) => SelectSlot(slot);
        }
    }

    private void PopulateSlots()
    {
        var items = ShopManager.GetItemsByType<Consumable>();

        Logger.Info($"[DEBUG] Populating {slots.Count} slots with {items.Count} items");

        foreach (var slot in slots)
        {
            ClearSlot(slot);
        }

        for (int i = 0; i < slots.Count && i < items.Count; i++)
        {
            var slot = slots[i];
            var item = items[i];

            SetupSlot(slot, item);
            Logger.Info($"[DEBUG] Set slot {i} with item: {item.Name}");
        }
    }

    private void SetupSlot(Slot slot, Item item)
    {
        var itemGroup = new ItemGroup
        {
            Item = item,
            Quantity = 1
        };

        slot.ItemGroup = itemGroup;

        slot.Visible = true;

        Logger.Info($"[DEBUG] Setup slot with {item.Name}, Icon: {item.Icon != null}");
    }

    private void ClearSlot(Slot slot)
    {
        slot.ItemGroup = null;
        slot.Modulate = Colors.White;
        slot.Visible = true;
    }

    private void SelectSlot(Slot slot)
    {
        if (slot.ItemGroup?.Item != null)
        {
            UpdateSelectedItem(slot.ItemGroup.Item);
            Logger.Info($"[DEBUG] Selected item: {slot.ItemGroup.Item.Name}");
            selectedItemName.Text = slot.ItemGroup.Item.Name;
            selectedItemDescription.Text = slot.ItemGroup.Item.Description;
            itemIcon.Texture = slot.ItemGroup.Item.Icon;

            HighlightSlot(slot);
        }
    }

    private void HighlightSlot(Slot selectedSlot)
    {
        foreach (var slot in slots)
        {
            slot.Modulate = Colors.White;
        }

        selectedSlot.Modulate = Colors.Yellow;
    }

    private void UpdateSelectedItem(Item item)
    {
        selectedItem = item;
        selectedItemName.Text = item?.Name ?? "No item selected";
        selectedItemDescription.Text = item?.Description ?? string.Empty;
        UpdateButtonState();
    }

    private void OnCoinsChanged(int newCoins)
    {
        UpdateCoinLabel();
        UpdateButtonState();
    }

    private void UpdateCoinLabel()
    {
        coinLabel.Text = $"Coins: {ShopManager.Stars}";
    }

    private void OnBuyButtonPress()
    {
       AudioManager.Instance.PlayClick();

        if (!CanBuy()) return;

        ShopManager.BuyItem(selectedItem);
        EmitSignalItemBought();

        UpdateCoinLabel();
        UpdateButtonState();
    }

    private void Reset()
    {
        if (!IsInstanceValid(this) || slots.Count == 0) return;

        var firstSlotWithItem = slots.FirstOrDefault(s => s.ItemGroup?.Item != null);
        if (firstSlotWithItem != null)
        {
            SelectSlot(firstSlotWithItem);
        }
        else
        {
            selectedItem = null;
            UpdateSelectedItem(null);
        }
    }

    private void UpdateButtonState()
    {
        bool canBuy = CanBuy();
        buyButton.Visible = selectedItem != null;
        buyButton.Disabled = !canBuy;
        buyButton.Text = selectedItem == null ? "Select an item" :
            canBuy ? $"Buy ({selectedItem.Cost} coins)" :
            "Not enough coins";
    }

    private bool CanBuy()
    {
        return selectedItem != null && ShopManager.Stars >= selectedItem.Cost;
    }

    public override void _ExitTree()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.CoinsChanged -= OnCoinsChanged;
        }
    }
}