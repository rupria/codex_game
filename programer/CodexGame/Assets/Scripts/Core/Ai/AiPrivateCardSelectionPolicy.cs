using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Ai
{
  public sealed class AiPrivateCardSelectionPolicy
  {
    public const int StrengthChoicePercent = 60;

    public IReadOnlyList<CardId> Select(
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

      var cards = new List<Card>(candidates.Count);
      var ids = new HashSet<CardId>();
      for (var index = 0; index < candidates.Count; index++)
      {
        if (!candidates[index].IsValid || !ids.Add(candidates[index].Id))
        {
          throw new ArgumentException("AI candidates must be valid and unique.", nameof(candidates));
        }

        cards.Add(candidates[index]);
      }

      if (random.NextInt(100) < StrengthChoicePercent)
      {
        cards.Sort((left, right) =>
        {
          if (left.IsJoker != right.IsJoker) return left.IsJoker ? -1 : 1;
          var rank = ((int)right.Rank).CompareTo((int)left.Rank);
          return rank != 0 ? rank : ((int)right.Suit).CompareTo((int)left.Suit);
        });
      }
      else
      {
        for (var index = cards.Count - 1; index > 0; index--)
        {
          var swapIndex = random.NextInt(index + 1);
          var temporary = cards[index];
          cards[index] = cards[swapIndex];
          cards[swapIndex] = temporary;
        }
      }

      var selected = new CardId[requiredCount];
      for (var index = 0; index < requiredCount; index++) selected[index] = cards[index].Id;
      return Array.AsReadOnly(selected);
    }
  }
}
