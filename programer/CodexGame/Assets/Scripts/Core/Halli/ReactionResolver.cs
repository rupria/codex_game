using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public static class ReactionResolver
  {
    public static ReactionWinner Resolve(GameTimestamp? player, GameTimestamp? ai)
    {
      return Resolve(
        player,
        ai,
        new DurationUs(GameRules.SimultaneousBellThresholdMicroseconds));
    }

    public static ReactionWinner Resolve(
      GameTimestamp? player,
      GameTimestamp? ai,
      DurationUs simultaneousThreshold)
    {
      if (!player.HasValue && !ai.HasValue)
      {
        return ReactionWinner.None;
      }

      if (!player.HasValue)
      {
        return ReactionWinner.Ai;
      }

      if (!ai.HasValue)
      {
        return ReactionWinner.Player;
      }

      var difference = player.Value.Microseconds - ai.Value.Microseconds;
      var threshold = simultaneousThreshold.Microseconds;

      if (difference >= -threshold && difference <= threshold)
      {
        return ReactionWinner.Player;
      }

      return difference < 0
        ? ReactionWinner.Player
        : ReactionWinner.Ai;
    }
  }
}
