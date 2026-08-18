using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public readonly struct HalliAiBellDecision
  {
    public HalliAiBellDecision(
      AiBellOutcome outcome,
      PileSide? pile,
      long baseReactionDelayMicroseconds,
      int stageMultiplierPercent,
      long reactionDelayMicroseconds)
    {
      if (!Enum.IsDefined(typeof(AiBellOutcome), outcome))
      {
        throw new ArgumentOutOfRangeException(nameof(outcome));
      }

      if ((outcome == AiBellOutcome.Miss) != !pile.HasValue)
      {
        throw new ArgumentException("Only a miss may omit the selected pile.", nameof(pile));
      }

      if (baseReactionDelayMicroseconds < GameRules.AiMinimumReactionMicroseconds
        || baseReactionDelayMicroseconds > GameRules.AiMaximumReactionMicroseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(baseReactionDelayMicroseconds));
      }

      if (stageMultiplierPercent < GameRules.AiStageThreeReactionMultiplierPercent
        || stageMultiplierPercent > GameRules.AiStageOneReactionMultiplierPercent)
      {
        throw new ArgumentOutOfRangeException(nameof(stageMultiplierPercent));
      }

      if (reactionDelayMicroseconds < GameRules.AiMinimumReactionMicroseconds
        || reactionDelayMicroseconds > GameRules.AiMaximumReactionMicroseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(reactionDelayMicroseconds));
      }

      Outcome = outcome;
      Pile = pile;
      BaseReactionDelayMicroseconds = baseReactionDelayMicroseconds;
      StageMultiplierPercent = stageMultiplierPercent;
      ReactionDelayMicroseconds = reactionDelayMicroseconds;
    }

    public AiBellOutcome Outcome { get; }
    public PileSide? Pile { get; }
    public long BaseReactionDelayMicroseconds { get; }
    public int StageMultiplierPercent { get; }
    public long ReactionDelayMicroseconds { get; }
  }
}
