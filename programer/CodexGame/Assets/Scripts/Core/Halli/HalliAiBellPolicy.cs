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
      var bandRoll = reactionRandom.NextInt(100);
      if (bandRoll < GameRules.AiFastReactionWeightPercent)
      {
        return Uniform(
          reactionRandom,
          GameRules.AiMinimumReactionMicroseconds,
          GameRules.AiFastReactionMaximumMicroseconds);
      }
      if (bandRoll < GameRules.AiFastReactionWeightPercent + GameRules.AiMidReactionWeightPercent)
      {
        return Uniform(
          reactionRandom,
          GameRules.AiFastReactionMaximumMicroseconds,
          GameRules.AiMidReactionMaximumMicroseconds);
      }
      return Uniform(
        reactionRandom,
        GameRules.AiMidReactionMaximumMicroseconds,
        GameRules.AiMaximumReactionMicroseconds);
    }

    public HalliAiBellDecision Decide(
      bool leftValid,
      bool rightValid,
      long baseReactionDelayMicroseconds,
      int stageNumber,
      Func<PileSide, int> strength,
      IRandomSource reactionRandom,
      IRandomSource choiceRandom)
    {
      if (strength == null) throw new ArgumentNullException(nameof(strength));
      if (reactionRandom == null) throw new ArgumentNullException(nameof(reactionRandom));
      if (choiceRandom == null) throw new ArgumentNullException(nameof(choiceRandom));

      var stageMultiplierPercent = StageMultiplierPercent(stageNumber);
      var reactionDelayMicroseconds = ApplyStageMultiplier(
        baseReactionDelayMicroseconds,
        stageMultiplierPercent);

      if (!leftValid && !rightValid)
      {
        return CreateDecision(
          AiBellOutcome.Miss,
          null,
          baseReactionDelayMicroseconds,
          stageMultiplierPercent,
          reactionDelayMicroseconds);
      }

      if (reactionRandom.NextInt(100) < ConditionalMissPercent(baseReactionDelayMicroseconds))
      {
        return CreateDecision(
          AiBellOutcome.Miss,
          null,
          baseReactionDelayMicroseconds,
          stageMultiplierPercent,
          reactionDelayMicroseconds);
      }

      if (choiceRandom.NextInt(100) >= GameRules.AiNonMissCorrectPercent)
      {
        if (leftValid && rightValid)
        {
          return CreateDecision(
            AiBellOutcome.Miss,
            null,
            baseReactionDelayMicroseconds,
            stageMultiplierPercent,
            reactionDelayMicroseconds);
        }

        return CreateDecision(
          AiBellOutcome.Wrong,
          leftValid ? PileSide.Right : PileSide.Left,
          baseReactionDelayMicroseconds,
          stageMultiplierPercent,
          reactionDelayMicroseconds);
      }

      return CreateDecision(
        AiBellOutcome.Correct,
        SelectCorrectPile(leftValid, rightValid, strength, choiceRandom),
        baseReactionDelayMicroseconds,
        stageMultiplierPercent,
        reactionDelayMicroseconds);
    }

    public static int StageMultiplierPercent(int stageNumber)
    {
      if (stageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(stageNumber));
      return stageNumber == 1
        ? GameRules.AiStageOneReactionMultiplierPercent
        : stageNumber == 2
          ? GameRules.AiStageTwoReactionMultiplierPercent
          : GameRules.AiStageThreeReactionMultiplierPercent;
    }

    public static long ApplyStageMultiplier(long baseDelayMicroseconds, int multiplierPercent)
    {
      if (baseDelayMicroseconds < GameRules.AiMinimumReactionMicroseconds
        || baseDelayMicroseconds > GameRules.AiMaximumReactionMicroseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(baseDelayMicroseconds));
      }
      if (multiplierPercent < GameRules.AiStageThreeReactionMultiplierPercent
        || multiplierPercent > GameRules.AiStageOneReactionMultiplierPercent)
      {
        throw new ArgumentOutOfRangeException(nameof(multiplierPercent));
      }
      return Math.Max(
        GameRules.AiMinimumReactionMicroseconds,
        baseDelayMicroseconds * multiplierPercent / 100L);
    }

    public static int ConditionalMissPercent(long baseDelayMicroseconds)
    {
      if (baseDelayMicroseconds < GameRules.AiMinimumReactionMicroseconds
        || baseDelayMicroseconds > GameRules.AiMaximumReactionMicroseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(baseDelayMicroseconds));
      }
      if (baseDelayMicroseconds <= GameRules.AiFastReactionMaximumMicroseconds)
      {
        return GameRules.AiFastConditionalMissPercent;
      }
      return baseDelayMicroseconds <= GameRules.AiMidReactionMaximumMicroseconds
        ? GameRules.AiMidConditionalMissPercent
        : GameRules.AiSlowConditionalMissPercent;
    }

    private static HalliAiBellDecision CreateDecision(
      AiBellOutcome outcome,
      PileSide? pile,
      long baseDelay,
      int multiplierPercent,
      long finalDelay)
    {
      return new HalliAiBellDecision(
        outcome,
        pile,
        baseDelay,
        multiplierPercent,
        finalDelay);
    }

    private static long Uniform(IRandomSource random, long minimum, long maximum)
    {
      return minimum + random.NextInt(checked((int)(maximum - minimum + 1L)));
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
