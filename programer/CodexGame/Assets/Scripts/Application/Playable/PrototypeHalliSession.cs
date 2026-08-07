using System;
using System.Collections.Generic;
using CodexGame.Application.Distribution;
using CodexGame.Core.Ai;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeHalliSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards = Array.AsReadOnly(Array.Empty<Card>());

    private readonly AiPrivateCardSelectionPolicy _aiRewardSelectionPolicy =
      new AiPrivateCardSelectionPolicy();
    private readonly HalliAiBellPolicy _aiBellPolicy = new HalliAiBellPolicy();

    private Deck _deck = null!;
    private CardLedger _ledger = null!;
    private HalliField _field = null!;
    private HalliTurnOrder _turnOrder = new HalliTurnOrder();
    private IRandomSource _revealRandom = null!;
    private IRandomSource _aiReactionRandom = null!;
    private IRandomSource _aiChoiceRandom = null!;
    private IRandomSource _wrongRewardRandom = null!;
    private WrongBellRewardSelectionSession _wrongBellRewardSelection =
      new WrongBellRewardSelectionSession();
    private Card? _firstPublicCard;
    private GameTimestamp _readyDeadline;
    private GameTimestamp _followerRevealAt;
    private GameTimestamp _nextFlipAvailableAt;
    private GameTimestamp _reviewDeadline;
    private GameTimestamp _flipStartedAt;
    private GameTimestamp? _aiBellAt;
    private GameTimestamp? _queuedPlayerBellAt;
    private PileSide? _aiPile;
    private PileSide? _queuedPlayerPile;
    private PileSide _pendingFollowerPile;
    private AiBellOutcome _aiOutcome;
    private HalliActor _wrongBellRewardWinner = HalliActor.Player;
    private string _statusMessage = "Press START to play.";
    private long _combatRoundSeed;
    private int _combatRoundNumber = 1;
    private int _playerWins;
    private int _aiWins;
    private int _flipCount;
    private HalliStageEndReason _endReason;
    private PrototypeAcquirer _lastAcquirer;
    private IReadOnlyList<Card> _lastAcquiredCards = EmptyCards;

    public PrototypeSessionPhase Phase { get; private set; } = PrototypeSessionPhase.Intro;

    public void StartNew(
      GameTimestamp now,
      long combatRoundSeed,
      int combatRoundNumber = 1)
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
      _wrongRewardRandom = DeterministicRandomFactory.Create(
        combatRoundSeed,
        RandomChannel.WrongBellReward);
      _wrongBellRewardSelection = new WrongBellRewardSelectionSession();
      _playerWins = 0;
      _aiWins = 0;
      _flipCount = 0;
      _endReason = HalliStageEndReason.None;
      CloseBellWindow();
      ClearLastAcquisition();

      var firstPublic = _deck.Draw();
      _ledger.Move(firstPublic.Id, CardZone.Deck, CardZone.FirstPublic);
      _firstPublicCard = firstPublic;
      BeginReady(now, "First public card opened. Player leads the first flip.");
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
        && _turnOrder.LeadActor == HalliActor.Player
        && now.Microseconds >= _nextFlipAvailableAt.Microseconds)
      {
        StartFlip(now);
      }
    }

    public void Ring(PileSide selectedPile, GameTimestamp now)
    {
      if (Phase == PrototypeSessionPhase.SequentialReveal)
      {
        if (!_queuedPlayerPile.HasValue)
        {
          _queuedPlayerPile = selectedPile;
          _queuedPlayerBellAt = now;
          _statusMessage = "Bell queued until both sequential cards are visible.";
        }
        return;
      }

      if (Phase != PrototypeSessionPhase.BellOpen)
      {
        _statusMessage = "Bell input is unavailable during this phase.";
        return;
      }

      ResolvePlayerBell(selectedPile, now, now);
    }

    public bool SelectWrongBellReward(CardId cardId, GameTimestamp now)
    {
      if (Phase != PrototypeSessionPhase.WrongBellRewardSelection
        || _wrongBellRewardWinner != HalliActor.Player
        || !_wrongBellRewardSelection.TrySelect(cardId, now))
      {
        return false;
      }

      CompleteWrongBellRewardSelection(now);
      return true;
    }

    public void Tick(GameTimestamp now)
    {
      switch (Phase)
      {
        case PrototypeSessionPhase.ReadyToFlip:
          if (_turnOrder.LeadActor == HalliActor.Ai)
          {
            StartFlip(now);
          }
          else if (now.Microseconds >= _readyDeadline.Microseconds)
          {
            ResolvePlayerFlipTimeout(now);
          }
          break;
        case PrototypeSessionPhase.SequentialReveal:
          if (now.Microseconds >= _followerRevealAt.Microseconds)
          {
            CompleteSequentialReveal(now);
          }
          break;
        case PrototypeSessionPhase.BellOpen:
          TickBellWindow(now);
          break;
        case PrototypeSessionPhase.WrongBellRewardSelection:
          TickWrongBellReward(now);
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
      var remaining = GetRemainingMicroseconds(now);
      var canFlip = (Phase == PrototypeSessionPhase.ReadyToFlip
          || Phase == PrototypeSessionPhase.BellOpen)
        && _turnOrder.LeadActor == HalliActor.Player
        && (Phase != PrototypeSessionPhase.BellOpen
          || now.Microseconds >= _nextFlipAvailableAt.Microseconds);
      var canRing = Phase == PrototypeSessionPhase.SequentialReveal
        || Phase == PrototypeSessionPhase.BellOpen;

      return new PrototypeHalliSnapshot(
        Phase,
        _statusMessage,
        _combatRoundSeed,
        _combatRoundNumber,
        _playerWins,
        _aiWins,
        _ledger == null ? 0 : _ledger.Count(CardZone.PlayerAcquired),
        _ledger == null ? 0 : _ledger.Count(CardZone.AiAcquired),
        HalliStageRules.GetWinTarget(_combatRoundNumber),
        _flipCount,
        _deck == null ? 0 : _deck.RemainingCount,
        remaining,
        _turnOrder.LeadActor,
        canFlip,
        canRing,
        Phase == PrototypeSessionPhase.WrongBellRewardSelection
          && _wrongBellRewardWinner == HalliActor.Player
          && _wrongBellRewardSelection.CanSelect(now),
        _firstPublicCard,
        left,
        right,
        _lastAcquirer,
        _lastAcquiredCards,
        Phase == PrototypeSessionPhase.WrongBellRewardSelection
          ? _wrongBellRewardSelection.Candidates
          : EmptyCards,
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
      AssignScoreOnlyWinnerFallback(winner, playerCandidates, aiCandidates, otherCandidates);

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
      var endReason = ResolveEndReason();
      if (endReason != HalliStageEndReason.None)
      {
        Finish(endReason);
        return;
      }

      CloseBellWindow();
      ClearLastAcquisition();
      _flipStartedAt = now;
      var lead = _turnOrder.LeadActor;
      var follower = _turnOrder.GetFollower();
      var leadPile = _turnOrder.TakeNextPile(lead);
      _pendingFollowerPile = _turnOrder.TakeNextPile(follower);
      ExposeFromDeck(leadPile);
      var delay = GameRules.SequentialRevealMinimumMicroseconds
        + _revealRandom.NextInt((int)GameRules.SequentialRevealRangeMicroseconds + 1);
      _followerRevealAt = Add(now, delay);
      Phase = PrototypeSessionPhase.SequentialReveal;
      _statusMessage = lead == HalliActor.Player
        ? "Player lead card opened. AI card follows in 0.3-0.5 seconds."
        : "AI lead card opened. Player card follows in 0.3-0.5 seconds.";
    }

    private void CompleteSequentialReveal(GameTimestamp now)
    {
      ExposeFromDeck(_pendingFollowerPile);
      _flipCount++;
      var leftValid = IsAcquirable(Evaluate(PileSide.Left));
      var rightValid = IsAcquirable(Evaluate(PileSide.Right));

      if (!leftValid && !rightValid)
      {
        if (_queuedPlayerPile.HasValue && _queuedPlayerBellAt.HasValue)
        {
          ResolveWrongBell(HalliActor.Player, now, "Player rang before a valid field existed.");
          return;
        }

        var endReason = ResolveEndReason();
        if (endReason != HalliStageEndReason.None) Finish(endReason);
        else BeginReady(now, "No valid bell. The current leader continues.");
        return;
      }

      OpenBellWindow(leftValid, rightValid, now);
      if (_queuedPlayerPile.HasValue && _queuedPlayerBellAt.HasValue)
      {
        ResolvePlayerBell(_queuedPlayerPile.Value, _queuedPlayerBellAt.Value, now);
      }
      else
      {
        TickBellWindow(now);
      }
    }

    private void OpenBellWindow(bool leftValid, bool rightValid, GameTimestamp now)
    {
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
        : Add(_flipStartedAt, decision.ReactionDelayMicroseconds);
      _nextFlipAvailableAt = Add(now, GameRules.NextFlipLockMicroseconds);
      _readyDeadline = Add(_nextFlipAvailableAt, GameRules.CardFlipTimeoutMicroseconds);
      Phase = PrototypeSessionPhase.BellOpen;
      _statusMessage = "Bell opportunity open. Next flip unlocks after one second.";
    }

    private void TickBellWindow(GameTimestamp now)
    {
      if (_aiBellAt.HasValue && now.Microseconds >= _aiBellAt.Value.Microseconds)
      {
        ResolveAiBell(now);
        return;
      }

      if (now.Microseconds < _nextFlipAvailableAt.Microseconds) return;

      if (_turnOrder.LeadActor == HalliActor.Ai)
      {
        StartFlip(now);
      }
      else if (now.Microseconds >= _readyDeadline.Microseconds)
      {
        ResolvePlayerFlipTimeout(now);
      }
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
        ResolveWrongBell(HalliActor.Player, resolutionTime, "Player selected an invalid pile.");
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
          "Both simultaneous bell inputs were wrong; player input resolves first.");
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
        ResolveWrongBell(HalliActor.Ai, now, "AI selected an invalid pile.");
      }
    }

    private void ResolveCorrectBell(HalliActor actor, PileSide pile, GameTimestamp now)
    {
      var resolution = Evaluate(pile);
      Acquire(
        pile,
        resolution,
        actor == HalliActor.Player ? CardZone.PlayerAcquired : CardZone.AiAcquired);
      if (actor == HalliActor.Player) _playerWins++;
      else _aiWins++;
      _turnOrder.SetLead(actor);
      EnterReview(
        now,
        GameRules.NextFlipLockMicroseconds,
        actor == HalliActor.Player
          ? "Correct bell. Player gains one Halli win and leads next."
          : "AI rang correctly, gains one Halli win, and leads next.");
    }

    private void ResolveWrongBell(HalliActor loser, GameTimestamp now, string reason)
    {
      var winner = loser == HalliActor.Player ? HalliActor.Ai : HalliActor.Player;
      if (winner == HalliActor.Player) _playerWins++;
      else _aiWins++;
      _turnOrder.SetLead(winner);
      BeginWrongBellRewardSelection(now, winner, reason);
    }

    private void BeginWrongBellRewardSelection(
      GameTimestamp now,
      HalliActor winner,
      string reason)
    {
      CloseBellWindow();
      ClearLastAcquisition();
      _wrongBellRewardWinner = winner;
      var candidates = _ledger.GetCards(CardZone.UnacquiredPool);
      if (candidates.Count == 0)
      {
        EnterReview(
          now,
          GameRules.WrongBellRewardResultLockMicroseconds,
          reason + " Opponent gains one Halli win; no reward card is available.");
        return;
      }

      _wrongBellRewardSelection.Begin(
        candidates,
        _wrongRewardRandom,
        now);
      Phase = PrototypeSessionPhase.WrongBellRewardSelection;
      _statusMessage = reason
        + (winner == HalliActor.Player
          ? " Reward list unlocks in two seconds, then select within 30 seconds."
          : " AI reward is shown and resolves after the two-second review lock.");
    }

    private void TickWrongBellReward(GameTimestamp now)
    {
      if (_wrongBellRewardWinner == HalliActor.Ai
        && _wrongBellRewardSelection.CanSelect(now))
      {
        var candidates = _wrongBellRewardSelection.Candidates;
        var selected = _aiRewardSelectionPolicy.Select(candidates, 1, _aiChoiceRandom)[0];
        if (!_wrongBellRewardSelection.TrySelect(selected, now))
        {
          throw new InvalidOperationException("AI reward policy returned an invalid card.");
        }
        CompleteWrongBellRewardSelection(now);
        return;
      }

      if (_wrongBellRewardWinner == HalliActor.Player
        && _wrongBellRewardSelection.Tick(now))
      {
        CompleteWrongBellRewardSelection(now);
      }
    }

    private void CompleteWrongBellRewardSelection(GameTimestamp now)
    {
      if (!_wrongBellRewardSelection.SelectedCard.HasValue)
      {
        throw new InvalidOperationException("Wrong-bell reward selection has no selected card.");
      }

      var selected = _wrongBellRewardSelection.SelectedCard.Value;
      var destination = _wrongBellRewardWinner == HalliActor.Player
        ? CardZone.PlayerAcquired
        : CardZone.AiAcquired;
      _ledger.Move(selected.Id, CardZone.UnacquiredPool, destination);
      SetLastAcquisition(
        _wrongBellRewardWinner == HalliActor.Player
          ? PrototypeAcquirer.Player
          : PrototypeAcquirer.Ai,
        selected);
      EnterReview(
        now,
        GameRules.WrongBellRewardResultLockMicroseconds,
        _wrongBellRewardSelection.TimedOut
          ? "Reward selection timed out; one deterministic random card was awarded."
          : "Wrong-bell reward card awarded. Winner leads next.");
    }

    private void AssignScoreOnlyWinnerFallback(
      HalliStageWinner winner,
      List<Card> playerCandidates,
      List<Card> aiCandidates,
      List<Card> otherCandidates)
    {
      var winnerCandidates = winner == HalliStageWinner.Player
        ? playerCandidates
        : winner == HalliStageWinner.Ai
          ? aiCandidates
          : null;

      if (winnerCandidates == null || winnerCandidates.Count > 0) return;
      if (otherCandidates.Count == 0)
      {
        throw new InvalidOperationException(
          "A score-only Halli winner needs one fallback card candidate.");
      }

      var random = DeterministicRandomFactory.Create(
        _combatRoundSeed,
        RandomChannel.ScoreOnlyWinFallback);
      var fallbackIndex = random.NextInt(otherCandidates.Count);
      winnerCandidates.Add(otherCandidates[fallbackIndex]);
      otherCandidates.RemoveAt(fallbackIndex);
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
    }

    private void SetLastAcquisition(PrototypeAcquirer acquirer, Card card)
    {
      _lastAcquirer = acquirer;
      _lastAcquiredCards = Array.AsReadOnly(new[] { card });
    }

    private void EnterReview(GameTimestamp now, long durationMicroseconds, string message)
    {
      CloseBellWindow();
      Phase = PrototypeSessionPhase.Review;
      _reviewDeadline = Add(now, durationMicroseconds);
      _statusMessage = message;
    }

    private void CompleteReview(GameTimestamp now)
    {
      var endReason = ResolveEndReason();
      if (endReason != HalliStageEndReason.None) Finish(endReason);
      else BeginReady(now, "Result lock ended. The Halli winner leads next.");
    }

    private void BeginReady(GameTimestamp now, string message)
    {
      ClearLastAcquisition();
      Phase = PrototypeSessionPhase.ReadyToFlip;
      _readyDeadline = _turnOrder.LeadActor == HalliActor.Player
        ? Add(now, GameRules.CardFlipTimeoutMicroseconds)
        : now;
      _statusMessage = message;
    }

    private void ResolvePlayerFlipTimeout(GameTimestamp now)
    {
      _aiWins++;
      _turnOrder.SetLead(HalliActor.Ai);
      BeginWrongBellRewardSelection(
        now,
        HalliActor.Ai,
        "Player exceeded the 30-second flip limit.");
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
      ClearLastAcquisition();
      Phase = PrototypeSessionPhase.Finished;
      _endReason = endReason;
      _statusMessage = endReason == HalliStageEndReason.PlayerTargetReached
        ? "PLAYER WINS the Halli stage. Continue to private cards."
        : endReason == HalliStageEndReason.AiTargetReached
          ? "AI WINS the Halli stage. Continue to private cards."
          : "Halli stage ended without a target winner. Continue to private cards.";
    }

    private long GetRemainingMicroseconds(GameTimestamp now)
    {
      if (Phase == PrototypeSessionPhase.SequentialReveal)
      {
        return Math.Max(0, _followerRevealAt.Microseconds - now.Microseconds);
      }

      if (Phase == PrototypeSessionPhase.ReadyToFlip
        && _turnOrder.LeadActor == HalliActor.Player)
      {
        return Math.Max(0, _readyDeadline.Microseconds - now.Microseconds);
      }

      if (Phase == PrototypeSessionPhase.BellOpen)
      {
        var deadline = now.Microseconds < _nextFlipAvailableAt.Microseconds
          ? _nextFlipAvailableAt
          : _turnOrder.LeadActor == HalliActor.Player
            ? _readyDeadline
            : now;
        return Math.Max(0, deadline.Microseconds - now.Microseconds);
      }

      if (Phase == PrototypeSessionPhase.WrongBellRewardSelection)
      {
        return _wrongBellRewardSelection.GetRemainingMicroseconds(now);
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
      _queuedPlayerBellAt = null;
      _queuedPlayerPile = null;
    }

    private void ClearLastAcquisition()
    {
      _lastAcquirer = PrototypeAcquirer.None;
      _lastAcquiredCards = EmptyCards;
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
