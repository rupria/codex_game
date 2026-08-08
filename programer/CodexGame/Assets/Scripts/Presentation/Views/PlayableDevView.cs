using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Rewards;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
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
    private LocalizationRuntime _localization;

    public event Action StartRequested;
    public event Action AdvanceRequested;
    public event Action LeftBellRequested;
    public event Action RightBellRequested;
    public event Action<CardId> PrivateCardToggleRequested;
    public event Action PrivateCardsConfirmRequested;
    public event Action<PredictionChoice> PredictionRequested;
    public event Action MainRequested;

    private void Awake()
    {
      _localization = GetComponent<LocalizationRuntime>();
      if (_localization == null) _localization = gameObject.AddComponent<LocalizationRuntime>();
      _localization.Changed += HandleLocalizationChanged;
    }

    private void OnDestroy()
    {
      if (_localization != null) _localization.Changed -= HandleLocalizationChanged;
    }

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
      if (_snapshot == null || _localization == null || !_localization.IsReady) return;

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
      if (_localization == null || !_localization.IsReady) return;
      EnsureRenderers();

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
          _localization,
          () => AdvanceRequested?.Invoke(),
          () => LeftBellRequested?.Invoke(),
          () => RightBellRequested?.Invoke());
        return;
      }

      GUILayout.BeginArea(new Rect(48f, 28f, 864f, 484f));
      GUILayout.Label(L("UI_GAME_TITLE"), _styles.Title);
      GUILayout.BeginHorizontal();
      GUILayout.Label(L("UI_HUD_STAGE", new LocalizationArgument("stage", _snapshot.StageNumber)), _styles.Heading);
      GUILayout.Label(L("UI_HUD_COMBAT_ROUND", new LocalizationArgument("round", _snapshot.CombatRoundNumber)), _styles.Heading);
      GUILayout.Label(L("UI_HUD_PLAYER_HP", new LocalizationArgument("current", _snapshot.Health.Player)), _styles.Heading);
      GUILayout.Label(L("UI_HUD_AI_HP", new LocalizationArgument("current", _snapshot.Health.Ai)), _styles.Heading);
      GUILayout.EndHorizontal();
      GUILayout.Label(
        L(
          "UI_HUD_REWARDS",
          new LocalizationArgument("coins", _snapshot.CoinCount)),
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
              _localization,
              index => _selectionFocus = index,
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
              _pokerCards,
              _localization,
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
      GUI.Label(new Rect(250f, 118f, 460f, 70f), L("UI_GAME_TITLE"), _styles.Title);
      GUI.Label(new Rect(300f, 184f, 360f, 34f), L("UI_GAME_SUBTITLE"), _styles.Heading);
      if (GUI.Button(new Rect(330f, 246f, 300f, 58f), L("UI_MAIN_START")))
      {
        StartRequested?.Invoke();
      }
      if (GUI.Button(new Rect(330f, 318f, 300f, 58f), L("UI_MAIN_GUIDE")))
      {
        _guideOpen = true;
      }
      GUI.enabled = _localization.Language != LocalizationCatalog.DefaultLanguage;
      if (GUI.Button(new Rect(330f, 390f, 145f, 38f), "한국어"))
      {
        _localization.SetLanguage(LocalizationCatalog.DefaultLanguage);
      }
      GUI.enabled = _localization.Language != LocalizationCatalog.FallbackLanguage;
      if (GUI.Button(new Rect(485f, 390f, 145f, 38f), "English"))
      {
        _localization.SetLanguage(LocalizationCatalog.FallbackLanguage);
      }
      GUI.enabled = true;
      if (_guideOpen) DrawGuideOverlay();
    }

    private void DrawGuideOverlay()
    {
      GUI.Box(new Rect(86f, 54f, 788f, 432f), GUIContent.none);
      GUI.Label(new Rect(120f, 72f, 720f, 44f), L("UI_GUIDE_TITLE"), _styles.Title);
      DrawGuideStep(new Rect(126f, 136f, 210f, 230f), "1", L("UI_GUIDE_FLIP_KEY"), L("UI_GUIDE_FLIP_DESC"));
      DrawGuideStep(
        new Rect(375f, 136f, 210f, 230f),
        "2",
        L("UI_GUIDE_BELL_KEY"),
        L("UI_GUIDE_BELL_DESC"));
      DrawGuideStep(
        new Rect(624f, 136f, 210f, 230f),
        "3",
        L("UI_GUIDE_PREDICT_KEY"),
        L("UI_GUIDE_PREDICT_DESC"));
      if (GUI.Button(new Rect(360f, 404f, 240f, 52f), L("UI_COMMON_CLOSE_ESC"))) _guideOpen = false;
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
        L("UI_BATTLE_DEFEATED"),
        _styles.Status,
        GUILayout.Height(90f));
      if (GUILayout.Button(L("UI_RETURN_MAIN"), GUILayout.Height(62f)))
      {
        MainRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void DrawStageWon()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(
        L("UI_STAGE_CLEAR", new LocalizationArgument("coins", _snapshot.CoinCount)),
        _styles.Status,
        GUILayout.Height(80f));
      if (GUILayout.Button(L("UI_NEXT_STAGE"), GUILayout.Height(62f)))
      {
        AdvanceRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void HandleHalliInput()
    {
      var halli = _snapshot.Halli;
      if (halli == null) return;
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
          _styles,
          _localization);
      }
      if (_pokerCards == null)
      {
        _pokerCards = new PlayableCardRenderer(
          _cardArtSet?.Poker,
          _cardArtSet?.BackTexture,
          _styles,
          _localization);
      }
    }

    private string L(string key, params LocalizationArgument[] arguments)
    {
      return _localization.Get(key, arguments);
    }

    private void HandleLocalizationChanged()
    {
      _halliCards = null;
      _pokerCards = null;
    }

    private static bool Pressed(KeyCode first, KeyCode second)
    {
      return Input.GetKeyDown(first) || Input.GetKeyDown(second);
    }
  }
}
