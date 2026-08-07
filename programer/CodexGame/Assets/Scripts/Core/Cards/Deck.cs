using System;
using System.Collections.Generic;

namespace CodexGame.Core.Cards
{
  public sealed class Deck
  {
    private readonly Card[] _cards;
    private int _nextIndex;

    private Deck(Card[] cards)
    {
      _cards = cards;
    }

    public int RemainingCount => _cards.Length - _nextIndex;

    public static Deck CreateShuffled(IReadOnlyList<Card> cards, IRandomSource random)
    {
      ValidateCardSet(cards);

      if (random == null)
      {
        throw new ArgumentNullException(nameof(random));
      }

      var shuffled = new Card[cards.Count];

      for (var index = 0; index < cards.Count; index++)
      {
        shuffled[index] = cards[index];
      }

      for (var index = shuffled.Length - 1; index > 0; index--)
      {
        var swapIndex = random.NextInt(index + 1);
        var temporary = shuffled[index];
        shuffled[index] = shuffled[swapIndex];
        shuffled[swapIndex] = temporary;
      }

      return new Deck(shuffled);
    }

    public Card Draw()
    {
      if (!TryDraw(out var card))
      {
        throw new InvalidOperationException("The deck is empty.");
      }

      return card;
    }

    public bool TryDraw(out Card card)
    {
      if (_nextIndex >= _cards.Length)
      {
        card = default;
        return false;
      }

      card = _cards[_nextIndex];
      _nextIndex++;
      return true;
    }

    public IReadOnlyList<Card> SnapshotRemaining()
    {
      var remaining = new Card[RemainingCount];
      Array.Copy(_cards, _nextIndex, remaining, 0, remaining.Length);
      return Array.AsReadOnly(remaining);
    }

    private static void ValidateCardSet(IReadOnlyList<Card> cards)
    {
      if (cards == null)
      {
        throw new ArgumentNullException(nameof(cards));
      }

      if (cards.Count != CardId.CardCount)
      {
        throw new ArgumentException("A combat-round deck must contain exactly 52 cards.", nameof(cards));
      }

      var identities = new HashSet<CardId>();

      for (var index = 0; index < cards.Count; index++)
      {
        var card = cards[index];

        if (!card.IsValid)
        {
          throw new ArgumentException("The card set contains an invalid card.", nameof(cards));
        }

        if (!identities.Add(card.Id))
        {
          throw new ArgumentException("The card set contains a duplicate card identity.", nameof(cards));
        }
      }
    }
  }
}
