using CodexGame.Core.Cards;

namespace CodexGame.Application.Items
{
  public enum BottomDealAuditOutcome
  {
    Entered = 0,
    Cancelled = 1,
    Confirmed = 2,
    TimedOut = 3
  }

  public sealed class BottomDealAuditEntry
  {
    public BottomDealAuditEntry(
      long timestampMicroseconds,
      long remainingMicroseconds,
      CardId targetCardId,
      BottomDealAuditOutcome outcome)
    {
      TimestampMicroseconds = timestampMicroseconds;
      RemainingMicroseconds = remainingMicroseconds;
      TargetCardId = targetCardId;
      Outcome = outcome;
    }

    public long TimestampMicroseconds { get; }
    public long RemainingMicroseconds { get; }
    public CardId TargetCardId { get; }
    public BottomDealAuditOutcome Outcome { get; }
  }
}
