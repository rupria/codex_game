using System;

namespace CodexGame.Core.Halli
{
  public readonly struct BellWindowId : IEquatable<BellWindowId>
  {
    internal BellWindowId(long value)
    {
      Value = value;
    }

    public long Value { get; }

    public bool IsValid => Value > 0;

    public bool Equals(BellWindowId other)
    {
      return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
      return obj is BellWindowId other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    public static bool operator ==(BellWindowId left, BellWindowId right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(BellWindowId left, BellWindowId right)
    {
      return !left.Equals(right);
    }
  }
}
