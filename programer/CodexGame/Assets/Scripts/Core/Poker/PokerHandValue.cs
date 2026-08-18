#nullable enable
using System;
using System.Collections.Generic;

namespace CodexGame.Core.Poker
{
  public sealed class PokerHandValue : IComparable<PokerHandValue>
  {
    public PokerHandValue(
      PokerHandCategory category,
      IReadOnlyList<int> rankVector,
      IReadOnlyList<int> suitVector)
    {
      if (!Enum.IsDefined(typeof(PokerHandCategory), category))
      {
        throw new ArgumentOutOfRangeException(nameof(category));
      }

      Category = category;
      RankVector = Copy(rankVector, nameof(rankVector));
      SuitVector = Copy(suitVector, nameof(suitVector));
    }

    public PokerHandCategory Category { get; }
    public IReadOnlyList<int> RankVector { get; }
    public IReadOnlyList<int> SuitVector { get; }

    public int CompareTo(PokerHandValue? other)
    {
      if (other == null)
      {
        return 1;
      }

      var categoryComparison = Category.CompareTo(other.Category);
      if (categoryComparison != 0)
      {
        return categoryComparison;
      }

      var rankComparison = CompareVectors(RankVector, other.RankVector);
      return rankComparison != 0
        ? rankComparison
        : CompareVectors(SuitVector, other.SuitVector);
    }

    private static IReadOnlyList<int> Copy(IReadOnlyList<int> source, string parameterName)
    {
      if (source == null)
      {
        throw new ArgumentNullException(parameterName);
      }

      var copy = new int[source.Count];
      for (var index = 0; index < source.Count; index++)
      {
        copy[index] = source[index];
      }

      return Array.AsReadOnly(copy);
    }

    private static int CompareVectors(IReadOnlyList<int> left, IReadOnlyList<int> right)
    {
      var count = Math.Min(left.Count, right.Count);
      for (var index = 0; index < count; index++)
      {
        var comparison = left[index].CompareTo(right[index]);
        if (comparison != 0)
        {
          return comparison;
        }
      }

      return left.Count.CompareTo(right.Count);
    }
  }
}
