using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace DongniDefense.DongniDefenseCode.Cards;

/// <summary>
/// 东尼打击 / Dongni Strike
/// 1 cost colorless Attack (Uncommon). Deal 6 damage, plus 1 more for every card in your deck.
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public sealed class DongniStrike : DongniDefenseCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(6m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            // Counts the player's out-of-combat deck (player.Deck). Combat draws/plays are clones of
            // those cards, so this number does not drop as cards are drawn, played or exhausted.
            .WithMultiplier((card, _) => (decimal)(card.Owner?.Deck.Cards.Count ?? 0))
    ];

    public DongniStrike() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await CommonActions.CardAttack(this, cardPlay, vfx: "vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc(
            "东尼打击",
            "造成{CalculatedDamage:diff()}点伤害。\n你的牌组中每有一张牌，伤害增加{ExtraDamage:diff()}。"),
        _ => new CardLoc(
            "Dongni Strike",
            "Deal {CalculatedDamage:diff()} damage.\nDeals {ExtraDamage:diff()} additional damage for each card in your deck.")
    };
}
