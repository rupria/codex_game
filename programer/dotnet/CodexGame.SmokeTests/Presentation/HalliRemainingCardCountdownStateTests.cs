using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class HalliRemainingCardCountdownStateTests
  {
    public static void Run(TestHarness tests)
    {
      var state = new HalliRemainingCardCountdownState();
      tests.Check(
        !state.Observe(7, 6, 0d)
          && state.Observe(7, 5, 1d)
          && state.ActiveValue == 5
          && state.FrameIndex == 0,
        "The last-five badge must start only when a successful reveal decreases the deck to five.");
      tests.Check(
        !state.Observe(7, 5, 1.1d)
          && state.IsVisible(1.47d)
          && !state.IsVisible(1.48d),
        "An unchanged snapshot must not restart the 0.48-second countdown badge.");
      tests.Check(
        state.Observe(7, 4, 2d)
          && state.ActiveValue == 4
          && state.FrameIndex == 1,
        "Each decreased value from five through one must select its matching art frame once.");
      tests.Check(
        !state.Observe(8, 3, 3d) && !state.IsVisible(3d),
        "A new combat round must establish a silent baseline instead of replaying an old alert.");
      tests.Check(
        HalliRemainingCardCountdownState.Scale(0d) < 1f
          && HalliRemainingCardCountdownState.Scale(0.08d) == 1f
          && HalliRemainingCardCountdownState.Alpha(0.28d) == 1f
          && HalliRemainingCardCountdownState.Alpha(0.48d) == 0f,
        "The badge must follow the approved pop, hold, and fade timing.");
    }
  }
}
