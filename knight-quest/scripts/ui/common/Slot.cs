using Game.Autoloads;
using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.UI;

[Scene]
public partial class Slot : Panel
{
    [Node] public TextureRect icon;
    [Node] private AnimationPlayer animationPlayer;
    [Node] private Label label;

    [Signal] public delegate void PressedEventHandler(Consumable consumable);

    private ItemGroup itemGroup;

    public ItemGroup ItemGroup
    {
        get => itemGroup;
        set
        {
            itemGroup = value;
            icon.Texture = itemGroup?.Item.Icon;
            if (itemGroup is { Item: Consumable })
                label.Text = itemGroup.Quantity.ToString();
            else
                label.Text = string.Empty;
        }
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        GuiInput += OnGuiInput;
    }

    private void OnGuiInput(InputEvent @event)
    {
        
        if (@event is not InputEventMouseButton mouseAction) return;
        if (!mouseAction.Pressed || mouseAction.ButtonIndex != MouseButton.Left) return;
        AudioManager.Instance.PlayClick();
        EmitSignalPressed(ItemGroup.Item as Consumable);
    }
}