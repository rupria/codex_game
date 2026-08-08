using System;
using System.Collections.Generic;

namespace CodexGame.Core.Shop
{
  public static class BarShopCatalog
  {
    private static readonly IReadOnlyList<BarShopProductDefinition> DummyProducts =
      Array.AsReadOnly(new[]
      {
        Product("dummy-01", "UI_BAR_DUMMY_ITEM_01", "BAR_DUMMY_ICON_01", "BAR_DUMMY_EFFECT_01"),
        Product("dummy-02", "UI_BAR_DUMMY_ITEM_02", "BAR_DUMMY_ICON_02", "BAR_DUMMY_EFFECT_02"),
        Product("dummy-03", "UI_BAR_DUMMY_ITEM_03", "BAR_DUMMY_ICON_03", "BAR_DUMMY_EFFECT_03"),
        Product("dummy-04", "UI_BAR_DUMMY_ITEM_04", "BAR_DUMMY_ICON_04", "BAR_DUMMY_EFFECT_04"),
        Product("dummy-05", "UI_BAR_DUMMY_ITEM_05", "BAR_DUMMY_ICON_05", "BAR_DUMMY_EFFECT_05"),
        Product("dummy-06", "UI_BAR_DUMMY_ITEM_06", "BAR_DUMMY_ICON_06", "BAR_DUMMY_EFFECT_06")
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
