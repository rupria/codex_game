using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class HalliRemainingCardCountdownStateTests
  {
    public static void Run(TestHarness tests)
    {
      var state = new HalliRemainingCardCountdownState();
      tests.Check(
        !state.Observe(7, 6, false, true, 0d)
          && !state.Observe(7, 5, false, true, 1d)
          && !state.IsVisible(1d),
        "Accepting the fifth remaining player input must not consume the countdown alert during card motion.");
      tests.Check(
        state.Observe(7, 5, true, true, 1.2d)
          && state.ActiveValue == 5
          && state.FrameIndex == 0,
        "The last-five badge must start when the player face-up reveal commits with five inputs left.");
      tests.Check(
        !state.Observe(7, 5, true, true, 1.3d)
          && state.IsVisible(1.67d)
          && !state.IsVisible(1.68d),
        "An unchanged snapshot must not restart the 0.48-second countdown badge.");
      tests.Check(
        !state.Observe(7, 4, false, true, 2d)
          && state.Observe(7, 4, true, true, 2.2d)
          && state.ActiveValue == 4
          && state.FrameIndex == 1,
        "Each decreased value from five through one must select its matching art frame once.");
      tests.Check(
        !state.Observe(8, 3, false, false, 3d)
          && state.Observe(8, 3, false, true, 3.1d)
          && state.ActiveValue == 3,
        "Entering Three Call with five or fewer inputs must show only the current value once.");
      tests.Check(
        !state.Observe(8, 3, false, true, 3.2d)
          && !state.Observe(8, 1, false, true, 4d)
          && state.Observe(8, 1, true, true, 4.2d)
          && state.ActiveValue == 1
          && !state.Observe(8, 0, true, true, 4.3d),
        "One must leave exactly one accepted player input and zero must never start a badge.");
      tests.Check(
        !state.Observe(8, 0, true, false, 4.4d) && !state.IsVisible(4.4d),
        "Early Three Call termination must clear an active countdown without replaying skipped values.");
      tests.Check(
        HalliRemainingCardCountdownState.Scale(0d) < 1f
          && HalliRemainingCardCountdownState.Scale(0.08d) == 1f
          && HalliRemainingCardCountdownState.Alpha(0.28d) == 1f
          && HalliRemainingCardCountdownState.Alpha(0.48d) == 0f,
        "The badge must follow the approved pop, hold, and fade timing.");
    }
  }
}
