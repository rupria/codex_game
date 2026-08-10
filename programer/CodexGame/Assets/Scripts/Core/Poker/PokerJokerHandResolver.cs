using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Poker
{
  public static class PokerJokerHandResolver
  {
    public static IReadOnlyList<JokerHandOption> GetLegalOptions(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet)
    {
      Validate(cards, ruleSet, out var jokerIndex);
      var strongestByCategory = new Dictionary<PokerHandCategory, JokerHandOption>();

      for (var suitValue = (int)CardSuit.Clubs; suitValue <= (int)CardSuit.Spades; suitValue++)
      {
        for (var rankValue = (int)CardRank.Two; rankValue <= (int)CardRank.Ace; rankValue++)
        {
          var replacement = new Card((CardSuit)suitValue, (CardRank)rankValue, 1);
          if (ContainsStandardCard(cards, replacement.Id)) continue;

          var substituted = new Card[cards.Count];
          for (var index = 0; index < cards.Count; index++)
          {
            substituted[index] = index == jokerIndex ? replacement : cards[index];
          }

          var value = PokerEvaluator.Evaluate(Array.AsReadOnly(substituted), ruleSet);
          var option = new JokerHandOption(value.Category, replacement, value);
          if (!strongestByCategory.TryGetValue(value.Category, out var current)
            || value.CompareTo(current.HandValue) > 0)
          {
            strongestByCategory[value.Category] = option;
          }
        }
      }

      var options = new List<JokerHandOption>(strongestByCategory.Values);
      options.Sort((left, right) => right.HandValue.CompareTo(left.HandValue));
      return Array.AsReadOnly(options.ToArray());
    }

    public static JokerHandOption Resolve(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet,
      PokerHandCategory category)
    {
      if (!Enum.IsDefined(typeof(PokerHandCategory), category))
      {
        throw new ArgumentOutOfRangeException(nameof(category));
      }

      var options = GetLegalOptions(cards, ruleSet);
      for (var index = 0; index < options.Count; index++)
      {
        if (options[index].Category == category) return options[index];
      }

      throw new ArgumentException("The selected Joker hand category cannot be completed.", nameof(category));
    }

    public static JokerHandOption ResolveStrongest(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet)
    {
      var options = GetLegalOptions(cards, ruleSet);
      if (options.Count == 0) throw new InvalidOperationException("Joker has no legal substitution.");
      return options[0];
    }

    private static void Validate(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet,
      out int jokerIndex)
    {
      if (cards == null) throw new ArgumentNullException(nameof(cards));
      if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));
      if (cards.Count != PokerEvaluator.HandSize)
      {
        throw new ArgumentException("A Joker hand must contain exactly five cards.", nameof(cards));
      }

      var ids = new HashSet<CardId>();
      jokerIndex = -1;
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsValid || !ids.Add(cards[index].Id))
        {
          throw new ArgumentException("A Joker hand must contain unique valid cards.", nameof(cards));
        }
        if (!cards[index].IsJoker) continue;
        if (jokerIndex >= 0) throw new ArgumentException("A hand can contain only one Joker.", nameof(cards));
        jokerIndex = index;
      }

      if (jokerIndex < 0) throw new ArgumentException("A Joker hand must contain one Joker.", nameof(cards));
    }

    private static bool ContainsStandardCard(IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsJoker && cards[index].Id == id) return true;
      }
      return false;
    }
  }
}
