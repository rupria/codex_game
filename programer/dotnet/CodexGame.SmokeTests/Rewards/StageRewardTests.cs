using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Rewards
{
  internal static class StageRewardTests
  {
    public static void Run(TestHarness tests)
    {
      var streak = new PredictionStreak();
      for (var count = 0; count < 8; count++)
      {
        streak.Record(new PredictionResult(PredictionChoice.PlayerWins, PokerWinner.Player, true));
      }
      streak.Record(new PredictionResult(PredictionChoice.PlayerLoses, PokerWinner.Player, false));
      tests.Check(
        streak.SuccessCount == GameRules.MaximumPredictionSuccessCount,
        "Prediction successes must accumulate without loss and cap at five.");

      var ledger = new BulletLedger();
      var reward = ledger.SettleStageVictory(1, 3, streak.SuccessCount);
      tests.Check(
        reward.BaseBullets == 3
          && reward.BonusBullets == 7
          && reward.TotalBullets == 10
          && ledger.Balance == 10,
        "Stage reward must be HP plus floor(HP x successful predictions x 0.5).");
      tests.Check(
        ledger.SettleStageVictory(1, 3, 5).TotalBullets == 0 && ledger.Balance == 10,
        "A stage reward must remain idempotent after prediction bonus settlement.");
      tests.Check(
        ledger.TrySpend(2) && ledger.Balance == 8 && !ledger.TrySpend(9),
        "Bullet spending must be atomic and reject an unaffordable cost.");

      var expected = new[,]
      {
        { 1, 1, 2, 2, 3, 3 },
        { 2, 3, 4, 5, 6, 7 },
        { 3, 4, 6, 7, 9, 10 }
      };
      var matrixMatches = true;
      for (var hp = 1; hp <= 3; hp++)
      {
        for (var successes = 0; successes <= 5; successes++)
        {
          var matrixLedger = new BulletLedger();
          var matrixReward = matrixLedger.SettleStageVictory(1, hp, successes);
          matrixMatches &= matrixReward.TotalBullets == expected[hp - 1, successes];
        }
      }
      tests.Check(
        matrixMatches,
        "All 18 HP and prediction-success reward combinations must match the 0.1.2 matrix.");
    }
  }
}
