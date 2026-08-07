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

    public static bool RequiresSelectionUi(int combatRoundNumber, int winnerCandidateCount)
    {
      if (winnerCandidateCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(winnerCandidateCount));
      }

      var required = GetDirectSelectionCount(combatRoundNumber);

      if (winnerCandidateCount < required)
      {
        throw new ArgumentException(
          "The Halli winner must have enough acquired cards for the round's direct selection.",
          nameof(winnerCandidateCount));
      }

      return winnerCandidateCount > required;
    }
  }
}
