using Godot;

namespace Game.Data;

[GlobalClass]
public partial class Character : Resource
{
    [Export] public string Name;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public PackedScene Scene;

    public override bool Equals(object other)
    {
        return other != null && GetType() == other.GetType();
    }

    public override int GetHashCode() => GetType().GetHashCode();
}