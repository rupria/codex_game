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
  internal sealed class HalliDevPanel
  {
    private readonly HalliBellControl _bellControl = new HalliBellControl();
    private LocalizationRuntime _localization;

    public void Draw(
      PrototypeHalliSnapshot snapshot,
      PlayableGamePhase gamePhase,
      PlayableTransitionSnapshot transition,
      int playerHealth,
      int aiHealth,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      LocalizationRuntime localization,
      Action advance,
      Action leftBell,
      Action rightBell)
    {
      _localization = localization;
      if (gamePhase == PlayableGamePhase.HalliOpening)
      {
        DrawOpening(snapshot, transition.Progress, playerHealth, aiHealth, styles, cards, uiArt);
        return;
      }

      var previousColor = GUI.color;
      if (gamePhase == PlayableGamePhase.HalliTransition)
      {
        GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.35f, transition.Progress));
      }

      DrawScoreboard(snapshot, playerHealth, aiHealth, styles);
      DrawPublic(snapshot, styles, cards, uiArt);
      DrawAiDeck(cards);
      DrawPiles(snapshot, cards);
      DrawStatus(snapshot, styles);
      DrawPlayerTray(snapshot, styles, cards, uiArt);
      DrawAiTray(snapshot, styles, cards, uiArt);
      DrawAiStatus(snapshot, styles);

      DrawControls(
        snapshot,
        gamePhase == PlayableGamePhase.Halli,
        styles,
        cards,
        uiArt,
        advance,
        leftBell,
        rightBell);

      DrawRevealMotion(snapshot, cards);
      DrawAcquisitionMotion(snapshot, cards);
      GUI.color = previousColor;
    }

    private void DrawOpening(
      PrototypeHalliSnapshot snapshot,
      float progress,
      int playerHealth,
      int aiHealth,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      DrawScoreboard(snapshot, playerHealth, aiHealth, styles);
      var cardProgress = Mathf.Clamp01(progress / 0.55f);
      var start = new Rect(432f, 214f, 96f, 135f);
      var destination = HalliBoardLayout.PublicCard;
      var cardRect = LerpRect(start, destination, Smooth(cardProgress));
      if (snapshot.FirstPublicCard.HasValue)
      {
        cards.DrawAt(cardRect, snapshot.FirstPublicCard.Value);
      }

      if (progress >= 0.55f)
      {
        DrawLockedPublicSlot(styles, uiArt);
      }
      GUI.Label(
        new Rect(270f, 370f, 420f, 52f),
        progress < 0.55f ? L("UI_HALLI_DEALER_FIRST") : L("UI_HALLI_TABLE_READY"),
        styles.Status);
      GUI.Label(new Rect(330f, 430f, 300f, 28f), L("UI_HALLI_INPUT_AFTER_CAMERA"), styles.Small);
    }

    private void DrawScoreboard(
      PrototypeHalliSnapshot snapshot,
      int playerHealth,
      int aiHealth,
      PlayableDevStyles styles)
    {
      GUI.Box(HalliBoardLayout.PlayerScore, GUIContent.none);
      GUI.Label(
        HalliBoardLayout.PlayerScore,
        L(
          "UI_HALLI_PLAYER_SCORE",
          new LocalizationArgument("hp", playerHealth),
          new LocalizationArgument("wins", snapshot.PlayerWins),
          new LocalizationArgument("target", snapshot.WinTarget)),
        styles.Heading);
      GUI.Box(HalliBoardLayout.AiScore, GUIContent.none);
      GUI.Label(
        HalliBoardLayout.AiScore,
        L(
          "UI_HALLI_AI_SCORE",
          new LocalizationArgument("hp", aiHealth),
          new LocalizationArgument("wins", snapshot.AiWins),
          new LocalizationArgument("target", snapshot.WinTarget)),
        styles.Heading);
      GUI.Label(
        new Rect(350f, 12f, 260f, 24f),
        L(
          "UI_HALLI_DISTRIBUTION_DECK",
          new LocalizationArgument("count", snapshot.FlipCount),
          new LocalizationArgument("remaining", snapshot.RemainingDeckCards)),
        styles.Small);
    }

    private void DrawPublic(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      if (snapshot.FirstPublicCard.HasValue)
      {
        cards.DrawAt(HalliBoardLayout.PublicCard, snapshot.FirstPublicCard.Value);
      }
      DrawLockedPublicSlot(styles, uiArt);
      GUI.Label(new Rect(260f, 104f, 80f, 22f), L("UI_HALLI_COMMUNITY"), styles.Small);
    }

    private static void DrawAiDeck(PlayableCardRenderer cards)
    {
      for (var index = 2; index >= 0; index--)
      {
        cards.DrawBackAt(
          new Rect(
            HalliBoardLayout.AiDeck.x - index * 3f,
            HalliBoardLayout.AiDeck.y + index * 3f,
            HalliBoardLayout.AiDeck.width,
            HalliBoardLayout.AiDeck.height),
          180f);
      }
    }

    private void DrawLockedPublicSlot(PlayableDevStyles styles, HalliUiArtSet uiArt)
    {
      if (uiArt != null && uiArt.PublicCardLockedSlot != null)
      {
        GUI.DrawTexture(
          HalliBoardLayout.LockedPublicCard,
          uiArt.PublicCardLockedSlot,
          ScaleMode.ScaleToFit,
          true);
      }
      else
      {
        GUI.Box(HalliBoardLayout.LockedPublicCard, L("UI_COMMON_LOCKED"), styles.Card);
      }
    }

    private static void DrawPiles(PrototypeHalliSnapshot snapshot, PlayableCardRenderer cards)
    {
      DrawPile(snapshot, PileSide.Left, snapshot.LeftPile, cards);
      DrawPile(snapshot, PileSide.Right, snapshot.RightPile, cards);
    }

    private static void DrawPile(
      PrototypeHalliSnapshot snapshot,
      PileSide side,
      IReadOnlyList<Card> pile,
      PlayableCardRenderer cards)
    {
      var isLeft = side == PileSide.Left;
      for (var index = 0; index < pile.Count; index++)
      {
        var card = pile[index];
        if (snapshot.Phase == PrototypeSessionPhase.SequentialReveal
          && snapshot.RevealingCard.HasValue
          && snapshot.RevealingCard.Value.Id == card.Id
          && snapshot.RevealProgress < 1f)
        {
          continue;
        }
        cards.DrawAt(HalliBoardLayout.PileCard(isLeft, index), card);
      }
    }

    private void DrawStatus(PrototypeHalliSnapshot snapshot, PlayableDevStyles styles)
    {
      var timer = snapshot.RemainingMicroseconds > 0
        ? Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d).ToString("0") + "s"
        : "--";
      GUI.Box(HalliBoardLayout.Status, GUIContent.none);
      GUI.Label(
        HalliBoardLayout.Status,
        _localization.Catalog.Get(snapshot.Status, _localization.Language),
        styles.Small);
      GUI.Label(new Rect(448f, 336f, 64f, 22f), timer, styles.Small);
    }

    private void DrawPlayerTray(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      DrawPanelTexture(HalliBoardLayout.PlayerTray, uiArt?.PlayerAcquiredTray);
      GUI.Label(
        new Rect(44f, 396f, 268f, 22f),
        L("UI_HALLI_PLAYER_ACQUIRED", new LocalizationArgument("count", snapshot.PlayerAcquiredCount)),
        styles.Small);
      var visible = Math.Min(3, snapshot.PlayerAcquiredCards.Count);
      var start = Math.Max(0, snapshot.PlayerAcquiredCards.Count - visible);
      for (var index = 0; index < visible; index++)
      {
        var card = snapshot.PlayerAcquiredCards[start + index];
        var moving = snapshot.LastAcquirer == PrototypeAcquirer.Player
          && snapshot.LastAcquiredPile.HasValue
          && Contains(snapshot.LastAcquiredCards, card.Id)
          && AcquisitionProgress(snapshot) < 1f;
        if (moving) continue;
        cards.DrawAt(new Rect(52f + index * 72f, 424f, 56f, 78f), card);
      }
    }

    private void DrawAiTray(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      DrawPanelTexture(HalliBoardLayout.AiTray, uiArt?.AiAcquiredStatusPanel);
      GUI.Label(
        new Rect(712f, 396f, 204f, 22f),
        L("UI_HALLI_AI_ACQUIRED", new LocalizationArgument("count", snapshot.AiAcquiredCount)),
        styles.Small);
      var movingCount = snapshot.LastAcquirer == PrototypeAcquirer.Ai
        && AcquisitionProgress(snapshot) < 1f
        ? snapshot.LastAcquiredCards.Count
        : 0;
      var visible = Math.Min(3, Math.Max(0, snapshot.AiAcquiredCount - movingCount));
      for (var index = 0; index < visible; index++)
      {
        cards.DrawBackAt(new Rect(748f + index * 30f, 424f, 56f, 78f));
      }
    }

    private void DrawAiStatus(PrototypeHalliSnapshot snapshot, PlayableDevStyles styles)
    {
      GUI.Box(HalliBoardLayout.AiStatus, GUIContent.none);
      var state = snapshot.LeadActor == HalliActor.Ai
        ? L("UI_HALLI_AI_STARTS_NEXT")
        : L("UI_HALLI_AI_WATCHING");
      if (snapshot.CanRing) state = L("UI_HALLI_AI_JUDGING");
      GUI.Label(HalliBoardLayout.AiStatus, state, styles.Small);
    }

    private void DrawControls(
      PrototypeHalliSnapshot snapshot,
      bool phaseAllowsInput,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      Action advance,
      Action leftBell,
      Action rightBell)
    {
      var canRing = phaseAllowsInput && snapshot.CanRing;
      if (_bellControl.Draw(
        PileSide.Left,
        HalliBoardLayout.LeftBellVisual,
        HalliBoardLayout.LeftBellHit,
        L("UI_HALLI_LEFT_BELL"),
        L("UI_HALLI_BELL"),
        canRing,
        snapshot,
        uiArt,
        styles)) leftBell();
      if (_bellControl.Draw(
        PileSide.Right,
        HalliBoardLayout.RightBellVisual,
        HalliBoardLayout.RightBellHit,
        L("UI_HALLI_RIGHT_BELL"),
        L("UI_HALLI_BELL"),
        canRing,
        snapshot,
        uiArt,
        styles)) rightBell();

      var canFlip = phaseAllowsInput && snapshot.CanFlip;
      var flipHovered = HalliBoardLayout.FlipHit.Contains(Event.current.mousePosition);
      var flipPressed = flipHovered && Input.GetMouseButton(0);
      var flipTexture = SelectFlipDeckTexture(canFlip, flipHovered, flipPressed, uiArt);
      if (flipTexture != null)
      {
        GUI.DrawTexture(HalliBoardLayout.FlipDeck, flipTexture, ScaleMode.ScaleToFit, true);
      }
      else
      {
        var previousColor = GUI.color;
        if (!canFlip) GUI.color = new Color(0.5f, 0.52f, 0.56f, 0.7f);
        for (var index = 2; index >= 0; index--)
        {
          cards.DrawBackAt(new Rect(
            HalliBoardLayout.FlipDeck.x + index * 3f,
            HalliBoardLayout.FlipDeck.y - index * 3f,
            HalliBoardLayout.FlipDeck.width,
            HalliBoardLayout.FlipDeck.height));
        }
        GUI.color = previousColor;
      }
      GUI.Label(
        new Rect(410f, 462f, 140f, 24f),
        canFlip ? L("UI_HALLI_FLIP_ONE") : L("UI_HALLI_FLIP_LOCKED"),
        styles.Heading);
      GUI.enabled = canFlip;
      if (GUI.Button(HalliBoardLayout.FlipHit, GUIContent.none, GUIStyle.none)) advance();
      GUI.enabled = true;
    }

    private static Texture2D SelectFlipDeckTexture(
      bool enabled,
      bool hovered,
      bool pressed,
      HalliUiArtSet uiArt)
    {
      if (uiArt == null) return null;
      if (!enabled) return uiArt.FlipDeckDisabled;
      if (pressed) return uiArt.FlipDeckPressed;
      if (hovered) return uiArt.FlipDeckHover;
      return uiArt.FlipDeckIdle;
    }

    private static void DrawPanelTexture(Rect rect, Texture2D texture)
    {
      if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      else GUI.Box(rect, GUIContent.none);
    }

    private static void DrawRevealMotion(PrototypeHalliSnapshot snapshot, PlayableCardRenderer cards)
    {
      if (snapshot.Phase != PrototypeSessionPhase.SequentialReveal
        || !snapshot.RevealingCard.HasValue
        || !snapshot.RevealingPile.HasValue
        || snapshot.RevealProgress >= 1f)
      {
        return;
      }

      var pile = snapshot.RevealingPile.Value == PileSide.Left ? snapshot.LeftPile : snapshot.RightPile;
      var target = HalliBoardLayout.RevealTarget(
        snapshot.RevealingPile.Value == PileSide.Left,
        Math.Min(GameRules.ExposedCardsPerPile - 1, pile.Count));
      CardFlipMotion.Draw(
        cards,
        snapshot.RevealingCard.Value,
        snapshot.RevealingActor == HalliActor.Ai
          ? HalliBoardLayout.AiDeck
          : HalliBoardLayout.PlayerDeck,
        target,
        snapshot.RevealProgress,
        snapshot.RevealingActor == HalliActor.Ai);
    }

    private static void DrawAcquisitionMotion(PrototypeHalliSnapshot snapshot, PlayableCardRenderer cards)
    {
      if (!snapshot.LastAcquiredPile.HasValue || snapshot.LastAcquiredCards.Count == 0) return;
      var progress = AcquisitionProgress(snapshot);
      if (progress >= 1f) return;
      var source = HalliBoardLayout.PileCard(snapshot.LastAcquiredPile.Value == PileSide.Left, 1);
      var destination = snapshot.LastAcquirer == PrototypeAcquirer.Player
        ? new Rect(52f, 424f, 56f, 78f)
        : new Rect(782f, 424f, 56f, 78f);
      for (var index = 0; index < snapshot.LastAcquiredCards.Count; index++)
      {
        var target = new Rect(destination.x + index * 58f, destination.y, destination.width, destination.height);
        var movingRect = LerpRect(source, target, Smooth(progress));
        if (snapshot.LastAcquirer == PrototypeAcquirer.Ai) cards.DrawBackAt(movingRect);
        else cards.DrawAt(movingRect, snapshot.LastAcquiredCards[index]);
      }
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

    private string L(string key, params LocalizationArgument[] arguments)
    {
      return _localization.Get(key, arguments);
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
