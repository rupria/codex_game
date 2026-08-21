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
      Rect popupRect,
      int baseReward,
      int temporaryReward,
      EconomyUiArtSet art,
      GUIStyle countStyle)
    {
      if (art?.StageRewardSummaryPanel != null)
      {
        GUI.DrawTexture(popupRect, art.StageRewardSummaryPanel, ScaleMode.StretchToFill, true);
      }
      else GUI.Box(popupRect, GUIContent.none);

      var contentRect = new Rect(
        popupRect.x + StageRewardGridLayout.ContentX,
        popupRect.y + StageRewardGridLayout.ContentY,
        StageRewardGridLayout.ContentWidth,
        StageRewardGridLayout.ContentHeight);
      GUI.BeginGroup(contentRect);
      if (art?.StageRewardContentBackground != null)
      {
        GUI.DrawTexture(
          new Rect(0f, 0f, contentRect.width, contentRect.height),
          art.StageRewardContentBackground,
          ScaleMode.StretchToFill,
          true);
      }
      var first = StageRewardGridLayout.Slot(2, 0, 0);
      DrawRewardRow(
        new Rect(first.X, first.Y, StageRewardGridLayout.RowWidth, StageRewardGridLayout.RowHeight),
        baseReward,
        art?.BaseCurrencyIcon,
        art?.StageRewardBaseRow,
        BaseFallback,
        "●",
        countStyle);
      var second = StageRewardGridLayout.Slot(2, 0, 1);
      DrawRewardRow(
        new Rect(second.X, second.Y, StageRewardGridLayout.RowWidth, StageRewardGridLayout.RowHeight),
        temporaryReward,
        art?.TemporaryCurrencyIcon,
        art?.StageRewardPredictionRow,
        TemporaryFallback,
        "◇",
        countStyle);
      GUI.EndGroup();

      var totalRect = new Rect(
        popupRect.x + StageRewardGridLayout.TotalX,
        popupRect.y + StageRewardGridLayout.TotalY,
        StageRewardGridLayout.TotalWidth,
        StageRewardGridLayout.TotalHeight);
      if (art?.StageRewardTotalRow != null)
      {
        GUI.DrawTexture(totalRect, art.StageRewardTotalRow, ScaleMode.StretchToFill, true);
      }
      else GUI.Box(totalRect, GUIContent.none);
      GUI.Label(
        new Rect(totalRect.x + 184f, totalRect.y + 6f, 116f, 36f),
        "+" + (baseReward + temporaryReward),
        countStyle);
    }

    public static bool DrawStageRewardContinue(
      Rect popupRect,
      EconomyUiArtSet art,
      GUIStyle style,
      string label,
      bool enabled = true)
    {
      var visualRect = new Rect(
        popupRect.x + StageRewardGridLayout.ContinueX,
        popupRect.y + StageRewardGridLayout.ContinueY,
        StageRewardGridLayout.ContinueWidth,
        StageRewardGridLayout.ContinueHeight);
      var hovered = enabled && visualRect.Contains(Event.current.mousePosition);
      var pressed = hovered && Input.GetMouseButton(0);
      var texture = !enabled
        ? art?.StageRewardContinueDisabled
        : pressed
          ? art?.StageRewardContinuePressed
          : hovered
            ? art?.StageRewardContinueHover
            : art?.StageRewardContinueIdle;
      if (texture != null) GUI.DrawTexture(visualRect, texture, ScaleMode.StretchToFill, true);
      else GUI.Box(visualRect, GUIContent.none);
      GUI.Label(new Rect(visualRect.x + 24f, visualRect.y + 8f, 192f, 36f), label, style);
      GUI.enabled = enabled;
      var clicked = GUI.Button(visualRect, GUIContent.none, GUIStyle.none);
      GUI.enabled = true;
      return clicked;
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
      if (art?.ExitWarningPulseSheet != null)
      {
        const int frameCount = 6;
        var frame = Mathf.FloorToInt(Time.unscaledTime / 0.1f) % frameCount;
        GUI.DrawTextureWithTexCoords(
          rect,
          art.ExitWarningPulseSheet,
          new Rect((float)frame / frameCount, 0f, 1f / frameCount, 1f),
          true);
        return;
      }
      var previous = GUI.color;
      var pulse = 0.72f + Mathf.PingPong(Time.unscaledTime * 1.8f, 0.28f);
      GUI.color = new Color(1f, 1f, 1f, pulse);
      DrawIcon(rect, art?.ExitWarningIcon, new Color(0.82f, 0.25f, 0.12f, 0.96f), "!", style);
      GUI.color = previous;
    }

    public static void DrawTemporaryExpiration(
      Rect rect,
      int expiredCount,
      EconomyUiArtSet art,
      GUIStyle style)
    {
      if (expiredCount <= 0) return;
      if (art?.TemporaryExpireSheet != null)
      {
        const int frameCount = 8;
        var frame = Mathf.FloorToInt(Time.unscaledTime / 0.08f) % frameCount;
        GUI.DrawTextureWithTexCoords(
          new Rect(rect.x, rect.y, 40f, 40f),
          art.TemporaryExpireSheet,
          new Rect((float)frame / frameCount, 0f, 1f / frameCount, 1f),
          true);
      }
      else
      {
        DrawIcon(
          new Rect(rect.x, rect.y, 40f, 40f),
          art?.TemporaryCurrencyIcon,
          TemporaryFallback,
          "×",
          style);
      }
      GUI.Label(new Rect(rect.x + 44f, rect.y, rect.width - 44f, 40f), "-" + expiredCount, style);
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

    private static void DrawRewardRow(
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
      DrawIcon(
        new Rect(rect.x + 16f, rect.y + 12f, 40f, 40f),
        icon,
        fallback,
        fallbackGlyph,
        countStyle);
      GUI.Label(
        new Rect(rect.x + 72f, rect.y + 8f, rect.width - 88f, rect.height - 16f),
        "+" + count,
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
