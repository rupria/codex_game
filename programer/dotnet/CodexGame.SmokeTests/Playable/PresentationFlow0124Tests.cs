using CodexGame.Application.Playable;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Playable
{
  internal static class PresentationFlow0124Tests
  {
    public static void Run(TestHarness tests)
    {
      CheckNormalEntryTiming(tests);
      CheckSkipPreservesGameState(tests);
      CheckTransitionRuleConstants(tests);
    }

    private static void CheckNormalEntryTiming(TestHarness tests)
    {
      var game = new PlayableGameSession();
      game.StartNewBattle(new GameTimestamp(0), 4012);
      tests.Check(
        game.Phase == PlayableGamePhase.StageEntry
          && game.GetSnapshot(new GameTimestamp(0)).Transition.RemainingMicroseconds
            == GameRules.StageEntryPresentationMicroseconds,
        "START must enter the skippable six-second stage presentation first.");

      game.Tick(new GameTimestamp(GameRules.StageEntryPresentationMicroseconds));
      tests.Check(game.Phase == PlayableGamePhase.HalliOpening,
        "The stage presentation must lead into a separate unskippable Three Call entry.");

      var readyAt = new GameTimestamp(
        GameRules.StageEntryPresentationMicroseconds + GameRules.ThreeCallEntryPresentationMicroseconds);
      game.Tick(readyAt);
      var snapshot = game.GetSnapshot(readyAt);
      tests.Check(
        game.Phase == PlayableGamePhase.Halli
          && snapshot.Halli != null
          && snapshot.Halli.RemainingMicroseconds == GameRules.BellInputTimeoutMicroseconds,
        "The 30-second Three Call timer must start only after the full 11-second entry.");
    }

    private static void CheckSkipPreservesGameState(TestHarness tests)
    {
      var normal = new PlayableGameSession();
      var skipped = new PlayableGameSession();
      normal.StartNewBattle(new GameTimestamp(0), 9912);
      skipped.StartNewBattle(new GameTimestamp(0), 9912);

      var normalReady = new GameTimestamp(
        GameRules.StageEntryPresentationMicroseconds + GameRules.ThreeCallEntryPresentationMicroseconds);
      normal.Tick(normalReady);
      var skipAt = new GameTimestamp(1_000_000);
      tests.Check(skipped.SkipStageEntry(skipAt), "SKIP must be accepted only during stage entry.");
      var skippedReady = new GameTimestamp(skipAt.Microseconds + GameRules.ThreeCallEntryPresentationMicroseconds);
      skipped.Tick(skippedReady);

      var normalHalli = normal.GetSnapshot(normalReady).Halli;
      var skippedHalli = skipped.GetSnapshot(skippedReady).Halli;
      tests.Check(
        normalHalli != null
          && skippedHalli != null
          && normalHalli.CombatRoundSeed == skippedHalli.CombatRoundSeed
          && normalHalli.RemainingDeckCards == skippedHalli.RemainingDeckCards
          && normalHalli.LeadActor == skippedHalli.LeadActor
          && !skipped.SkipStageEntry(skippedReady),
        "Skipping presentation must not reroll gameplay state and cannot skip Three Call entry.");
    }

    private static void CheckTransitionRuleConstants(TestHarness tests)
    {
      tests.Check(GameRules.ThreeCallToSelectionPresentationMicroseconds == 2_000_000,
        "Three Call completion must keep input locked for exactly two seconds before selection.");
    }
  }
}
