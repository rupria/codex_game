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
      CheckPurchaseTransaction(tests);
      CheckRejectedPurchases(tests);
    }

    private static void CheckVisitAndReroll(TestHarness tests)
    {
      var first = new BarShopSession(BarShopCatalog.All);
      first.Begin(20260809);
      var initial = first.GetSnapshot();
      tests.Check(
        initial.Slots.Count == 4
          && initial.CanReroll
          && initial.RerollCost == 0
          && UniqueCount(initial.Slots) == 4,
        "A bar visit must open four unique product slots with one free reroll.");

      tests.Check(first.TryReroll(), "The first reroll during a bar visit must succeed.");
      var rerolled = first.GetSnapshot();
      tests.Check(
        rerolled.Slots.Count == 4 && !rerolled.CanReroll,
        "The free reroll must replace the four slots and then disable itself.");
      tests.Check(
        !first.TryReroll() && SameIds(rerolled.Slots, first.GetSnapshot().Slots),
        "A second reroll in the same visit must not mutate the shop.");

      var replay = new BarShopSession(BarShopCatalog.All);
      replay.Begin(20260809);
      tests.Check(
        SameIds(initial.Slots, replay.GetSnapshot().Slots),
        "The initial shop layout must be reproducible from the visit seed.");
    }

    private static void CheckProductCatalog(TestHarness tests)
    {
      tests.Check(
        BarShopCatalog.All.Count == 4
          && BarShopCatalog.All[0].ItemId == GameItemId.Reload
          && BarShopCatalog.All[0].Price == 1
          && BarShopCatalog.All[1].ItemId == GameItemId.BottomDeal
          && BarShopCatalog.All[1].Price == 2
          && BarShopCatalog.All[2].ItemId == GameItemId.HypeMan
          && BarShopCatalog.All[2].Price == 2
          && BarShopCatalog.All[3].ItemId == GameItemId.HealthRecovery
          && BarShopCatalog.All[3].Price == 1,
        "The 0.1.2 shop catalog must expose the three poker items and health recovery at fixed prices.");
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
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPurchaseContactMicroseconds - 1),
        ledger,
        inventory);
      tests.Check(
        ledger.Balance == 3 && inventory.Count == 0,
        "Bullets and inventory must not change before the tossed bullet contacts the product.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPurchaseContactMicroseconds),
        ledger,
        inventory);
      tests.Check(
        ledger.Balance == 2 && inventory.Contains(GameItemId.Reload),
        "Purchase cost and item delivery must commit together at the 0.50 second contact point.");
      purchase.Tick(
        new GameTimestamp(GameRules.BarShopPurchaseLockMicroseconds),
        ledger,
        inventory);
      tests.Check(
        !purchase.IsInputLocked && ledger.Balance == 2 && inventory.Count == 1,
        "The 0.60 second purchase lock must end without committing a second time.");
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
