using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;

namespace CodexGame.SmokeTests.Poker
{
  internal static class PokerEvaluatorTests
  {
    public static void Run(TestHarness tests)
    {
      ChecksEveryCategory(tests);
      ChecksCategoryOrdering(tests);
      ChecksRankAndSuitTieBreaks(tests);
      ChecksAceStraightPolicy(tests);
      RejectsInvalidHands(tests);
    }

    private static void ChecksEveryCategory(TestHarness tests)
    {
      var cases = new[]
      {
        Case(PokerHandCategory.HighCard, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Jack), C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five), C(CardSuit.Spades, CardRank.Two)),
        Case(PokerHandCategory.OnePair, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five), C(CardSuit.Spades, CardRank.Two)),
        Case(PokerHandCategory.TwoPair, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Nine), C(CardSuit.Spades, CardRank.Two)),
        Case(PokerHandCategory.ThreeOfAKind, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Ace), C(CardSuit.Clubs, CardRank.Five), C(CardSuit.Spades, CardRank.Two)),
        Case(PokerHandCategory.Straight, C(CardSuit.Spades, CardRank.Ten), C(CardSuit.Hearts, CardRank.Nine), C(CardSuit.Diamonds, CardRank.Eight), C(CardSuit.Clubs, CardRank.Seven), C(CardSuit.Spades, CardRank.Six)),
        Case(PokerHandCategory.Flush, C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Hearts, CardRank.Jack), C(CardSuit.Hearts, CardRank.Nine), C(CardSuit.Hearts, CardRank.Five), C(CardSuit.Hearts, CardRank.Two)),
        Case(PokerHandCategory.FullHouse, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Ace), C(CardSuit.Clubs, CardRank.Nine), C(CardSuit.Spades, CardRank.Nine)),
        Case(PokerHandCategory.FourOfAKind, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Diamonds, CardRank.Ace), C(CardSuit.Clubs, CardRank.Ace), C(CardSuit.Spades, CardRank.Nine)),
        Case(PokerHandCategory.StraightFlush, C(CardSuit.Spades, CardRank.Ten), C(CardSuit.Spades, CardRank.Nine), C(CardSuit.Spades, CardRank.Eight), C(CardSuit.Spades, CardRank.Seven), C(CardSuit.Spades, CardRank.Six)),
        Case(PokerHandCategory.RoyalStraightFlush, C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Spades, CardRank.King), C(CardSuit.Spades, CardRank.Queen), C(CardSuit.Spades, CardRank.Jack), C(CardSuit.Spades, CardRank.Ten))
      };

      for (var index = 0; index < cases.Length; index++)
      {
        var actual = PokerEvaluator.Evaluate(cases[index].Cards, PokerRuleSet.Development);
        tests.Check(actual.Category == cases[index].Category, "Poker evaluator should identify " + cases[index].Category + ".");
      }
    }

    private static void ChecksCategoryOrdering(TestHarness tests)
    {
      var high = PokerEvaluator.Evaluate(Cards(
        C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Jack),
        C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Spades, CardRank.Two)), PokerRuleSet.Development);
      var pair = PokerEvaluator.Evaluate(Cards(
        C(CardSuit.Spades, CardRank.Two), C(CardSuit.Hearts, CardRank.Two),
        C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Spades, CardRank.Three)), PokerRuleSet.Development);
      tests.Check(pair.CompareTo(high) > 0, "A pair should beat any high-card hand.");
    }

    private static void ChecksRankAndSuitTieBreaks(TestHarness tests)
    {
      var kings = PokerEvaluator.Evaluate(Cards(
        C(CardSuit.Spades, CardRank.King), C(CardSuit.Hearts, CardRank.King),
        C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Spades, CardRank.Two)), PokerRuleSet.Development);
      var queens = PokerEvaluator.Evaluate(Cards(
        C(CardSuit.Spades, CardRank.Queen), C(CardSuit.Hearts, CardRank.Queen),
        C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Spades, CardRank.Two)), PokerRuleSet.Development);
      tests.Check(kings.CompareTo(queens) > 0, "Category-specific pair rank should break a tie first.");

      var spadeAce = PokerEvaluator.Evaluate(Cards(
        C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Jack),
        C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Spades, CardRank.Two)), PokerRuleSet.Development);
      var heartAce = PokerEvaluator.Evaluate(Cards(
        C(CardSuit.Hearts, CardRank.Ace), C(CardSuit.Spades, CardRank.Jack),
        C(CardSuit.Diamonds, CardRank.Nine), C(CardSuit.Clubs, CardRank.Five),
        C(CardSuit.Spades, CardRank.Two)), PokerRuleSet.Development);
      tests.Check(spadeAce.CompareTo(heartAce) > 0, "Spades should beat hearts after rank vectors tie.");
    }

    private static void ChecksAceStraightPolicy(TestHarness tests)
    {
      var wheel = Cards(
        C(CardSuit.Spades, CardRank.Ace), C(CardSuit.Hearts, CardRank.Two),
        C(CardSuit.Diamonds, CardRank.Three), C(CardSuit.Clubs, CardRank.Four),
        C(CardSuit.Spades, CardRank.Five));
      tests.Check(
        PokerEvaluator.Evaluate(wheel, new PokerRuleSet(AceStraightMode.HighOnly)).Category == PokerHandCategory.HighCard,
        "The provisional high-only rule must reject A-2-3-4-5.");
      tests.Check(
        PokerEvaluator.Evaluate(wheel, new PokerRuleSet(AceStraightMode.HighAndLow)).Category == PokerHandCategory.Straight,
        "The alternate policy must support A-2-3-4-5 without evaluator changes.");
      tests.Check(
        PokerEvaluator.Evaluate(wheel, PokerRuleSet.Development).Category == PokerHandCategory.Straight,
        "The 0.08 development rules must enable A-2-3-4-5 as the lowest straight.");
    }

    private static void RejectsInvalidHands(TestHarness tests)
    {
      tests.CheckThrows<ArgumentException>(
        () => PokerEvaluator.Evaluate(Cards(C(CardSuit.Spades, CardRank.Ace)), PokerRuleSet.Development),
        "Poker evaluator should reject hands that are not exactly five cards.");
      var duplicate = C(CardSuit.Spades, CardRank.Ace);
      tests.CheckThrows<ArgumentException>(
        () => PokerEvaluator.Evaluate(Cards(duplicate, duplicate,
          C(CardSuit.Hearts, CardRank.King), C(CardSuit.Diamonds, CardRank.Queen),
          C(CardSuit.Clubs, CardRank.Jack)), PokerRuleSet.Development),
        "Poker evaluator should reject duplicate card identities.");
    }

    private static HandCase Case(PokerHandCategory category, params Card[] cards)
    {
      return new HandCase(category, Cards(cards));
    }

    private static IReadOnlyList<Card> Cards(params Card[] cards)
    {
      return Array.AsReadOnly(cards);
    }

    private static Card C(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }

    private sealed class HandCase
    {
      public HandCase(PokerHandCategory category, IReadOnlyList<Card> cards)
      {
        Category = category;
        Cards = cards;
      }

      public PokerHandCategory Category { get; }
      public IReadOnlyList<Card> Cards { get; }
    }
  }
}
