using System;
using System.Collections.Generic;
using CodexGame.Application.Distribution;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;
using CodexGame.SmokeTests.Cards;

namespace CodexGame.SmokeTests.Distribution
{
  internal static class PrivateCardDistributionTests
  {
    public static void Run(TestHarness tests)
    {
      CheckRoundRules(tests);
      CheckWinnerDistribution(tests);
      CheckWinnerFillByRound(tests);
      CheckWinnerlessAlternation(tests);
      CheckDeterminism(tests);
      CheckTimeoutRetention(tests);
      CheckSelectionSession(tests);
    }

    private static void CheckRoundRules(TestHarness tests)
    {
      tests.Check(
        PrivateCardDistributionRules.GetDirectSelectionCount(1) == 3
          && PrivateCardDistributionRules.GetWinnerRandomFillCount(1) == 0,
        "Combat round 1 distribution must use direct 3 and random fill 0.");
      tests.Check(
        PrivateCardDistributionRules.GetDirectSelectionCount(2) == 2
          && PrivateCardDistributionRules.GetWinnerRandomFillCount(2) == 1,
        "Combat round 2 distribution must use direct 2 and random fill 1.");
      tests.Check(
        PrivateCardDistributionRules.GetDirectSelectionCount(3) == 1
          && PrivateCardDistributionRules.GetWinnerRandomFillCount(3) == 2,
        "Combat round 3+ distribution must use direct 1 and random fill 2.");
      tests.Check(
        !PrivateCardDistributionRules.RequiresSelectionUi(2, 2)
          && PrivateCardDistributionRules.RequiresSelectionUi(2, 3),
        "Selection UI must be skipped for an exact candidate count and opened only when candidates exceed it.");
    }

    private static void CheckWinnerDistribution(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 4);
      var ai = Slice(cards, 4, 3);
      var other = Slice(cards, 7, 20);
      var selected = Array.AsReadOnly(new[] { player[3].Id, player[0].Id, player[2].Id });
      var result = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        1,
        selected,
        PrivateCardSelectionMode.Confirmed,
        DeterministicRandomFactory.Create(100, RandomChannel.CardDistribution));

      tests.Check(result.PlayerPrivateCards.Count == 3, "The Halli winner must finish with exactly three private cards.");
      tests.Check(result.AiPrivateCards.Count == 3, "The Halli loser must receive exactly three private cards.");
      tests.Check(
        result.PlayerPrivateCards[0].Id == player[0].Id
          && result.PlayerPrivateCards[1].Id == player[2].Id
          && result.PlayerPrivateCards[2].Id == player[3].Id,
        "Confirmed winner selections must be normalized to the winner-pool order.");
      CheckUniqueAndConserved(tests, result, player.Count + ai.Count + other.Count);
    }

    private static void CheckWinnerFillByRound(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 3);
      var ai = Slice(cards, 3, 4);
      var other = Slice(cards, 7, 20);
      var roundTwoSelected = Array.AsReadOnly(new[] { player[0].Id, player[1].Id });
      var roundTwo = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        2,
        roundTwoSelected,
        PrivateCardSelectionMode.Confirmed,
        new ZeroRandom());

      tests.Check(
        Contains(roundTwo.PlayerPrivateCards, player[0].Id)
          && Contains(roundTwo.PlayerPrivateCards, player[1].Id)
          && roundTwo.PlayerPrivateCards.Count == 3,
        "Round 2 must retain two direct selections and randomly fill the winner to three.");

      var roundThreeSelected = Array.AsReadOnly(new[] { ai[2].Id });
      var roundThree = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.Ai,
        3,
        roundThreeSelected,
        PrivateCardSelectionMode.Confirmed,
        new ZeroRandom());

      tests.Check(
        Contains(roundThree.AiPrivateCards, ai[2].Id) && roundThree.AiPrivateCards.Count == 3,
        "Round 3+ must retain one direct selection and randomly fill the winner to three.");
      CheckUniqueAndConserved(tests, roundThree, player.Count + ai.Count + other.Count);
    }

    private static void CheckWinnerlessAlternation(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 2);
      var ai = Slice(cards, 2, 2);
      var other = Slice(cards, 4, 12);
      var result = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.None,
        1,
        Array.AsReadOnly(Array.Empty<CardId>()),
        PrivateCardSelectionMode.Confirmed,
        new ZeroRandom());

      tests.Check(
        result.PlayerPrivateCards[0].Id == cards[0].Id
          && result.AiPrivateCards[0].Id == cards[1].Id
          && result.PlayerPrivateCards[1].Id == cards[2].Id
          && result.AiPrivateCards[1].Id == cards[3].Id,
        "A winner-less distribution must alternate player then AI while consuming the candidate pool.");
      CheckUniqueAndConserved(tests, result, player.Count + ai.Count + other.Count);
    }

    private static void CheckDeterminism(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 3);
      var ai = Slice(cards, 3, 3);
      var other = Slice(cards, 6, 20);
      var selected = Array.AsReadOnly(new[] { player[1].Id });
      var first = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        3,
        selected,
        PrivateCardSelectionMode.Confirmed,
        DeterministicRandomFactory.Create(20260807, RandomChannel.CardDistribution));
      var second = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        3,
        selected,
        PrivateCardSelectionMode.Confirmed,
        DeterministicRandomFactory.Create(20260807, RandomChannel.CardDistribution));

      tests.Check(Same(first.PlayerPrivateCards, second.PlayerPrivateCards), "The same seed must reproduce player distribution.");
      tests.Check(Same(first.AiPrivateCards, second.AiPrivateCards), "The same seed must reproduce AI distribution.");
      tests.Check(first.SecondPublicCard.Id == second.SecondPublicCard.Id, "The same seed must reproduce the second public card.");
    }

    private static void CheckTimeoutRetention(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 4);
      var ai = Slice(cards, 4, 3);
      var other = Slice(cards, 7, 20);
      var retained = player[2].Id;
      var result = PrivateCardDistributionResolver.Resolve(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        2,
        Array.AsReadOnly(new[] { retained }),
        PrivateCardSelectionMode.TimedOut,
        DeterministicRandomFactory.Create(5, RandomChannel.CardDistribution));

      tests.Check(
        Contains(result.PlayerPrivateCards, retained),
        "A selection timeout must retain every card selected before the deadline.");
      tests.Check(result.PlayerPrivateCards.Count == 3, "A timeout must randomly fill the winner to three cards.");
      CheckUniqueAndConserved(tests, result, player.Count + ai.Count + other.Count);
    }

    private static void CheckSelectionSession(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var exact = new PrivateCardSelectionSession();
      exact.Begin(
        Slice(cards, 0, 2),
        Slice(cards, 2, 3),
        Slice(cards, 5, 20),
        HalliStageWinner.Player,
        2,
        10,
        new GameTimestamp(0));
      var exactSnapshot = exact.GetSnapshot(new GameTimestamp(0));

      tests.Check(
        exactSnapshot.Phase == PrivateCardSelectionPhase.Completed && exactSnapshot.Result != null,
        "A winner candidate count equal to the direct count must skip UI and auto-complete.");

      var interactive = new PrivateCardSelectionSession();
      var player = Slice(cards, 0, 4);
      interactive.Begin(
        player,
        Slice(cards, 4, 3),
        Slice(cards, 7, 20),
        HalliStageWinner.Player,
        2,
        11,
        new GameTimestamp(100));
      var waiting = interactive.GetSnapshot(new GameTimestamp(100));

      tests.Check(
        waiting.Phase == PrivateCardSelectionPhase.AwaitingSelection
          && waiting.RemainingMicroseconds == GameRules.PrivateSelectionTimeoutMicroseconds,
        "Excess winner candidates must open a one-minute selection window.");
      tests.Check(interactive.Toggle(player[1].Id), "A winner candidate must be selectable.");
      tests.Check(!interactive.TryConfirm(), "Confirm must stay disabled below the direct-selection count.");
      tests.Check(interactive.Tick(new GameTimestamp(100 + GameRules.PrivateSelectionTimeoutMicroseconds)), "The exact deadline must complete by timeout.");
      var timedOut = interactive.GetSnapshot(new GameTimestamp(100 + GameRules.PrivateSelectionTimeoutMicroseconds));

      tests.Check(
        timedOut.Result != null && Contains(timedOut.Result.PlayerPrivateCards, player[1].Id),
        "The application selection session must retain a partial selection on timeout.");

      var noWinner = new PrivateCardSelectionSession();
      noWinner.Begin(
        Slice(cards, 0, 2),
        Slice(cards, 2, 2),
        Slice(cards, 4, 20),
        HalliStageWinner.None,
        1,
        12,
        new GameTimestamp(0));
      tests.Check(
        noWinner.GetSnapshot(new GameTimestamp(0)).Phase == PrivateCardSelectionPhase.Completed,
        "A winner-less stage must skip selection UI and distribute immediately.");
    }

    private static void CheckUniqueAndConserved(
      TestHarness tests,
      PrivateCardDistributionResult result,
      int inputCount)
    {
      var ids = new HashSet<CardId>();
      Add(ids, result.PlayerPrivateCards);
      Add(ids, result.AiPrivateCards);
      ids.Add(result.SecondPublicCard.Id);
      Add(ids, result.RemainingCandidates);

      tests.Check(ids.Count == inputCount, "Private distribution must conserve every input card exactly once.");
      tests.Check(
        !Contains(result.PlayerPrivateCards, result.SecondPublicCard.Id)
          && !Contains(result.AiPrivateCards, result.SecondPublicCard.Id),
        "The second public card must never duplicate a private card.");
    }

    private static IReadOnlyList<Card> Slice(IReadOnlyList<Card> cards, int start, int count)
    {
      var result = new Card[count];

      for (var index = 0; index < count; index++)
      {
        result[index] = cards[start + index];
      }

      return Array.AsReadOnly(result);
    }

    private static bool Contains(IReadOnlyList<Card> cards, CardId cardId)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == cardId)
        {
          return true;
        }
      }

      return false;
    }

    private static bool Same(IReadOnlyList<Card> left, IReadOnlyList<Card> right)
    {
      if (left.Count != right.Count)
      {
        return false;
      }

      for (var index = 0; index < left.Count; index++)
      {
        if (left[index].Id != right[index].Id)
        {
          return false;
        }
      }

      return true;
    }

    private static void Add(HashSet<CardId> ids, IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        ids.Add(cards[index].Id);
      }
    }

    private sealed class ZeroRandom : IRandomSource
    {
      public int NextInt(int exclusiveMax)
      {
        if (exclusiveMax <= 0)
        {
          throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
        }

        return 0;
      }
    }
  }
}
