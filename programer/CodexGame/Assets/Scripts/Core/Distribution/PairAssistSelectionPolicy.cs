using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Distribution
{
  public static class PairAssistSelectionPolicy
  {
    public static IReadOnlyList<CardId> Select(
      IReadOnlyList<Card> candidates,
      int requiredCount,
      IRandomSource random)
    {
      if (candidates == null) throw new ArgumentNullException(nameof(candidates));
      if (random == null) throw new ArgumentNullException(nameof(random));
      if (requiredCount < 0 || requiredCount > candidates.Count)
      {
        throw new ArgumentOutOfRangeException(nameof(requiredCount));
      }

      var available = CopyAndValidate(candidates);
      var selected = new List<Card>(requiredCount);
      var pairRanks = FindPairRanks(available);

      if (requiredCount >= 2 && pairRanks.Count > 0)
      {
        var pairRank = pairRanks[random.NextInt(pairRanks.Count)];
        TakeMatchingRank(selected, available, pairRank, 2, random);
      }

      while (selected.Count < requiredCount)
      {
        selected.Add(TakeRandom(available, random));
      }

      var result = new CardId[selected.Count];
      for (var index = 0; index < selected.Count; index++) result[index] = selected[index].Id;
      return Array.AsReadOnly(result);
    }

    private static List<Card> CopyAndValidate(IReadOnlyList<Card> candidates)
    {
      var result = new List<Card>(candidates.Count);
      var ids = new HashSet<CardId>();
      for (var index = 0; index < candidates.Count; index++)
      {
        var card = candidates[index];
        if (!card.IsValid || !ids.Add(card.Id))
        {
          throw new ArgumentException("Pair-assist candidates must be valid and unique.", nameof(candidates));
        }

        result.Add(card);
      }

      return result;
    }

    private static List<CardRank> FindPairRanks(IReadOnlyList<Card> cards)
    {
      var counts = new Dictionary<CardRank, int>();
      for (var index = 0; index < cards.Count; index++)
      {
        counts.TryGetValue(cards[index].Rank, out var count);
        counts[cards[index].Rank] = count + 1;
      }

      var result = new List<CardRank>();
      foreach (var pair in counts)
      {
        if (pair.Value >= 2) result.Add(pair.Key);
      }

      result.Sort();
      return result;
    }

    private static void TakeMatchingRank(
      ICollection<Card> destination,
      IList<Card> available,
      CardRank rank,
      int count,
      IRandomSource random)
    {
      for (var selected = 0; selected < count; selected++)
      {
        var matchingIndexes = new List<int>();
        for (var index = 0; index < available.Count; index++)
        {
          if (available[index].Rank == rank) matchingIndexes.Add(index);
        }

        var matchIndex = matchingIndexes[random.NextInt(matchingIndexes.Count)];
        destination.Add(available[matchIndex]);
        available.RemoveAt(matchIndex);
      }
    }

    private static Card TakeRandom(IList<Card> cards, IRandomSource random)
    {
      var index = random.NextInt(cards.Count);
      var card = cards[index];
      cards.RemoveAt(index);
      return card;
    }
  }
}
