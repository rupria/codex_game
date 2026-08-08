using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class RopeTimerViewStateTests
  {
    private const long Timeout = 30_000_000;

    public static void Run(TestHarness tests)
    {
      var full = RopeTimerViewState.Create(true, Timeout, Timeout, false);
      tests.Check(
        full.Mode == RopeTimerMode.Normal && full.DisplayedSeconds == 30 && full.RemainingRatio == 1f,
        "The Halli rope must start full at 30 seconds.");

      var eleven = RopeTimerViewState.Create(true, 11_000_000, Timeout, false);
      var ten = RopeTimerViewState.Create(true, 10_000_000, Timeout, false);
      tests.Check(
        eleven.Mode == RopeTimerMode.Normal && ten.Mode == RopeTimerMode.Urgent,
        "The rope must enter its non-colour-only urgent state at the 10 second boundary.");

      var one = RopeTimerViewState.Create(true, 1_000_000, Timeout, false);
      var zero = RopeTimerViewState.Create(true, 0, Timeout, false);
      tests.Check(
        one.DisplayedSeconds == 1 && zero.DisplayedSeconds == 0 && zero.RemainingRatio == 0f,
        "The rope must clamp the 1 to 0 second transition without a negative ratio.");

      var review = RopeTimerViewState.Create(false, 2_000_000, Timeout, false);
      tests.Check(
        review.Mode == RopeTimerMode.Hidden,
        "The 2 second result review lock must never be displayed as the 30 second Halli rope.");

      var explosion = RopeTimerViewState.Create(false, 2_000_000, Timeout, true);
      tests.Check(
        explosion.Mode == RopeTimerMode.Exploding && explosion.DisplayedSeconds == 0,
        "A timeout transition must expose one presentation-only explosion state without changing game rules.");
    }
  }
}
