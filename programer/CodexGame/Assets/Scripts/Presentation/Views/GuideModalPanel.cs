using System;
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

    public void Draw(
      GuideModalState state,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action previous,
      Action next,
      Action close)
    {
      var previousColor = GUI.color;
      GUI.color = new Color(0.025f, 0.03f, 0.04f, 1f);
      GUI.DrawTexture(
        new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previousColor;

      GUI.Box(new Rect(72f, 34f, 816f, 472f), GUIContent.none);
      GUI.Label(new Rect(110f, 52f, 740f, 42f), localization.Get("UI_GUIDE_TITLE"), styles.Title);
      GUI.Label(
        new Rect(120f, 102f, 720f, 38f),
        localization.Get(TitleKeys[state.PageIndex]),
        styles.Heading);
      GUI.Label(
        new Rect(132f, 146f, 696f, 236f),
        localization.Get(BodyKeys[state.PageIndex]),
        BodyStyle(styles));
      GUI.Label(
        new Rect(182f, 386f, 596f, 24f),
        localization.Get("UI_GUIDE_MODAL_HINT"),
        styles.Small);
      GUI.Label(
        new Rect(410f, 414f, 140f, 24f),
        localization.Get(
          "UI_GUIDE_PAGE_INDICATOR",
          new LocalizationArgument("page", state.PageIndex + 1),
          new LocalizationArgument("total", GuideModalState.PageCount)),
        styles.Small);

      GUI.enabled = state.CanMovePrevious;
      if (GUI.Button(new Rect(124f, 448f, 190f, 42f), localization.Get("UI_GUIDE_PREV"))) previous();
      GUI.enabled = state.CanMoveNext;
      if (GUI.Button(new Rect(646f, 448f, 190f, 42f), localization.Get("UI_GUIDE_NEXT"))) next();
      GUI.enabled = true;
      if (GUI.Button(new Rect(350f, 448f, 260f, 42f), localization.Get("UI_COMMON_CLOSE_ESC"))) close();
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
