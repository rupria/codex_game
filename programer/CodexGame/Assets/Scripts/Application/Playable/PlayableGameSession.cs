using System;
using CodexGame.Application.Distribution;
using CodexGame.Application.Poker;
using CodexGame.Core.Ai;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class PlayableGameSession
  {
    private readonly AiPrivateCardSelectionPolicy _aiSelectionPolicy;
    private readonly PokerRuleSet _pokerRuleSet;
    private PrototypeHalliSession _halli = new PrototypeHalliSession();
    private PrivateCardSelectionSession? _selection;
    private PokerRoundSession? _poker;
    private PredictionRewardLedger _rewards = new PredictionRewardLedger();
    private IRandomSource? _rewardRandom;
    private BattleHealth _health = BattleHealth.Initial;
    private Card? _firstPublicCard;
    private GameTimestamp _lastUserInputAt;
    private long _combatRoundSeed;
    private int _stageNumber = 1;
    private int _combatRoundNumber = 1;
    private int _lastStageBaseCoinReward;
    private PredictionRewardKind _lastPredictionReward;

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
      _rewards = new PredictionRewardLedger();
      _rewardRandom = DeterministicRandomFactory.Create(combatRoundSeed, RandomChannel.Reward);
      _lastStageBaseCoinReward = 0;
      _lastPredictionReward = PredictionRewardKind.None;
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
          BeginPrivateSelection(now, halliSnapshot);
        }
        else
        {
          _halli.Advance(now);
        }
        return;
      }

      if (Phase == PlayableGamePhase.PokerItemWindow && _poker != null)
      {
        RecordInput(now);
        if (_poker.SkipItemWindow(now)) Phase = PlayableGamePhase.PokerPrediction;
        return;
      }

      if (Phase == PlayableGamePhase.PokerResult)
      {
        RecordInput(now);
        if (_health.IsBattleOver)
        {
          if (_health.Ai == 0)
          {
            _lastStageBaseCoinReward = _health.Player;
            Phase = PlayableGamePhase.StageWon;
          }
          else
          {
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
        _stageNumber++;
        _combatRoundNumber = 1;
        _health = BattleHealth.Initial;
        StartCombatRound(now, nextCombatRoundSeed);
      }
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

    public bool SelectWrongBellReward(CardId cardId, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.Halli) return false;
      var selected = _halli.SelectWrongBellReward(cardId, now);
      if (selected) RecordInput(now);
      return selected;
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

      if (Phase == PlayableGamePhase.Halli)
      {
        _halli.Tick(now);
      }
      else if (Phase == PlayableGamePhase.PrivateSelection && _selection != null)
      {
        if (_selection.Tick(now)) BeginPokerIfReady(now);
      }
      else if ((Phase == PlayableGamePhase.PokerItemWindow
          || Phase == PlayableGamePhase.PokerPrediction)
        && _poker != null
        && _poker.Tick(now))
      {
        CompletePokerRound();
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
        _rewards.ItemRewardCount,
        _rewards.CoinIncreaseEventCount,
        _lastStageBaseCoinReward,
        _lastPredictionReward,
        inactivityRemaining,
        Phase == PlayableGamePhase.Halli ? _halli.GetSnapshot(now) : null,
        Phase == PlayableGamePhase.PrivateSelection && _selection != null
          ? _selection.GetSnapshot(now)
          : null,
        (Phase == PlayableGamePhase.PokerItemWindow
          || Phase == PlayableGamePhase.PokerPrediction
          || Phase == PlayableGamePhase.PokerResult)
          && _poker != null
            ? _poker.GetSnapshot(now)
            : null);
    }

    private void StartCombatRound(GameTimestamp now, long combatRoundSeed)
    {
      _combatRoundSeed = combatRoundSeed;
      _halli = new PrototypeHalliSession();
      _selection = null;
      _poker = null;
      _firstPublicCard = null;
      _halli.StartNew(now, combatRoundSeed, _combatRoundNumber);
      Phase = PlayableGamePhase.Halli;
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
      _selection = _halli.BeginPrivateCardDistribution(now);
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

      _poker = new PokerRoundSession();
      _poker.Begin(
        _firstPublicCard.Value,
        snapshot.Result,
        _health,
        _pokerRuleSet,
        now,
        _rewards.ItemRewardCount);
      Phase = _poker.Phase == PokerRoundPhase.ItemWindow
        ? PlayableGamePhase.PokerItemWindow
        : PlayableGamePhase.PokerPrediction;
    }

    private void CompletePokerRound()
    {
      if (_poker == null || _poker.Result == null)
      {
        throw new InvalidOperationException("Resolved poker session has no result.");
      }

      _health = _poker.Result.Damage.After;
      _lastPredictionReward = PredictionRewardKind.None;
      if (_poker.Result.Prediction.IsCorrect)
      {
        if (_rewardRandom == null)
        {
          _rewardRandom = DeterministicRandomFactory.Create(_combatRoundSeed, RandomChannel.Reward);
        }
        _lastPredictionReward = _rewards.Award(_rewardRandom);
      }
      Phase = PlayableGamePhase.PokerResult;
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
      _poker = null;
      _firstPublicCard = null;
      _rewards = new PredictionRewardLedger();
      _stageNumber = 1;
      _combatRoundNumber = 1;
      _lastStageBaseCoinReward = 0;
      _lastPredictionReward = PredictionRewardKind.None;
      Phase = PlayableGamePhase.Intro;
    }

    private static bool IsActiveBattlePhase(PlayableGamePhase phase)
    {
      return phase == PlayableGamePhase.Halli
        || phase == PlayableGamePhase.PrivateSelection
        || phase == PlayableGamePhase.PokerItemWindow
        || phase == PlayableGamePhase.PokerPrediction
        || phase == PlayableGamePhase.PokerResult;
    }
  }
}
