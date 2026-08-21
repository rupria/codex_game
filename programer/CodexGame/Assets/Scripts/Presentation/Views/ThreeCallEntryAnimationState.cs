using System;
using CodexGame.Core.Shared;

namespace CodexGame.Presentation.Views
{
  internal readonly struct ThreeCallEntryAnimationFrame
  {
    public ThreeCallEntryAnimationFrame(
      float scale,
      float alpha,
      int bellFrameIndex,
      bool showTitle)
    {
      Scale = scale;
      Alpha = alpha;
      BellFrameIndex = bellFrameIndex;
      ShowTitle = showTitle;
    }

    public float Scale { get; }
    public float Alpha { get; }
    public int BellFrameIndex { get; }
    public bool ShowTitle { get; }
  }

  internal static class ThreeCallEntryAnimationState
  {
    private const float CompactScale = 0.5f;
    private const float CompactEndSeconds = 0.12f;
    private const float ScaleInEndSeconds = 0.32f;
    private const float FadeStartSeconds = 1.25f;
    private const float TotalSeconds =
      GameRules.ThreeCallEntryPresentationMicroseconds / 1_000_000f;
    private const int BellFrameCount = 8;

    public static ThreeCallEntryAnimationFrame Evaluate(float progress)
    {
      var normalized = Clamp01(progress);
      var elapsed = normalized * TotalSeconds;
      var scale = CompactScale;
      if (elapsed > CompactEndSeconds)
      {
        var scaleProgress = Clamp01(
          (elapsed - CompactEndSeconds) / (ScaleInEndSeconds - CompactEndSeconds));
        scale = CompactScale + (1f - CompactScale) * scaleProgress;
      }

      var alpha = elapsed <= FadeStartSeconds
        ? 1f
        : 1f - Clamp01((elapsed - FadeStartSeconds) / (TotalSeconds - FadeStartSeconds));
      var bellFrameIndex = Math.Min(
        BellFrameCount - 1,
        (int)Math.Floor(normalized * BellFrameCount));
      return new ThreeCallEntryAnimationFrame(
        scale,
        alpha,
        bellFrameIndex,
        elapsed >= CompactEndSeconds);
    }

    private static float Clamp01(float value)
    {
      return Math.Max(0f, Math.Min(1f, value));
    }
  }
}

