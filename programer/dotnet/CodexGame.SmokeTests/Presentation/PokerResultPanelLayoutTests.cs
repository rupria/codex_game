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
    }
  }
}
