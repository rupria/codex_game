using System;

namespace CodexGame.Core.Shared
{
  public readonly struct DurationUs : IEquatable<DurationUs>
  {
    public DurationUs(long microseconds)
    {
      if (microseconds < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(microseconds));
      }

      Microseconds = microseconds;
    }

    public long Microseconds { get; }

    public bool Equals(DurationUs other)
    {
      return Microseconds == other.Microseconds;
    }

    public override bool Equals(object obj)
    {
      return obj is DurationUs other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Microseconds.GetHashCode();
    }

    public static bool operator ==(DurationUs left, DurationUs right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(DurationUs left, DurationUs right)
    {
      return !left.Equals(right);
    }
  }
}
