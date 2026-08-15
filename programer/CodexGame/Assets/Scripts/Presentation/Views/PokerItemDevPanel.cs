using System;
using CodexGame.Application.Items;
using CodexGame.Core.Cards;
using CodexGame.Core.Items;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PokerItemDevPanel
  {
    private static readonly Rect FullScreen = new Rect(0f, 0f, 960f, 540f);
    private static readonly Rect CrateRect = new Rect(760f, 326f, 128f, 128f);
    private static readonly Rect PopupRect = new Rect(160f, 102f, 640f, 336f);
    private static readonly Rect CloseButton = new Rect(754f, 114f, 32f, 32f);
    private static readonly Rect DetailRect = new Rect(404f, 236f, 376f, 112f);
    private static readonly Rect UseButton = new Rect(410f, 372f, 172f, 44f);
    private static readonly Rect ConfirmButton = new Rect(598f, 372f, 172f, 44f);

    private GameItemId? _selectedItem;
    private CardId? _selectedTargetCard;
    private bool _inventoryOpen;

    public void Draw(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      PokerItemUiArtSet art,
      LocalizationRuntime localization,
      Action<CardId> reload,
      Action<CardId> beginBottomDeal,
      Action<CardId> chooseBottomDeal,
      Action hypeMan,
      Action healthRecovery,
      Action confirm)
    {
      if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
      SyncSelection(snapshot);

      GUI.Label(new Rect(56f, 24f, 848f, 38f), localization.Get("UI_ITEM_WINDOW_TITLE"), styles.Title);
      DrawTableCards(snapshot, cards, styles, localization);

      var canAct = snapshot.Phase == PokerItemPhase.AwaitingActions;
      DrawClosedCrate(
        snapshot.Inventory.Count,
        canAct && !_inventoryOpen,
        art,
        styles,
        localization);

      GUI.enabled = canAct && !_inventoryOpen;
      if (GUI.Button(new Rect(734f, 472f, 170f, 42f), localization.Get("UI_ITEM_CONFIRM_HAND")))
      {
        ClearSelection();
        confirm();
      }
      GUI.enabled = true;

      if (snapshot.Phase == PokerItemPhase.AwaitingBottomDealChoice)
      {
        DrawBottomDealCandidates(snapshot, cards, styles, localization, chooseBottomDeal);
        return;
      }

      if (_inventoryOpen)
      {
        DrawInventoryModal(
          snapshot,
          cards,
          art,
          styles,
          localization,
          reload,
          beginBottomDeal,
          hypeMan,
          healthRecovery,
          confirm);
      }

      if (snapshot.LastFailure != PokerItemFailure.None)
      {
        GUI.Label(
          new Rect(155f, 468f, 540f, 28f),
          localization.Get("UI_ITEM_ACTION_FAILED") + ": " + snapshot.LastFailure,
          styles.Small);
      }
    }

    private void DrawClosedCrate(
      int inventoryCount,
      bool enabled,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      var texture = art?.CrateClosed;
      if (texture != null) GUI.DrawTexture(CrateRect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(CrateRect, GUIContent.none);
      GUI.Label(
        new Rect(CrateRect.x - 26f, CrateRect.y + CrateRect.height - 6f, 180f, 24f),
        localization.Get("UI_ITEM_INVENTORY") + "  " + inventoryCount + "/4",
        styles.Small);

      GUI.enabled = enabled;
      if (GUI.Button(CrateRect, GUIContent.none, GUIStyle.none))
      {
        _inventoryOpen = true;
        ClearSelection();
      }
      GUI.enabled = true;
    }

    private static void DrawTableCards(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      GUI.Label(new Rect(56f, 82f, 180f, 26f), localization.Get("UI_POKER_AI_HAND"), styles.Heading);
      if (snapshot.VisibleAiPrivateCards.Count == 0)
      {
        for (var index = 0; index < 3; index++)
        {
          cards.DrawBackAt(new Rect(382f + index * 66f, 74f, 56f, 78f), 180f);
        }
      }
      else
      {
        for (var index = 0; index < snapshot.VisibleAiPrivateCards.Count; index++)
        {
          cards.DrawAt(new Rect(382f + index * 66f, 74f, 56f, 78f), snapshot.VisibleAiPrivateCards[index]);
        }
      }

      GUI.Label(new Rect(56f, 206f, 180f, 26f), localization.Get("UI_POKER_PUBLIC"), styles.Heading);
      for (var index = 0; index < snapshot.PublicCards.Count; index++)
      {
        cards.DrawAt(new Rect(416f + index * 66f, 188f, 56f, 78f), snapshot.PublicCards[index]);
      }

      GUI.Label(new Rect(56f, 346f, 220f, 26f), localization.Get("UI_POKER_PLAYER_PRIVATE"), styles.Heading);
      for (var index = 0; index < snapshot.PlayerPrivateCards.Count; index++)
      {
        cards.DrawAt(
          new Rect(382f + index * 78f, 328f, 64f, 90f),
          snapshot.PlayerPrivateCards[index]);
      }
    }

    private void DrawInventoryModal(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<CardId> reload,
      Action<CardId> beginBottomDeal,
      Action hypeMan,
      Action healthRecovery,
      Action confirm)
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.74f);
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;

      var panel = art?.SelectionPanel ?? art?.PopupFrame;
      if (panel != null) GUI.DrawTexture(PopupRect, panel, ScaleMode.StretchToFill, true);
      else GUI.Box(PopupRect, GUIContent.none);
      GUI.Label(new Rect(388f, 118f, 390f, 32f), localization.Get("UI_ITEM_INVENTORY"), styles.Heading);
      if (GUI.Button(CloseButton, "X"))
      {
        CloseInventoryModal();
        return;
      }

      var crate = snapshot.Inventory.Count == 0 ? art?.CrateOpenEmpty : art?.CrateOpenFilled;
      if (crate != null)
      {
        GUI.DrawTexture(new Rect(184f, 154f, 190f, 190f), crate, ScaleMode.ScaleToFit, true);
      }

      for (var index = 0; index < 4; index++)
      {
        DrawInventorySlot(
          snapshot,
          index,
          new Rect(408f + index * 80f, 158f, 64f, 64f),
          art,
          styles,
          localization);
      }

      DrawItemDetail(snapshot, cards, art, styles, localization);
      var canUse = CanUseSelected();
      if (DrawActionButton(
        UseButton,
        localization.Get("UI_COMMON_SELECT"),
        canUse,
        art,
        styles.Heading))
      {
        UseSelectedItem(reload, beginBottomDeal, hypeMan, healthRecovery);
      }
      if (DrawActionButton(
        ConfirmButton,
        localization.Get("UI_ITEM_CONFIRM_HAND"),
        true,
        art,
        styles.Heading))
      {
        CloseInventoryModal();
        confirm();
      }
    }

    private void DrawInventorySlot(
      PokerItemSnapshot snapshot,
      int index,
      Rect rect,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      var occupied = index < snapshot.Inventory.Count;
      var itemId = occupied ? snapshot.Inventory[index] : default;
      var selected = occupied && _selectedItem == itemId;
      var hovered = occupied && rect.Contains(Event.current.mousePosition);
      var texture = !occupied
        ? art?.SlotDisabled
        : selected
          ? art?.SlotSelected
          : hovered
            ? art?.SlotHover
            : art?.SlotIdle;
      if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(rect, GUIContent.none);
      if (!occupied) return;

      var icon = art?.FindItemIcon(itemId);
      if (icon != null)
      {
        GUI.DrawTexture(
          new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f),
          icon,
          ScaleMode.ScaleToFit,
          true);
      }
      if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
      {
        _selectedItem = itemId;
        _selectedTargetCard = null;
      }

      if (GameItemCatalog.TryGet(itemId, out var definition) && definition != null)
      {
        GUI.Label(
          new Rect(rect.x - 8f, rect.y + 66f, rect.width + 16f, 18f),
          localization.Get(definition.LocalizationNameKey),
          styles.Small);
      }
    }

    private void DrawItemDetail(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (art?.DetailPanel != null)
      {
        GUI.DrawTexture(DetailRect, art.DetailPanel, ScaleMode.StretchToFill, true);
      }
      else GUI.Box(DetailRect, GUIContent.none);

      if (!_selectedItem.HasValue)
      {
        GUI.Label(DetailRect, localization.Get("UI_ITEM_EMPTY_SLOT"), styles.Small);
        return;
      }

      if (RequiresTarget(_selectedItem.Value))
      {
        for (var index = 0; index < snapshot.PlayerPrivateCards.Count; index++)
        {
          var card = snapshot.PlayerPrivateCards[index];
          var rect = new Rect(438f + index * 92f, 252f, 56f, 78f);
          cards.DrawAt(rect, card);
          if (_selectedTargetCard.HasValue && _selectedTargetCard.Value == card.Id)
          {
            var color = GUI.color;
            GUI.color = new Color(0.08f, 0.9f, 0.9f, 0.32f);
            GUI.DrawTexture(
              new Rect(rect.x - 4f, rect.y - 4f, rect.width + 8f, rect.height + 8f),
              Texture2D.whiteTexture,
              ScaleMode.StretchToFill,
              true);
            GUI.color = color;
          }
          if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) _selectedTargetCard = card.Id;
        }
        return;
      }

      if (GameItemCatalog.TryGet(_selectedItem.Value, out var definition) && definition != null)
      {
        var icon = art?.FindItemIcon(_selectedItem.Value);
        if (icon != null)
        {
          GUI.DrawTexture(new Rect(416f, 252f, 80f, 80f), icon, ScaleMode.ScaleToFit, true);
        }
        GUI.Label(
          new Rect(510f, 254f, 250f, 32f),
          localization.Get(definition.LocalizationNameKey),
          styles.Heading);
        GUI.Label(
          new Rect(510f, 292f, 250f, 34f),
          localization.Get("UI_ITEM_WINDOW_TITLE"),
          styles.Small);
      }
    }

    private bool CanUseSelected()
    {
      if (!_selectedItem.HasValue) return false;
      return !RequiresTarget(_selectedItem.Value) || _selectedTargetCard.HasValue;
    }

    private void UseSelectedItem(
      Action<CardId> reload,
      Action<CardId> beginBottomDeal,
      Action hypeMan,
      Action healthRecovery)
    {
      if (!_selectedItem.HasValue) return;
      var item = _selectedItem.Value;
      if (item == GameItemId.Reload && _selectedTargetCard.HasValue)
      {
        reload(_selectedTargetCard.Value);
      }
      else if (item == GameItemId.BottomDeal && _selectedTargetCard.HasValue)
      {
        beginBottomDeal(_selectedTargetCard.Value);
        _inventoryOpen = false;
      }
      else if (item == GameItemId.HypeMan) hypeMan();
      else if (item == GameItemId.HealthRecovery) healthRecovery();
      ClearSelection();
    }

    private static bool RequiresTarget(GameItemId itemId)
    {
      return itemId == GameItemId.Reload || itemId == GameItemId.BottomDeal;
    }

    private static bool DrawActionButton(
      Rect rect,
      string label,
      bool enabled,
      PokerItemUiArtSet art,
      GUIStyle style)
    {
      var hovered = enabled && rect.Contains(Event.current.mousePosition);
      var texture = !enabled
        ? art?.ActionButtonDisabled
        : hovered
          ? art?.ActionButtonHover
          : art?.ActionButtonIdle;
      GUI.enabled = enabled;
      bool clicked;
      if (texture != null)
      {
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        GUI.Label(rect, label, style);
        clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
      }
      else clicked = GUI.Button(rect, label);
      GUI.enabled = true;
      return clicked;
    }

    private void SyncSelection(PokerItemSnapshot snapshot)
    {
      if (!_selectedItem.HasValue) return;
      for (var index = 0; index < snapshot.Inventory.Count; index++)
      {
        if (snapshot.Inventory[index] == _selectedItem.Value) return;
      }
      ClearSelection();
      if (snapshot.Inventory.Count == 0) _inventoryOpen = false;
    }

    private void ClearSelection()
    {
      _selectedItem = null;
      _selectedTargetCard = null;
    }

    private void CloseInventoryModal()
    {
      _inventoryOpen = false;
      ClearSelection();
    }

    private static void DrawBottomDealCandidates(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<CardId> chooseBottomDeal)
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.74f);
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;
      GUI.Box(new Rect(280f, 132f, 400f, 270f), GUIContent.none);
      GUI.Label(new Rect(330f, 154f, 300f, 34f), localization.Get("UI_ITEM_CHOOSE_CARD"), styles.Heading);
      for (var index = 0; index < snapshot.BottomDealCandidates.Count; index++)
      {
        var rect = new Rect(364f + index * 116f, 220f, 80f, 112f);
        cards.DrawAt(rect, snapshot.BottomDealCandidates[index]);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
          chooseBottomDeal(snapshot.BottomDealCandidates[index].Id);
        }
      }
    }
  }
}
