namespace CodexGame.Core.Rewards
{
  public sealed class PredictionRecordAuditEntry
  {
    public PredictionRecordAuditEntry(
      int sequence,
      PredictionChoice choice,
      bool wasActualSuccess,
      bool wasInsuredSuccess,
      int insuranceChargesBefore,
      int insuranceChargesAfter)
    {
      Sequence = sequence;
      Choice = choice;
      WasActualSuccess = wasActualSuccess;
      WasInsuredSuccess = wasInsuredSuccess;
      InsuranceChargesBefore = insuranceChargesBefore;
      InsuranceChargesAfter = insuranceChargesAfter;
    }

    public int Sequence { get; }
    public PredictionChoice Choice { get; }
    public bool WasActualSuccess { get; }
    public bool WasInsuredSuccess { get; }
    public bool CountedAsSuccess => WasActualSuccess || WasInsuredSuccess;
    public int InsuranceChargesBefore { get; }
    public int InsuranceChargesAfter { get; }
  }
}
