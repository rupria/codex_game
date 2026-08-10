using System;
using CodexGame.Application.Distribution;
using CodexGame.Core.Cards;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PrivateSelectionDevPanel
  {
    public void Draw(
      PrivateCardSelectionSnapshot snapshot,
      int focusedIndex,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      LocalizationRuntime localization,
      Action<int> focus,
      Action<CardId> toggle,
      Action confirm)
    {
      var seconds = Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d);
      GUILayout.Label(localization.Get("UI_PRIVATE_SELECTION_TITLE"), styles.Heading);
      GUILayout.Label(
        localization.Get(
          "UI_PRIVATE_SELECTION_GUIDE",
          new LocalizationArgument("required", snapshot.RequiredSelectionCount),
          new LocalizationArgument("seconds", seconds.ToString("0"))),
        styles.Body);

      if (snapshot.FirstPublicCard.HasValue && snapshot.SecondPublicCard.HasValue)
      {
        GUILayout.Label(localization.Get("UI_POKER_PUBLIC"), styles.Small);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        cards.Draw(snapshot.FirstPublicCard.Value, 82f, 112f, false);
        cards.Draw(snapshot.SecondPublicCard.Value, 82f, 112f, false);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }

      for (var row = 0; row * 8 < snapshot.WinnerCandidates.Count; row++)
      {
        GUILayout.BeginHorizontal();
        for (var column = 0; column < 8; column++)
        {
          var index = (row * 8) + column;
          if (index >= snapshot.WinnerCandidates.Count) break;
          var card = snapshot.WinnerCandidates[index];
          var selected = Contains(snapshot.SelectedCards, card.Id);
          GUILayout.BeginVertical(GUILayout.Width(105f));
          GUILayout.Label(index == focusedIndex ? localization.Get("UI_COMMON_FOCUS") : " ", styles.Small);
          var cardRect = cards.Draw(card, 96f, 130f, selected);
          if (cardRect.Contains(Event.current.mousePosition) && index != focusedIndex)
          {
            focus(index);
          }
          if (GUILayout.Button(
            localization.Get(selected ? "UI_COMMON_REMOVE" : "UI_COMMON_SELECT"),
            GUILayout.Width(96f),
            GUILayout.Height(24f)))
          {
            toggle(card.Id);
          }
          GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
      }

      GUI.enabled = snapshot.CanConfirm;
      if (GUILayout.Button(
        localization.Get(
          "UI_PRIVATE_CONFIRM",
          new LocalizationArgument("selected", snapshot.SelectedCards.Count),
          new LocalizationArgument("required", snapshot.RequiredSelectionCount)),
        GUILayout.Height(52f)))
      {
        confirm();
      }
      GUI.enabled = true;
    }

    private static bool Contains(System.Collections.Generic.IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == id) return true;
      }
      return false;
    }
  }
}
