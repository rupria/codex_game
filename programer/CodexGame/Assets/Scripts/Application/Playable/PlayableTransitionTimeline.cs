using System;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  internal sealed class PlayableTransitionTimeline
  {
    private GameTimestamp _startedAt;
    private long _durationMicroseconds;

    public PlayableTransitionKind Kind { get; private set; }
    public GameTimestamp EndsAt => new GameTimestamp(
      _startedAt.Microseconds + _durationMicroseconds);

    public void Begin(
      PlayableTransitionKind kind,
      GameTimestamp now,
      long durationMicroseconds)
    {
      if (kind == PlayableTransitionKind.None)
      {
        throw new ArgumentOutOfRangeException(nameof(kind));
      }
      if (durationMicroseconds <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(durationMicroseconds));
      }

      Kind = kind;
      _startedAt = now;
      _durationMicroseconds = durationMicroseconds;
    }

    public bool IsComplete(GameTimestamp now)
    {
      return Kind != PlayableTransitionKind.None
        && now.Microseconds - _startedAt.Microseconds >= _durationMicroseconds;
    }

    public PlayableTransitionSnapshot GetSnapshot(GameTimestamp now)
    {
      if (Kind == PlayableTransitionKind.None)
      {
        return new PlayableTransitionSnapshot(Kind, 0, 0f);
      }

      var elapsed = Math.Max(0, now.Microseconds - _startedAt.Microseconds);
      var remaining = Math.Max(0, _durationMicroseconds - elapsed);
      var progress = Math.Min(1f, (float)elapsed / _durationMicroseconds);
      return new PlayableTransitionSnapshot(Kind, remaining, progress);
    }

    public void Clear()
    {
      Kind = PlayableTransitionKind.None;
      _durationMicroseconds = 0;
    }
  }
}
