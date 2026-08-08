using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerResultOverlayRenderer
  {
    public static void Draw(
      Rect rect,
      string message,
      bool predictionStep,
      bool predictionSucceeded,
      PlayableDevStyles styles)
    {
      var previousColor = GUI.color;
      GUI.color = new Color(0.055f, 0.035f, 0.02f, 0.84f);
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);

      var accent = predictionStep
        ? predictionSucceeded
          ? new Color(0.2f, 0.86f, 0.82f, 1f)
          : new Color(1f, 0.3f, 0.3f, 1f)
        : new Color(0.9f, 0.68f, 0.26f, 1f);
      GUI.color = accent;
      GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4f), Texture2D.whiteTexture);
      GUI.DrawTexture(new Rect(rect.x, rect.yMax - 4f, rect.width, 4f), Texture2D.whiteTexture);
      GUI.color = previousColor;

      var labelStyle = new GUIStyle(styles.Heading)
      {
        fontSize = predictionStep ? 23 : 18,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      labelStyle.normal.textColor = predictionStep ? accent : Color.white;
      GUI.Label(new Rect(rect.x + 20f, rect.y + 8f, rect.width - 40f, rect.height - 16f), message, labelStyle);
    }
  }
}
