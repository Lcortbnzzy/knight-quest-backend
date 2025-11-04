using Godot;

namespace Game.Data;

[GlobalClass]
public abstract partial class Consumable : Item
{
    [Export] public StatusEffect[] StatusEffects = [];

    public virtual void Use(Entity target)
    {
        foreach (var statusEffect in StatusEffects)
            target.StatusEffectManager.ApplyStatusEffect(statusEffect);
    }
}