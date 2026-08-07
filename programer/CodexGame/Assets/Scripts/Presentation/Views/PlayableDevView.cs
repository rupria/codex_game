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
    private PlayableCardArtLibrary _cardArt;

    private readonly HalliDevPanel _halliPanel = new HalliDevPanel();
    private readonly PrivateSelectionDevPanel _selectionPanel = new PrivateSelectionDevPanel();
    private readonly PokerDevPanel _pokerPanel = new PokerDevPanel();
    private PlayableGameSnapshot _snapshot;
    private PlayableDevStyles _styles;
    private PlayableCardRenderer _cards;
    private int _selectionFocus;

    public event Action StartRequested;
    public event Action AdvanceRequested;
    public event Action LeftBellRequested;
    public event Action RightBellRequested;
    public event Action<CardId> PrivateCardToggleRequested;
    public event Action PrivateCardsConfirmRequested;
    public event Action<PredictionChoice> PredictionRequested;
    public event Action RestartRequested;

    public void Configure(Texture2D boardTexture, PlayableCardArtLibrary cardArt)
    {
      _boardTexture = boardTexture;
      _cardArt = cardArt;
      _cards = null;
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
        case PlayableGamePhase.PokerPrediction:
          if (Input.GetKeyDown(KeyCode.Alpha1)) PredictionRequested?.Invoke(PredictionChoice.PlayerWins);
          else if (Input.GetKeyDown(KeyCode.Alpha2)) PredictionRequested?.Invoke(PredictionChoice.PlayerLoses);
          break;
        case PlayableGamePhase.PokerResult:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
        case PlayableGamePhase.BattleFinished:
          if (Pressed(KeyCode.R, KeyCode.Return)) RestartRequested?.Invoke();
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
      GUILayout.Label("CODEX GAME 0.06 - HALLI TO POKER DEV BUILD", _styles.Title);
      GUILayout.BeginHorizontal();
      GUILayout.Label("COMBAT ROUND " + _snapshot.CombatRoundNumber, _styles.Heading);
      GUILayout.Label("PLAYER HP " + _snapshot.Health.Player + "/3", _styles.Heading);
      GUILayout.Label("AI HP " + _snapshot.Health.Ai + "/3", _styles.Heading);
      GUILayout.EndHorizontal();
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
              _cards,
              () => AdvanceRequested?.Invoke(),
              () => LeftBellRequested?.Invoke(),
              () => RightBellRequested?.Invoke());
          }
          break;
        case PlayableGamePhase.PrivateSelection:
          if (_snapshot.Selection != null)
          {
            _selectionPanel.Draw(
              _snapshot.Selection,
              _selectionFocus,
              _styles,
              _cards,
              cardId => PrivateCardToggleRequested?.Invoke(cardId),
              () => PrivateCardsConfirmRequested?.Invoke());
          }
          break;
        case PlayableGamePhase.PokerPrediction:
        case PlayableGamePhase.PokerResult:
          if (_snapshot.Poker != null)
          {
            _pokerPanel.Draw(
              _snapshot.Poker,
              _styles,
              _cards,
              prediction => PredictionRequested?.Invoke(prediction),
              () => AdvanceRequested?.Invoke());
          }
          break;
        case PlayableGamePhase.BattleFinished:
          DrawBattleFinished();
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
        + "Poker: Q/E/W/ENTER select, then 1/2 predict.",
        _styles.Body,
        GUILayout.Height(54f));
      var art = _cardArt != null && _cardArt.IsComplete && _cardArt.BackTexture != null
        ? "156 card fronts and shared card back loaded."
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
      var playerWon = _snapshot.Health.Ai == 0;
      GUILayout.Label(
        playerWon ? "PLAYER WINS THE BATTLE" : "AI WINS THE BATTLE",
        _styles.Status,
        GUILayout.Height(90f));
      if (GUILayout.Button("NEW BATTLE  [R / ENTER]", GUILayout.Height(62f)))
      {
        RestartRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void HandleHalliInput()
    {
      var halli = _snapshot.Halli;
      if (halli == null) return;
      if (halli.Phase == PrototypeSessionPhase.Finished)
      {
        if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
        return;
      }

      var canRing = halli.Phase == PrototypeSessionPhase.ReadyToFlip
        || halli.Phase == PrototypeSessionPhase.BellOpen;
      if (canRing && Input.GetKeyDown(KeyCode.LeftArrow)) LeftBellRequested?.Invoke();
      else if (canRing && Input.GetKeyDown(KeyCode.RightArrow)) RightBellRequested?.Invoke();
      else if (Pressed(KeyCode.UpArrow, KeyCode.Space)
        || (halli.Phase == PrototypeSessionPhase.Review && Input.GetKeyDown(KeyCode.W)))
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
      if (_cards == null) _cards = new PlayableCardRenderer(_cardArt, _styles);
    }

    private static bool Pressed(KeyCode first, KeyCode second)
    {
      return Input.GetKeyDown(first) || Input.GetKeyDown(second);
    }
  }
}
