using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class PokerResultPanelLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        PokerResultPanelLayout.Select(70f, false).Size == PokerResultPanelSize.Compact
          && PokerResultPanelLayout.Select(80f, false).Size == PokerResultPanelSize.Standard
          && PokerResultPanelLayout.Select(120f, false).Size == PokerResultPanelSize.Expanded,
        "Localized result summaries must select the smallest panel height that can contain them.");
      tests.Check(
        PokerResultPanelLayout.Select(70f, true).Size == PokerResultPanelSize.Expanded
          && PokerResultPanelLayout.Select(58f, true).Size == PokerResultPanelSize.Standard,
        "Optional item status chips must reserve their own vertical area.");
      tests.Check(
        PokerResultPanelLayout.ContinueVisualX == 398f
          && PokerResultPanelLayout.ContinueVisualY == 465f
          && PokerResultPanelLayout.ContinueVisualWidth == 164f
          && PokerResultPanelLayout.ContinueVisualHeight == 44f
          && PokerResultPanelLayout.ContinueHitX == 390f
          && PokerResultPanelLayout.ContinueHitY == 455f
          && PokerResultPanelLayout.ContinueHitWidth == 180f
          && PokerResultPanelLayout.ContinueHitHeight == 64f,
        "The resolved continue action must preserve the native 164x44 asset and approved hit area.");
    }
  }
}
