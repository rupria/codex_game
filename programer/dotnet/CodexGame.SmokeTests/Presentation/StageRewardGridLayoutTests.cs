using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class StageRewardGridLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      var fourth = StageRewardGridLayout.Slot(5, 0, 3);
      var fifth = StageRewardGridLayout.Slot(5, 1, 0);
      tests.Check(
        StageRewardGridLayout.PageCount(5) == 2
          && StageRewardGridLayout.VisibleCount(5, 0) == 4
          && StageRewardGridLayout.VisibleCount(5, 1) == 1
          && fifth.ItemIndex == 4,
        "More than four stage rewards must remain inside a second internal page.");
      tests.Check(
        fourth.X >= 0f
          && fourth.Y >= 0f
          && fourth.X + StageRewardGridLayout.RowWidth <= StageRewardGridLayout.ContentWidth
          && fourth.Y + StageRewardGridLayout.RowHeight <= StageRewardGridLayout.ContentHeight,
        "Every visible reward row must fit inside the 672 by 198 safe content region.");
    }
  }
}
