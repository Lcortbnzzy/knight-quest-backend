using GodotUtilities.Util;

namespace Game.Data;

public partial class ErrorShieldPotion : Consumable
{
    public override void Use(Entity target)
    {
        var shield = new Shield();
        target.StatusEffectManager.ApplyStatusEffect(shield);

        Logger.Debug("[ErrorShieldConsumable::Use] Error Shield applied to " + target.Name);
    }
}