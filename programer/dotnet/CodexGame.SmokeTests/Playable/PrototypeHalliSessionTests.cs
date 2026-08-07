using System;
using CodexGame.Application.Distribution;
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
      tests.CheckThrows<InvalidOperationException>(
        () => session.BeginPrivateCardDistribution(zero),
        "Private-card distribution must be unavailable before the Halli stage finishes.");

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
      CheckWrongBellWithoutValidPile(tests, zero);
      CheckWrongPileAwardsOpponent(tests, zero);

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
      CheckDistributionBridge(tests, completionSession, new GameTimestamp(now));
    }

    private static void CheckDistributionBridge(
      TestHarness tests,
      PrototypeHalliSession halliSession,
      GameTimestamp now)
    {
      var selection = halliSession.BeginPrivateCardDistribution(now);
      var snapshot = selection.GetSnapshot(now);

      if (snapshot.Phase == PrivateCardSelectionPhase.AwaitingSelection)
      {
        for (var index = 0; index < snapshot.RequiredSelectionCount; index++)
        {
          tests.Check(
            selection.Toggle(snapshot.WinnerCandidates[index].Id),
            "The Halli winner's candidate cards must be selectable through the distribution bridge.");
        }

        tests.Check(selection.TryConfirm(), "A complete winner selection must confirm through the bridge.");
        snapshot = selection.GetSnapshot(now);
      }

      tests.Check(
        snapshot.Phase == PrivateCardSelectionPhase.Completed && snapshot.Result != null,
        "A completed Halli stage must produce a private-card distribution result.");

      if (snapshot.Result == null)
      {
        return;
      }

      var distributedCount = snapshot.Result.PlayerPrivateCards.Count
        + snapshot.Result.AiPrivateCards.Count
        + 1
        + snapshot.Result.RemainingCandidates.Count;
      tests.Check(
        distributedCount == CardId.CardCount - 1,
        "The Halli-to-distribution bridge must preserve all 51 cards except the first public card.");
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

    private static void CheckWrongBellWithoutValidPile(TestHarness tests, GameTimestamp zero)
    {
      for (var seed = 1L; seed <= 1000L; seed++)
      {
        var session = new PrototypeHalliSession();
        var flipTime = new GameTimestamp(1);
        session.StartNew(zero, seed);
        session.Advance(flipTime);
        var ready = session.GetSnapshot(flipTime);

        if (ready.Phase != PrototypeSessionPhase.ReadyToFlip
          || IsAcquirable(Evaluate(ready.LeftPile))
          || IsAcquirable(Evaluate(ready.RightPile)))
        {
          continue;
        }

        session.Ring(PileSide.Left, flipTime);
        var review = session.GetSnapshot(flipTime);

        tests.Check(review.Phase == PrototypeSessionPhase.Review, "A wrong bell without a valid pile must enter review.");
        tests.Check(review.PlayerWins == 0 && review.AiWins == 0, "A wrong bell without a valid pile must grant no Halli win.");
        tests.Check(review.LastAcquirer == PrototypeAcquirer.None, "A wrong bell without a valid pile must acquire no cards.");
        tests.Check(
          review.LeftPile.Count == ready.LeftPile.Count && review.RightPile.Count == ready.RightPile.Count,
          "A wrong bell without a valid pile must preserve both piles.");
        return;
      }

      tests.Check(false, "At least one deterministic seed must expose no valid pile on the first flip.");
    }

    private static void CheckWrongPileAwardsOpponent(TestHarness tests, GameTimestamp zero)
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

        var leftValid = IsAcquirable(Evaluate(bell.LeftPile));
        var rightValid = IsAcquirable(Evaluate(bell.RightPile));
        if (leftValid == rightValid)
        {
          continue;
        }

        session.Ring(leftValid ? PileSide.Right : PileSide.Left, flipTime);
        var review = session.GetSnapshot(flipTime);

        tests.Check(review.Phase == PrototypeSessionPhase.Review, "Choosing the wrong pile must enter review.");
        tests.Check(review.AiWins == 1 && review.PlayerWins == 0, "Choosing the wrong pile must award the valid pile and win to AI.");
        tests.Check(review.LastAcquirer == PrototypeAcquirer.Ai, "The opponent must acquire the valid pile after a wrong-pile bell.");
        tests.Check(review.LastAcquiredCards.Count > 0, "A wrong-pile bell must expose the opponent's acquired cards during review.");
        return;
      }

      tests.Check(false, "At least one deterministic seed must expose exactly one valid pile on the first flip.");
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
