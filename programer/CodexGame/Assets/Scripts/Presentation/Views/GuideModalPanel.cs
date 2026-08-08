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
    private GUIStyle _navStyle;

    public void Draw(
      GuideModalState state,
      PlayableDevStyles styles,
      GuideUiArtSet art,
      LocalizationRuntime localization,
      Action previous,
      Action next,
      Action close)
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
      GUI.Label(
        new Rect(48f, 432f, 650f, 30f),
        localization.Get("UI_GUIDE_MODAL_HINT"),
        styles.Small);
      if (art != null && art.PageIndicatorPlate != null)
      {
        GUI.DrawTexture(
          new Rect(430f, 475f, 100f, 34f),
          art.PageIndicatorPlate,
          ScaleMode.StretchToFill,
          true);
      }
      GUI.Label(
        new Rect(430f, 475f, 100f, 34f),
        localization.Get(
          "UI_GUIDE_PAGE_INDICATOR",
          new LocalizationArgument("page", state.PageIndex + 1),
          new LocalizationArgument("total", GuideModalState.PageCount)),
        NavStyle(styles));

      if (DrawNavigationButton(
        new Rect(270f, 471f, 128f, 42f),
        localization.Get("UI_GUIDE_PREV"),
        state.CanMovePrevious,
        styles,
        art)) previous();
      if (DrawNavigationButton(
        new Rect(542f, 471f, 128f, 42f),
        localization.Get("UI_GUIDE_NEXT"),
        state.CanMoveNext,
        styles,
        art)) next();
      if (DrawNavigationButton(
        new Rect(788f, 471f, 128f, 42f),
        localization.Get("UI_COMMON_CLOSE_ESC"),
        true,
        styles,
        art)) close();
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

    private bool DrawNavigationButton(
      Rect rect,
      string label,
      bool enabled,
      PlayableDevStyles styles,
      GuideUiArtSet art)
    {
      if (art != null && art.IsComplete)
      {
        var hovered = enabled && rect.Contains(Event.current.mousePosition);
        var texture = !enabled ? art.NavDisabled : hovered ? art.NavHover : art.NavIdle;
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        GUI.Label(rect, label, NavStyle(styles));
        GUI.enabled = enabled;
        var clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
        GUI.enabled = true;
        return clicked;
      }

      GUI.enabled = enabled;
      var fallbackClicked = GUI.Button(rect, label);
      GUI.enabled = true;
      return fallbackClicked;
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

    private GUIStyle NavStyle(PlayableDevStyles styles)
    {
      if (_navStyle != null) return _navStyle;
      _navStyle = new GUIStyle(styles.Heading)
      {
        fontSize = 13,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = false
      };
      return _navStyle;
    }
  }
}
