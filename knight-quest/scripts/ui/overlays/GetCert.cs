using Godot;
using Game.Autoloads;
using GodotUtilities;

namespace Game.UI;

[Scene]
public partial class GetCert : CanvasLayer
{
    [Node] private Button getCertBtn;

    [Export] private string url = "https://knight.lawrenceallen.tech/certificate";

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        getCertBtn.Pressed += OnOpenLinkButtonPressed;
    }

    public void OnOpenLinkButtonPressed()
    {
        AudioManager.Instance.PlayClick();
        OS.ShellOpen(url);
    }
}