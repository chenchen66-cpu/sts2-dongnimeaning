using BaseLib.Abstracts;
using DongniDefense.DongniDefenseCode.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace DongniDefense.DongniDefenseCode.Events;

/// <summary>
/// 东尼意思 / Dongni Meaning
/// Anthony Giovannetti himself shows up to warn you about playing small decks.
/// Appears in the unknown (?) rooms of acts 1-3.
/// </summary>
public sealed class DongniMeaning : CustomEventModel
{
    /// <summary>
    /// Shared event restricted to the first three acts, which is where the base game puts its
    /// unknown (?) rooms' shared pool. Using IsAllowed instead of Acts keeps model construction
    /// free of ModelDb lookups.
    /// </summary>
    public override bool IsAllowed(IRunState runState) => runState.CurrentActIndex is >= 0 and <= 2;

    public override string LocTable => "events";

    public override string? CustomInitialPortraitPath => $"{MainFile.ResPath}/images/events/dongni_meaning.png";

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Accept, HoverTipFactory.FromCardWithCardHoverTips<DongniStrike>()
            .Concat(HoverTipFactory.FromCardWithCardHoverTips<global::DongniDefense.DongniDefenseCode.Cards.DongniDefense>())
            .Concat(HoverTipFactory.FromEnchantment<Clone>())
            .Concat(HoverTipFactory.FromRelic<BingBong>())),
        Option(Refuse),
        Option(Punch, HoverTipFactory.FromCardWithCardHoverTips<DongniCurse>())
    ];

    /// <summary>① 接受提议：拿两张牌、给一张牌附魔克隆、再拿遗物宾梆。</summary>
    public async Task Accept()
    {
        Player player = Owner!;

        var added = new List<CardPileAddResult>
        {
            await AddCardToDeck(ModelDb.Card<DongniStrike>(), player),
            await AddCardToDeck(ModelDb.Card<global::DongniDefense.DongniDefenseCode.Cards.DongniDefense>(), player)
        };
        CardCmd.PreviewCardPileAdd(added, 2f);

        EnchantmentModel clone = ModelDb.Enchantment<Clone>();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        var selected = await CardSelectCmd.FromDeckForEnchantment(
            player,
            clone,
            1,
            card => card != null && clone.CanEnchant(card),
            prefs);

        foreach (CardModel card in selected)
        {
            CardCmd.Enchant<Clone>(card, 1m);
        }

        await RelicCmd.Obtain<BingBong>(player);

        SetEventFinished(PageDescription("ACCEPT"));
    }

    /// <summary>② 拒绝：失去 16 点生命，删除 3 张牌。</summary>
    public async Task Refuse()
    {
        Player player = Owner!;

        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            player.Creature,
            DynamicVars.HpLoss.IntValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 3);
        List<CardModel> removed = (await CardSelectCmd.FromDeckForRemoval(player, prefs)).ToList();
        if (removed.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(removed);
        }

        SetEventFinished(PageDescription("REFUSE"));
    }

    /// <summary>③ 一拳下去：获得【东尼的诅咒】。</summary>
    public async Task Punch()
    {
        await CardPileCmd.AddCurseToDeck<DongniCurse>(Owner!);
        SetEventFinished(PageDescription("PUNCH"));
    }

    private static async Task<CardPileAddResult> AddCardToDeck(CardModel canonical, Player player)
    {
        CardModel card = player.RunState.CreateCard(canonical, player);
        return await CardPileCmd.Add(card, PileType.Deck);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(16)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new EventLoc(
            "东尼意思",
            new EventPageLoc(
                "INITIAL",
                "一位自称安东尼·乔瓦内蒂的人找到了你，他听说你最近在玩小卡组，准备警告你不准再玩小卡组，否则找人弄你。\n你该怎么办？",
                new EventOptionLoc("ACCEPT", "接受提议", "获得【东尼打击】【东尼防御】，然后为一张卡牌附魔【克隆】，获得遗物【宾梆】。"),
                new EventOptionLoc("REFUSE", "拒绝", "失去16点生命，删除3张牌。"),
                new EventOptionLoc("PUNCH", "一拳下去", "获得【东尼的诅咒】。")),
            new EventPageLoc("ACCEPT", "东尼意思。"),
            new EventPageLoc("REFUSE", "你不顾安东尼，执意要删辣个，然后被安东尼找人弄了。"),
            new EventPageLoc("PUNCH", "你一拳打死了安东尼，但是总感觉它的阴魂不散...")),
        _ => new EventLoc(
            "Dongni Meaning",
            new EventPageLoc(
                "INITIAL",
                "A man calling himself Anthony Giovannetti has found you. He heard you have been playing small decks, and he is here to warn you: stop playing small decks, or he will send someone after you.\nWhat do you do?",
                new EventOptionLoc("ACCEPT", "Accept the proposal", "Gain 【Dongni Strike】【Dongni Defense】, enchant a card with 【Clone】, and gain the relic 【BinBang】."),
                new EventOptionLoc("REFUSE", "Refuse", "Lose 16 HP. Remove 3 cards."),
                new EventOptionLoc("PUNCH", "Throw a punch", "Gain 【Dongni's Curse】.")),
            new EventPageLoc("ACCEPT", "Dongni meaning."),
            new EventPageLoc("REFUSE", "You ignored Anthony and went ahead with the deletion. So Anthony sent someone after you."),
            new EventPageLoc("PUNCH", "You killed Anthony with a single punch, but somehow his ghost lingers..."))
    };
}
