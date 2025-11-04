// File: scripts/data/status_effect/DoubleDamage.cs
using Game.Data;

namespace Game.Data;

public partial class AttackUp : StatusEffect
{
    public override void ModifyOutgoingAttack(Attack attack)
    {
        attack.Damage = 2;
    }
}