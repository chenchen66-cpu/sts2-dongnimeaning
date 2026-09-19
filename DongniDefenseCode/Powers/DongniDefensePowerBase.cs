using BaseLib.Abstracts;
using BaseLib.Extensions;
using DongniDefense.DongniDefenseCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace DongniDefense.DongniDefenseCode.Powers;

/// <summary>
/// Base class for this mod's powers; loads power icons from the mod's own resources.
/// </summary>
public abstract class DongniDefensePowerBase : CustomPowerModel
{
    //Loads from DongniDefense/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

    /// <summary>
    /// Whether this power is a buff or debuff.
    /// </summary>
    public abstract override PowerType Type { get; }
    
    /// <summary>
    /// How this power stacks if reapplied. Counter is the most common type, where applying the power again just
    /// adds to the amount. Single means the power does not stack, like Barricade. None functions identically to
    /// Single, but you're suggested to use Single as it is more explicit about how it will work.
    /// </summary>
    public abstract override PowerStackType StackType { get; }
}
