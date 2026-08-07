using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeHalliSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards = Array.AsReadOnly(Array.Empty<Card>());

    private Deck _deck = null!;
    private CardLedger _ledger = null!;
    private HalliField _field = null!;
    private IRandomSource _aiReactionRandom = null!;
    private IRandomSource _aiChoiceRandom = null!;
    private BellWindowTracker _bellWindows = null!;
    private Card? _firstPublicCard;
    private GameTimestamp _readyDeadline;
    private GameTimestamp _reviewDeadline;
    private GameTimestamp? _aiBellAt;
    private PileSide? _aiPile;
    private string _statusMessage = "Press START to play.";
    private long _combatRoundSeed;
    private int _playerWins;
    private int _aiWins;
    private int _flipCount;
    private HalliStageEndReason _endReason;

    public PrototypeSessionPhase Phase { get; private set; } = PrototypeSessionPhase.Intro;

    public void StartNew(GameTimestamp now, long combatRoundSeed)
    {
      var cards = CardSetFactory.CreateStandard52(new PrototypeSkullPolicy());
      _combatRoundSeed = combatRoundSeed;
      _deck = Deck.CreateShuffled(
        cards,
        DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.CardOrder));
      _ledger = new CardLedger(cards);
      _field = new HalliField();
      _aiReactionRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiReaction);
      _aiChoiceRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiChoice);
      _bellWindows = new BellWindowTracker();
      _playerWins = 0;
      _aiWins = 0;
      _flipCount = 0;
      _endReason = HalliStageEndReason.None;
      _aiBellAt = null;
      _aiPile = null;

      var firstPublic = _deck.Draw();
      _ledger.Move(firstPublic.Id, CardZone.Deck, CardZone.FirstPublic);
      _firstPublicCard = firstPublic;

      BeginReady(now, "First public card opened. Flip two cards.");
    }

    public void Advance(GameTimestamp now)
    {
      switch (Phase)
      {
        case PrototypeSessionPhase.ReadyToFlip:
          Flip(now);
          break;
        case PrototypeSessionPhase.BellOpen:
          CloseBellWindow();
          Flip(now);
          break;
        case PrototypeSessionPhase.Review:
          CompleteReview(now);
          break;
      }
    }

    public void Ring(PileSide selectedPile, GameTimestamp now)
    {
      if (Phase != PrototypeSessionPhase.BellOpen)
      {
        _statusMessage = "No bell opportunity is open.";
        return;
      }

      if (_aiBellAt.HasValue
        && ReactionResolver.Resolve(now, _aiBellAt.Value) == ReactionWinner.Ai)
      {
        ResolveAiBell(now);
        return;
      }

      ResolvePlayerBell(selectedPile, now);
    }

    public void Tick(GameTimestamp now)
    {
      switch (Phase)
      {
        case PrototypeSessionPhase.ReadyToFlip:
          if (now.Microseconds >= _readyDeadline.Microseconds)
          {
            EnterReview(now, "30 second timeout. Both sides lose this Halli round.");
          }
          break;
        case PrototypeSessionPhase.BellOpen:
          if (_aiBellAt.HasValue && now.Microseconds >= _aiBellAt.Value.Microseconds)
          {
            ResolveAiBell(now);
          }
          break;
        case PrototypeSessionPhase.Review:
          if (now.Microseconds >= _reviewDeadline.Microseconds)
          {
            CompleteReview(now);
          }
          break;
      }
    }

    public PrototypeHalliSnapshot GetSnapshot(GameTimestamp now)
    {
      var left = _field == null ? EmptyCards : _field.GetExposedCards(PileSide.Left);
      var right = _field == null ? EmptyCards : _field.GetExposedCards(PileSide.Right);
      var remaining = 0L;

      if (Phase == PrototypeSessionPhase.ReadyToFlip)
      {
        remaining = Math.Max(0, _readyDeadline.Microseconds - now.Microseconds);
      }
      else if (Phase == PrototypeSessionPhase.Review)
      {
        remaining = Math.Max(0, _reviewDeadline.Microseconds - now.Microseconds);
      }

      return new PrototypeHalliSnapshot(
        Phase,
        _statusMessage,
        _combatRoundSeed,
        _playerWins,
        _aiWins,
        HalliStageRules.GetWinTarget(1),
        _flipCount,
        _deck == null ? 0 : _deck.RemainingCount,
        remaining,
        _firstPublicCard,
        left,
        right,
        _endReason);
    }

    private void Flip(GameTimestamp now)
    {
      var endReason = ResolveEndReason();

      if (endReason != HalliStageEndReason.None)
      {
        Finish(endReason);
        return;
      }

      CloseBellWindow();
      ExposeFromDeck(PileSide.Left);
      ExposeFromDeck(PileSide.Right);
      _flipCount++;

      var leftValid = IsAcquirable(Evaluate(PileSide.Left));
      var rightValid = IsAcquirable(Evaluate(PileSide.Right));

      if (!leftValid && !rightValid)
      {
        endReason = ResolveEndReason();

        if (endReason != HalliStageEndReason.None)
        {
          Finish(endReason);
        }
        else
        {
          BeginReady(now, "No valid bell. Flip again.");
        }

        return;
      }

      Phase = PrototypeSessionPhase.BellOpen;
      _bellWindows.OpenForCurrentField();
      _statusMessage = "Bell opportunity open. Judge LEFT or RIGHT yourself.";
      ScheduleAi(now, leftValid, rightValid);
    }

    private void ExposeFromDeck(PileSide side)
    {
      var destination = side == PileSide.Left ? CardZone.LeftPile : CardZone.RightPile;
      var card = _deck.Draw();
      _ledger.Move(card.Id, CardZone.Deck, destination);
      var displaced = _field.Expose(side, card);

      if (displaced.HasValue)
      {
        _ledger.Move(displaced.Value.Id, destination, CardZone.UnacquiredPool);
      }
    }

    private void ScheduleAi(GameTimestamp now, bool leftValid, bool rightValid)
    {
      if (_aiReactionRandom.NextInt(100) < 30)
      {
        _aiBellAt = null;
        _aiPile = null;
        return;
      }

      if (leftValid && rightValid)
      {
        _aiPile = _aiChoiceRandom.NextInt(2) == 0 ? PileSide.Left : PileSide.Right;
      }
      else
      {
        _aiPile = leftValid ? PileSide.Left : PileSide.Right;
      }

      var delay = _aiReactionRandom.NextInt((int)GameRules.AiMaximumReactionMicroseconds + 1);
      _aiBellAt = Add(now, delay);
    }

    private void ResolvePlayerBell(PileSide selectedPile, GameTimestamp now)
    {
      var selected = Evaluate(selectedPile);
      var oppositePile = selectedPile == PileSide.Left ? PileSide.Right : PileSide.Left;
      var opposite = Evaluate(oppositePile);

      if (IsAcquirable(selected))
      {
        Acquire(selectedPile, selected, CardZone.PlayerAcquired);
        _playerWins++;
        EnterReview(now, "Correct bell. Player gains one Halli win.");
        return;
      }

      if (IsAcquirable(opposite))
      {
        Acquire(oppositePile, opposite, CardZone.AiAcquired);
        _aiWins++;
        EnterReview(now, "Wrong pile. AI acquires the valid pile.");
        return;
      }

      EnterReview(now, "Wrong bell. No cards acquired and both piles stay.");
    }

    private void ResolveAiBell(GameTimestamp now)
    {
      if (!_aiPile.HasValue)
      {
        return;
      }

      var pile = _aiPile.Value;
      var resolution = Evaluate(pile);

      if (IsAcquirable(resolution))
      {
        Acquire(pile, resolution, CardZone.AiAcquired);
        _aiWins++;
        EnterReview(now, "AI rang first and gains one Halli win.");
      }
      else
      {
        EnterReview(now, "AI bell became invalid. No cards acquired.");
      }
    }

    private AcquisitionKind Evaluate(PileSide side)
    {
      var cards = _field.GetExposedCards(side);
      var first = cards.Count > 0 ? cards[0] : (Card?)null;
      var second = cards.Count > 1 ? cards[1] : (Card?)null;
      return SkullAcquisitionResolver.Resolve(first, second);
    }

    private void Acquire(PileSide side, AcquisitionKind resolution, CardZone destination)
    {
      var source = side == PileSide.Left ? CardZone.LeftPile : CardZone.RightPile;
      var cards = _field.Clear(side);

      for (var index = 0; index < cards.Count; index++)
      {
        var acquired = resolution == AcquisitionKind.Both
          || (resolution == AcquisitionKind.LeftOnly && index == 0)
          || (resolution == AcquisitionKind.RightOnly && index == 1);

        _ledger.Move(
          cards[index].Id,
          source,
          acquired ? destination : CardZone.UnacquiredPool);
      }
    }

    private void EnterReview(GameTimestamp now, string message)
    {
      CloseBellWindow();
      Phase = PrototypeSessionPhase.Review;
      _reviewDeadline = Add(now, GameRules.ReviewGraceMicroseconds);
      _statusMessage = message;
    }

    private void CompleteReview(GameTimestamp now)
    {
      var endReason = ResolveEndReason();

      if (endReason != HalliStageEndReason.None)
      {
        Finish(endReason);
      }
      else
      {
        BeginReady(now, "Review complete. Flip again.");
      }
    }

    private void BeginReady(GameTimestamp now, string message)
    {
      Phase = PrototypeSessionPhase.ReadyToFlip;
      _readyDeadline = Add(now, GameRules.CardFlipTimeoutMicroseconds);
      _statusMessage = message;
    }

    private void CloseBellWindow()
    {
      if (_bellWindows != null)
      {
        _bellWindows.CloseForNextFlip();
      }

      _aiBellAt = null;
      _aiPile = null;
    }

    private HalliStageEndReason ResolveEndReason()
    {
      return HalliStageRules.ResolveEndReason(
        _playerWins,
        _aiWins,
        _flipCount,
        _deck.RemainingCount,
        1);
    }

    private void Finish(HalliStageEndReason endReason)
    {
      CloseBellWindow();
      Phase = PrototypeSessionPhase.Finished;
      _endReason = endReason;

      switch (endReason)
      {
        case HalliStageEndReason.PlayerTargetReached:
          _statusMessage = "PLAYER WINS the Halli stage. Press RESTART.";
          break;
        case HalliStageEndReason.AiTargetReached:
          _statusMessage = "AI WINS the Halli stage. Press RESTART.";
          break;
        default:
          _statusMessage = "Stage ended without a target winner. Press RESTART.";
          break;
      }
    }

    private static bool IsAcquirable(AcquisitionKind resolution)
    {
      return resolution == AcquisitionKind.Both
        || resolution == AcquisitionKind.LeftOnly
        || resolution == AcquisitionKind.RightOnly;
    }

    private static GameTimestamp Add(GameTimestamp timestamp, long microseconds)
    {
      if (microseconds < 0 || timestamp.Microseconds > long.MaxValue - microseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(microseconds));
      }

      return new GameTimestamp(timestamp.Microseconds + microseconds);
    }
  }
}
