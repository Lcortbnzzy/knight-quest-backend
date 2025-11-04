using Game.Autoloads;
using Godot;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game;

[Scene]
public partial class MainMenu : CanvasLayer
{
    [Node] private Button startButton;
    [Node] private Button shopButton;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        startButton.Pressed += OnStartButtonPressed;
        shopButton.Pressed += OnShopButtonPressed;
        SaveManager.StartSaving();
    }

    private void OnStartButtonPressed()
    {
        AudioManager.Instance.PlayClick();
        Navigator.Push("res://scenes/ui/screens/subject_select.tscn");
    }

    private void OnShopButtonPressed()
    {
        AudioManager.Instance.PlayClick();
        Navigator.Push("res://scenes/ui/screens/shop.tscn");
    }
}