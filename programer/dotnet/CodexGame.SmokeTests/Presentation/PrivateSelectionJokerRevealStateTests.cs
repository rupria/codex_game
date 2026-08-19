using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class PrivateSelectionJokerRevealStateTests
  {
    public static void Run(TestHarness tests)
    {
      var noJoker = new PrivateSelectionJokerRevealState();
      noJoker.Observe(10L, false, 4d);
      tests.Check(
        !noJoker.IsActive(4d) && !noJoker.IsInputLocked(4d),
        "A selection without a Joker must open immediately without reveal delay.");

      var reveal = new PrivateSelectionJokerRevealState();
      reveal.Observe(20L, true, 10d);
      tests.Check(
        reveal.Step(10d) == PrivateSelectionJokerRevealStep.Focus
          && reveal.Step(10.15d) == PrivateSelectionJokerRevealStep.Flip
          && reveal.Step(10.35d) == PrivateSelectionJokerRevealStep.Accent,
        "The first Joker reveal must follow focus, flip and readable accent phases.");
      tests.Check(
        reveal.IsInputLocked(10.849d) && !reveal.IsInputLocked(10.85d),
        "Private selection input must unlock exactly when the Joker settles at 0.85 seconds.");
      tests.Check(
        reveal.Step(10.85d) == PrivateSelectionJokerRevealStep.Settle
          && reveal.IsActive(10.999d),
        "The settle treatment must remain visible until the one-second reveal completes.");

      reveal.Observe(20L, true, 11d);
      tests.Check(
        !reveal.IsActive(11d) && !reveal.IsInputLocked(11d),
        "A completed reveal must not replay when the same selection is redrawn.");
      reveal.Observe(20L, true, 20d);
      tests.Check(
        !reveal.IsActive(20d),
        "Focus, language or resolution refreshes must not restart a completed selection reveal.");

      reveal.Observe(21L, true, 30d);
      tests.Check(
        reveal.IsActive(30d) && reveal.IsInputLocked(30d),
        "A different selection session that contains a Joker must start a fresh reveal.");
    }
  }
}
