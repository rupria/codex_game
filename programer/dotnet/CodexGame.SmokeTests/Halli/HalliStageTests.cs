using System;
using CodexGame.Core.Halli;

namespace CodexGame.SmokeTests.Halli
{
  internal static class HalliStageTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(HalliStageRules.GetWinTarget(1) == 3, "Combat round 1 must require three Halli wins.");
      tests.Check(HalliStageRules.GetWinTarget(2) == 2, "Combat round 2 must require two Halli wins.");
      tests.Check(HalliStageRules.GetWinTarget(3) == 2, "Combat round 3+ must require two Halli wins.");
      tests.Check(
        HalliStageRules.ResolveEndReason(3, 0, 2, 40, 1) == HalliStageEndReason.PlayerTargetReached,
        "The player target must end the Halli stage.");
      tests.Check(
        HalliStageRules.ResolveEndReason(0, 3, 2, 40, 1) == HalliStageEndReason.AiTargetReached,
        "The AI target must end the Halli stage.");
      tests.Check(
        HalliStageRules.ResolveEndReason(0, 0, 12, 40, 1) == HalliStageEndReason.FlipLimitReached,
        "Twelve four-card distributions must end the Halli stage.");
      tests.Check(
        HalliStageRules.ResolveEndReason(0, 0, 10, 3, 1) == HalliStageEndReason.None,
        "One to three deck cards must still allow the next partial distribution.");
      tests.Check(
        HalliStageRules.ResolveEndReason(0, 0, 10, 0, 1) == HalliStageEndReason.InsufficientCards,
        "The Halli stage must end only when the next requested card does not exist.");
      tests.CheckThrows<ArgumentOutOfRangeException>(
        () => HalliStageRules.GetWinTarget(0),
        "Combat round numbers below one must be rejected.");
    }
  }
}
