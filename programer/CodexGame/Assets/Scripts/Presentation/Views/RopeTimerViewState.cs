using System;

namespace CodexGame.Presentation.Views
{
  internal enum RopeTimerMode
  {
    Hidden,
    Normal,
    Urgent,
    Exploding
  }

  internal readonly struct RopeTimerViewState
  {
    private RopeTimerViewState(RopeTimerMode mode, float remainingRatio, int displayedSeconds)
    {
      Mode = mode;
      RemainingRatio = remainingRatio;
      DisplayedSeconds = displayedSeconds;
    }

    public RopeTimerMode Mode { get; }
    public float RemainingRatio { get; }
    public int DisplayedSeconds { get; }
    public bool IsVisible => Mode != RopeTimerMode.Hidden;

    public static RopeTimerViewState Create(
      bool bellTimerActive,
      long remainingMicroseconds,
      long timeoutMicroseconds,
      bool exploding)
    {
      if (timeoutMicroseconds <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutMicroseconds));
      if (exploding) return new RopeTimerViewState(RopeTimerMode.Exploding, 0f, 0);
      if (!bellTimerActive) return new RopeTimerViewState(RopeTimerMode.Hidden, 0f, 0);

      var clamped = Math.Max(0, Math.Min(timeoutMicroseconds, remainingMicroseconds));
      var ratio = (float)clamped / timeoutMicroseconds;
      var seconds = (int)Math.Ceiling(clamped / 1_000_000d);
      var mode = seconds <= 10 ? RopeTimerMode.Urgent : RopeTimerMode.Normal;
      return new RopeTimerViewState(mode, ratio, seconds);
    }

    public static int LoopingFrame(
      double elapsedSeconds,
      double frameDurationSeconds,
      int frameCount)
    {
      if (elapsedSeconds < 0) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
      if (frameDurationSeconds <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(frameDurationSeconds));
      }
      if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
      return (int)Math.Floor(elapsedSeconds / frameDurationSeconds) % frameCount;
    }

    public static int OneShotFrame(
      double elapsedSeconds,
      double durationSeconds,
      int frameCount)
    {
      if (elapsedSeconds < 0) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
      if (durationSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
      if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
      var progress = Math.Min(1d, elapsedSeconds / durationSeconds);
      return Math.Min(frameCount - 1, (int)Math.Floor(progress * frameCount));
    }

    public static double RopeTipX(double ropeX, double ropeWidth, double remainingRatio)
    {
      if (ropeWidth < 0d) throw new ArgumentOutOfRangeException(nameof(ropeWidth));
      if (remainingRatio < 0d || remainingRatio > 1d)
      {
        throw new ArgumentOutOfRangeException(nameof(remainingRatio));
      }
      return ropeX + ropeWidth * remainingRatio;
    }
  }
}
