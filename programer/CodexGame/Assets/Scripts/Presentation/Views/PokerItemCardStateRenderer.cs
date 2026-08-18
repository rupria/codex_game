using CodexGame.Core.Cards;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerItemCardStateRenderer
  {
    public static void DrawWildInkState(
      Rect cardRect,
      Card card,
      bool exchangeLocked,
      PokerItemUiArtSet art)
    {
      if (art == null) return;

      var hasWildInk = !card.IsJoker && card.EffectiveSuit != card.Suit;
      if (hasWildInk)
      {
        DrawBottomMarker(cardRect, art.WildInkAppliedMarker, 0.5f);
        DrawBottomMarker(cardRect, art.FindWildInkSuitSeal(card.EffectiveSuit), 1f);
      }

      if (exchangeLocked)
      {
        DrawBottomMarker(cardRect, art.WildInkExchangeLockedMarker, 0f);
      }
    }

    public static void DrawMercenaryTarget(Rect cardRect, PokerItemUiArtSet art)
    {
      DrawBottomMarker(cardRect, art?.MercenaryPlayerTargetMarker, 0.5f);
    }

    private static void DrawBottomMarker(Rect cardRect, Texture2D marker, float horizontalAnchor)
    {
      if (marker == null) return;
      var size = Mathf.Min(30f, cardRect.width * 0.5f);
      var x = Mathf.Lerp(cardRect.x + 2f, cardRect.xMax - size - 2f, horizontalAnchor);
      GUI.DrawTexture(
        new Rect(x, cardRect.yMax - size - 2f, size, size),
        marker,
        ScaleMode.ScaleToFit,
        true);
    }
  }
}
