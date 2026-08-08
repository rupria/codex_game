using CodexGame.Core.Cards;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PlayableCardRenderer
  {
    private readonly PlayableCardArtLibrary _art;
    private readonly Texture2D _backTexture;
    private readonly PlayableDevStyles _styles;
    private readonly LocalizationRuntime _localization;

    public PlayableCardRenderer(
      PlayableCardArtLibrary art,
      Texture2D backTexture,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      _art = art;
      _backTexture = backTexture;
      _styles = styles;
      _localization = localization;
    }

    public Rect Draw(Card card, float width, float height, bool selected = false)
    {
      var rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
      DrawAt(rect, card, selected);
      return rect;
    }

    public void DrawAt(Rect rect, Card card, bool selected = false)
    {
      DrawAt(rect, card, selected, true);
    }

    public void DrawAt(Rect rect, Card card, bool selected, bool drawFrame)
    {
      if (drawFrame) GUI.Box(rect, GUIContent.none, selected ? _styles.SelectedCard : _styles.Card);
      if (_art != null && _art.TryGetTexture(card, out var texture))
      {
        GUI.DrawTexture(drawFrame ? Inset(rect) : rect, texture, ScaleMode.ScaleToFit, true);
      }
      else
      {
        GUI.Label(rect, Format(card), _styles.Card);
      }

      if (selected)
      {
        GUI.Label(new Rect(rect.x, rect.y, rect.width, 24f), L("UI_COMMON_SELECTED"), _styles.Small);
      }

    }

    public void DrawBack(float width, float height)
    {
      var rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
      DrawBackAt(rect);
    }

    public void DrawBackAt(Rect rect, float rotationDegrees = 0f)
    {
      DrawBackAt(rect, rotationDegrees, true);
    }

    public void DrawBackAt(Rect rect, float rotationDegrees, bool drawFrame)
    {
      var previousMatrix = GUI.matrix;
      if (Mathf.Abs(rotationDegrees) > 0.01f)
      {
        GUIUtility.RotateAroundPivot(rotationDegrees, rect.center);
      }
      if (drawFrame) GUI.Box(rect, GUIContent.none, _styles.Card);
      if (_backTexture != null)
      {
        GUI.DrawTexture(drawFrame ? Inset(rect) : rect, _backTexture, ScaleMode.ScaleToFit, true);
      }
      else
      {
        GUI.Label(rect, L("UI_COMMON_HIDDEN"), _styles.Card);
      }
      GUI.matrix = previousMatrix;
    }

    public string FormatInline(Card card)
    {
      return RankText(card.Rank) + " " + SuitText(card.Suit) + " / "
        + L("UI_CARD_SKULL_COUNT", new LocalizationArgument("count", card.SkullCount));
    }

    private static Rect Inset(Rect rect)
    {
      return new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
    }

    private string Format(Card card)
    {
      return RankText(card.Rank) + " " + SuitText(card.Suit) + "\n"
        + L("UI_CARD_SKULL_COUNT", new LocalizationArgument("count", card.SkullCount));
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

    private string SuitText(CardSuit suit)
    {
      switch (suit)
      {
        case CardSuit.Spades: return L("UI_SUIT_SPADE");
        case CardSuit.Diamonds: return L("UI_SUIT_DIAMOND");
        case CardSuit.Hearts: return L("UI_SUIT_HEART");
        default: return L("UI_SUIT_CLUB");
      }
    }

    private string L(string key, params LocalizationArgument[] arguments)
    {
      return _localization.Get(key, arguments);
    }
  }
}
