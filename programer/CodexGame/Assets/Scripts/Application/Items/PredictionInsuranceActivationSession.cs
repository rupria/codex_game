#nullable enable
using System;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Items
{
  public sealed class PredictionInsuranceActivationSession
  {
    private PredictionRecordAuditEntry? _record;
    private GameTimestamp _startedAt;

    public bool IsActive => _record != null;

    public void Begin(PredictionRecordAuditEntry record, GameTimestamp now)
    {
      if (record == null) throw new ArgumentNullException(nameof(record));
      if (!record.WasInsuredSuccess)
      {
        throw new ArgumentException("Only an insured result can start insurance activation.", nameof(record));
      }
      _record = record;
      _startedAt = now;
    }

    public bool Tick(GameTimestamp now)
    {
      if (!IsActive
        || now.Microseconds - _startedAt.Microseconds
          < GameRules.PredictionInsuranceActivationPresentationMicroseconds)
      {
        return false;
      }
      Reset();
      return true;
    }

    public PredictionInsuranceActivationSnapshot GetSnapshot(GameTimestamp now)
    {
      if (_record == null) return PredictionInsuranceActivationSnapshot.Inactive;
      var duration = GameRules.PredictionInsuranceActivationPresentationMicroseconds;
      var elapsed = Math.Max(0, now.Microseconds - _startedAt.Microseconds);
      var remaining = Math.Max(0, duration - elapsed);
      var progress = duration == 0
        ? 1f
        : (float)Math.Min(1d, (double)elapsed / duration);
      return new PredictionInsuranceActivationSnapshot(
        true,
        progress,
        remaining,
        _record.Sequence,
        _record.Choice,
        _record.InsuranceChargesBefore,
        _record.InsuranceChargesAfter);
    }

    public void Reset()
    {
      _record = null;
    }
  }
}
