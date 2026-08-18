using System;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class GuideModalPanel
  {
    private static readonly string[] TitleKeys =
    {
      "UI_GUIDE_PAGE_FLOW_TITLE",
      "UI_GUIDE_PAGE_HALLI_TITLE",
      "UI_GUIDE_PAGE_CARDS_TITLE",
      "UI_GUIDE_PAGE_RESULT_TITLE"
    };

    private static readonly string[] BodyKeys =
    {
      "UI_GUIDE_PAGE_FLOW_BODY",
      "UI_GUIDE_PAGE_HALLI_BODY",
      "UI_GUIDE_PAGE_CARDS_BODY",
      "UI_GUIDE_PAGE_RESULT_BODY"
    };

    private GUIStyle _bodyStyle;
    private static readonly Rect PreviousButton = new Rect(299f, 451f, 56f, 58f);
    private static readonly Rect NextButton = new Rect(602f, 451f, 56f, 58f);
    private static readonly Rect CloseButton = new Rect(800f, 451f, 56f, 58f);
    private static readonly Rect PageIndicatorPlate = new Rect(430f, 449f, 100f, 33f);

    public void Draw(
      GuideModalState state,
      PlayableDevStyles styles,
      GuideUiArtSet art,
      LocalizationRuntime localization,
      Action previous,
      Action next,
      Action close,
      Action completeTutorial)
    {
      DrawOpaqueBackground(art);
      if (art != null && art.IsComplete)
      {
        GUI.DrawTexture(
          new Rect(37f, 126f, 320f, 280f),
          art.GetPageArt(state.PageIndex),
          ScaleMode.ScaleToFit,
          true);
      }
      GUI.Label(
        new Rect(336f, 48f, 267f, 50f),
        localization.Get(TitleKeys[state.PageIndex]),
        styles.Heading);
      GUI.Label(
        new Rect(390f, 145f, 475f, 260f),
        localization.Get(BodyKeys[state.PageIndex]),
        BodyStyle(styles));
      DrawPageDots(state.PageIndex, art?.PageIndicatorPlate);

      // The visible arrow and close icons are already part of the approved 960x540 art.
      // Keep a single transparent hit target on each icon instead of drawing a second
      // grey text button over it.
      if (DrawIconHitTarget(PreviousButton, state.CanMovePrevious, art?.PreviousIcon)) previous();
      var canAdvance = state.CanMoveNext || state.IsFirstStartTutorial;
      if (DrawIconHitTarget(NextButton, canAdvance, art?.NextIcon))
      {
        if (state.CanMoveNext) next();
        else completeTutorial();
      }
      if (!state.IsFirstStartTutorial
        && DrawIconHitTarget(CloseButton, true, art?.CloseIcon)) close();
    }

    private static void DrawOpaqueBackground(GuideUiArtSet art)
    {
      if (art != null && art.ModalBackground != null)
      {
        GUI.DrawTexture(
          new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
          art.ModalBackground,
          ScaleMode.StretchToFill,
          true);
        return;
      }

      var previousColor = GUI.color;
      GUI.color = new Color(0.025f, 0.03f, 0.04f, 1f);
      GUI.DrawTexture(
        new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previousColor;
    }

    private static bool DrawIconHitTarget(Rect rect, bool enabled, Texture2D icon)
    {
      var previousColor = GUI.color;
      if (icon != null)
      {
        GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
      }
      if (!enabled)
      {
        GUI.color = new Color(0.02f, 0.02f, 0.025f, 0.62f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      }
      else if (rect.Contains(Event.current.mousePosition))
      {
        GUI.color = new Color(1f, 0.78f, 0.32f, 0.14f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      }
      GUI.color = previousColor;
      GUI.enabled = enabled;
      var clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
      GUI.enabled = true;
      return clicked;
    }

    private static void DrawPageDots(int pageIndex, Texture2D plate)
    {
      var previousColor = GUI.color;
      GUI.color = Color.white;
      if (plate != null)
      {
        GUI.DrawTexture(PageIndicatorPlate, plate, ScaleMode.StretchToFill, true);
      }
      else
      {
        GUI.color = new Color(0.055f, 0.047f, 0.039f, 1f);
        GUI.DrawTexture(PageIndicatorPlate, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      }

      const float startX = 448f;
      const float gap = 21f;
      for (var index = 0; index < GuideModalState.PageCount; index++)
      {
        GUI.color = index == pageIndex
          ? new Color(0.95f, 0.63f, 0.18f, 1f)
          : new Color(0.24f, 0.21f, 0.17f, 1f);
        var center = new Vector2(startX + gap * index, 466f);
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

    private GUIStyle BodyStyle(PlayableDevStyles styles)
    {
      if (_bodyStyle != null) return _bodyStyle;
      _bodyStyle = new GUIStyle(styles.Body)
      {
        alignment = TextAnchor.UpperLeft,
        fontSize = 15,
        padding = new RectOffset(18, 18, 14, 14)
      };
      return _bodyStyle;
    }

  }
}
