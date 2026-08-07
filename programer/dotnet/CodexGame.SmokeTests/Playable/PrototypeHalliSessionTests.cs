using CodexGame.Application.Playable;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Playable
{
  internal static class PrototypeHalliSessionTests
  {
    public static void Run(TestHarness tests)
    {
      var session = new PrototypeHalliSession();
      var zero = new GameTimestamp(0);
      session.StartNew(zero, 20260807);
      var start = session.GetSnapshot(zero);

      tests.Check(start.Phase == PrototypeSessionPhase.ReadyToFlip, "A new prototype session must be ready to flip.");
      tests.Check(start.FirstPublicCard.HasValue, "A new prototype session must open the first public card.");
      tests.Check(start.RemainingDeckCards == 51, "The first public card must leave 51 deck cards.");
      tests.Check(start.WinTarget == 3, "The first combat round must target three Halli wins.");

      session.Advance(new GameTimestamp(1));
      var afterFlip = session.GetSnapshot(new GameTimestamp(1));
      tests.Check(afterFlip.FlipCount == 1, "One advance from ready must flip one card pair.");
      tests.Check(afterFlip.LeftPile.Count == 1 && afterFlip.RightPile.Count == 1, "One flip must expose one card on each pile.");

      var timeoutSession = new PrototypeHalliSession();
      timeoutSession.StartNew(zero, 7);
      timeoutSession.Tick(new GameTimestamp(GameRules.CardFlipTimeoutMicroseconds));
      var timeout = timeoutSession.GetSnapshot(new GameTimestamp(GameRules.CardFlipTimeoutMicroseconds));
      tests.Check(timeout.Phase == PrototypeSessionPhase.Review, "A 30-second flip timeout must enter review.");
      tests.Check(timeout.PlayerWins == 0 && timeout.AiWins == 0, "A flip timeout must not grant a win.");

      var completionSession = new PrototypeHalliSession();
      completionSession.StartNew(zero, 99);
      var now = 1L;

      for (var step = 0; step < 100 && completionSession.GetSnapshot(new GameTimestamp(now)).Phase != PrototypeSessionPhase.Finished; step++)
      {
        completionSession.Advance(new GameTimestamp(now));
        now++;
      }

      var completed = completionSession.GetSnapshot(new GameTimestamp(now));
      tests.Check(completed.Phase == PrototypeSessionPhase.Finished, "The playable Halli slice must reach a terminal state.");
      tests.Check(completed.FlipCount <= 25, "The playable Halli slice must never exceed 25 flips.");
    }
  }
}
