using System;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Distribution
{
  public static class PrivateCardDistributionRules
  {
    public static int GetDirectSelectionCount(int combatRoundNumber)
    {
      return HalliStageRules.GetWinTarget(combatRoundNumber);
    }

    public static int GetWinnerRandomFillCount(int combatRoundNumber)
    {
      return GameRules.RequiredPrivateCards - GetDirectSelectionCount(combatRoundNumber);
    }

    public static int GetAvailableDirectSelectionCount(
      int combatRoundNumber,
      int winnerCandidateCount)
    {
      if (winnerCandidateCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(winnerCandidateCount));
      }

      return Math.Min(GetDirectSelectionCount(combatRoundNumber), winnerCandidateCount);
    }

    public static bool RequiresSelectionUi(int combatRoundNumber, int winnerCandidateCount)
    {
      var required = GetAvailableDirectSelectionCount(combatRoundNumber, winnerCandidateCount);

      return winnerCandidateCount > required;
    }
  }
}
