using System;
using System.Collections.Generic;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliDevPanel
  {
    private readonly HalliBellControl _bellControl = new HalliBellControl();
    private Vector2 _rewardScroll;

    public void Draw(
      PrototypeHalliSnapshot snapshot,
      PlayableGamePhase gamePhase,
      PlayableTransitionSnapshot transition,
      int playerHealth,
      int aiHealth,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      int rewardFocus,
      Action advance,
      Action leftBell,
      Action rightBell,
      Action<CardId> selectWrongBellReward)
    {
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
      DrawPiles(snapshot, cards);
      DrawStatus(snapshot, styles);
      DrawPlayerTray(snapshot, styles, cards, uiArt);
      DrawAiTray(snapshot, styles, cards, uiArt);
      DrawAiStatus(snapshot, styles);

      if (snapshot.Phase == PrototypeSessionPhase.WrongBellRewardSelection)
      {
        DrawWrongBellRewardSelection(
          snapshot,
          rewardFocus,
          styles,
          cards,
          selectWrongBellReward);
      }
      else
      {
        DrawControls(
          snapshot,
          gamePhase == PlayableGamePhase.Halli,
          styles,
          cards,
          uiArt,
          advance,
          leftBell,
          rightBell);
      }

      DrawRevealMotion(snapshot, cards);
      DrawAcquisitionMotion(snapshot, cards);
      GUI.color = previousColor;
    }

    private static void DrawOpening(
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
        progress < 0.55f ? "DEALER OPENS THE FIRST COMMUNITY CARD" : "HALLI TABLE READY",
        styles.Status);
      GUI.Label(new Rect(330f, 430f, 300f, 28f), "INPUT UNLOCKS AFTER THE CAMERA SETTLES", styles.Small);
    }

    private static void DrawScoreboard(
      PrototypeHalliSnapshot snapshot,
      int playerHealth,
      int aiHealth,
      PlayableDevStyles styles)
    {
      GUI.Box(HalliBoardLayout.PlayerScore, GUIContent.none);
      GUI.Label(
        HalliBoardLayout.PlayerScore,
        "PLAYER  HP " + playerHealth + "/3\nHALLI " + snapshot.PlayerWins + "/" + snapshot.WinTarget,
        styles.Heading);
      GUI.Box(HalliBoardLayout.AiScore, GUIContent.none);
      GUI.Label(
        HalliBoardLayout.AiScore,
        "AI  HP " + aiHealth + "/3\nHALLI " + snapshot.AiWins + "/" + snapshot.WinTarget,
        styles.Heading);
      GUI.Label(
        new Rect(350f, 16f, 260f, 28f),
        "DISTRIBUTIONS " + snapshot.FlipCount + "/12  ·  DECK " + snapshot.RemainingDeckCards,
        styles.Small);
    }

    private static void DrawPublic(
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
      GUI.Label(new Rect(388f, 120f, 176f, 22f), "COMMUNITY", styles.Small);
    }

    private static void DrawLockedPublicSlot(PlayableDevStyles styles, HalliUiArtSet uiArt)
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
        GUI.Box(HalliBoardLayout.LockedPublicCard, "LOCKED", styles.Card);
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

    private static void DrawStatus(PrototypeHalliSnapshot snapshot, PlayableDevStyles styles)
    {
      var timer = snapshot.RemainingMicroseconds > 0
        ? Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d).ToString("0") + "s"
        : "--";
      GUI.Box(HalliBoardLayout.Status, GUIContent.none);
      GUI.Label(
        HalliBoardLayout.Status,
        snapshot.Phase == PrototypeSessionPhase.SequentialReveal
          ? "CARD " + snapshot.RevealStepNumber + "/4  ·  INPUT LOCKED"
          : snapshot.StatusMessage,
        styles.Small);
      GUI.Label(new Rect(448f, 336f, 64f, 22f), timer, styles.Small);
    }

    private static void DrawPlayerTray(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      DrawPanelTexture(HalliBoardLayout.PlayerTray, uiArt?.PlayerAcquiredTray);
      GUI.Label(new Rect(44f, 396f, 268f, 22f), "PLAYER ACQUIRED  " + snapshot.PlayerAcquiredCount, styles.Small);
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

    private static void DrawAiTray(
      PrototypeHalliSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      DrawPanelTexture(HalliBoardLayout.AiTray, uiArt?.AiAcquiredStatusPanel);
      GUI.Label(new Rect(712f, 396f, 204f, 22f), "AI ACQUIRED  " + snapshot.AiAcquiredCount, styles.Small);
      if (snapshot.LastAcquirer == PrototypeAcquirer.Ai
        && snapshot.LastAcquiredCards.Count > 0
        && snapshot.Phase == PrototypeSessionPhase.Review)
      {
        for (var index = 0; index < Math.Min(2, snapshot.LastAcquiredCards.Count); index++)
        {
          cards.DrawAt(
            new Rect(748f + index * 58f, 424f, 56f, 78f),
            snapshot.LastAcquiredCards[index]);
        }
      }
      else if (snapshot.AiAcquiredCount > 0)
      {
        cards.DrawBackAt(new Rect(782f, 424f, 56f, 78f));
      }
    }

    private static void DrawAiStatus(PrototypeHalliSnapshot snapshot, PlayableDevStyles styles)
    {
      GUI.Box(HalliBoardLayout.AiStatus, GUIContent.none);
      var state = snapshot.LeadActor == HalliActor.Ai ? "AI STARTS NEXT" : "AI WATCHING";
      if (snapshot.Phase == PrototypeSessionPhase.BellOpen) state = "AI JUDGING";
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
        "Q  LEFT",
        canRing,
        snapshot,
        uiArt,
        styles)) leftBell();
      if (_bellControl.Draw(
        PileSide.Right,
        HalliBoardLayout.RightBellVisual,
        HalliBoardLayout.RightBellHit,
        "E  RIGHT",
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
      GUI.Label(new Rect(410f, 462f, 140f, 24f), canFlip ? "W  DISTRIBUTE 4" : "W  LOCKED", styles.Heading);
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

    private void DrawWrongBellRewardSelection(
      PrototypeHalliSnapshot snapshot,
      int focusedIndex,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      Action<CardId> select)
    {
      var candidates = snapshot.WrongBellRewardCandidates;
      GUILayout.BeginArea(new Rect(160f, 250f, 640f, 260f), GUI.skin.box);
      GUILayout.Label(
        "REWARD CARD " + (focusedIndex + 1) + "/" + candidates.Count
        + (snapshot.WrongBellRewardSelectionEnabled ? "  ·  Q/E MOVE, W SELECT" : "  ·  REVIEW LOCK"),
        styles.Heading);
      _rewardScroll = GUILayout.BeginScrollView(_rewardScroll, true, false, GUILayout.Height(190f));
      GUILayout.BeginHorizontal();
      for (var index = 0; index < candidates.Count; index++)
      {
        var card = candidates[index];
        GUILayout.BeginVertical(GUILayout.Width(92f));
        cards.Draw(card, 82f, 112f, index == focusedIndex);
        GUI.enabled = snapshot.WrongBellRewardSelectionEnabled;
        if (GUILayout.Button(index == focusedIndex ? "SELECT [W]" : "SELECT", GUILayout.Height(25f))) select(card.Id);
        GUI.enabled = true;
        GUILayout.EndVertical();
      }
      GUILayout.EndHorizontal();
      GUILayout.EndScrollView();
      GUILayout.EndArea();
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
        Math.Max(0, pile.Count - 1));
      var progress = Smooth(snapshot.RevealProgress);
      var rect = LerpRect(HalliBoardLayout.FlipDeck, target, progress);
      var flipScale = Mathf.Max(0.08f, Mathf.Abs(progress * 2f - 1f));
      rect = new Rect(
        rect.center.x - rect.width * flipScale * 0.5f,
        rect.y,
        rect.width * flipScale,
        rect.height);
      if (progress < 0.5f) cards.DrawBackAt(rect);
      else cards.DrawAt(rect, snapshot.RevealingCard.Value);
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
        cards.DrawAt(LerpRect(source, target, Smooth(progress)), snapshot.LastAcquiredCards[index]);
      }
    }

    private static float AcquisitionProgress(PrototypeHalliSnapshot snapshot)
    {
      if (snapshot.Phase != PrototypeSessionPhase.Review || !snapshot.LastAcquiredPile.HasValue) return 1f;
      var elapsed = GameRules.NextFlipLockMicroseconds - snapshot.RemainingMicroseconds;
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
