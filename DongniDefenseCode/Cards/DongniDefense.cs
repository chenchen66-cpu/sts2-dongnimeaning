using BaseLib.Abstracts;
using BaseLib.Utils;
using DongniDefense.DongniDefenseCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DongniDefense.DongniDefenseCode.Cards;

/// <summary>
/// 东尼防御 / Dongni Defense
/// 0 cost colorless Power (Uncommon).
/// At the start of your turn, gain Block. You can no longer gain Block from cards.
/// Upgraded: gains Innate.
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public sealed class DongniDefense : DongniDefenseCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DongniDefensePower>(15m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public DongniDefense() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<DongniDefensePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["DongniDefensePower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }

    /// <summary>
    /// Localization defined in code as well as in this mod's localization/*/cards.json. The json tables are
    /// what translators should edit; this copy guarantees the card never shows a raw localization key.
    /// </summary>
    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc(
            "东尼防御",
            "在你的回合开始时，获得{DongniDefensePower:diff()}点[gold]格挡[/gold]。\n你无法从卡牌中获得[gold]格挡[/gold]。"),
        _ => new CardLoc(
            "Dongni Defense",
            "At the start of your turn, gain {DongniDefensePower:diff()} [gold]Block[/gold].\nYou can no longer gain [gold]Block[/gold] from cards.")
    };
}
