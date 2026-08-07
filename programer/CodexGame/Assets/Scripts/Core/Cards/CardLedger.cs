using System;
using System.Collections.Generic;

namespace CodexGame.Core.Cards
{
  public sealed class CardLedger
  {
    private readonly Dictionary<CardId, Card> _cards = new Dictionary<CardId, Card>();
    private readonly Dictionary<CardId, CardZone> _locations = new Dictionary<CardId, CardZone>();

    public CardLedger(IReadOnlyList<Card> cards)
    {
      if (cards == null)
      {
        throw new ArgumentNullException(nameof(cards));
      }

      if (cards.Count != CardId.CardCount)
      {
        throw new ArgumentException("The ledger must start with exactly 52 cards.", nameof(cards));
      }

      for (var index = 0; index < cards.Count; index++)
      {
        var card = cards[index];

        if (!card.IsValid || !_cards.TryAdd(card.Id, card))
        {
          throw new ArgumentException("The ledger requires 52 valid unique card identities.", nameof(cards));
        }

        _locations.Add(card.Id, CardZone.Deck);
      }

      for (var value = 0; value < CardId.CardCount; value++)
      {
        if (!_cards.ContainsKey(CardId.FromValue(value)))
        {
          throw new ArgumentException("The ledger is missing a standard card identity.", nameof(cards));
        }
      }
    }

    public int TotalCount => _locations.Count;

    public Card GetCard(CardId cardId)
    {
      if (!_cards.TryGetValue(cardId, out var card))
      {
        throw new ArgumentOutOfRangeException(nameof(cardId));
      }

      return card;
    }

    public CardZone GetZone(CardId cardId)
    {
      if (!_locations.TryGetValue(cardId, out var zone))
      {
        throw new ArgumentOutOfRangeException(nameof(cardId));
      }

      return zone;
    }

    public void Move(CardId cardId, CardZone expectedSource, CardZone destination)
    {
      ValidateZone(expectedSource, nameof(expectedSource));
      ValidateZone(destination, nameof(destination));

      var current = GetZone(cardId);

      if (current != expectedSource)
      {
        throw new InvalidOperationException(
          $"Card {cardId.Value} is in {current}, not the expected {expectedSource} zone.");
      }

      if (expectedSource == destination)
      {
        throw new InvalidOperationException("A card move must change its zone.");
      }

      _locations[cardId] = destination;
    }

    public int Count(CardZone zone)
    {
      ValidateZone(zone, nameof(zone));
      var count = 0;

      foreach (var location in _locations.Values)
      {
        if (location == zone)
        {
          count++;
        }
      }

      return count;
    }

    public IReadOnlyList<Card> GetCards(CardZone zone)
    {
      ValidateZone(zone, nameof(zone));
      var cards = new List<Card>();

      for (var value = 0; value < CardId.CardCount; value++)
      {
        var id = CardId.FromValue(value);

        if (_locations[id] == zone)
        {
          cards.Add(_cards[id]);
        }
      }

      return cards.AsReadOnly();
    }

    private static void ValidateZone(CardZone zone, string parameterName)
    {
      if (!Enum.IsDefined(typeof(CardZone), zone))
      {
        throw new ArgumentOutOfRangeException(parameterName);
      }
    }
  }
}
