using CodexGame.Core.Items;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Items
{
  internal static class RunInventoryTests
  {
    public static void Run(TestHarness tests)
    {
      var inventory = new RunInventory();
      tests.Check(
        inventory.TryAdd(GameItemId.Reload) == InventoryAddResult.Added
          && inventory.TryAdd(GameItemId.Reload) == InventoryAddResult.DuplicateItem,
        "Run inventory must enforce unique ItemId values.");
      inventory.TryAdd(GameItemId.BottomDeal);
      inventory.TryAdd(GameItemId.HypeMan);
      inventory.TryAdd(GameItemId.HealthRecovery);
      tests.Check(
        inventory.Count == GameRules.InventoryCapacity && inventory.IsFull,
        "Run inventory must expose exactly four slots.");
      tests.Check(
        inventory.TryConsume(GameItemId.Reload)
          && !inventory.Contains(GameItemId.Reload)
          && inventory.Count == 3,
        "Using an item must consume only that unique inventory entry.");
      inventory.Clear();
      tests.Check(inventory.Count == 0, "Run reset must clear all carried items.");
    }
  }
}
