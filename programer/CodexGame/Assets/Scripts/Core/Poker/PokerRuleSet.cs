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

    // The 0.06 design leaves A-2-3-4-5 unresolved. The dev build uses high-A only.
    public static PokerRuleSet Development { get; } = new PokerRuleSet(AceStraightMode.HighOnly);
  }
}
