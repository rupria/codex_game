using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Rewards
{
  public sealed class PredictionStreak
  {
    public int ActualSuccessCount { get; private set; }
    public int InsuredSuccessCount { get; private set; }
    public int InsuranceChargesRemaining { get; private set; }
    public bool InsuranceActivatedThisStage { get; private set; }
    public bool LastResultWasInsured { get; private set; }
    public int SuccessCount => RewardSuccessCount;
    public int RewardSuccessCount => Math.Min(
      GameRules.MaximumPredictionSuccessCount,
      ActualSuccessCount + InsuredSuccessCount);

    public bool CanActivateInsurance => !InsuranceActivatedThisStage;

    public bool ActivateInsurance()
    {
      if (!CanActivateInsurance) return false;
      InsuranceActivatedThisStage = true;
      InsuranceChargesRemaining = GameRules.PredictionInsuranceCharges;
      return true;
    }

    public bool Record(PredictionResult result)
    {
      if (result == null) return false;
      LastResultWasInsured = false;
      if (result.IsCorrect)
      {
        checked { ActualSuccessCount++; }
        return true;
      }
      if (InsuranceChargesRemaining <= 0) return false;
      InsuranceChargesRemaining--;
      checked { InsuredSuccessCount++; }
      LastResultWasInsured = true;
      return true;
    }

    public void Reset()
    {
      ResetStage();
    }

    public void ResetStage()
    {
      ActualSuccessCount = 0;
      InsuredSuccessCount = 0;
      InsuranceChargesRemaining = 0;
      InsuranceActivatedThisStage = false;
      LastResultWasInsured = false;
    }

    public PredictionRewardSnapshot GetSnapshot()
    {
      return new PredictionRewardSnapshot(
        ActualSuccessCount,
        InsuredSuccessCount,
        InsuranceChargesRemaining,
        RewardSuccessCount,
        InsuranceActivatedThisStage,
        LastResultWasInsured);
    }
  }
}
