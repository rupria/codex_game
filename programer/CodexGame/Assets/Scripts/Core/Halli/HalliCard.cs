using System;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Halli
{
  public readonly struct HalliCard
  {
    public HalliCard(CardSuit suit, int skullCount)
    {
      if (!Enum.IsDefined(typeof(CardSuit), suit))
      {
        throw new ArgumentOutOfRangeException(nameof(suit));
      }

      if (skullCount < 1 || skullCount > 3)
      {
        throw new ArgumentOutOfRangeException(nameof(skullCount));
      }

      Suit = suit;
      SkullCount = skullCount;
    }

    public CardSuit Suit { get; }

    public int SkullCount { get; }
  }
}
