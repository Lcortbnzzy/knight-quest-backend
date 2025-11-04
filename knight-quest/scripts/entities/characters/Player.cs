using System.Threading.Tasks;
using Game.Components;
using Game.UI;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public abstract partial class Player : Entity
{
    protected const string ATTACK = "attack";
    protected const string HURT = "hurt";
    protected const string DIE = "die";

    [Node] private AnimationTree animationTree;
    [Export] public PackedScene ProjectileScene;
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

        StatsManager.StatDepleted += OnStatDepleted;
        StatsManager.StatDecreased += OnStatDecreased;
    }

    // NOTE: always put the turn-taking logic after the animation playback to avoid animation race conditions
    //       (e.g. if the player attacks and the enemy dies, the player will still play the attack animation)
    public override async Task TakeTurn(Entity target)
    {
        playback.Travel(ATTACK);
        await ToSignal(animationTree, "animation_finished");
        await base.TakeTurn(target);
    }

    public override Utils.TurnAction GetTurnAction(Entity target)
    {
        return new Utils.TurnAction
        {
            Actor = this,
            Targets = [target],
            Steps = new System.Collections.Generic.List<Utils.ITurnStep>
            {
                new Utils.PlayAnimationTreeStateStep.CustomStep(ctx => Shoot()),
                new Utils.PlayAnimationTreeStateStep(animationTree, ATTACK),
                new Utils.DamageStep(ctx => ctx.ActorStats.CreateAttack()),
                new Utils.ResolveDeathsStep()
            },

            Name = "PlayerAttack"
        };
    }

    private void OnStatDepleted(StatsManager.Stat stat)
    {
        if (stat != StatsManager.Stat.Health) return;

        playback.Travel(DIE);
    }

    private void OnStatDecreased(int _, StatsManager.Stat stat)
    {
        if (stat != StatsManager.Stat.Health) return;

        playback.Travel(HURT);
    }

    private async void Shoot()
    {
        if (ProjectileScene == null) return;

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        var projectileInstance = (Projectile)ProjectileScene.Instantiate();
        GetTree().CurrentScene.AddChild(projectileInstance);

        projectileInstance.GlobalPosition = GetNode<Marker2D>("ProjectileSpawnPoint").GlobalPosition;
        projectileInstance.Direction = Vector2.Right; 
    }
}