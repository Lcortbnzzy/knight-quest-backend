using System.Linq;
using Game.Components;
using Game.Entities;
using Game.UI;
using Godot;

namespace Game.Utils;

#nullable enable
public static class NodeExtensions
{
    public static Player? GetPlayer(this Node node)
    {
        var player = node.GetTree().GetNodesInGroup("Player").FirstOrDefault();

        if (player is not null) return player as Player;

        // manually search for Player in case it's not yet in the group
        player = node.GetTree().Root.FindChildOfType<Player>();

        if (player is null) return null;

        player?.AddToGroup("Player");

        return player as Player;
    }

    public static RunManager? GetRunManager(this Node node)
    {
        var runManager = node.GetTree().GetNodesInGroup("RunManager").FirstOrDefault();

        return runManager as RunManager;
    }

    public static HeadsUpDisplay? GetHeadsUpDisplay(this Node node)
    {
        var hud = node.GetTree().GetNodesInGroup("HeadsUpDisplay").FirstOrDefault();

        if (hud is null)
        {
            // manually search for HUD in case it's not yet in the group
            hud = node.GetTree().Root.FindChildOfType<HeadsUpDisplay>();
            hud?.AddToGroup("HeadsUpDisplay");
        }

        return hud as HeadsUpDisplay;
    }

    /// <summary>
    /// Recursively finds the first child node of the given type.
    /// </summary>
    public static T? FindChildOfType<T>(this Node node) where T : Node
    {
        foreach (var child in node.GetChildren())
        {
            switch (child)
            {
                case T tChild:
                    return tChild;
                case { } childNode:
                {
                    var result = childNode.FindChildOfType<T>();
                    if (result is not null)
                        return result;
                    break;
                }
            }
        }

        return null;
    }
}