using System;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliRemainingCardCountdownState
  {
    public const double PopInSeconds = 0.08d;
    public const double HoldSeconds = 0.20d;
    public const double FadeOutSeconds = 0.20d;
    public const double TotalDurationSeconds = PopInSeconds + HoldSeconds + FadeOutSeconds;

    private long _entryToken = long.MinValue;
    private int _previousRemainingPlayerInputs = -1;
    private bool _wasActive;
    private double _startedAtSeconds = double.NegativeInfinity;

    public int ActiveValue { get; private set; }
    public int FrameIndex => 5 - ActiveValue;

    public bool Observe(
      long entryToken,
      int remainingPlayerInputs,
      bool revealCommitted,
      bool isThreeCallActive,
      double nowSeconds)
    {
      if (remainingPlayerInputs < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(remainingPlayerInputs));
      }
      if (nowSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(nowSeconds));

      if (_entryToken != entryToken || _previousRemainingPlayerInputs < 0)
      {
        _entryToken = entryToken;
        _previousRemainingPlayerInputs = remainingPlayerInputs;
        _wasActive = false;
        ActiveValue = 0;
        _startedAtSeconds = double.NegativeInfinity;
      }

      if (!isThreeCallActive)
      {
        _wasActive = false;
        ActiveValue = 0;
        _startedAtSeconds = double.NegativeInfinity;
        return false;
      }

      if (!_wasActive)
      {
        _wasActive = true;
        _previousRemainingPlayerInputs = remainingPlayerInputs;
        if (remainingPlayerInputs < 1 || remainingPlayerInputs > 5) return false;

        ActiveValue = remainingPlayerInputs;
        _startedAtSeconds = nowSeconds;
        return true;
      }

      // The count drops when the player input is accepted. Wait for that player's
      // face-up reveal to commit so the badge is not consumed by card motion.
      if (!revealCommitted) return false;

      var decreased = remainingPlayerInputs < _previousRemainingPlayerInputs;
      _previousRemainingPlayerInputs = remainingPlayerInputs;
      if (!decreased || remainingPlayerInputs < 1 || remainingPlayerInputs > 5)
      {
        return false;
      }

      ActiveValue = remainingPlayerInputs;
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
