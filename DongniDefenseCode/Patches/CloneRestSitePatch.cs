using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace DongniDefense.DongniDefenseCode.Patches;

/// <summary>
/// The base game only offers the rest site "Clone" option through the PaelsGrowth relic, even though the
/// Clone enchantment's own text promises the card can be duplicated at a rest site. This adds the same
/// option whenever the player actually has a Clone enchanted card in their deck, so enchanting a card
/// with Clone (e.g. from the 东尼意思 event) really does let it be duplicated at rest sites.
/// </summary>
[HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Generate))]
internal static class CloneRestSitePatch
{
    [HarmonyPostfix]
    private static void AddCloneOption(Player player, ref List<RestSiteOption> __result)
    {
        if (__result.Any(option => option is CloneRestSiteOption)) return;
        if (!player.Deck.Cards.Any(card => card.Enchantment is Clone)) return;

        __result.Add(new CloneRestSiteOption(player));
    }
}
