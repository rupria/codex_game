using CodexGame.Core.Shared;

namespace CodexGame.Core.Rewards
{
  public sealed class PredictionStreak
  {
    public int SuccessCount { get; private set; }

    public bool Record(PredictionResult result)
    {
      if (result == null || !result.IsCorrect) return false;
      if (SuccessCount >= GameRules.MaximumPredictionSuccessCount) return false;
      SuccessCount++;
      return true;
    }

    public void Reset()
    {
      SuccessCount = 0;
    }
  }
}
