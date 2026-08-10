using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Application.Development
{
  public static class PokerCheatPresetCatalog
  {
    public static PokerCheatSetup Create(PokerCheatPreset preset)
    {
      if (preset == PokerCheatPreset.SuitTieBoundary)
      {
        return new PokerCheatSetup(
          Array.AsReadOnly(new[]
          {
            Card(CardSuit.Spades, CardRank.Ace),
            Card(CardSuit.Clubs, CardRank.King),
            Card(CardSuit.Hearts, CardRank.Nine)
          }),
          Array.AsReadOnly(new[]
          {
            Card(CardSuit.Hearts, CardRank.Ace),
            Card(CardSuit.Diamonds, CardRank.King),
            Card(CardSuit.Clubs, CardRank.Nine)
          }),
          Array.AsReadOnly(new[]
          {
            Card(CardSuit.Clubs, CardRank.Two),
            Card(CardSuit.Diamonds, CardRank.Seven)
          }));
      }
      var hand = CreatePlayerHand(preset);
      var publicCards = Array.AsReadOnly(new[] { hand[3], hand[4] });
      var playerCards = Array.AsReadOnly(new[] { hand[0], hand[1], hand[2] });
      var used = new HashSet<CardId>();
      for (var index = 0; index < hand.Count; index++) used.Add(hand[index].Id);

      var aiCards = new List<Card>(3);
      if (preset == PokerCheatPreset.AiJoker)
      {
        var joker = new Card(PokerJokerKind.CrimsonCardsharp);
        aiCards.Add(joker);
        used.Add(joker.Id);
      }
      for (var suit = CardSuit.Clubs; suit <= CardSuit.Spades && aiCards.Count < 3; suit++)
      {
        for (var rank = CardRank.Two; rank <= CardRank.Ace && aiCards.Count < 3; rank++)
        {
          var card = Card(suit, rank);
          if (used.Add(card.Id)) aiCards.Add(card);
        }
      }
      return new PokerCheatSetup(playerCards, aiCards, publicCards);
    }

    private static IReadOnlyList<Card> CreatePlayerHand(PokerCheatPreset preset)
    {
      switch (preset)
      {
        case PokerCheatPreset.NoPair:
          return Hand(Card(CardSuit.Clubs, CardRank.Two), Card(CardSuit.Hearts, CardRank.Four), Card(CardSuit.Diamonds, CardRank.Six), Card(CardSuit.Spades, CardRank.Nine), Card(CardSuit.Clubs, CardRank.Jack));
        case PokerCheatPreset.OnePair:
          return Hand(Card(CardSuit.Clubs, CardRank.Five), Card(CardSuit.Spades, CardRank.Five), Card(CardSuit.Diamonds, CardRank.Eight), Card(CardSuit.Hearts, CardRank.Jack), Card(CardSuit.Clubs, CardRank.Ace));
        case PokerCheatPreset.TwoPair:
          return Hand(Card(CardSuit.Clubs, CardRank.Four), Card(CardSuit.Spades, CardRank.Four), Card(CardSuit.Diamonds, CardRank.Nine), Card(CardSuit.Hearts, CardRank.Nine), Card(CardSuit.Clubs, CardRank.King));
        case PokerCheatPreset.ThreeOfAKind:
          return Hand(Card(CardSuit.Clubs, CardRank.Seven), Card(CardSuit.Hearts, CardRank.Seven), Card(CardSuit.Spades, CardRank.Seven), Card(CardSuit.Diamonds, CardRank.Ten), Card(CardSuit.Clubs, CardRank.Ace));
        case PokerCheatPreset.AceLowStraight:
          return Hand(Card(CardSuit.Spades, CardRank.Ace), Card(CardSuit.Clubs, CardRank.Two), Card(CardSuit.Hearts, CardRank.Three), Card(CardSuit.Diamonds, CardRank.Four), Card(CardSuit.Clubs, CardRank.Five));
        case PokerCheatPreset.AceHighStraight:
          return Hand(Card(CardSuit.Clubs, CardRank.Ten), Card(CardSuit.Hearts, CardRank.Jack), Card(CardSuit.Diamonds, CardRank.Queen), Card(CardSuit.Spades, CardRank.King), Card(CardSuit.Clubs, CardRank.Ace));
        case PokerCheatPreset.Flush:
          return Hand(Card(CardSuit.Hearts, CardRank.Two), Card(CardSuit.Hearts, CardRank.Five), Card(CardSuit.Hearts, CardRank.Eight), Card(CardSuit.Hearts, CardRank.Jack), Card(CardSuit.Hearts, CardRank.Ace));
        case PokerCheatPreset.FullHouse:
          return Hand(Card(CardSuit.Clubs, CardRank.Eight), Card(CardSuit.Hearts, CardRank.Eight), Card(CardSuit.Spades, CardRank.Eight), Card(CardSuit.Clubs, CardRank.Queen), Card(CardSuit.Diamonds, CardRank.Queen));
        case PokerCheatPreset.FourOfAKind:
          return Hand(Card(CardSuit.Clubs, CardRank.Nine), Card(CardSuit.Hearts, CardRank.Nine), Card(CardSuit.Diamonds, CardRank.Nine), Card(CardSuit.Spades, CardRank.Nine), Card(CardSuit.Clubs, CardRank.Ace));
        case PokerCheatPreset.StraightFlush:
          return Hand(Card(CardSuit.Spades, CardRank.Five), Card(CardSuit.Spades, CardRank.Six), Card(CardSuit.Spades, CardRank.Seven), Card(CardSuit.Spades, CardRank.Eight), Card(CardSuit.Spades, CardRank.Nine));
        case PokerCheatPreset.RoyalStraightFlush:
          return Hand(Card(CardSuit.Diamonds, CardRank.Ten), Card(CardSuit.Diamonds, CardRank.Jack), Card(CardSuit.Diamonds, CardRank.Queen), Card(CardSuit.Diamonds, CardRank.King), Card(CardSuit.Diamonds, CardRank.Ace));
        case PokerCheatPreset.PlayerJoker:
          return Hand(new Card(PokerJokerKind.BrassSheriffRevolver), Card(CardSuit.Clubs, CardRank.Two), Card(CardSuit.Hearts, CardRank.Five), Card(CardSuit.Diamonds, CardRank.Nine), Card(CardSuit.Spades, CardRank.King));
        case PokerCheatPreset.AiJoker:
          return Hand(Card(CardSuit.Clubs, CardRank.Three), Card(CardSuit.Hearts, CardRank.Six), Card(CardSuit.Diamonds, CardRank.Eight), Card(CardSuit.Spades, CardRank.Jack), Card(CardSuit.Clubs, CardRank.Ace));
        case PokerCheatPreset.PlayerJokerIneligible:
        case PokerCheatPreset.PlayerJokerNotAwarded:
        case PokerCheatPreset.AiJokerIneligible:
        case PokerCheatPreset.AiJokerNotAwarded:
          return Hand(Card(CardSuit.Clubs, CardRank.Three), Card(CardSuit.Hearts, CardRank.Six), Card(CardSuit.Diamonds, CardRank.Eight), Card(CardSuit.Spades, CardRank.Jack), Card(CardSuit.Clubs, CardRank.Ace));
        default:
          throw new ArgumentOutOfRangeException(nameof(preset));
      }
    }

    private static Card Card(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }

    private static IReadOnlyList<Card> Hand(params Card[] cards)
    {
      return Array.AsReadOnly(cards);
    }
  }
}
