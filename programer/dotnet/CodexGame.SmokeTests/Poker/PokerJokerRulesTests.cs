using System;
using CodexGame.Application.Distribution;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.SmokeTests.Cards;

namespace CodexGame.SmokeTests.Poker
{
  internal static class PokerJokerRulesTests
  {
    public static void Run(TestHarness tests)
    {
      CheckNormalizedProfile(tests);
      CheckIndependentJokerRoll(tests);
      CheckSelectionCandidateIntegration(tests);
      CheckBalancedHiddenDeal(tests);
      CheckBestLegalSubstitution(tests);
      CheckPlayerPresentationLock(tests);
      CheckAiSecrecy(tests);
    }

    private static void CheckNormalizedProfile(TestHarness tests)
    {
      var sum = 0;
      for (var index = 0; index < PokerHandDistributionProfile.NormalizedEntries.Count; index++)
      {
        sum += PokerHandDistributionProfile.NormalizedEntries[index].Weight;
      }
      tests.Check(
        sum == PokerHandDistributionProfile.Scale
          && PokerHandDistributionProfile.NormalizedEntries[0].Weight == 40_000_000,
        "Poker hand target weights must normalize exactly to 100% with High Card fixed at 40%.");
      tests.Check(
        PokerHandDistributionProfile.Roll(new FixedRandom(39_999_999)) == PokerHandCategory.HighCard
          && PokerHandDistributionProfile.Roll(new FixedRandom(40_000_000)) == PokerHandCategory.OnePair
          && PokerHandDistributionProfile.Roll(new FixedRandom(99_999_999))
            == PokerHandCategory.RoyalStraightFlush,
        "Poker hand weighted boundaries must preserve every approved category bucket.");
    }

    private static void CheckIndependentJokerRoll(TestHarness tests)
    {
      var ineligible = new CountingRandom(0);
      tests.Check(
        !JokerAwardResolver.Roll(3, ineligible) && ineligible.CallCount == 0,
        "Fewer than four acquired cards must not consume the Joker RNG channel.");
      tests.Check(
        JokerAwardResolver.Roll(4, new FixedRandom(9))
          && !JokerAwardResolver.Roll(4, new FixedRandom(10)),
        "Four or more acquired cards must use an independent exact 10% Joker roll.");
    }

    private static void CheckBestLegalSubstitution(TestHarness tests)
    {
      var hand = Array.AsReadOnly(new[]
      {
        C(CardSuit.Spades, CardRank.Ace),
        C(CardSuit.Hearts, CardRank.Ace),
        C(CardSuit.Clubs, CardRank.King),
        C(CardSuit.Diamonds, CardRank.Queen),
        new Card(PokerJokerKind.BrassSheriffRevolver)
      });
      tests.Check(
        PokerEvaluator.Evaluate(hand, PokerRuleSet.Development).Category
          == PokerHandCategory.ThreeOfAKind,
        "Joker must choose the strongest legal non-duplicate substitution.");
    }

    private static void CheckBalancedHiddenDeal(TestHarness tests)
    {
      var deck = TestCardSet.Create();
      var candidates = new Card[deck.Count - 1];
      for (var index = 1; index < deck.Count; index++) candidates[index - 1] = deck[index];
      var result = PrivateCardDistributionResolver.ResolveBothBalanced(
        Array.AsReadOnly(Array.Empty<Card>()),
        Array.AsReadOnly(Array.Empty<Card>()),
        Array.AsReadOnly(candidates),
        HalliStageWinner.None,
        1,
        Array.AsReadOnly(Array.Empty<CardId>()),
        Array.AsReadOnly(Array.Empty<CardId>()),
        PrivateCardSelectionMode.Confirmed,
        DeterministicRandomFactory.Create(123, RandomChannel.CardDistribution),
        deck[0],
        new FixedRandom(0),
        new FixedRandom(40_000_000));
      var playerHand = Join(result.PlayerPrivateCards, deck[0], result.SecondPublicCard);
      var aiHand = Join(result.AiPrivateCards, deck[0], result.SecondPublicCard);
      tests.Check(
        PokerEvaluator.Evaluate(playerHand, PokerRuleSet.Development).Category
            == PokerHandCategory.HighCard
          && PokerEvaluator.Evaluate(aiHand, PokerRuleSet.Development).Category
            == PokerHandCategory.OnePair,
        "Hidden deal generation must apply the independently rolled normalized category targets before exposure.");
    }

    private static void CheckSelectionCandidateIntegration(TestHarness tests)
    {
      var deck = TestCardSet.Create();
      var seed = FindPlayerJokerSeed();
      var player = Slice(deck, 0, 4);
      var session = new PrivateCardSelectionSession();
      session.Begin(
        player,
        Array.AsReadOnly(Array.Empty<Card>()),
        Slice(deck, 4, deck.Count - 4),
        HalliStageWinner.Player,
        1,
        seed,
        new GameTimestamp(0));
      var snapshot = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        snapshot.WinnerCandidates.Count == 5 && ContainsJoker(snapshot.WinnerCandidates),
        "An awarded player Joker must join the private selection candidates exactly once.");
      session.Toggle(snapshot.WinnerCandidates[0].Id);
      session.Toggle(snapshot.WinnerCandidates[1].Id);
      session.Toggle(snapshot.WinnerCandidates[4].Id);
      tests.Check(
        session.TryConfirm()
          && ContainsJoker(session.GetSnapshot(new GameTimestamp(0)).Result!.PlayerPrivateCards),
        "A selected Joker must enter the three-card private hand without joining the standard deck pool.");
    }

    private static void CheckPlayerPresentationLock(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(playerJoker: true, aiJoker: false),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0));
      tests.Check(
        session.GetSnapshot(new GameTimestamp(0)).Phase == PokerRoundPhase.PlayerJokerPresentation
          && !session.SubmitPrediction(PredictionChoice.PlayerWins, new GameTimestamp(0)),
        "Player Joker presentation must lock prediction input.");
      tests.Check(
        !session.Tick(new GameTimestamp(GameRules.PlayerJokerPresentationMicroseconds))
          && session.GetSnapshot(new GameTimestamp(GameRules.PlayerJokerPresentationMicroseconds)).Phase
            == PokerRoundPhase.AwaitingPrediction,
        "Joker presentation must complete once and then open the full prediction window.");
      session.Tick(new GameTimestamp(GameRules.PlayerJokerPresentationMicroseconds + 1));
      tests.Check(
        session.GetSnapshot(new GameTimestamp(GameRules.PlayerJokerPresentationMicroseconds + 1))
          .RemainingMicroseconds == GameRules.PredictionTimeoutMicroseconds - 1,
        "Repeated ticks must not restart the Joker presentation or prediction timer.");
    }

    private static void CheckAiSecrecy(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(playerJoker: false, aiJoker: true),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0));
      var concealed = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        concealed.Phase == PokerRoundPhase.AwaitingPrediction
          && concealed.VisibleAiPrivateCards.Count == 0,
        "AI Joker must not change the pre-showdown phase, count, or visible card payload.");
      session.SubmitPrediction(PredictionChoice.PlayerWins, new GameTimestamp(0));
      session.Tick(new GameTimestamp(GameRules.PokerResultAnnouncementMicroseconds));
      var revealed = session.GetSnapshot(new GameTimestamp(GameRules.PokerResultAnnouncementMicroseconds));
      tests.Check(
        revealed.VisibleAiPrivateCards.Count == 3
          && ContainsJoker(revealed.VisibleAiPrivateCards),
        "AI Joker may reveal only at showdown when it is one of the three actually used cards.");
    }

    private static PrivateCardDistributionResult Distribution(bool playerJoker, bool aiJoker)
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          playerJoker ? new Card(PokerJokerKind.BrassSheriffRevolver) : C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Diamonds, CardRank.Ace),
          C(CardSuit.Clubs, CardRank.King)
        }),
        Array.AsReadOnly(new[]
        {
          aiJoker ? new Card(PokerJokerKind.CrimsonCardsharp) : C(CardSuit.Spades, CardRank.Queen),
          C(CardSuit.Diamonds, CardRank.Jack),
          C(CardSuit.Clubs, CardRank.Ten)
        }),
        C(CardSuit.Hearts, CardRank.Seven),
        Array.AsReadOnly(Array.Empty<Card>()));
    }

    private static bool ContainsJoker(System.Collections.Generic.IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++) if (cards[index].IsJoker) return true;
      return false;
    }

    private static System.Collections.Generic.IReadOnlyList<Card> Join(
      System.Collections.Generic.IReadOnlyList<Card> privateCards,
      Card firstPublic,
      Card secondPublic)
    {
      return Array.AsReadOnly(new[]
      {
        privateCards[0], privateCards[1], privateCards[2], firstPublic, secondPublic
      });
    }

    private static System.Collections.Generic.IReadOnlyList<Card> Slice(
      System.Collections.Generic.IReadOnlyList<Card> source,
      int start,
      int count)
    {
      var result = new Card[count];
      for (var index = 0; index < count; index++) result[index] = source[start + index];
      return Array.AsReadOnly(result);
    }

    private static long FindPlayerJokerSeed()
    {
      for (long seed = 0; seed < 10_000; seed++)
      {
        if (JokerAwardResolver.Roll(
          4,
          DeterministicRandomFactory.Create(seed, RandomChannel.PlayerJokerAward))) return seed;
      }
      throw new InvalidOperationException("No deterministic player Joker seed found.");
    }

    private static Card C(CardSuit suit, CardRank rank) => new Card(suit, rank, 1);

    private sealed class FixedRandom : IRandomSource
    {
      private readonly int _value;
      public FixedRandom(int value) { _value = value; }
      public int NextInt(int exclusiveMax)
      {
        if (_value < 0 || _value >= exclusiveMax) throw new InvalidOperationException();
        return _value;
      }
    }

    private sealed class CountingRandom : IRandomSource
    {
      private readonly int _value;
      public CountingRandom(int value) { _value = value; }
      public int CallCount { get; private set; }
      public int NextInt(int exclusiveMax)
      {
        CallCount++;
        return _value % exclusiveMax;
      }
    }
  }
}
