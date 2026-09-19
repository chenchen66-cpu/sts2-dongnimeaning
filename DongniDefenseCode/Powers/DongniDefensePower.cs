using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DongniDefense.DongniDefenseCode.Powers;

/// <summary>
/// 东尼防御 / Dongni Defense power.
/// At the start of your turn, gain Block. While this power is active, Block from cards is disabled.
/// The amount of this power is the block gained each turn.
/// </summary>
public sealed class DongniDefensePower : DongniDefensePowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    /// <summary>
    /// Start of the owner's turn: gain block. Unpowered so block modifiers (e.g. Frail, Dexterity) do not
    /// change it, matching how the base game handles "gain block at the start of your turn" powers.
    /// </summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }

    /// <summary>
    /// "You can no longer gain Block from cards."
    /// Mirrors the base game's NoBlockPower: card-sourced block is multiplied by zero, while block that does
    /// not come from a card (relics, powers, potions) is unaffected.
    /// </summary>
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1m;
        if (props.HasFlag(ValueProp.Unpowered)) return 1m;
        if (cardSource == null) return 1m;

        return 0m;
    }

    /// <summary>
    /// Localization defined in code as well as in this mod's localization/*/powers.json. The json tables are
    /// what translators should edit; this copy guarantees the power never shows a raw localization key.
    /// </summary>
    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "东尼防御",
            "在你的回合开始时，获得[blue]15[/blue]点[gold]格挡[/gold]。你无法从卡牌中获得[gold]格挡[/gold]。",
            "在你的回合开始时，获得[blue]{Amount}[/blue]点[gold]格挡[/gold]。你无法从卡牌中获得[gold]格挡[/gold]。"),
        _ => new PowerLoc(
            "Dongni Defense",
            "At the start of your turn, gain [blue]15[/blue] [gold]Block[/gold]. You can no longer gain [gold]Block[/gold] from cards.",
            "At the start of your turn, gain [blue]{Amount}[/blue] [gold]Block[/gold]. You can no longer gain [gold]Block[/gold] from cards.")
    };
}
