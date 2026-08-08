using System;
using System.Collections.Generic;
using CodexGame.Application.Distribution;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeHalliSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards = Array.AsReadOnly(Array.Empty<Card>());

    private readonly HalliAiBellPolicy _aiBellPolicy = new HalliAiBellPolicy();

    private Deck _deck = null!;
    private CardLedger _ledger = null!;
    private HalliField _field = null!;
    private HalliTurnOrder _turnOrder = new HalliTurnOrder();
    private IRandomSource _revealRandom = null!;
    private IRandomSource _aiReactionRandom = null!;
    private IRandomSource _aiChoiceRandom = null!;
    private Card? _firstPublicCard;
    private GameTimestamp _bellDeadline;
    private GameTimestamp _nextRevealEventAt;
    private GameTimestamp _currentRevealStartedAt;
    private GameTimestamp _currentRevealCompleteAt;
    private GameTimestamp _reviewDeadline;
    private GameTimestamp? _aiBellAt;
    private PileSide? _aiPile;
    private HalliRevealStep? _currentRevealStep;
    private Card? _currentRevealCard;
    private AiBellOutcome _aiOutcome;
    private bool _bellTimerActive;
    private bool _currentRevealCommitted;
    private LocalizedStatus _status = LocalizedStatus.Of("STATUS_HALLI_PRESS_START");
    private long _combatRoundSeed;
    private int _combatRoundNumber = 1;
    private int _playerWins;
    private int _aiWins;
    private int _flipCount;
    private int _nextRevealStepIndex;
    private HalliStageEndReason _endReason;
    private PrototypeAcquirer _lastAcquirer;
    private IReadOnlyList<Card> _lastAcquiredCards = EmptyCards;
    private PileSide? _lastAcquiredPile;
    private PileSide? _lastBellPile;
    private PrototypeBellFeedback _bellFeedback;

    public PrototypeSessionPhase Phase { get; private set; } = PrototypeSessionPhase.Intro;

    public void StartNew(
      GameTimestamp now,
      long combatRoundSeed,
      int combatRoundNumber = 1,
      bool waitForOpeningPresentation = false)
    {
      HalliStageRules.GetWinTarget(combatRoundNumber);
      var cards = CardSetFactory.CreateStandard52(new PrototypeSkullPolicy());
      _combatRoundSeed = combatRoundSeed;
      _combatRoundNumber = combatRoundNumber;
      _deck = Deck.CreateShuffled(
        cards,
        DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.CardOrder));
      _ledger = new CardLedger(cards);
      _field = new HalliField();
      _turnOrder = new HalliTurnOrder();
      _revealRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.RevealTiming);
      _aiReactionRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiReaction);
      _aiChoiceRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiChoice);
      _playerWins = 0;
      _aiWins = 0;
      _flipCount = 0;
      _nextRevealStepIndex = 0;
      _bellTimerActive = false;
      _endReason = HalliStageEndReason.None;
      CloseBellWindow();
      ClearLastAcquisition();

      var firstPublic = _deck.Draw();
      _ledger.Move(firstPublic.Id, CardZone.Deck, CardZone.FirstPublic);
      _firstPublicCard = firstPublic;
      if (waitForOpeningPresentation)
      {
        Phase = PrototypeSessionPhase.Opening;
        _status = LocalizedStatus.Of("STATUS_HALLI_DEALER_OPENING");
      }
      else
      {
        BeginReady(now, LocalizedStatus.Of("STATUS_HALLI_FIRST_PUBLIC_READY"), true);
      }
    }

    public bool CompleteOpening(GameTimestamp now)
    {
      if (Phase != PrototypeSessionPhase.Opening) return false;
      BeginReady(now, LocalizedStatus.Of("STATUS_HALLI_FIRST_PUBLIC_READY"), true);
      return true;
    }

    public void Advance(GameTimestamp now)
    {
      if (Phase == PrototypeSessionPhase.ReadyToFlip
        && _turnOrder.LeadActor == HalliActor.Player)
      {
        StartFlip(now);
        return;
      }

      if (Phase == PrototypeSessionPhase.BellOpen
        && _turnOrder.LeadActor == HalliActor.Player)
      {
        StartFlip(now);
      }
    }

    public void Ring(PileSide selectedPile, GameTimestamp now)
    {
      if (!CanAcceptBell())
      {
        _status = LocalizedStatus.Of("STATUS_HALLI_BELL_UNAVAILABLE");
        return;
      }

      ResolvePlayerBell(selectedPile, now, now);
    }

    public void Tick(GameTimestamp now)
    {
      switch (Phase)
      {
        case PrototypeSessionPhase.ReadyToFlip:
          if (TryResolveScheduledBellOrTimeout(now)) break;
          if (_turnOrder.LeadActor == HalliActor.Ai)
          {
            StartFlip(now);
          }
          break;
        case PrototypeSessionPhase.SequentialReveal:
          TickSequentialReveal(now);
          break;
        case PrototypeSessionPhase.BellOpen:
          TickBellWindow(now);
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
      var playerAcquired = _ledger == null
        ? EmptyCards
        : _ledger.GetCards(CardZone.PlayerAcquired);
      var aiAcquired = _ledger == null
        ? EmptyCards
        : _ledger.GetCards(CardZone.AiAcquired);
      var remaining = GetRemainingMicroseconds(now);
      var canFlip = (Phase == PrototypeSessionPhase.ReadyToFlip
          || Phase == PrototypeSessionPhase.BellOpen)
        && _turnOrder.LeadActor == HalliActor.Player;
      var canRing = CanAcceptBell();
      var revealProgress = GetRevealProgress(now);

      return new PrototypeHalliSnapshot(
        Phase,
        _status,
        _combatRoundSeed,
        _combatRoundNumber,
        _playerWins,
        _aiWins,
        playerAcquired.Count,
        aiAcquired.Count,
        playerAcquired,
        aiAcquired,
        HalliStageRules.GetWinTarget(_combatRoundNumber),
        _flipCount,
        _deck == null ? 0 : _deck.RemainingCount,
        remaining,
        _turnOrder.LeadActor,
        canFlip,
        canRing,
        _currentRevealStep?.Number ?? 0,
        _currentRevealStep?.Actor,
        _currentRevealStep?.RelativeSide,
        _currentRevealStep?.PhysicalPile,
        _currentRevealCard,
        revealProgress,
        _firstPublicCard,
        left,
        right,
        _lastAcquirer,
        _lastAcquiredCards,
        _lastAcquiredPile,
        _lastBellPile,
        _bellFeedback,
        _endReason);
    }

    public PrivateCardSelectionSession BeginPrivateCardDistribution(GameTimestamp now)
    {
      if (Phase != PrototypeSessionPhase.Finished)
      {
        throw new InvalidOperationException(
          "Private-card distribution can begin only after the Halli stage finishes.");
      }

      var winner = _endReason == HalliStageEndReason.PlayerTargetReached
        ? HalliStageWinner.Player
        : _endReason == HalliStageEndReason.AiTargetReached
          ? HalliStageWinner.Ai
          : HalliStageWinner.None;
      var playerCandidates = new List<Card>(_ledger.GetCards(CardZone.PlayerAcquired));
      var aiCandidates = new List<Card>(_ledger.GetCards(CardZone.AiAcquired));
      var otherCandidates = new List<Card>();
      otherCandidates.AddRange(_ledger.GetCards(CardZone.UnacquiredPool));
      otherCandidates.AddRange(_ledger.GetCards(CardZone.LeftPile));
      otherCandidates.AddRange(_ledger.GetCards(CardZone.RightPile));
      otherCandidates.AddRange(_ledger.GetCards(CardZone.Deck));
      otherCandidates.AddRange(_ledger.GetCards(CardZone.Reserved));
      var selection = new PrivateCardSelectionSession();
      selection.Begin(
        Array.AsReadOnly(playerCandidates.ToArray()),
        Array.AsReadOnly(aiCandidates.ToArray()),
        Array.AsReadOnly(otherCandidates.ToArray()),
        winner,
        _combatRoundNumber,
        _combatRoundSeed,
        now);
      return selection;
    }

    private void StartFlip(GameTimestamp now)
    {
      if (_nextRevealStepIndex == 0)
      {
        var endReason = ResolveEndReason();
        if (endReason != HalliStageEndReason.None)
        {
          Finish(endReason);
          return;
        }

        _flipCount++;
      }

      CloseBellWindow();
      ClearLastAcquisition();
      Phase = PrototypeSessionPhase.SequentialReveal;
      if (!RevealStep(_nextRevealStepIndex, now))
      {
        Finish(HalliStageEndReason.InsufficientCards);
      }
    }

    private void TickSequentialReveal(GameTimestamp now)
    {
      while (Phase == PrototypeSessionPhase.SequentialReveal)
      {
        if (!_currentRevealStep.HasValue)
        {
          throw new InvalidOperationException("Sequential reveal has no current step.");
        }

        var revealCompletesBeforeBell = !_currentRevealCommitted
          && (!_aiBellAt.HasValue
            || _currentRevealCompleteAt.Microseconds <= _aiBellAt.Value.Microseconds)
          && (!_bellTimerActive
            || _currentRevealCompleteAt.Microseconds <= _bellDeadline.Microseconds);
        if (revealCompletesBeforeBell
          && now.Microseconds >= _currentRevealCompleteAt.Microseconds)
        {
          CommitCurrentReveal(_currentRevealCompleteAt);
          if (Phase != PrototypeSessionPhase.SequentialReveal) return;
        }

        if (TryResolveScheduledBellOrTimeout(now)) return;

        if (!_currentRevealCommitted
          && now.Microseconds >= _currentRevealCompleteAt.Microseconds)
        {
          CommitCurrentReveal(_currentRevealCompleteAt);
          if (Phase != PrototypeSessionPhase.SequentialReveal) return;
          if (TryResolveScheduledBellOrTimeout(now)) return;
        }

        if (now.Microseconds < _nextRevealEventAt.Microseconds) return;
        var eventAt = _nextRevealEventAt;
        var step = _currentRevealStep.Value;
        if (step.Actor == HalliActor.Player
          && step.Number < HalliRevealSequence.Count)
        {
          if (!RevealStep(step.Number, eventAt))
          {
            Finish(HalliStageEndReason.InsufficientCards);
            return;
          }
        }
        else if (step.Number < HalliRevealSequence.Count)
        {
          WaitForNextPlayerFlip(eventAt);
        }
        else
        {
          CompleteSequentialReveal(eventAt);
        }
      }
    }

    private bool RevealStep(int zeroBasedIndex, GameTimestamp startedAt)
    {
      if (!_deck.TryDraw(out var card)) return false;
      var step = HalliRevealSequence.GetStep(zeroBasedIndex);
      _currentRevealStep = step;
      _currentRevealCard = card;
      _currentRevealCommitted = false;
      _ledger.Move(card.Id, CardZone.Deck, CardZone.Reserved);
      _currentRevealStartedAt = startedAt;
      var motionDuration = GameRules.CardRevealMotionMinimumMicroseconds
        + _revealRandom.NextInt((int)GameRules.CardRevealMotionRangeMicroseconds + 1);
      _currentRevealCompleteAt = Add(startedAt, motionDuration);
      if (step.Number < HalliRevealSequence.Count)
      {
        var gap = GameRules.CardRevealGapMinimumMicroseconds
          + _revealRandom.NextInt((int)GameRules.CardRevealGapRangeMicroseconds + 1);
        _nextRevealEventAt = Add(_currentRevealCompleteAt, gap);
      }
      else
      {
        _nextRevealEventAt = _currentRevealCompleteAt;
      }

      _status = new LocalizedStatus(
        "STATUS_HALLI_DISTRIBUTING",
        new LocalizedStatusArgument("step", step.Number.ToString()),
        new LocalizedStatusArgument(
          "actor",
          step.Actor == HalliActor.Player ? "UI_ACTOR_PLAYER" : "UI_ACTOR_AI",
          true),
        new LocalizedStatusArgument(
          "side",
          step.RelativeSide == HalliRelativeSide.Left ? "UI_SIDE_LEFT" : "UI_SIDE_RIGHT",
          true));
      return true;
    }

    private void CommitCurrentReveal(GameTimestamp committedAt)
    {
      if (_currentRevealCommitted || !_currentRevealStep.HasValue || !_currentRevealCard.HasValue)
      {
        return;
      }

      var step = _currentRevealStep.Value;
      var card = _currentRevealCard.Value;
      var destination = step.PhysicalPile == PileSide.Left ? CardZone.LeftPile : CardZone.RightPile;
      _ledger.Move(card.Id, CardZone.Reserved, destination);
      var displaced = _field.Expose(step.PhysicalPile, card);
      if (displaced.HasValue)
      {
        _ledger.Move(displaced.Value.Id, destination, CardZone.UnacquiredPool);
      }
      _currentRevealCommitted = true;
      RefreshBellOpportunity(committedAt);
    }

    private void WaitForNextPlayerFlip(GameTimestamp now)
    {
      if (!_currentRevealStep.HasValue
        || _currentRevealStep.Value.Actor != HalliActor.Ai)
      {
        throw new InvalidOperationException("Only a completed AI reveal can pause for player input.");
      }

      _nextRevealStepIndex = _currentRevealStep.Value.Number;
      ClearRevealState();
      BeginReady(now, LocalizedStatus.Of("STATUS_HALLI_AI_CARD_OPENED"), false);
    }

    private void CompleteSequentialReveal(GameTimestamp now)
    {
      _nextRevealStepIndex = 0;
      ClearRevealState();
      var leftValid = IsAcquirable(Evaluate(PileSide.Left));
      var rightValid = IsAcquirable(Evaluate(PileSide.Right));

      if (!leftValid && !rightValid)
      {
        var endReason = ResolveEndReason();
        if (endReason != HalliStageEndReason.None) Finish(endReason);
        else BeginReady(now, LocalizedStatus.Of("STATUS_HALLI_NO_VALID_BELL"), false);
        return;
      }

      OpenBellWindow(leftValid, rightValid, now);
      TickBellWindow(now);
    }

    private void OpenBellWindow(bool leftValid, bool rightValid, GameTimestamp now)
    {
      RefreshBellOpportunity(now, leftValid, rightValid);
      Phase = PrototypeSessionPhase.BellOpen;
      _status = LocalizedStatus.Of("STATUS_HALLI_BELL_OPEN");
    }

    private void RefreshBellOpportunity(GameTimestamp now)
    {
      RefreshBellOpportunity(
        now,
        IsAcquirable(Evaluate(PileSide.Left)),
        IsAcquirable(Evaluate(PileSide.Right)));
    }

    private void RefreshBellOpportunity(
      GameTimestamp now,
      bool leftValid,
      bool rightValid)
    {
      CloseBellWindow();
      var reactionDelay = _aiBellPolicy.CreateReactionDelay(_aiReactionRandom);
      var decision = _aiBellPolicy.Decide(
        leftValid,
        rightValid,
        reactionDelay,
        ScorePile,
        _aiReactionRandom,
        _aiChoiceRandom);
      _aiOutcome = decision.Outcome;
      _aiPile = decision.Pile;
      _aiBellAt = decision.Outcome == AiBellOutcome.Miss
        ? (GameTimestamp?)null
        : Add(now, decision.ReactionDelayMicroseconds);
    }

    private void TickBellWindow(GameTimestamp now)
    {
      if (TryResolveScheduledBellOrTimeout(now)) return;

      if (_turnOrder.LeadActor == HalliActor.Ai)
      {
        StartFlip(now);
      }
    }

    private bool TryResolveScheduledBellOrTimeout(GameTimestamp now)
    {
      if (_aiBellAt.HasValue
        && now.Microseconds >= _aiBellAt.Value.Microseconds
        && (!_bellTimerActive
          || _aiBellAt.Value.Microseconds <= _bellDeadline.Microseconds))
      {
        ResolveAiBell(_aiBellAt.Value);
        return true;
      }

      if (_bellTimerActive && now.Microseconds >= _bellDeadline.Microseconds)
      {
        ResolveBellTimeout(_bellDeadline);
        return true;
      }

      return false;
    }

    private void ResolvePlayerBell(
      PileSide selectedPile,
      GameTimestamp playerBellAt,
      GameTimestamp resolutionTime)
    {
      if (_aiBellAt.HasValue)
      {
        var difference = playerBellAt.Microseconds - _aiBellAt.Value.Microseconds;
        if (Math.Abs(difference) <= GameRules.SimultaneousBellThresholdMicroseconds)
        {
          ResolveSimultaneousBell(selectedPile, resolutionTime);
          return;
        }

        if (difference > 0)
        {
          ResolveAiBell(resolutionTime);
          return;
        }
      }

      if (IsAcquirable(Evaluate(selectedPile)))
      {
        ResolveCorrectBell(HalliActor.Player, selectedPile, resolutionTime);
      }
      else
      {
        ResolveWrongBell(
          HalliActor.Player,
          resolutionTime,
          "STATUS_HALLI_PLAYER_INVALID_PILE",
          selectedPile);
      }
    }

    private void ResolveSimultaneousBell(PileSide playerPile, GameTimestamp now)
    {
      var playerCorrect = IsAcquirable(Evaluate(playerPile));
      var aiCorrect = _aiOutcome == AiBellOutcome.Correct
        && _aiPile.HasValue
        && IsAcquirable(Evaluate(_aiPile.Value));

      if (playerCorrect)
      {
        ResolveCorrectBell(HalliActor.Player, playerPile, now);
      }
      else if (aiCorrect)
      {
        ResolveCorrectBell(HalliActor.Ai, _aiPile!.Value, now);
      }
      else
      {
        ResolveWrongBell(
          HalliActor.Player,
          now,
          "STATUS_HALLI_SIMULTANEOUS_WRONG",
          playerPile);
      }
    }

    private void ResolveAiBell(GameTimestamp now)
    {
      if (!_aiPile.HasValue || _aiOutcome == AiBellOutcome.Miss) return;

      if (_aiOutcome == AiBellOutcome.Correct
        && IsAcquirable(Evaluate(_aiPile.Value)))
      {
        ResolveCorrectBell(HalliActor.Ai, _aiPile.Value, now);
      }
      else
      {
        ResolveWrongBell(HalliActor.Ai, now, "STATUS_HALLI_AI_INVALID_PILE", _aiPile);
      }
    }

    private void ResolveCorrectBell(HalliActor actor, PileSide pile, GameTimestamp now)
    {
      StopCurrentDistribution();
      _lastBellPile = pile;
      _bellFeedback = PrototypeBellFeedback.Correct;
      var resolution = Evaluate(pile);
      Acquire(
        pile,
        resolution,
        actor == HalliActor.Player ? CardZone.PlayerAcquired : CardZone.AiAcquired);
      MoveUnselectedPileTopToBottom(pile);
      if (actor == HalliActor.Player) _playerWins++;
      else _aiWins++;
      _turnOrder.SetLead(actor);
      EnterReview(
        now,
        GameRules.HalliResultLockMicroseconds,
        LocalizedStatus.Of(
          actor == HalliActor.Player
            ? "STATUS_HALLI_PLAYER_CORRECT"
            : "STATUS_HALLI_AI_CORRECT"));
    }

    private void ResolveWrongBell(
      HalliActor loser,
      GameTimestamp now,
      string statusKey,
      PileSide? selectedPile = null)
    {
      StopCurrentDistribution();
      var winner = loser == HalliActor.Player ? HalliActor.Ai : HalliActor.Player;
      if (winner == HalliActor.Player) _playerWins++;
      else _aiWins++;
      _turnOrder.SetLead(winner);
      if (selectedPile.HasValue) MoveUnselectedPileTopToBottom(selectedPile.Value);
      _lastBellPile = selectedPile;
      _bellFeedback = selectedPile.HasValue
        ? PrototypeBellFeedback.Wrong
        : PrototypeBellFeedback.None;
      EnterReview(
        now,
        GameRules.HalliResultLockMicroseconds,
        LocalizedStatus.Of(statusKey));
    }

    private AcquisitionKind Evaluate(PileSide side)
    {
      var cards = _field.GetExposedCards(side);
      var first = cards.Count > 0 ? cards[0] : (Card?)null;
      var second = cards.Count > 1 ? cards[1] : (Card?)null;
      return SkullAcquisitionResolver.Resolve(first, second);
    }

    private int ScorePile(PileSide side)
    {
      var resolution = Evaluate(side);
      var cards = _field.GetExposedCards(side);
      var score = 0;
      for (var index = 0; index < cards.Count; index++)
      {
        var acquired = resolution == AcquisitionKind.Both
          || (resolution == AcquisitionKind.LeftOnly && index == 0)
          || (resolution == AcquisitionKind.RightOnly && index == 1);
        if (acquired)
        {
          score += ((int)cards[index].Rank * 10) + (int)cards[index].Suit;
        }
      }
      return score;
    }

    private void Acquire(PileSide side, AcquisitionKind resolution, CardZone destination)
    {
      var source = side == PileSide.Left ? CardZone.LeftPile : CardZone.RightPile;
      var cards = _field.Clear(side);
      var acquiredCards = new List<Card>();
      for (var index = 0; index < cards.Count; index++)
      {
        var acquired = resolution == AcquisitionKind.Both
          || (resolution == AcquisitionKind.LeftOnly && index == 0)
          || (resolution == AcquisitionKind.RightOnly && index == 1);
        _ledger.Move(
          cards[index].Id,
          source,
          acquired ? destination : CardZone.UnacquiredPool);
        if (acquired) acquiredCards.Add(cards[index]);
      }

      _lastAcquirer = destination == CardZone.PlayerAcquired
        ? PrototypeAcquirer.Player
        : PrototypeAcquirer.Ai;
      _lastAcquiredCards = Array.AsReadOnly(acquiredCards.ToArray());
      _lastAcquiredPile = side;
    }

    private void EnterReview(
      GameTimestamp now,
      long durationMicroseconds,
      LocalizedStatus status)
    {
      CloseBellWindow();
      _bellTimerActive = false;
      Phase = PrototypeSessionPhase.Review;
      _reviewDeadline = Add(now, durationMicroseconds);
      _status = status;
    }

    private void CompleteReview(GameTimestamp now)
    {
      var endReason = ResolveEndReason();
      if (endReason != HalliStageEndReason.None) Finish(endReason);
      else BeginReady(now, LocalizedStatus.Of("STATUS_HALLI_RESULT_LOCK_END"), true);
    }

    private void BeginReady(GameTimestamp now, LocalizedStatus status, bool startBellTimer)
    {
      ClearLastAcquisition();
      Phase = PrototypeSessionPhase.ReadyToFlip;
      if (startBellTimer || !_bellTimerActive)
      {
        _bellDeadline = Add(now, GameRules.BellInputTimeoutMicroseconds);
        _bellTimerActive = true;
      }
      _status = status;
    }

    private void ResolveBellTimeout(GameTimestamp now)
    {
      ResolveWrongBell(
        HalliActor.Player,
        now,
        "STATUS_HALLI_FLIP_TIMEOUT");
    }

    private HalliStageEndReason ResolveEndReason()
    {
      return HalliStageRules.ResolveEndReason(
        _playerWins,
        _aiWins,
        _flipCount,
        _deck.RemainingCount,
        _combatRoundNumber);
    }

    private void Finish(HalliStageEndReason endReason)
    {
      CloseBellWindow();
      _nextRevealStepIndex = 0;
      ClearRevealState();
      ClearLastAcquisition();
      Phase = PrototypeSessionPhase.Finished;
      _endReason = endReason;
      _bellTimerActive = false;
      _status = LocalizedStatus.Of(
        endReason == HalliStageEndReason.PlayerTargetReached
          ? "STATUS_HALLI_PLAYER_STAGE_WIN"
          : endReason == HalliStageEndReason.AiTargetReached
            ? "STATUS_HALLI_AI_STAGE_WIN"
            : "STATUS_HALLI_NO_TARGET_WINNER");
    }

    private long GetRemainingMicroseconds(GameTimestamp now)
    {
      if (_bellTimerActive && CanAcceptBell())
      {
        return Math.Max(0, _bellDeadline.Microseconds - now.Microseconds);
      }

      return Phase == PrototypeSessionPhase.Review
        ? Math.Max(0, _reviewDeadline.Microseconds - now.Microseconds)
        : 0;
    }

    private void CloseBellWindow()
    {
      _aiBellAt = null;
      _aiPile = null;
      _aiOutcome = AiBellOutcome.Miss;
    }

    private void ClearLastAcquisition()
    {
      _lastAcquirer = PrototypeAcquirer.None;
      _lastAcquiredCards = EmptyCards;
      _lastAcquiredPile = null;
      _lastBellPile = null;
      _bellFeedback = PrototypeBellFeedback.None;
    }

    private void ClearRevealState()
    {
      _currentRevealStep = null;
      _currentRevealCard = null;
      _currentRevealCommitted = false;
    }

    private bool CanAcceptBell()
    {
      return _bellTimerActive
        && (Phase == PrototypeSessionPhase.ReadyToFlip
          || Phase == PrototypeSessionPhase.SequentialReveal
          || Phase == PrototypeSessionPhase.BellOpen);
    }

    private void StopCurrentDistribution()
    {
      _nextRevealStepIndex = 0;
      ClearRevealState();
    }

    private void MoveUnselectedPileTopToBottom(PileSide selectedPile)
    {
      _field.MoveTopToBottom(
        selectedPile == PileSide.Left ? PileSide.Right : PileSide.Left);
    }

    private float GetRevealProgress(GameTimestamp now)
    {
      if (Phase != PrototypeSessionPhase.SequentialReveal || !_currentRevealStep.HasValue)
      {
        return 0f;
      }

      var duration = _currentRevealCompleteAt.Microseconds - _currentRevealStartedAt.Microseconds;
      if (duration <= 0 || now.Microseconds >= _currentRevealCompleteAt.Microseconds) return 1f;
      if (now.Microseconds <= _currentRevealStartedAt.Microseconds) return 0f;
      return (float)(now.Microseconds - _currentRevealStartedAt.Microseconds) / duration;
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
