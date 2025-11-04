using Game.Autoloads;
using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.UI;

[Scene]
public partial class Level : Button
{
    [Export] public LevelInfo levelInfo;
    
    [Node] private RichTextLabel levelLabel;
    [Node] private Label starLabel;

    private bool hovered;

    public override void _Notification(int what)
    {;
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        ButtonUp += OnButtonUp;
        ButtonDown += OnButtonDown;
    }

    public void Setup(LevelInfo level)
    {
        levelInfo = level;
        levelLabel.Text = levelInfo.LevelName;
        starLabel.Text = $"Stars: {levelInfo.StarCount}";
    }

    private void OnMouseEntered()
    {
        hovered = true;

        var tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(1.02f, 1.02f), 0.05f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
    }

    private void OnMouseExited()
    {
        hovered = false;

        var tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(1f, 1f), 0.05f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
    }

    private void OnButtonUp()
    {
        var tween = CreateTween();
        var targetScale = hovered ? new Vector2(1.02f, 1.02f) : new Vector2(1f, 1f);

        tween.TweenProperty(this, "scale", targetScale, 0.05f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
    }

    private void OnButtonDown()
    {
        AudioManager.Instance.PlayClick();
        var tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(0.98f, 0.98f), 0.05f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
    }

    public void DisableLevel()
    {
        Disabled = true;
        Modulate = new Color(0.5f, 0.5f, 0.5f);
        MouseFilter = MouseFilterEnum.Ignore;
    }
}