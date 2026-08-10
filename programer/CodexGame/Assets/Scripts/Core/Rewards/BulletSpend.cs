using System;

namespace CodexGame.Core.Rewards
{
  public readonly struct BulletSpend
  {
    public BulletSpend(
      int baseBefore,
      int temporaryBefore,
      int baseSpent,
      int temporarySpent)
    {
      if (baseBefore < 0) throw new ArgumentOutOfRangeException(nameof(baseBefore));
      if (temporaryBefore < 0) throw new ArgumentOutOfRangeException(nameof(temporaryBefore));
      if (baseSpent < 0 || baseSpent > baseBefore)
      {
        throw new ArgumentOutOfRangeException(nameof(baseSpent));
      }
      if (temporarySpent < 0 || temporarySpent > temporaryBefore)
      {
        throw new ArgumentOutOfRangeException(nameof(temporarySpent));
      }

      BaseBefore = baseBefore;
      TemporaryBefore = temporaryBefore;
      BaseSpent = baseSpent;
      TemporarySpent = temporarySpent;
    }

    public int BaseBefore { get; }
    public int TemporaryBefore { get; }
    public int BaseSpent { get; }
    public int TemporarySpent { get; }
    public int BaseAfter => BaseBefore - BaseSpent;
    public int TemporaryAfter => TemporaryBefore - TemporarySpent;
    public int Amount => BaseSpent + TemporarySpent;
    public int TotalBefore => BaseBefore + TemporaryBefore;
    public int TotalAfter => BaseAfter + TemporaryAfter;

    public static BulletSpend None(int baseBalance, int temporaryBalance)
    {
      return new BulletSpend(baseBalance, temporaryBalance, 0, 0);
    }
  }
}
