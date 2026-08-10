using System;
using CodexGame.Application.Items;
using CodexGame.Core.Cards;
using CodexGame.Core.Items;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PokerItemDevPanel
  {
    private GameItemId? _selectedTargetItem;

    public void Draw(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<CardId> reload,
      Action<CardId> beginBottomDeal,
      Action<CardId> chooseBottomDeal,
      Action hypeMan,
      Action healthRecovery,
      Action confirm)
    {
      if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

      GUI.Box(new Rect(36f, 24f, 888f, 492f), GUIContent.none);
      GUI.Label(new Rect(56f, 38f, 848f, 38f), localization.Get("UI_ITEM_WINDOW_TITLE"), styles.Title);

      DrawAi(snapshot, cards, styles, localization);
      DrawPlayerCards(snapshot, cards, reload, beginBottomDeal);
      DrawInventory(snapshot, styles, localization, hypeMan, healthRecovery);

      if (snapshot.Phase == PokerItemPhase.AwaitingBottomDealChoice)
      {
        GUI.Label(new Rect(360f, 190f, 240f, 26f), localization.Get("UI_ITEM_CHOOSE_CARD"), styles.Heading);
        for (var index = 0; index < snapshot.BottomDealCandidates.Count; index++)
        {
          var rect = new Rect(400f + (index * 86f), 220f, 72f, 100f);
          cards.DrawAt(rect, snapshot.BottomDealCandidates[index]);
          if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
          {
            chooseBottomDeal(snapshot.BottomDealCandidates[index].Id);
          }
        }
      }

      GUI.enabled = snapshot.Phase == PokerItemPhase.AwaitingActions;
      if (GUI.Button(new Rect(734f, 456f, 170f, 42f), localization.Get("UI_ITEM_CONFIRM_HAND")))
      {
        _selectedTargetItem = null;
        confirm();
      }
      GUI.enabled = true;

      if (snapshot.LastFailure != PokerItemFailure.None)
      {
        GUI.Label(
          new Rect(56f, 426f, 650f, 24f),
          localization.Get("UI_ITEM_ACTION_FAILED") + ": " + snapshot.LastFailure,
          styles.Small);
      }
    }

    private void DrawAi(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      GUI.Label(new Rect(56f, 88f, 180f, 26f), localization.Get("UI_POKER_AI_HAND"), styles.Heading);
      if (snapshot.VisibleAiPrivateCards.Count == 0)
      {
        for (var index = 0; index < 3; index++)
        {
          cards.DrawBackAt(new Rect(250f + (index * 66f), 84f, 56f, 78f));
        }
      }
      else
      {
        cards.DrawAt(new Rect(316f, 84f, 56f, 78f), snapshot.VisibleAiPrivateCards[0]);
      }
    }

    private void DrawPlayerCards(
      PokerItemSnapshot snapshot,
      PlayableCardRenderer cards,
      Action<CardId> reload,
      Action<CardId> beginBottomDeal)
    {
      for (var index = 0; index < snapshot.PlayerPrivateCards.Count; index++)
      {
        var rect = new Rect(330f + (index * 94f), 334f, 80f, 112f);
        cards.DrawAt(rect, snapshot.PlayerPrivateCards[index]);
        if (_selectedTargetItem.HasValue
          && snapshot.Phase == PokerItemPhase.AwaitingActions
          && GUI.Button(rect, GUIContent.none, GUIStyle.none))
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

    private void DrawInventory(
      PokerItemSnapshot snapshot,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action hypeMan,
      Action healthRecovery)
    {
      GUI.Label(new Rect(56f, 188f, 220f, 26f), localization.Get("UI_ITEM_INVENTORY"), styles.Heading);
      for (var index = 0; index < 4; index++)
      {
        var rect = new Rect(56f, 224f + (index * 48f), 236f, 40f);
        if (index >= snapshot.Inventory.Count)
        {
          GUI.enabled = false;
          GUI.Button(rect, localization.Get("UI_ITEM_EMPTY_SLOT"));
          GUI.enabled = true;
          continue;
        }

        var itemId = snapshot.Inventory[index];
        GameItemCatalog.TryGet(itemId, out var definition);
        GUI.enabled = snapshot.Phase == PokerItemPhase.AwaitingActions;
        if (GUI.Button(rect, localization.Get(definition.LocalizationNameKey)))
        {
          if (itemId == GameItemId.Reload || itemId == GameItemId.BottomDeal)
          {
            _selectedTargetItem = itemId;
          }
          else if (itemId == GameItemId.HypeMan) hypeMan();
          else if (itemId == GameItemId.HealthRecovery) healthRecovery();
        }
        GUI.enabled = true;
      }
    }
  }
}
