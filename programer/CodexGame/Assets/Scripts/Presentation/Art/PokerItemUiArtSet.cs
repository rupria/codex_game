using System;
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
      Texture2D actionButtonDisabled = null)
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

    public Texture2D FindItemIcon(GameItemId itemId)
    {
      switch (itemId)
      {
        case GameItemId.Reload: return _reloadIcon;
        case GameItemId.BottomDeal: return _bottomDealIcon;
        case GameItemId.HypeMan: return _hypeManIcon;
        case GameItemId.HealthRecovery: return _healthRecoveryIcon;
        default: return null;
      }
    }

    private static Texture2D Require(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }
  }
}
