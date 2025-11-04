using System;
using System.Collections.Generic;
using Game.Data;
using Game.Utils;
using Godot;

using Logger = Game.Utils.Logger;

namespace Game.Components;

public partial class StatusEffectManager : Node
{
    [Signal] public delegate void StatusEffectAppliedEventHandler(StatusEffect effect);
    [Signal] public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);
    [Signal] public delegate void StatusEffectTickedEventHandler(StatusEffect effect);

    private Entity entity;
    private readonly Dictionary<Type, StatusEffect> activeStatusEffects = [];
    private readonly List<StatusEffect> statusEffectsToRemove = [];
    public IReadOnlyDictionary<Type, StatusEffect> ActiveStatusEffects => activeStatusEffects;

    public override void _Ready()
    {
        entity = GetParentOrNull<Entity>();

        if (entity != null) return;

        Logger.Error("StatsManager must be a child of an Entity node.");
    }

    public bool ApplyStatusEffect(StatusEffect effect)
    {
        var type = effect.GetType();

        if (activeStatusEffects.TryGetValue(type, out var existing))
        {
            if (!existing.CanStackWith(effect)) return false;

            Logger.Info($"{effect} applied to {entity}");

            existing.Stack(effect);
            return true;
        }

        effect.ApplyStatusEffect(entity);
        activeStatusEffects[type] = effect;
        EmitSignalStatusEffectApplied(effect);

        return true;
    }

    public bool RemoveStatusEffect<T>() where T : StatusEffect
    {
        return RemoveStatusEffect(typeof(T));
    }

    public bool RemoveStatusEffect(Type type)
    {
        if (!activeStatusEffects.TryGetValue(type, out var effect)) return false;

        effect.Remove();
        activeStatusEffects.Remove(type);
        EmitSignalStatusEffectRemoved(effect);

        return true;
    }

    public void TickAllStatusEffects()
    {
        statusEffectsToRemove.Clear();

        foreach (var effect in activeStatusEffects.Values)
        {
            effect.Update();
            EmitSignalStatusEffectTicked(effect);

            if (effect.RemainingDuration <= 0)
            {
                statusEffectsToRemove.Add(effect);
            }
        }

        foreach (var effect in statusEffectsToRemove)
        {
            RemoveStatusEffect(effect.GetType());
        }
    }

    public void ClearAllStatusEffects()
    {
        foreach (var effect in activeStatusEffects.Values)
        {
            effect.Remove();
            EmitSignalStatusEffectRemoved(effect);
        }

        activeStatusEffects.Clear();
    }

    public void OnAttackReceived(Attack attack)
    {
        foreach (var effect in activeStatusEffects.Values)
        {
            effect.ModifyIncomingAttack(attack);
        }
    }

    public void OnAttackOutgoing(Attack attack)
    {
        foreach (var effect in activeStatusEffects.Values)
        {
            effect.ModifyOutgoingAttack(attack);
        }
    }
}