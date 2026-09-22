using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace DongniDefense.DongniDefenseCode.Relics;

/// <summary>
/// 东尼圣遗物 / Dongni's Sacred Relic
/// Event relic from the 东尼意思 event: at the end of your turn, if you have 15 or less Block,
/// you gain 2 Energy and draw 2 cards on your next turn.
/// </summary>
[Pool(typeof(EventRelicPool))]
public sealed class DongniSacredRelic : DongniDefenseRelicBase
{
    private const decimal MaxBlockForTrigger = 15m;
    private const decimal EnergyNextTurn = 2m;
    private const decimal CardsNextTurn = 2m;

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<EnergyNextTurnPower>(),
        HoverTipFactory.FromPower<DrawCardsNextTurnPower>()
    ];

    /// <summary>
    /// End of the owner's turn: hand out the base game's "next turn energy" / "next turn draw" powers.
    /// Block is still the end-of-turn value here, it is only cleared at the start of the next turn.
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner.Creature)) return;
        if (Owner.Creature.Block > MaxBlockForTrigger) return;

        Flash();

        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext,
            Owner.Creature,
            EnergyNextTurn,
            Owner.Creature,
            null);

        await PowerCmd.Apply<DrawCardsNextTurnPower>(
            choiceContext,
            Owner.Creature,
            CardsNextTurn,
            Owner.Creature,
            null);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new RelicLoc(
            "东尼圣遗物",
            "每回合结束时，若你的[gold]格挡[/gold]不超过[blue]15[/blue]，则在下回合获得[blue]2[/blue]点[gold]能量[/gold]并抽[blue]2[/blue]张牌。",
            "东尼打过来的那一拳，留下的东西。"),
        _ => new RelicLoc(
            "Dongni's Sacred Relic",
            "At the end of each turn, if you have [blue]15[/blue] or less [gold]Block[/gold], gain [blue]2[/blue] [gold]Energy[/gold] and draw [blue]2[/blue] cards next turn.",
            "What was left behind by the punch Dongni threw.")
    };
}
