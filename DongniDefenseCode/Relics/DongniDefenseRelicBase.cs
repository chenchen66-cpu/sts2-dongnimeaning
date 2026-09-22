using BaseLib.Abstracts;
using BaseLib.Extensions;
using DongniDefense.DongniDefenseCode.Extensions;

namespace DongniDefense.DongniDefenseCode.Relics;

/// <summary>
/// Base class for this mod's relics; loads relic icons from the mod's own resources
/// (packed 94x94, outline 94x94, big 256x256).
/// </summary>
public abstract class DongniDefenseRelicBase : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}
