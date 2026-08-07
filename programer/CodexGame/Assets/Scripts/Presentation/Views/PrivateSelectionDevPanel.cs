using System;
using CodexGame.Application.Distribution;
using CodexGame.Core.Cards;
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
      Action<CardId> toggle,
      Action confirm)
    {
      var seconds = Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d);
      GUILayout.Label("PRIVATE CARD SELECTION", styles.Heading);
      GUILayout.Label(
        "Select exactly " + snapshot.RequiredSelectionCount
        + " cards. Q/E moves focus, W toggles, ENTER confirms. "
        + seconds.ToString("0") + "s",
        styles.Body);

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
          GUILayout.Label(index == focusedIndex ? "FOCUS" : " ", styles.Small);
          cards.Draw(card, 96f, 130f, selected);
          if (GUILayout.Button(selected ? "REMOVE" : "SELECT", GUILayout.Width(96f), GUILayout.Height(24f)))
          {
            toggle(card.Id);
          }
          GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
      }

      GUI.enabled = snapshot.CanConfirm;
      if (GUILayout.Button(
        "CONFIRM " + snapshot.SelectedCards.Count + "/" + snapshot.RequiredSelectionCount + "  [ENTER]",
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
