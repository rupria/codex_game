using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Rewards
{
  public sealed class PredictionRewardSnapshot
  {
    public PredictionRewardSnapshot(
      int actualSuccessCount,
      int insuredSuccessCount,
      int insuranceChargesRemaining,
      int rewardSuccessCount,
      bool insuranceActivatedThisStage,
      bool lastResultWasInsured)
    {
      if (actualSuccessCount < 0) throw new ArgumentOutOfRangeException(nameof(actualSuccessCount));
      if (insuredSuccessCount < 0) throw new ArgumentOutOfRangeException(nameof(insuredSuccessCount));
      if (insuranceChargesRemaining < 0
        || insuranceChargesRemaining > GameRules.PredictionInsuranceCharges)
      {
        throw new ArgumentOutOfRangeException(nameof(insuranceChargesRemaining));
      }
      if (rewardSuccessCount < 0
        || rewardSuccessCount > GameRules.MaximumPredictionSuccessCount)
      {
        throw new ArgumentOutOfRangeException(nameof(rewardSuccessCount));
      }
      ActualSuccessCount = actualSuccessCount;
      InsuredSuccessCount = insuredSuccessCount;
      InsuranceChargesRemaining = insuranceChargesRemaining;
      RewardSuccessCount = rewardSuccessCount;
      InsuranceActivatedThisStage = insuranceActivatedThisStage;
      LastResultWasInsured = lastResultWasInsured;
    }

    public int ActualSuccessCount { get; }
    public int InsuredSuccessCount { get; }
    public int InsuranceChargesRemaining { get; }
    public int RewardSuccessCount { get; }
    public bool InsuranceActivatedThisStage { get; }
    public bool LastResultWasInsured { get; }
  }
}
