using System;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Distribution
{
  public static class PrivateCardDistributionRules
  {
    public static int GetDirectSelectionCount(int combatRoundNumber)
    {
      HalliStageRules.GetWinTarget(combatRoundNumber);
      return combatRoundNumber == 1 ? GameRules.RequiredPrivateCards : 2;
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

      var required = GetDirectSelectionCount(combatRoundNumber);
      return winnerCandidateCount > required
        ? required
        : winnerCandidateCount;
    }

    public static bool RequiresSelectionUi(int combatRoundNumber, int winnerCandidateCount)
    {
      return winnerCandidateCount > GetDirectSelectionCount(combatRoundNumber);
    }
  }
}
