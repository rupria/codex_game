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
      BarShopPurchaseSnapshot? purchase = null,
      bool exitWarningArmed = false,
      int rerollsUsed = 0,
      int maximumRerolls = 0)
    {
      if (slots == null) throw new ArgumentNullException(nameof(slots));
      Slots = Array.AsReadOnly(Copy(slots));
      CanReroll = canReroll;
      RerollCost = rerollCost;
      RerollsUsed = rerollsUsed;
      MaximumRerolls = maximumRerolls;
      Purchase = purchase;
      ExitWarningArmed = exitWarningArmed;
    }

    public IReadOnlyList<BarShopProductDefinition> Slots { get; }
    public bool CanReroll { get; }
    public int RerollCost { get; }
    public int RerollsUsed { get; }
    public int MaximumRerolls { get; }
    public int RemainingRerolls => Math.Max(0, MaximumRerolls - RerollsUsed);
    public BarShopPurchaseSnapshot? Purchase { get; }
    public bool ExitWarningArmed { get; }

    private static BarShopProductDefinition[] Copy(
      IReadOnlyList<BarShopProductDefinition> source)
    {
      var result = new BarShopProductDefinition[source.Count];
      for (var index = 0; index < result.Length; index++) result[index] = source[index];
      return result;
    }
  }
}
