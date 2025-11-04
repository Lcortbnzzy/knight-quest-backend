using Godot;
using System;
using Game.Autoloads;
using GodotUtilities;


namespace Game.UI;

[Scene]
public partial class BackButton : Button
{
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();

        Pressed += OnBackButtonPressed;
    }


    private async void OnBackButtonPressed()
    {
        AudioManager.Instance.PlayClick();
        Navigator.Back();
    }
}