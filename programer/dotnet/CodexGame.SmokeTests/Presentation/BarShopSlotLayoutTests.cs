using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class BarShopSlotLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        BarShopSlotLayout.IconX >= BarShopSlotLayout.IconPlateX
          && BarShopSlotLayout.IconY >= BarShopSlotLayout.IconPlateY
          && BarShopSlotLayout.IconX + BarShopSlotLayout.IconSize
            <= BarShopSlotLayout.IconPlateX + BarShopSlotLayout.IconPlateSize
          && BarShopSlotLayout.IconY + BarShopSlotLayout.IconSize
            <= BarShopSlotLayout.IconPlateY + BarShopSlotLayout.IconPlateSize,
        "The approved 80px shop icon must remain centered inside its 88px state plate.");
      tests.Check(
        !BarShopSlotLayout.VerticalRangesOverlap(
          BarShopSlotLayout.NameY,
          BarShopSlotLayout.NameHeight,
          BarShopSlotLayout.PriceY,
          BarShopSlotLayout.PriceHeight)
          && !BarShopSlotLayout.VerticalRangesOverlap(
            BarShopSlotLayout.PriceY,
            BarShopSlotLayout.PriceHeight,
            BarShopSlotLayout.PurchaseY,
            BarShopSlotLayout.PurchaseHeight),
        "Shop item name, price, and purchase controls must not overlap.");
    }
  }
}
