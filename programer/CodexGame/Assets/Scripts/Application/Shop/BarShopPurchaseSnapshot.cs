#nullable enable
using CodexGame.Core.Shop;

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
      bool committed)
    {
      Phase = phase;
      Failure = failure;
      Product = product;
      ElapsedMicroseconds = elapsedMicroseconds;
      InputLocked = inputLocked;
      Committed = committed;
    }

    public BarShopPurchasePhase Phase { get; }
    public BarShopPurchaseFailure Failure { get; }
    public BarShopProductDefinition? Product { get; }
    public long ElapsedMicroseconds { get; }
    public bool InputLocked { get; }
    public bool Committed { get; }
  }
}
