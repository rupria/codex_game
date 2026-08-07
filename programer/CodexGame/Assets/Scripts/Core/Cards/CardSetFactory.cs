using System;
using System.Collections.Generic;

namespace CodexGame.Core.Cards
{
  public static class CardSetFactory
  {
    public static IReadOnlyList<Card> CreateStandard52(ICardSkullPolicy skullPolicy)
    {
      if (skullPolicy == null)
      {
        throw new ArgumentNullException(nameof(skullPolicy));
      }

      var cards = new Card[CardId.CardCount];
      var index = 0;

      for (var suitValue = (int)CardSuit.Clubs; suitValue <= (int)CardSuit.Spades; suitValue++)
      {
        var suit = (CardSuit)suitValue;

        for (var rankValue = (int)CardRank.Two; rankValue <= (int)CardRank.Ace; rankValue++)
        {
          var rank = (CardRank)rankValue;
          var skullCount = skullPolicy.ResolveSkullCount(suit, rank);
          cards[index] = new Card(suit, rank, skullCount);
          index++;
        }
      }

      return Array.AsReadOnly(cards);
    }
  }
}
