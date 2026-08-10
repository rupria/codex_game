#nullable enable
using System;
using System.Collections.Generic;

namespace CodexGame.Core.Items
{
  public static class GameItemCatalog
  {
    private static readonly IReadOnlyList<GameItemDefinition> RegisteredItems =
      Array.AsReadOnly(new[]
      {
        new GameItemDefinition(
          GameItemId.Reload,
          "IT-01",
          "UI_ITEM_RELOAD",
          "bar_shop.item.reload",
          1),
        new GameItemDefinition(
          GameItemId.BottomDeal,
          "IT-02",
          "UI_ITEM_BOTTOM_DEAL",
          "bar_shop.item.bottom_deal",
          2),
        new GameItemDefinition(
          GameItemId.HypeMan,
          "IT-03",
          "UI_ITEM_HYPE_MAN",
          "bar_shop.item.hype_man",
          2),
        new GameItemDefinition(
          GameItemId.HealthRecovery,
          "IT-HP-01",
          "UI_ITEM_HEALTH_RECOVERY",
          "bar_shop.item.health_recovery",
          1,
          1)
      });

    public static IReadOnlyList<GameItemDefinition> All => RegisteredItems;

    public static bool TryGet(GameItemId id, out GameItemDefinition? definition)
    {
      for (var index = 0; index < RegisteredItems.Count; index++)
      {
        if (RegisteredItems[index].Id == id)
        {
          definition = RegisteredItems[index];
          return true;
        }
      }

      definition = null;
      return false;
    }
  }
}
