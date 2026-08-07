using System;

namespace CodexGame.Core.Halli
{
  public static class ReactionResolver
  {
    public static ReactionWinner Resolve(double? playerSeconds, double? aiSeconds)
    {
      ValidateTimestamp(playerSeconds, nameof(playerSeconds));
      ValidateTimestamp(aiSeconds, nameof(aiSeconds));

      if (!playerSeconds.HasValue && !aiSeconds.HasValue)
      {
        return ReactionWinner.None;
      }

      if (!playerSeconds.HasValue)
      {
        return ReactionWinner.Ai;
      }

      if (!aiSeconds.HasValue)
      {
        return ReactionWinner.Player;
      }

      return playerSeconds.Value <= aiSeconds.Value
        ? ReactionWinner.Player
        : ReactionWinner.Ai;
    }

    private static void ValidateTimestamp(double? seconds, string parameterName)
    {
      if (seconds.HasValue && (double.IsNaN(seconds.Value) || seconds.Value < 0.0))
      {
        throw new ArgumentOutOfRangeException(parameterName);
      }
    }
  }
}
