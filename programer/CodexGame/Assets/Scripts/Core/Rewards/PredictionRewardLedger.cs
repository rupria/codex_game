using System;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Rewards
{
  public sealed class PredictionRewardLedger
  {
    public int ItemRewardCount { get; private set; }
    public int CoinIncreaseEventCount { get; private set; }

    public PredictionRewardKind Award(IRandomSource random)
    {
      if (random == null) throw new ArgumentNullException(nameof(random));
      if (random.NextInt(2) == 0)
      {
        ItemRewardCount++;
        return PredictionRewardKind.Item;
      }

      CoinIncreaseEventCount++;
      return PredictionRewardKind.CoinIncrease;
    }
  }
}
