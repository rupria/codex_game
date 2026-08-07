using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
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

      var secondRound = new PrototypeHalliSession();
      secondRound.StartNew(zero, 11, 2);
      tests.Check(secondRound.GetSnapshot(zero).WinTarget == 2, "The second combat round must target two Halli wins.");

      var thirdRound = new PrototypeHalliSession();
      thirdRound.StartNew(zero, 12, 3);
      tests.Check(thirdRound.GetSnapshot(zero).WinTarget == 1, "The third combat round must target one Halli win.");

      session.Advance(new GameTimestamp(1));
      var afterFlip = session.GetSnapshot(new GameTimestamp(1));
      tests.Check(afterFlip.FlipCount == 1, "One advance from ready must flip one card pair.");
      tests.Check(afterFlip.LeftPile.Count == 1 && afterFlip.RightPile.Count == 1, "One flip must expose one card on each pile.");

      var timeoutSession = new PrototypeHalliSession();
      timeoutSession.StartNew(zero, 7);
      timeoutSession.Tick(new GameTimestamp(GameRules.CardFlipTimeoutMicroseconds));
      var timeout = timeoutSession.GetSnapshot(new GameTimestamp(GameRules.CardFlipTimeoutMicroseconds));
      tests.Check(timeout.Phase == PrototypeSessionPhase.ReadyToFlip, "A 30-second flip timeout must immediately reopen flip input.");
      tests.Check(timeout.PlayerWins == 0 && timeout.AiWins == 0, "A flip timeout must not grant a win.");
      tests.Check(timeout.RemainingMicroseconds == GameRules.CardFlipTimeoutMicroseconds, "A flip timeout must restart the 30-second deadline.");

      CheckAcquisitionReview(tests, zero);

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

    private static void CheckAcquisitionReview(TestHarness tests, GameTimestamp zero)
    {
      for (var seed = 1L; seed <= 1000L; seed++)
      {
        var session = new PrototypeHalliSession();
        var flipTime = new GameTimestamp(1);
        session.StartNew(zero, seed);
        session.Advance(flipTime);
        var bell = session.GetSnapshot(flipTime);

        if (bell.Phase != PrototypeSessionPhase.BellOpen)
        {
          continue;
        }

        var selectedPile = IsAcquirable(Evaluate(bell.LeftPile))
          ? PileSide.Left
          : PileSide.Right;
        session.Ring(selectedPile, flipTime);
        var review = session.GetSnapshot(flipTime);

        tests.Check(review.Phase == PrototypeSessionPhase.Review, "A correct bell must enter the 15-second review.");
        tests.Check(review.LastAcquirer == PrototypeAcquirer.Player, "A player bell inside the tie threshold must win.");
        tests.Check(review.LastAcquiredCards.Count > 0, "The review must expose the cards that were actually acquired.");
        tests.Check(review.PlayerAcquiredCount == review.LastAcquiredCards.Count, "The player ledger count must include the reviewed cards.");

        var reviewEnd = new GameTimestamp(1 + GameRules.ReviewGraceMicroseconds);
        session.Tick(reviewEnd);
        var ready = session.GetSnapshot(reviewEnd);
        tests.Check(ready.Phase == PrototypeSessionPhase.ReadyToFlip, "The review must end after 15 seconds.");
        tests.Check(ready.LastAcquirer == PrototypeAcquirer.None, "Reviewed AI or player cards must be hidden after the grace period.");
        return;
      }

      tests.Check(false, "At least one deterministic seed must open a bell opportunity on the first flip.");
    }

    private static AcquisitionKind Evaluate(System.Collections.Generic.IReadOnlyList<Card> cards)
    {
      var first = cards.Count > 0 ? cards[0] : (Card?)null;
      var second = cards.Count > 1 ? cards[1] : (Card?)null;
      return SkullAcquisitionResolver.Resolve(first, second);
    }

    private static bool IsAcquirable(AcquisitionKind result)
    {
      return result == AcquisitionKind.Both
        || result == AcquisitionKind.LeftOnly
        || result == AcquisitionKind.RightOnly;
    }
  }
}
