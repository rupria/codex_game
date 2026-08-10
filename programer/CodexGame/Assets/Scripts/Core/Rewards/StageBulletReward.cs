using System;

namespace CodexGame.Core.Rewards
{
  public readonly struct StageBulletReward
  {
    public StageBulletReward(
      int baseBullets,
      int bonusBullets,
      int predictionSuccessCount)
    {
      if (baseBullets < 0) throw new ArgumentOutOfRangeException(nameof(baseBullets));
      if (bonusBullets < 0) throw new ArgumentOutOfRangeException(nameof(bonusBullets));
      if (predictionSuccessCount < 0) throw new ArgumentOutOfRangeException(nameof(predictionSuccessCount));
      BaseBullets = baseBullets;
      BonusBullets = bonusBullets;
      PredictionSuccessCount = predictionSuccessCount;
    }

    public int BaseBullets { get; }
    public int BonusBullets { get; }
    public int PredictionSuccessCount { get; }
    public int TotalBullets => BaseBullets + BonusBullets;
    public static StageBulletReward None => new StageBulletReward(0, 0, 0);
  }
}
