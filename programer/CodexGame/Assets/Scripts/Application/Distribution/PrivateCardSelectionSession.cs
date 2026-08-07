using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Distribution
{
  public sealed class PrivateCardSelectionSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards = Array.AsReadOnly(Array.Empty<Card>());

    private IReadOnlyList<Card> _playerAcquiredCards = EmptyCards;
    private IReadOnlyList<Card> _aiAcquiredCards = EmptyCards;
    private IReadOnlyList<Card> _otherCandidates = EmptyCards;
    private IReadOnlyList<Card> _winnerCandidates = EmptyCards;
    private readonly HashSet<CardId> _selectedIds = new HashSet<CardId>();
    private IRandomSource? _random;
    private GameTimestamp _deadline;
    private HalliStageWinner _winner;
    private int _combatRoundNumber;
    private int _requiredSelectionCount;
    private PrivateCardDistributionResult? _result;

    public PrivateCardSelectionPhase Phase { get; private set; } = PrivateCardSelectionPhase.NotStarted;

    public void Begin(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      long combatRoundSeed,
      GameTimestamp now)
    {
      if (Phase == PrivateCardSelectionPhase.AwaitingSelection)
      {
        throw new InvalidOperationException("An active selection session cannot be restarted.");
      }

      ValidatePools(playerAcquiredCards, aiAcquiredCards, otherCandidates);

      if (!Enum.IsDefined(typeof(HalliStageWinner), winner))
      {
        throw new ArgumentOutOfRangeException(nameof(winner));
      }

      _requiredSelectionCount = PrivateCardDistributionRules.GetDirectSelectionCount(combatRoundNumber);
      _playerAcquiredCards = Copy(playerAcquiredCards);
      _aiAcquiredCards = Copy(aiAcquiredCards);
      _otherCandidates = Copy(otherCandidates);
      _winner = winner;
      _combatRoundNumber = combatRoundNumber;
      _winnerCandidates = winner == HalliStageWinner.Player
        ? _playerAcquiredCards
        : winner == HalliStageWinner.Ai
          ? _aiAcquiredCards
          : EmptyCards;
      _selectedIds.Clear();
      _result = null;
      _random = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.CardDistribution);

      if (winner == HalliStageWinner.None)
      {
        Complete(PrivateCardSelectionMode.Confirmed);
        return;
      }

      var requiresSelection = PrivateCardDistributionRules.RequiresSelectionUi(
        combatRoundNumber,
        _winnerCandidates.Count);

      if (!requiresSelection)
      {
        for (var index = 0; index < _winnerCandidates.Count; index++)
        {
          _selectedIds.Add(_winnerCandidates[index].Id);
        }

        Complete(PrivateCardSelectionMode.Confirmed);
        return;
      }

      Phase = PrivateCardSelectionPhase.AwaitingSelection;
      _deadline = Add(now, GameRules.PrivateSelectionTimeoutMicroseconds);
    }

    public bool Toggle(CardId cardId)
    {
      if (Phase != PrivateCardSelectionPhase.AwaitingSelection)
      {
        return false;
      }

      if (_selectedIds.Remove(cardId))
      {
        return true;
      }

      if (_selectedIds.Count >= _requiredSelectionCount || !Contains(_winnerCandidates, cardId))
      {
        return false;
      }

      _selectedIds.Add(cardId);
      return true;
    }

    public bool TryConfirm()
    {
      if (Phase != PrivateCardSelectionPhase.AwaitingSelection
        || _selectedIds.Count != _requiredSelectionCount)
      {
        return false;
      }

      Complete(PrivateCardSelectionMode.Confirmed);
      return true;
    }

    public bool Tick(GameTimestamp now)
    {
      if (Phase != PrivateCardSelectionPhase.AwaitingSelection
        || now.Microseconds < _deadline.Microseconds)
      {
        return false;
      }

      Complete(PrivateCardSelectionMode.TimedOut);
      return true;
    }

    public PrivateCardSelectionSnapshot GetSnapshot(GameTimestamp now)
    {
      var remaining = Phase == PrivateCardSelectionPhase.AwaitingSelection
        ? Math.Max(0, _deadline.Microseconds - now.Microseconds)
        : 0;

      return new PrivateCardSelectionSnapshot(
        Phase,
        _winner,
        _combatRoundNumber,
        _requiredSelectionCount,
        remaining,
        _winnerCandidates,
        GetSelectedCards(),
        _result);
    }

    private void Complete(PrivateCardSelectionMode mode)
    {
      if (_random == null)
      {
        throw new InvalidOperationException("The selection session has no distribution random source.");
      }

      _result = PrivateCardDistributionResolver.Resolve(
        _playerAcquiredCards,
        _aiAcquiredCards,
        _otherCandidates,
        _winner,
        _combatRoundNumber,
        GetSelectedCardIds(),
        mode,
        _random);
      Phase = PrivateCardSelectionPhase.Completed;
    }

    private IReadOnlyList<Card> GetSelectedCards()
    {
      var cards = new List<Card>(_selectedIds.Count);

      for (var index = 0; index < _winnerCandidates.Count; index++)
      {
        if (_selectedIds.Contains(_winnerCandidates[index].Id))
        {
          cards.Add(_winnerCandidates[index]);
        }
      }

      return Array.AsReadOnly(cards.ToArray());
    }

    private IReadOnlyList<CardId> GetSelectedCardIds()
    {
      var cards = GetSelectedCards();
      var ids = new CardId[cards.Count];

      for (var index = 0; index < cards.Count; index++)
      {
        ids[index] = cards[index].Id;
      }

      return Array.AsReadOnly(ids);
    }

    private static bool Contains(IReadOnlyList<Card> cards, CardId cardId)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == cardId)
        {
          return true;
        }
      }

      return false;
    }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> source)
    {
      var copy = new Card[source.Count];

      for (var index = 0; index < source.Count; index++)
      {
        copy[index] = source[index];
      }

      return Array.AsReadOnly(copy);
    }

    private static void ValidatePools(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates)
    {
      if (playerAcquiredCards == null)
      {
        throw new ArgumentNullException(nameof(playerAcquiredCards));
      }

      if (aiAcquiredCards == null)
      {
        throw new ArgumentNullException(nameof(aiAcquiredCards));
      }

      if (otherCandidates == null)
      {
        throw new ArgumentNullException(nameof(otherCandidates));
      }

      var ids = new HashSet<CardId>();
      AddAndValidate(playerAcquiredCards, ids, nameof(playerAcquiredCards));
      AddAndValidate(aiAcquiredCards, ids, nameof(aiAcquiredCards));
      AddAndValidate(otherCandidates, ids, nameof(otherCandidates));
    }

    private static void AddAndValidate(
      IReadOnlyList<Card> cards,
      HashSet<CardId> ids,
      string parameterName)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsValid || !ids.Add(cards[index].Id))
        {
          throw new ArgumentException(
            "Selection inputs must contain valid cards with no duplicate identities.",
            parameterName);
        }
      }
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
