using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public sealed class PileState
  {
    private readonly List<Card> _cards = new List<Card>(GameRules.ExposedCardsPerPile);

    public int Count => _cards.Count;

    public IReadOnlyList<Card> ExposedCards => _cards.AsReadOnly();

    public Card? Expose(Card card)
    {
      if (!card.IsValid)
      {
        throw new ArgumentException("Only a valid card can be exposed.", nameof(card));
      }

      Card? displaced = null;

      if (_cards.Count == GameRules.ExposedCardsPerPile)
      {
        displaced = _cards[0];
        _cards.RemoveAt(0);
      }

      _cards.Add(card);
      return displaced;
    }

    public IReadOnlyList<Card> Clear()
    {
      var removed = _cards.ToArray();
      _cards.Clear();
      return Array.AsReadOnly(removed);
    }
  }
}
