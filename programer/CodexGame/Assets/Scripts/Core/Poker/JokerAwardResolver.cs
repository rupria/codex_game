using System;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Poker
{
  public static class JokerAwardResolver
  {
    public static bool Roll(
      int acquiredCardCount,
      IRandomSource random,
      int awardPercent = GameRules.JokerAwardPercent)
    {
      if (acquiredCardCount < 0) throw new ArgumentOutOfRangeException(nameof(acquiredCardCount));
      if (random == null) throw new ArgumentNullException(nameof(random));
      if (awardPercent < 0 || awardPercent > 100)
      {
        throw new ArgumentOutOfRangeException(nameof(awardPercent));
      }
      if (acquiredCardCount < GameRules.JokerEligibilityAcquiredCards) return false;
      return random.NextInt(100) < awardPercent;
    }
  }
}
