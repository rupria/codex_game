namespace CodexGame.Presentation.Views
{
  internal static class BarShopSlotLayout
  {
    public const float Width = 190f;
    public const float Height = 174f;
    public const float IconPlateX = 51f;
    public const float IconPlateY = 10f;
    public const float IconPlateSize = 88f;
    public const float IconX = 55f;
    public const float IconY = 14f;
    public const float IconSize = 80f;
    public const float NameX = 12f;
    public const float NameY = 112f;
    public const float NameWidth = 166f;
    public const float NameHeight = 20f;
    public const float PriceX = 60f;
    public const float PriceY = 134f;
    public const float PriceWidth = 70f;
    public const float PriceHeight = 28f;
    public const float PurchaseX = 24f;
    public const float PurchaseY = 168f;
    public const float PurchaseWidth = 142f;
    public const float PurchaseHeight = 28f;

    public static bool VerticalRangesOverlap(
      float firstY,
      float firstHeight,
      float secondY,
      float secondHeight)
    {
      return firstY < secondY + secondHeight
        && secondY < firstY + firstHeight;
    }
  }
}
