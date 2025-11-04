using System;
using Godot;

namespace Game.Data;

[GlobalClass]
public abstract partial class StatusEffect : Resource
{
    [Export] public string Id;
    [Export] public string Name;
    [Export] private int TurnDuration = 1;
    [Export] public bool IsStackable;
    [Export] public int MaxStacks = 1;

    public int Stacks { get; protected set; } = 1;
    public int RemainingDuration;
    protected Entity Target;

    public void Update()
    {
        RemainingDuration--;
        Tick();
    }

    public void ApplyStatusEffect(Entity target)
    {
        Target = target;
        RemainingDuration += TurnDuration;
        Apply();
    }

    public virtual void Tick() { }
    public virtual void Apply() { }
    public virtual void Remove() { }

    public virtual void ModifyIncomingAttack(Attack attack) { }
    public virtual void ModifyOutgoingAttack(Attack attack) { }

    public virtual bool CanStackWith(StatusEffect other) =>
        IsStackable && other.GetType() == GetType() && Stacks < MaxStacks;

    public virtual void Stack(StatusEffect other)
    {
        Stacks += Math.Min(Stacks + other.Stacks, MaxStacks);
        RemainingDuration = Math.Max(RemainingDuration, other.TurnDuration);
    }
}