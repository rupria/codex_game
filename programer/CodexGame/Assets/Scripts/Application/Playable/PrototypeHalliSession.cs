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

    private Deck _deck = null!;
    private CardLedger _ledger = null!;
    private HalliField _field = null!;
    private IRandomSource _aiReactionRandom = null!;
    private IRandomSource _aiChoiceRandom = null!;
    private BellWindowTracker _bellWindows = null!;
    private readonly AiPrivateCardSelectionPolicy _aiRewardSelectionPolicy =
      new AiPrivateCardSelectionPolicy();
    private WrongBellRewardSelectionSession _wrongBellRewardSelection =
      new WrongBellRewardSelectionSession();
    private Card? _firstPublicCard;
    private GameTimestamp _readyDeadline;
    private GameTimestamp _reviewDeadline;
    private GameTimestamp? _aiBellAt;
    private PileSide? _aiPile;
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
      _aiReactionRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiReaction);
      _aiChoiceRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiChoice);
      _bellWindows = new BellWindowTracker();
      _wrongBellRewardSelection = new WrongBellRewardSelectionSession();
      _playerWins = 0;
      _aiWins = 0;
      _flipCount = 0;
      _endReason = HalliStageEndReason.None;
      _aiBellAt = null;
      _aiPile = null;
      ClearLastAcquisition();

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
      if (Phase != PrototypeSessionPhase.ReadyToFlip
        && Phase != PrototypeSessionPhase.BellOpen)
      {
        _statusMessage = "Bell input is unavailable during this phase.";
        return;
      }

      if (Phase == PrototypeSessionPhase.BellOpen
        && _aiBellAt.HasValue
        && ReactionResolver.Resolve(now, _aiBellAt.Value) == ReactionWinner.Ai)
      {
        ResolveAiBell(now);
        return;
      }

      ResolvePlayerBell(selectedPile, now);
    }

    public bool SelectWrongBellReward(CardId cardId, GameTimestamp now)
    {
      if (Phase != PrototypeSessionPhase.WrongBellRewardSelection
        || !_wrongBellRewardSelection.TrySelect(cardId))
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
          if (now.Microseconds >= _readyDeadline.Microseconds)
          {
            ResolvePlayerFlipTimeout(now);
          }
          break;
        case PrototypeSessionPhase.BellOpen:
          if (_aiBellAt.HasValue && now.Microseconds >= _aiBellAt.Value.Microseconds)
          {
            ResolveAiBell(now);
          }
          else if (now.Microseconds >= _readyDeadline.Microseconds)
          {
            ResolvePlayerFlipTimeout(now);
          }
          break;
        case PrototypeSessionPhase.WrongBellRewardSelection:
          if (_wrongBellRewardSelection.Tick(now))
          {
            CompleteWrongBellRewardSelection(now);
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

      if (Phase == PrototypeSessionPhase.ReadyToFlip
        || Phase == PrototypeSessionPhase.BellOpen)
      {
        remaining = Math.Max(0, _readyDeadline.Microseconds - now.Microseconds);
      }
      else if (Phase == PrototypeSessionPhase.Review)
      {
        remaining = Math.Max(0, _reviewDeadline.Microseconds - now.Microseconds);
      }
      else if (Phase == PrototypeSessionPhase.WrongBellRewardSelection)
      {
        remaining = _wrongBellRewardSelection.GetRemainingMicroseconds(now);
      }

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
      AssignScoreOnlyWinnerFallback(
        winner,
        playerCandidates,
        aiCandidates,
        otherCandidates);

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

      if (winnerCandidates == null || winnerCandidates.Count > 0)
      {
        return;
      }

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

    private void Flip(GameTimestamp now)
    {
      var endReason = ResolveEndReason();

      if (endReason != HalliStageEndReason.None)
      {
        Finish(endReason);
        return;
      }

      CloseBellWindow();
      ClearLastAcquisition();
      ExposeFromDeck(PileSide.Left);
      ExposeFromDeck(PileSide.Right);
      _flipCount++;
      ResetFlipDeadline(now);

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

      _aiWins++;
      var rewarded = TryAwardAiWrongBellReward();
      EnterReview(
        now,
        rewarded
          ? "Wrong bell. AI gains one Halli win and selects one unacquired card."
          : "Wrong bell. AI gains one Halli win; no unacquired reward card is available.");
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
        _playerWins++;
        BeginWrongBellRewardSelection(now);
      }
    }

    private bool TryAwardAiWrongBellReward()
    {
      var candidates = _ledger.GetCards(CardZone.UnacquiredPool);
      if (candidates.Count == 0)
      {
        return false;
      }

      var selected = _aiRewardSelectionPolicy.Select(candidates, 1, _aiChoiceRandom)[0];
      for (var index = 0; index < candidates.Count; index++)
      {
        if (candidates[index].Id != selected)
        {
          continue;
        }

        _ledger.Move(selected, CardZone.UnacquiredPool, CardZone.AiAcquired);
        SetLastAcquisition(PrototypeAcquirer.Ai, candidates[index]);
        return true;
      }

      throw new InvalidOperationException("AI wrong-bell reward selection returned an unknown card.");
    }

    private void BeginWrongBellRewardSelection(GameTimestamp now)
    {
      var candidates = _ledger.GetCards(CardZone.UnacquiredPool);
      if (candidates.Count == 0)
      {
        EnterReview(
          now,
          "AI chose incorrectly. Player gains one Halli win; no unacquired reward card is available.");
        return;
      }

      CloseBellWindow();
      ClearLastAcquisition();
      _wrongBellRewardSelection.Begin(
        candidates,
        DeterministicRandomFactory.Create(
          _combatRoundSeed,
          RandomChannel.WrongBellReward),
        now);
      Phase = PrototypeSessionPhase.WrongBellRewardSelection;
      _statusMessage =
        "AI chose incorrectly. Select one unacquired card within 30 seconds (Q/E, W/ENTER).";
    }

    private void CompleteWrongBellRewardSelection(GameTimestamp now)
    {
      if (!_wrongBellRewardSelection.SelectedCard.HasValue)
      {
        throw new InvalidOperationException("Wrong-bell reward selection has no selected card.");
      }

      var selected = _wrongBellRewardSelection.SelectedCard.Value;
      _ledger.Move(selected.Id, CardZone.UnacquiredPool, CardZone.PlayerAcquired);
      SetLastAcquisition(PrototypeAcquirer.Player, selected);
      EnterReview(
        now,
        _wrongBellRewardSelection.TimedOut
          ? "AI chose incorrectly. Selection timed out; player receives one random unacquired card."
          : "AI chose incorrectly. Player receives the selected unacquired card.");
    }

    private void SetLastAcquisition(PrototypeAcquirer acquirer, Card card)
    {
      _lastAcquirer = acquirer;
      _lastAcquiredCards = Array.AsReadOnly(new[] { card });
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

        if (acquired)
        {
          acquiredCards.Add(cards[index]);
        }
      }

      _lastAcquirer = destination == CardZone.PlayerAcquired
        ? PrototypeAcquirer.Player
        : PrototypeAcquirer.Ai;
      _lastAcquiredCards = Array.AsReadOnly(acquiredCards.ToArray());
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
      ClearLastAcquisition();
      Phase = PrototypeSessionPhase.ReadyToFlip;
      ResetFlipDeadline(now);
      _statusMessage = message;
    }

    private void ResolvePlayerFlipTimeout(GameTimestamp now)
    {
      CloseBellWindow();
      ClearLastAcquisition();
      _aiWins++;
      var endReason = ResolveEndReason();

      if (endReason != HalliStageEndReason.None)
      {
        Finish(endReason);
        return;
      }

      Phase = PrototypeSessionPhase.ReadyToFlip;
      ResetFlipDeadline(now);
      _statusMessage = "30 second flip timeout. Player loses; AI gains one Halli win. The field stays.";
    }

    private void ResetFlipDeadline(GameTimestamp now)
    {
      _readyDeadline = Add(now, GameRules.CardFlipTimeoutMicroseconds);
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
        _combatRoundNumber);
    }

    private void Finish(HalliStageEndReason endReason)
    {
      CloseBellWindow();
      ClearLastAcquisition();
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

    private void ClearLastAcquisition()
    {
      _lastAcquirer = PrototypeAcquirer.None;
      _lastAcquiredCards = EmptyCards;
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
