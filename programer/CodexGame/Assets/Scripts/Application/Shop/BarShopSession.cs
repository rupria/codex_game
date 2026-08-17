#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Items;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Core.Shop;

namespace CodexGame.Application.Shop
{
  public sealed class BarShopSession
  {
    public const int SlotCount = GameRules.BarShopSlotCount;
    public const int RerollCost = GameRules.BarShopRerollCost;

    private static readonly IReadOnlyList<BarShopProductDefinition> Empty =
      Array.AsReadOnly(new BarShopProductDefinition[0]);

    private readonly IReadOnlyList<BarShopProductDefinition> _catalog;
    private IReadOnlyList<BarShopProductDefinition> _slots = Empty;
    private bool _isOpen;
    private int _rerollCount;
    private long _visitSeed;

    public BarShopSession(IReadOnlyList<BarShopProductDefinition> catalog)
    {
      if (catalog == null) throw new ArgumentNullException(nameof(catalog));
      if (catalog.Count < SlotCount)
      {
        throw new ArgumentException(
          "The shop requires enough products to fill all four slots.",
          nameof(catalog));
      }
      _catalog = Copy(catalog);
    }

    public void Begin(long visitSeed)
    {
      Begin(visitSeed, null, -1);
    }

    public void Begin(long visitSeed, RunInventory? inventory, int currentHealth)
    {
      _visitSeed = visitSeed;
      _rerollCount = 0;
      _slots = BuildSlots(_rerollCount, inventory, currentHealth);
      _isOpen = true;
    }

    public bool TryReroll(BulletLedger bullets)
    {
      return TryReroll(bullets, null, -1);
    }

    public bool TryReroll(
      BulletLedger bullets,
      RunInventory? inventory,
      int currentHealth)
    {
      if (bullets == null) throw new ArgumentNullException(nameof(bullets));
      if (!_isOpen
        || _rerollCount >= GameRules.BarShopMaximumRerolls
        || !bullets.TrySpend(RerollCost)) return false;
      _rerollCount++;
      _slots = BuildSlots(_rerollCount, inventory, currentHealth);
      return true;
    }

    public BarShopSnapshot GetSnapshot(
      BarShopPurchaseSnapshot? purchase = null,
      bool exitWarningArmed = false,
      int availableBullets = int.MaxValue)
    {
      return new BarShopSnapshot(
        _slots,
        _isOpen
          && _rerollCount < GameRules.BarShopMaximumRerolls
          && availableBullets >= RerollCost
          && !(purchase?.InputLocked ?? false),
        RerollCost,
        purchase,
        exitWarningArmed,
        _rerollCount,
        GameRules.BarShopMaximumRerolls);
    }

    public bool TryGetSlot(int slotIndex, out BarShopProductDefinition? product)
    {
      if (!_isOpen || slotIndex < 0 || slotIndex >= _slots.Count)
      {
        product = null;
        return false;
      }
      product = _slots[slotIndex];
      return true;
    }

    public void Close()
    {
      _slots = Empty;
      _isOpen = false;
      _rerollCount = 0;
      _visitSeed = 0;
    }

    private IReadOnlyList<BarShopProductDefinition> BuildSlots(
      int rerollSequence,
      RunInventory? inventory,
      int currentHealth)
    {
      var eligible = new List<BarShopProductDefinition>();
      for (var index = 0; index < _catalog.Count; index++)
      {
        var product = _catalog[index];
        if (product.DisplayState != BarShopProductDisplayState.VisiblePreview) continue;
        if (product.ItemId.HasValue
          && inventory != null
          && inventory.Contains(product.ItemId.Value)) continue;
        if (product.ItemId == GameItemId.HealthRecovery
          && currentHealth >= GameRules.StartingHealth) continue;
        eligible.Add(product);
      }

      var random = DeterministicRandomFactory.Create(
        MixSeed(_visitSeed, rerollSequence),
        RandomChannel.BarShop);
      for (var index = eligible.Count - 1; index > 0; index--)
      {
        var swapIndex = random.NextInt(index + 1);
        var value = eligible[index];
        eligible[index] = eligible[swapIndex];
        eligible[swapIndex] = value;
      }

      var count = Math.Min(SlotCount, eligible.Count);
      var result = new BarShopProductDefinition[count];
      for (var index = 0; index < result.Length; index++) result[index] = eligible[index];
      return Array.AsReadOnly(result);
    }

    private static long MixSeed(long visitSeed, int rerollSequence)
    {
      unchecked
      {
        return visitSeed ^ (0x5DEECE66DL * (rerollSequence + 1));
      }
    }

    private static IReadOnlyList<BarShopProductDefinition> Copy(
      IReadOnlyList<BarShopProductDefinition> source)
    {
      var result = new BarShopProductDefinition[source.Count];
      for (var index = 0; index < result.Length; index++) result[index] = source[index];
      return Array.AsReadOnly(result);
    }
  }
}
