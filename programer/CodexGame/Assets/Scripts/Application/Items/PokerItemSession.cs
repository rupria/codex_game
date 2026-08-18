#nullable enable
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
    private readonly List<BottomDealAuditEntry> _bottomDealAuditTrail =
      new List<BottomDealAuditEntry>();
    private RunInventory _inventory = null!;
    private PrivateCardDistributionResult _source = null!;
    private IRandomSource _random = null!;
    private IRandomSource _mercenaryRandom = null!;
    private StageItemRestrictionSession? _stageRestriction;
    private CardId _bottomDealTarget;
    private Card _firstPublicCard;
    private int _visibleAiCardIndex = -1;
    private GameTimestamp _handConfirmationDeadline;
    private long _combatRoundSeed;
    private CardId? _wildInkCardId;
    private bool _barrelDefenseArmed;
    private bool _insuranceActivated;
    private bool _mercenaryExchangeApplied;
    private bool _handConfirmationTimedOut;
    private bool _canRecoverHealth;
    private GameItemUseTiming _currentUseTiming = GameItemUseTiming.None;

    public PokerItemPhase Phase { get; private set; } = PokerItemPhase.NotStarted;
    public PokerItemFailure LastFailure { get; private set; }

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long combatRoundSeed,
      StageItemRestrictionSession? stageRestriction = null,
      bool canRecoverHealth = true)
    {
      BeginCore(
        firstPublicCard,
        distribution,
        inventory,
        combatRoundSeed,
        stageRestriction,
        new GameTimestamp(0),
        canRecoverHealth);
    }

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long combatRoundSeed,
      GameTimestamp now,
      StageItemRestrictionSession? stageRestriction = null,
      bool canRecoverHealth = true)
    {
      BeginCore(
        firstPublicCard,
        distribution,
        inventory,
        combatRoundSeed,
        stageRestriction,
        now,
        canRecoverHealth);
    }

    private void BeginCore(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      RunInventory inventory,
      long combatRoundSeed,
      StageItemRestrictionSession? stageRestriction,
      GameTimestamp now,
      bool canRecoverHealth)
    {
      if (!firstPublicCard.IsValid)
      {
        throw new ArgumentException("The first public card is invalid.", nameof(firstPublicCard));
      }
      _source = distribution ?? throw new ArgumentNullException(nameof(distribution));
      _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
      _random = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.ItemUse);
      _mercenaryRandom = DeterministicRandomFactory.Create(
        combatRoundSeed,
        RandomChannel.MercenaryExchange);
      _stageRestriction = stageRestriction;
      _combatRoundSeed = combatRoundSeed;
      _firstPublicCard = firstPublicCard;
      Replace(_playerCards, distribution.PlayerPrivateCards);
      Replace(_aiCards, distribution.AiPrivateCards);
      Replace(_remainingCandidates, distribution.RemainingCandidates);
      _bottomDealCandidates.Clear();
      _bottomDealAuditTrail.Clear();
      _visibleAiCardIndex = -1;
      _wildInkCardId = null;
      _barrelDefenseArmed = false;
      _insuranceActivated = false;
      _mercenaryExchangeApplied = false;
      _handConfirmationTimedOut = false;
      _canRecoverHealth = canRecoverHealth;
      _currentUseTiming = GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction;
      LastFailure = PokerItemFailure.None;
      Phase = inventory.Count == 0
        ? PokerItemPhase.Completed
        : PokerItemPhase.AwaitingActions;
      if (Phase == PokerItemPhase.AwaitingActions)
      {
        _handConfirmationDeadline = Add(
          now,
          GameRules.PokerHandConfirmationTimeoutMicroseconds);
      }
    }

    public bool Tick(GameTimestamp now)
    {
      if ((Phase == PokerItemPhase.AwaitingActions
          || Phase == PokerItemPhase.AwaitingBottomDealChoice)
        && now.Microseconds >= _handConfirmationDeadline.Microseconds)
      {
        if (Phase == PokerItemPhase.AwaitingBottomDealChoice)
        {
          RecordBottomDealAudit(
            _handConfirmationDeadline,
            BottomDealAuditOutcome.TimedOut);
          _bottomDealCandidates.Clear();
        }
        _handConfirmationTimedOut = true;
        Phase = PokerItemPhase.Completed;
        LastFailure = PokerItemFailure.None;
        return true;
      }
      return false;
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

    public PokerItemFailure BeginBottomDeal(CardId target, GameTimestamp now)
    {
      if (!CanUse(GameItemId.BottomDeal, target, out var targetIndex)) return LastFailure;
      var candidates = new List<Card>(_remainingCandidates) { _playerCards[targetIndex] };
      if (!ValidateCandidatePool(candidates, 2)) return LastFailure;

      if (_bottomDealCandidates.Count != 2 || _bottomDealTarget != target)
      {
        _bottomDealCandidates.Clear();
        var previewRandom = CreateBottomDealPreviewRandom(target);
        for (var count = 0; count < 2; count++)
        {
          var selectedIndex = previewRandom.NextInt(candidates.Count);
          _bottomDealCandidates.Add(candidates[selectedIndex]);
          candidates.RemoveAt(selectedIndex);
        }
      }
      _bottomDealTarget = target;
      LastFailure = PokerItemFailure.None;
      Phase = PokerItemPhase.AwaitingBottomDealChoice;
      RecordBottomDealAudit(now, BottomDealAuditOutcome.Entered);
      return LastFailure;
    }

    public PokerItemFailure ChooseBottomDeal(CardId candidateId, GameTimestamp now)
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
      RecordBottomDealAudit(now, BottomDealAuditOutcome.Confirmed);
      _bottomDealCandidates.Clear();
      Consume(GameItemId.BottomDeal);
      if (Phase != PokerItemPhase.Completed) Phase = PokerItemPhase.AwaitingActions;
      return LastFailure;
    }

    public bool CancelBottomDeal(GameTimestamp now)
    {
      // Q-028: the confirmation popup is the final cancellable boundary.
      // Target/candidate selection keeps the original deadline and cannot be closed.
      return false;
    }

    public PokerItemFailure UseHypeMan()
    {
      if (!CanUseWithoutTarget(GameItemId.HypeMan)) return LastFailure;
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
      if (!CanUseWithoutTarget(GameItemId.HealthRecovery)) return LastFailure;
      if (!canRecover) return Fail(PokerItemFailure.HealthAlreadyFull);
      Consume(GameItemId.HealthRecovery);
      return LastFailure;
    }

    public PokerItemFailure UseWildInk(CardId target, CardSuit effectiveSuit)
    {
      if (!CanUse(GameItemId.WildInk, target, out var targetIndex)) return LastFailure;
      if (_playerCards[targetIndex].IsJoker) return Fail(PokerItemFailure.InvalidTarget);
      if (!Enum.IsDefined(typeof(CardSuit), effectiveSuit)
        || _playerCards[targetIndex].EffectiveSuit == effectiveSuit)
      {
        return Fail(PokerItemFailure.InvalidSuit);
      }
      _playerCards[targetIndex] = _playerCards[targetIndex].WithEffectiveSuit(effectiveSuit);
      _wildInkCardId = target;
      Consume(GameItemId.WildInk);
      return LastFailure;
    }

    public PokerItemFailure UseBarrel()
    {
      if (!CanUseWithoutTarget(GameItemId.Barrel)) return LastFailure;
      if (_barrelDefenseArmed) return Fail(PokerItemFailure.EffectAlreadyActive);
      _barrelDefenseArmed = true;
      Consume(GameItemId.Barrel);
      return LastFailure;
    }

    public PokerItemFailure UsePredictionInsurance(bool canActivate)
    {
      if (!CanUseWithoutTarget(GameItemId.PredictionInsurance)) return LastFailure;
      if (!canActivate || _insuranceActivated)
      {
        return Fail(PokerItemFailure.EffectAlreadyActive);
      }
      _insuranceActivated = true;
      Consume(GameItemId.PredictionInsurance);
      return LastFailure;
    }

    public PokerItemFailure UseMercenary(CardId playerTarget)
    {
      if (!CanUse(GameItemId.Mercenary, playerTarget, out _)) return LastFailure;
      var publicCards = Array.AsReadOnly(new[] { _firstPublicCard, _source.SecondPublicCard });
      if (!MercenaryExchangeResolver.TryResolve(
        _playerCards,
        _aiCards,
        publicCards,
        _remainingCandidates,
        playerTarget,
        _mercenaryRandom,
        out var result))
      {
        return Fail(result.Failure == MercenaryExchangeFailure.InvalidPlayerTarget
          ? PokerItemFailure.InvalidTarget
          : PokerItemFailure.NoValidReplacementPair);
      }
      Replace(_playerCards, result.PlayerCards);
      Replace(_aiCards, result.AiCards);
      Replace(_remainingCandidates, result.RemainingCandidates);
      _visibleAiCardIndex = -1;
      _mercenaryExchangeApplied = true;
      Consume(GameItemId.Mercenary);
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
        : Array.AsReadOnly(new[] { _firstPublicCard, _source.SecondPublicCard });
      var handConfirmationRemaining = GetHandConfirmationRemaining(
        now ?? _handConfirmationDeadline);
      var mercenaryTargets = new List<CardId>();
      if (_source != null
        && _inventory != null
        && _inventory.Contains(GameItemId.Mercenary)
        && !_wildInkCardId.HasValue)
      {
        var lockedPublicCards = Array.AsReadOnly(
          new[] { _firstPublicCard, _source.SecondPublicCard });
        for (var index = 0; index < _playerCards.Count; index++)
        {
          if (MercenaryExchangeResolver.CanResolve(
            _playerCards,
            _aiCards,
            lockedPublicCards,
            _remainingCandidates,
            _playerCards[index].Id))
          {
            mercenaryTargets.Add(_playerCards[index].Id);
          }
        }
      }
      return new PokerItemSnapshot(
        Phase,
        _currentUseTiming,
        _inventory != null ? _inventory.Snapshot() : Array.AsReadOnly(Array.Empty<GameItemId>()),
        _playerCards,
        visibleAi,
        publicCards,
        _bottomDealCandidates,
        LastFailure,
        _stageRestriction?.GetSnapshot(),
        usePresentation,
        _wildInkCardId,
        _barrelDefenseArmed,
        _insuranceActivated,
        _mercenaryExchangeApplied,
        handConfirmationRemaining,
        _handConfirmationTimedOut,
        Array.AsReadOnly(mercenaryTargets.ToArray()),
        _canRecoverHealth,
        Array.AsReadOnly(_bottomDealAuditTrail.ToArray()));
    }

    public int VisibleAiCardIndex => _visibleAiCardIndex;
    public bool BarrelDefenseArmed => _barrelDefenseArmed;
    public bool HandConfirmationTimedOut => _handConfirmationTimedOut;

    private bool CanUse(GameItemId itemId, CardId target, out int targetIndex)
    {
      targetIndex = -1;
      if (!CanUseAtCurrentTiming(itemId))
      {
        return false;
      }
      if (!_inventory.Contains(itemId))
      {
        Fail(PokerItemFailure.ItemNotOwned);
        return false;
      }
      if (_wildInkCardId.HasValue && IsCardExchangeItem(itemId))
      {
        Fail(PokerItemFailure.CardExchangeLocked);
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

    private bool CanUseWithoutTarget(GameItemId itemId)
    {
      if (!CanUseAtCurrentTiming(itemId))
      {
        return false;
      }
      if (!_inventory.Contains(itemId))
      {
        Fail(PokerItemFailure.ItemNotOwned);
        return false;
      }
      return CanUseInCurrentStage();
    }

    private bool CanUseAtCurrentTiming(GameItemId itemId)
    {
      if (Phase != PokerItemPhase.AwaitingActions
        || !GameItemCatalog.TryGet(itemId, out var definition)
        || definition == null
        || !GameItemUseTimingPolicy.IsUsable(definition, _currentUseTiming))
      {
        Fail(PokerItemFailure.WrongPhase);
        return false;
      }
      return true;
    }

    private static bool IsCardExchangeItem(GameItemId itemId)
    {
      return itemId == GameItemId.Reload
        || itemId == GameItemId.BottomDeal
        || itemId == GameItemId.Mercenary;
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

    private IRandomSource CreateBottomDealPreviewRandom(CardId target)
    {
      var targetSalt = unchecked(
        (long)((ulong)(target.Value + 1) * 0x9E3779B97F4A7C15UL));
      return DeterministicRandomFactory.Create(
        unchecked(_combatRoundSeed ^ targetSalt),
        RandomChannel.BottomDealPreview);
    }

    private long GetHandConfirmationRemaining(GameTimestamp now)
    {
      return Phase == PokerItemPhase.AwaitingActions
          || Phase == PokerItemPhase.AwaitingBottomDealChoice
        ? Math.Max(0, _handConfirmationDeadline.Microseconds - now.Microseconds)
        : 0;
    }

    private void RecordBottomDealAudit(
      GameTimestamp now,
      BottomDealAuditOutcome outcome)
    {
      _bottomDealAuditTrail.Add(new BottomDealAuditEntry(
        now.Microseconds,
        GetHandConfirmationRemaining(now),
        _bottomDealTarget,
        outcome));
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
