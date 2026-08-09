using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class HalliPileOverlapLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        HalliPileOverlapLayout.X(true, 1) - HalliPileOverlapLayout.X(true, 0) == 84f
          && HalliPileOverlapLayout.X(false, 1) - HalliPileOverlapLayout.X(false, 0) == 84f,
        "Both Halli piles must expose the latest two 96px cards at the approved 84px step.");
      tests.Check(
        HalliPileOverlapLayout.CardWidth - HalliPileOverlapLayout.CardStep == 12f,
        "The approved Halli pile layout must retain a 12px visual overlap.");
    }
  }
}
