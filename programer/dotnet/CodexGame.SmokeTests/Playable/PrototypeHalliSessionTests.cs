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
      CheckRoundTargetsAndSequentialReveal(tests);
      CheckTurnOrder(tests);
      CheckAiPolicy(tests);
      CheckFlipTimeout(tests);
      CheckWrongBellRewardSelection(tests);
      CheckCorrectAcquisitionLock(tests);
      CheckCompletionAndDistribution(tests);
      CheckGlobalInactivity(tests);
    }

    private static void CheckRoundTargetsAndSequentialReveal(TestHarness tests)
    {
      var zero = new GameTimestamp(0);
      var session = new PrototypeHalliSession();
      session.StartNew(zero, 20260807);
      var start = session.GetSnapshot(zero);
      tests.Check(
        start.Phase == PrototypeSessionPhase.ReadyToFlip
          && start.LeadActor == HalliActor.Player
          && start.CanFlip,
        "A new Halli stage must wait for the player's lead flip.");
      tests.Check(start.WinTarget == 3 && start.RemainingDeckCards == 51,
        "Round one must target three wins after opening one public card.");
      tests.CheckThrows<InvalidOperationException>(
        () => session.BeginPrivateCardDistribution(zero),
        "Private distribution must remain unavailable before Halli finishes.");

      var laterRound = new PrototypeHalliSession();
      laterRound.StartNew(zero, 11, 3);
      tests.Check(laterRound.GetSnapshot(zero).WinTarget == 2,
        "Round three and later must retain the 0.08 two-win target.");

      var flipAt = new GameTimestamp(100);
      session.Advance(flipAt);
      var reveal = session.GetSnapshot(flipAt);
      tests.Check(
        reveal.Phase == PrototypeSessionPhase.SequentialReveal
          && reveal.FlipCount == 0
          && reveal.RevealStepNumber == 1
          && reveal.RevealingActor == HalliActor.Player
          && reveal.RevealingRelativeSide == HalliRelativeSide.Left
          && reveal.RevealingPile == PileSide.Left
          && reveal.LeftPile.Count + reveal.RightPile.Count == 1,
        "A distribution must begin with the player's relative-left card.");
      tests.Check(
        !reveal.CanRing && !reveal.CanFlip,
        "Q/W/E gameplay input must remain locked during the four-card reveal.");
      session.Ring(PileSide.Left, flipAt);
      tests.Check(session.GetSnapshot(flipAt).PlayerWins == 0,
        "A bell input during sequential reveal must be ignored instead of queued.");

      var expectedActors = new[] { HalliActor.Player, HalliActor.Ai, HalliActor.Player, HalliActor.Ai };
      var expectedSides = new[]
      {
        HalliRelativeSide.Left,
        HalliRelativeSide.Left,
        HalliRelativeSide.Right,
        HalliRelativeSide.Right
      };
      var expectedPiles = new[] { PileSide.Left, PileSide.Right, PileSide.Left, PileSide.Right };
      var revealTime = flipAt.Microseconds;
      for (var step = 1; step < 4; step++)
      {
        reveal = session.GetSnapshot(new GameTimestamp(revealTime));
        revealTime += reveal.RemainingMicroseconds;
        session.Tick(new GameTimestamp(revealTime));
        reveal = session.GetSnapshot(new GameTimestamp(revealTime));
        tests.Check(
          reveal.RevealStepNumber == step + 1
            && reveal.RevealingActor == expectedActors[step]
            && reveal.RevealingRelativeSide == expectedSides[step]
            && reveal.RevealingPile == expectedPiles[step],
          "The four-card reveal order must remain player-left, AI-left, player-right, AI-right.");
      }

      revealTime += reveal.RemainingMicroseconds;
      session.Tick(new GameTimestamp(revealTime));
      var batch = session.GetSnapshot(new GameTimestamp(revealTime));
      tests.Check(
        batch.FlipCount == 1
          && batch.LeftPile.Count == 2
          && batch.RightPile.Count == 2
          && batch.RemainingDeckCards == 47,
        "One completed distribution must consume four cards and leave two cards per physical pile.");
    }

    private static void CheckTurnOrder(TestHarness tests)
    {
      var order = new HalliTurnOrder();
      tests.Check(
        order.LeadActor == HalliActor.Player,
        "The player must start the first four-card distribution.");
      order.SetLead(HalliActor.Ai);
      tests.Check(
        order.LeadActor == HalliActor.Ai && order.GetFollower() == HalliActor.Player,
        "The Halli winner changes only who starts the next fixed reveal sequence.");
      tests.Check(
        HalliRevealSequence.GetStep(0).PhysicalPile == PileSide.Left
          && HalliRevealSequence.GetStep(1).PhysicalPile == PileSide.Right
          && HalliRevealSequence.GetStep(2).PhysicalPile == PileSide.Left
          && HalliRevealSequence.GetStep(3).PhysicalPile == PileSide.Right,
        "Player cards must remain in the left field and AI cards in the right field.");
    }

    private static void CheckAiPolicy(TestHarness tests)
    {
      var policy = new HalliAiBellPolicy();
      var minimum = policy.CreateReactionDelay(new FixedRandom(0));
      var maximum = policy.CreateReactionDelay(new FixedRandom(int.MaxValue));
      tests.Check(
        minimum >= GameRules.AiMinimumReactionMicroseconds
          && maximum <= GameRules.AiMaximumReactionMicroseconds,
        "AI reaction delay must stay within the 0.08 one-to-three-second range.");

      var correct = policy.Decide(
        true,
        true,
        GameRules.AiTypicalReactionMicroseconds,
        side => side == PileSide.Left ? 10 : 1,
        new FixedRandom(0),
        new FixedRandom(0));
      tests.Check(
        correct.Outcome == AiBellOutcome.Correct && correct.Pile == PileSide.Left,
        "A correct AI roll must select the stronger valid pile under the 60% policy branch.");

      var wrong = policy.Decide(
        true,
        false,
        GameRules.AiTypicalReactionMicroseconds,
        _ => 0,
        new FixedRandom(65),
        new FixedRandom(0));
      tests.Check(
        wrong.Outcome == AiBellOutcome.Wrong && wrong.Pile == PileSide.Right,
        "An AI wrong roll must select the invalid pile when one exists.");

      var convertedMiss = policy.Decide(
        true,
        true,
        GameRules.AiTypicalReactionMicroseconds,
        _ => 0,
        new FixedRandom(65),
        new FixedRandom(0));
      tests.Check(convertedMiss.Outcome == AiBellOutcome.Miss && !convertedMiss.Pile.HasValue,
        "An AI wrong roll must convert to miss when both piles are valid.");
    }

    private static void CheckFlipTimeout(TestHarness tests)
    {
      var session = new PrototypeHalliSession();
      session.StartNew(new GameTimestamp(0), 7, 3);
      var timeoutAt = new GameTimestamp(GameRules.CardFlipTimeoutMicroseconds);
      session.Tick(timeoutAt);
      var timeout = session.GetSnapshot(timeoutAt);
      tests.Check(
        timeout.AiWins == 1
          && timeout.LeadActor == HalliActor.Ai
          && timeout.Phase == PrototypeSessionPhase.Review,
        "A 30-second player lead timeout must give AI one win, preserve the field, and make AI lead.");
      session.Tick(new GameTimestamp(
        timeoutAt.Microseconds + GameRules.WrongBellRewardResultLockMicroseconds));
      var ready = session.GetSnapshot(new GameTimestamp(
        timeoutAt.Microseconds + GameRules.WrongBellRewardResultLockMicroseconds));
      tests.Check(
        ready.Phase == PrototypeSessionPhase.ReadyToFlip && ready.LeadActor == HalliActor.Ai,
        "An empty reward pool must still enforce the two-second result lock before AI auto lead.");
    }

    private static void CheckWrongBellRewardSelection(TestHarness tests)
    {
      var cards = CardSetFactory.CreateStandard52(new PrototypeSkullPolicy());
      var candidates = Array.AsReadOnly(new[] { cards[0], cards[1], cards[2] });
      var session = new WrongBellRewardSelectionSession();
      session.Begin(
        candidates,
        DeterministicRandomFactory.Create(21, RandomChannel.WrongBellReward),
        new GameTimestamp(0));
      tests.Check(
        !session.CanSelect(new GameTimestamp(0))
          && session.GetRemainingMicroseconds(new GameTimestamp(0))
            == GameRules.WrongBellRewardInitialLockMicroseconds,
        "Wrong-bell reward selection must begin with a two-second read-only lock.");
      tests.Check(
        !session.TrySelect(candidates[1].Id, new GameTimestamp(1_999_999)),
        "Reward cards must not confirm during the initial lock.");
      var unlockedAt = new GameTimestamp(GameRules.WrongBellRewardInitialLockMicroseconds);
      tests.Check(
        session.TrySelect(candidates[1].Id, unlockedAt)
          && session.SelectedCard.HasValue
          && session.SelectedCard.Value.Id == candidates[1].Id,
        "A player must be able to confirm one face-up reward after the initial lock.");

      var timeout = new WrongBellRewardSelectionSession();
      timeout.Begin(
        candidates,
        DeterministicRandomFactory.Create(22, RandomChannel.WrongBellReward),
        new GameTimestamp(0));
      var deadline = new GameTimestamp(
        GameRules.WrongBellRewardInitialLockMicroseconds
          + GameRules.WrongBellRewardSelectionTimeoutMicroseconds);
      tests.Check(
        timeout.Tick(deadline) && timeout.SelectedCard.HasValue && timeout.TimedOut,
        "Thirty seconds after unlock must award one deterministic random reward card.");

      var single = new WrongBellRewardSelectionSession();
      single.Begin(
        Array.AsReadOnly(new[] { cards[0] }),
        new FixedRandom(0),
        new GameTimestamp(0));
      tests.Check(single.Tick(unlockedAt) && !single.TimedOut,
        "A single reward candidate must auto-confirm when the initial lock ends.");
    }

    private static void CheckCorrectAcquisitionLock(TestHarness tests)
    {
      for (var seed = 1L; seed <= 2000L; seed++)
      {
        var session = new PrototypeHalliSession();
        session.StartNew(new GameTimestamp(0), seed);
        session.Advance(new GameTimestamp(1));
        var visibleAt = new GameTimestamp(1_300_001);
        session.Tick(visibleAt);
        var field = session.GetSnapshot(visibleAt);
        if (field.Phase != PrototypeSessionPhase.BellOpen) continue;

        var pile = IsAcquirable(Evaluate(field.LeftPile)) ? PileSide.Left : PileSide.Right;
        session.Ring(pile, visibleAt);
        var review = session.GetSnapshot(visibleAt);
        tests.Check(
          review.Phase == PrototypeSessionPhase.Review
            && review.PlayerWins == 1
            && review.LeadActor == HalliActor.Player
            && review.LastAcquiredCards.Count > 0,
          "A correct player bell must acquire only the selected valid pile and set player lead.");
        session.Tick(new GameTimestamp(
          visibleAt.Microseconds + GameRules.NextFlipLockMicroseconds));
        var ready = session.GetSnapshot(new GameTimestamp(
          visibleAt.Microseconds + GameRules.NextFlipLockMicroseconds));
        tests.Check(
          ready.Phase == PrototypeSessionPhase.ReadyToFlip
            && ready.LastAcquirer == PrototypeAcquirer.None,
          "Correct acquisition must hide its review after the one-second next-flip lock.");
        return;
      }

      tests.Check(false, "At least one deterministic seed must open a first-pair bell opportunity.");
    }

    private static void CheckCompletionAndDistribution(TestHarness tests)
    {
      var session = new PrototypeHalliSession();
      var now = 0L;
      session.StartNew(new GameTimestamp(now), 99);
      for (var step = 0; step < 500; step++)
      {
        var snapshot = session.GetSnapshot(new GameTimestamp(now));
        if (snapshot.Phase == PrototypeSessionPhase.Finished) break;

        if (snapshot.Phase == PrototypeSessionPhase.ReadyToFlip)
        {
          if (snapshot.LeadActor == HalliActor.Player) session.Advance(new GameTimestamp(now));
          else session.Tick(new GameTimestamp(now));
        }
        else if (snapshot.Phase == PrototypeSessionPhase.SequentialReveal)
        {
          now += 1_300_001;
          session.Tick(new GameTimestamp(now));
        }
        else if (snapshot.Phase == PrototypeSessionPhase.BellOpen)
        {
          var pile = IsAcquirable(Evaluate(snapshot.LeftPile)) ? PileSide.Left : PileSide.Right;
          session.Ring(pile, new GameTimestamp(now));
        }
        else if (snapshot.Phase == PrototypeSessionPhase.WrongBellRewardSelection)
        {
          if (snapshot.WrongBellRewardSelectionEnabled)
          {
            session.SelectWrongBellReward(snapshot.WrongBellRewardCandidates[0].Id, new GameTimestamp(now));
          }
          else
          {
            now += GameRules.WrongBellRewardInitialLockMicroseconds;
            session.Tick(new GameTimestamp(now));
          }
        }
        else if (snapshot.Phase == PrototypeSessionPhase.Review)
        {
          now += GameRules.WrongBellRewardResultLockMicroseconds;
          session.Tick(new GameTimestamp(now));
        }
        now++;
      }

      var completed = session.GetSnapshot(new GameTimestamp(now));
      tests.Check(completed.Phase == PrototypeSessionPhase.Finished,
        "The 0.08 Halli state machine must reach a terminal state.");
      tests.Check(completed.FlipCount <= GameRules.HalliFlipLimit,
        "The Halli state machine must never exceed 12 completed four-card distributions.");
      if (completed.Phase != PrototypeSessionPhase.Finished) return;

      var selection = session.BeginPrivateCardDistribution(new GameTimestamp(now));
      var distribution = selection.GetSnapshot(new GameTimestamp(now));
      if (distribution.Phase == PrivateCardSelectionPhase.AwaitingSelection)
      {
        for (var index = 0; index < distribution.RequiredSelectionCount; index++)
        {
          selection.Toggle(distribution.WinnerCandidates[index].Id);
        }
        selection.TryConfirm();
        distribution = selection.GetSnapshot(new GameTimestamp(now));
      }
      tests.Check(distribution.Phase == PrivateCardSelectionPhase.Completed && distribution.Result != null,
        "A completed Halli stage must bridge to deterministic private-card distribution.");
    }

    private static void CheckGlobalInactivity(TestHarness tests)
    {
      var game = new PlayableGameSession();
      game.StartNewBattle(new GameTimestamp(0), 123);
      tests.Check(game.GetSnapshot(new GameTimestamp(0)).Phase == PlayableGamePhase.HalliOpening,
        "Starting a battle must enter a timer-free first-public-card presentation.");
      var readyAt = new GameTimestamp(GameRules.HalliOpeningPresentationMicroseconds);
      game.Tick(readyAt);
      tests.Check(game.GetSnapshot(readyAt).Phase == PlayableGamePhase.Halli,
        "The Halli input phase must start only after the opening presentation completes.");
      var timeoutAt = new GameTimestamp(
        GameRules.HalliOpeningPresentationMicroseconds
          + GameRules.GlobalInactivityTimeoutMicroseconds);
      game.Tick(timeoutAt);
      tests.Check(game.GetSnapshot(timeoutAt).Phase
        == PlayableGamePhase.Intro,
        "Three minutes without valid game input must abort the battle to the main screen.");
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

    private sealed class FixedRandom : IRandomSource
    {
      private readonly int _value;
      public FixedRandom(int value) { _value = value; }
      public int NextInt(int exclusiveMax)
      {
        if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
        return (int)((uint)_value % (uint)exclusiveMax);
      }
    }
  }
}
