using Game.Components;
using Godot;

namespace Game.Utils;

public static class SceneTreeExtensions
{
    public static TurnTimer CreateTurnTimer(this SceneTree tree, int turns = 3)
    {
        var turnTimer = new TurnTimer
        {
            TurnWait = turns,
            AutoStart = true
        };

        tree.Root.AddChild(turnTimer);

        return turnTimer;
    }
}