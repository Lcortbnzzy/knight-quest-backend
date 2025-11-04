using Godot;
using System.Threading.Tasks;
using Game.Components;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game.Entities;

[Scene]
public partial class Enemy : Entity
{
    private const string ATTACK = "attack";
    private const string HURT = "hurt";
    private const string DIE = "die";


    [Node] private AnimationTree animationTree;
    [Node] private StatsManager statsManager;
    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        this.AddToGroup();
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        statsManager.StatDecreased += OnStatDecreased;
        statsManager.StatDepleted += OnStatDepleted;
    }

    private async void OnStatDepleted(StatsManager.Stat stat)
    {
        if (stat != StatsManager.Stat.Health) return;

        playback.Travel(DIE);
        await ToSignal(animationTree, "animation_finished");
        QueueFree();
    }

    private void OnStatDecreased(int amount, StatsManager.Stat stat)
    {
        if (stat != StatsManager.Stat.Health) return;

        playback.Travel(HURT);
    }

    public override async Task TakeTurn(Entity target)
    {
        Logger.Info("Enemy is taking a turn!");

        if (target == null || target.StatsManager.Health <= 0)
        {
            Logger.Info("No valid target.");
            return;
        }

        Logger.Info("Enemy attacking...");
        playback.Travel(ATTACK);
        Logger.Info("Waiting for animation to finish...");
        await ToSignal(animationTree, "animation_finished");
        Logger.Info("Applying damage to player.");
        target.StatsManager.TakeDamage(1);
    }

    public override Utils.TurnAction GetTurnAction(Entity target)
    {
        return new Utils.TurnAction
        {
            Actor = this,
            Targets = [target],
            Steps = new System.Collections.Generic.List<Utils.ITurnStep>
            {
                new Utils.PlayAnimationTreeStateStep(animationTree, ATTACK),
                new Utils.DamageStep(ctx => ctx.ActorStats.CreateAttack()),
                new Utils.ResolveDeathsStep()
            },
            Name = "EnemyAttack"
        };
    }
}