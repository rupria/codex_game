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
      JokerKind = default;
      IsJoker = false;
    }

    public Card(PokerJokerKind jokerKind)
    {
      if (!Enum.IsDefined(typeof(PokerJokerKind), jokerKind))
      {
        throw new ArgumentOutOfRangeException(nameof(jokerKind));
      }

      Id = CardId.CreateJoker(jokerKind);
      Suit = default;
      Rank = default;
      SkullCount = 0;
      JokerKind = jokerKind;
      IsJoker = true;
    }

    public CardId Id { get; }

    public CardSuit Suit { get; }

    public CardRank Rank { get; }

    public int SkullCount { get; }

    public PokerJokerKind JokerKind { get; }

    public bool IsJoker { get; }

    public bool IsValid
    {
      get
      {
        if (IsJoker)
        {
          return Enum.IsDefined(typeof(PokerJokerKind), JokerKind)
            && Id == CardId.CreateJoker(JokerKind)
            && SkullCount == 0;
        }

        return Enum.IsDefined(typeof(CardSuit), Suit)
          && Enum.IsDefined(typeof(CardRank), Rank)
          && SkullCount >= 1
          && SkullCount <= 3
          && Id == CardId.Create(Suit, Rank);
      }
    }
  }
}
