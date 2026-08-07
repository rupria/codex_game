using System;

namespace CodexGame.Core.Cards
{
  public sealed class DeterministicRandom : IRandomSource
  {
    private ulong _state;

    internal DeterministicRandom(ulong seed)
    {
      _state = seed;
    }

    public int NextInt(int exclusiveMax)
    {
      if (exclusiveMax <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
      }

      var bound = (ulong)exclusiveMax;
      var threshold = unchecked(0UL - bound) % bound;
      ulong value;

      do
      {
        value = NextUInt64();
      }
      while (value < threshold);

      return (int)(value % bound);
    }

    private ulong NextUInt64()
    {
      _state += 0x9E3779B97F4A7C15UL;
      var value = _state;
      value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
      value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
      return value ^ (value >> 31);
    }
  }
}
