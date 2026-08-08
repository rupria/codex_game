using System.Collections.Generic;
using CodexGame.Application.Shop;
using CodexGame.Core.Shop;

namespace CodexGame.SmokeTests.Shop
{
  internal static class BarShopSessionTests
  {
    public static void Run(TestHarness tests)
    {
      var first = new BarShopSession(BarShopCatalog.Dummy);
      first.Begin(20260809);
      var initial = first.GetSnapshot();
      tests.Check(
        initial.Slots.Count == 3
          && initial.CanReroll
          && initial.RerollCost == 0
          && UniqueCount(initial.Slots) == 3,
        "A bar visit must open three unique preview slots with one free reroll.");

      var initialIds = Ids(initial.Slots);
      tests.Check(first.TryReroll(), "The first reroll during a bar visit must succeed.");
      var rerolled = first.GetSnapshot();
      tests.Check(
        rerolled.Slots.Count == 3
          && !rerolled.CanReroll
          && NoOverlap(initialIds, rerolled.Slots),
        "The free reroll must replace all three slots and then disable itself.");
      tests.Check(
        !first.TryReroll() && SameIds(rerolled.Slots, first.GetSnapshot().Slots),
        "A second reroll in the same visit must not mutate the shop.");

      var replay = new BarShopSession(BarShopCatalog.Dummy);
      replay.Begin(20260809);
      tests.Check(
        SameIds(initial.Slots, replay.GetSnapshot().Slots),
        "The initial shop layout must be reproducible from the visit seed.");
      replay.TryReroll();
      tests.Check(
        SameIds(rerolled.Slots, replay.GetSnapshot().Slots),
        "The rerolled shop layout must be reproducible from the same visit seed.");

      first.Begin(20260810);
      tests.Check(
        first.GetSnapshot().CanReroll,
        "A new stage shop visit must restore the single free reroll.");

      var product = BarShopCatalog.Dummy[0];
      tests.Check(
        product.Price == 0
          && product.DisplayState == BarShopProductDisplayState.VisiblePreview
          && !string.IsNullOrWhiteSpace(product.LocalizationNameKey)
          && !string.IsNullOrWhiteSpace(product.IconKey)
          && !string.IsNullOrWhiteSpace(product.EffectKey),
        "Every dummy product must expose replaceable data keys without enabling purchase effects.");

      var catalogKeysMatchArtHandoff = BarShopCatalog.Dummy.Count == 6;
      for (var index = 0; index < BarShopCatalog.Dummy.Count; index++)
      {
        catalogKeysMatchArtHandoff &= BarShopCatalog.Dummy[index].IconKey
          == $"bar_shop.item.dummy_{index + 1:00}";
      }
      tests.Check(
        catalogKeysMatchArtHandoff,
        "Dummy product icon keys must match the replaceable BarShop 0.3.0 art catalog.");
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

    private static bool NoOverlap(
      HashSet<string> first,
      IReadOnlyList<BarShopProductDefinition> second)
    {
      for (var index = 0; index < second.Count; index++)
      {
        if (first.Contains(second[index].Id)) return false;
      }
      return true;
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
