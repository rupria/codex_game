using System;
using System.Collections.Generic;

namespace CodexGame.Core.Rewards
{
  public sealed class BulletLedger
  {
    private readonly HashSet<int> _settledStageNumbers = new HashSet<int>();

    public int BaseBalance { get; private set; }
    public int TemporaryBalance { get; private set; }
    public int Balance => BaseBalance + TemporaryBalance;

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
        BaseBalance += reward.BaseBullets;
        TemporaryBalance += reward.BonusBullets;
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
      return TrySpend(amount, out _);
    }

    public bool TryPreviewSpend(int amount, out BulletSpend spend)
    {
      if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
      if (!CanSpend(amount))
      {
        spend = BulletSpend.None(BaseBalance, TemporaryBalance);
        return false;
      }

      var temporarySpent = Math.Min(amount, TemporaryBalance);
      spend = new BulletSpend(
        BaseBalance,
        TemporaryBalance,
        amount - temporarySpent,
        temporarySpent);
      return true;
    }

    public bool TrySpend(int amount, out BulletSpend spend)
    {
      if (!TryPreviewSpend(amount, out spend)) return false;
      BaseBalance = spend.BaseAfter;
      TemporaryBalance = spend.TemporaryAfter;
      return true;
    }

    public int ExpireTemporary()
    {
      var expired = TemporaryBalance;
      TemporaryBalance = 0;
      return expired;
    }
  }
}
