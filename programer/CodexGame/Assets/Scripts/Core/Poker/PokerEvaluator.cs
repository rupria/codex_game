using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Poker
{
  public static class PokerEvaluator
  {
    public const int HandSize = 5;

    public static PokerHandValue Evaluate(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet)
    {
      Validate(cards, ruleSet);
      var jokerIndex = FindJoker(cards);
      if (jokerIndex >= 0)
      {
        return EvaluateWithJoker(cards, ruleSet, jokerIndex);
      }

      return EvaluateStandard(cards, ruleSet);
    }

    private static PokerHandValue EvaluateStandard(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet)
    {
      var byRank = GroupByRank(cards);
      var flush = IsFlush(cards);
      var straightHigh = GetStraightHigh(byRank, ruleSet.AceStraightMode);
      var groups = SortGroups(byRank);
      PokerHandCategory category;
      IReadOnlyList<int> rankVector;

      if (flush && straightHigh > 0)
      {
        category = straightHigh == (int)CardRank.Ace
          && byRank.ContainsKey((int)CardRank.Ten)
          ? PokerHandCategory.RoyalStraightFlush
          : PokerHandCategory.StraightFlush;
        rankVector = Vector(straightHigh);
      }
      else if (groups[0].Cards.Count == 4)
      {
        category = PokerHandCategory.FourOfAKind;
        rankVector = Vector(groups[0].Rank, groups[1].Rank);
      }
      else if (groups[0].Cards.Count == 3 && groups[1].Cards.Count == 2)
      {
        category = PokerHandCategory.FullHouse;
        rankVector = Vector(groups[0].Rank, groups[1].Rank);
      }
      else if (flush)
      {
        category = PokerHandCategory.Flush;
        rankVector = DescendingRanks(cards, false);
      }
      else if (straightHigh > 0)
      {
        category = PokerHandCategory.Straight;
        rankVector = Vector(straightHigh);
      }
      else if (groups[0].Cards.Count == 3)
      {
        category = PokerHandCategory.ThreeOfAKind;
        rankVector = GroupRankVector(groups);
      }
      else if (groups[0].Cards.Count == 2 && groups[1].Cards.Count == 2)
      {
        category = PokerHandCategory.TwoPair;
        rankVector = GroupRankVector(groups);
      }
      else if (groups[0].Cards.Count == 2)
      {
        category = PokerHandCategory.OnePair;
        rankVector = GroupRankVector(groups);
      }
      else
      {
        category = PokerHandCategory.HighCard;
        rankVector = DescendingRanks(cards, false);
      }

      var wheel = straightHigh == 5
        && byRank.ContainsKey((int)CardRank.Ace)
        && ruleSet.AceStraightMode == AceStraightMode.HighAndLow;
      return new PokerHandValue(category, rankVector, DescendingSuits(cards, wheel));
    }

    private static PokerHandValue EvaluateWithJoker(
      IReadOnlyList<Card> cards,
      PokerRuleSet ruleSet,
      int jokerIndex)
    {
      PokerHandValue? best = null;
      for (var suitValue = (int)CardSuit.Clubs; suitValue <= (int)CardSuit.Spades; suitValue++)
      {
        for (var rankValue = (int)CardRank.Two; rankValue <= (int)CardRank.Ace; rankValue++)
        {
          var replacement = new Card((CardSuit)suitValue, (CardRank)rankValue, 1);
          if (ContainsId(cards, replacement.Id)) continue;
          var substituted = new Card[cards.Count];
          for (var index = 0; index < cards.Count; index++)
          {
            substituted[index] = index == jokerIndex ? replacement : cards[index];
          }

          var value = EvaluateStandard(Array.AsReadOnly(substituted), ruleSet);
          if (best == null || value.CompareTo(best) > 0) best = value;
        }
      }

      if (best == null) throw new InvalidOperationException("Joker has no legal substitution.");
      return best;
    }

    private static void Validate(IReadOnlyList<Card> cards, PokerRuleSet ruleSet)
    {
      if (cards == null)
      {
        throw new ArgumentNullException(nameof(cards));
      }

      if (ruleSet == null)
      {
        throw new ArgumentNullException(nameof(ruleSet));
      }

      if (cards.Count != HandSize)
      {
        throw new ArgumentException("A poker hand must contain exactly five cards.", nameof(cards));
      }

      var ids = new HashSet<CardId>();
      var jokerCount = 0;
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsValid || !ids.Add(cards[index].Id))
        {
          throw new ArgumentException(
            "A poker hand must contain valid cards with no duplicate identities.",
            nameof(cards));
        }

        if (cards[index].IsJoker) jokerCount++;
      }


      if (jokerCount > 1)
      {
        throw new ArgumentException("A poker hand can use at most one Joker.", nameof(cards));
      }
    }

    private static int FindJoker(IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].IsJoker) return index;
      }
      return -1;
    }

    private static bool ContainsId(IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsJoker && cards[index].Id == id) return true;
      }
      return false;
    }

    private static Dictionary<int, List<Card>> GroupByRank(IReadOnlyList<Card> cards)
    {
      var result = new Dictionary<int, List<Card>>();
      for (var index = 0; index < cards.Count; index++)
      {
        var rank = (int)cards[index].Rank;
        if (!result.TryGetValue(rank, out var group))
        {
          group = new List<Card>();
          result.Add(rank, group);
        }

        group.Add(cards[index]);
      }

      return result;
    }

    private static List<RankGroup> SortGroups(Dictionary<int, List<Card>> byRank)
    {
      var groups = new List<RankGroup>(byRank.Count);
      foreach (var pair in byRank)
      {
        groups.Add(new RankGroup(pair.Key, pair.Value));
      }

      groups.Sort((left, right) =>
      {
        var countComparison = right.Cards.Count.CompareTo(left.Cards.Count);
        return countComparison != 0
          ? countComparison
          : right.Rank.CompareTo(left.Rank);
      });
      return groups;
    }

    private static bool IsFlush(IReadOnlyList<Card> cards)
    {
      for (var index = 1; index < cards.Count; index++)
      {
        if (cards[index].Suit != cards[0].Suit)
        {
          return false;
        }
      }

      return true;
    }

    private static int GetStraightHigh(
      Dictionary<int, List<Card>> byRank,
      AceStraightMode aceStraightMode)
    {
      if (byRank.Count != HandSize)
      {
        return 0;
      }

      var ranks = new List<int>(byRank.Keys);
      ranks.Sort();
      var consecutive = true;
      for (var index = 1; index < ranks.Count; index++)
      {
        if (ranks[index] != ranks[index - 1] + 1)
        {
          consecutive = false;
          break;
        }
      }

      if (consecutive)
      {
        return ranks[ranks.Count - 1];
      }

      return aceStraightMode == AceStraightMode.HighAndLow
        && ranks[0] == 2
        && ranks[1] == 3
        && ranks[2] == 4
        && ranks[3] == 5
        && ranks[4] == 14
          ? 5
          : 0;
    }

    private static IReadOnlyList<int> GroupRankVector(IReadOnlyList<RankGroup> groups)
    {
      var ranks = new int[groups.Count];
      for (var index = 0; index < groups.Count; index++)
      {
        ranks[index] = groups[index].Rank;
      }

      return Array.AsReadOnly(ranks);
    }

    private static IReadOnlyList<int> DescendingRanks(IReadOnlyList<Card> cards, bool wheel)
    {
      var ranks = new int[cards.Count];
      for (var index = 0; index < cards.Count; index++)
      {
        ranks[index] = EffectiveRank(cards[index], wheel);
      }

      Array.Sort(ranks);
      Array.Reverse(ranks);
      return Array.AsReadOnly(ranks);
    }

    private static IReadOnlyList<int> DescendingSuits(IReadOnlyList<Card> cards, bool wheel)
    {
      var ordered = new List<Card>(cards.Count);
      for (var index = 0; index < cards.Count; index++)
      {
        ordered.Add(cards[index]);
      }

      ordered.Sort((left, right) =>
      {
        var rankComparison = EffectiveRank(right, wheel).CompareTo(EffectiveRank(left, wheel));
        return rankComparison != 0
          ? rankComparison
          : ((int)right.Suit).CompareTo((int)left.Suit);
      });

      var suits = new int[ordered.Count];
      for (var index = 0; index < ordered.Count; index++)
      {
        suits[index] = (int)ordered[index].Suit;
      }

      return Array.AsReadOnly(suits);
    }

    private static int EffectiveRank(Card card, bool wheel)
    {
      return wheel && card.Rank == CardRank.Ace ? 1 : (int)card.Rank;
    }

    private static IReadOnlyList<int> Vector(params int[] values)
    {
      return Array.AsReadOnly(values);
    }

    private sealed class RankGroup
    {
      public RankGroup(int rank, List<Card> cards)
      {
        Rank = rank;
        Cards = cards;
      }

      public int Rank { get; }
      public List<Card> Cards { get; }
    }
  }
}
