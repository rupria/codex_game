using System;

namespace CodexGame.Presentation.Views
{
  internal sealed class FlipReadyPromptState
  {
    public const double VisibleSeconds = 0.85d;
    private bool _wasEnabled;
    private double _startedAtSeconds = double.NegativeInfinity;

    public void Observe(bool enabled, double nowSeconds)
    {
      if (nowSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(nowSeconds));
      if (enabled && !_wasEnabled) _startedAtSeconds = nowSeconds;
      if (!enabled) _startedAtSeconds = double.NegativeInfinity;
      _wasEnabled = enabled;
    }

    public void Dismiss()
    {
      _startedAtSeconds = double.NegativeInfinity;
    }

    public bool IsVisible(double nowSeconds)
    {
      if (nowSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(nowSeconds));
      return _wasEnabled
        && nowSeconds - _startedAtSeconds >= 0d
        && nowSeconds - _startedAtSeconds < VisibleSeconds;
    }

    public float Alpha(double nowSeconds)
    {
      if (!IsVisible(nowSeconds)) return 0f;
      var elapsed = nowSeconds - _startedAtSeconds;
      const double fadeSeconds = 0.20d;
      if (elapsed < fadeSeconds) return (float)(elapsed / fadeSeconds);
      if (elapsed > VisibleSeconds - fadeSeconds)
      {
        return (float)((VisibleSeconds - elapsed) / fadeSeconds);
      }
      return 1f;
    }
  }
}
