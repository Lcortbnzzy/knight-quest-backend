using System;
using Godot;

namespace Game.Data;

public partial class Item : Resource
{
    [Export] public string Id { get; set; } 
    [Export] public string Name;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public Texture2D Icon;

    [Export] public int Cost;
    [Export] public bool Owned;

    public override bool Equals(object other)
    {
        return other != null && GetType() == other.GetType();
    }

    public override int GetHashCode() => GetType().GetHashCode();
}