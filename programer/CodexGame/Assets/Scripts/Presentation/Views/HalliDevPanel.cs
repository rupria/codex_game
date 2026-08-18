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
    private readonly HalliLowerHudRenderer _lowerHud = new HalliLowerHudRenderer();
    private readonly HalliRopeTimer _ropeTimer = new HalliRopeTimer();
    private readonly List<SharedPileCard> _leftPileHistory = new List<SharedPileCard>(3);
    private readonly List<SharedPileCard> _rightPileHistory = new List<SharedPileCard>(3);
    private long _historyRoundSeed = long.MinValue;
    private CardId? _lastHistoryReveal;
    private CardId? _lastHistoryAcquisition;
    private LocalizationRuntime _localization;

    public void Draw(
      PrototypeHalliSnapshot snapshot,
      PlayableGamePhase gamePhase,
      PlayableTransitionSnapshot transition,
      int playerHealth,
      int aiHealth,
      bool playerDamage,
      bool aiDamage,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      HealthUiArtSet healthArt,
      LocalizationRuntime localization,
      Action advance,
      Action leftBell,
      Action rightBell)
    {
      _localization = localization;
      UpdateRevealHistory(snapshot);
      if (gamePhase == PlayableGamePhase.HalliOpening)
      {
        DrawOpening(
          snapshot,
          transition.Progress,
          playerHealth,
          aiHealth,
          playerDamage,
          aiDamage,
          styles,
          cards,
          uiArt,
          healthArt);
        return;
      }

      var previousColor = GUI.color;
      if (gamePhase == PlayableGamePhase.HalliTransition)
      {
        GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.35f, transition.Progress));
      }

      DrawScoreboard(
        snapshot,
        playerHealth,
        aiHealth,
        playerDamage,
        aiDamage,
        styles,
        healthArt);
      DrawPublic(snapshot, styles, cards, uiArt);
      DrawAiDeck(cards);
      DrawRevealHistories(snapshot, cards, uiArt);
      DrawFlipReady(snapshot, gamePhase, styles);
      _lowerHud.Draw(snapshot, styles, cards, uiArt, _localization);
      DrawRoundWins(snapshot, uiArt);
      _ropeTimer.Draw(snapshot, styles, uiArt);

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
      DrawAiThinking(snapshot, uiArt);
      _lowerHud.DrawAcquisitionMotion(snapshot, cards, uiArt);
      GUI.color = previousColor;
    }

    private void DrawOpening(
      PrototypeHalliSnapshot snapshot,
      float progress,
      int playerHealth,
      int aiHealth,
      bool playerDamage,
      bool aiDamage,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt,
      HealthUiArtSet healthArt)
    {
      DrawScoreboard(
        snapshot,
        playerHealth,
        aiHealth,
        playerDamage,
        aiDamage,
        styles,
        healthArt);
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
    }

    private void DrawScoreboard(
      PrototypeHalliSnapshot snapshot,
      int playerHealth,
      int aiHealth,
      bool playerDamage,
      bool aiDamage,
      PlayableDevStyles styles,
      HealthUiArtSet healthArt)
    {
      GUI.Box(HalliBoardLayout.PlayerScore, GUIContent.none);
      GUI.Label(
        new Rect(36f, 24f, 250f, 26f),
        L("UI_ACTOR_PLAYER"),
        styles.Small);
      HealthHeartRenderer.Draw(
        new Rect(36f, 48f, 250f, 30f),
        playerHealth,
        GameRules.StartingHealth,
        false,
        healthArt,
        playerDamage);
      GUI.Box(HalliBoardLayout.AiScore, GUIContent.none);
      GUI.Label(
        new Rect(674f, 24f, 250f, 26f),
        L("UI_ACTOR_AI"),
        styles.Small);
      HealthHeartRenderer.Draw(
        new Rect(674f, 48f, 250f, 30f),
        aiHealth,
        GameRules.StartingHealth,
        true,
        healthArt,
        aiDamage);
      GUI.Label(
        new Rect(350f, 12f, 260f, 24f),
        snapshot.FlipCount + "/" + GameRules.HalliDistributionLimit
          + "  ·  " + snapshot.RemainingDeckCards,
        styles.Small);
    }

    private static void DrawRoundWins(PrototypeHalliSnapshot snapshot, HalliUiArtSet uiArt)
    {
      DrawWinPips(
        new Vector2(18f, 365f),
        snapshot.PlayerWins,
        snapshot.WinTarget,
        uiArt?.PlayerWinPipEmpty,
        uiArt?.PlayerWinPipFilled,
        new Color(0.08f, 0.85f, 0.88f));
      DrawWinPips(
        new Vector2(790f, 92f),
        snapshot.AiWins,
        snapshot.WinTarget,
        uiArt?.AiWinPipEmpty,
        uiArt?.AiWinPipFilled,
        new Color(0.9f, 0.2f, 0.24f));
    }

    private static void DrawWinPips(
      Vector2 origin,
      int wins,
      int target,
      Texture2D empty,
      Texture2D filled,
      Color fallbackColor)
    {
      var count = Math.Min(3, target);
      for (var index = 0; index < count; index++)
      {
        var isFilled = index < wins;
        var texture = isFilled ? filled : empty;
        var rect = new Rect(origin.x + index * 46f, origin.y, 32f, 32f);
        if (texture != null)
        {
          GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
          continue;
        }

        var previousColor = GUI.color;
        GUI.color = isFilled ? fallbackColor : new Color(0.18f, 0.18f, 0.2f, 0.9f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true);
        GUI.color = previousColor;
      }
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

    private void DrawRevealHistories(
      PrototypeHalliSnapshot snapshot,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      DrawRevealHistory(
        snapshot,
        PileSide.Left,
        _leftPileHistory,
        cards,
        uiArt);
      DrawRevealHistory(
        snapshot,
        PileSide.Right,
        _rightPileHistory,
        cards,
        uiArt);
    }

    private static void DrawRevealHistory(
      PrototypeHalliSnapshot snapshot,
      PileSide pile,
      IReadOnlyList<SharedPileCard> history,
      PlayableCardRenderer cards,
      HalliUiArtSet uiArt)
    {
      var railRect = HalliBoardLayout.SharedPileRail(pile);
      var rail = SelectSharedPileRail(history, uiArt);
      if (rail != null) GUI.DrawTexture(railRect, rail, ScaleMode.StretchToFill, true);
      for (var drawIndex = 0; drawIndex < history.Count; drawIndex++)
      {
        var cardIndex = HalliPileOverlapLayout.DrawOrderIndex(drawIndex, history.Count);
        var entry = history[cardIndex];
        if (snapshot.Phase == PrototypeSessionPhase.SequentialReveal
          && snapshot.RevealingCard.HasValue
          && snapshot.RevealingCard.Value.Id == entry.Card.Id
          && snapshot.RevealProgress < 1f)
        {
          continue;
        }
        cards.DrawAt(
          HalliBoardLayout.SharedPileCard(pile, cardIndex, history.Count),
          entry.Card);
      }
    }

    private static Texture2D SelectSharedPileRail(
      IReadOnlyList<SharedPileCard> history,
      HalliUiArtSet uiArt)
    {
      if (uiArt == null) return null;
      if (history.Count == 0) return uiArt.SharedPileRailIdle;
      return history[history.Count - 1].Actor == HalliActor.Player
        ? uiArt.SharedPileRailPlayerActive
        : uiArt.SharedPileRailAiActive;
    }

    private void DrawFlipReady(
      PrototypeHalliSnapshot snapshot,
      PlayableGamePhase gamePhase,
      PlayableDevStyles styles)
    {
      if (gamePhase != PlayableGamePhase.Halli
        || snapshot.Phase != PrototypeSessionPhase.ReadyToFlip
        || snapshot.FlipCount != 0) return;
      GUI.Label(
        new Rect(350f, 148f, 260f, 44f),
        _localization.Get("UI_THREE_CALL_FLIP_READY"),
        styles.Status);
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
        L("UI_HALLI_BELL"),
        canRing,
        snapshot,
        uiArt,
        styles)) leftBell();
      if (_bellControl.Draw(
        PileSide.Right,
        HalliBoardLayout.RightBellVisual,
        HalliBoardLayout.RightBellHit,
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

    private void DrawRevealMotion(PrototypeHalliSnapshot snapshot, PlayableCardRenderer cards)
    {
      if (snapshot.Phase != PrototypeSessionPhase.SequentialReveal
        || !snapshot.RevealingCard.HasValue
        || !snapshot.RevealingActor.HasValue
        || !snapshot.RevealingPile.HasValue
        || snapshot.RevealProgress >= 1f)
      {
        return;
      }

      var history = GetRevealHistory(snapshot.RevealingPile.Value);
      var target = HalliBoardLayout.SharedPileCard(
        snapshot.RevealingPile.Value,
        Math.Max(0, history.Count - 1),
        Math.Max(1, history.Count));
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

    private static void DrawAiThinking(PrototypeHalliSnapshot snapshot, HalliUiArtSet uiArt)
    {
      if (uiArt?.AiThinkingSheet == null
        || snapshot.Phase != PrototypeSessionPhase.SequentialReveal
        || snapshot.RevealingActor != HalliActor.Ai
        || snapshot.RevealProgress >= 1f) return;
      const int frameCount = 8;
      var frame = Mathf.FloorToInt(Time.unscaledTime / 0.08f) % frameCount;
      GUI.DrawTextureWithTexCoords(
        new Rect(456f, 92f, 48f, 48f),
        uiArt.AiThinkingSheet,
        new Rect((float)frame / frameCount, 0f, 1f / frameCount, 1f),
        true);
    }

    private void UpdateRevealHistory(PrototypeHalliSnapshot snapshot)
    {
      if (_historyRoundSeed != snapshot.CombatRoundSeed)
      {
        ClearAllRevealHistory();
        _historyRoundSeed = snapshot.CombatRoundSeed;
      }

      if (snapshot.RevealingCard.HasValue
        && snapshot.RevealingActor.HasValue
        && snapshot.RevealingRelativeSide.HasValue
        && (!_lastHistoryReveal.HasValue
          || _lastHistoryReveal.Value != snapshot.RevealingCard.Value.Id))
      {
        var pile = HalliPileOverlapLayout.PhysicalPile(
          snapshot.RevealingActor.Value,
          snapshot.RevealingRelativeSide.Value);
        var history = GetRevealHistory(pile);
        AppendRecent(
          history,
          new SharedPileCard(snapshot.RevealingCard.Value, snapshot.RevealingActor.Value));
        _lastHistoryReveal = snapshot.RevealingCard.Value.Id;
      }

      if (snapshot.LastAcquiredCards.Count > 0)
      {
        var acquisitionId = snapshot.LastAcquiredCards[0].Id;
        if (!_lastHistoryAcquisition.HasValue || _lastHistoryAcquisition.Value != acquisitionId)
        {
          if (snapshot.LastAcquiredPile == PileSide.Left)
          {
            _leftPileHistory.Clear();
          }
          else if (snapshot.LastAcquiredPile == PileSide.Right)
          {
            _rightPileHistory.Clear();
          }
          _lastHistoryAcquisition = acquisitionId;
        }
      }
    }

    private List<SharedPileCard> GetRevealHistory(PileSide pile)
    {
      return pile == PileSide.Left ? _leftPileHistory : _rightPileHistory;
    }

    private static void AppendRecent(List<SharedPileCard> history, SharedPileCard card)
    {
      if (history.Count == HalliPileOverlapLayout.MaximumPileCards) history.RemoveAt(0);
      history.Add(card);
    }

    private void ClearAllRevealHistory()
    {
      _leftPileHistory.Clear();
      _rightPileHistory.Clear();
      _lastHistoryReveal = null;
      _lastHistoryAcquisition = null;
    }

    private readonly struct SharedPileCard
    {
      public SharedPileCard(Card card, HalliActor actor)
      {
        Card = card;
        Actor = actor;
      }

      public Card Card { get; }
      public HalliActor Actor { get; }
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
