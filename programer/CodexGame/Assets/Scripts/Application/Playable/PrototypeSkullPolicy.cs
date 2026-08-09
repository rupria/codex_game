using System;
using CodexGame.Core.Cards;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeSkullPolicy : ICardSkullPolicy
  {
    public int ResolveSkullCount(CardSuit suit, CardRank rank)
    {
      // Validate public inputs before resolving the explicit balance table.
      CardId.Create(suit, rank);

      switch (suit)
      {
        case CardSuit.Clubs:
          if (rank == CardRank.Ten) return 3;
          return rank == CardRank.Three
            || rank == CardRank.Six
            || rank == CardRank.Nine
            || rank == CardRank.Queen
            || rank == CardRank.Ace
              ? 2
              : 1;
        case CardSuit.Hearts:
          if (rank == CardRank.Three) return 3;
          return rank == CardRank.Four
            || rank == CardRank.Seven
            || rank == CardRank.Ten
            || rank == CardRank.Ace
              ? 2
              : 1;
        case CardSuit.Diamonds:
          if (rank == CardRank.Six) return 3;
          return rank == CardRank.Two
            || rank == CardRank.Five
            || rank == CardRank.Eight
            || rank == CardRank.Jack
            || rank == CardRank.King
              ? 2
              : 1;
        case CardSuit.Spades:
          if (rank == CardRank.Queen) return 3;
          return rank == CardRank.Four
            || rank == CardRank.Seven
            || rank == CardRank.Ten
            || rank == CardRank.King
              ? 2
              : 1;
        default:
          throw new ArgumentOutOfRangeException(nameof(suit));
      }
    }
  }
}
