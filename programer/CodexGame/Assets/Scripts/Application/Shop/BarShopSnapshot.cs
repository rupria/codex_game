#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Shop;

namespace CodexGame.Application.Shop
{
  public sealed class BarShopSnapshot
  {
    public BarShopSnapshot(
      IReadOnlyList<BarShopProductDefinition> slots,
      bool canReroll,
      int rerollCost,
      BarShopPurchaseSnapshot? purchase = null)
    {
      if (slots == null) throw new ArgumentNullException(nameof(slots));
      Slots = Array.AsReadOnly(Copy(slots));
      CanReroll = canReroll;
      RerollCost = rerollCost;
      Purchase = purchase;
    }

    public IReadOnlyList<BarShopProductDefinition> Slots { get; }
    public bool CanReroll { get; }
    public int RerollCost { get; }
    public BarShopPurchaseSnapshot? Purchase { get; }

    private static BarShopProductDefinition[] Copy(
      IReadOnlyList<BarShopProductDefinition> source)
    {
      var result = new BarShopProductDefinition[source.Count];
      for (var index = 0; index < result.Length; index++) result[index] = source[index];
      return result;
    }
  }
}
