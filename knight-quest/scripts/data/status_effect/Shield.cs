using Game.Utils;

namespace Game.Data;

public partial class Shield : StatusEffect
{
    public override void Apply()
    {
        Target.StatsManager.SetInvulnerable(true);
        Logger.Debug("[Shield::Apply] Shield applied to " + Target.Name);
    }

    public override void Remove()
    {
        Target.StatsManager.SetInvulnerable(false);
        Logger.Debug("[Shield::Remove] Shield removed from " + Target.Name);
    }
}
