using Godot;
using GodotUtilities;

namespace Game.UI;

[Scene]
public partial class HealthBar : GridContainer
{
    [Export] public int MaxHealth;
    [Export] public int CurrentHealth;
    [Export] public PackedScene HeartScene;

    public override void _Ready()
    {
        Columns = 10;
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        for (int i = 0; i < MaxHealth; i++)
        {
            var heart = HeartScene.Instantiate();
            AddChild(heart);

            var emptyNode = heart.GetNodeOrNull("Empty");
            var filledNode = heart.GetNodeOrNull("Filled");

            if (emptyNode != null) emptyNode.Set("visible", true);
            if (filledNode != null) filledNode.Set("visible", i < CurrentHealth);
        }


    }
}