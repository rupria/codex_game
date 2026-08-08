using CodexGame.Core.Battle;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;

namespace CodexGame.SmokeTests.Battle
{
  internal static class BattleRuleTests
  {
    public static void Run(TestHarness tests)
    {
      var playerLoss = DamageResolver.ApplyPokerLoss(BattleHealth.Initial, PokerWinner.Ai);
      tests.Check(playerLoss.After.Player == 2 && playerLoss.After.Ai == 3,
        "An AI poker win should reduce only player HP by one.");

      var wrongPrediction = PredictionResolver.Resolve(PredictionChoice.PlayerWins, PokerWinner.Ai);
      tests.Check(!wrongPrediction.IsCorrect, "Prediction should report incorrect without changing battle health.");

      var nextStage = NextStageHealthResolver.RestoreAfterVictory(new BattleHealth(2, 0));
      tests.Check(
        nextStage.Player == 3 && nextStage.Ai == 3,
        "A new stage must restore both player and AI to maximum HP instead of carrying two HP forward.");
    }
  }
}
