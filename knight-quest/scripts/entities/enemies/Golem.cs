using Godot;
using System.Threading.Tasks;

namespace Game.Entities;

public partial class Golem : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
    }
    
    public override Task TakeTurn(Entity target)
    {
        GD.Print("Enemy1 attacks with a special move!");
        _ = base.TakeTurn(target); 
        return Task.CompletedTask;
    }
    

}