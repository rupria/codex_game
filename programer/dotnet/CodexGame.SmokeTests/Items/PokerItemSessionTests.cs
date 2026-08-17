using System;
using System.Collections.Generic;
using CodexGame.Application.Items;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Items;
using CodexGame.Core.Poker;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Items
{
  internal static class PokerItemSessionTests
  {
    public static void Run(TestHarness tests)
    {
      CheckReloadAndBottomDeal(tests);
      CheckDiscardReentersCandidatePool(tests);
      CheckFailedBottomDealIsAtomic(tests);
      CheckHypeManReveal(tests);
      CheckHealthRecoveryGuard(tests);
      CheckStageUseLimit(tests);
      CheckSecondPublicRevealOrder(tests);
    }

    private static void CheckReloadAndBottomDeal(TestHarness tests)
    {
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.Reload);
      inventory.TryAdd(GameItemId.BottomDeal);
      var session = new PokerItemSession();
      session.Begin(C(CardSuit.Spades, CardRank.Ace), Distribution(), inventory, 501);
      var before = session.GetSnapshot();
      tests.Check(
        before.PublicCards.Count == 2,
        "The item window must show both locked public cards before prediction.");
      var reloadTarget = before.PlayerPrivateCards[0].Id;
      tests.Check(
        session.UseReload(reloadTarget) == PokerItemFailure.None
          && !inventory.Contains(GameItemId.Reload)
          && session.GetSnapshot().PlayerPrivateCards.Count == 3,
        "Reload must exchange one selected private card and consume IT-01.");

      var bottomTarget = session.GetSnapshot().PlayerPrivateCards[1].Id;
      tests.Check(
        session.BeginBottomDeal(bottomTarget) == PokerItemFailure.None
          && session.GetSnapshot().BottomDealCandidates.Count == 2,
        "Bottom Deal must offer two unique replacement candidates before committing.");
      var choice = session.GetSnapshot().BottomDealCandidates[0].Id;
      tests.Check(
        session.ChooseBottomDeal(choice) == PokerItemFailure.None
          && !inventory.Contains(GameItemId.BottomDeal)
          && session.Phase == PokerItemPhase.Completed,
        "Bottom Deal must consume IT-02 only after a valid replacement choice.");
      var result = session.GetResult();
      tests.Check(
        result.PlayerPrivateCards.Count == 3
          && Unique(result.PlayerPrivateCards)
          && Unique(result.RemainingCandidates),
        "Item replacement must preserve three unique private cards and a unique candidate pool.");
    }

    private static void CheckDiscardReentersCandidatePool(TestHarness tests)
    {
      var reloadCanRedrawDiscard = false;
      var bottomDealCanOfferDiscard = false;
      for (var seed = 0; seed < 256 && (!reloadCanRedrawDiscard || !bottomDealCanOfferDiscard); seed++)
      {
        var reloadInventory = new RunInventory();
        reloadInventory.TryAdd(GameItemId.Reload);
        var reload = new PokerItemSession();
        reload.Begin(C(CardSuit.Spades, CardRank.Ace), Distribution(), reloadInventory, seed);
        var reloadTarget = reload.GetSnapshot().PlayerPrivateCards[0];
        reload.UseReload(reloadTarget.Id);
        reloadCanRedrawDiscard |= reload.GetSnapshot().PlayerPrivateCards[0].Id == reloadTarget.Id;

        var bottomInventory = new RunInventory();
        bottomInventory.TryAdd(GameItemId.BottomDeal);
        var bottom = new PokerItemSession();
        bottom.Begin(C(CardSuit.Spades, CardRank.Ace), Distribution(), bottomInventory, seed);
        var bottomTarget = bottom.GetSnapshot().PlayerPrivateCards[0];
        bottom.BeginBottomDeal(bottomTarget.Id);
        var candidates = bottom.GetSnapshot().BottomDealCandidates;
        for (var index = 0; index < candidates.Count; index++)
        {
          bottomDealCanOfferDiscard |= candidates[index].Id == bottomTarget.Id;
        }
      }
      tests.Check(
        reloadCanRedrawDiscard && bottomDealCanOfferDiscard,
        "A discarded card must re-enter the same Reload or Bottom Deal random candidate pool.");
    }

    private static void CheckFailedBottomDealIsAtomic(TestHarness tests)
    {
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.BottomDeal);
      var distribution = new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Clubs, CardRank.Two),
          C(CardSuit.Hearts, CardRank.Three),
          C(CardSuit.Diamonds, CardRank.Four)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Five),
          C(CardSuit.Clubs, CardRank.Six),
          C(CardSuit.Hearts, CardRank.Seven)
        }),
        C(CardSuit.Diamonds, CardRank.Eight),
        Array.AsReadOnly(Array.Empty<Card>()));
      var session = new PokerItemSession();
      session.Begin(C(CardSuit.Spades, CardRank.Ace), distribution, inventory, 1);
      var target = session.GetSnapshot().PlayerPrivateCards[0];
      tests.Check(
        session.BeginBottomDeal(target.Id) == PokerItemFailure.CandidatePoolExhausted
          && inventory.Contains(GameItemId.BottomDeal)
          && session.GetSnapshot().PlayerPrivateCards[0].Id == target.Id
          && session.Phase == PokerItemPhase.AwaitingActions,
        "A candidate-pool failure must leave both the item and hand unchanged.");
    }

    private static void CheckHypeManReveal(TestHarness tests)
    {
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.HypeMan);
      var session = new PokerItemSession();
      var distribution = Distribution();
      session.Begin(C(CardSuit.Spades, CardRank.Ace), distribution, inventory, 777);
      tests.Check(
        session.UseHypeMan() == PokerItemFailure.None
          && session.GetSnapshot().VisibleAiPrivateCards.Count == 1,
        "Hype Man must reveal exactly one deterministic AI private card for this poker round.");

      var poker = new PokerRoundSession();
      poker.Begin(
        C(CardSuit.Spades, CardRank.Ace),
        session.GetResult(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0),
        session.VisibleAiCardIndex);
      tests.Check(
        poker.GetSnapshot(new GameTimestamp(0)).VisibleAiPrivateCards.Count == 1,
        "The Hype Man reveal must remain visible before the prediction result opens the full AI hand.");

      var jokerInventory = new RunInventory();
      jokerInventory.TryAdd(GameItemId.HypeMan);
      var jokerSession = new PokerItemSession();
      var jokerDistribution = new PrivateCardDistributionResult(
        HalliStageWinner.Ai,
        1,
        distribution.PlayerPrivateCards,
        Array.AsReadOnly(new[]
        {
          new Card(PokerJokerKind.CrimsonCardsharp),
          distribution.AiPrivateCards[1],
          distribution.AiPrivateCards[2]
        }),
        distribution.SecondPublicCard,
        distribution.RemainingCandidates);
      jokerSession.Begin(C(CardSuit.Spades, CardRank.Ace), jokerDistribution, jokerInventory, 777);
      jokerSession.UseHypeMan();
      tests.Check(
        jokerSession.GetSnapshot().VisibleAiPrivateCards.Count == 1
          && !jokerSession.GetSnapshot().VisibleAiPrivateCards[0].IsJoker,
        "Hype Man must keep an AI Joker concealed and reveal one standard AI card instead.");
    }

    private static void CheckHealthRecoveryGuard(TestHarness tests)
    {
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.HealthRecovery);
      var session = new PokerItemSession();
      session.Begin(C(CardSuit.Spades, CardRank.Ace), Distribution(), inventory, 900);
      tests.Check(
        session.UseHealthRecovery(false) == PokerItemFailure.HealthAlreadyFull
          && inventory.Contains(GameItemId.HealthRecovery),
        "Health recovery must remain in inventory when HP is already full.");
      tests.Check(
        session.UseHealthRecovery(true) == PokerItemFailure.None
          && !inventory.Contains(GameItemId.HealthRecovery),
        "Health recovery must consume only after a valid heal.");
    }

    private static void CheckStageUseLimit(TestHarness tests)
    {
      StageItemRestrictionSession restriction = null!;
      for (long seed = 0; seed < 10000; seed++)
      {
        var candidate = new StageItemRestrictionSession();
        candidate.ResetRun();
        candidate.EnterStage(1, seed);
        var snapshot = candidate.EnterStage(2, seed);
        if (snapshot.IsActive && snapshot.UseLimit == 1)
        {
          restriction = candidate;
          break;
        }
      }
      if (restriction == null)
      {
        tests.Check(false, "A deterministic one-use stage restriction seed must exist.");
        return;
      }

      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.Reload);
      inventory.TryAdd(GameItemId.HypeMan);
      var session = new PokerItemSession();
      session.Begin(
        C(CardSuit.Spades, CardRank.Ace),
        Distribution(),
        inventory,
        1204,
        restriction);
      var target = session.GetSnapshot().PlayerPrivateCards[0].Id;
      var first = session.UseReload(target);
      var second = session.UseHypeMan();
      tests.Check(
        first == PokerItemFailure.None
          && second == PokerItemFailure.StageUseLimitReached
          && inventory.Contains(GameItemId.HypeMan)
          && session.GetSnapshot().StageRestriction?.IsExhausted == true,
        "Poker item actions must consume and enforce the shared stage-use allowance atomically.");
    }

    private static void CheckSecondPublicRevealOrder(TestHarness tests)
    {
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.Reload);
      var session = new PokerItemSession();
      var distribution = Distribution();
      session.Begin(
        C(CardSuit.Spades, CardRank.Ace),
        distribution,
        inventory,
        1205,
        new GameTimestamp(50));
      var during = session.GetSnapshot(new GameTimestamp(50));
      tests.Check(
        during.Phase == PokerItemPhase.RevealingSecondPublic
          && during.PublicCards.Count == 1
          && during.RevealingSecondPublicCard.HasValue
          && during.RevealingSecondPublicCard.Value.Id == distribution.SecondPublicCard.Id,
        "The second public card must reveal only after private distribution and item-phase entry.");
      tests.Check(
        !session.Tick(new GameTimestamp(50 + GameRules.SecondPublicCardRevealMicroseconds - 1))
          && session.Tick(new GameTimestamp(50 + GameRules.SecondPublicCardRevealMicroseconds))
          && session.GetSnapshot().PublicCards.Count == 2
          && session.Phase == PokerItemPhase.AwaitingActions,
        "The second public-card flip must lock item input until its exact reveal boundary.");
    }

    private static PrivateCardDistributionResult Distribution()
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Clubs, CardRank.Two),
          C(CardSuit.Hearts, CardRank.Three),
          C(CardSuit.Diamonds, CardRank.Four)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Five),
          C(CardSuit.Clubs, CardRank.Six),
          C(CardSuit.Hearts, CardRank.Seven)
        }),
        C(CardSuit.Diamonds, CardRank.Eight),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Nine),
          C(CardSuit.Clubs, CardRank.Ten),
          C(CardSuit.Hearts, CardRank.Jack),
          C(CardSuit.Diamonds, CardRank.Queen)
        }));
    }

    private static bool Unique(IReadOnlyList<Card> cards)
    {
      var ids = new HashSet<CardId>();
      for (var index = 0; index < cards.Count; index++)
      {
        if (!ids.Add(cards[index].Id)) return false;
      }
      return true;
    }

    private static Card C(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }
  }
}
