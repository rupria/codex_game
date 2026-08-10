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
    private static readonly Rect CrateRect = new Rect(760f, 326f, 128f, 128f);
    private static readonly Rect PopupRect = new Rect(200f, 120f, 560f, 300f);
    private GameItemId? _selectedTargetItem;
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

      GUI.Label(new Rect(56f, 24f, 848f, 38f), localization.Get("UI_ITEM_WINDOW_TITLE"), styles.Title);
      DrawTableCards(snapshot, cards, styles, localization, reload, beginBottomDeal);

      var canAct = snapshot.Phase == PokerItemPhase.AwaitingActions;
      DrawClosedCrate(snapshot.Inventory.Count, canAct, art, styles, localization);

      GUI.enabled = canAct && !_inventoryOpen && !_selectedTargetItem.HasValue;
      if (GUI.Button(new Rect(734f, 472f, 170f, 42f), localization.Get("UI_ITEM_CONFIRM_HAND")))
      {
        _selectedTargetItem = null;
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
        DrawInventoryModal(snapshot, art, styles, localization, hypeMan, healthRecovery);
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
      if (GUI.Button(CrateRect, GUIContent.none, GUIStyle.none)) _inventoryOpen = true;
      GUI.enabled = true;
    }

    private void DrawTableCards(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<CardId> reload,
      Action<CardId> beginBottomDeal)
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
        var rect = new Rect(382f + index * 78f, 328f, 64f, 90f);
        cards.DrawAt(rect, snapshot.PlayerPrivateCards[index]);
        if (_selectedTargetItem.HasValue
          && snapshot.Phase == PokerItemPhase.AwaitingActions)
        {
          var previous = GUI.color;
          GUI.color = new Color(0.1f, 0.9f, 0.9f, 0.25f);
          GUI.DrawTexture(
            new Rect(rect.x - 4f, rect.y - 4f, rect.width + 8f, rect.height + 8f),
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill,
            true);
          GUI.color = previous;
          if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
          {
            if (_selectedTargetItem == GameItemId.Reload) reload(snapshot.PlayerPrivateCards[index].Id);
            else if (_selectedTargetItem == GameItemId.BottomDeal)
            {
              beginBottomDeal(snapshot.PlayerPrivateCards[index].Id);
            }
            _selectedTargetItem = null;
          }
        }
      }

      if (_selectedTargetItem.HasValue)
      {
        GUI.Label(new Rect(310f, 430f, 420f, 28f), localization.Get("UI_ITEM_CHOOSE_CARD"), styles.Heading);
      }
    }

    private void DrawInventoryModal(
      PokerItemSnapshot snapshot,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action hypeMan,
      Action healthRecovery)
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 168f / 255f);
      GUI.DrawTexture(new Rect(0f, 0f, 960f, 540f), Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;

      if (art?.PopupFrame != null) GUI.DrawTexture(PopupRect, art.PopupFrame, ScaleMode.StretchToFill, true);
      else GUI.Box(PopupRect, GUIContent.none);
      GUI.Label(new Rect(230f, 142f, 460f, 34f), localization.Get("UI_ITEM_INVENTORY"), styles.Heading);

      var crate = snapshot.Inventory.Count == 0 ? art?.CrateOpenEmpty : art?.CrateOpenFilled;
      var openCrateRect = new Rect(218f, 188f, 132f, 132f);
      if (crate != null) GUI.DrawTexture(openCrateRect, crate, ScaleMode.ScaleToFit, true);

      var trayRect = new Rect(350f, 192f, 388f, 92f);
      if (art?.InventoryTray != null) GUI.DrawTexture(trayRect, art.InventoryTray, ScaleMode.StretchToFill, true);

      for (var index = 0; index < 4; index++)
      {
        var rect = new Rect(362f + index * 91f, 202f, 72f, 72f);
        DrawInventorySlot(snapshot, index, rect, art, styles, localization, hypeMan, healthRecovery);
      }

      GUI.Label(
        new Rect(354f, 310f, 376f, 48f),
        snapshot.Inventory.Count == 0
          ? localization.Get("UI_ITEM_EMPTY_SLOT")
          : localization.Get("UI_ITEM_WINDOW_TITLE"),
        styles.Small);
      if (GUI.Button(new Rect(700f, 140f, 38f, 32f), "X")) _inventoryOpen = false;
    }

    private void DrawInventorySlot(
      PokerItemSnapshot snapshot,
      int index,
      Rect rect,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action hypeMan,
      Action healthRecovery)
    {
      var occupied = index < snapshot.Inventory.Count;
      var hovered = occupied && rect.Contains(Event.current.mousePosition);
      var texture = !occupied
        ? art?.SlotDisabled
        : hovered
          ? art?.SlotHover
          : art?.SlotIdle;
      if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(rect, GUIContent.none);
      if (!occupied) return;

      var itemId = snapshot.Inventory[index];
      var icon = art?.FindItemIcon(itemId);
      if (icon != null)
      {
        GUI.DrawTexture(
          new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f),
          icon,
          ScaleMode.ScaleToFit,
          true);
      }

      GUI.enabled = snapshot.Phase == PokerItemPhase.AwaitingActions;
      if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
      {
        if (itemId == GameItemId.Reload || itemId == GameItemId.BottomDeal)
        {
          _selectedTargetItem = itemId;
          _inventoryOpen = false;
        }
        else if (itemId == GameItemId.HypeMan) hypeMan();
        else if (itemId == GameItemId.HealthRecovery) healthRecovery();
      }
      GUI.enabled = true;

      if (GameItemCatalog.TryGet(itemId, out var definition) && definition != null)
      {
        GUI.Label(
          new Rect(rect.x - 8f, rect.y + 74f, rect.width + 16f, 18f),
          localization.Get(definition.LocalizationNameKey),
          styles.Small);
      }
    }

    private static void DrawBottomDealCandidates(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<CardId> chooseBottomDeal)
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.62f);
      GUI.DrawTexture(new Rect(0f, 0f, 960f, 540f), Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
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
