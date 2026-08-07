using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Rewards;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  public sealed class PlayableDevView : MonoBehaviour
  {
    [SerializeField]
    private Texture2D _boardTexture;

    [SerializeField]
    private PlayableCardArtSet _cardArtSet;

    private readonly HalliDevPanel _halliPanel = new HalliDevPanel();
    private readonly PrivateSelectionDevPanel _selectionPanel = new PrivateSelectionDevPanel();
    private readonly PokerDevPanel _pokerPanel = new PokerDevPanel();
    private PlayableGameSnapshot _snapshot;
    private PlayableDevStyles _styles;
    private PlayableCardRenderer _halliCards;
    private PlayableCardRenderer _pokerCards;
    private int _selectionFocus;

    public event Action StartRequested;
    public event Action AdvanceRequested;
    public event Action LeftBellRequested;
    public event Action RightBellRequested;
    public event Action<CardId> WrongBellRewardRequested;
    public event Action<CardId> PrivateCardToggleRequested;
    public event Action PrivateCardsConfirmRequested;
    public event Action<PredictionChoice> PredictionRequested;
    public event Action MainRequested;

    public void Configure(Texture2D boardTexture, PlayableCardArtSet cardArtSet)
    {
      _boardTexture = boardTexture;
      _cardArtSet = cardArtSet;
      _halliCards = null;
      _pokerCards = null;
    }

    public void Present(PlayableGameSnapshot snapshot)
    {
      if (_snapshot == null || _snapshot.Phase != snapshot.Phase)
      {
        _selectionFocus = 0;
      }

      _snapshot = snapshot;
    }

    private void Update()
    {
      if (_snapshot == null) return;

      switch (_snapshot.Phase)
      {
        case PlayableGamePhase.Intro:
          if (Pressed(KeyCode.Return, KeyCode.Space)) StartRequested?.Invoke();
          break;
        case PlayableGamePhase.Halli:
          HandleHalliInput();
          break;
        case PlayableGamePhase.PrivateSelection:
          HandleSelectionInput();
          break;
        case PlayableGamePhase.PokerItemWindow:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
        case PlayableGamePhase.PokerPrediction:
          if (_snapshot.Poker != null
            && _snapshot.Poker.Phase == Application.Poker.PokerRoundPhase.AwaitingPrediction)
          {
            if (Input.GetKeyDown(KeyCode.Alpha1)) PredictionRequested?.Invoke(PredictionChoice.PlayerWins);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) PredictionRequested?.Invoke(PredictionChoice.PlayerLoses);
          }
          break;
        case PlayableGamePhase.PokerResult:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
        case PlayableGamePhase.BattleFinished:
          if (Pressed(KeyCode.R, KeyCode.Return)) MainRequested?.Invoke();
          break;
        case PlayableGamePhase.StageWon:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
      }
    }

    private void OnGUI()
    {
      if (_snapshot == null) return;
      EnsureRenderers();
      var scale = Mathf.Min(Screen.width / 960f, Screen.height / 600f);
      GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
      if (_boardTexture != null)
      {
        GUI.DrawTexture(new Rect(0f, 0f, 960f, 540f), _boardTexture, ScaleMode.StretchToFill, true);
      }

      GUILayout.BeginArea(new Rect(18f, 12f, 924f, 578f));
      GUILayout.Label("CODEX GAME 0.08 - HALLI TO POKER DEV BUILD", _styles.Title);
      GUILayout.BeginHorizontal();
      GUILayout.Label("STAGE " + _snapshot.StageNumber, _styles.Heading);
      GUILayout.Label("COMBAT ROUND " + _snapshot.CombatRoundNumber, _styles.Heading);
      GUILayout.Label("PLAYER HP " + _snapshot.Health.Player + "/3", _styles.Heading);
      GUILayout.Label("AI HP " + _snapshot.Health.Ai + "/3", _styles.Heading);
      GUILayout.EndHorizontal();
      GUILayout.Label(
        "REWARDS  ITEM " + _snapshot.ItemRewardCount
        + " / COIN BONUS EVENTS " + _snapshot.CoinIncreaseEventCount,
        _styles.Small);
      GUILayout.Space(4f);

      switch (_snapshot.Phase)
      {
        case PlayableGamePhase.Intro:
          DrawIntro();
          break;
        case PlayableGamePhase.Halli:
          if (_snapshot.Halli != null)
          {
            _halliPanel.Draw(
              _snapshot.Halli,
              _styles,
              _halliCards,
              _selectionFocus,
              () => AdvanceRequested?.Invoke(),
              () => LeftBellRequested?.Invoke(),
              () => RightBellRequested?.Invoke(),
              cardId => WrongBellRewardRequested?.Invoke(cardId));
          }
          break;
        case PlayableGamePhase.PrivateSelection:
          if (_snapshot.Selection != null)
          {
            _selectionPanel.Draw(
              _snapshot.Selection,
              _selectionFocus,
                _styles,
                _pokerCards,
                index => _selectionFocus = index,
                cardId => PrivateCardToggleRequested?.Invoke(cardId),
              () => PrivateCardsConfirmRequested?.Invoke());
          }
          break;
        case PlayableGamePhase.PokerItemWindow:
        case PlayableGamePhase.PokerPrediction:
        case PlayableGamePhase.PokerResult:
          if (_snapshot.Poker != null)
          {
            _pokerPanel.Draw(
              _snapshot.Poker,
              _styles,
              _pokerCards,
              prediction => PredictionRequested?.Invoke(prediction),
              () => AdvanceRequested?.Invoke());
          }
          break;
        case PlayableGamePhase.BattleFinished:
          DrawBattleFinished();
          break;
        case PlayableGamePhase.StageWon:
          DrawStageWon();
          break;
      }

      GUILayout.EndArea();
    }

    private void DrawIntro()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(
        "Goal: win Halli rounds, select 3 private cards, predict the poker result, then reveal. "
        + "The poker loser takes 1 HP damage. First to 0 HP loses the battle.",
        _styles.Status,
        GUILayout.Height(72f));
      GUILayout.Label(
        "Halli: UP/SPACE flip, LEFT/RIGHT ring. Wrong bell loses only that Halli round. "
        + "Wrong-AI reward: Q/E choose, W/ENTER take. Poker: Q/E/W/ENTER select, then 1/2 predict.",
        _styles.Body,
        GUILayout.Height(54f));
      var art = _cardArtSet != null && _cardArtSet.IsComplete
        ? "Halli 156 + poker 52 card fronts and shared back loaded."
        : "Missing card art uses text fallback.";
      GUILayout.Label(art, _styles.Body);
      if (GUILayout.Button("START BATTLE  [ENTER / SPACE]", GUILayout.Height(62f)))
      {
        StartRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void DrawBattleFinished()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(
        "PLAYER DEFEATED - RETURN TO MAIN",
        _styles.Status,
        GUILayout.Height(90f));
      if (GUILayout.Button("RETURN TO MAIN  [R / ENTER]", GUILayout.Height(62f)))
      {
        MainRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void DrawStageWon()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(
        "STAGE CLEAR - BASE COINS " + _snapshot.LastStageBaseCoinReward,
        _styles.Status,
        GUILayout.Height(80f));
      GUILayout.Label(
        "Coin bonus event values and item effects remain pending in the 0.08 design.",
        _styles.Body);
      if (GUILayout.Button("NEXT STAGE  [ENTER / SPACE]", GUILayout.Height(62f)))
      {
        AdvanceRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void HandleHalliInput()
    {
      var halli = _snapshot.Halli;
      if (halli == null) return;
      if (halli.Phase == PrototypeSessionPhase.WrongBellRewardSelection)
      {
        var count = halli.WrongBellRewardCandidates.Count;
        if (count == 0) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
          _selectionFocus = (_selectionFocus - 1 + count) % count;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
          _selectionFocus = (_selectionFocus + 1) % count;
        }
        else if (halli.WrongBellRewardSelectionEnabled
          && Pressed(KeyCode.W, KeyCode.Return))
        {
          WrongBellRewardRequested?.Invoke(
            halli.WrongBellRewardCandidates[_selectionFocus].Id);
        }
        return;
      }

      if (halli.Phase == PrototypeSessionPhase.Finished)
      {
        if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
        return;
      }

      if (halli.CanRing && Input.GetKeyDown(KeyCode.LeftArrow)) LeftBellRequested?.Invoke();
      else if (halli.CanRing && Input.GetKeyDown(KeyCode.RightArrow)) RightBellRequested?.Invoke();
      else if (halli.CanFlip && Pressed(KeyCode.UpArrow, KeyCode.Space))
      {
        AdvanceRequested?.Invoke();
      }
    }

    private void HandleSelectionInput()
    {
      var selection = _snapshot.Selection;
      if (selection == null || selection.WinnerCandidates.Count == 0) return;
      if (Input.GetKeyDown(KeyCode.Q))
      {
        _selectionFocus = (_selectionFocus - 1 + selection.WinnerCandidates.Count)
          % selection.WinnerCandidates.Count;
      }
      else if (Input.GetKeyDown(KeyCode.E))
      {
        _selectionFocus = (_selectionFocus + 1) % selection.WinnerCandidates.Count;
      }
      else if (Input.GetKeyDown(KeyCode.W))
      {
        PrivateCardToggleRequested?.Invoke(selection.WinnerCandidates[_selectionFocus].Id);
      }
      else if (Input.GetKeyDown(KeyCode.Return) && selection.CanConfirm)
      {
        PrivateCardsConfirmRequested?.Invoke();
      }
    }

    private void EnsureRenderers()
    {
      if (_styles == null) _styles = new PlayableDevStyles();
      if (_halliCards == null)
      {
        _halliCards = new PlayableCardRenderer(
          _cardArtSet?.Halli,
          _cardArtSet?.BackTexture,
          _styles);
      }
      if (_pokerCards == null)
      {
        _pokerCards = new PlayableCardRenderer(
          _cardArtSet?.Poker,
          _cardArtSet?.BackTexture,
          _styles);
      }
    }

    private static bool Pressed(KeyCode first, KeyCode second)
    {
      return Input.GetKeyDown(first) || Input.GetKeyDown(second);
    }
  }
}
