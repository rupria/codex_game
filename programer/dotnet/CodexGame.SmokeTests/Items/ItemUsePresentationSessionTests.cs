using CodexGame.Application.Items;
using CodexGame.Core.Items;
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
  }
}
