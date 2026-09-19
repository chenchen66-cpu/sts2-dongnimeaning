using DongniDefense.DongniDefenseCode.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace DongniDefense.DongniDefenseCode.Patches;

/// <summary>
/// 东尼的诅咒 needs the size of a draw before it cancels it. The game only exposes "should this draw
/// happen", so record the requested amount here; <see cref="DongniCurse.ShouldDraw"/> cancels the draw
/// and <see cref="DongniCurse.AfterPreventingDraw"/> converts it into the base game's
/// "draw N cards next turn" power.
/// </summary>
[HarmonyPatch(typeof(CardPileCmd), "DrawInternal")]
internal static class DongniCurseDrawPatch
{
    [HarmonyPrefix]
    private static void RecordDeferredDraw(decimal count, Player player)
    {
        if (count <= 0m || player.PlayerCombatState == null) return;

        foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
        {
            if (card is not DongniCurse curse) continue;
            if (curse.Pile?.Type != PileType.Hand) continue;

            curse.RecordBlockedDraw(count);
            return;
        }
    }
}
