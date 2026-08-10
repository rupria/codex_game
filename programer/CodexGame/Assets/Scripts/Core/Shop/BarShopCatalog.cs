using System;
using System.Collections.Generic;
using CodexGame.Core.Items;

namespace CodexGame.Core.Shop
{
  public static class BarShopCatalog
  {
    private static readonly IReadOnlyList<BarShopProductDefinition> Products = CreateProducts();

    public static IReadOnlyList<BarShopProductDefinition> All => Products;

    // Compatibility alias for older callers. The catalog now contains the 0.1.2 products.
    public static IReadOnlyList<BarShopProductDefinition> Dummy => Products;

    private static IReadOnlyList<BarShopProductDefinition> CreateProducts()
    {
      var products = new BarShopProductDefinition[GameItemCatalog.All.Count];
      for (var index = 0; index < products.Length; index++)
      {
        products[index] = new BarShopProductDefinition(GameItemCatalog.All[index]);
      }
      return Array.AsReadOnly(products);
    }
  }
}
