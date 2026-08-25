using CodexGame.Application.Poker;

namespace CodexGame.Presentation.Views
{
  public enum PokerResultOverlayStep
  {
    Result = 0,
    Prediction = 1
  }

  public readonly struct PokerResultOverlayState
  {
    public const double ResultDurationSeconds = 1.4d;

    private PokerResultOverlayState(PokerResultOverlayStep step)
    {
      Step = step;
    }

    public PokerResultOverlayStep Step { get; }

    public static PokerResultOverlayState FromElapsedSeconds(double elapsedSeconds)
    {
      return new PokerResultOverlayState(
        elapsedSeconds < ResultDurationSeconds
          ? PokerResultOverlayStep.Result
          : PokerResultOverlayStep.Prediction);
    }
  }

  public readonly struct PokerRoundActionVisibility
  {
    private PokerRoundActionVisibility(bool showPredictionActions, bool showContinueAction)
    {
      ShowPredictionActions = showPredictionActions;
      ShowContinueAction = showContinueAction;
    }

    public bool ShowPredictionActions { get; }
    public bool ShowContinueAction { get; }

    public static PokerRoundActionVisibility FromPhase(PokerRoundPhase phase)
    {
      return new PokerRoundActionVisibility(
        phase == PokerRoundPhase.AwaitingPrediction
          || phase == PokerRoundPhase.ResultPending,
        phase == PokerRoundPhase.Resolved);
    }
  }
}
