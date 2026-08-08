using System;

namespace CodexGame.Core.Rewards
{
  public sealed class CoinLedger
  {
    public int Balance { get; private set; }

    public void AwardPredictionCoin()
    {
      checked
      {
        Balance++;
      }
    }

    public void AwardStageCoins(int amount)
    {
      if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
      checked
      {
        Balance += amount;
      }
    }
  }
}
