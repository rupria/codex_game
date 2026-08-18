using System;

namespace CodexGame.Presentation.Views
{
  internal readonly struct StageRewardGridSlot
  {
    public StageRewardGridSlot(int itemIndex, float x, float y)
    {
      ItemIndex = itemIndex;
      X = x;
      Y = y;
    }

    public int ItemIndex { get; }
    public float X { get; }
    public float Y { get; }
  }

  internal static class StageRewardGridLayout
  {
    public const int Columns = 2;
    public const int Rows = 2;
    public const int PageCapacity = Columns * Rows;
    public const float ContentWidth = 672f;
    public const float ContentHeight = 198f;
    public const float RowWidth = 320f;
    public const float RowHeight = 64f;
    public const float GapX = 32f;
    public const float GapY = 8f;

    public static int PageCount(int itemCount)
    {
      if (itemCount < 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
      return Math.Max(1, (itemCount + PageCapacity - 1) / PageCapacity);
    }

    public static int VisibleCount(int itemCount, int pageIndex)
    {
      ValidatePage(itemCount, pageIndex);
      return Math.Min(PageCapacity, Math.Max(0, itemCount - pageIndex * PageCapacity));
    }

    public static StageRewardGridSlot Slot(int itemCount, int pageIndex, int slotIndex)
    {
      var visibleCount = VisibleCount(itemCount, pageIndex);
      if (slotIndex < 0 || slotIndex >= visibleCount)
      {
        throw new ArgumentOutOfRangeException(nameof(slotIndex));
      }

      var column = slotIndex % Columns;
      var row = slotIndex / Columns;
      return new StageRewardGridSlot(
        pageIndex * PageCapacity + slotIndex,
        column * (RowWidth + GapX),
        row * (RowHeight + GapY));
    }

    private static void ValidatePage(int itemCount, int pageIndex)
    {
      if (itemCount < 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
      if (pageIndex < 0 || pageIndex >= PageCount(itemCount))
      {
        throw new ArgumentOutOfRangeException(nameof(pageIndex));
      }
    }
  }
}
