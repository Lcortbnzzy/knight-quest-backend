using Godot;

namespace Game;

public partial class Main : Node
{
    public override void _Ready()
    {
        Callable.From(() => { GetTree().ChangeSceneToFile("res://scenes/ui/screens/login.tscn"); }).CallDeferred();
    }
}