using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Utils;

public interface ITurnStep
{
    Task ExecuteAsync(TurnContext ctx, CancellationToken ct);
}

public sealed class TurnAction
{
    public Entity Actor { get; init; }
    public IReadOnlyList<Entity> Targets { get; init; } = Array.Empty<Entity>();
    public IReadOnlyList<ITurnStep> Steps { get; init; } = Array.Empty<ITurnStep>();
    public string Name { get; init; } = "Action";
}

public sealed class TurnContext
{
    public RunManager Run { get; }
    public Entity Actor { get; }
    public IReadOnlyList<Entity> Targets { get; }

    public StatsManager ActorStats => Actor.StatsManager;

    public TurnContext(RunManager run, Entity actor, IReadOnlyList<Entity> targets)
    {
        Run = run;
        Actor = actor;
        Targets = targets;
    }
}

public sealed class TurnEngine
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task ExecuteAsync(RunManager run, TurnAction action, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (action.Actor == null) return;

            var validTargets = action.Targets.Where(t => GodotObject.IsInstanceValid(t) && t.IsAlive).ToArray();
            var ctx = new TurnContext(run, action.Actor, validTargets);

            foreach (var step in action.Steps)
            {
                ct.ThrowIfCancellationRequested();
                await step.ExecuteAsync(ctx, ct);
            }
        }
        finally
        {
            _gate.Release();
        }
    }
}

public sealed class DamageStep : ITurnStep
{
    private readonly Func<TurnContext, Game.Data.Attack> _attackFactory;

    public DamageStep(Func<TurnContext, Game.Data.Attack> attackFactory)
    {
        _attackFactory = attackFactory;
    }

    public Task ExecuteAsync(TurnContext ctx, CancellationToken ct)
    {
        var attack = _attackFactory(ctx);
        foreach (var target in ctx.Targets.Where(t => GodotObject.IsInstanceValid(t) && t.IsAlive))
        {
            target.StatsManager.ReceiveAttack(attack);
        }

        return Task.CompletedTask;
    }
}

public sealed class ResolveDeathsStep : ITurnStep
{
    public async Task ExecuteAsync(TurnContext ctx, CancellationToken ct)
    {
        // If entities support async death sequences, await them here.
        // Currently, Entity queues free immediately on death; nothing to await.
        await Task.CompletedTask;
    }
}

public sealed class TickStatusEffectsStep : ITurnStep
{
    private readonly Func<TurnContext, IEnumerable<Entity>> _participants;

    public TickStatusEffectsStep(Func<TurnContext, IEnumerable<Entity>> participants)
    {
        _participants = participants;
    }

    public Task ExecuteAsync(TurnContext ctx, CancellationToken ct)
    {
        foreach (var e in _participants(ctx))
        {
            if (GodotObject.IsInstanceValid(e))
                e.StatsManager.TickStatusEffects();
        }

        return Task.CompletedTask;
    }
}

public sealed class PlayAnimationTreeStateStep : ITurnStep
{
    private readonly AnimationTree _animationTree;
    private readonly string _stateName;

    public PlayAnimationTreeStateStep(AnimationTree animationTree, string stateName)
    {
        _animationTree = animationTree;
        _stateName = stateName;
    }

    public async Task ExecuteAsync(TurnContext ctx, CancellationToken ct)
    {
        if (_animationTree == null || string.IsNullOrEmpty(_stateName)) return;

        var playback = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback");
        playback.Travel(_stateName);
        await ctx.Run.ToSignal(_animationTree, "animation_finished");
    }

    public sealed class CustomStep : ITurnStep
    {
        private readonly Action<TurnContext> _action;

        public CustomStep(Action<TurnContext> action)
        {
            _action = action;
        }

        public Task ExecuteAsync(TurnContext ctx, CancellationToken ct)
        {
            _action?.Invoke(ctx);
            return Task.CompletedTask;
        }
    }
}