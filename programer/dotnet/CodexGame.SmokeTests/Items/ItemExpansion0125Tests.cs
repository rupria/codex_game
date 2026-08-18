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
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Items
{
  internal static class ItemExpansion0125Tests
  {
    public static void Run(TestHarness tests)
    {
      CheckCatalogData(tests);
      CheckItemUseTimingData(tests);
      CheckWildInk(tests);
      CheckMercenary(tests);
      CheckBarrelAndHandTimeout(tests);
      CheckPredictionInsurance(tests);
      CheckHandConfirmationTimer(tests);
      CheckStageRestrictionIntegration(tests);
    }

    private static void CheckStageRestrictionIntegration(TestHarness tests)
    {
      var restriction = CreateActiveRestriction(1);
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.Barrel);
      inventory.TryAdd(GameItemId.PredictionInsurance);
      var session = new PokerItemSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        inventory,
        2201,
        restriction);
      tests.Check(
        session.UseBarrel() == PokerItemFailure.None
          && session.UsePredictionInsurance(true) == PokerItemFailure.StageUseLimitReached
          && inventory.Contains(GameItemId.PredictionInsurance)
          && restriction.GetSnapshot().UsedCount == 1,
        "A one-use stage restriction must count a successful new item once and reject the next without consumption.");

      var twoUseRestriction = CreateActiveRestriction(2);
      var pairInventory = new RunInventory();
      pairInventory.TryAdd(GameItemId.Reload);
      pairInventory.TryAdd(GameItemId.WildInk);
      var pairSession = new PokerItemSession();
      pairSession.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        pairInventory,
        2202,
        twoUseRestriction);
      var firstTarget = pairSession.GetSnapshot().PlayerPrivateCards[0].Id;
      pairSession.UseReload(firstTarget);
      var inkTarget = pairSession.GetSnapshot().PlayerPrivateCards[1];
      var inkSuit = inkTarget.EffectiveSuit == CardSuit.Spades
        ? CardSuit.Hearts
        : CardSuit.Spades;
      tests.Check(
        pairSession.UseWildInk(inkTarget.Id, inkSuit) == PokerItemFailure.None
          && twoUseRestriction.GetSnapshot().UsedCount == 2,
        "A two-use stage restriction must allow card exchange followed by Wild Ink and count both successes.");
    }

    private static StageItemRestrictionSession CreateActiveRestriction(int requiredLimit)
    {
      for (long seed = 0; seed < 10000; seed++)
      {
        var restriction = new StageItemRestrictionSession();
        restriction.ResetRun();
        restriction.EnterStage(1, seed);
        var stage = restriction.EnterStage(2, seed);
        if (stage.IsActive && stage.UseLimit == requiredLimit) return restriction;
      }
      throw new InvalidOperationException("Could not find a deterministic item restriction seed.");
    }

    private static void CheckCatalogData(TestHarness tests)
    {
      var valid = GameItemCatalog.All.Count == 8;
      for (var index = 0; index < GameItemCatalog.All.Count; index++)
      {
        var item = GameItemCatalog.All[index];
        valid &= !string.IsNullOrWhiteSpace(item.LocalizationNameKey)
          && !string.IsNullOrWhiteSpace(item.LocalizationDescriptionKey)
          && !string.IsNullOrWhiteSpace(item.IconKey)
          && !string.IsNullOrWhiteSpace(item.PresentationKey)
          && item.ShopWeight == 1;
      }
      valid &= GameItemCatalog.TryGet(GameItemId.PredictionInsurance, out var insurance)
        && insurance != null
        && insurance.ConfiguredMagnitude == GameRules.PredictionInsuranceCharges;
      tests.Check(
        valid,
        "All eight item definitions must expose localized name/description, art, effect, equal shop weight, and configured magnitude data.");
    }

    private static void CheckWildInk(TestHarness tests)
    {
      var original = C(CardSuit.Hearts, CardRank.Nine);
      var inked = original.WithEffectiveSuit(CardSuit.Spades);
      var before = PokerEvaluator.Evaluate(
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Spades, CardRank.King),
          C(CardSuit.Spades, CardRank.Queen),
          C(CardSuit.Spades, CardRank.Jack),
          original
        }),
        PokerRuleSet.Development);
      var after = PokerEvaluator.Evaluate(
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Spades, CardRank.King),
          C(CardSuit.Spades, CardRank.Queen),
          C(CardSuit.Spades, CardRank.Jack),
          inked
        }),
        PokerRuleSet.Development);
      tests.Check(
        inked.Id == original.Id
          && inked.Suit == original.Suit
          && inked.Rank == original.Rank
          && inked.EffectiveSuit == CardSuit.Spades
          && before.Category != PokerHandCategory.Flush
          && after.Category == PokerHandCategory.Flush,
        "Wild Ink must preserve CardId/rank while its effective suit participates in poker evaluation.");

      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.WildInk);
      inventory.TryAdd(GameItemId.Reload);
      var session = new PokerItemSession();
      session.Begin(C(CardSuit.Clubs, CardRank.Two), Distribution(), inventory, 901);
      var target = session.GetSnapshot().PlayerPrivateCards[0].Id;
      var secondTarget = session.GetSnapshot().PlayerPrivateCards[1].Id;
      tests.Check(
        session.UseWildInk(target, CardSuit.Clubs) == PokerItemFailure.None
          && session.UseReload(secondTarget) == PokerItemFailure.CardExchangeLocked
          && inventory.Contains(GameItemId.Reload),
        "A successful Wild Ink use must block later Reload/Bottom Deal/Mercenary without consuming them.");
    }

    private static void CheckMercenary(TestHarness tests)
    {
      var player = Array.AsReadOnly(new[]
      {
        C(CardSuit.Spades, CardRank.Ace),
        C(CardSuit.Hearts, CardRank.Two),
        C(CardSuit.Diamonds, CardRank.Three)
      });
      var ai = Array.AsReadOnly(new[]
      {
        C(CardSuit.Clubs, CardRank.King),
        C(CardSuit.Hearts, CardRank.Queen),
        C(CardSuit.Diamonds, CardRank.Jack)
      });
      var publicCards = Array.AsReadOnly(new[]
      {
        C(CardSuit.Spades, CardRank.Nine),
        C(CardSuit.Clubs, CardRank.Eight)
      });
      var pool = Array.AsReadOnly(new[]
      {
        C(CardSuit.Spades, CardRank.Four),
        C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Hearts, CardRank.Six)
      });
      var random = new CountingRandom();
      var succeeded = MercenaryExchangeResolver.TryResolve(
        player,
        ai,
        publicCards,
        pool,
        player[1].Id,
        random,
        out var result);
      tests.Check(
        succeeded
          && result.PlayerCards[1].Suit == CardSuit.Spades
          && result.AiCards[result.AiTargetIndex].Suit == CardSuit.Clubs
          && result.PlayerCards[1].Id != result.AiCards[result.AiTargetIndex].Id
          && result.RemainingCandidates.Count == pool.Count
          && random.CallCount == 1
          && AllUnique(result.PlayerCards, result.AiCards, publicCards, result.RemainingCandidates),
        "Mercenaries must atomically exchange distinct dominant-suit cards and preserve unique pool size.");

      var failedRandom = new CountingRandom();
      var failed = MercenaryExchangeResolver.TryResolve(
        player,
        ai,
        publicCards,
        Array.AsReadOnly(new[] { C(CardSuit.Spades, CardRank.Four) }),
        player[1].Id,
        failedRandom,
        out var failedResult);
      tests.Check(
        !failed
          && failedResult.Failure == MercenaryExchangeFailure.NoReplacementPair
          && failedRandom.CallCount == 0,
        "An unavailable Mercenary pair must fail without consuming its deterministic random channel.");
    }

    private static void CheckBarrelAndHandTimeout(TestHarness tests)
    {
      var normal = new PokerRoundSession();
      normal.Begin(
        C(CardSuit.Clubs, CardRank.Seven),
        LosingDistribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0),
        -1,
        true);
      var normalResult = normal.Resolve(PredictionChoice.PlayerLoses);
      tests.Check(
        normalResult.Comparison.Winner == PokerWinner.Ai
          && normalResult.Prediction.IsCorrect
          && normalResult.WasPlayerDamagePrevented
          && normalResult.Damage.Damage == 0
          && normalResult.Damage.After.Player == 3,
        "Barrel must prevent only HP damage while preserving the AI winner and prediction result.");

      var timeout = new PokerRoundSession();
      timeout.BeginHandConfirmationTimeout(
        C(CardSuit.Clubs, CardRank.Seven),
        WinningDistribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0));
      timeout.Tick(new GameTimestamp(GameRules.PokerResultAnnouncementMicroseconds));
      var timeoutResult = timeout.Result;
      tests.Check(
        timeoutResult != null
          && timeoutResult.WasHandConfirmationTimeout
          && !timeoutResult.WasPlayerDamagePrevented
          && !timeoutResult.PredictionEligibleForInsurance
          && timeoutResult.Comparison.Winner == PokerWinner.Ai
          && timeoutResult.Damage.After.Player == 2,
        "Hand-confirm timeout must force an AI win and HP loss while excluding Barrel and insurance.");
    }

    private static void CheckPredictionInsurance(TestHarness tests)
    {
      var streak = new PredictionStreak();
      tests.Check(
        streak.ActivateInsurance()
          && !streak.ActivateInsurance()
          && streak.InsuranceChargesRemaining == 2,
        "Prediction Insurance must activate once per stage with exactly two charges.");
      streak.Record(new PredictionResult(PredictionChoice.PlayerWins, PokerWinner.Player, true));
      var correctRecord = streak.LastRecord;
      streak.Record(new PredictionResult(PredictionChoice.PlayerWins, PokerWinner.Ai, false));
      var firstInsuredRecord = streak.LastRecord;
      streak.Record(new PredictionResult(PredictionChoice.Skipped, PokerWinner.Ai, false));
      var secondInsuredRecord = streak.LastRecord;
      var thirdFailureAdjusted = streak.Record(
        new PredictionResult(PredictionChoice.PlayerWins, PokerWinner.Ai, false));
      tests.Check(
        streak.ActualSuccessCount == 1
          && streak.InsuredSuccessCount == 2
          && streak.InsuranceChargesRemaining == 0
          && !thirdFailureAdjusted
          && streak.RewardSuccessCount == 3,
        "Correct predictions must preserve charges; two wrong/timeout predictions must consume them separately.");
      tests.Check(
        correctRecord != null
          && correctRecord.WasActualSuccess
          && !correctRecord.WasInsuredSuccess
          && correctRecord.InsuranceChargesBefore == 2
          && correctRecord.InsuranceChargesAfter == 2
          && firstInsuredRecord != null
          && firstInsuredRecord.WasInsuredSuccess
          && firstInsuredRecord.InsuranceChargesBefore == 2
          && firstInsuredRecord.InsuranceChargesAfter == 1
          && secondInsuredRecord != null
          && secondInsuredRecord.WasInsuredSuccess
          && secondInsuredRecord.InsuranceChargesBefore == 1
          && secondInsuredRecord.InsuranceChargesAfter == 0,
        "Prediction audit snapshots must separate actual/insured success and capture charge counts before/after.");
      for (var index = 0; index < 6; index++)
      {
        streak.Record(new PredictionResult(PredictionChoice.PlayerWins, PokerWinner.Player, true));
      }
      tests.Check(
        streak.ActualSuccessCount == 7 && streak.RewardSuccessCount == 5,
        "Actual and insured prediction totals must remain separate while reward calculation caps at five.");
      streak.ResetStage();
      tests.Check(
        streak.ActualSuccessCount == 0
          && streak.InsuredSuccessCount == 0
          && streak.InsuranceChargesRemaining == 0
          && streak.CanActivateInsurance,
        "Stage transition must clear prediction totals and unused insurance charges.");
    }

    private static void CheckItemUseTimingData(TestHarness tests)
    {
      var allCurrent = true;
      for (var index = 0; index < GameItemCatalog.All.Count; index++)
      {
        var definition = GameItemCatalog.All[index];
        allCurrent &= definition.UseTiming
            == GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction
          && GameItemUseTimingPolicy.IsUsable(
            definition,
            GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction)
          && !GameItemUseTimingPolicy.IsUsable(definition, GameItemUseTiming.DuringPrediction);
      }
      tests.Check(
        allCurrent && GameItemCatalog.All.Count == 8,
        "All eight items must share the data-driven post-selection/pre-prediction timing.");

      var movedDefinition = new GameItemDefinition(
        GameItemId.Reload,
        "QA-MOVED",
        "UI_ITEM_RELOAD",
        "UI_ITEM_RELOAD_DESC",
        "bar_shop.item.reload",
        1,
        GameItemTargetMode.PlayerCard,
        GameItemEffectType.ExchangeOne,
        GameItemUseTiming.DuringPrediction,
        "item.reload");
      tests.Check(
        !GameItemUseTimingPolicy.IsUsable(
          movedDefinition,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction)
          && GameItemUseTimingPolicy.IsUsable(
            movedDefinition,
            GameItemUseTiming.DuringPrediction),
        "Changing only UseTiming data must move the policy decision without item-specific code.");

      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.Reload);
      var session = new PokerItemSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        inventory,
        88,
        new GameTimestamp(0));
      var before = session.GetSnapshot(new GameTimestamp(0));
      session.Confirm();
      var failure = session.UseReload(before.PlayerPrivateCards[0].Id);
      var after = session.GetSnapshot(new GameTimestamp(1));
      tests.Check(
        failure == PokerItemFailure.WrongPhase
          && inventory.Contains(GameItemId.Reload)
          && before.PlayerPrivateCards[0].Id == after.PlayerPrivateCards[0].Id
          && before.CurrentUseTiming == after.CurrentUseTiming,
        "Wrong-timing requests must not consume an item or mutate cards/timing state.");
    }

    private static void CheckHandConfirmationTimer(TestHarness tests)
    {
      var inventory = new RunInventory();
      inventory.TryAdd(GameItemId.Barrel);
      var session = new PokerItemSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        inventory,
        777,
        new GameTimestamp(0));
      var opened = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        opened.Phase == PokerItemPhase.AwaitingActions
          && opened.HandConfirmationRemainingMicroseconds
            == GameRules.PokerHandConfirmationTimeoutMicroseconds,
        "The shared two-minute hand-confirm timer must start when the item phase opens.");
      var deadline = new GameTimestamp(GameRules.PokerHandConfirmationTimeoutMicroseconds);
      session.Tick(deadline);
      tests.Check(
        session.Phase == PokerItemPhase.Completed
          && session.HandConfirmationTimedOut
          && inventory.Contains(GameItemId.Barrel),
        "Hand-confirm timeout must complete the item window without consuming an unused item.");
    }

    private static PrivateCardDistributionResult Distribution()
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Diamonds, CardRank.King),
          C(CardSuit.Hearts, CardRank.Queen)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Clubs, CardRank.Jack),
          C(CardSuit.Spades, CardRank.Ten),
          C(CardSuit.Diamonds, CardRank.Nine)
        }),
        C(CardSuit.Hearts, CardRank.Eight),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Clubs, CardRank.Seven),
          C(CardSuit.Spades, CardRank.Six),
          C(CardSuit.Diamonds, CardRank.Five),
          C(CardSuit.Hearts, CardRank.Four)
        }));
    }

    private static PrivateCardDistributionResult LosingDistribution()
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Clubs, CardRank.Two),
          C(CardSuit.Diamonds, CardRank.Three),
          C(CardSuit.Hearts, CardRank.Four)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Diamonds, CardRank.Ace),
          C(CardSuit.Clubs, CardRank.King)
        }),
        C(CardSuit.Hearts, CardRank.Eight),
        Array.AsReadOnly(Array.Empty<Card>()));
    }

    private static PrivateCardDistributionResult WinningDistribution()
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Diamonds, CardRank.Ace),
          C(CardSuit.Clubs, CardRank.King)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Clubs, CardRank.Two),
          C(CardSuit.Diamonds, CardRank.Three),
          C(CardSuit.Hearts, CardRank.Four)
        }),
        C(CardSuit.Hearts, CardRank.Eight),
        Array.AsReadOnly(Array.Empty<Card>()));
    }

    private static bool AllUnique(params IReadOnlyList<Card>[] pools)
    {
      var ids = new HashSet<CardId>();
      for (var poolIndex = 0; poolIndex < pools.Length; poolIndex++)
      {
        for (var index = 0; index < pools[poolIndex].Count; index++)
        {
          if (!ids.Add(pools[poolIndex][index].Id)) return false;
        }
      }
      return true;
    }

    private static Card C(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }

    private sealed class CountingRandom : IRandomSource
    {
      public int CallCount { get; private set; }

      public int NextInt(int exclusiveMax)
      {
        CallCount++;
        return 0;
      }
    }
  }
}
