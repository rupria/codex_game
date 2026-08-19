using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerResultOverlayRenderer
  {
    public static Rect Draw(
      string message,
      string itemStatus,
      PokerResultPanelVisualState visualState,
      Texture2D badge,
      PokerResultUiArtSet art,
      PlayableDevStyles styles)
    {
      var labelStyle = new GUIStyle(styles.Heading)
      {
        fontSize = 20,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      var hasItemStatus = !string.IsNullOrWhiteSpace(itemStatus);
      var measuredHeight = labelStyle.CalcHeight(
        new GUIContent(message),
        PokerResultPanelLayout.MessageWidth);
      var layout = PokerResultPanelLayout.Select(measuredHeight, hasItemStatus);
      var rect = new Rect(86f, layout.Y, PokerResultPanelLayout.Width, layout.Height);
      var panel = art?.FindPanel(visualState, layout.Size);
      if (panel != null)
      {
        GUI.DrawTexture(rect, panel, ScaleMode.StretchToFill, true);
      }
      else
      {
        DrawFallbackPanel(rect, visualState);
      }

      if (badge != null)
      {
        GUI.DrawTexture(
          new Rect(rect.x + 18f, rect.y + (rect.height - 44f) * 0.5f, 44f, 44f),
          badge,
          ScaleMode.ScaleToFit,
          true);
      }

      labelStyle.normal.textColor = Color.white;
      var messageHeight = rect.height - 32f - (hasItemStatus ? 40f : 0f);
      GUI.Label(
        new Rect(rect.x + 72f, rect.y + 16f, PokerResultPanelLayout.MessageWidth, messageHeight),
        message,
        labelStyle);

      if (hasItemStatus)
      {
        var chipRect = new Rect(rect.x + 214f, rect.yMax - 40f, 360f, 32f);
        if (art?.ItemStatusChip != null)
        {
          GUI.DrawTexture(chipRect, art.ItemStatusChip, ScaleMode.StretchToFill, true);
        }
        var statusStyle = new GUIStyle(styles.Small)
        {
          fontSize = 16,
          fontStyle = FontStyle.Bold,
          alignment = TextAnchor.MiddleCenter,
          wordWrap = false
        };
        statusStyle.normal.textColor = Color.white;
        GUI.Label(chipRect, itemStatus, statusStyle);
      }

      return rect;
    }

    private static void DrawFallbackPanel(Rect rect, PokerResultPanelVisualState state)
    {
      var previousColor = GUI.color;
      GUI.color = new Color(0.055f, 0.035f, 0.02f, 0.94f);
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = state == PokerResultPanelVisualState.Success
        ? new Color(0.2f, 0.86f, 0.82f, 1f)
        : state == PokerResultPanelVisualState.Failure
          ? new Color(1f, 0.3f, 0.3f, 1f)
          : new Color(0.9f, 0.68f, 0.26f, 1f);
      GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4f), Texture2D.whiteTexture);
      GUI.DrawTexture(new Rect(rect.x, rect.yMax - 4f, rect.width, 4f), Texture2D.whiteTexture);
      GUI.color = previousColor;
    }
  }
}
