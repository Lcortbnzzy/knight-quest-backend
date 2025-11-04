namespace Game.Data;

public partial class Potion : Consumable
{
    public override void Use(Entity target)
    {
        target.StatsManager.Heal(3);
    }
}