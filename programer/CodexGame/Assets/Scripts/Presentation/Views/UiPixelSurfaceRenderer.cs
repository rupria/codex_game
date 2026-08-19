using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class UiPixelSurfaceRenderer
  {
    public static void Fill(Rect rect, Color color)
    {
      var previousColor = GUI.color;
      GUI.color = color;
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previousColor;
    }

    public static void DrawDiamond(Vector2 center, Color color)
    {
      var previousColor = GUI.color;
      GUI.color = color;
      DrawRow(center, -3f, 2f);
      DrawRow(center, -2f, 4f);
      DrawRow(center, -1f, 6f);
      DrawRow(center, 0f, 8f);
      DrawRow(center, 1f, 6f);
      DrawRow(center, 2f, 4f);
      DrawRow(center, 3f, 2f);
      GUI.color = previousColor;
    }

    private static void DrawRow(Vector2 center, float offsetY, float width)
    {
      GUI.DrawTexture(
        new Rect(center.x - width * 0.5f, center.y + offsetY, width, 1f),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
    }
  }
}
