using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Poker
{
  public static class PokerHandDistributionProfile
  {
    public const int Scale = 100_000_000;

    private static readonly IReadOnlyList<Entry> Entries = Array.AsReadOnly(new[]
    {
      new Entry(PokerHandCategory.HighCard, 40_000_000),
      new Entry(PokerHandCategory.OnePair, 50_827_972),
      new Entry(PokerHandCategory.TwoPair, 5_718_147),
      new Entry(PokerHandCategory.ThreeOfAKind, 2_541_399),
      new Entry(PokerHandCategory.Straight, 472_069),
      new Entry(PokerHandCategory.Flush, 236_405),
      new Entry(PokerHandCategory.FullHouse, 173_277),
      new Entry(PokerHandCategory.FourOfAKind, 28_880),
      new Entry(PokerHandCategory.StraightFlush, 1_666),
      new Entry(PokerHandCategory.RoyalStraightFlush, 185)
    });

    public static IReadOnlyList<Entry> NormalizedEntries => Entries;

    public static PokerHandCategory Roll(IRandomSource random)
    {
      if (random == null) throw new ArgumentNullException(nameof(random));
      var roll = random.NextInt(Scale);
      var cumulative = 0;
      for (var index = 0; index < Entries.Count; index++)
      {
        cumulative += Entries[index].Weight;
        if (roll < cumulative) return Entries[index].Category;
      }

      throw new InvalidOperationException("Poker hand distribution weights are not normalized.");
    }

    public readonly struct Entry
    {
      public Entry(PokerHandCategory category, int weight)
      {
        Category = category;
        Weight = weight;
      }

      public PokerHandCategory Category { get; }
      public int Weight { get; }
    }
  }
}
