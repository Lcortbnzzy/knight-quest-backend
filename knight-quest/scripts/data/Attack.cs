using System.Collections.Generic;
using Godot;

namespace Game.Data;

public partial class Attack: RefCounted
{
    public int Damage;
    public Entity Source;
    public bool IsCritical;
    public bool IsMissed;
    public List<StatusEffect> StatusEffects;
    public bool HasStatusEffects => StatusEffects.Count > 0;
}