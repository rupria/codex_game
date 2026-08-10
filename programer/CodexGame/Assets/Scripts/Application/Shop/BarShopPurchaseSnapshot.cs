#nullable enable
using CodexGame.Core.Shop;
using CodexGame.Core.Rewards;

namespace CodexGame.Application.Shop
{
  public sealed class BarShopPurchaseSnapshot
  {
    public BarShopPurchaseSnapshot(
      BarShopPurchasePhase phase,
      BarShopPurchaseFailure failure,
      BarShopProductDefinition? product,
      long elapsedMicroseconds,
      bool inputLocked,
      bool committed,
      int bulletCountBefore,
      int bulletCountAfter,
      BulletSpend plannedSpend)
    {
      Phase = phase;
      Failure = failure;
      Product = product;
      ElapsedMicroseconds = elapsedMicroseconds;
      InputLocked = inputLocked;
      Committed = committed;
      BulletCountBefore = bulletCountBefore;
      BulletCountAfter = bulletCountAfter;
      PlannedSpend = plannedSpend;
    }

    public BarShopPurchasePhase Phase { get; }
    public BarShopPurchaseFailure Failure { get; }
    public BarShopProductDefinition? Product { get; }
    public long ElapsedMicroseconds { get; }
    public bool InputLocked { get; }
    public bool Committed { get; }
    public int BulletCountBefore { get; }
    public int BulletCountAfter { get; }
    public BulletSpend PlannedSpend { get; }
    public int BaseBulletCountBefore => PlannedSpend.BaseBefore;
    public int BaseBulletCountAfter => PlannedSpend.BaseAfter;
    public int TemporaryBulletCountBefore => PlannedSpend.TemporaryBefore;
    public int TemporaryBulletCountAfter => PlannedSpend.TemporaryAfter;
    public int BaseBulletsSpent => PlannedSpend.BaseSpent;
    public int TemporaryBulletsSpent => PlannedSpend.TemporarySpent;
  }
}
