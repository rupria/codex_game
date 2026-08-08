using CodexGame.Application.Playable;

namespace CodexGame.SmokeTests.Playable
{
  internal static class StageFlowPlanTests
  {
    public static void Run(TestHarness tests)
    {
      var initial = StageFlowPlan.InitialStage.Steps;
      tests.Check(
        initial.Count == 7
          && initial[0] == StageFlowStep.WalkToTavern
          && initial[1] == StageFlowStep.OpenSaloonDoor
          && initial[4] == StageFlowStep.SitAtTable
          && initial[5] == StageFlowStep.OpponentReveal
          && initial[6] == StageFlowStep.StageStart,
        "Initial stage flow must enter the saloon, sit, reveal the opponent, then start.");

      var next = StageFlowPlan.AfterStageVictory.Steps;
      tests.Check(
        next.Count == 11
          && next[0] == StageFlowStep.StageClear
          && next[1] == StageFlowStep.BulletRewardSettlement
          && next[2] == StageFlowStep.BarShop
          && next[3] == StageFlowStep.LeaveTavern
          && next[4] == StageFlowStep.WalkToNextTavernLoading
          && next[10] == StageFlowStep.NextStageStart,
        "Next-stage flow must settle bullets, visit the bar, leave, load, and enter the next saloon.");

      tests.Check(
        StageFlowPlan.IsBattleInputLocked(StageFlowStep.WalkToTavern)
          && StageFlowPlan.IsBattleInputLocked(StageFlowStep.BarShop)
          && !StageFlowPlan.IsBattleInputLocked(StageFlowStep.StageStart),
        "Battle input must stay locked until the stage-start boundary.");
    }
  }
}
