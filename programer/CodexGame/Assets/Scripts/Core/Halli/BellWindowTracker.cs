using System;

namespace CodexGame.Core.Halli
{
  public sealed class BellWindowTracker
  {
    private long _lastIssued;
    private BellWindowId? _current;

    public bool IsOpen => _current.HasValue;

    public BellWindowId OpenForCurrentField()
    {
      if (_lastIssued == long.MaxValue)
      {
        throw new InvalidOperationException("The bell-window sequence is exhausted.");
      }

      _lastIssued++;
      _current = new BellWindowId(_lastIssued);
      return _current.Value;
    }

    public void CloseForNextFlip()
    {
      _current = null;
    }

    public bool IsCurrent(BellWindowId windowId)
    {
      return windowId.IsValid
        && _current.HasValue
        && _current.Value == windowId;
    }
  }
}
