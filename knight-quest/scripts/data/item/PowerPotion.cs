using Game;
using Game.Data;
using Game.Entities;
using GodotUtilities.Util;

namespace Game.Data;

public partial class PowerPotion : Consumable
{
    public override void Use(Entity target)
    {
        
        var attackUp = new AttackUp();
        target.StatusEffectManager.ApplyStatusEffect(attackUp);
        Logger.Debug("[PowerPotion::Use] Power Potion applied to " + target.Name);
    
    }
}