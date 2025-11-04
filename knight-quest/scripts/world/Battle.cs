using Game.Autoloads;
using Game.Components;
using Game.Entities;
using Game.UI;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Battle : Node2D
{
    [Node] private HeadsUpDisplay headsUpDisplay;
    [Node] private RunManager runManager;
    [Node] private Node entities;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override async void _Ready()
    {
        AudioManager.Instance.StopMusic();
        // Setup RunManager
        runManager.EncounterStarted += OnEncounterStarted;
        runManager.SetConfiguration(GameManager.Config.Level.CombatSequence);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Spawn selected character
        // TODO: Create a proper character factory
        var player = GameManager.Config.Character.Scene.InstantiateOrNull<Player>();

        if (player == null) return;

        entities.AddChild(player);
        player.GlobalPosition = headsUpDisplay.PlayerGlobalPosition;
        headsUpDisplay.setPlayer(player);
    }

    private void OnEncounterStarted(Entity[] enemies)
    {
        foreach (var enemy in enemies)
        {
            entities.AddChild(enemy);
            enemy.GlobalPosition = headsUpDisplay.EnemyGlobalPosition;
            headsUpDisplay.SetEnemy(enemy);
        }
    }
}