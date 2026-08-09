using System;
using System.Collections.Generic;
using CodexGame.Application.Poker;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PokerDevPanel
  {
    private const float AiRevealDuration = 0.48f;
    private int _lastVisibleAiCardCount;
    private float _aiRevealStartedAt = float.NegativeInfinity;
    private bool _wasResolved;
    private float _resultOverlayStartedAt = float.NegativeInfinity;

    public void Draw(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HealthUiArtSet healthArt,
      PokerUiArtSet pokerArt,
      LocalizationRuntime localization,
      Action<PredictionChoice> predict,
      Action advance)
    {
      UpdateRevealState(snapshot);
      UpdateResultOverlayState(snapshot);
      DrawGroupLabels(styles, localization);
      DrawHealth(snapshot, styles, healthArt, localization);
      DrawItems();
      DrawCards(snapshot, cards);

      if (snapshot.Phase == PokerRoundPhase.PlayerJokerPresentation)
      {
        DrawPlayerJokerPresentation(snapshot, styles, cards, localization);
      }
      else if (snapshot.Phase == PokerRoundPhase.AwaitingPrediction)
      {
        DrawPredictionTimer(snapshot, styles, localization);
      }
      else if (snapshot.Phase == PokerRoundPhase.ResultPending)
      {
        GUI.Label(
          new Rect(300f, 18f, 360f, 44f),
          localization.Get("UI_POKER_RESULT_PENDING"),
          styles.Status);
      }
      else
      {
        DrawResult(snapshot, styles, localization);
      }

      var canPredict = snapshot.Phase == PokerRoundPhase.AwaitingPrediction;
      if (DrawPredictionButton(
        PokerTableLayout.WinVisual,
        PokerTableLayout.WinHit,
        pokerArt?.WinIdle,
        pokerArt?.WinHover,
        localization.Get("UI_POKER_PLAYER_WINS"),
        canPredict,
        styles)) predict(PredictionChoice.PlayerWins);
      if (DrawPredictionButton(
        PokerTableLayout.LoseVisual,
        PokerTableLayout.LoseHit,
        pokerArt?.LoseIdle,
        pokerArt?.LoseHover,
        localization.Get("UI_POKER_PLAYER_LOSES"),
        canPredict,
        styles)) predict(PredictionChoice.PlayerLoses);

      if (snapshot.Phase == PokerRoundPhase.Resolved
        && GUI.Button(new Rect(748f, 470f, 164f, 48f), localization.Get("UI_COMMON_CONTINUE")))
      {
        advance();
      }
    }

    private void UpdateRevealState(PokerRoundSnapshot snapshot)
    {
      var visibleCount = snapshot.VisibleAiPrivateCards.Count;
      if (visibleCount > 0 && _lastVisibleAiCardCount == 0)
      {
        _aiRevealStartedAt = Time.unscaledTime;
      }
      if (visibleCount == 0) _aiRevealStartedAt = float.NegativeInfinity;
      _lastVisibleAiCardCount = visibleCount;
    }

    private void UpdateResultOverlayState(PokerRoundSnapshot snapshot)
    {
      var resolved = snapshot.Phase == PokerRoundPhase.Resolved && snapshot.Result != null;
      if (resolved && !_wasResolved) _resultOverlayStartedAt = Time.unscaledTime;
      if (!resolved) _resultOverlayStartedAt = float.NegativeInfinity;
      _wasResolved = resolved;
    }

    private static void DrawGroupLabels(PlayableDevStyles styles, LocalizationRuntime localization)
    {
      GUI.Label(new Rect(382f, 62f, 196f, 24f), localization.Get("UI_POKER_AI_PRIVATE"), styles.Small);
      GUI.Label(new Rect(416f, 192f, 130f, 24f), localization.Get("UI_POKER_PUBLIC"), styles.Small);
      GUI.Label(new Rect(382f, 316f, 196f, 24f), localization.Get("UI_POKER_PLAYER_PRIVATE"), styles.Small);
    }

    private static void DrawHealth(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      HealthUiArtSet healthArt,
      LocalizationRuntime localization)
    {
      GUI.Box(PokerTableLayout.AiHealth, GUIContent.none);
      GUI.Label(
        new Rect(PokerTableLayout.AiHealth.x, PokerTableLayout.AiHealth.y, PokerTableLayout.AiHealth.width, 20f),
        localization.Get("UI_ACTOR_AI"),
        styles.Small);
      HealthHeartRenderer.Draw(
        new Rect(PokerTableLayout.AiHealth.x, PokerTableLayout.AiHealth.y + 20f, PokerTableLayout.AiHealth.width, 30f),
        snapshot.Health.Ai,
        GameRules.StartingHealth,
        true,
        healthArt);

      GUI.Box(PokerTableLayout.PlayerHealth, GUIContent.none);
      GUI.Label(
        new Rect(PokerTableLayout.PlayerHealth.x, PokerTableLayout.PlayerHealth.y, PokerTableLayout.PlayerHealth.width, 20f),
        localization.Get("UI_ACTOR_PLAYER"),
        styles.Small);
      HealthHeartRenderer.Draw(
        new Rect(PokerTableLayout.PlayerHealth.x, PokerTableLayout.PlayerHealth.y + 20f, PokerTableLayout.PlayerHealth.width, 30f),
        snapshot.Health.Player,
        GameRules.StartingHealth,
        false,
        healthArt);
    }

    private static void DrawItems()
    {
      PokerItemBoxRenderer.DrawEmpty(PokerTableLayout.AiItem);
      PokerItemBoxRenderer.DrawEmpty(PokerTableLayout.PlayerItem);
    }

    private void DrawCards(PokerRoundSnapshot snapshot, PlayableCardRenderer cards)
    {
      DrawFaceCards(snapshot.PublicCards, PokerTableLayout.CommunityCard, 2, cards);
      for (var index = 0; index < Math.Min(3, snapshot.PlayerPrivateCards.Count); index++)
      {
        var card = snapshot.PlayerPrivateCards[index];
        if (snapshot.Phase == PokerRoundPhase.PlayerJokerPresentation && card.IsJoker) continue;
        var rect = PokerTableLayout.PlayerCard(index);
        DrawShadow(rect);
        cards.DrawAt(rect, card, false, false);
      }

      if (snapshot.VisibleAiPrivateCards.Count == 0)
      {
        for (var index = 0; index < 3; index++)
        {
          DrawShadow(PokerTableLayout.AiCard(index));
          cards.DrawBackAt(PokerTableLayout.AiCard(index), 180f, false);
        }
        return;
      }

      var revealProgress = Mathf.Clamp01((Time.unscaledTime - _aiRevealStartedAt) / AiRevealDuration);
      for (var index = 0; index < snapshot.VisibleAiPrivateCards.Count; index++)
      {
        var rect = PokerTableLayout.AiCard(index);
        DrawShadow(rect);
        if (revealProgress < 1f)
        {
          CardFlipMotion.Draw(
            cards,
            snapshot.VisibleAiPrivateCards[index],
            rect,
            rect,
            revealProgress,
            true,
            false);
        }
        else
        {
          cards.DrawAt(rect, snapshot.VisibleAiPrivateCards[index], false, false);
        }
      }
    }

    private static void DrawPlayerJokerPresentation(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      LocalizationRuntime localization)
    {
      var jokerIndex = -1;
      for (var index = 0; index < snapshot.PlayerPrivateCards.Count; index++)
      {
        if (snapshot.PlayerPrivateCards[index].IsJoker)
        {
          jokerIndex = index;
          break;
        }
      }
      if (jokerIndex < 0) return;

      var progress = 1f - Mathf.Clamp01(
        (float)snapshot.RemainingMicroseconds / GameRules.PlayerJokerPresentationMicroseconds);
      var target = PokerTableLayout.PlayerCard(jokerIndex);
      CardFlipMotion.Draw(
        cards,
        snapshot.PlayerPrivateCards[jokerIndex],
        PokerTableLayout.PlayerItem,
        target,
        progress,
        false,
        false);
      var previousColor = GUI.color;
      GUI.color = new Color(1f, 0.72f, 0.18f, 0.34f * Mathf.Sin(progress * Mathf.PI));
      GUI.DrawTexture(
        new Rect(target.x - 8f, target.y - 8f, target.width + 16f, target.height + 16f),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previousColor;
      GUI.Label(
        new Rect(280f, 18f, 400f, 44f),
        localization.Get("UI_POKER_JOKER_REVEAL"),
        styles.Status);
    }

    private static void DrawFaceCards(
      IReadOnlyList<Card> source,
      Func<int, Rect> rectAt,
      int maximum,
      PlayableCardRenderer cards)
    {
      var count = Math.Min(maximum, source.Count);
      for (var index = 0; index < count; index++)
      {
        var rect = rectAt(index);
        DrawShadow(rect);
        cards.DrawAt(rect, source[index], false, false);
      }
    }

    private static void DrawShadow(Rect rect)
    {
      var previousColor = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.42f);
      GUI.DrawTexture(
        new Rect(rect.x + 4f, rect.y + 5f, rect.width, rect.height),
        Texture2D.whiteTexture,
        ScaleMode.StretchToFill,
        true);
      GUI.color = previousColor;
    }

    private static void DrawPredictionTimer(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      var seconds = Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d);
      GUI.Label(
        new Rect(120f, 18f, 720f, 38f),
        localization.Get(
          "UI_POKER_PREDICTION_GUIDE",
          new LocalizationArgument("seconds", seconds.ToString("0"))),
        styles.Small);
    }

    private static bool DrawPredictionButton(
      Rect visualRect,
      Rect hitRect,
      Texture2D idle,
      Texture2D hover,
      string label,
      bool enabled,
      PlayableDevStyles styles)
    {
      var hovered = enabled && hitRect.Contains(Event.current.mousePosition);
      var pressed = hovered && Input.GetMouseButton(0);
      var previousColor = GUI.color;
      if (!enabled) GUI.color = new Color(0.42f, 0.42f, 0.44f, 0.76f);
      var texture = hovered && hover != null ? hover : idle;
      var drawRect = pressed
        ? new Rect(visualRect.x, visualRect.y + 2f, visualRect.width, visualRect.height)
        : visualRect;
      if (texture != null) GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(drawRect, GUIContent.none);
      GUI.color = previousColor;
      var labelBackground = new Rect(drawRect.x + 7f, drawRect.y + 38f, drawRect.width - 14f, 17f);
      GUI.color = new Color(0.04f, 0.025f, 0.02f, 0.82f);
      GUI.DrawTexture(labelBackground, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previousColor;
      GUI.Label(
        labelBackground,
        label,
        styles.Small);
      GUI.enabled = enabled;
      var clicked = GUI.Button(hitRect, GUIContent.none, GUIStyle.none);
      GUI.enabled = true;
      return clicked;
    }

    private void DrawResult(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot.Result == null) return;
      var comparison = snapshot.Result.Comparison;
      var winner = localization.Get(
        comparison.Winner == PokerWinner.Player ? "UI_ACTOR_PLAYER" : "UI_ACTOR_AI");
      var overlayState = PokerResultOverlayState.FromElapsedSeconds(
        Time.unscaledTime - _resultOverlayStartedAt);
      string message;
      var predictionSucceeded = snapshot.Result.Prediction.IsCorrect;
      if (overlayState.Step == PokerResultOverlayStep.Result)
      {
        message = localization.Get(
          "UI_POKER_RESULT_SUMMARY",
          new LocalizationArgument("winner", winner),
          new LocalizationArgument("playerHand", CategoryName(comparison.PlayerValue.Category, localization)),
          new LocalizationArgument("aiHand", CategoryName(comparison.AiValue.Category, localization)),
          new LocalizationArgument("prediction", string.Empty),
          new LocalizationArgument("playerHp", snapshot.Health.Player),
          new LocalizationArgument("aiHp", snapshot.Health.Ai));
      }
      else
      {
        message = snapshot.Result.Prediction.Choice == PredictionChoice.Skipped
          ? localization.Get("UI_PREDICTION_SKIPPED")
          : localization.Get(predictionSucceeded
            ? "UI_PREDICTION_CORRECT"
            : "UI_PREDICTION_WRONG");
      }

      PokerResultOverlayRenderer.Draw(
        new Rect(86f, 204f, 788f, 92f),
        message,
        overlayState.Step == PokerResultOverlayStep.Prediction,
        predictionSucceeded,
        styles);
    }

    private static string CategoryName(PokerHandCategory category, LocalizationRuntime localization)
    {
      switch (category)
      {
        case PokerHandCategory.OnePair: return localization.Get("UI_HAND_ONE_PAIR");
        case PokerHandCategory.TwoPair: return localization.Get("UI_HAND_TWO_PAIR");
        case PokerHandCategory.ThreeOfAKind: return localization.Get("UI_HAND_THREE_KIND");
        case PokerHandCategory.Straight: return localization.Get("UI_HAND_STRAIGHT");
        case PokerHandCategory.Flush: return localization.Get("UI_HAND_FLUSH");
        case PokerHandCategory.FullHouse: return localization.Get("UI_HAND_FULL_HOUSE");
        case PokerHandCategory.FourOfAKind: return localization.Get("UI_HAND_FOUR_KIND");
        case PokerHandCategory.StraightFlush: return localization.Get("UI_HAND_STRAIGHT_FLUSH");
        case PokerHandCategory.RoyalStraightFlush: return localization.Get("UI_HAND_ROYAL_STRAIGHT_FLUSH");
        default: return localization.Get("UI_HAND_HIGH_CARD");
      }
    }
  }
}
