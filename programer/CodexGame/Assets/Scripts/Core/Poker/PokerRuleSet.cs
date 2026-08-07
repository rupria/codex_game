using System;

namespace CodexGame.Core.Poker
{
  public sealed class PokerRuleSet
  {
    public PokerRuleSet(AceStraightMode aceStraightMode)
    {
      if (!Enum.IsDefined(typeof(AceStraightMode), aceStraightMode))
      {
        throw new ArgumentOutOfRangeException(nameof(aceStraightMode));
      }

      AceStraightMode = aceStraightMode;
    }

    public AceStraightMode AceStraightMode { get; }

    public static PokerRuleSet Development { get; } = new PokerRuleSet(AceStraightMode.HighAndLow);
  }
}
