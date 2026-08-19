using System;
using System.Collections.Generic;
using CodexGame.Application.Poker;
using CodexGame.Application.Items;
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
    private const float AiRevealDuration =
      GameRules.AiJokerShowdownHighlightMicroseconds / 1_000_000f;
    private int _lastVisibleAiCardCount;
    private float _aiRevealStartedAt = float.NegativeInfinity;
    private bool _wasOutcomeVisible;
    private float _resultOverlayStartedAt = float.NegativeInfinity;

    public void Draw(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HealthUiArtSet healthArt,
      PokerUiArtSet pokerArt,
      PokerItemUiArtSet pokerItemArt,
      PokerResultUiArtSet pokerResultArt,
      PredictionRewardSnapshot predictionReward,
      PredictionInsuranceActivationSnapshot insuranceActivation,
      LocalizationRuntime localization,
      bool playerDamage,
      bool aiDamage,
      Action<PokerHandCategory> chooseJokerHand,
      Action<PredictionChoice> predict,
      Action advance)
    {
      UpdateRevealState(snapshot);
      UpdateResultOverlayState(snapshot);
      DrawGroupLabels(styles, localization);
      DrawHealth(snapshot, styles, healthArt, localization, playerDamage, aiDamage);
      DrawItems(pokerItemArt);
      DrawPredictionReward(
        predictionReward,
        insuranceActivation,
        pokerItemArt,
        styles,
        localization);
      DrawCards(snapshot, cards, pokerItemArt);

      if (snapshot.Phase == PokerRoundPhase.PlayerJokerPresentation)
      {
        DrawPlayerJokerPresentation(snapshot, styles, cards, localization);
      }
      else if (snapshot.Phase == PokerRoundPhase.AwaitingPlayerJokerChoice)
      {
        DrawJokerHandChoice(snapshot, styles, localization, chooseJokerHand);
      }
      else if (snapshot.Phase == PokerRoundPhase.AwaitingPrediction)
      {
        DrawPredictionTimer(snapshot, styles, localization);
      }
      else if (snapshot.Phase == PokerRoundPhase.ResultPending)
      {
        if (snapshot.ResultPresentationStep == PokerResultPresentationStep.Outcome)
        {
          DrawResult(
            snapshot,
            pokerArt,
            pokerItemArt,
            pokerResultArt,
            predictionReward,
            styles,
            localization);
        }
      }
      else
      {
        DrawResult(
          snapshot,
          pokerArt,
          pokerItemArt,
          pokerResultArt,
          predictionReward,
          styles,
          localization);
      }

      var canPredict = snapshot.Phase == PokerRoundPhase.AwaitingPrediction;
      if (DrawPredictionButton(
        PokerTableLayout.WinVisual,
        PokerTableLayout.WinHit,
        pokerArt?.PlayerPredictionIdle,
        pokerArt?.PlayerPredictionHover,
        localization.Get("UI_POKER_PLAYER_WINS"),
        canPredict,
        styles)) predict(PredictionChoice.PlayerWins);
      if (DrawPredictionButton(
        PokerTableLayout.LoseVisual,
        PokerTableLayout.LoseHit,
        pokerArt?.AiPredictionIdle,
        pokerArt?.AiPredictionHover,
        localization.Get("UI_POKER_PLAYER_LOSES"),
        canPredict,
        styles)) predict(PredictionChoice.PlayerLoses);

      if (snapshot.Phase == PokerRoundPhase.Resolved
        && GUI.Button(new Rect(748f, 470f, 164f, 48f), localization.Get("UI_COMMON_CONTINUE")))
      {
        advance();
      }

      DrawPredictionInsuranceActivation(
        insuranceActivation,
        pokerItemArt,
        styles,
        localization);
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
      var outcomeVisible = snapshot.Result != null
        && (snapshot.ResultPresentationStep == PokerResultPresentationStep.Outcome
          || snapshot.ResultPresentationStep == PokerResultPresentationStep.Complete);
      if (outcomeVisible && !_wasOutcomeVisible)
      {
        _resultOverlayStartedAt = Time.unscaledTime;
      }
      if (!outcomeVisible) _resultOverlayStartedAt = float.NegativeInfinity;
      _wasOutcomeVisible = outcomeVisible;
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
      LocalizationRuntime localization,
      bool playerDamage,
      bool aiDamage)
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
        healthArt,
        aiDamage);

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
        healthArt,
        playerDamage);
    }

    private static void DrawItems(PokerItemUiArtSet pokerItemArt)
    {
      if (pokerItemArt?.CrateClosed != null)
      {
        GUI.DrawTexture(
          new Rect(
            PokerTableLayout.PlayerItem.x - 16f,
            PokerTableLayout.PlayerItem.y - 16f,
            PokerTableLayout.PlayerItem.width + 32f,
            PokerTableLayout.PlayerItem.height + 32f),
          pokerItemArt.CrateClosed,
          ScaleMode.ScaleToFit,
          true);
        return;
      }

      PokerItemBoxRenderer.DrawEmpty(PokerTableLayout.PlayerItem);
    }

    private void DrawCards(
      PokerRoundSnapshot snapshot,
      PlayableCardRenderer cards,
      PokerItemUiArtSet pokerItemArt)
    {
      DrawFaceCards(snapshot.PublicCards, PokerTableLayout.CommunityCard, 2, cards);
      for (var index = 0; index < Math.Min(3, snapshot.PlayerPrivateCards.Count); index++)
      {
        var card = snapshot.PlayerPrivateCards[index];
        if (snapshot.Phase == PokerRoundPhase.PlayerJokerPresentation && card.IsJoker) continue;
        var rect = PokerTableLayout.PlayerCard(index);
        DrawShadow(rect);
        cards.DrawAt(rect, card, false, false);
        PokerItemCardStateRenderer.DrawWildInkState(rect, card, false, pokerItemArt);
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
        if (snapshot.VisibleAiPrivateCards[index].IsJoker && revealProgress < 1f)
        {
          var previousColor = GUI.color;
          GUI.color = new Color(1f, 0.72f, 0.18f, 0.28f + 0.28f * Mathf.Sin(revealProgress * Mathf.PI));
          GUI.DrawTexture(
            new Rect(rect.x - 8f, rect.y - 8f, rect.width + 16f, rect.height + 16f),
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill,
            true);
          GUI.color = previousColor;
        }
      }
    }

    private static void DrawPredictionReward(
      PredictionRewardSnapshot reward,
      PredictionInsuranceActivationSnapshot activation,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (reward == null) return;
      var x = 720f;
      var displayedCharges = activation?.IsActive == true
        ? activation.DisplayedCharges
        : reward.InsuranceChargesRemaining;
      var charge = art?.FindInsuranceCharges(displayedCharges);
      if (reward.InsuranceActivatedThisStage && charge != null)
      {
        GUI.DrawTexture(new Rect(x, 94f, 32f, 32f), charge, ScaleMode.ScaleToFit, true);
      }
      if (art?.PredictionActualSuccess != null)
      {
        GUI.DrawTexture(new Rect(x, 132f, 28f, 28f), art.PredictionActualSuccess, ScaleMode.ScaleToFit, true);
      }
      if (art?.PredictionInsuredSuccess != null)
      {
        GUI.DrawTexture(new Rect(x, 164f, 28f, 28f), art.PredictionInsuredSuccess, ScaleMode.ScaleToFit, true);
      }
      GUI.Label(
        new Rect(x + 34f, 98f, 190f, 22f),
        localization.Get(
          "UI_PREDICTION_CHARGES",
          new LocalizationArgument("count", displayedCharges)),
        styles.Small);
      GUI.Label(
        new Rect(x + 34f, 136f, 190f, 22f),
        localization.Get(
          "UI_PREDICTION_ACTUAL_COUNT",
          new LocalizationArgument("count", reward.ActualSuccessCount)),
        styles.Small);
      GUI.Label(
        new Rect(x + 34f, 168f, 190f, 22f),
        localization.Get(
          "UI_PREDICTION_INSURED_COUNT",
          new LocalizationArgument("count", reward.InsuredSuccessCount)),
        styles.Small);
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
      if (progress >= 0.35f && progress <= 0.85f)
      {
        var highlightProgress = (progress - 0.35f) / 0.5f;
        var previousColor = GUI.color;
        GUI.color = new Color(
          1f,
          0.72f,
          0.18f,
          0.32f + 0.22f * Mathf.Sin(highlightProgress * Mathf.PI));
        GUI.DrawTexture(
          new Rect(target.x - 8f, target.y - 8f, target.width + 16f, target.height + 16f),
          Texture2D.whiteTexture,
          ScaleMode.StretchToFill,
          true);
        GUI.color = previousColor;
      }
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

    private static void DrawJokerHandChoice(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<PokerHandCategory> chooseJokerHand)
    {
      var panel = new Rect(238f, 110f, 484f, 300f);
      GUI.Box(panel, GUIContent.none);
      GUI.Label(
        new Rect(panel.x + 22f, panel.y + 14f, panel.width - 44f, 34f),
        localization.Get("UI_POKER_JOKER_CHOICE_TITLE"),
        styles.Status);
      GUI.Label(
        new Rect(panel.x + 22f, panel.y + 48f, panel.width - 44f, 30f),
        localization.Get("UI_POKER_JOKER_CHOICE_GUIDE"),
        styles.Small);

      for (var index = 0; index < snapshot.LegalPlayerJokerOptions.Count; index++)
      {
        var option = snapshot.LegalPlayerJokerOptions[index];
        var column = index % 2;
        var row = index / 2;
        var button = new Rect(
          panel.x + 22f + column * 222f,
          panel.y + 86f + row * 44f,
          208f,
          36f);
        if (GUI.Button(button, CategoryName(option.Category, localization)))
        {
          chooseJokerHand(option.Category);
        }
      }
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
      GUI.enabled = enabled;
      var clicked = GUI.Button(hitRect, new GUIContent(string.Empty, label), GUIStyle.none);
      GUI.enabled = true;
      return clicked;
    }

    private void DrawResult(
      PokerRoundSnapshot snapshot,
      PokerUiArtSet pokerArt,
      PokerItemUiArtSet pokerItemArt,
      PokerResultUiArtSet pokerResultArt,
      PredictionRewardSnapshot predictionReward,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot.Result == null) return;
      var comparison = snapshot.Result.Comparison;
      var winner = localization.Get(
        comparison.Winner == PokerWinner.Player ? "UI_ACTOR_PLAYER" : "UI_ACTOR_AI");
      var predictionSucceeded = snapshot.Result.Prediction.IsCorrect;
      var insuranceApplied = !snapshot.Result.WasHandConfirmationTimeout
        && predictionReward?.LastResultWasInsured == true;
      var prediction = snapshot.Result.WasHandConfirmationTimeout
        ? localization.Get("UI_ITEM_CONFIRM_TIMEOUT")
        : insuranceApplied
          ? localization.Get("UI_ITEM_INSURANCE_APPLIED")
        : snapshot.Result.Prediction.Choice == PredictionChoice.Skipped
        ? localization.Get("UI_PREDICTION_SKIPPED")
        : localization.Get(predictionSucceeded ? "UI_PREDICTION_CORRECT" : "UI_PREDICTION_WRONG");
      var message = localization.Get(
        "UI_POKER_RESULT_SUMMARY",
        new LocalizationArgument("winner", winner),
        new LocalizationArgument("playerHand", CategoryName(comparison.PlayerValue.Category, localization)),
        new LocalizationArgument("aiHand", CategoryName(comparison.AiValue.Category, localization)),
        new LocalizationArgument("prediction", prediction),
        new LocalizationArgument("playerHp", snapshot.Health.Player),
        new LocalizationArgument("aiHp", snapshot.Health.Ai));
      string itemStatus = null;
      if (snapshot.Result.WasHandConfirmationTimeout
        || insuranceApplied
        || snapshot.Result.WasPlayerDamagePrevented)
      {
        itemStatus = snapshot.Result.WasHandConfirmationTimeout
          ? localization.Get("UI_ITEM_CONFIRM_TIMEOUT")
          : snapshot.Result.WasPlayerDamagePrevented
            ? localization.Get("UI_BARREL_DAMAGE_PREVENTED")
            : localization.Get("UI_ITEM_INSURANCE_APPLIED");
      }

      var predictionBadge = insuranceApplied
        ? pokerItemArt?.PredictionInsuredSuccess
        : predictionSucceeded
          ? pokerItemArt?.PredictionActualSuccess ?? pokerArt?.PredictionResultFilled
          : pokerArt?.PredictionResultEmpty;
      var visualState = snapshot.Result.WasHandConfirmationTimeout
        ? PokerResultPanelVisualState.Neutral
        : predictionSucceeded || insuranceApplied
          ? PokerResultPanelVisualState.Success
          : PokerResultPanelVisualState.Failure;
      PokerResultOverlayRenderer.Draw(
        message,
        itemStatus,
        visualState,
        predictionBadge,
        pokerResultArt,
        styles);
      DrawBarrelDefenseResult(snapshot, pokerItemArt, localization, styles);
    }

    private static void DrawPredictionInsuranceActivation(
      PredictionInsuranceActivationSnapshot snapshot,
      PokerItemUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot == null || !snapshot.IsActive) return;
      GUI.Box(new Rect(348f, 184f, 264f, 172f), GUIContent.none);
      var badge = art?.PredictionInsuredSuccess;
      if (badge != null)
      {
        var pulse = 1f + 0.12f * Mathf.Sin(snapshot.Progress * Mathf.PI);
        var size = 96f * pulse;
        GUI.DrawTexture(
          new Rect(480f - size * 0.5f, 250f - size * 0.5f, size, size),
          badge,
          ScaleMode.ScaleToFit,
          true);
      }
      else
      {
        var icon = art?.FindPopupIcon(Core.Items.GameItemId.PredictionInsurance);
        if (icon != null)
        {
          GUI.DrawTexture(new Rect(432f, 202f, 96f, 96f), icon, ScaleMode.ScaleToFit, true);
        }
      }
      GUI.Label(
        new Rect(364f, 304f, 232f, 28f),
        localization.Get("UI_ITEM_INSURANCE_APPLIED"),
        styles.Status);
      GUI.Label(
        new Rect(364f, 332f, 232f, 20f),
        $"{snapshot.ChargesBefore} → {snapshot.DisplayedCharges}",
        styles.Small);
    }

    private void DrawBarrelDefenseResult(
      PokerRoundSnapshot snapshot,
      PokerItemUiArtSet art,
      LocalizationRuntime localization,
      PlayableDevStyles styles)
    {
      if (!snapshot.Result.WasPlayerDamagePrevented) return;
      var duration = GameRules.BarrelDefensePresentationMicroseconds / 1_000_000f;
      var progress = Mathf.Clamp01((Time.unscaledTime - _resultOverlayStartedAt) / duration);
      if (progress < 1f && art?.BarrelDefenseBreakSheet != null)
      {
        const int frameCount = 8;
        var frame = Mathf.Min(frameCount - 1, Mathf.FloorToInt(progress * frameCount));
        GUI.DrawTextureWithTexCoords(
          new Rect(670f, 206f, 96f, 96f),
          art.BarrelDefenseBreakSheet,
          new Rect(frame / (float)frameCount, 0f, 1f / frameCount, 1f),
          true);
      }
      if (progress >= 1f)
      {
        if (art?.BarrelDefenseBroken != null)
        {
          GUI.DrawTexture(
            new Rect(686f, 222f, 64f, 64f),
            art.BarrelDefenseBroken,
            ScaleMode.ScaleToFit,
            true);
        }
        if (art?.BarrelHpPreservedMarker != null)
        {
          GUI.DrawTexture(
            new Rect(786f, 234f, 32f, 32f),
            art.BarrelHpPreservedMarker,
            ScaleMode.ScaleToFit,
            true);
        }
      }
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
