using Godot;

namespace Game.Data;

[GlobalClass]
public partial class Encounter : Resource
{
    [Export] public PackedScene[] Enemies = [];
}