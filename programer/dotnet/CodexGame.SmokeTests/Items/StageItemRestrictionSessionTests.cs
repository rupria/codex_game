using CodexGame.Application.Items;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Items
{
  internal static class StageItemRestrictionSessionTests
  {
    public static void Run(TestHarness tests)
    {
      CheckStageOneIsUnrestricted(tests);
      CheckActivationIsOncePerRun(tests);
      CheckPokerItemsShareTheStageLimit(tests);
    }

    private static void CheckStageOneIsUnrestricted(TestHarness tests)
    {
      var session = new StageItemRestrictionSession();
      session.ResetRun();
      var snapshot = session.EnterStage(1, 10);
      tests.Check(snapshot.WasAssessed && !snapshot.IsActive && session.CanUse,
        "Stage 1 must never activate the item-use restriction.");
    }

    private static void CheckActivationIsOncePerRun(TestHarness tests)
    {
      var seed = FindActivationSeed();
      var session = new StageItemRestrictionSession();
      session.ResetRun();
      session.EnterStage(1, seed);
      var stageTwo = session.EnterStage(2, seed);
      var duplicate = session.EnterStage(2, seed + 1);
      var stageThree = session.EnterStage(3, seed + 2);
      tests.Check(
        stageTwo.IsActive
          && stageTwo.UseLimit >= GameRules.StageItemRestrictionMinimumUses
          && stageTwo.UseLimit <= GameRules.StageItemRestrictionMaximumUses
          && duplicate.UseLimit == stageTwo.UseLimit
          && !stageThree.IsActive,
        "The 20% stage restriction must be assessed once per stage and activate at most once per run.");
    }

    private static void CheckPokerItemsShareTheStageLimit(TestHarness tests)
    {
      var restriction = new StageItemRestrictionSession();
      restriction.ResetRun();
      restriction.EnterStage(1, 1);
      var active = restriction.EnterStage(2, FindActivationSeed());
      for (var index = 0; index < active.UseLimit; index++) restriction.RecordUse();
      tests.Check(!restriction.CanUse && restriction.GetSnapshot().IsExhausted,
        "Item uses from multiple Showdowns must exhaust one shared stage limit.");
    }

    private static long FindActivationSeed()
    {
      for (long seed = 0; seed < 10000; seed++)
      {
        var candidate = new StageItemRestrictionSession();
        candidate.ResetRun();
        candidate.EnterStage(1, seed);
        if (candidate.EnterStage(2, seed).IsActive) return seed;
      }
      throw new System.InvalidOperationException("Could not find a deterministic activation seed.");
    }
  }
}
