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
      EffectiveSuit = suit;
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
      EffectiveSuit = default;
      Rank = default;
      SkullCount = 0;
      JokerKind = jokerKind;
      IsJoker = true;
    }

    public CardId Id { get; }

    public CardSuit Suit { get; }

    public CardSuit EffectiveSuit { get; }

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
          && Enum.IsDefined(typeof(CardSuit), EffectiveSuit)
          && Enum.IsDefined(typeof(CardRank), Rank)
          && SkullCount >= 1
          && SkullCount <= 3
          && Id == CardId.Create(Suit, Rank);
      }
    }

    public Card WithEffectiveSuit(CardSuit effectiveSuit)
    {
      if (IsJoker) throw new InvalidOperationException("A Joker cannot receive a suit override.");
      CardId.ValidateSuit(effectiveSuit);
      return new Card(Id, Suit, effectiveSuit, Rank, SkullCount);
    }

    private Card(
      CardId id,
      CardSuit suit,
      CardSuit effectiveSuit,
      CardRank rank,
      int skullCount)
    {
      Id = id;
      Suit = suit;
      EffectiveSuit = effectiveSuit;
      Rank = rank;
      SkullCount = skullCount;
      JokerKind = default;
      IsJoker = false;
    }
  }
}
