#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
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
    private readonly List<IReadOnlyList<BarShopProductDefinition>> _visitSlots =
      new List<IReadOnlyList<BarShopProductDefinition>>();
    private bool _isOpen;
    private int _rerollCount;

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
      var visible = new List<BarShopProductDefinition>();
      for (var index = 0; index < _catalog.Count; index++)
      {
        if (_catalog[index].DisplayState == BarShopProductDisplayState.VisiblePreview)
        {
          visible.Add(_catalog[index]);
        }
      }
      if (visible.Count < SlotCount)
      {
        throw new InvalidOperationException("The visible shop pool must fill all four slots.");
      }

      var random = DeterministicRandomFactory.Create(visitSeed, RandomChannel.BarShop);
      for (var index = visible.Count - 1; index > 0; index--)
      {
        var swapIndex = random.NextInt(index + 1);
        var value = visible[index];
        visible[index] = visible[swapIndex];
        visible[swapIndex] = value;
      }

      _visitSlots.Clear();
      _visitSlots.Add(Take(visible, 0));
      for (var reroll = 0; reroll < GameRules.BarShopMaximumRerolls; reroll++)
      {
        // Each visit gets two deterministic reroll snapshots. With the current
        // four-product catalog this changes ordering without inventing products.
        for (var index = visible.Count - 1; index > 0; index--)
        {
          var swapIndex = random.NextInt(index + 1);
          var value = visible[index];
          visible[index] = visible[swapIndex];
          visible[swapIndex] = value;
        }
        _visitSlots.Add(Take(visible, 0));
      }
      _slots = _visitSlots[0];
      _rerollCount = 0;
      _isOpen = true;
    }

    public bool TryReroll(BulletLedger bullets)
    {
      if (bullets == null) throw new ArgumentNullException(nameof(bullets));
      if (!_isOpen
        || _rerollCount >= GameRules.BarShopMaximumRerolls
        || !bullets.TrySpend(RerollCost)) return false;
      _rerollCount++;
      _slots = _visitSlots[_rerollCount];
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
      _visitSlots.Clear();
      _isOpen = false;
      _rerollCount = 0;
    }

    private static IReadOnlyList<BarShopProductDefinition> Take(
      IReadOnlyList<BarShopProductDefinition> source,
      int start)
    {
      var result = new BarShopProductDefinition[SlotCount];
      for (var index = 0; index < result.Length; index++) result[index] = source[start + index];
      return Array.AsReadOnly(result);
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
