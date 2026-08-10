using System;

namespace CodexGame.Application.Shop
{
  public enum BarShopExitRequestResult
  {
    WarningArmed = 0,
    Proceed = 1
  }

  public sealed class BarShopExitGuard
  {
    public bool WarningArmed { get; private set; }

    public BarShopExitRequestResult Request(int temporaryBulletCount)
    {
      if (temporaryBulletCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(temporaryBulletCount));
      }

      if (temporaryBulletCount == 0 || WarningArmed)
      {
        WarningArmed = false;
        return BarShopExitRequestResult.Proceed;
      }

      WarningArmed = true;
      return BarShopExitRequestResult.WarningArmed;
    }

    public void Reset()
    {
      WarningArmed = false;
    }
  }
}
