using System;
using CodexGame.Application.Items;
using CodexGame.Application.Playable;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class Presentation0124Panel
  {
    private static readonly Rect FullScreen = new Rect(0f, 0f, 960f, 540f);
    private static readonly Rect EntryLabel = new Rect(336f, 34f, 288f, 80f);
    private static readonly Rect SkipButton = new Rect(816f, 476f, 120f, 44f);

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
        GUI.DrawTexture(new Rect(300f, 184f, 360f, 152f), art.OpponentIntroFrame, ScaleMode.StretchToFill, true);
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
      var opponentNameKey = OpponentNameKey(snapshot.StageNumber);
      if (opponentNameKey != null)
      {
        GUI.Label(
          new Rect(448f, 206f, 194f, 30f),
          localization.Get(opponentNameKey),
          styles.Heading);
      }
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
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      DrawPhaseLabel(art?.ThreeCallIcon, localization.Get("UI_THREE_CALL_ENTRY"), art, styles);
    }

    public void DrawThreeCallToSelection(
      PresentationUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (art?.DesaturateOverlay != null)
      {
        GUI.DrawTexture(FullScreen, art.DesaturateOverlay, ScaleMode.StretchToFill, true);
      }
      if (art?.FocusMask != null)
      {
        GUI.DrawTexture(FullScreen, art.FocusMask, ScaleMode.StretchToFill, true);
      }
      if (art?.PlayerAcquireTrail != null)
      {
        GUI.DrawTexture(new Rect(320f, 408f, 320f, 96f), art.PlayerAcquireTrail, ScaleMode.ScaleToFit, true);
      }
      if (art?.AiAcquireTrail != null)
      {
        GUI.DrawTexture(new Rect(320f, 36f, 320f, 96f), art.AiAcquireTrail, ScaleMode.ScaleToFit, true);
      }
      DrawPhaseLabel(art?.ShowdownIcon, localization.Get("UI_SHOWDOWN_ENTRY"), art, styles);
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

    private static string OpponentNameKey(int stageNumber)
    {
      return stageNumber switch
      {
        1 => "UI_OPPONENT_STAGE_1_NAME",
        2 => "UI_OPPONENT_STAGE_2_NAME",
        3 => "UI_OPPONENT_STAGE_3_NAME",
        4 => "UI_OPPONENT_STAGE_4_NAME",
        _ => null
      };
    }

    private static void DrawStagePips(int stageNumber)
    {
      var previousColor = GUI.color;
      var activeCount = Mathf.Clamp(stageNumber, 0, 3);
      for (var index = 0; index < 3; index++)
      {
        GUI.color = index < activeCount
          ? new Color(0.94f, 0.63f, 0.18f, 1f)
          : new Color(0.27f, 0.23f, 0.18f, 1f);
        var center = new Vector2(464f + 28f * index, 304f);
        var previousMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(45f, center);
        GUI.DrawTexture(
          new Rect(center.x - 4f, center.y - 4f, 8f, 8f),
          Texture2D.whiteTexture,
          ScaleMode.StretchToFill,
          true);
        GUI.matrix = previousMatrix;
      }
      GUI.color = previousColor;
    }
  }
}
