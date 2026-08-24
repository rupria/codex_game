using System;
using System.Reflection;
using CodexGame.Application.Playable;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Playable
{
  internal static class PredictionCommitTimingTests
  {
    public static void Run(TestHarness tests)
    {
      var submittedAt = new GameTimestamp(10);
      var poker = new PokerRoundSession();
      poker.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0));
      tests.Check(
        poker.SubmitPrediction(PredictionChoice.PlayerWins, submittedAt),
        "The prediction timing fixture must accept the player-win choice.");

      var game = new PlayableGameSession();
      SetField(game, "_poker", poker);
      SetPhase(game, PlayableGamePhase.PokerPrediction);

      game.Tick(submittedAt);
      tests.Check(
        game.GetSnapshot(submittedAt).PredictionReward.RewardSuccessCount == 0,
        "Prediction success must not be counted when result card reveal begins.");

      var outcomeAt = new GameTimestamp(
        submittedAt.Microseconds + GameRules.PokerResultCardRevealMicroseconds);
      game.Tick(outcomeAt);
      var outcome = game.GetSnapshot(outcomeAt);
      tests.Check(
        outcome.Poker != null
          && outcome.Poker.ResultPresentationStep == PokerResultPresentationStep.Outcome
          && outcome.PredictionReward.RewardSuccessCount == 0,
        "Prediction success must remain hidden while the result message is being shown.");

      var beforeComplete = new GameTimestamp(
        submittedAt.Microseconds + GameRules.PokerResultAnnouncementMicroseconds - 1);
      game.Tick(beforeComplete);
      tests.Check(
        game.GetSnapshot(beforeComplete).PredictionReward.RewardSuccessCount == 0,
        "Prediction success must not commit before the result announcement completes.");

      var completedAt = new GameTimestamp(
        submittedAt.Microseconds + GameRules.PokerResultAnnouncementMicroseconds);
      game.Tick(completedAt);
      var completed = game.GetSnapshot(completedAt);
      tests.Check(
        completed.Phase == PlayableGamePhase.PokerResult
          && completed.PredictionReward.RewardSuccessCount == 1,
        "Prediction success must commit once after the result announcement completes.");
    }

    private static void SetField(object target, string name, object value)
    {
      var field = target.GetType().GetField(
        name,
        BindingFlags.Instance | BindingFlags.NonPublic);
      if (field == null) throw new InvalidOperationException("Missing test field: " + name);
      field.SetValue(target, value);
    }

    private static void SetPhase(PlayableGameSession game, PlayableGamePhase phase)
    {
      var property = typeof(PlayableGameSession).GetProperty(
        nameof(PlayableGameSession.Phase),
        BindingFlags.Instance | BindingFlags.Public);
      if (property == null) throw new InvalidOperationException("Missing playable phase property.");
      property.SetValue(game, phase);
    }

    private static PrivateCardDistributionResult Distribution()
    {
      return new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Diamonds, CardRank.Ace),
          C(CardSuit.Clubs, CardRank.King)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Queen),
          C(CardSuit.Diamonds, CardRank.Jack),
          C(CardSuit.Clubs, CardRank.Ten)
        }),
        C(CardSuit.Hearts, CardRank.Seven),
        Array.AsReadOnly(Array.Empty<Card>()));
    }

    private static Card C(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }
  }
}
