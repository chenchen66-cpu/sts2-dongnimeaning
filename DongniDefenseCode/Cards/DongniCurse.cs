using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DongniDefense.DongniDefenseCode.Cards;

/// <summary>
/// 东尼的诅咒 / Dongni's Curse
/// Unplayable curse. While this card is in your hand, every card draw and energy gain is prevented and
/// turned into the base game's "next turn draw" / "next turn energy" powers instead.
/// </summary>
[Pool(typeof(CurseCardPool))]
public sealed class DongniCurse : DongniDefenseCardBase
{
    /// <summary>Amount captured by the hook that stopped the effect, handed over in the matching follow-up hook.</summary>
    private decimal _deferredDraw;
    private decimal _deferredEnergy;

    public DongniCurse() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
    {
    }

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DrawCardsNextTurnPower>(),
        HoverTipFactory.FromPower<EnergyNextTurnPower>()
    ];

    /// <summary>True while this exact (combat) instance sits in its owner's hand.</summary>
    private bool IsInOwnerHand => Owner != null && Pile?.Type == PileType.Hand;

    /* ---------- card draw ---------- */

    /// <summary>Start of turn hand draw: stop it and queue the same amount for the next turn.</summary>
    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || !IsInOwnerHand || count <= 0m) return count;

        _deferredDraw += count;
        return 0m;
    }

    public override Task AfterModifyingHandDraw() => GrantNextTurnDraw();

    /// <summary>Any other draw (cards, relics, potions): stop it. The amount is captured by the draw patch.</summary>
    public override bool ShouldDraw(Player player, bool fromHandDraw)
    {
        if (player != Owner) return true;

        return !IsInOwnerHand;
    }

    public override Task AfterPreventingDraw() => GrantNextTurnDraw();

    /// <summary>Called by the draw patch before the draw is cancelled.</summary>
    internal void RecordBlockedDraw(decimal count) => _deferredDraw += count;

    private async Task GrantNextTurnDraw()
    {
        if (_deferredDraw <= 0m || Owner?.Creature == null) return;

        decimal amount = _deferredDraw;
        _deferredDraw = 0m;

        await PowerCmd.Apply<DrawCardsNextTurnPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            amount,
            Owner.Creature,
            this);
    }

    /* ---------- energy gain ---------- */

    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        if (player != Owner || !IsInOwnerHand || amount <= 0m) return amount;

        _deferredEnergy += amount;
        return 0m;
    }

    public override async Task AfterModifyingEnergyGain()
    {
        if (_deferredEnergy <= 0m || Owner?.Creature == null) return;

        decimal amount = _deferredEnergy;
        _deferredEnergy = 0m;

        await PowerCmd.Apply<EnergyNextTurnPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            amount,
            Owner.Creature,
            this);
    }

    /// <summary>Safety net: nothing should be left over once the matching follow-up hook has run.</summary>
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        _deferredDraw = 0m;
        _deferredEnergy = 0m;
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc(
            "东尼的诅咒",
            "当这张牌在你的[gold]手牌[/gold]中时，你抽牌和获得[gold]能量[/gold]的效果改为下回合生效。"),
        _ => new CardLoc(
            "Dongni's Curse",
            "While this card is in your [gold]Hand[/gold], your card draw and [gold]Energy[/gold] gain effects take effect next turn instead.")
    };
}
