using System;
using System.Collections.Generic;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliDevPanel
  {
    public void Draw(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      int rewardFocus,
      Action advance,
      Action leftBell,
      Action rightBell,
      Action<CardId> selectWrongBellReward)
    {
      DrawScoreboard(snapshot, styles);
      GUILayout.BeginHorizontal();
      DrawPublic(snapshot, styles, cards);
      DrawPile("LEFT PILE", snapshot.LeftPile, styles, cards);
      DrawPile("RIGHT PILE", snapshot.RightPile, styles, cards);
      GUILayout.EndHorizontal();
      GUILayout.Label(snapshot.StatusMessage, styles.Status, GUILayout.Height(45f));
      if (snapshot.Phase == PrototypeSessionPhase.WrongBellRewardSelection)
      {
        DrawWrongBellRewardSelection(
          snapshot,
          rewardFocus,
          styles,
          cards,
          selectWrongBellReward);
        return;
      }
      DrawAcquisition(snapshot, styles);
      DrawActions(snapshot, advance, leftBell, rightBell);
    }

    private static void DrawWrongBellRewardSelection(
      PrototypeHalliSnapshot snapshot,
      int focusedIndex,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      Action<CardId> select)
    {
      const int visibleCount = 7;
      var candidates = snapshot.WrongBellRewardCandidates;
      var start = Math.Max(0, Math.Min(
        focusedIndex - (visibleCount / 2),
        candidates.Count - visibleCount));
      var end = Math.Min(candidates.Count, start + visibleCount);

      GUILayout.Label(
        "REWARD CARD " + (focusedIndex + 1) + "/" + candidates.Count
        + "  |  Q/E MOVE, W/ENTER SELECT",
        styles.Heading);
      GUILayout.BeginHorizontal();
      for (var index = start; index < end; index++)
      {
        var card = candidates[index];
        GUILayout.BeginVertical(GUILayout.Width(112f));
        GUILayout.Label(index == focusedIndex ? "FOCUS" : " ", styles.Small);
        cards.Draw(card, 100f, 130f);
        if (GUILayout.Button(
          index == focusedIndex ? "SELECT [W]" : "SELECT",
          GUILayout.Width(100f),
          GUILayout.Height(24f)))
        {
          select(card.Id);
        }
        GUILayout.EndVertical();
      }
      GUILayout.EndHorizontal();
    }

    private static void DrawScoreboard(PrototypeHalliSnapshot snapshot, PlayableDevStyles styles)
    {
      var timer = snapshot.RemainingMicroseconds > 0
        ? Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d).ToString("0") + "s"
        : "--";
      GUILayout.BeginHorizontal();
      GUILayout.Label("HALLI " + snapshot.PlayerWins + "/" + snapshot.WinTarget, styles.Heading);
      GUILayout.Label("AI " + snapshot.AiWins + "/" + snapshot.WinTarget, styles.Heading);
      GUILayout.Label("FLIPS " + snapshot.FlipCount + "/25", styles.Heading);
      GUILayout.Label("DECK " + snapshot.RemainingDeckCards, styles.Heading);
      GUILayout.Label("TIMER " + timer, styles.Heading);
      GUILayout.EndHorizontal();
    }

    private static void DrawPublic(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards)
    {
      GUILayout.BeginVertical(GUILayout.Width(190f));
      GUILayout.Label("PUBLIC 1", styles.Heading);
      if (snapshot.FirstPublicCard.HasValue) cards.Draw(snapshot.FirstPublicCard.Value, 170f, 150f);
      GUILayout.EndVertical();
    }

    private static void DrawPile(
      string label,
      IReadOnlyList<Card> pile,
      PlayableDevStyles styles,
      PlayableCardRenderer cards)
    {
      GUILayout.BeginVertical(GUILayout.Width(350f));
      GUILayout.Label(label, styles.Heading);
      GUILayout.BeginHorizontal();
      for (var index = 0; index < 2; index++)
      {
        if (index < pile.Count) cards.Draw(pile[index], 160f, 150f);
        else GUILayout.Box("EMPTY", styles.Card, GUILayout.Width(160f), GUILayout.Height(150f));
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
    }

    private static void DrawAcquisition(PrototypeHalliSnapshot snapshot, PlayableDevStyles styles)
    {
      if (snapshot.LastAcquiredCards.Count == 0) return;
      var text = snapshot.LastAcquirer == PrototypeAcquirer.Player ? "PLAYER: " : "AI: ";
      for (var index = 0; index < snapshot.LastAcquiredCards.Count; index++)
      {
        if (index > 0) text += " + ";
        text += PlayableCardRenderer.FormatInline(snapshot.LastAcquiredCards[index]);
      }
      GUILayout.Label("LAST ACQUIRED - " + text, styles.Body, GUILayout.Height(25f));
    }

    private static void DrawActions(
      PrototypeHalliSnapshot snapshot,
      Action advance,
      Action leftBell,
      Action rightBell)
    {
      GUILayout.BeginHorizontal();
      if (snapshot.Phase == PrototypeSessionPhase.Finished)
      {
        if (GUILayout.Button("CONTINUE TO PRIVATE CARDS  [ENTER / SPACE]", GUILayout.Height(50f))) advance();
      }
      else
      {
        var canRing = snapshot.Phase == PrototypeSessionPhase.ReadyToFlip
          || snapshot.Phase == PrototypeSessionPhase.BellOpen;
        GUI.enabled = canRing;
        if (GUILayout.Button("LEFT BELL  [LEFT]", GUILayout.Height(50f))) leftBell();
        if (GUILayout.Button("RIGHT BELL  [RIGHT]", GUILayout.Height(50f))) rightBell();
        GUI.enabled = true;
        var label = snapshot.Phase == PrototypeSessionPhase.Review
          ? "CONTINUE  [W / UP / SPACE]"
          : "FLIP / SKIP  [UP / SPACE]";
        if (GUILayout.Button(label, GUILayout.Height(50f))) advance();
      }
      GUILayout.EndHorizontal();
    }
  }
}
