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
    private BattleHealth _health = BattleHealth.Initial;
    private Card? _firstPublicCard;
    private int _combatRoundNumber = 1;

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
      _health = BattleHealth.Initial;
      _combatRoundNumber = 1;
      StartCombatRound(now, combatRoundSeed);
    }

    public void Advance(GameTimestamp now, long nextCombatRoundSeed)
    {
      if (Phase == PlayableGamePhase.Halli)
      {
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

      if (Phase != PlayableGamePhase.PokerResult)
      {
        return;
      }

      if (_health.IsBattleOver)
      {
        Phase = PlayableGamePhase.BattleFinished;
        return;
      }

      _combatRoundNumber++;
      StartCombatRound(now, nextCombatRoundSeed);
    }

    public void Ring(PileSide side, GameTimestamp now)
    {
      if (Phase == PlayableGamePhase.Halli)
      {
        _halli.Ring(side, now);
      }
    }

    public bool TogglePrivateCard(CardId cardId, GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PrivateSelection || _selection == null)
      {
        return false;
      }

      var snapshot = _selection.GetSnapshot(now);
      return snapshot.Winner == HalliStageWinner.Player && _selection.Toggle(cardId);
    }

    public bool ConfirmPrivateCards(GameTimestamp now)
    {
      if (Phase != PlayableGamePhase.PrivateSelection
        || _selection == null
        || !_selection.TryConfirm())
      {
        return false;
      }

      BeginPokerIfReady(now);
      return true;
    }

    public PokerRoundResult Predict(PredictionChoice choice)
    {
      if (Phase != PlayableGamePhase.PokerPrediction || _poker == null)
      {
        throw new InvalidOperationException("Prediction input is unavailable in the current phase.");
      }

      var result = _poker.Resolve(choice);
      _health = result.Damage.After;
      Phase = PlayableGamePhase.PokerResult;
      return result;
    }

    public void Tick(GameTimestamp now)
    {
      if (Phase == PlayableGamePhase.Halli)
      {
        _halli.Tick(now);
      }
      else if (Phase == PlayableGamePhase.PrivateSelection && _selection != null)
      {
        if (_selection.Tick(now))
        {
          BeginPokerIfReady(now);
        }
      }
    }

    public PlayableGameSnapshot GetSnapshot(GameTimestamp now)
    {
      return new PlayableGameSnapshot(
        Phase,
        _combatRoundNumber,
        _health,
        Phase == PlayableGamePhase.Halli ? _halli.GetSnapshot(now) : null,
        Phase == PlayableGamePhase.PrivateSelection && _selection != null
          ? _selection.GetSnapshot(now)
          : null,
        (Phase == PlayableGamePhase.PokerPrediction || Phase == PlayableGamePhase.PokerResult)
          && _poker != null
            ? _poker.GetSnapshot()
            : null);
    }

    private void StartCombatRound(GameTimestamp now, long combatRoundSeed)
    {
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
      if (_selection == null || !_firstPublicCard.HasValue)
      {
        return;
      }

      var snapshot = _selection.GetSnapshot(now);
      if (snapshot.Phase != PrivateCardSelectionPhase.Completed || snapshot.Result == null)
      {
        return;
      }

      _poker = new PokerRoundSession();
      _poker.Begin(_firstPublicCard.Value, snapshot.Result, _health, _pokerRuleSet);
      Phase = PlayableGamePhase.PokerPrediction;
    }
  }
}
