using Godot;

namespace Game.Data;

public partial class Regen : StatusEffect
{
    [Export] private int HealAmount = 1;

    public override void Tick()
    {
        Target.StatsManager.Heal(HealAmount);
    }
}