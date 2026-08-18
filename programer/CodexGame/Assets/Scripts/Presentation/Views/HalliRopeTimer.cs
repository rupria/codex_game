using CodexGame.Application.Playable;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliRopeTimer
  {
    private const float ExplosionDuration = 0.7f;
    private const int FlameFrameCount = 6;
    private const int ExplosionFrameCount = 8;
    private bool _wasTimeoutReview;
    private float _explosionStartedAt = float.NegativeInfinity;

    public void Draw(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      HalliUiArtSet art)
    {
      var timeoutReview = snapshot.Phase == PrototypeSessionPhase.Review
        && snapshot.Status.Key == "STATUS_HALLI_FLIP_TIMEOUT";
      if (timeoutReview && !_wasTimeoutReview) _explosionStartedAt = Time.unscaledTime;
      _wasTimeoutReview = timeoutReview;

      var exploding = Time.unscaledTime - _explosionStartedAt < ExplosionDuration;
      var timerActive = snapshot.CanRing
        && (snapshot.Phase == PrototypeSessionPhase.ReadyToFlip
          || snapshot.Phase == PrototypeSessionPhase.SequentialReveal
          || snapshot.Phase == PrototypeSessionPhase.BellOpen);
      var state = RopeTimerViewState.Create(
        timerActive,
        snapshot.RemainingMicroseconds,
        GameRules.BellInputTimeoutMicroseconds,
        exploding);
      if (!state.IsVisible) return;

      var ropeRect = new Rect(332f, 144f, 258f, 16f);

      if (state.Mode == RopeTimerMode.Exploding)
      {
        DrawExplosion(ropeRect, art?.RopeExplosion, art?.RopeScorch);
        return;
      }

      var filledRect = new Rect(ropeRect.x, ropeRect.y, ropeRect.width * state.RemainingRatio, ropeRect.height);
      var ropeTipX = (float)RopeTimerViewState.RopeTipX(
        ropeRect.x,
        ropeRect.width,
        state.RemainingRatio);
      var previousColor = GUI.color;
      GUI.color = new Color(0.11f, 0.075f, 0.04f, 0.92f);
      GUI.DrawTexture(ropeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      var pulse = state.Mode == RopeTimerMode.Urgent
        ? 0.78f + Mathf.Sin(Time.unscaledTime * 13f) * 0.2f
        : 1f;
      if (art?.RopeBody != null)
      {
        GUI.color = Color.white;
        GUI.DrawTextureWithTexCoords(
          filledRect,
          art.RopeBody,
          new Rect(0f, 0f, state.RemainingRatio, 1f),
          true);
      }
      else
      {
        GUI.color = state.Mode == RopeTimerMode.Urgent
          ? new Color(0.95f, 0.3f * pulse, 0.08f, 1f)
          : new Color(0.68f, 0.43f, 0.18f, 1f);
        GUI.DrawTexture(filledRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        DrawKnots(ropeRect, state.RemainingRatio);
      }
      if (art?.RopeCharCap != null)
      {
        GUI.color = Color.white;
        GUI.DrawTexture(
          new Rect(filledRect.xMax - 12f, ropeRect.y, 24f, 16f),
          art.RopeCharCap,
          ScaleMode.StretchToFill,
          true);
      }
      GUI.color = Color.white;
      var flameRect = new Rect(
        ropeTipX - 14f,
        ropeRect.center.y - 24f,
        48f,
        48f);
      if (art?.RopeFlame != null)
      {
        var frame = RopeTimerViewState.LoopingFrame(
          Time.unscaledTime,
          0.08f,
          FlameFrameCount);
        GUI.DrawTextureWithTexCoords(
          flameRect,
          art.RopeFlame,
          new Rect((float)frame / FlameFrameCount, 0f, 1f / FlameFrameCount, 1f),
          true);
      }
      else
      {
        GUI.DrawTexture(flameRect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true);
      }
      GUI.color = previousColor;
      GUI.Label(new Rect(596f, 141f, 58f, 24f), state.DisplayedSeconds + "s", styles.Small);
    }

    private static void DrawKnots(Rect ropeRect, float remainingRatio)
    {
      var previousColor = GUI.color;
      GUI.color = new Color(0.25f, 0.13f, 0.055f, 1f);
      var visibleWidth = ropeRect.width * remainingRatio;
      for (var x = 10f; x < visibleWidth; x += 18f)
      {
        GUI.DrawTexture(
          new Rect(ropeRect.x + x, ropeRect.y + 2f, 3f, ropeRect.height - 4f),
          Texture2D.whiteTexture,
          ScaleMode.StretchToFill,
          true);
      }
      GUI.color = previousColor;
    }

    private void DrawExplosion(Rect ropeRect, Texture2D explosionTexture, Texture2D scorchTexture)
    {
      var elapsed = Mathf.Clamp01((Time.unscaledTime - _explosionStartedAt) / ExplosionDuration);
      var terminal = new Vector2(ropeRect.x, ropeRect.center.y);
      var previousColor = GUI.color;
      GUI.color = Color.white;
      if (scorchTexture != null)
      {
        GUI.DrawTexture(
          new Rect(terminal.x - 10f, terminal.y - 12f, 32f, 24f),
          scorchTexture,
          ScaleMode.ScaleToFit,
          true);
      }
      var rect = new Rect(terminal.x - 24f, terminal.y - 48f, 96f, 96f);
      if (explosionTexture != null)
      {
        var frame = RopeTimerViewState.OneShotFrame(
          Time.unscaledTime - _explosionStartedAt,
          ExplosionDuration,
          ExplosionFrameCount);
        GUI.DrawTextureWithTexCoords(
          rect,
          explosionTexture,
          new Rect((float)frame / ExplosionFrameCount, 0f, 1f / ExplosionFrameCount, 1f),
          true);
      }
      else
      {
        GUI.color = new Color(1f, 0.84f, 0.22f, 1f - elapsed);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true);
      }
      GUI.color = previousColor;
    }
  }
}
