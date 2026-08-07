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
      CheckItemWindow(tests);
      CheckRewardLedger(tests);
    }

    private static void CheckPredictionAndAnnouncement(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(0),
        0);

      var concealed = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        concealed.Phase == PokerRoundPhase.AwaitingPrediction
          && concealed.RemainingMicroseconds == GameRules.PredictionTimeoutMicroseconds,
        "Poker prediction must open a one-minute deadline after the hand locks.");
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
        new GameTimestamp(0),
        0);
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
        "One-minute prediction inactivity must skip reward without changing poker damage.");
    }

    private static void CheckItemWindow(TestHarness tests)
    {
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        Distribution(),
        BattleHealth.Initial,
        PokerRuleSet.Development,
        new GameTimestamp(100),
        1);
      tests.Check(session.GetSnapshot(new GameTimestamp(100)).Phase == PokerRoundPhase.ItemWindow,
        "A stored item reward must expose the pre-lock item window.");
      tests.Check(session.SkipItemWindow(new GameTimestamp(200)), "The item window must support an explicit skip.");
      tests.Check(session.GetSnapshot(new GameTimestamp(200)).HandLocked,
        "Skipping the item window must lock the hand before prediction.");
    }

    private static void CheckRewardLedger(TestHarness tests)
    {
      var ledger = new PredictionRewardLedger();
      var item = ledger.Award(new FixedRandom(0));
      var coin = ledger.Award(new FixedRandom(1));
      tests.Check(
        item == PredictionRewardKind.Item
          && coin == PredictionRewardKind.CoinIncrease
          && ledger.ItemRewardCount == 1
          && ledger.CoinIncreaseEventCount == 1,
        "Correct predictions must accumulate deterministic item or coin-increase reward events.");
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

    private sealed class FixedRandom : IRandomSource
    {
      private readonly int _value;
      public FixedRandom(int value) { _value = value; }
      public int NextInt(int exclusiveMax) { return _value % exclusiveMax; }
    }
  }
}
