using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public static class HalliStageRules
  {
    public static int GetWinTarget(int combatRoundNumber)
    {
      if (combatRoundNumber < 1)
      {
        throw new ArgumentOutOfRangeException(nameof(combatRoundNumber));
      }

      if (combatRoundNumber == 1)
      {
        return 3;
      }

      return 2;
    }

    public static HalliStageEndReason ResolveEndReason(
      int playerWins,
      int aiWins,
      int flipCount,
      int remainingDeckCards,
      int combatRoundNumber)
    {
      if (playerWins < 0 || aiWins < 0 || flipCount < 0 || remainingDeckCards < 0)
      {
        throw new ArgumentOutOfRangeException(
          nameof(playerWins),
          "Stage counters cannot be negative.");
      }

      var target = GetWinTarget(combatRoundNumber);

      if (playerWins >= target)
      {
        return HalliStageEndReason.PlayerTargetReached;
      }

      if (aiWins >= target)
      {
        return HalliStageEndReason.AiTargetReached;
      }

      if (flipCount >= GameRules.HalliFlipLimit)
      {
        return HalliStageEndReason.FlipLimitReached;
      }

      return remainingDeckCards == 0
        ? HalliStageEndReason.InsufficientCards
        : HalliStageEndReason.None;
    }
  }
}
