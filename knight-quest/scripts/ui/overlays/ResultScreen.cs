using Godot;
using System;
using Game.Autoloads;
using GodotUtilities;
using GodotUtilities.Util;


namespace Game.UI;

[Scene]
public partial class ResultScreen : CanvasLayer
{
    [Node] private RichTextLabel resultLabel;
    [Node] private Label rewardsLabel;
    [Node] private Button menuButton;
    [Node] private Button retryButton;


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        menuButton.Pressed += OnMenuButtonPress;
        retryButton.Pressed += OnRetryButtonPress;
    }

    private async void OnMenuButtonPress()
    {
        AudioManager.Instance.PlayClick();
        AudioManager.Instance.ResumeMusic();
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        Navigator.Push("res://scenes/ui/screens/main_menu.tscn");
        QueueFree();
    }

    private async void OnRetryButtonPress()
    {
        AudioManager.Instance.PlayClick();
        AudioManager.Instance.ResumeMusic();
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        GetTree().ReloadCurrentScene();
        QueueFree();
    }


    public void ShowResult(bool victory, int starsEarned = 0)
    {
        SaveManager.Save();

        if (victory)
        {
            resultLabel.Text = "Victory!";
            resultLabel.AddThemeColorOverride("font_color", Colors.Green);
            rewardsLabel.Text = $"Stars earned: {starsEarned}";
        }
        else
        {
            resultLabel.Text = "Defeat...";
            resultLabel.AddThemeColorOverride("font_color", Colors.Red);
            rewardsLabel.Text = "No stars earned.";
        }
    }
}