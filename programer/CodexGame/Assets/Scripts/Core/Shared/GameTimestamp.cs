using System;

namespace CodexGame.Core.Shared
{
  public readonly struct GameTimestamp : IEquatable<GameTimestamp>, IComparable<GameTimestamp>
  {
    public GameTimestamp(long microseconds)
    {
      if (microseconds < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(microseconds));
      }

      Microseconds = microseconds;
    }

    public long Microseconds { get; }

    public int CompareTo(GameTimestamp other)
    {
      return Microseconds.CompareTo(other.Microseconds);
    }

    public bool Equals(GameTimestamp other)
    {
      return Microseconds == other.Microseconds;
    }

    public override bool Equals(object obj)
    {
      return obj is GameTimestamp other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Microseconds.GetHashCode();
    }

    public static bool operator ==(GameTimestamp left, GameTimestamp right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(GameTimestamp left, GameTimestamp right)
    {
      return !left.Equals(right);
    }
  }
}
