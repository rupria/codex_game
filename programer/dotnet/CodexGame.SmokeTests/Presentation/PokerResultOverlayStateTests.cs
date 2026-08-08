using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class PokerResultOverlayStateTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        PokerResultOverlayState.FromElapsedSeconds(0d).Step == PokerResultOverlayStep.Result
          && PokerResultOverlayState.FromElapsedSeconds(1.399d).Step == PokerResultOverlayStep.Result,
        "The result summary must be shown before the prediction outcome.");
      tests.Check(
        PokerResultOverlayState.FromElapsedSeconds(1.4d).Step == PokerResultOverlayStep.Prediction
          && PokerResultOverlayState.FromElapsedSeconds(30d).Step == PokerResultOverlayStep.Prediction,
        "The prediction success or failure message must remain visible until continue.");
    }
  }
}
