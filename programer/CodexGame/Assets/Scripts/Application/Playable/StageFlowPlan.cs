using System;
using System.Collections.Generic;

namespace CodexGame.Application.Playable
{
  public sealed class StageFlowPlan
  {
    private static readonly IReadOnlyList<StageFlowStep> InitialSteps = Array.AsReadOnly(new[]
    {
      StageFlowStep.WalkToTavern,
      StageFlowStep.OpenSaloonDoor,
      StageFlowStep.EnterTavern,
      StageFlowStep.ApproachCenterTable,
      StageFlowStep.SitAtTable,
      StageFlowStep.OpponentReveal,
      StageFlowStep.StageStart
    });

    private static readonly IReadOnlyList<StageFlowStep> NextStageSteps = Array.AsReadOnly(new[]
    {
      StageFlowStep.StageClear,
      StageFlowStep.BulletRewardSettlement,
      StageFlowStep.BarShop,
      StageFlowStep.LeaveTavern,
      StageFlowStep.WalkToNextTavernLoading,
      StageFlowStep.OpenSaloonDoor,
      StageFlowStep.EnterTavern,
      StageFlowStep.OpponentReveal,
      StageFlowStep.ApproachCenterTable,
      StageFlowStep.SitAtTable,
      StageFlowStep.NextStageStart
    });

    private StageFlowPlan(IReadOnlyList<StageFlowStep> steps)
    {
      Steps = steps;
    }

    public IReadOnlyList<StageFlowStep> Steps { get; }

    public static StageFlowPlan InitialStage { get; } = new StageFlowPlan(InitialSteps);
    public static StageFlowPlan AfterStageVictory { get; } = new StageFlowPlan(NextStageSteps);

    public static bool IsBattleInputLocked(StageFlowStep step)
    {
      return step != StageFlowStep.StageStart && step != StageFlowStep.NextStageStart;
    }
  }
}
