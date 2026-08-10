using System;
using System.Collections.Generic;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Items
{
  public sealed class RunInventory
  {
    private readonly List<GameItemId> _items = new List<GameItemId>();

    public int Count => _items.Count;
    public bool IsFull => _items.Count >= GameRules.InventoryCapacity;

    public bool Contains(GameItemId id)
    {
      return _items.Contains(id);
    }

    public InventoryAddResult CanAdd(GameItemId id)
    {
      if (!Enum.IsDefined(typeof(GameItemId), id)
        || !GameItemCatalog.TryGet(id, out _))
      {
        return InventoryAddResult.UnknownItem;
      }
      if (_items.Contains(id)) return InventoryAddResult.DuplicateItem;
      if (IsFull) return InventoryAddResult.InventoryFull;
      return InventoryAddResult.Added;
    }

    public InventoryAddResult TryAdd(GameItemId id)
    {
      var result = CanAdd(id);
      if (result == InventoryAddResult.Added) _items.Add(id);
      return result;
    }

    public bool TryConsume(GameItemId id)
    {
      return _items.Remove(id);
    }

    public IReadOnlyList<GameItemId> Snapshot()
    {
      return Array.AsReadOnly(_items.ToArray());
    }

    public void Clear()
    {
      _items.Clear();
    }
  }
}
