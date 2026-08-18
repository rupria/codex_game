using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class FlipReadyPromptStateTests
  {
    public static void Run(TestHarness tests)
    {
      var state = new FlipReadyPromptState();
      state.Observe(false, 0d);
      state.Observe(true, 1d);
      tests.Check(
        state.IsVisible(1.1d) && state.Alpha(1.2d) > 0.99f,
        "Flip must appear transiently as soon as the next flip becomes available.");
      state.Dismiss();
      tests.Check(
        !state.IsVisible(1.21d),
        "Accepting a flip must dismiss the transient Flip prompt immediately.");
      state.Observe(false, 2d);
      state.Observe(true, 3d);
      tests.Check(
        state.IsVisible(3.1d) && !state.IsVisible(3d + FlipReadyPromptState.VisibleSeconds),
        "Each newly enabled flip opportunity must start one bounded prompt session.");
    }
  }
}
