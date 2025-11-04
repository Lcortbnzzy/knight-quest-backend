using Game.Utils;

namespace Game.Data;

public partial class Hint : Consumable
{
    public override void Use(Entity target)
    {
        var hud = target.GetHeadsUpDisplay();
        Logger.Debug($"[Hint::Use] HUD is {(hud == null ? "null" : "not null")}");
        // TODO: fix 
        hud?.RevealCorrectAnswer();
    }
}