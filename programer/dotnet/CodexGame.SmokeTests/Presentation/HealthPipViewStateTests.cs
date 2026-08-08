using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class HealthPipViewStateTests
  {
    public static void Run(TestHarness tests)
    {
      for (var health = 0; health <= 3; health++)
      {
        var state = HealthPipViewState.Create(health, 3);
        tests.Check(
          state.FilledCount == health && state.EmptyCount == 3 - health,
          $"HP {health}/3 must render matching filled and empty heart counts.");
      }

      var clamped = HealthPipViewState.Create(8, 3);
      tests.Check(
        clamped.FilledCount == 3 && clamped.EmptyCount == 0,
        "Heart rendering must clamp unexpected health above the configured maximum.");
    }
  }
}
