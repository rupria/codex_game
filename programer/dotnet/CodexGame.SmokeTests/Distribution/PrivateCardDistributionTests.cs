using System;
using System.Collections.Generic;
using CodexGame.Application.Distribution;
using CodexGame.Core.Battle;
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
      CheckOverflowRule(tests);
      CheckBothActorsRetainAcquiredCards(tests);
      CheckBothActorsSelectOverflow(tests);
      CheckSeededFillAndTimeout(tests);
      CheckSelectionSession(tests);
      CheckPairAssistHealthRule(tests);
      CheckPairAssistRecommendation(tests);
      CheckPairAssistFill(tests);
    }

    private static void CheckOverflowRule(TestHarness tests)
    {
      tests.Check(
        PrivateCardDistributionRules.GetDirectSelectionCount(1) == 3
          && PrivateCardDistributionRules.GetDirectSelectionCount(2) == 2
          && PrivateCardDistributionRules.GetDirectSelectionCount(3) == 2
          && PrivateCardDistributionRules.RequiresSelectionUi(3, 3)
          && !PrivateCardDistributionRules.RequiresSelectionUi(3, 2),
        "Round one must select three cards; later rounds must select two and receive one random fill.");
    }

    private static void CheckBothActorsRetainAcquiredCards(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 2);
      var ai = Slice(cards, 2, 2);
      var other = Slice(cards, 4, 20);
      var result = PrivateCardDistributionResolver.ResolveBoth(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        1,
        EmptyIds(),
        EmptyIds(),
        PrivateCardSelectionMode.Confirmed,
        new ZeroRandom());

      tests.Check(
        ContainsAll(result.PlayerPrivateCards, player)
          && ContainsAll(result.AiPrivateCards, ai)
          && result.PlayerPrivateCards.Count == 3
          && result.AiPrivateCards.Count == 3,
        "Both Halli actors must retain their acquired cards and receive random fill only below three.");
      CheckUniqueAndConserved(tests, result, player.Count + ai.Count + other.Count);
    }

    private static void CheckBothActorsSelectOverflow(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 5);
      var ai = Slice(cards, 5, 4);
      var other = Slice(cards, 9, 20);
      var playerIds = Ids(player[0], player[4]);
      var aiIds = Ids(ai[1], ai[3]);
      var result = PrivateCardDistributionResolver.ResolveBoth(
        player,
        ai,
        other,
        HalliStageWinner.Player,
        2,
        playerIds,
        aiIds,
        PrivateCardSelectionMode.Confirmed,
        new ZeroRandom());

      tests.Check(
        ContainsAllIds(result.PlayerPrivateCards, playerIds)
          && ContainsAllIds(result.AiPrivateCards, aiIds),
        "Round-two direct selections must preserve exactly two cards before random fill.");
      CheckUniqueAndConserved(tests, result, player.Count + ai.Count + other.Count);
    }

    private static void CheckSeededFillAndTimeout(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Slice(cards, 0, 5);
      var ai = Slice(cards, 5, 2);
      var other = Slice(cards, 7, 20);
      var retained = player[3].Id;
      var first = PrivateCardDistributionResolver.ResolveBoth(
        player,
        ai,
        other,
        HalliStageWinner.Ai,
        3,
        Array.AsReadOnly(new[] { retained }),
        EmptyIds(),
        PrivateCardSelectionMode.TimedOut,
        DeterministicRandomFactory.Create(44, RandomChannel.CardDistribution));
      var second = PrivateCardDistributionResolver.ResolveBoth(
        player,
        ai,
        other,
        HalliStageWinner.Ai,
        3,
        Array.AsReadOnly(new[] { retained }),
        EmptyIds(),
        PrivateCardSelectionMode.TimedOut,
        DeterministicRandomFactory.Create(44, RandomChannel.CardDistribution));

      tests.Check(
        Contains(first.PlayerPrivateCards, retained)
          && Same(first.PlayerPrivateCards, second.PlayerPrivateCards)
          && Same(first.AiPrivateCards, second.AiPrivateCards),
        "Timeout fill must retain partial choices and remain deterministic for the combat seed.");
    }

    private static void CheckSelectionSession(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var session = new PrivateCardSelectionSession();
      var player = Slice(cards, 0, 5);
      var ai = Slice(cards, 5, 4);
      session.Begin(
        player,
        ai,
        Slice(cards, 10, 20),
        HalliStageWinner.Player,
        2,
        99,
        cards[9],
        new GameTimestamp(0));
      var waiting = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        waiting.Phase == PrivateCardSelectionPhase.AwaitingSelection
          && waiting.Winner == HalliStageWinner.Player
          && waiting.RequiredSelectionCount == 2
          && waiting.FirstPublicCard?.Id == cards[9].Id
          && waiting.SecondPublicCard.HasValue,
        "Round-two winner selection must expose both public cards before asking for two private cards.");
      session.Toggle(player[0].Id);
      session.Toggle(player[1].Id);
      tests.Check(session.TryConfirm(), "Exactly two round-two winner cards must confirm.");
      var completed = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        completed.Result != null
          && completed.Result.Winner == HalliStageWinner.Player
          && completed.Result.PlayerPrivateCards.Count == 3
          && completed.Result.AiPrivateCards.Count == 3
          && completed.Result.SecondPublicCard.Id == waiting.SecondPublicCard?.Id
          && ContainsAllIds(completed.Result.PlayerPrivateCards, Ids(player[0], player[1])),
        "Winner choices and the pre-revealed second public card must stay fixed through final distribution.");

      var restartRejected = false;
      try
      {
        session.Begin(player, ai, Slice(cards, 10, 20), HalliStageWinner.Player, 2, 99, cards[9], new GameTimestamp(0));
      }
      catch (InvalidOperationException)
      {
        restartRejected = true;
      }
      tests.Check(restartRejected, "Reopening a selection session must not reroll Joker or public-card channels.");

      var fillOnly = new PrivateCardSelectionSession();
      fillOnly.Begin(
        Slice(cards, 0, 0),
        Slice(cards, 0, 0),
        Slice(cards, 1, 20),
        HalliStageWinner.None,
        1,
        100,
        cards[0],
        new GameTimestamp(0));
      tests.Check(
        fillOnly.GetSnapshot(new GameTimestamp(0)).Result?.PlayerPrivateCards.Count == 3
          && fillOnly.GetSnapshot(new GameTimestamp(0)).SecondPublicCard.HasValue,
        "No-winner distribution must reveal the second public card first, then fill both hands randomly.");
    }

    private static void CheckPairAssistHealthRule(TestHarness tests)
    {
      tests.Check(
        PrivateCardDistributionRules.IsPairAssistEnabled(new BattleHealth(3, 3))
          && PrivateCardDistributionRules.IsPairAssistEnabled(new BattleHealth(3, 2))
          && !PrivateCardDistributionRules.IsPairAssistEnabled(new BattleHealth(2, 2))
          && !PrivateCardDistributionRules.IsPairAssistEnabled(new BattleHealth(0, 3)),
        "Pair assistance must use combined remaining health and stop at four or battle end.");
    }

    private static void CheckPairAssistRecommendation(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Array.AsReadOnly(new[]
      {
        Find(cards, CardSuit.Clubs, CardRank.Two),
        Find(cards, CardSuit.Hearts, CardRank.Two),
        Find(cards, CardSuit.Diamonds, CardRank.Five),
        Find(cards, CardSuit.Spades, CardRank.Nine),
        Find(cards, CardSuit.Clubs, CardRank.King)
      });
      var session = new PrivateCardSelectionSession();
      session.Begin(
        player,
        Array.AsReadOnly(Array.Empty<Card>()),
        Excluding(cards, player, 20),
        HalliStageWinner.Player,
        1,
        20260909,
        new GameTimestamp(0),
        true);
      var snapshot = session.GetSnapshot(new GameTimestamp(0));

      tests.Check(
        snapshot.SelectedCards.Count == 3
          && CountRank(snapshot.SelectedCards, CardRank.Two) == 2,
        "Early-health overflow selection must recommend an available rank pair.");
    }

    private static void CheckPairAssistFill(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var player = Array.AsReadOnly(new[]
      {
        Find(cards, CardSuit.Clubs, CardRank.Two),
        Find(cards, CardSuit.Clubs, CardRank.Five)
      });
      var other = Array.AsReadOnly(new[]
      {
        Find(cards, CardSuit.Hearts, CardRank.Two),
        Find(cards, CardSuit.Diamonds, CardRank.Seven),
        Find(cards, CardSuit.Spades, CardRank.Nine),
        Find(cards, CardSuit.Hearts, CardRank.Jack),
        Find(cards, CardSuit.Diamonds, CardRank.Queen),
        Find(cards, CardSuit.Spades, CardRank.King),
        Find(cards, CardSuit.Hearts, CardRank.Ace)
      });
      var result = PrivateCardDistributionResolver.ResolveBoth(
        player,
        Array.AsReadOnly(Array.Empty<Card>()),
        other,
        HalliStageWinner.Player,
        2,
        EmptyIds(),
        EmptyIds(),
        PrivateCardSelectionMode.Confirmed,
        new ZeroRandom(),
        true);

      tests.Check(
        CountRank(result.PlayerPrivateCards, CardRank.Two) == 2,
        "Early-health random fill must prefer an existing rank when pair assistance triggers.");
    }

    private static IReadOnlyList<Card> Slice(IReadOnlyList<Card> cards, int start, int count)
    {
      var result = new Card[count];
      for (var index = 0; index < count; index++) result[index] = cards[start + index];
      return Array.AsReadOnly(result);
    }

    private static Card Find(
      IReadOnlyList<Card> cards,
      CardSuit suit,
      CardRank rank)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Suit == suit && cards[index].Rank == rank) return cards[index];
      }

      throw new InvalidOperationException("Requested test card was not found.");
    }

    private static IReadOnlyList<Card> Excluding(
      IReadOnlyList<Card> cards,
      IReadOnlyList<Card> excluded,
      int count)
    {
      var result = new List<Card>(count);
      for (var index = 0; index < cards.Count && result.Count < count; index++)
      {
        if (!Contains(excluded, cards[index].Id)) result.Add(cards[index]);
      }

      return Array.AsReadOnly(result.ToArray());
    }

    private static int CountRank(IReadOnlyList<Card> cards, CardRank rank)
    {
      var count = 0;
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Rank == rank) count++;
      }

      return count;
    }

    private static IReadOnlyList<CardId> EmptyIds() => Array.AsReadOnly(Array.Empty<CardId>());
    private static IReadOnlyList<CardId> Ids(params Card[] cards)
    {
      var ids = new CardId[cards.Length];
      for (var index = 0; index < cards.Length; index++) ids[index] = cards[index].Id;
      return Array.AsReadOnly(ids);
    }

    private static bool Contains(IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++) if (cards[index].Id == id) return true;
      return false;
    }

    private static bool ContainsAll(IReadOnlyList<Card> cards, IReadOnlyList<Card> expected)
    {
      for (var index = 0; index < expected.Count; index++) if (!Contains(cards, expected[index].Id)) return false;
      return true;
    }

    private static bool ContainsAllIds(IReadOnlyList<Card> cards, IReadOnlyList<CardId> expected)
    {
      for (var index = 0; index < expected.Count; index++) if (!Contains(cards, expected[index])) return false;
      return true;
    }

    private static bool Same(IReadOnlyList<Card> left, IReadOnlyList<Card> right)
    {
      if (left.Count != right.Count) return false;
      for (var index = 0; index < left.Count; index++) if (left[index].Id != right[index].Id) return false;
      return true;
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
      tests.Check(ids.Count == inputCount, "Private distribution must conserve each card identity exactly once.");
    }

    private static void Add(HashSet<CardId> ids, IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++) ids.Add(cards[index].Id);
    }

    private sealed class ZeroRandom : IRandomSource
    {
      public int NextInt(int exclusiveMax)
      {
        if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
        return 0;
      }
    }
  }
}
