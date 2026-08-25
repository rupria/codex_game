using System;
using CodexGame.Application.Items;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class Presentation0124Panel
  {
    private static readonly Rect FullScreen = new Rect(0f, 0f, 960f, 540f);
    private static readonly Rect EntryLabel = new Rect(336f, 34f, 288f, 80f);
    private static readonly Rect ThreeCallPlaque = new Rect(290f, 214f, 380f, 112f);
    private static readonly Rect ThreeCallBell = new Rect(307f, 238f, 64f, 64f);
    private static readonly Rect ThreeCallTitle = new Rect(405f, 246f, 240f, 32f);
    private static readonly Rect SkipButton = new Rect(816f, 476f, 120f, 44f);
    private static readonly Rect OpponentIntroFrame = new Rect(300f, 184f, 360f, 152f);
    private static readonly Rect OpponentDescriptionMask = new Rect(454f, 243f, 182f, 47f);

    public void DrawStageEntry(
      PlayableGameSnapshot snapshot,
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action skip)
    {
      if (art?.TableFocusVignette != null)
      {
        GUI.DrawTexture(FullScreen, art.TableFocusVignette, ScaleMode.StretchToFill, true);
      }
      if (art?.OpponentIntroFrame != null)
      {
        GUI.DrawTexture(OpponentIntroFrame, art.OpponentIntroFrame, ScaleMode.StretchToFill, true);
        UiPixelSurfaceRenderer.Fill(
          OpponentDescriptionMask,
          new Color(0.055f, 0.047f, 0.039f, 1f));
      }
      var cutout = art?.GetOpponentCutout(snapshot.StageNumber);
      if (cutout != null)
      {
        GUI.DrawTexture(new Rect(320f, 206f, 108f, 108f), cutout, ScaleMode.ScaleToFit, true);
      }
      else
      {
        DrawPortraitFallback(new Rect(320f, 206f, 108f, 108f));
      }
      GUI.Label(
        new Rect(448f, 206f, 194f, 30f),
        localization.Get(StageOpponentNameKeys.ForStage(snapshot.StageNumber)),
        styles.Heading);
      DrawStagePips(snapshot.StageNumber);
      DrawRestriction(snapshot.StageItemRestriction, art, styles, localization, new Rect(320f, 344f, 320f, 84f));

      var hovered = SkipButton.Contains(Event.current.mousePosition);
      var pressed = hovered && Input.GetMouseButton(0);
      var button = pressed ? art?.SkipPressed : hovered ? art?.SkipHover : art?.SkipIdle;
      if (button != null) GUI.DrawTexture(SkipButton, button, ScaleMode.StretchToFill, true);
      GUI.Label(SkipButton, localization.Get("UI_STAGE_ENTRY_SKIP"), styles.Heading);
      if (GUI.Button(SkipButton, GUIContent.none, GUIStyle.none)) skip();
    }

    public void DrawThreeCallEntry(
      PlayableTransitionSnapshot transition,
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      var frame = ThreeCallEntryAnimationState.Evaluate(transition?.Progress ?? 1f);
      var previousColor = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.42f * frame.Alpha);
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = new Color(1f, 1f, 1f, frame.Alpha);

      var plaqueRect = ScaleFromCenter(ThreeCallPlaque, frame.Scale);
      if (art?.ThreeCallPlaque != null)
      {
        GUI.DrawTexture(plaqueRect, art.ThreeCallPlaque, ScaleMode.StretchToFill, true);
      }
      DrawSheetFrame(
        art?.ThreeCallBellPulseSheet,
        8,
        frame.BellFrameIndex,
        ScaleFromCenter(ThreeCallBell, frame.Scale));
      if (frame.ShowTitle)
      {
        GUI.Label(
          ScaleFromCenter(ThreeCallTitle, frame.Scale),
          localization.Get("UI_THREE_CALL_ENTRY"),
          styles.Heading);
      }
      GUI.color = previousColor;
    }

    public void DrawThreeCallToSelection(
      PlayableTransitionSnapshot transition,
      Card? firstCommunityCard,
      Card? secondCommunityCard,
      PlayableCardRenderer cards,
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      var previousDepth = GUI.depth;
      GUI.depth = -100;
      var previousColor = GUI.color;
      GUI.color = new Color(0.025f, 0.045f, 0.037f, 0.985f);
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previousColor;

      if (art?.FocusMask != null)
      {
        GUI.DrawTexture(FullScreen, art.FocusMask, ScaleMode.StretchToFill, true);
      }
      if (art?.ShowdownWideFrame != null)
      {
        GUI.DrawTexture(
          new Rect(236f, 126f, 488f, 330f),
          art.ShowdownWideFrame,
          ScaleMode.StretchToFill,
          true);
      }

      DrawPhaseLabel(art?.ShowdownIcon, localization.Get("UI_SHOWDOWN_ENTRY"), art, styles);

      if (cards == null
        || !firstCommunityCard.HasValue
        || !secondCommunityCard.HasValue)
      {
        GUI.depth = previousDepth;
        return;
      }

      var progress = transition?.Progress ?? 1f;
      DrawCommunityReveal(
        cards,
        firstCommunityCard.Value,
        new Rect(366f, 190f, 104f, 148f),
        Normalize(
          progress,
          0.04f,
          GameRules.ShowdownFirstCardRevealCompleteProgress));
      DrawCommunityReveal(
        cards,
        secondCommunityCard.Value,
        new Rect(490f, 190f, 104f, 148f),
        Normalize(
          progress,
          GameRules.ShowdownFirstCardRevealCompleteProgress,
          GameRules.ShowdownSecondCardRevealCompleteProgress));

      UiPixelSurfaceRenderer.DrawDiamond(
        new Vector2(480f, 366f),
        new Color(0.94f, 0.63f, 0.18f, 1f));
      GUI.depth = previousDepth;
    }

    public void DrawShowdownFrame(bool showResult, PresentationUiArtSet art)
    {
      var texture = showResult ? art?.ResultSummaryFrame : art?.ShowdownWideFrame;
      if (texture == null) return;
      var rect = showResult ? new Rect(120f, 90f, 720f, 360f) : new Rect(100f, 60f, 760f, 420f);
      GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
    }

    public void DrawItemRestriction(
      StageItemRestrictionSnapshot restriction,
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      DrawRestriction(restriction, art, styles, localization, new Rect(620f, 72f, 320f, 84f));
    }

    public void DrawStageClear(PresentationUiArtSet art)
    {
      if (art?.StageClearFrame != null)
      {
        GUI.DrawTexture(new Rect(200f, 150f, 560f, 240f), art.StageClearFrame, ScaleMode.StretchToFill, true);
      }
    }

    private static void DrawPhaseLabel(Texture2D icon, string label, PresentationUiArtSet art, PlayableDevStyles styles)
    {
      if (art?.EntryLabelFrame != null)
      {
        GUI.DrawTexture(EntryLabel, art.EntryLabelFrame, ScaleMode.StretchToFill, true);
      }
      if (icon != null) GUI.DrawTexture(new Rect(348f, 42f, 64f, 64f), icon, ScaleMode.ScaleToFit, true);
      GUI.Label(new Rect(420f, 50f, 188f, 48f), label, styles.Heading);
    }

    private static Rect ScaleFromCenter(Rect rect, float scale)
    {
      var width = rect.width * scale;
      var height = rect.height * scale;
      return new Rect(
        rect.center.x - width * 0.5f,
        rect.center.y - height * 0.5f,
        width,
        height);
    }

    private static void DrawCommunityReveal(
      PlayableCardRenderer cards,
      CodexGame.Core.Cards.Card card,
      Rect destination,
      float progress)
    {
      if (progress <= 0f)
      {
        cards.DrawBackAt(destination);
        return;
      }

      CardFlipMotion.Draw(
        cards,
        card,
        destination,
        destination,
        progress,
        false);
    }

    private static float Normalize(float value, float start, float end)
    {
      return Mathf.Clamp01((value - start) / (end - start));
    }

    private static void DrawSheetFrame(Texture2D sheet, int frameCount, int frameIndex, Rect rect)
    {
      if (sheet == null || frameCount <= 0) return;
      var safeIndex = Mathf.Clamp(frameIndex, 0, frameCount - 1);
      GUI.DrawTextureWithTexCoords(
        rect,
        sheet,
        new Rect(safeIndex / (float)frameCount, 0f, 1f / frameCount, 1f),
        true);
    }

    private static void DrawRestriction(
      StageItemRestrictionSnapshot restriction,
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Rect rect)
    {
      if (restriction == null || !restriction.IsActive || art == null) return;
      if (art.PenaltyLabelFrame != null)
      {
        GUI.DrawTexture(rect, art.PenaltyLabelFrame, ScaleMode.StretchToFill, true);
      }
      var icon = restriction.IsExhausted
        ? art.LimitExhausted
        : restriction.UsedCount > 0
          ? art.UsedOne
          : restriction.UseLimit == 1 ? art.LimitOne : art.LimitTwo;
      if (icon != null)
      {
        GUI.DrawTexture(new Rect(rect.x + 10f, rect.y + 10f, 64f, 64f), icon, ScaleMode.ScaleToFit, true);
      }
      GUI.Label(
        new Rect(rect.x + 82f, rect.y + 18f, rect.width - 92f, 48f),
        localization.Get(
          "UI_ITEM_LIMIT_REMAINING",
          new LocalizationArgument("remaining", restriction.RemainingUses),
          new LocalizationArgument("limit", restriction.UseLimit)),
        styles.Small);
    }

    private static void DrawPortraitFallback(Rect rect)
    {
      var previous = GUI.color;
      GUI.color = new Color(0.04f, 0.035f, 0.03f, 0.94f);
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;
    }

    private static void DrawStagePips(int stageNumber)
    {
      var previousColor = GUI.color;
      var activeCount = Mathf.Clamp(stageNumber, 0, 3);
      for (var index = 0; index < 3; index++)
      {
        var color = index < activeCount
          ? new Color(0.94f, 0.63f, 0.18f, 1f)
          : new Color(0.27f, 0.23f, 0.18f, 1f);
        var center = new Vector2(464f + 28f * index, 304f);
        UiPixelSurfaceRenderer.DrawDiamond(center, color);
      }
      GUI.color = previousColor;
    }
  }
}
