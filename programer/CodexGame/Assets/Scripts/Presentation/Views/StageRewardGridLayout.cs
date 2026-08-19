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
    public const float PopupX = 140f;
    public const float PopupY = 90f;
    public const float PopupWidth = 680f;
    public const float PopupHeight = 360f;
    public const float TitleX = 24f;
    public const float TitleY = 20f;
    public const float TitleWidth = 632f;
    public const float TitleHeight = 52f;
    public const float ContentX = 24f;
    public const float ContentY = 88f;
    public const float ContinueX = 220f;
    public const float ContinueY = 288f;
    public const float ContinueWidth = 240f;
    public const float ContinueHeight = 52f;
    public const int Columns = 2;
    public const int Rows = 2;
    public const int PageCapacity = Columns * Rows;
    public const float ContentWidth = 632f;
    public const float ContentHeight = 154f;
    public const float RowWidth = 304f;
    public const float RowHeight = 64f;
    public const float GapX = 24f;
    public const float GapY = 12f;

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
