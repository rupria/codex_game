using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Items
{
  public sealed class PredictionInsuranceActivationSnapshot
  {
    public static readonly PredictionInsuranceActivationSnapshot Inactive =
      new PredictionInsuranceActivationSnapshot(false, 1f, 0, 0, default, 0, 0);

    public PredictionInsuranceActivationSnapshot(
      bool isActive,
      float progress,
      long remainingMicroseconds,
      int recordSequence,
      PredictionChoice choice,
      int chargesBefore,
      int chargesAfter)
    {
      IsActive = isActive;
      Progress = progress;
      RemainingMicroseconds = remainingMicroseconds;
      RecordSequence = recordSequence;
      Choice = choice;
      ChargesBefore = chargesBefore;
      ChargesAfter = chargesAfter;
    }

    public bool IsActive { get; }
    public float Progress { get; }
    public long RemainingMicroseconds { get; }
    public int RecordSequence { get; }
    public PredictionChoice Choice { get; }
    public int ChargesBefore { get; }
    public int ChargesAfter { get; }
    public int DisplayedCharges => Progress * GameRules.PredictionInsuranceActivationPresentationMicroseconds
        < GameRules.PredictionInsuranceActivationChargeCommitMicroseconds
      ? ChargesBefore
      : ChargesAfter;
  }
}
