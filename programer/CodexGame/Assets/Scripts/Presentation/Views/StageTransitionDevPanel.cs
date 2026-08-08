using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class StageTransitionDevPanel
  {
    private static readonly Rect FullScreen = new Rect(0f, 0f, 960f, 540f);
    private static readonly Rect LeftDoor = new Rect(368f, 105f, 128f, 210f);
    private static readonly Rect RightDoor = new Rect(464f, 105f, 128f, 210f);
    private static readonly Rect Dust = new Rect(432f, 446f, 96f, 64f);
    private static readonly Rect Loading = new Rect(448f, 238f, 64f, 64f);

    public void Draw(
      NextStageTransitionSnapshot snapshot,
      StageTransitionUiArtSet art,
      BarShopUiArtSet barShopArt)
    {
      if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
      if (art == null || !art.IsComplete)
      {
        DrawBlack(null, 1f);
        return;
      }

      switch (snapshot.Step)
      {
        case NextStageTransitionStep.ShopUiClear:
          DrawShopClear(snapshot.StepProgress, art, barShopArt);
          break;
        case NextStageTransitionStep.CameraTurnToExit:
          DrawCameraTurn(snapshot.StepProgress, art, barShopArt);
          break;
        case NextStageTransitionStep.WalkToDoor:
          DrawWalk(snapshot.StepProgress, art);
          break;
        case NextStageTransitionStep.PushSwingDoors:
          DrawDoorOpening(snapshot.StepProgress, art);
          break;
        case NextStageTransitionStep.CrossThreshold:
          DrawThresholdCross(snapshot.StepProgress, art);
          break;
        case NextStageTransitionStep.FadeOutAndBeginLoad:
          DrawFadeOut(snapshot.StepProgress, art);
          break;
        case NextStageTransitionStep.LoadingLoop:
          DrawLoading(snapshot, art);
          break;
        case NextStageTransitionStep.NextStageFadeIn:
          DrawBlack(art.FadeBlack, 1f - Smooth(snapshot.StepProgress));
          break;
        case NextStageTransitionStep.Complete:
        case NextStageTransitionStep.Inactive:
          break;
      }
    }

    private static void DrawShopClear(
      float progress,
      StageTransitionUiArtSet art,
      BarShopUiArtSet barShopArt)
    {
      var shop = barShopArt?.Background;
      if (shop == null)
      {
        DrawFullScreen(art.ExitClosedBackground, 1f);
        return;
      }

      DrawFullScreen(shop, 1f);
      DrawFullScreen(art.ExitClosedBackground, Smooth(progress) * 0.25f);
    }

    private static void DrawCameraTurn(
      float progress,
      StageTransitionUiArtSet art,
      BarShopUiArtSet barShopArt)
    {
      var eased = Smooth(progress);
      if (barShopArt?.Background != null)
      {
        DrawFullScreen(barShopArt.Background, 1f - eased);
      }
      var offset = Mathf.Lerp(76f, 0f, eased);
      var scale = Mathf.Lerp(1.08f, 1f, eased);
      DrawZoomed(art.ExitClosedBackground, scale, offset, 0f, eased);
    }

    private static void DrawWalk(float progress, StageTransitionUiArtSet art)
    {
      var eased = Smooth(progress);
      var bob = Mathf.Sin(progress * Mathf.PI * 3f) * 6f * (1f - progress * 0.35f);
      DrawZoomed(
        art.ExitClosedBackground,
        Mathf.Lerp(1f, 1.16f, eased),
        0f,
        bob,
        1f);

      var vignetteAlpha = progress < 0.55f
        ? Mathf.Lerp(0f, 0.35f, progress / 0.55f)
        : Mathf.Lerp(0.35f, 0.15f, (progress - 0.55f) / 0.45f);
      DrawFullScreen(art.Vignette, vignetteAlpha);

      var dustIndex = Mathf.Min(
        art.DustFrames.Length - 1,
        Mathf.FloorToInt(progress * art.DustFrames.Length));
      DrawTexture(Dust, art.DustFrames[dustIndex], Mathf.Sin(progress * Mathf.PI) * 0.45f);
    }

    private static void DrawDoorOpening(float progress, StageTransitionUiArtSet art)
    {
      DrawFullScreen(art.ExitOpenBackground, 1f);
      var frame = Mathf.Min(
        art.LeftDoorFrames.Length - 1,
        Mathf.FloorToInt(progress * art.LeftDoorFrames.Length));
      DrawTexture(LeftDoor, art.LeftDoorFrames[frame], 1f);
      DrawTexture(RightDoor, art.RightDoorFrames[frame], 1f);
    }

    private static void DrawThresholdCross(float progress, StageTransitionUiArtSet art)
    {
      var eased = Smooth(progress);
      var scale = Mathf.Lerp(1f, 1.26f, eased);
      DrawZoomed(art.ExitOpenBackground, scale, 0f, Mathf.Lerp(0f, 10f, eased), 1f);

      var doorAlpha = 1f - eased;
      var left = ScaleRect(LeftDoor, scale, -eased * 48f, eased * 10f);
      var right = ScaleRect(RightDoor, scale, eased * 48f, eased * 10f);
      DrawTexture(left, art.LeftDoorFrames[art.LeftDoorFrames.Length - 1], doorAlpha);
      DrawTexture(right, art.RightDoorFrames[art.RightDoorFrames.Length - 1], doorAlpha);
      DrawFullScreen(art.Vignette, Mathf.Lerp(0.15f, 0.42f, eased));
    }

    private static void DrawFadeOut(float progress, StageTransitionUiArtSet art)
    {
      DrawZoomed(art.ExitOpenBackground, 1.26f, 0f, 10f, 1f);
      DrawBlack(art.FadeBlack, Smooth(progress));
    }

    private static void DrawLoading(
      NextStageTransitionSnapshot snapshot,
      StageTransitionUiArtSet art)
    {
      DrawBlack(art.FadeBlack, 1f);
      if (!snapshot.ShouldShowLoading) return;
      var loadingElapsed = Math.Max(
        0,
        snapshot.ElapsedMicroseconds
          - GameRules.NextStageTransitionFixedPreloadMicroseconds
          - GameRules.NextStageTransitionMinimumBlackHoldMicroseconds);
      var frame = (int)(loadingElapsed / 90_000L) % art.LoadingFrames.Length;
      DrawTexture(Loading, art.LoadingFrames[frame], 1f);
    }

    private static Rect ScaleRect(Rect rect, float scale, float offsetX, float offsetY)
    {
      var center = rect.center;
      var width = rect.width * scale;
      var height = rect.height * scale;
      return new Rect(
        center.x - width * 0.5f + offsetX,
        center.y - height * 0.5f + offsetY,
        width,
        height);
    }

    private static void DrawZoomed(
      Texture2D texture,
      float scale,
      float offsetX,
      float offsetY,
      float alpha)
    {
      var width = FullScreen.width * scale;
      var height = FullScreen.height * scale;
      DrawTexture(
        new Rect(
          (FullScreen.width - width) * 0.5f + offsetX,
          (FullScreen.height - height) * 0.5f + offsetY,
          width,
          height),
        texture,
        alpha);
    }

    private static void DrawFullScreen(Texture2D texture, float alpha)
    {
      DrawTexture(FullScreen, texture, alpha);
    }

    private static void DrawBlack(Texture2D blackTexture, float alpha)
    {
      if (alpha <= 0f) return;
      if (blackTexture != null)
      {
        DrawTexture(FullScreen, blackTexture, alpha);
        return;
      }

      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, false);
      GUI.color = previous;
    }

    private static void DrawTexture(Rect rect, Texture2D texture, float alpha)
    {
      if (texture == null || alpha <= 0f) return;
      var previous = GUI.color;
      GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
      GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      GUI.color = previous;
    }

    private static float Smooth(float value)
    {
      return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));
    }
  }
}
