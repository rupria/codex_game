using System;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliRemainingCardCountdownState
  {
    public const double PopInSeconds = 0.08d;
    public const double HoldSeconds = 0.20d;
    public const double FadeOutSeconds = 0.20d;
    public const double TotalDurationSeconds = PopInSeconds + HoldSeconds + FadeOutSeconds;

    private long _roundSeed = long.MinValue;
    private int _previousRemainingCards = -1;
    private double _startedAtSeconds = double.NegativeInfinity;

    public int ActiveValue { get; private set; }
    public int FrameIndex => 5 - ActiveValue;

    public bool Observe(long roundSeed, int remainingCards, double nowSeconds)
    {
      if (remainingCards < 0) throw new ArgumentOutOfRangeException(nameof(remainingCards));
      if (nowSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(nowSeconds));

      if (_roundSeed != roundSeed || _previousRemainingCards < 0)
      {
        _roundSeed = roundSeed;
        _previousRemainingCards = remainingCards;
        ActiveValue = 0;
        _startedAtSeconds = double.NegativeInfinity;
        return false;
      }

      var decreased = remainingCards < _previousRemainingCards;
      _previousRemainingCards = remainingCards;
      if (!decreased || remainingCards < 1 || remainingCards > 5) return false;

      ActiveValue = remainingCards;
      _startedAtSeconds = nowSeconds;
      return true;
    }

    public bool IsVisible(double nowSeconds)
    {
      const double epsilon = 0.000000001d;
      return ActiveValue >= 1
        && ActiveValue <= 5
        && ElapsedSeconds(nowSeconds) + epsilon < TotalDurationSeconds;
    }

    public double ElapsedSeconds(double nowSeconds)
    {
      if (nowSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(nowSeconds));
      return Math.Max(0d, nowSeconds - _startedAtSeconds);
    }

    public static float Scale(double elapsedSeconds)
    {
      if (elapsedSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
      if (elapsedSeconds >= PopInSeconds) return 1f;
      return (float)(0.72d + 0.28d * (elapsedSeconds / PopInSeconds));
    }

    public static float Alpha(double elapsedSeconds)
    {
      if (elapsedSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
      if (elapsedSeconds >= TotalDurationSeconds - 0.000000001d) return 0f;
      var fadeStartsAt = PopInSeconds + HoldSeconds;
      if (elapsedSeconds <= fadeStartsAt) return 1f;
      return (float)Math.Max(0d, 1d - (elapsedSeconds - fadeStartsAt) / FadeOutSeconds);
    }
  }
}
