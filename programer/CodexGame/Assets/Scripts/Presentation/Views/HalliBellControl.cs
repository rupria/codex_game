using CodexGame.Application.Playable;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliBellControl
  {
    public bool Draw(
      PileSide side,
      Rect visualRect,
      Rect hitRect,
      string key,
      bool enabled,
      PrototypeHalliSnapshot snapshot,
      HalliUiArtSet art,
      PlayableDevStyles styles)
    {
      var hovered = hitRect.Contains(Event.current.mousePosition);
      var pressed = hovered && Input.GetMouseButton(0);
      var feedback = snapshot.LastBellPile == side ? snapshot.BellFeedback : PrototypeBellFeedback.None;
      var texture = SelectTexture(enabled, hovered, pressed, feedback, art);
      var previousColor = GUI.color;
      if (!enabled) GUI.color = new Color(0.45f, 0.48f, 0.52f, 0.65f);
      else if (feedback == PrototypeBellFeedback.Correct) GUI.color = new Color(0.55f, 1f, 0.72f, 1f);
      if (texture != null) GUI.DrawTexture(visualRect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(visualRect, "BELL", styles.Card);
      GUI.color = previousColor;

      GUI.Label(new Rect(visualRect.x - 18f, visualRect.y + 58f, visualRect.width + 36f, 24f), key, styles.Heading);
      GUI.enabled = enabled;
      var clicked = GUI.Button(hitRect, GUIContent.none, GUIStyle.none);
      GUI.enabled = true;
      return clicked;
    }

    private static Texture2D SelectTexture(
      bool enabled,
      bool hovered,
      bool pressed,
      PrototypeBellFeedback feedback,
      HalliUiArtSet art)
    {
      if (art == null) return null;
      if (feedback == PrototypeBellFeedback.Wrong && art.BellWrong != null) return art.BellWrong;
      if (feedback == PrototypeBellFeedback.Correct && art.BellPressed != null) return art.BellPressed;
      if (pressed && art.BellPressed != null) return art.BellPressed;
      if (enabled && hovered && art.BellHover != null) return art.BellHover;
      return art.BellIdle;
    }
  }
}
