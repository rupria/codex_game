using System;

namespace CodexGame.Core.Battle
{
  public static class NextStageHealthResolver
  {
    public static BattleHealth RestoreAfterVictory(BattleHealth stageEndHealth)
    {
      if (stageEndHealth.Ai != 0 || stageEndHealth.Player == 0)
      {
        throw new InvalidOperationException(
          "Next-stage health can be restored only after a player stage victory.");
      }

      return BattleHealth.Initial;
    }
  }
}
