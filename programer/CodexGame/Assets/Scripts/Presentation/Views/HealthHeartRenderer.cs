using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class HealthHeartRenderer
  {
    private const float HeartSize = 24f;
    private const float HeartGap = 4f;

    public static void Draw(
      Rect rect,
      int currentHealth,
      int maximumHealth,
      bool ai,
      HealthUiArtSet art)
    {
      var state = HealthPipViewState.Create(currentHealth, maximumHealth);
      var totalWidth = maximumHealth * HeartSize + (maximumHealth - 1) * HeartGap;
      var startX = rect.x + (rect.width - totalWidth) * 0.5f;
      var y = rect.y + (rect.height - HeartSize) * 0.5f;

      for (var index = 0; index < maximumHealth; index++)
      {
        var filled = index < state.FilledCount;
        var texture = SelectTexture(ai, filled, art);
        var heartRect = new Rect(startX + index * (HeartSize + HeartGap), y, HeartSize, HeartSize);
        if (texture != null)
        {
          GUI.DrawTexture(heartRect, texture, ScaleMode.ScaleToFit, true);
        }
        else
        {
          var previousColor = GUI.color;
          GUI.color = filled
            ? ai ? new Color(0.95f, 0.22f, 0.25f) : new Color(0.1f, 0.88f, 0.9f)
            : new Color(0.2f, 0.2f, 0.22f);
          GUI.DrawTexture(heartRect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true);
          GUI.color = previousColor;
        }
      }
    }

    private static Texture2D SelectTexture(bool ai, bool filled, HealthUiArtSet art)
    {
      if (art == null || !art.IsComplete) return null;
      if (ai) return filled ? art.AiFilled : art.AiEmpty;
      return filled ? art.PlayerFilled : art.PlayerEmpty;
    }
  }
}
