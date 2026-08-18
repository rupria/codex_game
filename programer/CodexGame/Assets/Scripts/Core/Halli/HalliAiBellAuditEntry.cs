using System;

namespace CodexGame.Core.Halli
{
  public sealed class HalliAiBellAuditEntry
  {
    public HalliAiBellAuditEntry(
      int sequence,
      int stageNumber,
      int combatRoundNumber,
      long fieldOpenedAtMicroseconds,
      long baseReactionMicroseconds,
      int stageMultiplierPercent,
      long finalReactionMicroseconds,
      AiBellOutcome plannedOutcome,
      HalliAiBellResolution resolution,
      bool aiInputWasFirst)
    {
      if (sequence <= 0) throw new ArgumentOutOfRangeException(nameof(sequence));
      if (stageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(stageNumber));
      if (combatRoundNumber <= 0) throw new ArgumentOutOfRangeException(nameof(combatRoundNumber));
      if (fieldOpenedAtMicroseconds < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(fieldOpenedAtMicroseconds));
      }
      if (!Enum.IsDefined(typeof(AiBellOutcome), plannedOutcome))
      {
        throw new ArgumentOutOfRangeException(nameof(plannedOutcome));
      }
      if (!Enum.IsDefined(typeof(HalliAiBellResolution), resolution))
      {
        throw new ArgumentOutOfRangeException(nameof(resolution));
      }
      Sequence = sequence;
      StageNumber = stageNumber;
      CombatRoundNumber = combatRoundNumber;
      FieldOpenedAtMicroseconds = fieldOpenedAtMicroseconds;
      BaseReactionMicroseconds = baseReactionMicroseconds;
      StageMultiplierPercent = stageMultiplierPercent;
      FinalReactionMicroseconds = finalReactionMicroseconds;
      PlannedOutcome = plannedOutcome;
      Resolution = resolution;
      AiInputWasFirst = aiInputWasFirst;
    }

    public int Sequence { get; }
    public int StageNumber { get; }
    public int CombatRoundNumber { get; }
    public long FieldOpenedAtMicroseconds { get; }
    public long BaseReactionMicroseconds { get; }
    public int StageMultiplierPercent { get; }
    public long FinalReactionMicroseconds { get; }
    public AiBellOutcome PlannedOutcome { get; }
    public HalliAiBellResolution Resolution { get; }
    public bool AiInputWasFirst { get; }
  }
}
