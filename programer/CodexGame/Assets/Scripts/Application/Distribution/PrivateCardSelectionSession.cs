#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Poker;
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
    private IReadOnlyList<CardId> _selectedAiIds = Array.AsReadOnly(Array.Empty<CardId>());
    private IRandomSource? _random;
    private GameTimestamp _deadline;
    private HalliStageWinner _winner;
    private int _combatRoundNumber;
    private int _requiredSelectionCount;
    private bool _pairAssistEnabled;
    private PrivateCardDistributionResult? _result;
    private Card? _firstPublicCard;
    private Card? _secondPublicCard;
    private long _combatRoundSeed;

    public PrivateCardSelectionPhase Phase { get; private set; } = PrivateCardSelectionPhase.NotStarted;

    public void Begin(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      long combatRoundSeed,
      GameTimestamp now,
      bool pairAssistEnabled = false,
      int jokerAwardPercent = GameRules.JokerAwardPercent)
    {
      Begin(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        combatRoundSeed,
        null,
        now,
        pairAssistEnabled,
        jokerAwardPercent);
    }

    public void Begin(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      long combatRoundSeed,
      Card? firstPublicCard,
      GameTimestamp now,
      bool pairAssistEnabled = false,
      int jokerAwardPercent = GameRules.JokerAwardPercent)
    {
      if (Phase != PrivateCardSelectionPhase.NotStarted)
      {
        throw new InvalidOperationException("A selection session cannot be restarted or rerolled.");
      }

      ValidatePools(playerAcquiredCards, aiAcquiredCards, otherCandidates);

      if (!Enum.IsDefined(typeof(HalliStageWinner), winner))
      {
        throw new ArgumentOutOfRangeException(nameof(winner));
      }

      var retainedPlayer = winner == HalliStageWinner.Player
        ? Copy(playerAcquiredCards)
        : EmptyCards;
      var retainedAi = winner == HalliStageWinner.Ai
        ? Copy(aiAcquiredCards)
        : EmptyCards;
      var pooledCandidates = MergeWithLoserCards(
        otherCandidates,
        winner == HalliStageWinner.Player ? EmptyCards : playerAcquiredCards,
        winner == HalliStageWinner.Ai ? EmptyCards : aiAcquiredCards);

      var playerAwarded = JokerAwardResolver.Roll(
        retainedPlayer.Count,
        DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.PlayerJokerAward),
        jokerAwardPercent);
      var aiAwarded = JokerAwardResolver.Roll(
        retainedAi.Count,
        DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiJokerAward),
        jokerAwardPercent);
      _playerAcquiredCards = AppendJoker(
        retainedPlayer,
        playerAwarded,
        PokerJokerKind.BrassSheriffRevolver);
      _aiAcquiredCards = AppendJoker(
        retainedAi,
        aiAwarded,
        PokerJokerKind.CrimsonCardsharp);
      _random = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.CardDistribution);
      _firstPublicCard = firstPublicCard;
      _secondPublicCard = firstPublicCard.HasValue
        ? TakeSecondPublicCard(pooledCandidates, firstPublicCard.Value, _random)
        : null;
      _otherCandidates = Copy(pooledCandidates);
      _winner = winner;
      _combatRoundNumber = combatRoundNumber;
      _combatRoundSeed = combatRoundSeed;
      var actualWinnerCandidates = winner == HalliStageWinner.Player
        ? _playerAcquiredCards
        : winner == HalliStageWinner.Ai
          ? _aiAcquiredCards
          : EmptyCards;
      var requiresSelection = PrivateCardDistributionRules.RequiresSelectionUi(
        combatRoundNumber,
        actualWinnerCandidates.Count);
      _winnerCandidates = winner == HalliStageWinner.Player && requiresSelection
        ? actualWinnerCandidates
        : EmptyCards;
      _requiredSelectionCount = requiresSelection
        ? PrivateCardDistributionRules.GetDirectSelectionCount(combatRoundNumber)
        : 0;
      _pairAssistEnabled = pairAssistEnabled;
      _selectedIds.Clear();
      _result = null;
      _selectedAiIds = winner == HalliStageWinner.Ai && requiresSelection
        ? SelectAiPrivateCards(
          actualWinnerCandidates,
          _requiredSelectionCount,
          DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.AiChoice),
          pairAssistEnabled)
        : Array.AsReadOnly(Array.Empty<CardId>());

      if (_requiredSelectionCount == 0 || winner != HalliStageWinner.Player)
      {
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
        Phase == PrivateCardSelectionPhase.AwaitingSelection
          ? HalliStageWinner.Player
          : _winner,
        _combatRoundNumber,
        _requiredSelectionCount,
        remaining,
        _winnerCandidates,
        GetSelectedCards(),
        _result,
        _firstPublicCard,
        _secondPublicCard);
    }

    private void Complete(PrivateCardSelectionMode mode)
    {
      if (_random == null)
      {
        throw new InvalidOperationException("The selection session has no distribution random source.");
      }

      _result = _firstPublicCard.HasValue && _secondPublicCard.HasValue
        ? PrivateCardDistributionResolver.ResolveBothBalancedWithPublicCards(
          _playerAcquiredCards,
          _aiAcquiredCards,
          _otherCandidates,
          _winner,
          _combatRoundNumber,
          GetSelectedCardIds(),
          _selectedAiIds,
          mode,
          _random,
          _firstPublicCard.Value,
          _secondPublicCard.Value,
          DeterministicRandomFactory.Create(_combatRoundSeed, RandomChannel.PlayerPokerBalance),
          DeterministicRandomFactory.Create(_combatRoundSeed, RandomChannel.AiPokerBalance),
          _pairAssistEnabled)
        : PrivateCardDistributionResolver.ResolveBoth(
          _playerAcquiredCards,
          _aiAcquiredCards,
          _otherCandidates,
          _winner,
          _combatRoundNumber,
          GetSelectedCardIds(),
          _selectedAiIds,
          mode,
          _random,
          _pairAssistEnabled);
      Phase = PrivateCardSelectionPhase.Completed;
    }

    private static IReadOnlyList<Card> AppendJoker(
      IReadOnlyList<Card> source,
      bool awarded,
      PokerJokerKind kind)
    {
      var copy = new Card[source.Count + (awarded ? 1 : 0)];
      for (var index = 0; index < source.Count; index++) copy[index] = source[index];
      if (awarded) copy[copy.Length - 1] = new Card(kind);
      return Array.AsReadOnly(copy);
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

    private static IReadOnlyList<CardId> SelectAiPrivateCards(
      IReadOnlyList<Card> candidates,
      int requiredCount,
      IRandomSource random,
      bool pairAssistEnabled)
    {
      if (requiredCount == 0)
      {
        return Array.AsReadOnly(Array.Empty<CardId>());
      }

      if (pairAssistEnabled)
      {
        return PairAssistSelectionPolicy.Select(
          candidates,
          requiredCount,
          random);
      }

      var available = new List<Card>(candidates);
      var selected = new CardId[requiredCount];
      for (var index = 0; index < selected.Length; index++)
      {
        var selectedIndex = random.NextInt(available.Count);
        selected[index] = available[selectedIndex].Id;
        available.RemoveAt(selectedIndex);
      }
      return Array.AsReadOnly(selected);
    }

    private static List<Card> MergeWithLoserCards(
      IReadOnlyList<Card> otherCandidates,
      IReadOnlyList<Card> playerLoserCards,
      IReadOnlyList<Card> aiLoserCards)
    {
      var result = new List<Card>(
        otherCandidates.Count + playerLoserCards.Count + aiLoserCards.Count);
      for (var index = 0; index < otherCandidates.Count; index++) result.Add(otherCandidates[index]);
      for (var index = 0; index < playerLoserCards.Count; index++) result.Add(playerLoserCards[index]);
      for (var index = 0; index < aiLoserCards.Count; index++) result.Add(aiLoserCards[index]);
      return result;
    }

    private static Card TakeSecondPublicCard(
      List<Card> candidates,
      Card firstPublicCard,
      IRandomSource random)
    {
      if (!firstPublicCard.IsValid || firstPublicCard.IsJoker)
      {
        throw new ArgumentException("The first public card must be a standard card.", nameof(firstPublicCard));
      }
      for (var index = 0; index < candidates.Count; index++)
      {
        if (candidates[index].Id == firstPublicCard.Id)
        {
          throw new ArgumentException("The first public card cannot remain in the distribution pool.");
        }
      }
      if (candidates.Count == 0)
      {
        throw new ArgumentException("A second public card candidate is required.", nameof(candidates));
      }
      var selectedIndex = random.NextInt(candidates.Count);
      var selected = candidates[selectedIndex];
      if (selected.IsJoker)
      {
        throw new ArgumentException("The public candidate pool cannot contain a Joker.", nameof(candidates));
      }
      candidates.RemoveAt(selectedIndex);
      return selected;
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
