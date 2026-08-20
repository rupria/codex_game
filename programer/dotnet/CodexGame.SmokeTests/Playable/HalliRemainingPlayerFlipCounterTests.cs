using CodexGame.Application.Playable;

namespace CodexGame.SmokeTests.Playable
{
  internal static class HalliRemainingPlayerFlipCounterTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        HalliRemainingPlayerFlipCounter.Calculate(0, false, 51, false) == 24,
        "Three Call must begin with 24 player flip inputs, not 12 distributions.");
      tests.Check(
        HalliRemainingPlayerFlipCounter.Calculate(10, true, 14, true) == 5
          && HalliRemainingPlayerFlipCounter.Calculate(10, false, 12, true) == 4
          && HalliRemainingPlayerFlipCounter.Calculate(11, true, 10, true) == 3
          && HalliRemainingPlayerFlipCounter.Calculate(11, false, 8, true) == 2
          && HalliRemainingPlayerFlipCounter.Calculate(12, true, 6, true) == 1
          && HalliRemainingPlayerFlipCounter.Calculate(12, false, 4, true) == 0,
        "F-11 must count 5 through 1 on each player-left/right input across distributions 10 through 12.");
      tests.Check(
        HalliRemainingPlayerFlipCounter.Calculate(10, false, 12, false) == 4,
        "An interrupted distribution must not restore its skipped second player input.");
      tests.Check(
        HalliRemainingPlayerFlipCounter.Calculate(11, true, 2, false) == 1
          && HalliRemainingPlayerFlipCounter.Calculate(11, true, 1, false) == 0,
        "The remaining-input count must be capped by complete player/AI card pairs in the deck.");
    }
  }
}
