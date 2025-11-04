using System.Threading.Tasks;
using Game.Components;
using Godot;
using GodotUtilities;

using Logger = Game.Utils.Logger;

namespace Game;

[Scene]
public partial class Entity : Node2D
{
    [Node] public StatsManager StatsManager;
    [Node] public StatusEffectManager StatusEffectManager;

    [Signal] public delegate void DiedEventHandler();

    public bool IsAlive => StatsManager.Health > 0;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        StatsManager.StatDepleted += OnStatDepleted;
    }

    public virtual Task TakeTurn(Entity target)
    {
        var targetStatsManager = target.StatsManager;

        var attack = StatsManager.CreateAttack();

        targetStatsManager.ReceiveAttack(attack);
        Logger.Debug(
            $"{Name} attacks {target.Name} for {StatsManager.Damage} damage!" +
            $" Target has {targetStatsManager.Health} health remaining."
        );

        return Task.CompletedTask;
    }

    public virtual Utils.TurnAction GetTurnAction(Entity target)
    {
        return new Utils.TurnAction
        {
            Actor = this,
            Targets = new Entity[] { target },
            Steps = new System.Collections.Generic.List<Utils.ITurnStep>
            {
                new Utils.DamageStep(ctx => ctx.ActorStats.CreateAttack()),
                new Utils.ResolveDeathsStep()
            },
            Name = "BasicAttack"
        };
    }

    private void OnStatDepleted(StatsManager.Stat stat)
    {
        if (stat != StatsManager.Stat.Health) return;

        Logger.Debug($"{Name} has been defeated!");
        QueueFree();
        EmitSignalDied();
    }
}