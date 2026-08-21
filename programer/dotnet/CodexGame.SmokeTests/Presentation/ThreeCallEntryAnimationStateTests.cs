using System;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class ThreeCallEntryAnimationStateTests
  {
    public static void Run(TestHarness tests)
    {
      var compact = ThreeCallEntryAnimationState.Evaluate(0f);
      var full = ThreeCallEntryAnimationState.Evaluate(0.32f / 1.55f);
      var fading = ThreeCallEntryAnimationState.Evaluate(1.40f / 1.55f);
      var complete = ThreeCallEntryAnimationState.Evaluate(1f);

      tests.Check(
        GameRules.ThreeCallEntryPresentationMicroseconds == 1_550_000,
        "Three Call entry must complete in the approved 1.55-second input-lock window.");
      tests.Check(
        Near(compact.Scale, 0.5f)
          && Near(compact.Alpha, 1f)
          && compact.BellFrameIndex == 0
          && !compact.ShowTitle,
        "Three Call entry must begin as a compact bell plaque without premature title text.");
      tests.Check(
        Near(full.Scale, 1f)
          && Near(full.Alpha, 1f)
          && full.ShowTitle,
        "Three Call entry must reach the full approved layout by 0.32 seconds.");
      tests.Check(
        fading.Alpha > 0f && fading.Alpha < 1f,
        "Three Call entry must fade only during the final 0.30 seconds.");
      tests.Check(
        Near(complete.Alpha, 0f) && complete.BellFrameIndex == 7,
        "Three Call entry must finish transparent on the final bell pulse frame.");
    }

    private static bool Near(float left, float right)
    {
      return Math.Abs(left - right) < 0.0001f;
    }
  }
}

