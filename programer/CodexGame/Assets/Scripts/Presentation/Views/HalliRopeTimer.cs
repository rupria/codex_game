using CodexGame.Application.Playable;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliRopeTimer
  {
    private const float ExplosionDuration = 0.7f;
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

      if (state.Mode == RopeTimerMode.Exploding)
      {
        DrawExplosion(art?.RopeExplosion);
        return;
      }

      var ropeRect = new Rect(332f, 147f, 258f, 10f);
      var filledRect = new Rect(ropeRect.x, ropeRect.y, ropeRect.width * state.RemainingRatio, ropeRect.height);
      var previousColor = GUI.color;
      GUI.color = new Color(0.11f, 0.075f, 0.04f, 0.92f);
      GUI.DrawTexture(ropeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      var pulse = state.Mode == RopeTimerMode.Urgent
        ? 0.78f + Mathf.Sin(Time.unscaledTime * 13f) * 0.2f
        : 1f;
      GUI.color = state.Mode == RopeTimerMode.Urgent
        ? new Color(0.95f, 0.3f * pulse, 0.08f, 1f)
        : new Color(0.68f, 0.43f, 0.18f, 1f);
      GUI.DrawTexture(
        filledRect,
        art?.RopeBody != null ? art.RopeBody : Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      if (art?.RopeBody == null) DrawKnots(ropeRect, state.RemainingRatio);
      GUI.color = state.Mode == RopeTimerMode.Urgent
        ? new Color(1f, 0.7f, 0.1f, pulse)
        : new Color(1f, 0.52f, 0.08f, 1f);
      var flameSize = state.Mode == RopeTimerMode.Urgent ? 24f + 5f * pulse : 18f;
      GUI.DrawTexture(
        new Rect(filledRect.xMax - flameSize * 0.5f, ropeRect.center.y - flameSize * 0.5f, flameSize, flameSize),
        art?.RopeFlame != null ? art.RopeFlame : Texture2D.whiteTexture,
        ScaleMode.ScaleToFit,
        true);
      GUI.color = previousColor;
      GUI.Label(new Rect(596f, 140f, 58f, 24f), state.DisplayedSeconds + "s", styles.Small);
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

    private void DrawExplosion(Texture2D explosionTexture)
    {
      var elapsed = Mathf.Clamp01((Time.unscaledTime - _explosionStartedAt) / ExplosionDuration);
      var radius = Mathf.Lerp(18f, 78f, elapsed);
      var alpha = 1f - elapsed;
      var center = new Vector2(480f, 154f);
      var previousColor = GUI.color;
      GUI.color = new Color(1f, 0.32f, 0.04f, alpha);
      GUI.DrawTexture(
        new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f),
        explosionTexture != null ? explosionTexture : Texture2D.whiteTexture,
        ScaleMode.ScaleToFit,
        true);
      if (explosionTexture == null)
      {
        GUI.color = new Color(1f, 0.84f, 0.22f, alpha);
        GUI.DrawTexture(
          new Rect(center.x - radius * 0.45f, center.y - radius * 0.45f, radius * 0.9f, radius * 0.9f),
          Texture2D.whiteTexture,
          ScaleMode.ScaleToFit,
          true);
      }
      GUI.color = previousColor;
    }
  }
}
