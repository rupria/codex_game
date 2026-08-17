using System;
using CodexGame.Core.Cards;
using CodexGame.Core.Items;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class PokerItemUiArtSet
  {
    [SerializeField] private Texture2D _crateClosed;
    [SerializeField] private Texture2D _crateOpenEmpty;
    [SerializeField] private Texture2D _crateOpenFilled;
    [SerializeField] private Texture2D _popupFrame;
    [SerializeField] private Texture2D _inventoryTray;
    [SerializeField] private Texture2D _slotIdle;
    [SerializeField] private Texture2D _slotHover;
    [SerializeField] private Texture2D _slotSelected;
    [SerializeField] private Texture2D _slotDisabled;
    [SerializeField] private Texture2D _reloadIcon;
    [SerializeField] private Texture2D _bottomDealIcon;
    [SerializeField] private Texture2D _hypeManIcon;
    [SerializeField] private Texture2D _healthRecoveryIcon;
    [SerializeField] private Texture2D _selectionPanel;
    [SerializeField] private Texture2D _detailPanel;
    [SerializeField] private Texture2D _actionButtonIdle;
    [SerializeField] private Texture2D _actionButtonHover;
    [SerializeField] private Texture2D _actionButtonDisabled;
    [SerializeField] private Texture2D _reloadPopupIcon;
    [SerializeField] private Texture2D _bottomDealPopupIcon;
    [SerializeField] private Texture2D _hypeManPopupIcon;
    [SerializeField] private Texture2D _healthRecoveryPopupIcon;
    [SerializeField] private Texture2D _communityLocked;
    [SerializeField] private Texture2D _communityReveal;
    [SerializeField] private Texture2D _communityOpen;
    [SerializeField] private Texture2D _wildInkDefault;
    [SerializeField] private Texture2D _wildInkHover;
    [SerializeField] private Texture2D _wildInkSelected;
    [SerializeField] private Texture2D _wildInkDisabled;
    [SerializeField] private Texture2D _wildInkPopup;
    [SerializeField] private Texture2D _barrelDefault;
    [SerializeField] private Texture2D _barrelHover;
    [SerializeField] private Texture2D _barrelSelected;
    [SerializeField] private Texture2D _barrelDisabled;
    [SerializeField] private Texture2D _barrelPopup;
    [SerializeField] private Texture2D _insuranceDefault;
    [SerializeField] private Texture2D _insuranceHover;
    [SerializeField] private Texture2D _insuranceSelected;
    [SerializeField] private Texture2D _insuranceDisabled;
    [SerializeField] private Texture2D _insurancePopup;
    [SerializeField] private Texture2D _mercenaryDefault;
    [SerializeField] private Texture2D _mercenaryHover;
    [SerializeField] private Texture2D _mercenarySelected;
    [SerializeField] private Texture2D _mercenaryDisabled;
    [SerializeField] private Texture2D _mercenaryPopup;
    [SerializeField] private Texture2D _wildInkSpreadSheet;
    [SerializeField] private Texture2D _wildInkSpadeSeal;
    [SerializeField] private Texture2D _wildInkHeartSeal;
    [SerializeField] private Texture2D _wildInkClubSeal;
    [SerializeField] private Texture2D _wildInkDiamondSeal;
    [SerializeField] private Texture2D _wildInkAppliedMarker;
    [SerializeField] private Texture2D _wildInkExchangeLockedMarker;
    [SerializeField] private Texture2D _barrelDefenseReady;
    [SerializeField] private Texture2D _barrelDefenseBroken;
    [SerializeField] private Texture2D _barrelDefenseBreakSheet;
    [SerializeField] private Texture2D _barrelHpPreservedMarker;
    [SerializeField] private Texture2D _insuranceApplySheet;
    [SerializeField] private Texture2D _insuranceCharges0;
    [SerializeField] private Texture2D _insuranceCharges1;
    [SerializeField] private Texture2D _insuranceCharges2;
    [SerializeField] private Texture2D _predictionActualSuccess;
    [SerializeField] private Texture2D _predictionInsuredSuccess;
    [SerializeField] private Texture2D _mercenaryExchangeSheet;
    [SerializeField] private Texture2D _mercenaryPlayerTargetMarker;
    [SerializeField] private Texture2D _mercenaryAiHiddenMarker;

    public PokerItemUiArtSet(
      Texture2D crateClosed,
      Texture2D crateOpenEmpty,
      Texture2D crateOpenFilled,
      Texture2D popupFrame,
      Texture2D inventoryTray,
      Texture2D slotIdle,
      Texture2D slotHover,
      Texture2D slotSelected,
      Texture2D slotDisabled,
      Texture2D reloadIcon,
      Texture2D bottomDealIcon,
      Texture2D hypeManIcon,
      Texture2D healthRecoveryIcon,
      Texture2D selectionPanel = null,
      Texture2D detailPanel = null,
      Texture2D actionButtonIdle = null,
      Texture2D actionButtonHover = null,
      Texture2D actionButtonDisabled = null,
      Texture2D reloadPopupIcon = null,
      Texture2D bottomDealPopupIcon = null,
      Texture2D hypeManPopupIcon = null,
      Texture2D healthRecoveryPopupIcon = null,
      Texture2D communityLocked = null,
      Texture2D communityReveal = null,
      Texture2D communityOpen = null,
      Texture2D wildInkDefault = null,
      Texture2D wildInkHover = null,
      Texture2D wildInkSelected = null,
      Texture2D wildInkDisabled = null,
      Texture2D wildInkPopup = null,
      Texture2D barrelDefault = null,
      Texture2D barrelHover = null,
      Texture2D barrelSelected = null,
      Texture2D barrelDisabled = null,
      Texture2D barrelPopup = null,
      Texture2D insuranceDefault = null,
      Texture2D insuranceHover = null,
      Texture2D insuranceSelected = null,
      Texture2D insuranceDisabled = null,
      Texture2D insurancePopup = null,
      Texture2D mercenaryDefault = null,
      Texture2D mercenaryHover = null,
      Texture2D mercenarySelected = null,
      Texture2D mercenaryDisabled = null,
      Texture2D mercenaryPopup = null,
      Texture2D wildInkSpreadSheet = null,
      Texture2D wildInkSpadeSeal = null,
      Texture2D wildInkHeartSeal = null,
      Texture2D wildInkClubSeal = null,
      Texture2D wildInkDiamondSeal = null,
      Texture2D wildInkAppliedMarker = null,
      Texture2D wildInkExchangeLockedMarker = null,
      Texture2D barrelDefenseReady = null,
      Texture2D barrelDefenseBroken = null,
      Texture2D barrelDefenseBreakSheet = null,
      Texture2D barrelHpPreservedMarker = null,
      Texture2D insuranceApplySheet = null,
      Texture2D insuranceCharges0 = null,
      Texture2D insuranceCharges1 = null,
      Texture2D insuranceCharges2 = null,
      Texture2D predictionActualSuccess = null,
      Texture2D predictionInsuredSuccess = null,
      Texture2D mercenaryExchangeSheet = null,
      Texture2D mercenaryPlayerTargetMarker = null,
      Texture2D mercenaryAiHiddenMarker = null)
    {
      _crateClosed = Require(crateClosed, nameof(crateClosed));
      _crateOpenEmpty = Require(crateOpenEmpty, nameof(crateOpenEmpty));
      _crateOpenFilled = Require(crateOpenFilled, nameof(crateOpenFilled));
      _popupFrame = Require(popupFrame, nameof(popupFrame));
      _inventoryTray = Require(inventoryTray, nameof(inventoryTray));
      _slotIdle = Require(slotIdle, nameof(slotIdle));
      _slotHover = Require(slotHover, nameof(slotHover));
      _slotSelected = Require(slotSelected, nameof(slotSelected));
      _slotDisabled = Require(slotDisabled, nameof(slotDisabled));
      _reloadIcon = Require(reloadIcon, nameof(reloadIcon));
      _bottomDealIcon = Require(bottomDealIcon, nameof(bottomDealIcon));
      _hypeManIcon = Require(hypeManIcon, nameof(hypeManIcon));
      _healthRecoveryIcon = Require(healthRecoveryIcon, nameof(healthRecoveryIcon));
      _selectionPanel = selectionPanel;
      _detailPanel = detailPanel;
      _actionButtonIdle = actionButtonIdle;
      _actionButtonHover = actionButtonHover;
      _actionButtonDisabled = actionButtonDisabled;
      _reloadPopupIcon = reloadPopupIcon;
      _bottomDealPopupIcon = bottomDealPopupIcon;
      _hypeManPopupIcon = hypeManPopupIcon;
      _healthRecoveryPopupIcon = healthRecoveryPopupIcon;
      _communityLocked = communityLocked;
      _communityReveal = communityReveal;
      _communityOpen = communityOpen;
      _wildInkDefault = wildInkDefault;
      _wildInkHover = wildInkHover;
      _wildInkSelected = wildInkSelected;
      _wildInkDisabled = wildInkDisabled;
      _wildInkPopup = wildInkPopup;
      _barrelDefault = barrelDefault;
      _barrelHover = barrelHover;
      _barrelSelected = barrelSelected;
      _barrelDisabled = barrelDisabled;
      _barrelPopup = barrelPopup;
      _insuranceDefault = insuranceDefault;
      _insuranceHover = insuranceHover;
      _insuranceSelected = insuranceSelected;
      _insuranceDisabled = insuranceDisabled;
      _insurancePopup = insurancePopup;
      _mercenaryDefault = mercenaryDefault;
      _mercenaryHover = mercenaryHover;
      _mercenarySelected = mercenarySelected;
      _mercenaryDisabled = mercenaryDisabled;
      _mercenaryPopup = mercenaryPopup;
      _wildInkSpreadSheet = wildInkSpreadSheet;
      _wildInkSpadeSeal = wildInkSpadeSeal;
      _wildInkHeartSeal = wildInkHeartSeal;
      _wildInkClubSeal = wildInkClubSeal;
      _wildInkDiamondSeal = wildInkDiamondSeal;
      _wildInkAppliedMarker = wildInkAppliedMarker;
      _wildInkExchangeLockedMarker = wildInkExchangeLockedMarker;
      _barrelDefenseReady = barrelDefenseReady;
      _barrelDefenseBroken = barrelDefenseBroken;
      _barrelDefenseBreakSheet = barrelDefenseBreakSheet;
      _barrelHpPreservedMarker = barrelHpPreservedMarker;
      _insuranceApplySheet = insuranceApplySheet;
      _insuranceCharges0 = insuranceCharges0;
      _insuranceCharges1 = insuranceCharges1;
      _insuranceCharges2 = insuranceCharges2;
      _predictionActualSuccess = predictionActualSuccess;
      _predictionInsuredSuccess = predictionInsuredSuccess;
      _mercenaryExchangeSheet = mercenaryExchangeSheet;
      _mercenaryPlayerTargetMarker = mercenaryPlayerTargetMarker;
      _mercenaryAiHiddenMarker = mercenaryAiHiddenMarker;
    }

    public Texture2D CrateClosed => _crateClosed;
    public Texture2D CrateOpenEmpty => _crateOpenEmpty;
    public Texture2D CrateOpenFilled => _crateOpenFilled;
    public Texture2D PopupFrame => _popupFrame;
    public Texture2D InventoryTray => _inventoryTray;
    public Texture2D SlotIdle => _slotIdle;
    public Texture2D SlotHover => _slotHover;
    public Texture2D SlotSelected => _slotSelected;
    public Texture2D SlotDisabled => _slotDisabled;
    public Texture2D SelectionPanel => _selectionPanel;
    public Texture2D DetailPanel => _detailPanel;
    public Texture2D ActionButtonIdle => _actionButtonIdle;
    public Texture2D ActionButtonHover => _actionButtonHover;
    public Texture2D ActionButtonDisabled => _actionButtonDisabled;
    public Texture2D CommunityLocked => _communityLocked;
    public Texture2D CommunityReveal => _communityReveal;
    public Texture2D CommunityOpen => _communityOpen;
    public Texture2D WildInkAppliedMarker => _wildInkAppliedMarker;
    public Texture2D WildInkExchangeLockedMarker => _wildInkExchangeLockedMarker;
    public Texture2D BarrelDefenseReady => _barrelDefenseReady;
    public Texture2D BarrelDefenseBroken => _barrelDefenseBroken;
    public Texture2D BarrelDefenseBreakSheet => _barrelDefenseBreakSheet;
    public Texture2D BarrelHpPreservedMarker => _barrelHpPreservedMarker;
    public Texture2D PredictionActualSuccess => _predictionActualSuccess;
    public Texture2D PredictionInsuredSuccess => _predictionInsuredSuccess;
    public Texture2D MercenaryPlayerTargetMarker => _mercenaryPlayerTargetMarker;
    public Texture2D MercenaryAiHiddenMarker => _mercenaryAiHiddenMarker;

    public Texture2D FindItemIcon(GameItemId itemId)
    {
      return FindItemIcon(itemId, PokerItemIconState.Default);
    }

    public Texture2D FindItemIcon(GameItemId itemId, PokerItemIconState state)
    {
      switch (itemId)
      {
        case GameItemId.Reload: return _reloadIcon;
        case GameItemId.BottomDeal: return _bottomDealIcon;
        case GameItemId.HypeMan: return _hypeManIcon;
        case GameItemId.HealthRecovery: return _healthRecoveryIcon;
        case GameItemId.WildInk:
          return SelectState(state, _wildInkDefault, _wildInkHover, _wildInkSelected, _wildInkDisabled);
        case GameItemId.Barrel:
          return SelectState(state, _barrelDefault, _barrelHover, _barrelSelected, _barrelDisabled);
        case GameItemId.PredictionInsurance:
          return SelectState(state, _insuranceDefault, _insuranceHover, _insuranceSelected, _insuranceDisabled);
        case GameItemId.Mercenary:
          return SelectState(state, _mercenaryDefault, _mercenaryHover, _mercenarySelected, _mercenaryDisabled);
        default: return null;
      }
    }

    public Texture2D FindPopupIcon(GameItemId itemId)
    {
      switch (itemId)
      {
        case GameItemId.Reload: return _reloadPopupIcon ?? _reloadIcon;
        case GameItemId.BottomDeal: return _bottomDealPopupIcon ?? _bottomDealIcon;
        case GameItemId.HypeMan: return _hypeManPopupIcon ?? _hypeManIcon;
        case GameItemId.HealthRecovery: return _healthRecoveryPopupIcon ?? _healthRecoveryIcon;
        case GameItemId.WildInk: return _wildInkPopup ?? _wildInkDefault;
        case GameItemId.Barrel: return _barrelPopup ?? _barrelDefault;
        case GameItemId.PredictionInsurance: return _insurancePopup ?? _insuranceDefault;
        case GameItemId.Mercenary: return _mercenaryPopup ?? _mercenaryDefault;
        default: return null;
      }
    }

    public Texture2D FindWildInkSuitSeal(CardSuit suit)
    {
      switch (suit)
      {
        case CardSuit.Spades: return _wildInkSpadeSeal;
        case CardSuit.Hearts: return _wildInkHeartSeal;
        case CardSuit.Clubs: return _wildInkClubSeal;
        case CardSuit.Diamonds: return _wildInkDiamondSeal;
        default: return null;
      }
    }

    public Texture2D FindInsuranceCharges(int charges)
    {
      return charges >= 2 ? _insuranceCharges2 : charges == 1 ? _insuranceCharges1 : _insuranceCharges0;
    }

    public Texture2D FindUseAnimationSheet(GameItemId itemId)
    {
      switch (itemId)
      {
        case GameItemId.WildInk: return _wildInkSpreadSheet;
        case GameItemId.PredictionInsurance: return _insuranceApplySheet;
        case GameItemId.Mercenary: return _mercenaryExchangeSheet;
        default: return null;
      }
    }

    public int FindUseAnimationFrameCount(GameItemId itemId)
    {
      switch (itemId)
      {
        case GameItemId.WildInk: return 8;
        case GameItemId.PredictionInsurance: return 6;
        case GameItemId.Mercenary: return 10;
        default: return 0;
      }
    }

    private static Texture2D SelectState(
      PokerItemIconState state,
      Texture2D defaultIcon,
      Texture2D hoverIcon,
      Texture2D selectedIcon,
      Texture2D disabledIcon)
    {
      switch (state)
      {
        case PokerItemIconState.Hover: return hoverIcon ?? defaultIcon;
        case PokerItemIconState.Selected: return selectedIcon ?? defaultIcon;
        case PokerItemIconState.Disabled: return disabledIcon ?? defaultIcon;
        default: return defaultIcon;
      }
    }

    private static Texture2D Require(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }
  }

  public enum PokerItemIconState
  {
    Default = 0,
    Hover = 1,
    Selected = 2,
    Disabled = 3
  }
}
