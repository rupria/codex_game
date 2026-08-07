using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public readonly struct HalliAiBellDecision
  {
    public HalliAiBellDecision(
      AiBellOutcome outcome,
      PileSide? pile,
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

      if (reactionDelayMicroseconds < GameRules.AiMinimumReactionMicroseconds
        || reactionDelayMicroseconds > GameRules.AiMaximumReactionMicroseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(reactionDelayMicroseconds));
      }

      Outcome = outcome;
      Pile = pile;
      ReactionDelayMicroseconds = reactionDelayMicroseconds;
    }

    public AiBellOutcome Outcome { get; }
    public PileSide? Pile { get; }
    public long ReactionDelayMicroseconds { get; }
  }
}
