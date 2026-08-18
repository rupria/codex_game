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
    private CardSuit? _selectedSuit;
    private bool _inventoryOpen;
    private bool _targetSelectionCommitted;

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
      Action<CardId, CardSuit> wildInk,
      Action barrel,
      Action predictionInsurance,
      Action<CardId> mercenary,
      Action confirm)
    {
      if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
      SyncSelection(snapshot);

      GUI.Label(new Rect(56f, 24f, 848f, 38f), localization.Get("UI_ITEM_WINDOW_TITLE"), styles.Title);
      if (snapshot.Phase == PokerItemPhase.AwaitingActions)
      {
        var seconds = Math.Ceiling(snapshot.HandConfirmationRemainingMicroseconds / 1_000_000d);
        GUI.Label(
          new Rect(700f, 70f, 210f, 26f),
          localization.Get(
            "UI_ITEM_CONFIRM_TIMER",
            new LocalizationArgument("seconds", seconds.ToString("0"))),
          styles.Small);
      }
      DrawTableCards(snapshot, cards, art, styles, localization);
      DrawPreparedEffects(snapshot, art, styles, localization);

      var stageLimitExhausted = snapshot.StageRestriction?.IsExhausted == true;
      var canAct = snapshot.Phase == PokerItemPhase.AwaitingActions
        && HasUsableItemAtCurrentTiming(snapshot)
        && !stageLimitExhausted
        && !snapshot.UsePresentation.IsActive;
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
        DrawBottomDealCandidates(
          snapshot,
          cards,
          styles,
          localization,
          chooseBottomDeal);
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
          wildInk,
          barrel,
          predictionInsurance,
          mercenary,
          confirm);
      }

      if (snapshot.UsePresentation.IsActive)
      {
        DrawItemUsePresentation(snapshot.UsePresentation, art, styles, localization);
      }

      if (snapshot.LastFailure != PokerItemFailure.None)
      {
        GUI.Label(
          new Rect(155f, 468f, 540f, 28f),
          FailureMessage(snapshot.LastFailure, _selectedItem, localization),
          styles.Small);
      }
    }

    private static void DrawPreparedEffects(
      PokerItemSnapshot snapshot,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot.BarrelDefenseArmed && art?.BarrelDefenseReady != null)
      {
        GUI.DrawTexture(
          new Rect(796f, 250f, 64f, 64f),
          art.BarrelDefenseReady,
          ScaleMode.ScaleToFit,
          true);
      }
      if (snapshot.InsuranceActivated)
      {
        var marker = art?.FindInsuranceCharges(2);
        if (marker != null)
        {
          GUI.DrawTexture(new Rect(866f, 266f, 32f, 32f), marker, ScaleMode.ScaleToFit, true);
        }
        GUI.Label(
          new Rect(708f, 302f, 190f, 22f),
          localization.Get("UI_ITEM_INSURANCE_APPLIED"),
          styles.Small);
      }
    }

    private static string FailureMessage(
      PokerItemFailure failure,
      GameItemId? selectedItem,
      LocalizationRuntime localization)
    {
      var key = failure == PokerItemFailure.WrongPhase
          && selectedItem.HasValue
          && GameItemCatalog.TryGet(selectedItem.Value, out var definition)
          && definition != null
        ? GameItemUseTimingPolicy.LocalizationKey(definition.UseTiming)
        : failure == PokerItemFailure.CardExchangeLocked
        ? "UI_ITEM_EXCHANGE_LOCK_AFTER_INK"
        : failure == PokerItemFailure.NoValidReplacementPair
          ? "UI_ITEM_NO_VALID_REPLACEMENT_PAIR"
          : failure == PokerItemFailure.EffectAlreadyActive
            ? "UI_ITEM_INSURANCE_APPLIED"
            : "UI_ITEM_ACTION_FAILED";
      return localization.Get(key);
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
      PokerItemUiArtSet art,
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
      if (snapshot.PublicCards.Count >= 2 && art?.CommunityOpen != null)
      {
        GUI.DrawTexture(new Rect(548f, 209f, 36f, 36f), art.CommunityOpen, ScaleMode.ScaleToFit, true);
      }

      GUI.Label(new Rect(56f, 346f, 220f, 26f), localization.Get("UI_POKER_PLAYER_PRIVATE"), styles.Heading);
      for (var index = 0; index < snapshot.PlayerPrivateCards.Count; index++)
      {
        var rect = new Rect(382f + index * 78f, 328f, 64f, 90f);
        var card = snapshot.PlayerPrivateCards[index];
        cards.DrawAt(rect, card);
        PokerItemCardStateRenderer.DrawWildInkState(
          rect,
          card,
          snapshot.WildInkCardId.HasValue,
          art);
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
      Action<CardId, CardSuit> wildInk,
      Action barrel,
      Action predictionInsurance,
      Action<CardId> mercenary,
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
      if (!_targetSelectionCommitted && GUI.Button(CloseButton, "X"))
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
      var stageLimitExhausted = snapshot.StageRestriction?.IsExhausted == true;
      var selectedRequiresTarget = _selectedItem.HasValue && RequiresTarget(_selectedItem.Value);
      var canUse = !stageLimitExhausted
        && !snapshot.UsePresentation.IsActive
        && (selectedRequiresTarget && !_targetSelectionCommitted
          ? !IsItemDisabled(snapshot, _selectedItem!.Value)
          : CanUseSelected(snapshot));
      if (DrawActionButton(
        UseButton,
        localization.Get(
          selectedRequiresTarget && !_targetSelectionCommitted
            ? "UI_COMMON_CONFIRM"
            : "UI_COMMON_SELECT"),
        canUse,
        art,
        styles.Heading))
      {
        if (selectedRequiresTarget && !_targetSelectionCommitted)
        {
          _targetSelectionCommitted = true;
          _selectedTargetCard = null;
          _selectedSuit = null;
        }
        else
        {
          UseSelectedItem(
            reload,
            beginBottomDeal,
            hypeMan,
            healthRecovery,
            wildInk,
            barrel,
            predictionInsurance,
            mercenary);
        }
      }
      if (DrawActionButton(
        ConfirmButton,
        localization.Get("UI_ITEM_CONFIRM_HAND"),
        !_targetSelectionCommitted,
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
      var itemDisabled = occupied && IsItemDisabled(snapshot, itemId);
      var texture = !occupied || itemDisabled
        ? art?.SlotDisabled
        : selected
          ? art?.SlotSelected
          : hovered
            ? art?.SlotHover
            : art?.SlotIdle;
      if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(rect, GUIContent.none);
      if (!occupied) return;

      var iconState = itemDisabled
        ? PokerItemIconState.Disabled
        : selected
          ? PokerItemIconState.Selected
          : hovered
            ? PokerItemIconState.Hover
            : PokerItemIconState.Default;
      var icon = art?.FindItemIcon(itemId, iconState);
      if (icon != null)
      {
        GUI.DrawTexture(
          new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f),
          icon,
          ScaleMode.ScaleToFit,
          true);
      }
      GUI.enabled = !itemDisabled && !_targetSelectionCommitted;
      if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
      {
        _selectedItem = itemId;
        _selectedTargetCard = null;
        _selectedSuit = null;
        _targetSelectionCommitted = false;
      }
      GUI.enabled = true;

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

      if (RequiresTarget(_selectedItem.Value) && _targetSelectionCommitted)
      {
        var popupIcon = art?.FindPopupIcon(_selectedItem.Value);
        if (popupIcon != null)
        {
          GUI.DrawTexture(new Rect(410f, 252f, 80f, 80f), popupIcon, ScaleMode.ScaleToFit, true);
        }

        for (var index = 0; index < snapshot.PlayerPrivateCards.Count; index++)
        {
          var card = snapshot.PlayerPrivateCards[index];
          var rect = new Rect(500f + index * 64f, 252f, 56f, 78f);
          var targetEnabled = IsTargetEnabled(snapshot, _selectedItem.Value, card);
          var previousCardColor = GUI.color;
          if (!targetEnabled) GUI.color = new Color(0.45f, 0.45f, 0.45f, 0.78f);
          cards.DrawAt(rect, card);
          GUI.color = previousCardColor;
          if (_selectedTargetCard.HasValue && _selectedTargetCard.Value == card.Id)
          {
            if (_selectedItem.Value == GameItemId.Mercenary
              && art?.MercenaryPlayerTargetMarker != null)
            {
              PokerItemCardStateRenderer.DrawMercenaryTarget(rect, art);
            }
            else
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
          }
          GUI.enabled = targetEnabled;
          if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) _selectedTargetCard = card.Id;
          GUI.enabled = true;
        }
        if (_selectedItem.Value == GameItemId.WildInk)
        {
          DrawSuitChoices(snapshot, art);
        }
        else if (_selectedItem.Value == GameItemId.Mercenary)
        {
          DrawMercenaryAiHiddenArea(cards, art);
        }
        DrawUseTiming(_selectedItem.Value, localization, styles);
        return;
      }

      if (GameItemCatalog.TryGet(_selectedItem.Value, out var definition) && definition != null)
      {
        var icon = art?.FindPopupIcon(_selectedItem.Value);
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
          localization.Get(definition.LocalizationDescriptionKey),
          styles.Small);
        DrawUseTiming(_selectedItem.Value, localization, styles);
      }
    }

    private bool CanUseSelected(PokerItemSnapshot snapshot)
    {
      if (!_selectedItem.HasValue) return false;
      if (IsItemDisabled(snapshot, _selectedItem.Value)) return false;
      if (RequiresTarget(_selectedItem.Value) && !_selectedTargetCard.HasValue) return false;
      if (_selectedTargetCard.HasValue)
      {
        var targetIndex = FindCard(snapshot.PlayerPrivateCards, _selectedTargetCard.Value);
        if (targetIndex < 0
          || !IsTargetEnabled(snapshot, _selectedItem.Value, snapshot.PlayerPrivateCards[targetIndex]))
        {
          return false;
        }
      }
      if (_selectedItem.Value != GameItemId.WildInk) return true;
      if (!_selectedSuit.HasValue || !_selectedTargetCard.HasValue) return false;
      var wildTargetIndex = FindCard(snapshot.PlayerPrivateCards, _selectedTargetCard.Value);
      return wildTargetIndex >= 0
        && snapshot.PlayerPrivateCards[wildTargetIndex].EffectiveSuit != _selectedSuit.Value;
    }

    private void UseSelectedItem(
      Action<CardId> reload,
      Action<CardId> beginBottomDeal,
      Action hypeMan,
      Action healthRecovery,
      Action<CardId, CardSuit> wildInk,
      Action barrel,
      Action predictionInsurance,
      Action<CardId> mercenary)
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
      else if (item == GameItemId.WildInk
        && _selectedTargetCard.HasValue
        && _selectedSuit.HasValue)
      {
        wildInk(_selectedTargetCard.Value, _selectedSuit.Value);
      }
      else if (item == GameItemId.Barrel) barrel();
      else if (item == GameItemId.PredictionInsurance) predictionInsurance();
      else if (item == GameItemId.Mercenary && _selectedTargetCard.HasValue)
      {
        mercenary(_selectedTargetCard.Value);
      }
      ClearSelection();
    }

    private static bool RequiresTarget(GameItemId itemId)
    {
      return itemId == GameItemId.Reload
        || itemId == GameItemId.BottomDeal
        || itemId == GameItemId.WildInk
        || itemId == GameItemId.Mercenary;
    }

    private static bool IsItemDisabled(PokerItemSnapshot snapshot, GameItemId itemId)
    {
      if (!GameItemCatalog.TryGet(itemId, out var definition)
        || definition == null
        || !GameItemUseTimingPolicy.IsUsable(definition, snapshot.CurrentUseTiming)) return true;
      if (snapshot.StageRestriction?.IsExhausted == true) return true;
      if (itemId == GameItemId.HealthRecovery && !snapshot.CanRecoverHealth) return true;
      if (snapshot.WildInkCardId.HasValue
        && (itemId == GameItemId.Reload
          || itemId == GameItemId.BottomDeal
          || itemId == GameItemId.Mercenary)) return true;
      return itemId == GameItemId.Mercenary
        && snapshot.MercenaryEligibleTargets.Count == 0;
    }

    private static bool HasUsableItemAtCurrentTiming(PokerItemSnapshot snapshot)
    {
      for (var index = 0; index < snapshot.Inventory.Count; index++)
      {
        if (GameItemCatalog.TryGet(snapshot.Inventory[index], out var definition)
          && definition != null
          && GameItemUseTimingPolicy.IsUsable(definition, snapshot.CurrentUseTiming)) return true;
      }
      return false;
    }

    private static void DrawUseTiming(
      GameItemId itemId,
      LocalizationRuntime localization,
      PlayableDevStyles styles)
    {
      if (!GameItemCatalog.TryGet(itemId, out var definition) || definition == null) return;
      GUI.Label(
        new Rect(410f, 346f, 360f, 20f),
        localization.Get(GameItemUseTimingPolicy.LocalizationKey(definition.UseTiming)),
        styles.Small);
    }

    private static bool IsTargetEnabled(
      PokerItemSnapshot snapshot,
      GameItemId itemId,
      Card card)
    {
      if ((itemId == GameItemId.WildInk || itemId == GameItemId.Mercenary) && card.IsJoker)
      {
        return false;
      }
      if (itemId != GameItemId.Mercenary) return true;
      for (var index = 0; index < snapshot.MercenaryEligibleTargets.Count; index++)
      {
        if (snapshot.MercenaryEligibleTargets[index] == card.Id) return true;
      }
      return false;
    }

    private static int FindCard(System.Collections.Generic.IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == id) return index;
      }
      return -1;
    }

    private static void DrawMercenaryAiHiddenArea(
      PlayableCardRenderer cards,
      PokerItemUiArtSet art)
    {
      var hiddenCard = new Rect(704f, 252f, 56f, 78f);
      for (var index = 0; index < 3; index++)
      {
        cards.DrawBackAt(
          new Rect(hiddenCard.x + index * 3f, hiddenCard.y - index * 2f, hiddenCard.width, hiddenCard.height),
          180f);
      }
      if (art?.MercenaryAiHiddenMarker != null)
      {
        GUI.DrawTexture(
          new Rect(hiddenCard.x + 18f, hiddenCard.y + 23f, 32f, 32f),
          art.MercenaryAiHiddenMarker,
          ScaleMode.ScaleToFit,
          true);
      }
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
      _selectedSuit = null;
      _targetSelectionCommitted = false;
    }

    private void CloseInventoryModal()
    {
      if (_targetSelectionCommitted) return;
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
      GUI.Label(
        new Rect(320f, 352f, 320f, 36f),
        localization.Get(
          "UI_ITEM_CONFIRM_TIMER",
          new LocalizationArgument(
            "seconds",
            Math.Ceiling(snapshot.HandConfirmationRemainingMicroseconds / 1_000_000d)
              .ToString("0"))),
        styles.Small);
    }

    private static void DrawItemUsePresentation(
      ItemUsePresentationSnapshot presentation,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (!presentation.IsActive || !presentation.ItemId.HasValue) return;
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.58f);
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;

      var pulse = 1f + Mathf.Sin(presentation.Progress * Mathf.PI) * 0.16f;
      var size = 96f * pulse;
      var sheet = art?.FindUseAnimationSheet(presentation.ItemId.Value);
      var frameCount = art?.FindUseAnimationFrameCount(presentation.ItemId.Value) ?? 0;
      if (sheet != null && frameCount > 0)
      {
        var frameIndex = Mathf.Min(
          frameCount - 1,
          Mathf.FloorToInt(presentation.Progress * frameCount));
        GUI.DrawTextureWithTexCoords(
          new Rect(416f, 198f, 128f, 128f),
          sheet,
          new Rect(frameIndex / (float)frameCount, 0f, 1f / frameCount, 1f),
          true);
      }
      else
      {
        var icon = art?.FindPopupIcon(presentation.ItemId.Value);
        if (icon != null)
        {
          GUI.DrawTexture(
            new Rect(480f - size * 0.5f, 244f - size * 0.5f, size, size),
            icon,
            ScaleMode.ScaleToFit,
            true);
        }
      }
      GUI.Label(
        new Rect(300f, 318f, 360f, 42f),
        localization.Get("UI_ITEM_USING"),
        styles.Status);
    }

    private void DrawSuitChoices(PokerItemSnapshot snapshot, PokerItemUiArtSet art)
    {
      var suits = new[]
      {
        CardSuit.Spades,
        CardSuit.Diamonds,
        CardSuit.Hearts,
        CardSuit.Clubs
      };
      for (var index = 0; index < suits.Length; index++)
      {
        var rect = new Rect(
          700f + (index % 2) * 36f,
          246f + (index / 2) * 36f,
          32f,
          32f);
        var texture = art?.FindWildInkSuitSeal(suits[index]);
        var currentSuit = false;
        if (_selectedTargetCard.HasValue)
        {
          var targetIndex = FindCard(snapshot.PlayerPrivateCards, _selectedTargetCard.Value);
          currentSuit = targetIndex >= 0
            && snapshot.PlayerPrivateCards[targetIndex].EffectiveSuit == suits[index];
        }
        if (_selectedSuit == suits[index])
        {
          var previous = GUI.color;
          GUI.color = new Color(0.08f, 0.9f, 0.9f, 0.45f);
          GUI.DrawTexture(
            new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f),
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill,
            true);
          GUI.color = previous;
        }
        var previousColor = GUI.color;
        if (currentSuit) GUI.color = new Color(0.45f, 0.45f, 0.45f, 0.78f);
        if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
        GUI.color = previousColor;
        GUI.enabled = !currentSuit;
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) _selectedSuit = suits[index];
        GUI.enabled = true;
      }
    }
  }
}
