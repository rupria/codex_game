using System;

namespace CodexGame.Core.Cards
{
  public static class DeterministicRandomFactory
  {
    public static IRandomSource Create(long combatRoundSeed, RandomChannel channel)
    {
      if (!Enum.IsDefined(typeof(RandomChannel), channel))
      {
        throw new ArgumentOutOfRangeException(nameof(channel));
      }

      var seed = unchecked((ulong)combatRoundSeed);
      var channelKey = (ulong)channel * 0xD1B54A32D192ED03UL;
      return new DeterministicRandom(seed ^ channelKey);
    }
  }
}
