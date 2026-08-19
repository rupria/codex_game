using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Core.Items;
using CodexGame.Core.Poker;
#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
using CodexGame.Application.Development;
#endif
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
    private PokerItemUiArtSet _pokerItemUiArtSet;

    [SerializeField]
    private PokerResultUiArtSet _pokerResultUiArtSet;

    [SerializeField]
    private BarShopUiArtSet _barShopUiArtSet;

    [SerializeField]
    private StageTransitionUiArtSet _stageTransitionUiArtSet;

    [SerializeField]
    private EconomyUiArtSet _economyUiArtSet;

    [SerializeField]
    private PresentationUiArtSet _presentationUiArtSet;

    [SerializeField]
    private PrivateSelectionUiArtSet _privateSelectionUiArtSet;

    [SerializeField]
    private JokerRevealUiArtSet _jokerRevealUiArtSet;

    [SerializeField]
    private bool _useSceneBackdrop;

    [SerializeField]
    private bool _useIntroArtLayout;

    private readonly HalliDevPanel _halliPanel = new HalliDevPanel();
    private readonly GuideModalPanel _guidePanel = new GuideModalPanel();
    private readonly GuideModalState _guide = new GuideModalState();
    private readonly FirstStartTutorialSession _firstStartTutorial =
      new FirstStartTutorialSession();
    private readonly HalliTableLightOverlay _tableLightOverlay = new HalliTableLightOverlay();
    private readonly PrivateSelectionDevPanel _selectionPanel = new PrivateSelectionDevPanel();
    private readonly PokerDevPanel _pokerPanel = new PokerDevPanel();
    private readonly PokerItemDevPanel _pokerItemPanel = new PokerItemDevPanel();
    private readonly BarShopDevPanel _barShopPanel = new BarShopDevPanel();
    private readonly StageTransitionDevPanel _stageTransitionPanel =
      new StageTransitionDevPanel();
    private readonly Presentation0124Panel _presentation0124Panel =
      new Presentation0124Panel();
#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
    private readonly DevelopmentCheatPanel _cheatPanel = new DevelopmentCheatPanel();
    private bool _cheatOpen;
#endif
    private PlayableGameSnapshot _snapshot;
    private PlayableDevStyles _styles;
    private PlayableCardRenderer _halliCards;
    private PlayableCardRenderer _pokerCards;
    private int _selectionFocus = -1;
    private long _selectionSessionSerial;
    private LocalizationRuntime _localization;
    private float _playerDamageUntil = float.NegativeInfinity;
    private float _aiDamageUntil = float.NegativeInfinity;

    public event Action StartRequested;
    public event Action StageEntrySkipRequested;
    public event Action AdvanceRequested;
    public event Action LeftBellRequested;
    public event Action RightBellRequested;
    public event Action<CardId> PrivateCardToggleRequested;
    public event Action PrivateCardsConfirmRequested;
    public event Action<PredictionChoice> PredictionRequested;
    public event Action<PokerHandCategory> JokerHandRequested;
    public event Action<CardId> ReloadItemRequested;
    public event Action<CardId> BottomDealRequested;
    public event Action<CardId> BottomDealChoiceRequested;
    public event Action HypeManItemRequested;
    public event Action HealthRecoveryItemRequested;
    public event Action<CardId, CardSuit> WildInkItemRequested;
    public event Action BarrelItemRequested;
    public event Action PredictionInsuranceItemRequested;
    public event Action<CardId> MercenaryItemRequested;
    public event Action ItemsConfirmRequested;
    public event Action BarShopRerollRequested;
    public event Action<int> BarShopPurchaseRequested;
    public event Action MainRequested;
    public event Action InactivityAcknowledgedRequested;
#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
    public event Action CheatStagePassRequested;
    public event Action<bool> CheatJokerAwardRequested;
    public event Action<GameItemId> CheatGrantItemRequested;
    public event Action<PokerCheatPreset> CheatPokerPresetRequested;
    public event Action<ItemQaPreset> CheatItemQaPresetRequested;
#endif

    internal PlayableGamePhase CurrentPhase => _snapshot == null
      ? PlayableGamePhase.Intro
      : _snapshot.Phase;

    public bool IsNextStagePresentationReady => _stageTransitionUiArtSet != null
      && _stageTransitionUiArtSet.IsComplete;

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
      bool useIntroArtLayout = false,
      BarShopUiArtSet barShopUiArtSet = null,
      StageTransitionUiArtSet stageTransitionUiArtSet = null,
      PokerItemUiArtSet pokerItemUiArtSet = null,
      EconomyUiArtSet economyUiArtSet = null,
      PresentationUiArtSet presentationUiArtSet = null,
      PokerResultUiArtSet pokerResultUiArtSet = null,
      PrivateSelectionUiArtSet privateSelectionUiArtSet = null,
      JokerRevealUiArtSet jokerRevealUiArtSet = null)
    {
      _boardTexture = boardTexture;
      _cardArtSet = cardArtSet;
      _halliUiArtSet = halliUiArtSet;
      _guideUiArtSet = guideUiArtSet;
      _introTexture = introTexture;
      _healthUiArtSet = healthUiArtSet;
      _pokerUiArtSet = pokerUiArtSet;
      _pokerItemUiArtSet = pokerItemUiArtSet;
      _pokerResultUiArtSet = pokerResultUiArtSet;
      _barShopUiArtSet = barShopUiArtSet;
      _stageTransitionUiArtSet = stageTransitionUiArtSet;
      _economyUiArtSet = economyUiArtSet;
      _presentationUiArtSet = presentationUiArtSet;
      _privateSelectionUiArtSet = privateSelectionUiArtSet;
      _jokerRevealUiArtSet = jokerRevealUiArtSet;
      _useSceneBackdrop = useSceneBackdrop;
      _useIntroArtLayout = useIntroArtLayout;
      _halliCards = null;
      _pokerCards = null;
    }

    public void Present(PlayableGameSnapshot snapshot)
    {
      if ((_snapshot == null || _snapshot.Phase != PlayableGamePhase.PrivateSelection)
        && snapshot.Phase == PlayableGamePhase.PrivateSelection)
      {
        _selectionSessionSerial++;
      }
      if (_snapshot != null)
      {
        if (snapshot.Health.Player < _snapshot.Health.Player)
        {
          _playerDamageUntil = Time.unscaledTime + 0.35f;
        }
        if (snapshot.Health.Ai < _snapshot.Health.Ai)
        {
          _aiDamageUntil = Time.unscaledTime + 0.35f;
        }
      }
      if (_snapshot == null || _snapshot.Phase != snapshot.Phase)
      {
        _selectionFocus = -1;
      }

      _snapshot = snapshot;
    }

    private void Update()
    {
      if (_snapshot == null || _localization == null || !_localization.IsReady) return;

      if (_snapshot.InactivityReturnPending) return;

      if (_guide.IsOpen)
      {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
          if (_guide.IsFirstStartTutorial) CompleteFirstStartTutorial();
          else _guide.Close();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) _guide.MovePrevious();
        else if (Input.GetKeyDown(KeyCode.RightArrow)) AdvanceGuide();
        return;
      }

#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
      if (Input.GetKeyDown(KeyCode.BackQuote))
      {
        _cheatOpen = !_cheatOpen;
        return;
      }
      if (_cheatOpen) return;
#endif

      switch (_snapshot.Phase)
      {
        case PlayableGamePhase.Intro:
          if (Pressed(KeyCode.Return, KeyCode.Space)) RequestStart();
          else if (Input.GetKeyDown(KeyCode.G)) _guide.OpenMainGuide();
          break;
        case PlayableGamePhase.HalliOpening:
          break;
        case PlayableGamePhase.StageEntry:
          break;
        case PlayableGamePhase.Halli:
          HandleHalliInput();
          break;
        case PlayableGamePhase.HalliTransition:
          break;
        case PlayableGamePhase.PrivateSelection:
          if (_snapshot.Selection != null)
          {
            _selectionPanel.Observe(
              _selectionSessionSerial,
              _snapshot.Selection,
              Time.unscaledTime);
            if (!_selectionPanel.IsInputLocked(Time.unscaledTime)) HandleSelectionInput();
          }
          break;
        case PlayableGamePhase.PokerPrediction:
          if (_snapshot.Poker != null
            && _snapshot.Poker.Phase == Application.Poker.PokerRoundPhase.AwaitingPrediction)
          {
            if (Input.GetKeyDown(KeyCode.Alpha1)) PredictionRequested?.Invoke(PredictionChoice.PlayerWins);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) PredictionRequested?.Invoke(PredictionChoice.PlayerLoses);
          }
          break;
        case PlayableGamePhase.PokerItems:
          break;
        case PlayableGamePhase.PokerResult:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
        case PlayableGamePhase.BattleFinished:
          if (Pressed(KeyCode.R, KeyCode.Return)) MainRequested?.Invoke();
          break;
        case PlayableGamePhase.RunWon:
          if (Pressed(KeyCode.R, KeyCode.Return)) MainRequested?.Invoke();
          break;
        case PlayableGamePhase.StageWon:
          if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
        case PlayableGamePhase.BarShop:
          if (Input.GetKeyDown(KeyCode.R)) BarShopRerollRequested?.Invoke();
          else if (Pressed(KeyCode.Return, KeyCode.Space)) AdvanceRequested?.Invoke();
          break;
        case PlayableGamePhase.NextStageTransition:
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

      if (_snapshot.InactivityReturnPending)
      {
        DrawInactivityReturnModal();
        return;
      }

#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
      if (_snapshot.CheatUsed)
      {
        GUI.Label(new Rect(792f, 8f, 152f, 32f), "CHEAT / QA", _styles.Status);
      }
      if (_cheatOpen)
      {
        _cheatPanel.Draw(
          _snapshot,
          _styles,
          () => CheatStagePassRequested?.Invoke(),
          enabled => CheatJokerAwardRequested?.Invoke(enabled),
          itemId => CheatGrantItemRequested?.Invoke(itemId),
          preset => CheatPokerPresetRequested?.Invoke(preset),
          preset => CheatItemQaPresetRequested?.Invoke(preset),
          () => _cheatOpen = false);
        return;
      }
#endif

      if (_guide.IsOpen)
      {
        _guidePanel.Draw(
          _guide,
          _styles,
          _guideUiArtSet,
          _localization,
          _guide.MovePrevious,
          AdvanceGuide,
          _guide.Close,
          CompleteFirstStartTutorial);
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.Intro)
      {
        DrawIntro();
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.StageEntry)
      {
        _presentation0124Panel.DrawStageEntry(
          _snapshot,
          _presentationUiArtSet,
          _styles,
          _localization,
          () => StageEntrySkipRequested?.Invoke());
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
          Time.unscaledTime < _playerDamageUntil,
          Time.unscaledTime < _aiDamageUntil,
          _styles,
          _halliCards,
          _halliUiArtSet,
          _healthUiArtSet,
          _localization,
          () => AdvanceRequested?.Invoke(),
          () => LeftBellRequested?.Invoke(),
          () => RightBellRequested?.Invoke());
        DrawOpponentPortrait();
        if (_snapshot.Phase == PlayableGamePhase.HalliOpening)
        {
          _presentation0124Panel.DrawThreeCallEntry(
            _presentationUiArtSet,
            _styles,
            _localization);
        }
        else if (_snapshot.Phase == PlayableGamePhase.HalliTransition)
        {
          _presentation0124Panel.DrawThreeCallToSelection(
            _presentationUiArtSet,
            _styles,
            _localization);
        }
        DrawBattleEconomyHud();
        return;
      }

      if ((_snapshot.Phase == PlayableGamePhase.PokerPrediction
          || _snapshot.Phase == PlayableGamePhase.PokerResult)
        && _snapshot.Poker != null)
      {
        _presentation0124Panel.DrawShowdownFrame(
          _snapshot.Phase == PlayableGamePhase.PokerResult,
          _presentationUiArtSet);
        _pokerPanel.Draw(
          _snapshot.Poker,
          _styles,
          _pokerCards,
          _healthUiArtSet,
          _pokerUiArtSet,
          _pokerItemUiArtSet,
          _pokerResultUiArtSet,
          _jokerRevealUiArtSet,
          _snapshot.PredictionReward,
          _snapshot.PredictionInsuranceActivation,
          _localization,
          Time.unscaledTime < _playerDamageUntil,
          Time.unscaledTime < _aiDamageUntil,
          category => JokerHandRequested?.Invoke(category),
          prediction => PredictionRequested?.Invoke(prediction),
          () => AdvanceRequested?.Invoke());
        DrawOpponentPortrait();
        DrawBattleEconomyHud();
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.PrivateSelection
        && _snapshot.Selection != null)
      {
        _selectionPanel.Draw(
          _selectionSessionSerial,
          _snapshot.Selection,
          _selectionFocus,
          _styles,
          _pokerCards,
          _privateSelectionUiArtSet,
          _jokerRevealUiArtSet,
          _localization,
          index => _selectionFocus = index,
          cardId => PrivateCardToggleRequested?.Invoke(cardId),
          () => PrivateCardsConfirmRequested?.Invoke());
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.PokerItems && _snapshot.PokerItems != null)
      {
        _pokerItemPanel.Draw(
          _snapshot.PokerItems,
          _pokerCards,
          _styles,
          _pokerItemUiArtSet,
          _localization,
          cardId => ReloadItemRequested?.Invoke(cardId),
          cardId => BottomDealRequested?.Invoke(cardId),
          cardId => BottomDealChoiceRequested?.Invoke(cardId),
          () => HypeManItemRequested?.Invoke(),
          () => HealthRecoveryItemRequested?.Invoke(),
          (cardId, suit) => WildInkItemRequested?.Invoke(cardId, suit),
          () => BarrelItemRequested?.Invoke(),
          () => PredictionInsuranceItemRequested?.Invoke(),
          cardId => MercenaryItemRequested?.Invoke(cardId),
          () => ItemsConfirmRequested?.Invoke());
        DrawOpponentPortrait();
        _presentation0124Panel.DrawItemRestriction(
          _snapshot.StageItemRestriction,
          _presentationUiArtSet,
          _styles,
          _localization);
        DrawBattleEconomyHud();
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.BarShop && _snapshot.BarShop != null)
      {
        _barShopPanel.Draw(
          _snapshot.BarShop,
          _snapshot.BaseBulletCount,
          _snapshot.TemporaryBulletCount,
          _snapshot.Inventory,
          _styles,
          _barShopUiArtSet,
          _economyUiArtSet,
          _localization,
          () => BarShopRerollRequested?.Invoke(),
          index => BarShopPurchaseRequested?.Invoke(index),
          () => AdvanceRequested?.Invoke(),
          drawBackground: !_useSceneBackdrop);
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.NextStageTransition
        && _snapshot.NextStageTransition != null)
      {
        _stageTransitionPanel.Draw(
          _snapshot.NextStageTransition,
          _stageTransitionUiArtSet,
          _barShopUiArtSet);
        EconomyUiRenderer.DrawTemporaryExpiration(
          new Rect(790f, 472f, 140f, 40f),
          _snapshot.LastExpiredTemporaryBullets,
          _economyUiArtSet,
          _styles.Status);
        return;
      }

      if (_snapshot.Phase == PlayableGamePhase.StageWon)
      {
        DrawStageWon();
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
      GUILayout.Space(4f);

      switch (_snapshot.Phase)
      {
        case PlayableGamePhase.BattleFinished:
          DrawBattleFinished();
          break;
        case PlayableGamePhase.RunWon:
          DrawRunWon();
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
        RequestStart();
      }
      if (GUI.Button(new Rect(330f, 318f, 300f, 58f), L("UI_MAIN_GUIDE")))
      {
        _guide.OpenMainGuide();
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
        RequestStart();
      }
      if (DrawIntroArtButton(
        new Rect(312f, 354f, 336f, 78f),
        L("UI_MAIN_GUIDE"),
        new Color(0.055f, 0.02f, 0.035f, 0.96f)))
      {
        _guide.OpenMainGuide();
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

    private void RequestStart()
    {
      if (_firstStartTutorial.RequestStart() == FirstStartRequest.ShowTutorial)
      {
        _guide.OpenFirstStartTutorial();
        return;
      }
      StartRequested?.Invoke();
    }

    private void AdvanceGuide()
    {
      if (_guide.CanMoveNext)
      {
        _guide.MoveNext();
        return;
      }
      if (_guide.IsFirstStartTutorial) CompleteFirstStartTutorial();
    }

    private void CompleteFirstStartTutorial()
    {
      if (!_guide.IsFirstStartTutorial || !_firstStartTutorial.CompleteTutorial()) return;
      _guide.Close();
      StartRequested?.Invoke();
    }

    private void DrawInactivityReturnModal()
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.82f);
      GUI.DrawTexture(
        new Rect(0f, 0f, PlayableViewport.Width, PlayableViewport.Height),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previous;
      GUI.Box(new Rect(210f, 174f, 540f, 192f), GUIContent.none);
      GUI.Label(
        new Rect(250f, 204f, 460f, 74f),
        L("UI_INACTIVITY_RETURN_MESSAGE"),
        _styles.Status);
      if (GUI.Button(new Rect(380f, 296f, 200f, 48f), L("UI_COMMON_CONFIRM")))
      {
        InactivityAcknowledgedRequested?.Invoke();
      }
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

    private void DrawRunWon()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(L("UI_RUN_COMPLETE"), _styles.Status, GUILayout.Height(90f));
      if (GUILayout.Button(L("UI_RETURN_MAIN"), GUILayout.Height(62f)))
      {
        MainRequested?.Invoke();
      }
      GUILayout.FlexibleSpace();
    }

    private void DrawStageWon()
    {
      _presentation0124Panel.DrawStageClear(_presentationUiArtSet);
      EconomyUiRenderer.DrawStageRewards(
        new Rect(120f, 102f, 720f, 300f),
        _snapshot.LastStageBaseReward,
        _snapshot.LastStageBonusReward,
        _economyUiArtSet,
        _styles.Status);
      GUI.Label(new Rect(220f, 112f, 520f, 28f), L("UI_STAGE_REWARD_TITLE"), _styles.Heading);
      GUI.Label(
        new Rect(180f, 136f, 600f, 22f),
        L(
          "UI_STAGE_REWARD_DETAIL",
          new LocalizationArgument("base", _snapshot.LastStageBaseReward),
          new LocalizationArgument("bonus", _snapshot.LastStageBonusReward),
          new LocalizationArgument("success", _snapshot.PredictionReward.RewardSuccessCount),
          new LocalizationArgument("total", _snapshot.LastStageReward)),
        _styles.Small);
      if (GUI.Button(new Rect(360f, 430f, 240f, 54f), L("UI_COMMON_CONTINUE")))
      {
        AdvanceRequested?.Invoke();
      }
    }

    private void DrawBattleEconomyHud()
    {
      EconomyUiRenderer.DrawBattleBalances(
        new Rect(14f, 96f, 108f, 34f),
        _snapshot.BaseBulletCount,
        _snapshot.TemporaryBulletCount,
        _economyUiArtSet,
        _styles.Small);
    }

    private void DrawOpponentPortrait()
    {
      var portrait = _presentationUiArtSet?.GetOpponentPortrait(_snapshot.StageNumber);
      var rect = new Rect(872f, 10f, 72f, 72f);
      if (portrait != null)
      {
        GUI.DrawTexture(rect, portrait, ScaleMode.ScaleToFit, true);
        return;
      }
      var previous = GUI.color;
      GUI.color = new Color(0.04f, 0.035f, 0.03f, 0.9f);
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;
    }

    private void HandleHalliInput()
    {
      var halli = _snapshot.Halli;
      if (halli == null) return;
      if (halli.CanRing && Input.GetKeyDown(KeyCode.LeftArrow))
      {
        LeftBellRequested?.Invoke();
      }
      else if (halli.CanRing
        && Input.GetKeyDown(KeyCode.RightArrow))
      {
        RightBellRequested?.Invoke();
      }
      else if (halli.CanFlip
        && Pressed(KeyCode.UpArrow, KeyCode.Space))
      {
        AdvanceRequested?.Invoke();
      }
    }

    private void HandleSelectionInput()
    {
      var selection = _snapshot.Selection;
      if (selection == null || selection.WinnerCandidates.Count == 0) return;
      if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
        _selectionFocus = _selectionFocus < 0
          ? selection.WinnerCandidates.Count - 1
          : (_selectionFocus - 1 + selection.WinnerCandidates.Count)
            % selection.WinnerCandidates.Count;
      }
      else if (Input.GetKeyDown(KeyCode.RightArrow))
      {
        _selectionFocus = _selectionFocus < 0
          ? 0
          : (_selectionFocus + 1) % selection.WinnerCandidates.Count;
      }
      else if (Input.GetKeyDown(KeyCode.UpArrow) && _selectionFocus >= 0)
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
          _localization,
          _cardArtSet?.PlayerJokerTexture,
          _cardArtSet?.AiJokerTexture);
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
