#nullable enable
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
    public PredictionRecordAuditEntry? LastRecord { get; private set; }
    private int _recordSequence;
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
      return RecordWithAudit(result)?.CountedAsSuccess == true;
    }

    public PredictionRecordAuditEntry? RecordWithAudit(PredictionResult result)
    {
      if (result == null) return null;
      var chargesBefore = InsuranceChargesRemaining;
      LastResultWasInsured = false;
      var actual = false;
      var insured = false;
      if (result.IsCorrect)
      {
        checked { ActualSuccessCount++; }
        actual = true;
      }
      else if (InsuranceChargesRemaining > 0)
      {
        InsuranceChargesRemaining--;
        checked { InsuredSuccessCount++; }
        LastResultWasInsured = true;
        insured = true;
      }
      checked { _recordSequence++; }
      LastRecord = new PredictionRecordAuditEntry(
        _recordSequence,
        result.Choice,
        actual,
        insured,
        chargesBefore,
        InsuranceChargesRemaining);
      return LastRecord;
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
      LastRecord = null;
      _recordSequence = 0;
    }

    public PredictionRewardSnapshot GetSnapshot()
    {
      return new PredictionRewardSnapshot(
        ActualSuccessCount,
        InsuredSuccessCount,
        InsuranceChargesRemaining,
        RewardSuccessCount,
        InsuranceActivatedThisStage,
        LastResultWasInsured,
        LastRecord);
    }
  }
}
