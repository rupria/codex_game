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

    [SerializeField]
    private HalliUiArtSet _halliUiArtSet;

    private readonly HalliDevPanel _halliPanel = new HalliDevPanel();
    private readonly HalliTableLightOverlay _tableLightOverlay = new HalliTableLightOverlay();
    private readonly PrivateSelectionDevPanel _selectionPanel = new PrivateSelectionDevPanel();
    private readonly PokerDevPanel _pokerPanel = new PokerDevPanel();
    private PlayableGameSnapshot _snapshot;
    private PlayableDevStyles _styles;
    private PlayableCardRenderer _halliCards;
    private PlayableCardRenderer _pokerCards;
    private int _selectionFocus;
    private bool _guideOpen;

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
      Configure(boardTexture, cardArtSet, null);
    }

    public void Configure(
      Texture2D boardTexture,
      PlayableCardArtSet cardArtSet,
      HalliUiArtSet halliUiArtSet)
    {
      _boardTexture = boardTexture;
      _cardArtSet = cardArtSet;
      _halliUiArtSet = halliUiArtSet;
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
          if (_guideOpen)
          {
            if (Input.GetKeyDown(KeyCode.Escape)) _guideOpen = false;
          }
          else
          {
            if (Pressed(KeyCode.Return, KeyCode.Space)) StartRequested?.Invoke();
            else if (Input.GetKeyDown(KeyCode.G)) _guideOpen = true;
          }
          break;
        case PlayableGamePhase.HalliOpening:
          break;
        case PlayableGamePhase.Halli:
          HandleHalliInput();
          break;
        case PlayableGamePhase.HalliTransition:
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
      PlayableViewport.Apply();
      if (_boardTexture != null)
      {
        GUI.DrawTexture(
          new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
          _boardTexture,
          ScaleMode.StretchToFill,
          true);
      }
      _tableLightOverlay.Draw(Time.unscaledTime, _snapshot.Phase == PlayableGamePhase.Intro ? 0.65f : 1f);

      if (_snapshot.Phase == PlayableGamePhase.Intro)
      {
        DrawIntro();
        return;
      }

      if ((_snapshot.Phase == PlayableGamePhase.HalliOpening
          || _snapshot.Phase == PlayableGamePhase.Halli
          || _snapshot.Phase == PlayableGamePhase.HalliTransition)
        && _snapshot.Halli != null)
      {
        _halliPanel.Draw(
          _snapshot.Halli,
          _snapshot.Phase,
          _snapshot.Transition,
          _snapshot.Health.Player,
          _snapshot.Health.Ai,
          _styles,
          _halliCards,
          _halliUiArtSet,
          _selectionFocus,
          () => AdvanceRequested?.Invoke(),
          () => LeftBellRequested?.Invoke(),
          () => RightBellRequested?.Invoke(),
          cardId => WrongBellRewardRequested?.Invoke(cardId));
        return;
      }

      GUILayout.BeginArea(new Rect(48f, 28f, 864f, 484f));
      GUILayout.Label("CODEX GAME", _styles.Title);
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
      GUI.Box(new Rect(244f, 92f, 472f, 356f), GUIContent.none);
      GUI.Label(new Rect(250f, 118f, 460f, 70f), "CODEX GAME", _styles.Title);
      GUI.Label(new Rect(300f, 184f, 360f, 34f), "HALLI  ×  POKER", _styles.Heading);
      if (GUI.Button(new Rect(330f, 260f, 300f, 62f), "START"))
      {
        StartRequested?.Invoke();
      }
      if (GUI.Button(new Rect(330f, 338f, 300f, 62f), "GUIDE"))
      {
        _guideOpen = true;
      }
      if (_guideOpen) DrawGuideOverlay();
    }

    private void DrawGuideOverlay()
    {
      GUI.Box(new Rect(86f, 54f, 788f, 432f), GUIContent.none);
      GUI.Label(new Rect(120f, 72f, 720f, 44f), "HOW TO PLAY", _styles.Title);
      DrawGuideStep(new Rect(126f, 136f, 210f, 230f), "1", "W", "FLIP ONE CARD\nAI FOLLOWS");
      DrawGuideStep(
        new Rect(375f, 136f, 210f, 230f),
        "2",
        "Q / E",
        "RING THE MATCHING SIDE\nWHEN SAME-SUIT SKULLS TOTAL 3");
      DrawGuideStep(
        new Rect(624f, 136f, 210f, 230f),
        "3",
        "1 / 2",
        "BUILD A POKER HAND\nAND PREDICT THE RESULT");
      if (GUI.Button(new Rect(360f, 404f, 240f, 52f), "CLOSE  [ESC]")) _guideOpen = false;
    }

    private void DrawGuideStep(Rect rect, string number, string key, string description)
    {
      GUI.Box(rect, GUIContent.none);
      GUI.Label(new Rect(rect.x, rect.y + 14f, rect.width, 32f), number, _styles.Title);
      GUI.Label(new Rect(rect.x + 20f, rect.y + 62f, rect.width - 40f, 54f), key, _styles.Status);
      GUI.Label(new Rect(rect.x + 14f, rect.y + 128f, rect.width - 28f, 80f), description, _styles.Body);
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

      if (halli.CanRing && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow)))
      {
        LeftBellRequested?.Invoke();
      }
      else if (halli.CanRing
        && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow)))
      {
        RightBellRequested?.Invoke();
      }
      else if (halli.CanFlip
        && (Input.GetKeyDown(KeyCode.W) || Pressed(KeyCode.UpArrow, KeyCode.Space)))
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
