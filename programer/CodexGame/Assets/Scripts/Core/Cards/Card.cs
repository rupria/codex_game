using System;

namespace CodexGame.Core.Cards
{
  public readonly struct Card
  {
    public Card(CardSuit suit, CardRank rank, int skullCount)
    {
      CardId.ValidateSuit(suit);
      CardId.ValidateRank(rank);

      if (skullCount < 1 || skullCount > 3)
      {
        throw new ArgumentOutOfRangeException(nameof(skullCount));
      }

      Id = CardId.Create(suit, rank);
      Suit = suit;
      Rank = rank;
      SkullCount = skullCount;
    }

    public CardId Id { get; }

    public CardSuit Suit { get; }

    public CardRank Rank { get; }

    public int SkullCount { get; }

    public bool IsValid
    {
      get
      {
        return Enum.IsDefined(typeof(CardSuit), Suit)
          && Enum.IsDefined(typeof(CardRank), Rank)
          && SkullCount >= 1
          && SkullCount <= 3
          && Id == CardId.Create(Suit, Rank);
      }
    }
  }
}
