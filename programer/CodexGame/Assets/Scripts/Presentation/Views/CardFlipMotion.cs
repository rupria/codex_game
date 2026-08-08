using CodexGame.Core.Cards;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class CardFlipMotion
  {
    private const float MinimumFaceScale = 0.035f;
    private const float LiftHeight = 18f;

    public static void Draw(
      PlayableCardRenderer renderer,
      Card card,
      Rect source,
      Rect destination,
      float normalizedProgress,
      bool rotateBackForOpponent)
    {
      var progress = Mathf.Clamp01(normalizedProgress);
      var travel = progress * progress * (3f - 2f * progress);
      var rect = Lerp(source, destination, travel);
      rect.y -= Mathf.Sin(progress * Mathf.PI) * LiftHeight;

      var flipProgress = Mathf.Clamp01((progress - 0.58f) / 0.42f);
      var faceScale = Mathf.Max(
        MinimumFaceScale,
        Mathf.Abs(Mathf.Cos(flipProgress * Mathf.PI)));
      rect = ScaleWidthAroundCenter(rect, faceScale);

      DrawShadow(rect, progress);
      if (flipProgress < 0.5f)
      {
        renderer.DrawBackAt(rect, rotateBackForOpponent ? 180f : 0f);
      }
      else renderer.DrawAt(rect, card);
    }

    private static void DrawShadow(Rect cardRect, float progress)
    {
      var previousColor = GUI.color;
      var lift = Mathf.Sin(progress * Mathf.PI);
      GUI.color = new Color(0f, 0f, 0f, 0.18f + lift * 0.14f);
      GUI.DrawTexture(
        new Rect(
          cardRect.x + 5f + lift * 4f,
          cardRect.y + 8f + lift * 8f,
          cardRect.width,
          cardRect.height),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previousColor;
    }

    private static Rect ScaleWidthAroundCenter(Rect rect, float scale)
    {
      var width = rect.width * scale;
      return new Rect(rect.center.x - width * 0.5f, rect.y, width, rect.height);
    }

    private static Rect Lerp(Rect from, Rect to, float progress)
    {
      return new Rect(
        Mathf.Lerp(from.x, to.x, progress),
        Mathf.Lerp(from.y, to.y, progress),
        Mathf.Lerp(from.width, to.width, progress),
        Mathf.Lerp(from.height, to.height, progress));
    }
  }
}
