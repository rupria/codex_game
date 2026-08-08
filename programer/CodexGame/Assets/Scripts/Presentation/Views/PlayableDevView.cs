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

    [SerializeField]
    private GuideUiArtSet _guideUiArtSet;

    [SerializeField]
    private Texture2D _introTexture;

    [SerializeField]
    private HealthUiArtSet _healthUiArtSet;

    [SerializeField]
    private PokerUiArtSet _pokerUiArtSet;

    [SerializeField]
    private bool _useSceneBackdrop;

    [SerializeField]
    private bool _useIntroArtLayout;

    private readonly HalliDevPanel _halliPanel = new HalliDevPanel();
    private readonly GuideModalPanel _guidePanel = new GuideModalPanel();
    private readonly GuideModalState _guide = new GuideModalState();
    private readonly HalliTableLightOverlay _tableLightOverlay = new HalliTableLightOverlay();
    private readonly PrivateSelectionDevPanel _selectionPanel = new PrivateSelectionDevPanel();
    private readonly PokerDevPanel _pokerPanel = new PokerDevPanel();
    private PlayableGameSnapshot _snapshot;
    private PlayableDevStyles _styles;
    private PlayableCardRenderer _halliCards;
    private PlayableCardRenderer _pokerCards;
    private int _selectionFocus;
    private LocalizationRuntime _localization;

    public event Action StartRequested;
    public event Action AdvanceRequested;
    public event Action LeftBellRequested;
    public event Action RightBellRequested;
    public event Action<CardId> PrivateCardToggleRequested;
    public event Action PrivateCardsConfirmRequested;
    public event Action<PredictionChoice> PredictionRequested;
    public event Action MainRequested;

    internal PlayableGamePhase CurrentPhase => _snapshot == null
      ? PlayableGamePhase.Intro
      : _snapshot.Phase;

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
      Configure(boardTexture, cardArtSet, null, null);
    }

    public void Configure(
      Texture2D boardTexture,
      PlayableCardArtSet cardArtSet,
      HalliUiArtSet halliUiArtSet)
    {
      Configure(boardTexture, cardArtSet, halliUiArtSet, null);
    }

    public void Configure(
      Texture2D boardTexture,
      PlayableCardArtSet cardArtSet,
      HalliUiArtSet halliUiArtSet,
      GuideUiArtSet guideUiArtSet,
      Texture2D introTexture = null,
      HealthUiArtSet healthUiArtSet = null,
      PokerUiArtSet pokerUiArtSet = null,
      bool useSceneBackdrop = false,
      bool useIntroArtLayout = false)
    {
      _boardTexture = boardTexture;
      _cardArtSet = cardArtSet;
      _halliUiArtSet = halliUiArtSet;
      _guideUiArtSet = guideUiArtSet;
      _introTexture = introTexture;
      _healthUiArtSet = healthUiArtSet;
      _pokerUiArtSet = pokerUiArtSet;
      _useSceneBackdrop = useSceneBackdrop;
      _useIntroArtLayout = useIntroArtLayout;
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

      if (_guide.IsOpen)
      {
        if (Input.GetKeyDown(KeyCode.Escape)) _guide.Close();
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) _guide.MovePrevious();
        else if (Input.GetKeyDown(KeyCode.RightArrow)) _guide.MoveNext();
        return;
      }

      switch (_snapshot.Phase)
      {
        case PlayableGamePhase.Intro:
          if (Pressed(KeyCode.Return, KeyCode.Space)) StartRequested?.Invoke();
          else if (Input.GetKeyDown(KeyCode.G)) _guide.Open();
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
        case PlayableGamePhase.Bar:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
      }
    }

    private void OnGUI()
    {
      if (_snapshot == null) return;
      PlayableViewport.Apply();
      var drewExactIntroArt = _snapshot.Phase == PlayableGamePhase.Intro
        && _useIntroArtLayout
        && _introTexture != null;
      if (drewExactIntroArt)
      {
        GUI.DrawTexture(
          new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
          _introTexture,
          ScaleMode.StretchToFill,
          true);
      }
      else if (!_useSceneBackdrop && _boardTexture != null)
      {
        GUI.DrawTexture(
          new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
          _boardTexture,
          ScaleMode.StretchToFill,
          true);
      }
      if (!_useSceneBackdrop && !drewExactIntroArt)
      {
        _tableLightOverlay.Draw(
          Time.unscaledTime,
          _snapshot.Phase == PlayableGamePhase.Intro ? 0.65f : 1f);
      }
      if (_localization == null || !_localization.IsReady) return;
      EnsureRenderers();

      if (_guide.IsOpen)
      {
        _guidePanel.Draw(
          _guide,
          _styles,
          _guideUiArtSet,
          _localization,
          _guide.MovePrevious,
          _guide.MoveNext,
          _guide.Close);
        return;
      }

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
          _healthUiArtSet,
          _localization,
          () => AdvanceRequested?.Invoke(),
          () => LeftBellRequested?.Invoke(),
          () => RightBellRequested?.Invoke());
        return;
      }

      if ((_snapshot.Phase == PlayableGamePhase.PokerPrediction
          || _snapshot.Phase == PlayableGamePhase.PokerResult)
        && _snapshot.Poker != null)
      {
        _pokerPanel.Draw(
          _snapshot.Poker,
          _styles,
          _pokerCards,
          _healthUiArtSet,
          _pokerUiArtSet,
          _localization,
          prediction => PredictionRequested?.Invoke(prediction),
          () => AdvanceRequested?.Invoke());
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
          new LocalizationArgument("bullets", _snapshot.BulletCount)),
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
        case PlayableGamePhase.BattleFinished:
          DrawBattleFinished();
          break;
        case PlayableGamePhase.StageWon:
          DrawStageWon();
          break;
        case PlayableGamePhase.Bar:
          DrawBar();
          break;
      }

      GUILayout.EndArea();
    }

    private void DrawIntro()
    {
      if (_useIntroArtLayout)
      {
        DrawIntroArtLayout();
        return;
      }

      GUI.Box(new Rect(244f, 92f, 472f, 356f), GUIContent.none);
      GUI.Label(new Rect(250f, 118f, 460f, 70f), L("UI_GAME_TITLE"), _styles.Title);
      GUI.Label(new Rect(300f, 184f, 360f, 34f), L("UI_GAME_SUBTITLE"), _styles.Heading);
      if (GUI.Button(new Rect(330f, 246f, 300f, 58f), L("UI_MAIN_START")))
      {
        StartRequested?.Invoke();
      }
      if (GUI.Button(new Rect(330f, 318f, 300f, 58f), L("UI_MAIN_GUIDE")))
      {
        _guide.Open();
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
    }

    private void DrawIntroArtLayout()
    {
      if (DrawIntroArtButton(
        new Rect(312f, 266f, 336f, 76f),
        L("UI_MAIN_START"),
        new Color(0.015f, 0.055f, 0.075f, 0.96f)))
      {
        StartRequested?.Invoke();
      }
      if (DrawIntroArtButton(
        new Rect(312f, 354f, 336f, 78f),
        L("UI_MAIN_GUIDE"),
        new Color(0.055f, 0.02f, 0.035f, 0.96f)))
      {
        _guide.Open();
      }

      GUI.enabled = _localization.Language != LocalizationCatalog.DefaultLanguage;
      if (GUI.Button(new Rect(748f, 18f, 92f, 30f), "한국어"))
      {
        _localization.SetLanguage(LocalizationCatalog.DefaultLanguage);
      }
      GUI.enabled = _localization.Language != LocalizationCatalog.FallbackLanguage;
      if (GUI.Button(new Rect(846f, 18f, 96f, 30f), "English"))
      {
        _localization.SetLanguage(LocalizationCatalog.FallbackLanguage);
      }
      GUI.enabled = true;
    }

    private bool DrawIntroArtButton(Rect rect, string label, Color coverColor)
    {
      var hovered = rect.Contains(Event.current.mousePosition);
      var pressed = hovered && Input.GetMouseButton(0);
      var previousColor = GUI.color;
      GUI.color = hovered
        ? new Color(
          Mathf.Min(coverColor.r * 1.35f, 1f),
          Mathf.Min(coverColor.g * 1.35f, 1f),
          Mathf.Min(coverColor.b * 1.35f, 1f),
          1f)
        : new Color(coverColor.r, coverColor.g, coverColor.b, 1f);
      GUI.DrawTexture(
        new Rect(rect.x + 9f, rect.y + 9f, rect.width - 18f, rect.height - 18f),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previousColor;
      var labelRect = pressed
        ? new Rect(rect.x, rect.y + 2f, rect.width, rect.height)
        : rect;
      GUI.Label(labelRect, label, _styles.IntroButton);
      return GUI.Button(rect, GUIContent.none, GUIStyle.none);
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
        L("UI_STAGE_CLEAR", new LocalizationArgument("reward", _snapshot.LastStageReward)),
        _styles.Status,
        GUILayout.Height(80f));
      GUILayout.Label(
        L(
          "UI_STAGE_REWARD_FORMULA",
          new LocalizationArgument("hp", _snapshot.Health.Player),
          new LocalizationArgument("reward", _snapshot.LastStageReward)),
        _styles.Heading);
      GUILayout.Label(
        L("UI_BULLET_BALANCE", new LocalizationArgument("bullets", _snapshot.BulletCount)),
        _styles.Heading);
      if (GUILayout.Button(L("UI_COMMON_CONTINUE"), GUILayout.Height(62f)))
      {
        AdvanceRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void DrawBar()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(L("UI_BAR_TITLE"), _styles.Status, GUILayout.Height(70f));
      GUILayout.Label(
        L("UI_BULLET_BALANCE", new LocalizationArgument("bullets", _snapshot.BulletCount)),
        _styles.Heading);
      GUILayout.Label(
        L(
          "UI_BAR_HP_STATUS",
          new LocalizationArgument("current", _snapshot.Health.Player),
          new LocalizationArgument("max", 3)),
        _styles.Heading);
      if (GUILayout.Button(L("UI_BAR_CONTINUE"), GUILayout.Height(62f)))
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
      if (Pressed(KeyCode.LeftArrow, KeyCode.Q))
      {
        _selectionFocus = (_selectionFocus - 1 + selection.WinnerCandidates.Count)
          % selection.WinnerCandidates.Count;
      }
      else if (Pressed(KeyCode.RightArrow, KeyCode.E))
      {
        _selectionFocus = (_selectionFocus + 1) % selection.WinnerCandidates.Count;
      }
      else if (Pressed(KeyCode.UpArrow, KeyCode.W))
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
