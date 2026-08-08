using CodexGame.Application.Playable;

namespace CodexGame.SmokeTests.Playable
{
  internal static class NextStageTransitionGateTests
  {
    public static void Run(TestHarness tests)
    {
      var gate = new NextStageTransitionGate();
      tests.Check(
        gate.TryRequest(101) && !gate.TryRequest(202),
        "The next-tavern input must be accepted only once per stage transition.");
      tests.Check(
        gate.TryConsume(out var seed) && seed == 101 && !gate.TryConsume(out _),
        "A pending next-stage transition seed must be consumed exactly once.");

      gate.Reset();
      tests.Check(
        gate.TryRequest(303),
        "The duplicate-input gate must reset for the next completed stage visit.");
    }
  }
}
