using System;
using System.Collections.Generic;
using CodexGame.Application.Development;
using CodexGame.Application.Distribution;
using CodexGame.Application.Poker;
using CodexGame.Application.Shop;
using CodexGame.Application.Items;
using CodexGame.Core.Ai;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Items;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Core.Shop;

namespace CodexGame.Application.Playable
{
  public sealed class PlayableGameSession
  {
    private readonly AiPrivateCardSelectionPolicy _aiSelectionPolicy;
    private readonly PokerRuleSet _pokerRuleSet;
    private PrototypeHalliSession _halli = new PrototypeHalliSession();
    private PrivateCardSelectionSession? _selection;
    private PokerItemSession? _items;
    private PokerRoundSession? _poker;
    private readonly PlayableTransitionTimeline _transition = new PlayableTransitionTimeline();
    private readonly BarShopSession _barShop = new BarShopSession(BarShopCatalog.All);
    private readonly BarShopPurchaseSession _shopPurchase = new BarShopPurchaseSession();
    private readonly BarShopExitGuard _shopExitGuard = new BarShopExitGuard();
    private readonly NextStageTransitionGate _nextStageGate = new NextStageTransitionGate();
    private BulletLedger _bullets = new BulletLedger();
    private readonly RunInventory _inventory = new RunInventory();
    private readonly PredictionStreak _predictionStreak = new PredictionStreak();
    private readonly DevelopmentCheatHistory _cheatHistory = new DevelopmentCheatHistory();
    private BattleHealth _health = BattleHealth.Initial;
    private Card? _firstPublicCard;
    private GameTimestamp _lastUserInputAt;
    private int _stageNumber = 1;
    private int _combatRoundNumber = 1;
    private int _lastStageReward;
    private StageBulletReward _lastStageRewardDetails = StageBulletReward.None;

    public PlayableGameSession()
      : this(new AiPrivateCardSelectionPolicy(), PokerRuleSet.Development)
    {
    }

    public PlayableGameSession(
      AiPrivateCardSelectionPolicy aiSelectionPolicy,
      PokerRuleSet pokerRuleSet)
    {
      _aiSelectionPolicy = aiSelectionPolicy ?? throw new ArgumentNullException(nameof(aiSelectionPolicy));
      _pokerRuleSet = pokerRuleSet ?? throw new ArgumentNullException(nameof(pokerRuleSet));
    }

    public PlayableGamePhase Phase { get; private set; } = PlayableGamePhase.Intro;

    public void StartNewBattle(GameTimestamp now, long combatRoundSeed)
    {
      _stageNumber = 1;
      _combatRoundNumber = 1;
      _health = BattleHealth.Initial;
      _bullets = new BulletLedger();
      _inventory.Clear();
      _predictionStreak.Reset();
      _cheatHistory.Reset();
      _lastStageReward = 0;
      _lastStageRewardDetails = StageBulletReward.None;
      _barShop.Close();
      _shopPurchase.Reset();
      _shopExitGuard.Reset();
      _nextStageGate.Reset();
      RecordInput(now);
      StartCombatRound(now, combatRoundSeed);
    }

    public void Advance(GameTimestamp now, long nextCombatRoundSeed)
    {
      if (Phase == PlayableGamePhase.Halli)
      {
        RecordInput(now);
        var halliSnapshot = _halli.GetSnapshot(now);
        if (halliSnapshot.Phase == PrototypeSessionPhase.Finished)
        {
          BeginHalliTransition(now);
        }
        else
        {
          _halli.Advance(now);
          if (_halli.GetSnapshot(now).Phase == PrototypeSessionPhase.Finished)
          {
            BeginHalliTransition(now);
          }
        }
        return;
      }

      if (Phase == PlayableGamePhase.PokerResult)
      {
        RecordInput(now);
        if (_health.IsBattleOver)
        {
          if (_health.Ai == 0)
          {
            Phase = PlayableGamePhase.StageWon;
          }
          else
          {
            ResetRunProgress();
            Phase = PlayableGamePhase.BattleFinished;
          }
          return;
        }

        _combatRoundNumber++;
        StartCombatRound(now, nextCombatRoundSeed);
        return;
      }

      if (Phase == PlayableGamePhase.StageWon)
      {
        RecordInput(now);
        _barShop.Begin(nextCombatRoundSeed);
        _shopPurchase.Reset();
        _shopExitGuard.Reset();
        _nextStageGate.Reset();
        Phase = PlayableGamePhase.BarShop;
        return;
      }

      if (Phase == PlayableGamePhase.BarShop)
      {
        if (_shopPurchase.IsInputLocked) return;
        RecordInput(now);
        if (_shopExitGuard.Request(_bullets.TemporaryBalance)
          == BarShopExitRequestResult.WarningArmed)
        {
          return;
        }
        if (!_nextStageGate.TryRequest(nextCombatRoundSeed, now)) return;
        _bullets.ExpireTemporary();
        _transition.Begin(
          PlayableTransitionKind.NextStage,
          now,
          GameRules.NextStageTransitionFixedPreloadMicroseconds);
        Phase = PlayableGamePhase.NextStageTransition;
      }
    }

    public bool MarkNextStageLoadComplete(GameTimestamp now)
    {
      return Phase == PlayableGamePhase.NextStageTransition
        && _nextStageGate.MarkLoadComplete(now);
    }

    public bool RerollBarShop(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.BarShop
        || _shopPurchase.IsInputLocked
        || !_barShop.TryReroll()) return false;
      _shopExitGuard.Reset();
      RecordInput(now);
      return true;
    }

    public BarShopPurchaseFailure PurchaseBarShopSlot(int slotIndex, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.BarShop)
      {
        return BarShopPurchaseFailure.InvalidSlot;
      }
      if (!_barShop.TryGetSlot(slotIndex, out var product))
      {
        return BarShopPurchaseFailure.InvalidSlot;
      }
      _shopExitGuard.Reset();
      var result = _shopPurchase.TryBegin(product!, _bullets, _inventory, now);
      if (result == BarShopPurchaseFailure.None)
      {
        RecordInput(now);
      }
      return result;
    }

    public void Ring(PileSide side, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.Halli) return;
      var before = _halli.GetSnapshot(now);
      if (before.CanRing) RecordInput(now);
      _halli.Ring(side, now);
    }

    public bool TogglePrivateCard(CardId cardId, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PrivateSelection || _selection == null) return false;
      var snapshot = _selection.GetSnapshot(now);
      if (snapshot.Winner != HalliStageWinner.Player) return false;
      var changed = _selection.Toggle(cardId);
      if (changed) RecordInput(now);
      return changed;
    }

    public bool ConfirmPrivateCards(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PrivateSelection
        || _selection == null
        || !_selection.TryConfirm())
      {
        return false;
      }

      RecordInput(now);
      BeginPokerIfReady(now);
      return true;
    }

    public bool Predict(PredictionChoice choice, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerPrediction
        || _poker == null
        || !_poker.SubmitPrediction(choice, now))
      {
        return false;
      }

      RecordInput(now);
      return true;
    }

    public PokerItemFailure UseReload(CardId target, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerItems || _items == null)
      {
        return PokerItemFailure.WrongPhase;
      }
      var result = _items.UseReload(target);
      if (result == PokerItemFailure.None) RecordInput(now);
      CompleteItemWindowIfReady(now);
      return result;
    }

    public PokerItemFailure BeginBottomDeal(CardId target, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerItems || _items == null)
      {
        return PokerItemFailure.WrongPhase;
      }
      var result = _items.BeginBottomDeal(target);
      if (result == PokerItemFailure.None) RecordInput(now);
      return result;
    }

    public PokerItemFailure ChooseBottomDeal(CardId candidate, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerItems || _items == null)
      {
        return PokerItemFailure.WrongPhase;
      }
      var result = _items.ChooseBottomDeal(candidate);
      if (result == PokerItemFailure.None) RecordInput(now);
      CompleteItemWindowIfReady(now);
      return result;
    }

    public PokerItemFailure UseHypeMan(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerItems || _items == null)
      {
        return PokerItemFailure.WrongPhase;
      }
      var result = _items.UseHypeMan();
      if (result == PokerItemFailure.None) RecordInput(now);
      CompleteItemWindowIfReady(now);
      return result;
    }

    public PokerItemFailure UseHealthRecovery(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerItems || _items == null)
      {
        return PokerItemFailure.WrongPhase;
      }
      var result = _items.UseHealthRecovery(_health.Player < GameRules.StartingHealth);
      if (result == PokerItemFailure.None)
      {
        _health = new BattleHealth(Math.Min(GameRules.StartingHealth, _health.Player + 1), _health.Ai);
        RecordInput(now);
      }
      CompleteItemWindowIfReady(now);
      return result;
    }

    public bool ConfirmItems(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PokerItems || _items == null || !_items.Confirm())
      {
        return false;
      }
      RecordInput(now);
      BeginPokerFromItems(now);
      return true;
    }

    public bool ReturnToMain()
    {
      if (Phase != PlayableGamePhase.BattleFinished) return false;
      ResetToMain();
      return true;
    }

    public void Tick(GameTimestamp now)
    {
      if (IsActiveBattlePhase(Phase)
        && now.Microseconds - _lastUserInputAt.Microseconds
          >= GameRules.GlobalInactivityTimeoutMicroseconds)
      {
        ResetToMain();
        return;
      }

      if (Phase == PlayableGamePhase.HalliOpening)
      {
        if (_transition.IsComplete(now))
        {
          _transition.Clear();
          _halli.CompleteOpening(now);
          Phase = PlayableGamePhase.Halli;
          RecordInput(now);
        }
      }
      else if (Phase == PlayableGamePhase.Halli)
      {
        _halli.Tick(now);
        if (_halli.GetSnapshot(now).Phase == PrototypeSessionPhase.Finished)
        {
          BeginHalliTransition(now);
        }
      }
      else if (Phase == PlayableGamePhase.HalliTransition)
      {
        if (_transition.IsComplete(now))
        {
          var halliSnapshot = _halli.GetSnapshot(now);
          _transition.Clear();
          BeginPrivateSelection(now, halliSnapshot);
        }
      }
      else if (Phase == PlayableGamePhase.PrivateSelection && _selection != null)
      {
        if (_selection.Tick(now)) BeginPokerIfReady(now);
      }
      else if (Phase == PlayableGamePhase.PokerPrediction
        && _poker != null
        && _poker.Tick(now))
      {
        CompletePokerRound();
      }
      else if (Phase == PlayableGamePhase.BarShop)
      {
        _shopPurchase.Tick(now, _bullets, _inventory);
      }
      else if (Phase == PlayableGamePhase.NextStageTransition
        && _nextStageGate.IsComplete(now))
      {
        CompleteNextStageTransition(now);
      }
    }

    public PlayableGameSnapshot GetSnapshot(GameTimestamp now)
    {
      var inactivityRemaining = IsActiveBattlePhase(Phase)
        ? Math.Max(
          0,
          GameRules.GlobalInactivityTimeoutMicroseconds
            - (now.Microseconds - _lastUserInputAt.Microseconds))
        : 0;
      return new PlayableGameSnapshot(
        Phase,
        _stageNumber,
        _combatRoundNumber,
        _health,
        _bullets.BaseBalance,
        _bullets.TemporaryBalance,
        _lastStageReward,
        _lastStageRewardDetails.BaseBullets,
        _lastStageRewardDetails.BonusBullets,
        _predictionStreak.SuccessCount,
        _inventory.Snapshot(),
        _cheatHistory.CheatUsed,
        _cheatHistory.Snapshot(),
        inactivityRemaining,
        _transition.GetSnapshot(now),
        Phase == PlayableGamePhase.NextStageTransition
          ? _nextStageGate.GetSnapshot(now)
          : null,
        Phase == PlayableGamePhase.HalliOpening
          || Phase == PlayableGamePhase.Halli
          || Phase == PlayableGamePhase.HalliTransition
            ? _halli.GetSnapshot(now)
            : null,
        Phase == PlayableGamePhase.PrivateSelection && _selection != null
          ? _selection.GetSnapshot(now)
          : null,
        Phase == PlayableGamePhase.PokerItems && _items != null
          ? _items.GetSnapshot()
          : null,
        (Phase == PlayableGamePhase.PokerPrediction
          || Phase == PlayableGamePhase.PokerResult)
          && _poker != null
            ? _poker.GetSnapshot(now)
            : null,
        Phase == PlayableGamePhase.BarShop
          ? _barShop.GetSnapshot(
            _shopPurchase.GetSnapshot(now),
            _shopExitGuard.WarningArmed)
          : null);
    }

    private void StartCombatRound(GameTimestamp now, long combatRoundSeed)
    {
      _halli = new PrototypeHalliSession();
      _selection = null;
      _items = null;
      _poker = null;
      _firstPublicCard = null;
      _halli.StartNew(now, combatRoundSeed, _combatRoundNumber, true);
      _transition.Begin(
        PlayableTransitionKind.HalliOpening,
        now,
        GameRules.HalliOpeningPresentationMicroseconds);
      Phase = PlayableGamePhase.HalliOpening;
    }

    private void BeginHalliTransition(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.Halli) return;
      _transition.Begin(
        PlayableTransitionKind.HalliToPoker,
        now,
        GameRules.HalliClosingPresentationMicroseconds);
      Phase = PlayableGamePhase.HalliTransition;
    }

    private void BeginPrivateSelection(
      GameTimestamp now,
      PrototypeHalliSnapshot halliSnapshot)
    {
      if (!halliSnapshot.FirstPublicCard.HasValue)
      {
        throw new InvalidOperationException("Halli stage has no first public card.");
      }

      _firstPublicCard = halliSnapshot.FirstPublicCard.Value;
      _selection = _halli.BeginPrivateCardDistribution(
        now,
        PrivateCardDistributionRules.IsPairAssistEnabled(_health));
      Phase = PlayableGamePhase.PrivateSelection;
      var selectionSnapshot = _selection.GetSnapshot(now);

      if (selectionSnapshot.Phase == PrivateCardSelectionPhase.AwaitingSelection
        && selectionSnapshot.Winner == HalliStageWinner.Ai)
      {
        var random = DeterministicRandomFactory.Create(
          halliSnapshot.CombatRoundSeed,
          RandomChannel.AiChoice);
        var selected = _aiSelectionPolicy.Select(
          selectionSnapshot.WinnerCandidates,
          selectionSnapshot.RequiredSelectionCount,
          random);
        for (var index = 0; index < selected.Count; index++)
        {
          if (!_selection.Toggle(selected[index]))
          {
            throw new InvalidOperationException("AI selection policy returned an invalid card.");
          }
        }

        if (!_selection.TryConfirm())
        {
          throw new InvalidOperationException("AI private-card selection could not be confirmed.");
        }
      }

      BeginPokerIfReady(now);
    }

    private void BeginPokerIfReady(GameTimestamp now)
    {
      if (_selection == null || !_firstPublicCard.HasValue) return;
      var snapshot = _selection.GetSnapshot(now);
      if (snapshot.Phase != PrivateCardSelectionPhase.Completed || snapshot.Result == null) return;

      _items = new PokerItemSession();
      _items.Begin(
        _firstPublicCard.Value,
        snapshot.Result,
        _inventory,
        _halli.GetSnapshot(now).CombatRoundSeed);
      if (_items.Phase == PokerItemPhase.Completed)
      {
        BeginPokerFromItems(now);
      }
      else
      {
        Phase = PlayableGamePhase.PokerItems;
      }
    }

    private void CompletePokerRound()
    {
      if (_poker == null || _poker.Result == null)
      {
        throw new InvalidOperationException("Resolved poker session has no result.");
      }

      _health = _poker.Result.Damage.After;
      _predictionStreak.Record(_poker.Result.Prediction);
      if (_health.Ai == 0) SettleStageVictory();
      else
      {
        _lastStageRewardDetails = StageBulletReward.None;
        _lastStageReward = 0;
      }
      Phase = PlayableGamePhase.PokerResult;
    }

    private void CompleteNextStageTransition(GameTimestamp now)
    {
      if (!_nextStageGate.TryConsume(now, out var nextStageSeed)) return;
      _transition.Clear();
      _barShop.Close();
      _shopPurchase.Reset();
      _shopExitGuard.Reset();
      _stageNumber++;
      _combatRoundNumber = 1;
      _health = NextStageHealthResolver.RestoreAfterVictory(_health);
      _lastStageReward = 0;
      _lastStageRewardDetails = StageBulletReward.None;
      StartCombatRound(now, nextStageSeed);
    }

    private void RecordInput(GameTimestamp now)
    {
      _lastUserInputAt = now;
    }

    private void ResetToMain()
    {
      _health = BattleHealth.Initial;
      _halli = new PrototypeHalliSession();
      _selection = null;
      _items = null;
      _poker = null;
      _firstPublicCard = null;
      _transition.Clear();
      ResetRunProgress();
      _barShop.Close();
      _shopPurchase.Reset();
      _shopExitGuard.Reset();
      _nextStageGate.Reset();
      _stageNumber = 1;
      _combatRoundNumber = 1;
      Phase = PlayableGamePhase.Intro;
    }

    private static bool IsActiveBattlePhase(PlayableGamePhase phase)
    {
      return phase == PlayableGamePhase.Halli
        || phase == PlayableGamePhase.PrivateSelection
        || phase == PlayableGamePhase.PokerItems
        || phase == PlayableGamePhase.PokerPrediction
        || phase == PlayableGamePhase.PokerResult;
    }

    private void CompleteItemWindowIfReady(GameTimestamp now)
    {
      if (_items != null && _items.Phase == PokerItemPhase.Completed)
      {
        BeginPokerFromItems(now);
      }
    }

    private void BeginPokerFromItems(GameTimestamp now)
    {
      if (_items == null || !_firstPublicCard.HasValue) return;
      var itemResult = _items.GetResult();
      _poker = new PokerRoundSession();
      _poker.Begin(
        _firstPublicCard.Value,
        itemResult,
        _health,
        _pokerRuleSet,
        now,
        _items.VisibleAiCardIndex);
      Phase = PlayableGamePhase.PokerPrediction;
    }

    private void ResetRunProgress()
    {
      _bullets = new BulletLedger();
      _inventory.Clear();
      _predictionStreak.Reset();
      _cheatHistory.Reset();
      _shopExitGuard.Reset();
      _lastStageReward = 0;
      _lastStageRewardDetails = StageBulletReward.None;
    }

    private void SettleStageVictory()
    {
      _lastStageRewardDetails = _bullets.SettleStageVictory(
        _stageNumber,
        _health.Player,
        _predictionStreak.SuccessCount);
      _lastStageReward = _lastStageRewardDetails.TotalBullets;
    }

#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
    public bool CheatCompleteStage(GameTimestamp now)
    {
      if (!IsActiveBattlePhase(Phase) || _health.IsBattleOver)
      {
        _cheatHistory.Record(now.Microseconds, "stage-pass", string.Empty, "rejected:inactive");
        return false;
      }
      _health = new BattleHealth(_health.Player, 0);
      SettleStageVictory();
      _selection = null;
      _items = null;
      _poker = null;
      Phase = PlayableGamePhase.StageWon;
      _cheatHistory.Record(now.Microseconds, "stage-pass", string.Empty, "ok");
      return true;
    }

    public InventoryAddResult CheatGrantItem(GameItemId itemId, GameTimestamp now)
    {
      var result = _inventory.TryAdd(itemId);
      _cheatHistory.Record(
        now.Microseconds,
        "grant-item",
        itemId.ToString(),
        result.ToString());
      return result;
    }

    public bool CheatSetPokerCards(
      IReadOnlyList<Card> playerCards,
      IReadOnlyList<Card> aiCards,
      IReadOnlyList<Card> publicCards,
      GameTimestamp now)
    {
      if (!IsActiveBattlePhase(Phase)
        || playerCards == null
        || aiCards == null
        || publicCards == null
        || playerCards.Count != 3
        || aiCards.Count != 3
        || publicCards.Count != 2)
      {
        _cheatHistory.Record(now.Microseconds, "set-poker", "8 cards", "rejected:shape");
        return false;
      }

      try
      {
        PokerComparer.Compare(playerCards, aiCards, publicCards, _pokerRuleSet);
        var distribution = new CodexGame.Core.Distribution.PrivateCardDistributionResult(
          HalliStageWinner.Player,
          _combatRoundNumber,
          playerCards,
          aiCards,
          publicCards[1],
          Array.AsReadOnly(Array.Empty<Card>()));
        _firstPublicCard = publicCards[0];
        _selection = null;
        _items = null;
        _poker = new PokerRoundSession();
        _poker.Begin(publicCards[0], distribution, _health, _pokerRuleSet, now);
        Phase = PlayableGamePhase.PokerPrediction;
        _cheatHistory.Record(now.Microseconds, "set-poker", "8 cards", "ok");
        return true;
      }
      catch (Exception exception) when (
        exception is ArgumentException || exception is InvalidOperationException)
      {
        _cheatHistory.Record(now.Microseconds, "set-poker", "8 cards", "rejected:cards");
        return false;
      }
    }
#endif
  }
}
