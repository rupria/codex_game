#nullable enable
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

namespace CodexGame.Application.Development
{
  public static class ItemQaPresetRunner
  {
    public static ItemQaPresetResult Run(
      ItemQaPreset preset,
      long seed,
      int stageNumber = 2,
      string state = "PokerItems")
    {
      try
      {
        var observation = Observe(preset, seed);
        return new ItemQaPresetResult(
          preset,
          stageNumber,
          state,
          seed,
          observation.PlayerHand,
          observation.AiHand,
          observation.PublicCards,
          observation.Items,
          observation.Expected,
          observation.Actual,
          observation.Passed);
      }
      catch (Exception exception)
      {
        return new ItemQaPresetResult(
          preset,
          stageNumber,
          state,
          seed,
          "error",
          "error",
          "error",
          "error",
          "no exception",
          exception.GetType().Name + ":" + exception.Message,
          false);
      }
    }

    private static Observation Observe(ItemQaPreset preset, long seed)
    {
      switch (preset)
      {
        case ItemQaPreset.WildInkCompletesFlush:
          return WildInkCompletes(seed, false);
        case ItemQaPreset.WildInkCompletesStraightFlush:
          return WildInkCompletes(seed, true);
        case ItemQaPreset.WildInkRejectsJoker:
          return WildInkRejected(seed, true);
        case ItemQaPreset.WildInkRejectsSameSuit:
          return WildInkRejected(seed, false);
        case ItemQaPreset.WildInkLocksExchange:
          return WildInkLocksExchange(seed);
        case ItemQaPreset.BarrelBlocksNormalLoss:
          return Barrel(seed, BarrelCase.NormalLoss);
        case ItemQaPreset.BarrelDoesNotTriggerOnWin:
          return Barrel(seed, BarrelCase.PlayerWin);
        case ItemQaPreset.BarrelExcludedFromHandTimeout:
          return Barrel(seed, BarrelCase.HandTimeout);
        case ItemQaPreset.InsuranceCorrectPreservesCharge:
          return Insurance(InsuranceCase.Correct);
        case ItemQaPreset.InsuranceCorrectsTwoWrong:
          return Insurance(InsuranceCase.TwoWrong);
        case ItemQaPreset.InsuranceCorrectsTwoSkipped:
          return Insurance(InsuranceCase.TwoSkipped);
        case ItemQaPreset.InsuranceDoesNotCorrectThirdFailure:
          return Insurance(InsuranceCase.ThirdFailure);
        case ItemQaPreset.InsuranceExcludedFromHandTimeout:
          return Insurance(InsuranceCase.HandTimeout);
        case ItemQaPreset.MercenaryNormalExchange:
          return Mercenary(seed, MercenaryCase.Normal);
        case ItemQaPreset.MercenaryPreservesDominantSuit:
          return Mercenary(seed, MercenaryCase.DominantSuit);
        case ItemQaPreset.MercenaryNoReplacementPair:
          return Mercenary(seed, MercenaryCase.NoPair);
        case ItemQaPreset.MercenaryAiJokerHidden:
          return Mercenary(seed, MercenaryCase.AiJoker);
        case ItemQaPreset.MercenaryRepeatSeed:
          return Mercenary(seed, MercenaryCase.RepeatSeed);
        case ItemQaPreset.RestrictionZeroUses:
          return Restriction(seed, 0);
        case ItemQaPreset.RestrictionOneUse:
          return Restriction(seed, 1);
        case ItemQaPreset.RestrictionTwoUses:
          return Restriction(seed, 2);
        case ItemQaPreset.FourNewItemsCombined:
          return FourItems(seed, false);
        case ItemQaPreset.CardPoolIntegrity:
          return FourItems(seed, true);
        default:
          throw new ArgumentOutOfRangeException(nameof(preset));
      }
    }

    private static Observation WildInkCompletes(long seed, bool straightFlush)
    {
      var first = C(CardSuit.Hearts, straightFlush ? CardRank.Eight : CardRank.Jack);
      var distribution = D(
        straightFlush
          ? Cards(C(CardSuit.Spades, CardRank.Five), C(CardSuit.Spades, CardRank.Six), C(CardSuit.Hearts, CardRank.Seven))
          : Cards(C(CardSuit.Hearts, CardRank.Two), C(CardSuit.Hearts, CardRank.Five), C(CardSuit.Clubs, CardRank.Eight)),
        Cards(C(CardSuit.Clubs, CardRank.Three), C(CardSuit.Diamonds, CardRank.Four), C(CardSuit.Spades, CardRank.Queen)),
        straightFlush ? C(CardSuit.Spades, CardRank.Nine) : C(CardSuit.Hearts, CardRank.Ace),
        Cards(C(CardSuit.Diamonds, CardRank.Ten), C(CardSuit.Clubs, CardRank.King)));
      if (straightFlush) first = C(CardSuit.Spades, CardRank.Eight);
      var inventory = Inventory(GameItemId.WildInk);
      var session = Begin(first, distribution, inventory, seed);
      var target = distribution.PlayerPrivateCards[2];
      var result = session.UseWildInk(target.Id, straightFlush ? CardSuit.Spades : CardSuit.Hearts);
      var final = Finish(session);
      var category = PokerEvaluator.Evaluate(
        Join(final.PlayerPrivateCards, first, final.SecondPublicCard),
        PokerRuleSet.Development).Category;
      var expectedCategory = straightFlush ? PokerHandCategory.StraightFlush : PokerHandCategory.Flush;
      return CardsObservation(
        first,
        final,
        "WildInk",
        expectedCategory.ToString(),
        result + "/" + category,
        result == PokerItemFailure.None && category == expectedCategory,
        Total(distribution));
    }

    private static Observation WildInkRejected(long seed, bool joker)
    {
      var first = C(CardSuit.Hearts, CardRank.Ten);
      var target = joker
        ? new Card(PokerJokerKind.BrassSheriffRevolver)
        : C(CardSuit.Hearts, CardRank.Two);
      var distribution = D(
        Cards(target, C(CardSuit.Clubs, CardRank.Five), C(CardSuit.Diamonds, CardRank.Seven)),
        Cards(C(CardSuit.Spades, CardRank.Three), C(CardSuit.Spades, CardRank.Four), C(CardSuit.Spades, CardRank.Six)),
        C(CardSuit.Hearts, CardRank.Jack),
        Cards(C(CardSuit.Clubs, CardRank.Nine), C(CardSuit.Diamonds, CardRank.Queen)));
      var inventory = Inventory(GameItemId.WildInk);
      var session = Begin(first, distribution, inventory, seed);
      var failure = session.UseWildInk(
        target.Id,
        joker ? CardSuit.Spades : CardSuit.Hearts);
      var final = Finish(session);
      var expected = joker ? PokerItemFailure.InvalidTarget : PokerItemFailure.InvalidSuit;
      return CardsObservation(
        first,
        final,
        "WildInk(owned)",
        expected.ToString(),
        failure.ToString(),
        failure == expected && inventory.Contains(GameItemId.WildInk),
        Total(distribution));
    }

    private static Observation WildInkLocksExchange(long seed)
    {
      var scenario = MercenaryScenario(false, true);
      var inventory = Inventory(GameItemId.WildInk, GameItemId.Mercenary);
      var session = Begin(scenario.FirstPublic, scenario.Distribution, inventory, seed);
      var ink = session.UseWildInk(
        scenario.Distribution.PlayerPrivateCards[0].Id,
        CardSuit.Hearts);
      var exchange = session.UseMercenary(scenario.Distribution.PlayerPrivateCards[1].Id);
      var final = Finish(session);
      return CardsObservation(
        scenario.FirstPublic,
        final,
        "WildInk+Mercenary",
        "Mercenary=CardExchangeLocked",
        $"ink={ink}/mercenary={exchange}",
        ink == PokerItemFailure.None
          && exchange == PokerItemFailure.CardExchangeLocked
          && inventory.Contains(GameItemId.Mercenary),
        Total(scenario.Distribution));
    }

    private static Observation Barrel(long seed, BarrelCase barrelCase)
    {
      var playerWins = barrelCase == BarrelCase.PlayerWin;
      var scenario = BattleScenario(playerWins);
      var inventory = Inventory(GameItemId.Barrel);
      var itemSession = Begin(scenario.FirstPublic, scenario.Distribution, inventory, seed);
      var use = itemSession.UseBarrel();
      var itemResult = Finish(itemSession);
      var poker = new PokerRoundSession();
      if (barrelCase == BarrelCase.HandTimeout)
      {
        poker.BeginHandConfirmationTimeout(
          scenario.FirstPublic,
          itemResult,
          BattleHealth.Initial,
          PokerRuleSet.Development,
          new GameTimestamp(0));
      }
      else
      {
        poker.Begin(
          scenario.FirstPublic,
          itemResult,
          BattleHealth.Initial,
          PokerRuleSet.Development,
          new GameTimestamp(0),
          -1,
          true);
        poker.Resolve(playerWins ? PredictionChoice.PlayerWins : PredictionChoice.PlayerLoses);
      }
      var result = poker.Result!;
      var passed = barrelCase == BarrelCase.NormalLoss
        ? result.WasPlayerDamagePrevented && result.Damage.After.Player == 3
        : barrelCase == BarrelCase.PlayerWin
          ? !result.WasPlayerDamagePrevented && result.Damage.After.Ai == 2
          : result.WasHandConfirmationTimeout
            && !result.WasPlayerDamagePrevented
            && result.Damage.After.Player == 2;
      return CardsObservation(
        scenario.FirstPublic,
        itemResult,
        "Barrel(consumed)",
        barrelCase.ToString(),
        $"use={use}/shield={result.WasPlayerDamagePrevented}/hp={result.Damage.After.Player}:{result.Damage.After.Ai}",
        use == PokerItemFailure.None && !inventory.Contains(GameItemId.Barrel) && passed,
        Total(scenario.Distribution));
    }

    private static Observation Insurance(InsuranceCase insuranceCase)
    {
      var streak = new PredictionStreak();
      streak.ActivateInsurance();
      PredictionRecordAuditEntry? last = null;
      if (insuranceCase == InsuranceCase.Correct)
      {
        last = streak.RecordWithAudit(Prediction(true, false));
      }
      else if (insuranceCase == InsuranceCase.TwoWrong)
      {
        streak.RecordWithAudit(Prediction(false, false));
        last = streak.RecordWithAudit(Prediction(false, false));
      }
      else if (insuranceCase == InsuranceCase.TwoSkipped)
      {
        streak.RecordWithAudit(Prediction(false, true));
        last = streak.RecordWithAudit(Prediction(false, true));
      }
      else if (insuranceCase == InsuranceCase.ThirdFailure)
      {
        streak.RecordWithAudit(Prediction(false, false));
        streak.RecordWithAudit(Prediction(false, true));
        last = streak.RecordWithAudit(Prediction(false, false));
      }
      else
      {
        var scenario = BattleScenario(false);
        var poker = new PokerRoundSession();
        poker.BeginHandConfirmationTimeout(
          scenario.FirstPublic,
          scenario.Distribution,
          BattleHealth.Initial,
          PokerRuleSet.Development,
          new GameTimestamp(0));
        if (poker.Result!.PredictionEligibleForInsurance)
        {
          last = streak.RecordWithAudit(poker.Result.Prediction);
        }
      }

      var expectedCharges = insuranceCase == InsuranceCase.Correct
        || insuranceCase == InsuranceCase.HandTimeout ? 2 : 0;
      var expectedInsured = insuranceCase == InsuranceCase.TwoWrong
        || insuranceCase == InsuranceCase.TwoSkipped
        || insuranceCase == InsuranceCase.ThirdFailure ? 2 : 0;
      var thirdNotCorrected = insuranceCase != InsuranceCase.ThirdFailure
        || last != null && !last.CountedAsSuccess;
      var timeoutNotRecorded = insuranceCase != InsuranceCase.HandTimeout || last == null;
      var passed = streak.InsuranceChargesRemaining == expectedCharges
        && streak.InsuredSuccessCount == expectedInsured
        && thirdNotCorrected
        && timeoutNotRecorded;
      return new Observation(
        "N/A",
        "N/A",
        "N/A",
        "PredictionInsurance",
        insuranceCase.ToString(),
        $"actual={streak.ActualSuccessCount}/insured={streak.InsuredSuccessCount}/charges={streak.InsuranceChargesRemaining}/lastInsured={last?.WasInsuredSuccess == true}",
        passed);
    }

    private static Observation Mercenary(long seed, MercenaryCase mercenaryCase)
    {
      if (mercenaryCase == MercenaryCase.DominantSuit)
      {
        var scenario = MercenaryScenario(false, true);
        var random = DeterministicRandomFactory.Create(seed, RandomChannel.MercenaryExchange);
        var ok = MercenaryExchangeResolver.TryResolve(
          scenario.Distribution.PlayerPrivateCards,
          scenario.Distribution.AiPrivateCards,
          Cards(scenario.FirstPublic, scenario.Distribution.SecondPublicCard),
          scenario.Distribution.RemainingCandidates,
          scenario.Distribution.PlayerPrivateCards[0].Id,
          random,
          out var resolved);
        var final = D(
          resolved.PlayerCards,
          resolved.AiCards,
          scenario.Distribution.SecondPublicCard,
          resolved.RemainingCandidates);
        return CardsObservation(
          scenario.FirstPublic,
          final,
          "Mercenary",
          "off-suit target; dominant Hearts preserved",
          $"target={scenario.Distribution.PlayerPrivateCards[0].Suit}/dominant={resolved.PlayerDominantSuit}",
          ok
            && scenario.Distribution.PlayerPrivateCards[0].Suit != resolved.PlayerDominantSuit
            && resolved.PlayerDominantSuit == CardSuit.Hearts,
          Total(scenario.Distribution));
      }

      var noPair = mercenaryCase == MercenaryCase.NoPair;
      var aiJoker = mercenaryCase == MercenaryCase.AiJoker;
      var setup = MercenaryScenario(aiJoker, !noPair);
      var firstRun = RunMercenary(setup, seed);
      if (mercenaryCase == MercenaryCase.RepeatSeed)
      {
        var secondRun = RunMercenary(setup, seed);
        firstRun.Passed = firstRun.Passed
          && firstRun.PlayerHand == secondRun.PlayerHand
          && firstRun.AiHand == secondRun.AiHand;
        firstRun.Expected = "same seed -> same simultaneous exchange";
        firstRun.Actual += "/repeat=" + secondRun.PlayerHand + "/" + secondRun.AiHand;
      }
      else if (noPair)
      {
        firstRun.Passed = firstRun.Actual.Contains(PokerItemFailure.NoValidReplacementPair.ToString());
        firstRun.Expected = PokerItemFailure.NoValidReplacementPair.ToString();
      }
      else if (aiJoker)
      {
        firstRun.Passed = firstRun.Passed && firstRun.AiHand.Contains("Joker");
        firstRun.Expected = "exchange succeeds; AI Joker remains hidden/unchanged";
      }
      return firstRun;
    }

    private static Observation RunMercenary(Scenario scenario, long seed)
    {
      var inventory = Inventory(GameItemId.Mercenary);
      var session = Begin(scenario.FirstPublic, scenario.Distribution, inventory, seed);
      var failure = session.UseMercenary(scenario.Distribution.PlayerPrivateCards[0].Id);
      var final = Finish(session);
      var success = failure == PokerItemFailure.None;
      return CardsObservation(
        scenario.FirstPublic,
        final,
        success ? "Mercenary(consumed)" : "Mercenary(owned)",
        "simultaneous exchange; unique pool",
        failure.ToString(),
        success && PoolIsValid(scenario.FirstPublic, final, Total(scenario.Distribution)),
        Total(scenario.Distribution));
    }

    private static Observation Restriction(long seed, int limit)
    {
      var scenario = MercenaryScenario(false, true);
      var restriction = new StageItemRestrictionSession();
      restriction.ConfigureQaOverride(2, limit);
      var inventory = Inventory(
        GameItemId.Barrel,
        GameItemId.PredictionInsurance,
        GameItemId.WildInk);
      var session = Begin(scenario.FirstPublic, scenario.Distribution, inventory, seed, restriction);
      var first = session.UseBarrel();
      var second = session.UsePredictionInsurance(true);
      var third = session.UseWildInk(
        scenario.Distribution.PlayerPrivateCards[0].Id,
        CardSuit.Hearts);
      var expected = limit == 0
        ? first == PokerItemFailure.StageUseLimitReached
        : limit == 1
          ? first == PokerItemFailure.None && second == PokerItemFailure.StageUseLimitReached
          : first == PokerItemFailure.None
            && second == PokerItemFailure.None
            && third == PokerItemFailure.StageUseLimitReached;
      var snapshot = restriction.GetSnapshot();
      var final = Finish(session);
      return CardsObservation(
        scenario.FirstPublic,
        final,
        "Barrel+Insurance+WildInk",
        $"forced limit={limit}",
        $"{first}/{second}/{third};used={snapshot.UsedCount}",
        expected && snapshot.UsedCount == limit,
        Total(scenario.Distribution));
    }

    private static Observation FourItems(long seed, bool integrityOnly)
    {
      var scenario = MercenaryScenario(false, true);
      var inventory = Inventory(
        GameItemId.Mercenary,
        GameItemId.WildInk,
        GameItemId.Barrel,
        GameItemId.PredictionInsurance);
      var session = Begin(scenario.FirstPublic, scenario.Distribution, inventory, seed);
      var mercenary = session.UseMercenary(scenario.Distribution.PlayerPrivateCards[0].Id);
      var afterExchange = session.GetSnapshot(new GameTimestamp(0));
      var target = afterExchange.PlayerPrivateCards[0];
      var suit = (CardSuit)(((int)target.EffectiveSuit + 1) % 4);
      var ink = session.UseWildInk(target.Id, suit);
      var barrel = session.UseBarrel();
      var insurance = session.UsePredictionInsurance(true);
      var final = Finish(session);
      var poolValid = PoolIsValid(scenario.FirstPublic, final, Total(scenario.Distribution));
      var allUsed = mercenary == PokerItemFailure.None
        && ink == PokerItemFailure.None
        && barrel == PokerItemFailure.None
        && insurance == PokerItemFailure.None
        && inventory.Count == 0;
      return CardsObservation(
        scenario.FirstPublic,
        final,
        "Mercenary+WildInk+Barrel+Insurance",
        integrityOnly ? "pool total/unique preserved" : "all four items use runtime paths",
        $"{mercenary}/{ink}/{barrel}/{insurance};pool={poolValid}",
        integrityOnly ? poolValid : allUsed && poolValid,
        Total(scenario.Distribution));
    }

    private static PokerItemSession Begin(
      Card first,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long seed,
      StageItemRestrictionSession? restriction = null)
    {
      var session = new PokerItemSession();
      session.Begin(first, distribution, inventory, seed, new GameTimestamp(0), restriction);
      return session;
    }

    private static PrivateCardDistributionResult Finish(PokerItemSession session)
    {
      if (session.Phase == PokerItemPhase.AwaitingActions) session.Confirm();
      return session.GetResult();
    }

    private static Scenario MercenaryScenario(bool aiJoker, bool hasPair)
    {
      var first = C(CardSuit.Hearts, CardRank.Ten);
      return new Scenario(
        first,
        D(
          Cards(C(CardSuit.Clubs, CardRank.Two), C(CardSuit.Hearts, CardRank.Three), C(CardSuit.Hearts, CardRank.Four)),
          aiJoker
            ? Cards(new Card(PokerJokerKind.CrimsonCardsharp), C(CardSuit.Spades, CardRank.Three), C(CardSuit.Diamonds, CardRank.Four))
            : Cards(C(CardSuit.Diamonds, CardRank.Two), C(CardSuit.Spades, CardRank.Three), C(CardSuit.Spades, CardRank.Four)),
          C(CardSuit.Hearts, CardRank.Jack),
          hasPair
            ? Cards(C(CardSuit.Hearts, CardRank.Five), C(CardSuit.Hearts, CardRank.Six), C(CardSuit.Spades, CardRank.Five), C(CardSuit.Diamonds, CardRank.Six))
            : Cards(C(CardSuit.Hearts, CardRank.Five))));
    }

    private static Scenario BattleScenario(bool playerWins)
    {
      var first = C(CardSuit.Clubs, CardRank.Nine);
      return new Scenario(
        first,
        playerWins
          ? D(
            Cards(C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Ace), C(CardSuit.Spades, CardRank.Ace)),
            Cards(C(CardSuit.Clubs, CardRank.Three), C(CardSuit.Diamonds, CardRank.Five), C(CardSuit.Hearts, CardRank.Seven)),
            C(CardSuit.Spades, CardRank.Jack),
            Cards(C(CardSuit.Clubs, CardRank.Four), C(CardSuit.Hearts, CardRank.Five)))
          : D(
            Cards(C(CardSuit.Clubs, CardRank.Three), C(CardSuit.Diamonds, CardRank.Five), C(CardSuit.Hearts, CardRank.Seven)),
            Cards(C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Ace), C(CardSuit.Spades, CardRank.Ace)),
            C(CardSuit.Spades, CardRank.Jack),
            Cards(C(CardSuit.Clubs, CardRank.Four), C(CardSuit.Hearts, CardRank.Five))));
    }

    private static PredictionResult Prediction(bool correct, bool skipped)
    {
      return new PredictionResult(
        skipped ? PredictionChoice.Skipped : PredictionChoice.PlayerWins,
        correct ? PokerWinner.Player : PokerWinner.Ai,
        correct);
    }

    private static Observation CardsObservation(
      Card first,
      PrivateCardDistributionResult result,
      string items,
      string expected,
      string actual,
      bool passed,
      int expectedTotal)
    {
      var poolValid = PoolIsValid(first, result, expectedTotal);
      return new Observation(
        Format(result.PlayerPrivateCards),
        Format(result.AiPrivateCards),
        Format(Cards(first, result.SecondPublicCard)),
        items,
        expected,
        actual + $"/pool={poolValid}",
        passed && poolValid);
    }

    private static bool PoolIsValid(
      Card first,
      PrivateCardDistributionResult result,
      int expectedTotal)
    {
      var ids = new HashSet<CardId>();
      var count = 0;
      var groups = new[]
      {
        result.PlayerPrivateCards,
        result.AiPrivateCards,
        Cards(first, result.SecondPublicCard),
        result.RemainingCandidates
      };
      for (var group = 0; group < groups.Length; group++)
      {
        for (var index = 0; index < groups[group].Count; index++)
        {
          count++;
          if (!groups[group][index].IsValid || !ids.Add(groups[group][index].Id)) return false;
        }
      }
      return count == expectedTotal;
    }

    private static int Total(PrivateCardDistributionResult distribution)
    {
      return distribution.PlayerPrivateCards.Count
        + distribution.AiPrivateCards.Count
        + distribution.RemainingCandidates.Count
        + 2;
    }

    private static RunInventory Inventory(params GameItemId[] items)
    {
      var inventory = new RunInventory();
      for (var index = 0; index < items.Length; index++) inventory.TryAdd(items[index]);
      return inventory;
    }

    private static PrivateCardDistributionResult D(
      IReadOnlyList<Card> player,
      IReadOnlyList<Card> ai,
      Card secondPublic,
      IReadOnlyList<Card> remaining)
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        player,
        ai,
        secondPublic,
        remaining);
    }

    private static Card C(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }

    private static IReadOnlyList<Card> Cards(params Card[] cards)
    {
      return Array.AsReadOnly(cards);
    }

    private static IReadOnlyList<Card> Join(IReadOnlyList<Card> privateCards, params Card[] publicCards)
    {
      var joined = new List<Card>(privateCards.Count + publicCards.Length);
      joined.AddRange(privateCards);
      joined.AddRange(publicCards);
      return Array.AsReadOnly(joined.ToArray());
    }

    private static string Format(IReadOnlyList<Card> cards)
    {
      var values = new string[cards.Count];
      for (var index = 0; index < cards.Count; index++)
      {
        var card = cards[index];
        values[index] = card.IsJoker
          ? "Joker-" + card.JokerKind
          : $"{card.Rank}-{card.Suit}>{card.EffectiveSuit}#{card.Id.Value}";
      }
      return string.Join(",", values);
    }

    private sealed class Scenario
    {
      public Scenario(Card firstPublic, PrivateCardDistributionResult distribution)
      {
        FirstPublic = firstPublic;
        Distribution = distribution;
      }
      public Card FirstPublic { get; }
      public PrivateCardDistributionResult Distribution { get; }
    }

    private sealed class Observation
    {
      public Observation(
        string playerHand,
        string aiHand,
        string publicCards,
        string items,
        string expected,
        string actual,
        bool passed)
      {
        PlayerHand = playerHand;
        AiHand = aiHand;
        PublicCards = publicCards;
        Items = items;
        Expected = expected;
        Actual = actual;
        Passed = passed;
      }
      public string PlayerHand { get; }
      public string AiHand { get; }
      public string PublicCards { get; }
      public string Items { get; }
      public string Expected { get; set; }
      public string Actual { get; set; }
      public bool Passed { get; set; }
    }

    private enum BarrelCase { NormalLoss, PlayerWin, HandTimeout }
    private enum InsuranceCase { Correct, TwoWrong, TwoSkipped, ThirdFailure, HandTimeout }
    private enum MercenaryCase { Normal, DominantSuit, NoPair, AiJoker, RepeatSeed }
  }
}
