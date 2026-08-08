using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerItemBoxRenderer
  {
    public static void DrawEmpty(Rect rect)
    {
      DrawRect(new Rect(rect.x + 4f, rect.y + 5f, rect.width - 2f, rect.height - 1f),
        new Color(0f, 0f, 0f, 0.46f));

      // Raised wooden lid.
      DrawRect(new Rect(rect.x + 7f, rect.y + 3f, rect.width - 14f, 25f),
        new Color(0.18f, 0.085f, 0.035f, 1f));
      DrawRect(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 18f),
        new Color(0.34f, 0.17f, 0.075f, 1f));
      DrawRect(new Rect(rect.x + 10f, rect.y + 22f, rect.width - 20f, 3f),
        new Color(0.68f, 0.43f, 0.19f, 1f));

      // Open compartment and front wall.
      DrawRect(new Rect(rect.x + 4f, rect.y + 27f, rect.width - 8f, 33f),
        new Color(0.16f, 0.07f, 0.025f, 1f));
      DrawRect(new Rect(rect.x + 9f, rect.y + 31f, rect.width - 18f, 17f),
        new Color(0.045f, 0.025f, 0.016f, 1f));
      DrawRect(new Rect(rect.x + 6f, rect.y + 49f, rect.width - 12f, 9f),
        new Color(0.39f, 0.19f, 0.075f, 1f));

      // Brass hinges and latch keep the empty slot readable without an item icon.
      var brass = new Color(0.71f, 0.49f, 0.2f, 1f);
      DrawRect(new Rect(rect.x + 13f, rect.y + 25f, 8f, 3f), brass);
      DrawRect(new Rect(rect.x + rect.width - 21f, rect.y + 25f, 8f, 3f), brass);
      DrawRect(new Rect(rect.center.x - 4f, rect.y + 48f, 8f, 8f), brass);
      DrawRect(new Rect(rect.center.x - 2f, rect.y + 50f, 4f, 4f),
        new Color(0.12f, 0.065f, 0.03f, 1f));
    }

    private static void DrawRect(Rect rect, Color color)
    {
      var previousColor = GUI.color;
      GUI.color = color;
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previousColor;
    }
  }
}
