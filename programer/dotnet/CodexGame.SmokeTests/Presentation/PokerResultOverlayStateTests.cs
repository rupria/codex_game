using CodexGame.Presentation.Views;
using CodexGame.Application.Poker;

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

      var awaiting = PokerRoundActionVisibility.FromPhase(PokerRoundPhase.AwaitingPrediction);
      var pending = PokerRoundActionVisibility.FromPhase(PokerRoundPhase.ResultPending);
      var resolved = PokerRoundActionVisibility.FromPhase(PokerRoundPhase.Resolved);
      tests.Check(
        awaiting.ShowPredictionActions
          && !awaiting.ShowContinueAction
          && pending.ShowPredictionActions
          && !pending.ShowContinueAction
          && !resolved.ShowPredictionActions
          && resolved.ShowContinueAction,
        "Resolved must replace both prediction actions with exactly one continue action.");
    }
  }
}
