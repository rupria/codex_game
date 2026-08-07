using System;

namespace CodexGame.Core.Cards
{
  public readonly struct CardId : IEquatable<CardId>
  {
    public const int CardCount = 52;
    public const int RanksPerSuit = 13;

    private CardId(int value)
    {
      Value = value;
    }

    public int Value { get; }

    public static CardId Create(CardSuit suit, CardRank rank)
    {
      ValidateSuit(suit);
      ValidateRank(rank);

      var value = ((int)suit * RanksPerSuit) + ((int)rank - (int)CardRank.Two);
      return new CardId(value);
    }

    public static CardId FromValue(int value)
    {
      if (value < 0 || value >= CardCount)
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      return new CardId(value);
    }

    public bool Equals(CardId other)
    {
      return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
      return obj is CardId other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Value;
    }

    public override string ToString()
    {
      return Value.ToString();
    }

    public static bool operator ==(CardId left, CardId right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(CardId left, CardId right)
    {
      return !left.Equals(right);
    }

    internal static void ValidateSuit(CardSuit suit)
    {
      if (!Enum.IsDefined(typeof(CardSuit), suit))
      {
        throw new ArgumentOutOfRangeException(nameof(suit));
      }
    }

    internal static void ValidateRank(CardRank rank)
    {
      if (!Enum.IsDefined(typeof(CardRank), rank))
      {
        throw new ArgumentOutOfRangeException(nameof(rank));
      }
    }
  }
}
