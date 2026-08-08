using System;
using System.Collections.Generic;

namespace CodexGame.Core.Shop
{
  public static class BarShopCatalog
  {
    private static readonly IReadOnlyList<BarShopProductDefinition> DummyProducts =
      Array.AsReadOnly(new[]
      {
        Product("dummy-01", "UI_BAR_DUMMY_ITEM_01", "bar_shop.item.dummy_01", "BAR_DUMMY_EFFECT_01"),
        Product("dummy-02", "UI_BAR_DUMMY_ITEM_02", "bar_shop.item.dummy_02", "BAR_DUMMY_EFFECT_02"),
        Product("dummy-03", "UI_BAR_DUMMY_ITEM_03", "bar_shop.item.dummy_03", "BAR_DUMMY_EFFECT_03"),
        Product("dummy-04", "UI_BAR_DUMMY_ITEM_04", "bar_shop.item.dummy_04", "BAR_DUMMY_EFFECT_04"),
        Product("dummy-05", "UI_BAR_DUMMY_ITEM_05", "bar_shop.item.dummy_05", "BAR_DUMMY_EFFECT_05"),
        Product("dummy-06", "UI_BAR_DUMMY_ITEM_06", "bar_shop.item.dummy_06", "BAR_DUMMY_EFFECT_06")
      });

    public static IReadOnlyList<BarShopProductDefinition> Dummy => DummyProducts;

    private static BarShopProductDefinition Product(
      string id,
      string nameKey,
      string iconKey,
      string effectKey)
    {
      return new BarShopProductDefinition(
        id,
        nameKey,
        iconKey,
        0,
        effectKey,
        BarShopProductDisplayState.VisiblePreview);
    }
  }
}
