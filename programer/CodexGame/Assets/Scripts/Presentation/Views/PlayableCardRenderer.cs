using CodexGame.Core.Cards;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PlayableCardRenderer
  {
    private readonly PlayableCardArtLibrary _art;
    private readonly PlayableDevStyles _styles;

    public PlayableCardRenderer(PlayableCardArtLibrary art, PlayableDevStyles styles)
    {
      _art = art;
      _styles = styles;
    }

    public void Draw(Card card, float width, float height, bool selected = false)
    {
      var rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
      GUI.Box(rect, GUIContent.none, selected ? _styles.SelectedCard : _styles.Card);
      if (_art != null && _art.TryGetTexture(card, out var texture))
      {
        GUI.DrawTexture(Inset(rect), texture, ScaleMode.ScaleToFit, true);
      }
      else
      {
        GUI.Label(rect, Format(card), _styles.Card);
      }

      if (selected)
      {
        GUI.Label(new Rect(rect.x, rect.y, rect.width, 24f), "SELECTED", _styles.Small);
      }
    }

    public void DrawBack(float width, float height)
    {
      var rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
      GUI.Box(rect, GUIContent.none, _styles.Card);
      if (_art != null && _art.BackTexture != null)
      {
        GUI.DrawTexture(Inset(rect), _art.BackTexture, ScaleMode.ScaleToFit, true);
      }
      else
      {
        GUI.Label(rect, "HIDDEN", _styles.Card);
      }
    }

    public static string FormatInline(Card card)
    {
      return RankText(card.Rank) + " " + SuitText(card.Suit) + " / SKULL " + card.SkullCount;
    }

    private static Rect Inset(Rect rect)
    {
      return new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
    }

    private static string Format(Card card)
    {
      return RankText(card.Rank) + " " + SuitText(card.Suit) + "\nSKULL " + card.SkullCount;
    }

    private static string RankText(CardRank rank)
    {
      switch (rank)
      {
        case CardRank.Ace: return "A";
        case CardRank.King: return "K";
        case CardRank.Queen: return "Q";
        case CardRank.Jack: return "J";
        default: return ((int)rank).ToString();
      }
    }

    private static string SuitText(CardSuit suit)
    {
      switch (suit)
      {
        case CardSuit.Spades: return "SPADE";
        case CardSuit.Diamonds: return "DIAMOND";
        case CardSuit.Hearts: return "HEART";
        default: return "CLUB";
      }
    }
  }
}
