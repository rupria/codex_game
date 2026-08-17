using System;
using CodexGame.Core.Items;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Items
{
  public sealed class ItemUsePresentationSession
  {
    private GameItemId? _itemId;
    private GameTimestamp _startedAt;
    private long _durationMicroseconds;

    public bool IsActive => _itemId.HasValue;

    public void Begin(GameItemId itemId, GameTimestamp now)
    {
      if (IsActive) throw new InvalidOperationException("An item presentation is already active.");
      _itemId = itemId;
      _startedAt = now;
      _durationMicroseconds = DurationFor(itemId);
    }

    public bool Tick(GameTimestamp now)
    {
      if (!IsActive || now.Microseconds - _startedAt.Microseconds < _durationMicroseconds)
      {
        return false;
      }
      Reset();
      return true;
    }

    public ItemUsePresentationSnapshot GetSnapshot(GameTimestamp now)
    {
      if (!IsActive) return ItemUsePresentationSnapshot.Inactive;
      var elapsed = Math.Max(0, now.Microseconds - _startedAt.Microseconds);
      var remaining = Math.Max(0, _durationMicroseconds - elapsed);
      var progress = _durationMicroseconds == 0
        ? 1f
        : (float)Math.Min(1d, (double)elapsed / _durationMicroseconds);
      return new ItemUsePresentationSnapshot(true, _itemId, progress, remaining);
    }

    public void Reset()
    {
      _itemId = null;
      _durationMicroseconds = 0;
    }

    public static long DurationFor(GameItemId itemId)
    {
      switch (itemId)
      {
        case GameItemId.Reload: return GameRules.ReloadItemPresentationMicroseconds;
        case GameItemId.BottomDeal: return GameRules.BottomDealItemPresentationMicroseconds;
        case GameItemId.HypeMan: return GameRules.HypeManItemPresentationMicroseconds;
        case GameItemId.HealthRecovery: return GameRules.HealthRecoveryItemPresentationMicroseconds;
        default: throw new ArgumentOutOfRangeException(nameof(itemId));
      }
    }
  }
}
