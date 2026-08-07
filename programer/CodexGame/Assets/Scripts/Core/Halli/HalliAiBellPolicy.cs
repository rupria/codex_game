using System;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public sealed class HalliAiBellPolicy
  {
    public long CreateReactionDelay(IRandomSource reactionRandom)
    {
      if (reactionRandom == null) throw new ArgumentNullException(nameof(reactionRandom));

      // Two uniform samples form a triangular distribution with its mode near two seconds.
      var first = reactionRandom.NextInt(1_000_001);
      var second = reactionRandom.NextInt(1_000_001);
      return GameRules.AiMinimumReactionMicroseconds + first + second;
    }

    public HalliAiBellDecision Decide(
      bool leftValid,
      bool rightValid,
      long reactionDelayMicroseconds,
      Func<PileSide, int> strength,
      IRandomSource reactionRandom,
      IRandomSource choiceRandom)
    {
      if (strength == null) throw new ArgumentNullException(nameof(strength));
      if (reactionRandom == null) throw new ArgumentNullException(nameof(reactionRandom));
      if (choiceRandom == null) throw new ArgumentNullException(nameof(choiceRandom));

      if (!leftValid && !rightValid)
      {
        return new HalliAiBellDecision(AiBellOutcome.Miss, null, reactionDelayMicroseconds);
      }

      var roll = reactionRandom.NextInt(100);
      if (roll >= GameRules.AiCorrectBellPercent + GameRules.AiWrongBellPercent)
      {
        return new HalliAiBellDecision(AiBellOutcome.Miss, null, reactionDelayMicroseconds);
      }

      if (roll >= GameRules.AiCorrectBellPercent)
      {
        if (leftValid && rightValid)
        {
          return new HalliAiBellDecision(AiBellOutcome.Miss, null, reactionDelayMicroseconds);
        }

        return new HalliAiBellDecision(
          AiBellOutcome.Wrong,
          leftValid ? PileSide.Right : PileSide.Left,
          reactionDelayMicroseconds);
      }

      return new HalliAiBellDecision(
        AiBellOutcome.Correct,
        SelectCorrectPile(leftValid, rightValid, strength, choiceRandom),
        reactionDelayMicroseconds);
    }

    private static PileSide SelectCorrectPile(
      bool leftValid,
      bool rightValid,
      Func<PileSide, int> strength,
      IRandomSource random)
    {
      if (!leftValid) return PileSide.Right;
      if (!rightValid) return PileSide.Left;

      if (random.NextInt(100) >= 60)
      {
        return random.NextInt(2) == 0 ? PileSide.Left : PileSide.Right;
      }

      return strength(PileSide.Left) >= strength(PileSide.Right)
        ? PileSide.Left
        : PileSide.Right;
    }
  }
}
