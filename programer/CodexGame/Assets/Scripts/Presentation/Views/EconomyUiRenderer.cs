using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class EconomyUiRenderer
  {
    private static readonly Color BaseFallback = new Color(0.68f, 0.46f, 0.14f, 0.96f);
    private static readonly Color TemporaryFallback = new Color(0.36f, 0.32f, 0.26f, 0.96f);

    public static void DrawShopBalances(
      Rect area,
      int temporaryCount,
      int baseCount,
      EconomyUiArtSet art,
      GUIStyle countStyle)
    {
      var gap = 8f;
      var width = (area.width - gap) * 0.5f;
      DrawBalance(
        new Rect(area.x, area.y, width, area.height),
        temporaryCount,
        art?.TemporaryCurrencyIcon,
        art?.TemporaryBalanceFrame,
        TemporaryFallback,
        "◇",
        countStyle);
      DrawBalance(
        new Rect(area.x + width + gap, area.y, width, area.height),
        baseCount,
        art?.BaseCurrencyIcon,
        art?.BaseBalanceFrame,
        BaseFallback,
        "●",
        countStyle);
    }

    public static void DrawBattleBalances(
      Rect area,
      int baseCount,
      int temporaryCount,
      EconomyUiArtSet art,
      GUIStyle countStyle)
    {
      DrawBalance(
        new Rect(area.x, area.y, area.width, area.height),
        baseCount,
        art?.BaseCurrencyIcon,
        art?.BaseBalanceFrame,
        BaseFallback,
        "●",
        countStyle);
      if (temporaryCount <= 0) return;
      DrawBalance(
        new Rect(area.x, area.y + area.height + 4f, area.width, area.height),
        temporaryCount,
        art?.TemporaryCurrencyIcon,
        art?.TemporaryBalanceFrame,
        TemporaryFallback,
        "◇",
        countStyle);
    }

    public static void DrawStageRewards(
      Rect area,
      int baseReward,
      int temporaryReward,
      EconomyUiArtSet art,
      GUIStyle countStyle)
    {
      var gap = 24f;
      var width = (area.width - gap) * 0.5f;
      DrawBalance(
        new Rect(area.x, area.y, width, area.height),
        baseReward,
        art?.BaseCurrencyIcon,
        art?.BaseRewardFrame,
        BaseFallback,
        "●",
        countStyle);
      DrawBalance(
        new Rect(area.x + width + gap, area.y, width, area.height),
        temporaryReward,
        art?.TemporaryCurrencyIcon,
        art?.TemporaryRewardFrame,
        TemporaryFallback,
        "◇",
        countStyle);
    }

    public static void DrawPrice(Rect rect, int price, EconomyUiArtSet art, GUIStyle countStyle)
    {
      var iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
      DrawIcon(iconRect, art?.PriceIcon, BaseFallback, "●", countStyle);
      GUI.Label(
        new Rect(rect.x + rect.height + 4f, rect.y, rect.width - rect.height - 4f, rect.height),
        price.ToString(),
        countStyle);
    }

    public static void DrawExitWarning(Rect rect, EconomyUiArtSet art, GUIStyle style)
    {
      var previous = GUI.color;
      var pulse = 0.72f + Mathf.PingPong(Time.unscaledTime * 1.8f, 0.28f);
      GUI.color = new Color(1f, 1f, 1f, pulse);
      DrawIcon(rect, art?.ExitWarningIcon, new Color(0.82f, 0.25f, 0.12f, 0.96f), "!", style);
      GUI.color = previous;
    }

    private static void DrawBalance(
      Rect rect,
      int count,
      Texture2D icon,
      Texture2D frame,
      Color fallback,
      string fallbackGlyph,
      GUIStyle countStyle)
    {
      if (frame != null) GUI.DrawTexture(rect, frame, ScaleMode.StretchToFill, true);
      else GUI.Box(rect, GUIContent.none);
      var padding = Mathf.Max(5f, rect.height * 0.12f);
      var iconSize = rect.height - padding * 2f;
      DrawIcon(
        new Rect(rect.x + padding, rect.y + padding, iconSize, iconSize),
        icon,
        fallback,
        fallbackGlyph,
        countStyle);
      GUI.Label(
        new Rect(
          rect.x + iconSize + padding * 2f,
          rect.y,
          rect.width - iconSize - padding * 3f,
          rect.height),
        count.ToString(),
        countStyle);
    }

    private static void DrawIcon(
      Rect rect,
      Texture2D icon,
      Color fallback,
      string fallbackGlyph,
      GUIStyle style)
    {
      if (icon != null)
      {
        GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
        return;
      }

      var previous = GUI.color;
      GUI.color = fallback;
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true);
      GUI.color = previous;
      GUI.Label(rect, fallbackGlyph, style);
    }
  }
}
