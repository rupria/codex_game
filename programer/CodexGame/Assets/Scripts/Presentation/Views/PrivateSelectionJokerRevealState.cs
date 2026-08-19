using System;

namespace CodexGame.Presentation.Views
{
  public enum PrivateSelectionJokerRevealStep
  {
    None = 0,
    Focus = 1,
    Flip = 2,
    Accent = 3,
    Settle = 4
  }

  public sealed class PrivateSelectionJokerRevealState
  {
    private const double BoundaryEpsilon = 0.000000001d;
    public const double FocusEndSeconds = 0.15d;
    public const double FlipEndSeconds = 0.35d;
    public const double InputUnlockSeconds = 0.85d;
    public const double TotalSeconds = 1d;

    private long _sessionKey = long.MinValue;
    private double _startedAtSeconds;
    private bool _hasJoker;
    private bool _completed;

    public void Observe(long sessionKey, bool hasJoker, double nowSeconds)
    {
      if (_sessionKey != sessionKey)
      {
        _sessionKey = sessionKey;
        _hasJoker = hasJoker;
        _completed = !hasJoker;
        _startedAtSeconds = nowSeconds;
      }

      if (_hasJoker
        && !_completed
        && ElapsedSeconds(nowSeconds) + BoundaryEpsilon >= TotalSeconds)
      {
        _completed = true;
      }
    }

    public bool IsInputLocked(double nowSeconds)
    {
      return _hasJoker
        && !_completed
        && ElapsedSeconds(nowSeconds) + BoundaryEpsilon < InputUnlockSeconds;
    }

    public bool IsActive(double nowSeconds)
    {
      return _hasJoker
        && !_completed
        && ElapsedSeconds(nowSeconds) + BoundaryEpsilon < TotalSeconds;
    }

    public double ElapsedSeconds(double nowSeconds)
    {
      return Math.Max(0d, nowSeconds - _startedAtSeconds);
    }

    public PrivateSelectionJokerRevealStep Step(double nowSeconds)
    {
      if (!IsActive(nowSeconds)) return PrivateSelectionJokerRevealStep.None;
      var elapsed = ElapsedSeconds(nowSeconds);
      if (elapsed + BoundaryEpsilon < FocusEndSeconds)
      {
        return PrivateSelectionJokerRevealStep.Focus;
      }
      if (elapsed + BoundaryEpsilon < FlipEndSeconds)
      {
        return PrivateSelectionJokerRevealStep.Flip;
      }
      if (elapsed + BoundaryEpsilon < InputUnlockSeconds)
      {
        return PrivateSelectionJokerRevealStep.Accent;
      }
      return PrivateSelectionJokerRevealStep.Settle;
    }
  }
}
