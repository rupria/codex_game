using System;
using CodexGame.Core.Battle;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Distribution
{
  public static class PrivateCardDistributionRules
  {
    public static int GetDirectSelectionCount(int combatRoundNumber)
    {
      HalliStageRules.GetWinTarget(combatRoundNumber);
      return GameRules.RequiredPrivateCards;
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

      return winnerCandidateCount > GameRules.RequiredPrivateCards
        ? GameRules.RequiredPrivateCards
        : winnerCandidateCount;
    }

    public static bool RequiresSelectionUi(int combatRoundNumber, int winnerCandidateCount)
    {
      GetDirectSelectionCount(combatRoundNumber);
      return winnerCandidateCount > GameRules.RequiredPrivateCards;
    }

    public static bool IsPairAssistEnabled(BattleHealth health)
    {
      return !health.IsBattleOver
        && health.Player + health.Ai >= GameRules.PairAssistMinimumCombinedHealth;
    }
  }
}
