using Godot;

namespace Game.Data;

[GlobalClass]
public partial class CombatSequence : Resource
{
    [Export] public Encounter[] Encounters = [];
    [Export] public int TurnDuration = 30;
}