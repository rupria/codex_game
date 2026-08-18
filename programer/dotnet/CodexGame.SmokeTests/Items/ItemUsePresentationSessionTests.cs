using CodexGame.Application.Items;
using CodexGame.Core.Items;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Items
{
  internal static class ItemUsePresentationSessionTests
  {
    public static void Run(TestHarness tests)
    {
      CheckDuration(tests, GameItemId.Reload, 800_000);
      CheckDuration(tests, GameItemId.BottomDeal, 1_000_000);
      CheckDuration(tests, GameItemId.HypeMan, 800_000);
      CheckDuration(tests, GameItemId.HealthRecovery, 500_000);
      CheckInsuranceActivationDuration(tests);
    }

    private static void CheckDuration(TestHarness tests, GameItemId itemId, long duration)
    {
      var session = new ItemUsePresentationSession();
      session.Begin(itemId, new GameTimestamp(100));
      tests.Check(
        session.IsActive
          && session.GetSnapshot(new GameTimestamp(100 + duration / 2)).Progress > 0f
          && !session.Tick(new GameTimestamp(99 + duration))
          && session.Tick(new GameTimestamp(100 + duration))
          && !session.IsActive,
        itemId + " must lock input for its exact 0.1.2.5 presentation duration.");
    }

    private static void CheckInsuranceActivationDuration(TestHarness tests)
    {
      var record = new PredictionRecordAuditEntry(
        1,
        PredictionChoice.PlayerWins,
        false,
        true,
        2,
        1);
      var session = new PredictionInsuranceActivationSession();
      session.Begin(record, new GameTimestamp(100));
      var middle = session.GetSnapshot(new GameTimestamp(200_100));
      tests.Check(
        session.IsActive
          && middle.IsActive
          && middle.RecordSequence == 1
          && middle.ChargesBefore == 2
          && middle.ChargesAfter == 1
          && !session.Tick(new GameTimestamp(400_099))
          && session.Tick(new GameTimestamp(400_100))
          && !session.IsActive,
        "Actual insurance activation must be a distinct deterministic 0.40-second presentation.");
    }
  }
}
