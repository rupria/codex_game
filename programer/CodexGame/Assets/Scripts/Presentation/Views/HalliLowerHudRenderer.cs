using System;
using System.Collections.Generic;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliLowerHudRenderer
  {
    public void Draw(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      LocalizationRuntime localization)
    {
      var playerOnly = uiArt != null && uiArt.UsesPlayerOnlyLowerHud;
      DrawPlayerTray(snapshot, styles, cards, uiArt, localization, playerOnly);
      if (!playerOnly) DrawLegacyAiAcquired(snapshot, styles, cards, uiArt, localization);
    }

    public void DrawAcquisitionMotion(
      PrototypeHalliSnapshot snapshot,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      if (!snapshot.LastAcquiredPile.HasValue || snapshot.LastAcquiredCards.Count == 0) return;
      if (snapshot.LastAcquirer == PrototypeAcquirer.Ai) return;
      var progress = AcquisitionProgress(snapshot);
      if (progress >= 1f) return;

      var playerOnly = uiArt != null && uiArt.UsesPlayerOnlyLowerHud;
      var source = HalliBoardLayout.RevealPileSource(snapshot.LastAcquiredPile.Value);
      var firstNewIndex = Math.Max(
        0,
        snapshot.PlayerAcquiredCards.Count - snapshot.LastAcquiredCards.Count);
      var visibleStart = playerOnly
        ? HalliBoardLayout.PlayerOnlyVisibleStart(snapshot.PlayerAcquiredCards.Count)
        : 0;

      for (var index = 0; index < snapshot.LastAcquiredCards.Count; index++)
      {
        var cardIndex = firstNewIndex + index;
        if (cardIndex < visibleStart) continue;
        var target = PlayerCardRect(
          cardIndex,
          visibleStart,
          snapshot.PlayerAcquiredCards.Count,
          playerOnly);
        cards.DrawAt(LerpRect(source, target, Smooth(progress)), snapshot.LastAcquiredCards[index]);
      }
    }

    private static void DrawPlayerTray(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      LocalizationRuntime localization,
      bool playerOnly)
    {
      DrawPanelTexture(HalliBoardLayout.PlayerTray, uiArt?.PlayerAcquiredTray);
      if (!playerOnly)
      {
        GUI.Label(
          new Rect(44f, 396f, 356f, 22f),
          localization.Get(
            "UI_HALLI_PLAYER_ACQUIRED",
            new LocalizationArgument("count", snapshot.PlayerAcquiredCount)),
          styles.Small);
      }

      var visibleStart = playerOnly
        ? HalliBoardLayout.PlayerOnlyVisibleStart(snapshot.PlayerAcquiredCards.Count)
        : 0;
      for (var cardIndex = visibleStart; cardIndex < snapshot.PlayerAcquiredCards.Count; cardIndex++)
      {
        var card = snapshot.PlayerAcquiredCards[cardIndex];
        var moving = snapshot.LastAcquirer == PrototypeAcquirer.Player
          && snapshot.LastAcquiredPile.HasValue
          && Contains(snapshot.LastAcquiredCards, card.Id)
          && AcquisitionProgress(snapshot) < 1f;
        if (moving) continue;
        cards.DrawAt(
          PlayerCardRect(
            cardIndex,
            visibleStart,
            snapshot.PlayerAcquiredCards.Count,
            playerOnly),
          card);
      }
    }

    private static void DrawLegacyAiAcquired(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      LocalizationRuntime localization)
    {
      DrawPanelTexture(HalliBoardLayout.AiTray, uiArt?.AiAcquiredStatusPanel);
      GUI.Label(
        new Rect(712f, 396f, 204f, 22f),
        localization.Get(
          "UI_HALLI_AI_ACQUIRED",
          new LocalizationArgument("count", snapshot.AiAcquiredCount)),
        styles.Small);
      for (var index = 0; index < snapshot.AiAcquiredCards.Count; index++)
      {
        cards.DrawBackAt(
          HalliBoardLayout.AiAcquiredCard(index, snapshot.AiAcquiredCards.Count),
          180f);
      }
    }

    private static Rect PlayerCardRect(
      int cardIndex,
      int visibleStart,
      int cardCount,
      bool playerOnly)
    {
      return playerOnly
        ? HalliBoardLayout.PlayerOnlyAcquiredCard(cardIndex - visibleStart)
        : HalliBoardLayout.PlayerAcquiredCard(cardIndex, cardCount);
    }

    private static void DrawPanelTexture(Rect rect, Texture2D texture)
    {
      if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      else GUI.Box(rect, GUIContent.none);
    }

    private static float AcquisitionProgress(PrototypeHalliSnapshot snapshot)
    {
      if (snapshot.Phase != PrototypeSessionPhase.Review || !snapshot.LastAcquiredPile.HasValue) return 1f;
      var elapsed = GameRules.HalliResultLockMicroseconds - snapshot.RemainingMicroseconds;
      return Mathf.Clamp01((float)elapsed / 450_000f);
    }

    private static bool Contains(IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == id) return true;
      }
      return false;
    }

    private static float Smooth(float value)
    {
      value = Mathf.Clamp01(value);
      return value * value * (3f - 2f * value);
    }

    private static Rect LerpRect(Rect from, Rect to, float progress)
    {
      return new Rect(
        Mathf.Lerp(from.x, to.x, progress),
        Mathf.Lerp(from.y, to.y, progress),
        Mathf.Lerp(from.width, to.width, progress),
        Mathf.Lerp(from.height, to.height, progress));
    }
  }
}
