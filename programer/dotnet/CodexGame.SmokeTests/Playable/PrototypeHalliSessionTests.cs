using System;
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
      CheckOneCardInputAndFaceUpBoundary(tests);
      CheckBellDuringRevealStopsDistribution(tests);
      CheckResultLockAndBellTimeout(tests);
      CheckWrongBellHasNoRewardSelection(tests);
      CheckWrongBellWaitsForManualFlipAfterLock(tests);
      CheckWrongBellPreservesEarlierAcquiredCards(tests);
      CheckGlobalInactivity(tests);
    }

    private static void CheckOneCardInputAndFaceUpBoundary(TestHarness tests)
    {
      var session = new PrototypeHalliSession();
      session.StartNew(new GameTimestamp(0), 20260808);
      var ready = session.GetSnapshot(new GameTimestamp(0));
      tests.Check(
        ready.Phase == PrototypeSessionPhase.ReadyToFlip
          && ready.CanFlip
          && ready.CanRing
          && ready.RemainingMicroseconds == GameRules.BellInputTimeoutMicroseconds,
        "The opening must start a 30-second bell window without a flip timeout.");

      var startedAt = new GameTimestamp(1);
      session.Advance(startedAt);
      var moving = session.GetSnapshot(startedAt);
      tests.Check(
        moving.Phase == PrototypeSessionPhase.SequentialReveal
          && moving.FlipCount == 1
          && moving.RevealStepNumber == 1
          && moving.LeftPile.Count == 0
          && moving.CanRing,
        "The first W input must start one player card, count one distribution, and keep the moving card ineligible.");

      var firstFaceUpAt = new GameTimestamp(startedAt.Microseconds + 221_000);
      session.Tick(firstFaceUpAt);
      var firstFaceUp = session.GetSnapshot(firstFaceUpAt);
      tests.Check(
        firstFaceUp.LeftPile.Count == 1 && firstFaceUp.RightPile.Count == 0,
        "A card must enter Halli judgment only after its face-up motion completes.");

      var pairFinishedAt = new GameTimestamp(startedAt.Microseconds + 700_000);
      session.Tick(pairFinishedAt);
      var waiting = session.GetSnapshot(pairFinishedAt);
      tests.Check(
        waiting.Phase == PrototypeSessionPhase.ReadyToFlip
          && waiting.CanFlip
          && waiting.LeftPile.Count == 1
          && waiting.RightPile.Count == 1
          && waiting.RemainingDeckCards == 49,
        "After player-left and AI-left, #18 flow must wait for the next player input.");

      session.Advance(new GameTimestamp(pairFinishedAt.Microseconds + 1));
      var secondInput = session.GetSnapshot(new GameTimestamp(pairFinishedAt.Microseconds + 1));
      tests.Check(
        secondInput.RevealStepNumber == 3
          && secondInput.RevealingActor == HalliActor.Player
          && secondInput.RevealingRelativeSide == HalliRelativeSide.Right,
        "The second W input must start only the player's relative-right card.");
    }

    private static void CheckBellDuringRevealStopsDistribution(TestHarness tests)
    {
      for (var seed = 1L; seed <= 500; seed++)
      {
        var session = new PrototypeHalliSession();
        session.StartNew(new GameTimestamp(0), seed);
        session.Advance(new GameTimestamp(1));
        var faceUpAt = new GameTimestamp(221_001);
        session.Tick(faceUpAt);
        var snapshot = session.GetSnapshot(faceUpAt);
        if (!TryGetValidPile(snapshot, out var validPile)) continue;

        session.Ring(validPile, faceUpAt);
        var resolved = session.GetSnapshot(faceUpAt);
        tests.Check(
          resolved.Phase == PrototypeSessionPhase.Review
            && resolved.PlayerWins == 1
            && resolved.RevealStepNumber == 0
            && !resolved.CanRing,
          "A correct bell after one face-up card must stop the remaining reveal immediately.");
        return;
      }

      tests.Check(false, "A deterministic seed should expose a one-card skull-3 bell opportunity.");
    }

    private static void CheckResultLockAndBellTimeout(TestHarness tests)
    {
      var wrong = new PrototypeHalliSession();
      wrong.StartNew(new GameTimestamp(0), 17);
      wrong.Ring(PileSide.Left, new GameTimestamp(1));
      var review = wrong.GetSnapshot(new GameTimestamp(1));
      tests.Check(
        review.Phase == PrototypeSessionPhase.Review
          && review.AiWins == 1
          && review.RemainingMicroseconds == GameRules.HalliResultLockMicroseconds,
        "A result must apply exactly one two-second input lock.");
      var unlockedAt = new GameTimestamp(1 + GameRules.HalliResultLockMicroseconds);
      wrong.Tick(unlockedAt);
      var unlocked = wrong.GetSnapshot(unlockedAt);
      tests.Check(
        unlocked.Phase == PrototypeSessionPhase.ReadyToFlip
          && unlocked.CanRing
          && unlocked.RemainingMicroseconds == GameRules.BellInputTimeoutMicroseconds,
        "The 30-second bell timer must begin after the result lock ends.");

      var timeout = new PrototypeHalliSession();
      timeout.StartNew(new GameTimestamp(0), 18);
      timeout.Tick(new GameTimestamp(GameRules.BellInputTimeoutMicroseconds));
      var timedOut = timeout.GetSnapshot(new GameTimestamp(GameRules.BellInputTimeoutMicroseconds));
      tests.Check(
        timedOut.Phase == PrototypeSessionPhase.Review
          && timedOut.AiWins == 1
          && timedOut.Status.Key == "STATUS_HALLI_FLIP_TIMEOUT",
        "A 30-second bell-input timeout must record one AI Halli win.");
    }

    private static void CheckWrongBellHasNoRewardSelection(TestHarness tests)
    {
      var session = new PrototypeHalliSession();
      session.StartNew(new GameTimestamp(0), 19);
      session.Ring(PileSide.Right, new GameTimestamp(1));
      var snapshot = session.GetSnapshot(new GameTimestamp(1));
      tests.Check(
        snapshot.Phase == PrototypeSessionPhase.Review
          && snapshot.AiWins == 1,
        "Wrong input must be a loss only; the obsolete reward-card selection must not open.");
    }

    private static void CheckWrongBellWaitsForManualFlipAfterLock(TestHarness tests)
    {
      var session = new PrototypeHalliSession();
      session.StartNew(new GameTimestamp(0), 20);
      session.Ring(PileSide.Left, new GameTimestamp(1));

      var unlockedAt = new GameTimestamp(1 + GameRules.HalliResultLockMicroseconds);
      session.Tick(unlockedAt);
      var unlocked = session.GetSnapshot(unlockedAt);
      var initialDeckCount = unlocked.RemainingDeckCards;
      tests.Check(
        unlocked.Phase == PrototypeSessionPhase.ReadyToFlip
          && unlocked.AiWins == 1
          && unlocked.CanFlip
          && unlocked.FlipCount == 0,
        "After a wrong bell, the two-second lock must end in a manual W/click wait state.");

      var idleAt = new GameTimestamp(unlockedAt.Microseconds + 1_000_000);
      session.Tick(idleAt);
      var idle = session.GetSnapshot(idleAt);
      tests.Check(
        idle.Phase == PrototypeSessionPhase.ReadyToFlip
          && idle.FlipCount == 0
          && idle.RemainingDeckCards == initialDeckCount,
        "No cards may be revealed after a wrong bell until the player supplies a new flip input.");

      session.Advance(new GameTimestamp(idleAt.Microseconds + 1));
      var resumed = session.GetSnapshot(new GameTimestamp(idleAt.Microseconds + 1));
      tests.Check(
        resumed.Phase == PrototypeSessionPhase.SequentialReveal
          && resumed.FlipCount == 1,
        "One new W/click input must resume exactly one controlled reveal sequence.");
    }

    private static void CheckWrongBellPreservesEarlierAcquiredCards(TestHarness tests)
    {
      for (var seed = 1L; seed <= 500; seed++)
      {
        var session = new PrototypeHalliSession();
        session.StartNew(new GameTimestamp(0), seed);
        session.Advance(new GameTimestamp(1));
        var faceUpAt = new GameTimestamp(221_001);
        session.Tick(faceUpAt);
        var faceUp = session.GetSnapshot(faceUpAt);
        if (!TryGetValidPile(faceUp, out var validPile)) continue;

        session.Ring(validPile, faceUpAt);
        var acquired = session.GetSnapshot(faceUpAt).PlayerAcquiredCount;
        if (acquired == 0) continue;

        var unlockedAt = new GameTimestamp(
          faceUpAt.Microseconds + GameRules.HalliResultLockMicroseconds);
        session.Tick(unlockedAt);
        var ready = session.GetSnapshot(unlockedAt);
        var wrongPile = IsAcquirable(Evaluate(ready.LeftPile))
          ? PileSide.Right
          : PileSide.Left;
        if (IsAcquirable(Evaluate(
          wrongPile == PileSide.Left ? ready.LeftPile : ready.RightPile)))
        {
          continue;
        }

        session.Ring(wrongPile, new GameTimestamp(unlockedAt.Microseconds + 1));
        var afterWrong = session.GetSnapshot(new GameTimestamp(unlockedAt.Microseconds + 1));
        tests.Check(
          afterWrong.AiWins == 1
            && afterWrong.PlayerAcquiredCount == acquired,
          "A wrong bell must record only one opponent win and preserve earlier acquired cards.");
        return;
      }

      tests.Check(false, "A deterministic seed should verify acquired-card retention after a later wrong bell.");
    }

    private static void CheckGlobalInactivity(TestHarness tests)
    {
      var game = new PlayableGameSession();
      game.StartNewBattle(new GameTimestamp(0), 123);
      var readyAt = new GameTimestamp(GameRules.HalliOpeningPresentationMicroseconds);
      game.Tick(readyAt);
      var timeoutAt = new GameTimestamp(
        GameRules.HalliOpeningPresentationMicroseconds
          + GameRules.GlobalInactivityTimeoutMicroseconds);
      game.Tick(timeoutAt);
      tests.Check(
        game.GetSnapshot(timeoutAt).Phase == PlayableGamePhase.Intro,
        "Three minutes without valid user input must return to the main screen.");
    }

    private static bool TryGetValidPile(PrototypeHalliSnapshot snapshot, out PileSide pile)
    {
      if (IsAcquirable(Evaluate(snapshot.LeftPile)))
      {
        pile = PileSide.Left;
        return true;
      }
      if (IsAcquirable(Evaluate(snapshot.RightPile)))
      {
        pile = PileSide.Right;
        return true;
      }
      pile = PileSide.Left;
      return false;
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
