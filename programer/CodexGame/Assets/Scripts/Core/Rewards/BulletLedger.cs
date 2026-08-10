using System;
using System.Collections.Generic;

namespace CodexGame.Core.Rewards
{
  public sealed class BulletLedger
  {
    private readonly HashSet<int> _settledStageNumbers = new HashSet<int>();

    public int Balance { get; private set; }

    public StageBulletReward SettleStageVictory(
      int stageNumber,
      int remainingPlayerHealth,
      int predictionSuccessCount)
    {
      if (stageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(stageNumber));
      if (remainingPlayerHealth < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(remainingPlayerHealth));
      }
      if (predictionSuccessCount < 0
        || predictionSuccessCount > Core.Shared.GameRules.MaximumPredictionSuccessCount)
      {
        throw new ArgumentOutOfRangeException(nameof(predictionSuccessCount));
      }

      if (!_settledStageNumbers.Add(stageNumber)) return StageBulletReward.None;

      var bonus = (remainingPlayerHealth * predictionSuccessCount) / 2;
      var reward = new StageBulletReward(
        remainingPlayerHealth,
        bonus,
        predictionSuccessCount);
      checked
      {
        Balance += reward.TotalBullets;
      }
      return reward;
    }

    public int SettleStageVictory(int stageNumber, int remainingPlayerHealth)
    {
      return SettleStageVictory(stageNumber, remainingPlayerHealth, 0).TotalBullets;
    }

    public bool CanSpend(int amount)
    {
      if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
      return Balance >= amount;
    }

    public bool TrySpend(int amount)
    {
      if (!CanSpend(amount)) return false;
      Balance -= amount;
      return true;
    }
  }
}
