using System;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Poker
{
  internal static class PokerRoundSessionTests
  {
    public static void Run(TestHarness tests)
    {
      CheckPredictionAndAnnouncement(tests);
      CheckPredictionTimeout(tests);
      CheckPublicBuildSkipsItemWindow(tests);
      CheckBulletLedger(tests);
    }

    private static void CheckPredictionAndAnnouncement(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0));

      var concealed = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        concealed.Phase == PokerRoundPhase.AwaitingPrediction
          && concealed.RemainingMicroseconds == GameRules.PredictionTimeoutMicroseconds,
        "Poker prediction must open a two-minute deadline after the hand locks.");
      tests.Check(concealed.VisibleAiPrivateCards.Count == 0, "AI cards must stay concealed before result announcement.");
      tests.Check(session.SubmitPrediction(PredictionChoice.PlayerWins, new GameTimestamp(10)), "A prediction must be accepted once.");
      tests.Check(
        session.GetSnapshot(new GameTimestamp(10)).Phase == PokerRoundPhase.ResultPending,
        "A submitted prediction must enter the bounded result-announcement phase.");
      tests.Check(
        !session.Tick(new GameTimestamp(10 + GameRules.PokerResultAnnouncementMicroseconds - 1)),
        "Poker result must remain pending before the one-second announcement boundary.");
      tests.Check(
        session.Tick(new GameTimestamp(10 + GameRules.PokerResultAnnouncementMicroseconds)),
        "Poker result must announce at the one-second boundary.");

      var revealed = session.GetSnapshot(new GameTimestamp(10 + GameRules.PokerResultAnnouncementMicroseconds));
      tests.Check(revealed.Result != null && revealed.Result.Comparison.Winner == PokerWinner.Player,
        "The pair of aces should win this fixture.");
      tests.Check(revealed.Result != null && revealed.Result.Prediction.IsCorrect,
        "Prediction correctness should be recorded independently.");
      tests.Check(revealed.Health.Player == 3 && revealed.Health.Ai == 2,
        "Only the poker loser should take one HP damage.");
      tests.Check(revealed.VisibleAiPrivateCards.Count == 3, "AI cards must reveal only after result announcement.");
      tests.Check(!session.SubmitPrediction(PredictionChoice.PlayerLoses, new GameTimestamp(20)),
        "A poker prediction must be accepted only once.");
    }

    private static void CheckPredictionTimeout(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0));
      tests.Check(
        !session.Tick(new GameTimestamp(GameRules.PredictionTimeoutMicroseconds)),
        "Prediction timeout must first enter result announcement rather than reveal immediately.");
      tests.Check(
        session.Tick(new GameTimestamp(
          GameRules.PredictionTimeoutMicroseconds + GameRules.PokerResultAnnouncementMicroseconds)),
        "A skipped prediction must resolve after its result announcement delay.");
      var result = session.GetSnapshot(new GameTimestamp(
        GameRules.PredictionTimeoutMicroseconds + GameRules.PokerResultAnnouncementMicroseconds)).Result;
      tests.Check(
        result != null
          && result.Prediction.Choice == PredictionChoice.Skipped
          && !result.Prediction.IsCorrect,
        "Two-minute prediction inactivity must skip reward without changing poker damage.");
    }

    private static void CheckPublicBuildSkipsItemWindow(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(100));
      tests.Check(
        session.GetSnapshot(new GameTimestamp(100)).Phase == PokerRoundPhase.AwaitingPrediction,
        "The current public build must enter prediction directly without an item window.");
    }

    private static void CheckBulletLedger(TestHarness tests)
    {
      var ledger = new BulletLedger();
      tests.Check(
        ledger.SettleStageVictory(1, 3) == 3 && ledger.Balance == 3,
        "A stage victory must award one bullet per remaining player HP.");
      tests.Check(
        ledger.SettleStageVictory(1, 3) == 0 && ledger.Balance == 3,
        "The same stage reward must not be settled twice.");
      tests.Check(
        ledger.SettleStageVictory(2, 1) == 1 && ledger.Balance == 4,
        "A later stage must settle independently without prediction rewards.");
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
