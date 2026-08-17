using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Items;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Items
{
  public sealed class PokerItemSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards = Array.AsReadOnly(Array.Empty<Card>());

    private readonly List<Card> _playerCards = new List<Card>();
    private readonly List<Card> _aiCards = new List<Card>();
    private readonly List<Card> _remainingCandidates = new List<Card>();
    private readonly List<Card> _bottomDealCandidates = new List<Card>();
    private RunInventory _inventory = null!;
    private PrivateCardDistributionResult _source = null!;
    private IRandomSource _random = null!;
    private StageItemRestrictionSession? _stageRestriction;
    private CardId _bottomDealTarget;
    private Card _firstPublicCard;
    private int _visibleAiCardIndex = -1;
    private GameTimestamp _secondPublicRevealStartedAt;
    private GameTimestamp _secondPublicRevealEndsAt;

    public PokerItemPhase Phase { get; private set; } = PokerItemPhase.NotStarted;
    public PokerItemFailure LastFailure { get; private set; }

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long combatRoundSeed,
      StageItemRestrictionSession? stageRestriction = null)
    {
      BeginCore(
        firstPublicCard,
        distribution,
        inventory,
        combatRoundSeed,
        stageRestriction,
        new GameTimestamp(0),
        false);
    }

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long combatRoundSeed,
      GameTimestamp now,
      StageItemRestrictionSession? stageRestriction = null)
    {
      BeginCore(
        firstPublicCard,
        distribution,
        inventory,
        combatRoundSeed,
        stageRestriction,
        now,
        true);
    }

    private void BeginCore(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long combatRoundSeed,
      StageItemRestrictionSession? stageRestriction,
      GameTimestamp now,
      bool presentSecondPublicCard)
    {
      if (!firstPublicCard.IsValid)
      {
        throw new ArgumentException("The first public card is invalid.", nameof(firstPublicCard));
      }
      _source = distribution ?? throw new ArgumentNullException(nameof(distribution));
      _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
      _random = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.ItemUse);
      _stageRestriction = stageRestriction;
      _firstPublicCard = firstPublicCard;
      Replace(_playerCards, distribution.PlayerPrivateCards);
      Replace(_aiCards, distribution.AiPrivateCards);
      Replace(_remainingCandidates, distribution.RemainingCandidates);
      _bottomDealCandidates.Clear();
      _visibleAiCardIndex = -1;
      LastFailure = PokerItemFailure.None;
      if (presentSecondPublicCard)
      {
        _secondPublicRevealStartedAt = now;
        _secondPublicRevealEndsAt = Add(now, GameRules.SecondPublicCardRevealMicroseconds);
        Phase = PokerItemPhase.RevealingSecondPublic;
      }
      else
      {
        Phase = inventory.Count == 0
          ? PokerItemPhase.Completed
          : PokerItemPhase.AwaitingActions;
      }
    }

    public bool Tick(GameTimestamp now)
    {
      if (Phase != PokerItemPhase.RevealingSecondPublic
        || now.Microseconds < _secondPublicRevealEndsAt.Microseconds) return false;
      Phase = _inventory.Count == 0
        ? PokerItemPhase.Completed
        : PokerItemPhase.AwaitingActions;
      return true;
    }

    public PokerItemFailure UseReload(CardId target)
    {
      if (!CanUse(GameItemId.Reload, target, out var targetIndex)) return LastFailure;

      var candidates = new List<Card>(_remainingCandidates) { _playerCards[targetIndex] };
      if (!ValidateCandidatePool(candidates, 1)) return LastFailure;
      var selectedIndex = _random.NextInt(candidates.Count);
      var selected = candidates[selectedIndex];
      candidates.RemoveAt(selectedIndex);

      _playerCards[targetIndex] = selected;
      Replace(_remainingCandidates, candidates);
      Consume(GameItemId.Reload);
      return LastFailure;
    }

    public PokerItemFailure BeginBottomDeal(CardId target)
    {
      if (!CanUse(GameItemId.BottomDeal, target, out var targetIndex)) return LastFailure;
      var candidates = new List<Card>(_remainingCandidates) { _playerCards[targetIndex] };
      if (!ValidateCandidatePool(candidates, 2)) return LastFailure;

      _bottomDealCandidates.Clear();
      for (var count = 0; count < 2; count++)
      {
        var selectedIndex = _random.NextInt(candidates.Count);
        _bottomDealCandidates.Add(candidates[selectedIndex]);
        candidates.RemoveAt(selectedIndex);
      }
      _bottomDealTarget = target;
      LastFailure = PokerItemFailure.None;
      Phase = PokerItemPhase.AwaitingBottomDealChoice;
      return LastFailure;
    }

    public PokerItemFailure ChooseBottomDeal(CardId candidateId)
    {
      if (Phase != PokerItemPhase.AwaitingBottomDealChoice)
      {
        return Fail(PokerItemFailure.WrongPhase);
      }
      var targetIndex = Find(_playerCards, _bottomDealTarget);
      var choiceIndex = Find(_bottomDealCandidates, candidateId);
      if (targetIndex < 0) return Fail(PokerItemFailure.InvalidTarget);
      if (choiceIndex < 0) return Fail(PokerItemFailure.InvalidCandidate);

      var candidates = new List<Card>(_remainingCandidates) { _playerCards[targetIndex] };
      var selected = _bottomDealCandidates[choiceIndex];
      var selectedPoolIndex = Find(candidates, selected.Id);
      if (selectedPoolIndex < 0) return Fail(PokerItemFailure.InvalidCandidate);
      candidates.RemoveAt(selectedPoolIndex);
      _playerCards[targetIndex] = selected;
      Replace(_remainingCandidates, candidates);
      _bottomDealCandidates.Clear();
      Consume(GameItemId.BottomDeal);
      if (Phase != PokerItemPhase.Completed) Phase = PokerItemPhase.AwaitingActions;
      return LastFailure;
    }

    public PokerItemFailure UseHypeMan()
    {
      if (Phase != PokerItemPhase.AwaitingActions) return Fail(PokerItemFailure.WrongPhase);
      if (!_inventory.Contains(GameItemId.HypeMan)) return Fail(PokerItemFailure.ItemNotOwned);
      if (!CanUseInCurrentStage()) return LastFailure;
      if (_aiCards.Count == 0) return Fail(PokerItemFailure.CandidatePoolExhausted);
      var visibleCandidates = new List<int>(_aiCards.Count);
      for (var index = 0; index < _aiCards.Count; index++)
      {
        if (!_aiCards[index].IsJoker) visibleCandidates.Add(index);
      }
      if (visibleCandidates.Count == 0) return Fail(PokerItemFailure.CandidatePoolExhausted);
      _visibleAiCardIndex = visibleCandidates[_random.NextInt(visibleCandidates.Count)];
      Consume(GameItemId.HypeMan);
      return LastFailure;
    }

    public PokerItemFailure UseHealthRecovery(bool canRecover)
    {
      if (Phase != PokerItemPhase.AwaitingActions) return Fail(PokerItemFailure.WrongPhase);
      if (!_inventory.Contains(GameItemId.HealthRecovery)) return Fail(PokerItemFailure.ItemNotOwned);
      if (!CanUseInCurrentStage()) return LastFailure;
      if (!canRecover) return Fail(PokerItemFailure.HealthAlreadyFull);
      Consume(GameItemId.HealthRecovery);
      return LastFailure;
    }

    public bool Confirm()
    {
      if (Phase != PokerItemPhase.AwaitingActions) return false;
      Phase = PokerItemPhase.Completed;
      LastFailure = PokerItemFailure.None;
      return true;
    }

    public PrivateCardDistributionResult GetResult()
    {
      if (Phase != PokerItemPhase.Completed || _source == null)
      {
        throw new InvalidOperationException("Item actions must be completed before finalizing the hand.");
      }
      return new PrivateCardDistributionResult(
        _source.Winner,
        _source.CombatRoundNumber,
        _playerCards,
        _aiCards,
        _source.SecondPublicCard,
        _remainingCandidates);
    }

    public PokerItemSnapshot GetSnapshot(
      GameTimestamp? now = null,
      ItemUsePresentationSnapshot? usePresentation = null)
    {
      var visibleAi = _visibleAiCardIndex >= 0
        ? Array.AsReadOnly(new[] { _aiCards[_visibleAiCardIndex] })
        : EmptyCards;
      var publicCards = _source == null
        ? EmptyCards
        : Phase == PokerItemPhase.RevealingSecondPublic
          ? Array.AsReadOnly(new[] { _firstPublicCard })
          : Array.AsReadOnly(new[] { _firstPublicCard, _source.SecondPublicCard });
      var revealProgress = Phase == PokerItemPhase.RevealingSecondPublic
        ? GetSecondPublicRevealProgress(now ?? _secondPublicRevealStartedAt)
        : 1f;
      return new PokerItemSnapshot(
        Phase,
        _inventory != null ? _inventory.Snapshot() : Array.AsReadOnly(Array.Empty<GameItemId>()),
        _playerCards,
        visibleAi,
        publicCards,
        _bottomDealCandidates,
        LastFailure,
        _stageRestriction?.GetSnapshot(),
        Phase == PokerItemPhase.RevealingSecondPublic ? _source!.SecondPublicCard : (Card?)null,
        revealProgress,
        usePresentation);
    }

    public int VisibleAiCardIndex => _visibleAiCardIndex;

    private bool CanUse(GameItemId itemId, CardId target, out int targetIndex)
    {
      targetIndex = -1;
      if (Phase != PokerItemPhase.AwaitingActions)
      {
        Fail(PokerItemFailure.WrongPhase);
        return false;
      }
      if (!_inventory.Contains(itemId))
      {
        Fail(PokerItemFailure.ItemNotOwned);
        return false;
      }
      if (!CanUseInCurrentStage()) return false;
      targetIndex = Find(_playerCards, target);
      if (targetIndex < 0)
      {
        Fail(PokerItemFailure.InvalidTarget);
        return false;
      }
      return true;
    }

    private bool ValidateCandidatePool(IReadOnlyList<Card> candidates, int required)
    {
      if (candidates.Count < required)
      {
        Fail(PokerItemFailure.CandidatePoolExhausted);
        return false;
      }
      var ids = new HashSet<CardId>();
      for (var index = 0; index < candidates.Count; index++)
      {
        if (!candidates[index].IsValid || !ids.Add(candidates[index].Id))
        {
          Fail(PokerItemFailure.DuplicateCardIdentity);
          return false;
        }
      }
      return true;
    }

    private void Consume(GameItemId id)
    {
      if (!_inventory.TryConsume(id))
      {
        throw new InvalidOperationException("A validated item disappeared before consumption.");
      }
      _stageRestriction?.RecordUse();
      LastFailure = PokerItemFailure.None;
      if (_inventory.Count == 0) Phase = PokerItemPhase.Completed;
    }

    private PokerItemFailure Fail(PokerItemFailure failure)
    {
      LastFailure = failure;
      return failure;
    }

    private bool CanUseInCurrentStage()
    {
      if (_stageRestriction == null || _stageRestriction.CanUse) return true;
      Fail(PokerItemFailure.StageUseLimitReached);
      return false;
    }

    private static int Find(IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == id) return index;
      }
      return -1;
    }

    private static void Replace(List<Card> target, IReadOnlyList<Card> source)
    {
      target.Clear();
      for (var index = 0; index < source.Count; index++) target.Add(source[index]);
    }

    private float GetSecondPublicRevealProgress(GameTimestamp now)
    {
      var duration = _secondPublicRevealEndsAt.Microseconds - _secondPublicRevealStartedAt.Microseconds;
      if (duration <= 0) return 1f;
      var elapsed = Math.Max(0, now.Microseconds - _secondPublicRevealStartedAt.Microseconds);
      return (float)Math.Min(1d, (double)elapsed / duration);
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
