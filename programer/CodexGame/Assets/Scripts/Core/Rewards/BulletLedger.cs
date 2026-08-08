using System;
using System.Collections.Generic;

namespace CodexGame.Core.Rewards
{
  public sealed class BulletLedger
  {
    private readonly HashSet<int> _settledStageNumbers = new HashSet<int>();

    public int Balance { get; private set; }

    public int SettleStageVictory(int stageNumber, int remainingPlayerHealth)
    {
      if (stageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(stageNumber));
      if (remainingPlayerHealth < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(remainingPlayerHealth));
      }

      if (!_settledStageNumbers.Add(stageNumber)) return 0;

      checked
      {
        Balance += remainingPlayerHealth;
      }
      return remainingPlayerHealth;
    }
  }
}
