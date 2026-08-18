using System.Collections.Generic;
using CodexGame.Application.Shop;
using CodexGame.Core.Items;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Core.Shop;

namespace CodexGame.SmokeTests.Shop
{
  internal static class BarShopSessionTests
  {
    public static void Run(TestHarness tests)
    {
      CheckVisitAndReroll(tests);
      CheckProductCatalog(tests);
      CheckEligibilityFiltering(tests);
      CheckPurchaseTransaction(tests);
      CheckThreeBulletPurchaseLock(tests);
      CheckRejectedPurchases(tests);
      CheckTemporaryCurrencyPurchase(tests);
      CheckExitWarning(tests);
    }

    private static void CheckVisitAndReroll(TestHarness tests)
    {
      var first = new BarShopSession(BarShopCatalog.All);
      var bullets = FundedLedger();
      first.Begin(20260809);
      var initial = first.GetSnapshot();
      tests.Check(
        initial.Slots.Count == 4
          && initial.CanReroll
          && initial.RerollCost == 1
          && initial.RemainingRerolls == 2
          && UniqueCount(initial.Slots) == 4,
        "A bar visit must open four unique product slots with two one-bullet rerolls.");

      tests.Check(first.TryReroll(bullets), "The first paid reroll during a bar visit must succeed.");
      var firstReroll = first.GetSnapshot(availableBullets: bullets.Balance);
      tests.Check(
        firstReroll.CanReroll
          && firstReroll.RemainingRerolls == 1
          && bullets.Balance == 2,
        "The first reroll must spend exactly one bullet and leave one visit reroll.");
      tests.Check(first.TryReroll(bullets), "The second paid reroll during a bar visit must succeed.");
      var rerolled = first.GetSnapshot(availableBullets: bullets.Balance);
      tests.Check(
        rerolled.Slots.Count == 4
          && !rerolled.CanReroll
          && rerolled.RemainingRerolls == 0
          && bullets.Balance == 1
          && !first.TryReroll(bullets)
          && bullets.Balance == 1
          && SameIds(rerolled.Slots, first.GetSnapshot().Slots),
        "A third reroll must be rejected without spending a bullet or changing the slots.");

      var replay = new BarShopSession(BarShopCatalog.All);
      replay.Begin(20260809);
      tests.Check(
        SameIds(initial.Slots, replay.GetSnapshot().Slots),
        "The initial shop layout must be reproducible from the visit seed.");
    }

    private static void CheckProductCatalog(TestHarness tests)
    {
      tests.Check(
        BarShopCatalog.All.Count == 8
          && BarShopCatalog.All[0].ItemId == GameItemId.Reload
          && BarShopCatalog.All[0].Price == 1
          && BarShopCatalog.All[1].ItemId == GameItemId.BottomDeal
          && BarShopCatalog.All[1].Price == 2
          && BarShopCatalog.All[2].ItemId == GameItemId.HypeMan
          && BarShopCatalog.All[2].Price == 2
          && BarShopCatalog.All[3].ItemId == GameItemId.HealthRecovery
          && BarShopCatalog.All[3].Price == 1
          && BarShopCatalog.All[4].ItemId == GameItemId.WildInk
          && BarShopCatalog.All[4].Price == 3
          && BarShopCatalog.All[5].ItemId == GameItemId.Barrel
          && BarShopCatalog.All[5].Price == 4
          && BarShopCatalog.All[6].ItemId == GameItemId.PredictionInsurance
          && BarShopCatalog.All[6].Price == 3
          && BarShopCatalog.All[7].ItemId == GameItemId.Mercenary
          && BarShopCatalog.All[7].Price == 4,
        "The 0.1.2.5 shop catalog must expose all eight items at the fixed 1/2/2/1/3/4/3/4 prices.");
    }

    private static void CheckEligibilityFiltering(TestHarness tests)
    {
      var owned = new RunInventory();
      owned.TryAdd(GameItemId.Reload);
      var filtered = new BarShopSession(BarShopCatalog.All);
      filtered.Begin(114, owned, 2);
      var slots = filtered.GetSnapshot().Slots;
      var excludesOwned = true;
      for (var index = 0; index < slots.Count; index++)
      {
        excludesOwned &= slots[index].ItemId != GameItemId.Reload;
      }
      tests.Check(
        slots.Count == 4 && excludesOwned,
        "A shop draw must exclude an ItemId already owned by the player.");

      var fullInventory = new RunInventory();
      fullInventory.TryAdd(GameItemId.WildInk);
      fullInventory.TryAdd(GameItemId.Barrel);
      fullInventory.TryAdd(GameItemId.PredictionInsurance);
      fullInventory.TryAdd(GameItemId.Mercenary);
      var sparse = new BarShopSession(BarShopCatalog.All);
      sparse.Begin(114, fullInventory, GameRules.StartingHealth);
      var sparseSlots = sparse.GetSnapshot().Slots;
      var includesHealing = false;
      for (var index = 0; index < sparseSlots.Count; index++)
      {
        includesHealing |= sparseSlots[index].ItemId == GameItemId.HealthRecovery;
      }
      tests.Check(
        sparseSlots.Count == 4 && includesHealing,
        "Health Recovery may be displayed and purchased at full HP even though use stays disabled.");
    }

    private static void CheckPurchaseTransaction(TestHarness tests)
    {
      var ledger = FundedLedger();
      var inventory = new RunInventory();
      var purchase = new BarShopPurchaseSession();
      var product = BarShopCatalog.All[0];

      tests.Check(
        purchase.TryBegin(product, ledger, inventory, new GameTimestamp(0))
          == BarShopPurchaseFailure.None,
        "A valid shop purchase must begin its toss motion.");
      var started = purchase.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        started.BulletCountBefore == 3
          && started.BulletCountAfter == 2
          && started.BaseBulletCountBefore == 3
          && started.BaseBulletCountAfter == 2
          && started.TemporaryBulletsSpent == 0,
        "The purchase snapshot must capture a decreasing bullet count for the pouch rebuild.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPouchCoverMicroseconds - 1),
        ledger,
        inventory);
      tests.Check(
        ledger.Balance == 3 && inventory.Count == 0,
        "Bullets and inventory must not change before the hand fully covers the pouch.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPouchCoverMicroseconds),
        ledger,
        inventory);
      tests.Check(
        ledger.Balance == 2 && inventory.Contains(GameItemId.Reload),
        "Purchase cost and item delivery must commit together while the pouch count is covered.");
      purchase.Tick(
        new GameTimestamp(
          GameRules.BarShopPouchCoverMicroseconds
          + GameRules.BarShopCoinFlipDurationMicroseconds),
        ledger,
        inventory);
      tests.Check(
        !purchase.IsInputLocked && ledger.Balance == 2 && inventory.Count == 1,
        "A one-bullet coin-flip lock must end without committing a second time.");
    }

    private static void CheckThreeBulletPurchaseLock(TestHarness tests)
    {
      var ledger = FundedLedger();
      var inventory = new RunInventory();
      var purchase = new BarShopPurchaseSession();
      var product = new BarShopProductDefinition(
        "test.pour",
        "UI_ITEM_RELOAD",
        "bar_shop.item.reload",
        3,
        "test.pour",
        BarShopProductDisplayState.VisiblePreview,
        GameItemId.Reload);
      tests.Check(
        purchase.TryBegin(product, ledger, inventory, new GameTimestamp(0))
          == BarShopPurchaseFailure.None,
        "A three-bullet product must enter the pour-payment path.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPouchCoverMicroseconds),
        ledger,
        inventory);
      tests.Check(
        ledger.Balance == 0 && purchase.IsInputLocked,
        "A pour payment must deduct the exact price once and stay locked during its 0.75 second motion.");
      purchase.Tick(
        new GameTimestamp(
          GameRules.BarShopPouchCoverMicroseconds
          + GameRules.BarShopBulletPourDurationMicroseconds),
        ledger,
        inventory);
      tests.Check(
        !purchase.IsInputLocked && ledger.Balance == 0 && inventory.Count == 1,
        "A pour payment must unlock after its longer motion without a duplicate deduction.");
    }

    private static void CheckRejectedPurchases(TestHarness tests)
    {
      var emptyLedger = new BulletLedger();
      var inventory = new RunInventory();
      var purchase = new BarShopPurchaseSession();
      tests.Check(
        purchase.TryBegin(
          BarShopCatalog.All[0],
          emptyLedger,
          inventory,
          new GameTimestamp(0)) == BarShopPurchaseFailure.InsufficientBullets,
        "An unaffordable product must reject with the pouch-shake result.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPurchaseRejectedShakeMicroseconds),
        emptyLedger,
        inventory);
      tests.Check(
        emptyLedger.Balance == 0 && inventory.Count == 0 && !purchase.IsInputLocked,
        "A rejected purchase must never mutate bullets or inventory.");

      var ledger = FundedLedger();
      inventory.TryAdd(GameItemId.Reload);
      purchase.Reset();
      tests.Check(
        purchase.TryBegin(
          BarShopCatalog.All[0],
          ledger,
          inventory,
          new GameTimestamp(0)) == BarShopPurchaseFailure.DuplicateItem,
        "The shop must reject a second copy of a unique ItemId.");
    }

    private static BulletLedger FundedLedger()
    {
      var ledger = new BulletLedger();
      ledger.SettleStageVictory(1, 3, 0);
      return ledger;
    }

    private static void CheckTemporaryCurrencyPurchase(TestHarness tests)
    {
      var ledger = new BulletLedger();
      ledger.SettleStageVictory(1, 2, 1);
      var inventory = new RunInventory();
      var purchase = new BarShopPurchaseSession();
      tests.Check(
        purchase.TryBegin(
          BarShopCatalog.All[1],
          ledger,
          inventory,
          new GameTimestamp(0)) == BarShopPurchaseFailure.None,
        "A mixed-balance purchase must begin when the combined balance covers the price.");
      var planned = purchase.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        planned.TemporaryBulletsSpent == 1
          && planned.BaseBulletsSpent == 1
          && planned.TemporaryBulletCountAfter == 0
          && planned.BaseBulletCountAfter == 1,
        "The payment snapshot must expose temporary-first spending for art and motion.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPouchCoverMicroseconds),
        ledger,
        inventory);
      tests.Check(
        ledger.TemporaryBalance == 0
          && ledger.BaseBalance == 1
          && inventory.Contains(GameItemId.BottomDeal),
        "The committed purchase must match the temporary-first spend plan.");
    }

    private static void CheckExitWarning(TestHarness tests)
    {
      var guard = new BarShopExitGuard();
      tests.Check(
        guard.Request(2) == BarShopExitRequestResult.WarningArmed
          && guard.WarningArmed,
        "The first exit input with temporary currency must arm the icon-only warning.");
      tests.Check(
        guard.Request(2) == BarShopExitRequestResult.Proceed
          && !guard.WarningArmed,
        "The second exit input must confirm departure.");
      tests.Check(
        guard.Request(0) == BarShopExitRequestResult.Proceed,
        "An empty temporary balance must leave the shop without an extra confirmation.");
    }

    private static HashSet<string> Ids(IReadOnlyList<BarShopProductDefinition> products)
    {
      var result = new HashSet<string>();
      for (var index = 0; index < products.Count; index++) result.Add(products[index].Id);
      return result;
    }

    private static int UniqueCount(IReadOnlyList<BarShopProductDefinition> products)
    {
      return Ids(products).Count;
    }

    private static bool SameIds(
      IReadOnlyList<BarShopProductDefinition> left,
      IReadOnlyList<BarShopProductDefinition> right)
    {
      if (left.Count != right.Count) return false;
      for (var index = 0; index < left.Count; index++)
      {
        if (left[index].Id != right[index].Id) return false;
      }
      return true;
    }
  }
}
