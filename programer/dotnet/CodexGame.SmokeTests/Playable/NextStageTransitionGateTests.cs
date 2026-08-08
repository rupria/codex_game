using CodexGame.Application.Playable;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Playable
{
  internal static class NextStageTransitionGateTests
  {
    public static void Run(TestHarness tests)
    {
      var gate = new NextStageTransitionGate();
      tests.Check(
        gate.TryRequest(101, At(0)) && !gate.TryRequest(202, At(0)),
        "The next-tavern input must be accepted only once per stage transition.");
      tests.Check(
        gate.MarkLoadComplete(At(0)),
        "The current preloaded presentation must explicitly report load completion.");
      tests.Check(
        gate.GetSnapshot(At(219_999)).Step == NextStageTransitionStep.ShopUiClear
          && gate.GetSnapshot(At(220_000)).Step == NextStageTransitionStep.CameraTurnToExit
          && gate.GetSnapshot(At(1_899_999)).Step
            == NextStageTransitionStep.FadeOutAndBeginLoad,
        "The fixed 1.90-second exit sequence must preserve its authored step boundaries.");
      tests.Check(
        gate.GetSnapshot(At(1_900_000)).Step == NextStageTransitionStep.NextStageFadeIn
          && !gate.IsComplete(At(2_249_999))
          && gate.IsComplete(At(2_250_000)),
        "Input must remain locked through the 0.35-second fade-in after preload.");
      tests.Check(
        gate.TryConsume(At(2_250_000), out var seed)
          && seed == 101
          && !gate.TryConsume(At(2_250_000), out _),
        "A completed next-stage transition seed must be consumed exactly once.");

      gate.Reset();
      tests.Check(
        gate.TryRequest(303, At(0)),
        "The duplicate-input gate must reset for the next completed stage visit.");
      tests.Check(
        gate.GetSnapshot(At(2_049_999)).Step == NextStageTransitionStep.LoadingLoop
          && !gate.GetSnapshot(At(2_049_999)).ShouldShowLoading
          && gate.GetSnapshot(At(2_050_000)).ShouldShowLoading,
        "The loading skull must appear only after the minimum 0.15-second black hold.");
      tests.Check(
        !gate.IsComplete(At(3_000_000))
          && gate.MarkLoadComplete(At(3_000_000))
          && !gate.IsComplete(At(3_349_999))
          && gate.IsComplete(At(3_350_000)),
        "A slow load must hold input and start fade-in only after readiness is reported.");
    }

    private static GameTimestamp At(long microseconds)
    {
      return new GameTimestamp(microseconds);
    }
  }
}
