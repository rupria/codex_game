#nullable enable
using System;
using CodexGame.Core.Items;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Core.Shop;

namespace CodexGame.Application.Shop
{
  public sealed class BarShopPurchaseSession
  {
    private BarShopProductDefinition? _product;
    private GameTimestamp _startedAt;
    private BarShopPurchasePhase _phase;
    private BarShopPurchaseFailure _failure;
    private bool _committed;
    private int _bulletCountBefore;
    private int _bulletCountAfter;
    private BulletSpend _plannedSpend;

    public bool IsInputLocked { get; private set; }

    public BarShopPurchaseFailure TryBegin(
      BarShopProductDefinition product,
      BulletLedger bullets,
      RunInventory inventory,
      GameTimestamp now)
    {
      if (product == null) return Reject(BarShopPurchaseFailure.InvalidSlot, now);
      if (bullets == null) throw new ArgumentNullException(nameof(bullets));
      if (inventory == null) throw new ArgumentNullException(nameof(inventory));
      if (IsInputLocked) return BarShopPurchaseFailure.InputLocked;
      _bulletCountBefore = bullets.Balance;
      _bulletCountAfter = bullets.Balance;
      _plannedSpend = BulletSpend.None(bullets.BaseBalance, bullets.TemporaryBalance);
      if (!product.ItemId.HasValue
        || !GameItemCatalog.TryGet(product.ItemId.Value, out _))
      {
        return Reject(BarShopPurchaseFailure.UnknownItem, now);
      }
      if (!bullets.CanSpend(product.Price))
      {
        return Reject(BarShopPurchaseFailure.InsufficientBullets, now);
      }
      if (!bullets.TryPreviewSpend(product.Price, out _plannedSpend))
      {
        throw new InvalidOperationException("An affordable purchase did not produce a spend plan.");
      }

      var inventoryResult = inventory.CanAdd(product.ItemId.Value);
      if (inventoryResult == InventoryAddResult.DuplicateItem)
      {
        return Reject(BarShopPurchaseFailure.DuplicateItem, now);
      }
      if (inventoryResult == InventoryAddResult.InventoryFull)
      {
        return Reject(BarShopPurchaseFailure.InventoryFull, now);
      }
      if (inventoryResult != InventoryAddResult.Added)
      {
        return Reject(BarShopPurchaseFailure.UnknownItem, now);
      }

      _product = product;
      _startedAt = now;
      _phase = BarShopPurchasePhase.Tossing;
      _failure = BarShopPurchaseFailure.None;
      _committed = false;
      _bulletCountAfter = _bulletCountBefore - product.Price;
      IsInputLocked = true;
      return BarShopPurchaseFailure.None;
    }

    public bool Tick(GameTimestamp now, BulletLedger bullets, RunInventory inventory)
    {
      if (bullets == null) throw new ArgumentNullException(nameof(bullets));
      if (inventory == null) throw new ArgumentNullException(nameof(inventory));
      if (!IsInputLocked) return false;

      var elapsed = Math.Max(0, now.Microseconds - _startedAt.Microseconds);
      if (_phase == BarShopPurchasePhase.Tossing
        && !_committed
        && elapsed >= GameRules.BarShopPouchCoverMicroseconds)
      {
        if (_product == null || !_product.ItemId.HasValue)
        {
          throw new InvalidOperationException("A purchase cannot commit without an item product.");
        }
        if (inventory.CanAdd(_product.ItemId.Value) != InventoryAddResult.Added
          || !bullets.TrySpend(_product.Price, out var committedSpend)
          || !SameSpend(_plannedSpend, committedSpend)
          || inventory.TryAdd(_product.ItemId.Value) != InventoryAddResult.Added)
        {
          throw new InvalidOperationException("A validated purchase changed during its input lock.");
        }
        _committed = true;
        _phase = BarShopPurchasePhase.Completed;
      }

      var lockDuration = _phase == BarShopPurchasePhase.Rejected
        ? GameRules.BarShopPurchaseRejectedShakeMicroseconds
        : PurchaseLockDuration(_product?.Price ?? 0);
      if (elapsed < lockDuration) return _committed;

      IsInputLocked = false;
      return _committed;
    }

    public BarShopPurchaseSnapshot GetSnapshot(GameTimestamp now)
    {
      var elapsed = IsInputLocked
        ? Math.Max(0, now.Microseconds - _startedAt.Microseconds)
        : 0;
      return new BarShopPurchaseSnapshot(
        _phase,
        _failure,
        _product,
        elapsed,
        IsInputLocked,
        _committed,
        _bulletCountBefore,
        _bulletCountAfter,
        _plannedSpend);
    }

    public void Reset()
    {
      _product = null;
      _phase = BarShopPurchasePhase.Idle;
      _failure = BarShopPurchaseFailure.None;
      _committed = false;
      _bulletCountBefore = 0;
      _bulletCountAfter = 0;
      _plannedSpend = BulletSpend.None(0, 0);
      IsInputLocked = false;
    }

    private BarShopPurchaseFailure Reject(BarShopPurchaseFailure failure, GameTimestamp now)
    {
      _product = null;
      _startedAt = now;
      _phase = BarShopPurchasePhase.Rejected;
      _failure = failure;
      _committed = false;
      IsInputLocked = true;
      return failure;
    }

    private static bool SameSpend(BulletSpend expected, BulletSpend actual)
    {
      return expected.BaseBefore == actual.BaseBefore
        && expected.TemporaryBefore == actual.TemporaryBefore
        && expected.BaseSpent == actual.BaseSpent
        && expected.TemporarySpent == actual.TemporarySpent;
    }

    private static long PurchaseLockDuration(int price)
    {
      var paymentDuration = price <= 2
        ? GameRules.BarShopCoinFlipDurationMicroseconds
        : GameRules.BarShopBulletPourDurationMicroseconds;
      return GameRules.BarShopPouchCoverMicroseconds + paymentDuration;
    }
  }
}
