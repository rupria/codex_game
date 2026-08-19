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
    private static readonly Rect NavigationRail = new Rect(0f, 436f, 960f, 104f);
    private static readonly Rect PreviousButton = new Rect(352f, 451f, 56f, 58f);
    private static readonly Rect PageIndicatorPlate = new Rect(414f, 461f, 132f, 38f);
    private static readonly Rect NextButton = new Rect(552f, 451f, 56f, 58f);
    private static readonly Rect CloseButton = new Rect(850f, 451f, 56f, 58f);

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
      DrawNavigationRail(art);
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

      if (DrawNavButton(
        "GuidePrevious",
        PreviousButton,
        state.CanMovePrevious,
        art?.PreviousButton)) previous();
      var canAdvance = state.CanMoveNext || state.IsFirstStartTutorial;
      if (DrawNavButton("GuideNext", NextButton, canAdvance, art?.NextButton))
      {
        if (state.CanMoveNext) next();
        else completeTutorial();
      }
      if (!state.IsFirstStartTutorial
        && DrawNavButton("GuideClose", CloseButton, true, art?.CloseButton)) close();
    }

    private static void DrawNavigationRail(GuideUiArtSet art)
    {
      if (art?.NavRail == null) return;
      GUI.DrawTexture(NavigationRail, art.NavRail, ScaleMode.StretchToFill, true);
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

    private static bool DrawNavButton(
      string controlName,
      Rect rect,
      bool enabled,
      GuideNavButtonArtSet art)
    {
      var mouseHovered = rect.Contains(Event.current.mousePosition);
      var keyboardFocused = string.Equals(
        GUI.GetNameOfFocusedControl(),
        controlName,
        StringComparison.Ordinal);
      var pressed = enabled && mouseHovered && Input.GetMouseButton(0);
      var texture = art?.GetTexture(enabled, mouseHovered || keyboardFocused, pressed);
      if (texture != null)
      {
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      }

      GUI.enabled = enabled;
      GUI.SetNextControlName(controlName);
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

      const float startX = 444f;
      const float gap = 24f;
      for (var index = 0; index < GuideModalState.PageCount; index++)
      {
        var color = index == pageIndex
          ? new Color(0.95f, 0.63f, 0.18f, 1f)
          : new Color(0.24f, 0.21f, 0.17f, 1f);
        var center = new Vector2(startX + gap * index, 480f);
        UiPixelSurfaceRenderer.DrawDiamond(center, color);
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
