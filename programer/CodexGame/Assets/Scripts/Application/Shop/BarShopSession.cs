#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;
using CodexGame.Core.Shop;

namespace CodexGame.Application.Shop
{
  public sealed class BarShopSession
  {
    public const int SlotCount = GameRules.BarShopSlotCount;
    public const int DevelopmentRerollCost = 0;

    private static readonly IReadOnlyList<BarShopProductDefinition> Empty =
      Array.AsReadOnly(new BarShopProductDefinition[0]);

    private readonly IReadOnlyList<BarShopProductDefinition> _catalog;
    private IReadOnlyList<BarShopProductDefinition> _slots = Empty;
    private IReadOnlyList<BarShopProductDefinition> _rerollSlots = Empty;
    private bool _isOpen;
    private bool _rerollUsed;

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

      _slots = Take(visible, 0);
      // Reroll is deterministic and may retain products when the catalog has only four entries.
      for (var index = visible.Count - 1; index > 0; index--)
      {
        var swapIndex = random.NextInt(index + 1);
        var value = visible[index];
        visible[index] = visible[swapIndex];
        visible[swapIndex] = value;
      }
      _rerollSlots = Take(visible, 0);
      _rerollUsed = false;
      _isOpen = true;
    }

    public bool TryReroll()
    {
      if (!_isOpen || _rerollUsed) return false;
      _slots = _rerollSlots;
      _rerollUsed = true;
      return true;
    }

    public BarShopSnapshot GetSnapshot(
      BarShopPurchaseSnapshot? purchase = null,
      bool exitWarningArmed = false)
    {
      return new BarShopSnapshot(
        _slots,
        _isOpen && !_rerollUsed && !(purchase?.InputLocked ?? false),
        DevelopmentRerollCost,
        purchase,
        exitWarningArmed);
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
      _rerollSlots = Empty;
      _isOpen = false;
      _rerollUsed = false;
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
