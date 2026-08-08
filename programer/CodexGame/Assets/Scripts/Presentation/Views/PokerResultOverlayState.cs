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
}
