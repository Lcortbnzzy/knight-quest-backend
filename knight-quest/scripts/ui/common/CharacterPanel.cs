using Game.Data;
using Game.Utils;
using Godot;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game;

[Scene]
public partial class CharacterPanel : Control
{
    [Export] public Character character;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        Callable.From(() =>
        {
            var rect = GetRect();
            PivotOffset = rect.Size / 2;

            Logger.Debug(rect);

            var instance = character.Scene.InstantiateOrNull<Node2D>();

            if (instance == null) return;

            AddChild(instance);
            instance.Position = PivotOffset + new Vector2(0, 40);
        }).CallDeferred();
    }
}