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
    private Vector2 _jokerOptionScroll;

    public void Draw(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      HealthUiArtSet healthArt,
      PokerUiArtSet pokerArt,
      PokerItemUiArtSet pokerItemArt,
      PokerResultUiArtSet pokerResultArt,
      JokerRevealUiArtSet jokerRevealArt,
      PredictionRewardSnapshot predictionReward,
      PredictionInsuranceActivationSnapshot insuranceActivation,
      LocalizationRuntime localization,
      bool playerDamage,
      bool aiDamage,
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
        pokerArt,
        styles,
        localization);
      DrawCards(snapshot, cards, pokerItemArt, jokerRevealArt);

      if (snapshot.Phase == PokerRoundPhase.AwaitingPrediction)
      {
        DrawPredictionTimer(snapshot, styles);
      }
      else if (snapshot.Phase == PokerRoundPhase.ResultPending)
      {
        if (snapshot.ResultPresentationStep == PokerResultPresentationStep.Outcome)
        {
          DrawResult(
            snapshot,
            pokerItemArt,
            styles,
            localization);
        }
      }
      else
      {
        DrawResult(
          snapshot,
          pokerItemArt,
          styles,
          localization);
      }

      DrawPredictionHeader(snapshot, pokerArt, styles, localization);
      var canPredict = snapshot.Phase == PokerRoundPhase.AwaitingPrediction;
      var selectedPrediction = snapshot.Result?.Prediction.Choice ?? PredictionChoice.Skipped;
      if (DrawPredictionButton(
        PokerTableLayout.WinVisual,
        PokerTableLayout.WinHit,
        PokerTableLayout.WinText,
        pokerArt?.PlayerPredictionIdle,
        pokerArt?.PlayerPredictionHover,
        pokerArt?.PlayerPredictionSelected,
        pokerArt?.PlayerPredictionDisabled,
        localization.Get("UI_POKER_PLAYER_WINS"),
        canPredict,
        selectedPrediction == PredictionChoice.PlayerWins,
        styles)) predict(PredictionChoice.PlayerWins);
      if (DrawPredictionButton(
        PokerTableLayout.LoseVisual,
        PokerTableLayout.LoseHit,
        PokerTableLayout.LoseText,
        pokerArt?.AiPredictionIdle,
        pokerArt?.AiPredictionHover,
        pokerArt?.AiPredictionSelected,
        pokerArt?.AiPredictionDisabled,
        localization.Get("UI_POKER_PLAYER_LOSES"),
        canPredict,
        selectedPrediction == PredictionChoice.PlayerLoses,
        styles)) predict(PredictionChoice.PlayerLoses);

      if (snapshot.Phase == PokerRoundPhase.Resolved
        && DrawContinueButton(pokerArt, styles, localization))
      {
        advance();
      }

      DrawPredictionInsuranceActivation(
        insuranceActivation,
        pokerItemArt,
        styles,
        localization);
    }

    public void DrawJokerHandChoiceOverlay(
      PokerRoundSnapshot snapshot,
      JokerHandChoiceUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      int focusedIndex,
      Action<PokerHandCategory> chooseJokerHand)
    {
      if (snapshot?.Phase != PokerRoundPhase.AwaitingPlayerJokerChoice) return;

      var dimRect = new Rect(0f, 0f, 960f, 540f);
      var panelRect = new Rect(180f, 60f, 600f, 420f);
      if (art?.Dim != null)
      {
        GUI.DrawTexture(dimRect, art.Dim, ScaleMode.StretchToFill, true);
      }
      else
      {
        var previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.DrawTexture(dimRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        GUI.color = previous;
      }

      if (art?.Panel != null)
      {
        GUI.DrawTexture(panelRect, art.Panel, ScaleMode.StretchToFill, true);
      }
      else
      {
        GUI.Box(panelRect, GUIContent.none);
      }

      GUI.Label(
        new Rect(220f, 178f, 520f, 40f),
        localization.Get("UI_POKER_JOKER_CHOICE_TITLE"),
        styles.Heading);
      GUI.Label(
        new Rect(220f, 208f, 520f, 24f),
        localization.Get("UI_POKER_JOKER_CHOICE_GUIDE"),
        styles.Small);

      var options = snapshot.LegalPlayerJokerOptions;
      if (options.Count <= 5)
      {
        _jokerOptionScroll = Vector2.zero;
        for (var index = 0; index < options.Count; index++)
        {
          DrawJokerOptionButton(
            new Rect(260f, 236f + index * 46f, 440f, 44f),
            options[index],
            index == focusedIndex,
            art,
            styles,
            localization,
            chooseJokerHand);
        }
        return;
      }

      const float viewportHeight = 230f;
      var contentHeight = options.Count * 46f - 2f;
      KeepFocusedJokerOptionVisible(focusedIndex, viewportHeight, contentHeight);
      _jokerOptionScroll = GUI.BeginScrollView(
        new Rect(260f, 236f, 460f, viewportHeight),
        _jokerOptionScroll,
        new Rect(0f, 0f, 440f, contentHeight),
        false,
        true);
      for (var index = 0; index < options.Count; index++)
      {
        DrawJokerOptionButton(
          new Rect(0f, index * 46f, 440f, 44f),
          options[index],
          index == focusedIndex,
          art,
          styles,
          localization,
          chooseJokerHand);
      }
      GUI.EndScrollView();
    }

    private void UpdateRevealState(PokerRoundSnapshot snapshot)
    {
      var visibleCount = snapshot.VisibleAiPrivateCards.Count;
      if (visibleCount == GameRules.RequiredPrivateCards
        && _lastVisibleAiCardCount < GameRules.RequiredPrivateCards)
      {
        _aiRevealStartedAt = Time.unscaledTime;
      }
      if (visibleCount < GameRules.RequiredPrivateCards)
      {
        _aiRevealStartedAt = float.NegativeInfinity;
      }
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
          PokerTableLayout.PlayerItem,
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
      PokerItemUiArtSet pokerItemArt,
      JokerRevealUiArtSet jokerRevealArt)
    {
      DrawFaceCards(snapshot.PublicCards, PokerTableLayout.CommunityCard, 2, cards);
      for (var index = 0; index < Math.Min(3, snapshot.PlayerPrivateCards.Count); index++)
      {
        var card = snapshot.PlayerPrivateCards[index];
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
          DrawAiJokerReveal(rect, revealProgress, jokerRevealArt);
        }
      }
    }

    private static void DrawAiJokerReveal(
      Rect cardRect,
      float progress,
      JokerRevealUiArtSet art)
    {
      if (art == null || !art.IsComplete) return;
      DrawSheetFrame(
        art.GunsightRingSheet,
        8,
        progress,
        Centered(cardRect.center, 144f, 144f));
      DrawSheetFrame(
        art.MuzzleFlashSheet,
        8,
        progress,
        Centered(cardRect.center, 120f, 120f));
      DrawSheetFrame(
        art.CardGlintSheet,
        6,
        progress,
        Centered(cardRect.center, 112f, 156f));
    }

    private static void DrawPredictionReward(
      PredictionRewardSnapshot reward,
      PredictionInsuranceActivationSnapshot activation,
      PokerUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (reward == null) return;
      var displayedCharges = activation?.IsActive == true
        ? activation.DisplayedCharges
        : reward.InsuranceChargesRemaining;
      if (reward.InsuranceActivatedThisStage)
      {
        if (art?.InsuranceRemainingIcon != null)
        {
          GUI.DrawTexture(
            PokerTableLayout.InsuranceRemainingIcon,
            art.InsuranceRemainingIcon,
            ScaleMode.ScaleToFit,
            true);
        }
        GUI.Label(
          PokerTableLayout.InsuranceRemainingText,
          localization.Get(
            "UI_PREDICTION_CHARGES",
            new LocalizationArgument("count", displayedCharges)),
          styles.Small);
      }

      if (art?.PredictionSuccessIcon != null)
      {
        GUI.DrawTexture(
          PokerTableLayout.PredictionSuccessIcon,
          art.PredictionSuccessIcon,
          ScaleMode.ScaleToFit,
          true);
      }
      GUI.Label(
        PokerTableLayout.PredictionSuccessText,
        localization.Get(
          "UI_PREDICTION_SUCCESS_COUNT",
          new LocalizationArgument("count", reward.RewardSuccessCount)),
        styles.Small);
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
      PlayableDevStyles styles)
    {
      var seconds = Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d);
      GUI.Label(
        PokerTableLayout.PredictionTimerText,
        seconds.ToString("0") + "s",
        styles.Small);
    }

    private static void DrawPredictionHeader(
      PokerRoundSnapshot snapshot,
      PokerUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot.Phase != PokerRoundPhase.AwaitingPrediction
        && snapshot.Phase != PokerRoundPhase.ResultPending
        && snapshot.Phase != PokerRoundPhase.Resolved)
      {
        return;
      }

      if (art?.PredictionTitlePlate != null)
      {
        GUI.DrawTexture(
          PokerTableLayout.PredictionTitlePlate,
          art.PredictionTitlePlate,
          ScaleMode.StretchToFill,
          true);
      }
      if (art?.PredictionStageEmblem != null)
      {
        GUI.DrawTexture(
          PokerTableLayout.PredictionStageEmblem,
          art.PredictionStageEmblem,
          ScaleMode.ScaleToFit,
          true);
      }
      var title = localization.Get("UI_POKER_PREDICTION_TITLE");
      if (snapshot.Result != null)
      {
        var winner = localization.Get(
          snapshot.Result.Comparison.Winner == PokerWinner.Player
            ? "UI_ACTOR_PLAYER"
            : "UI_ACTOR_AI");
        title = winner + " " + localization.Get("UI_POKER_PLAYER_WINS");
      }
      GUI.Label(
        PokerTableLayout.PredictionTitleText,
        title,
        styles.Small);
    }

    private void KeepFocusedJokerOptionVisible(
      int focusedIndex,
      float viewportHeight,
      float contentHeight)
    {
      if (focusedIndex < 0) return;
      var optionTop = focusedIndex * 46f;
      var optionBottom = optionTop + 44f;
      if (optionTop < _jokerOptionScroll.y)
      {
        _jokerOptionScroll.y = optionTop;
      }
      else if (optionBottom > _jokerOptionScroll.y + viewportHeight)
      {
        _jokerOptionScroll.y = optionBottom - viewportHeight;
      }
      _jokerOptionScroll.y = Mathf.Clamp(
        _jokerOptionScroll.y,
        0f,
        Mathf.Max(0f, contentHeight - viewportHeight));
    }

    private static void DrawJokerOptionButton(
      Rect rect,
      JokerHandOption option,
      bool focused,
      JokerHandChoiceUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action<PokerHandCategory> chooseJokerHand)
    {
      var hovered = rect.Contains(Event.current.mousePosition);
      var selected = focused || (hovered && Input.GetMouseButton(0));
      var texture = art?.GetOptionTexture(true, hovered, selected);
      if (texture != null)
      {
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      }
      else
      {
        GUI.Box(rect, GUIContent.none);
      }
      var label = CategoryName(option.Category, localization);
      GUI.Label(rect, label, styles.Small);
      if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
      {
        chooseJokerHand(option.Category);
      }
    }

    private static bool DrawPredictionButton(
      Rect visualRect,
      Rect hitRect,
      Rect textRect,
      Texture2D idle,
      Texture2D hover,
      Texture2D selected,
      Texture2D disabled,
      string label,
      bool enabled,
      bool isSelected,
      PlayableDevStyles styles)
    {
      var hovered = enabled && hitRect.Contains(Event.current.mousePosition);
      var pressed = hovered && Input.GetMouseButton(0);
      var texture = isSelected && selected != null
        ? selected
        : !enabled && disabled != null
          ? disabled
          : hovered && hover != null
            ? hover
            : idle;
      var drawRect = pressed
        ? new Rect(visualRect.x, visualRect.y + 2f, visualRect.width, visualRect.height)
        : visualRect;
      if (texture != null) GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit, true);
      else GUI.Box(drawRect, GUIContent.none);
      var drawTextRect = pressed
        ? new Rect(textRect.x, textRect.y + 2f, textRect.width, textRect.height)
        : textRect;
      GUI.Label(drawTextRect, label, styles.Small);
      GUI.enabled = enabled;
      var clicked = GUI.Button(hitRect, GUIContent.none, GUIStyle.none);
      GUI.enabled = true;
      return clicked;
    }

    private static bool DrawContinueButton(
      PokerUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      var hovered = PokerTableLayout.ContinueHit.Contains(Event.current.mousePosition);
      var texture = hovered && art?.ResultContinueHover != null
        ? art.ResultContinueHover
        : art?.ResultContinueIdle;
      if (texture != null)
      {
        GUI.DrawTexture(
          PokerTableLayout.ContinueVisual,
          texture,
          ScaleMode.ScaleToFit,
          true);
      }
      else
      {
        GUI.Box(PokerTableLayout.ContinueVisual, GUIContent.none);
      }
      GUI.Label(
        PokerTableLayout.ContinueText,
        localization.Get("UI_COMMON_CONTINUE"),
        styles.Small);
      return GUI.Button(
        PokerTableLayout.ContinueHit,
        new GUIContent(string.Empty, localization.Get("UI_COMMON_CONTINUE")),
        GUIStyle.none);
    }

    private void DrawResult(
      PokerRoundSnapshot snapshot,
      PokerItemUiArtSet pokerItemArt,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot.Result == null) return;
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

    private static Rect Centered(Vector2 center, float width, float height)
    {
      return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
    }

    private static void DrawSheetFrame(
      Texture2D sheet,
      int frameCount,
      float progress,
      Rect rect)
    {
      if (sheet == null || frameCount <= 0) return;
      var frame = Mathf.Clamp(
        Mathf.FloorToInt(Mathf.Clamp01(progress) * frameCount),
        0,
        frameCount - 1);
      GUI.DrawTextureWithTexCoords(
        rect,
        sheet,
        new Rect((float)frame / frameCount, 0f, 1f / frameCount, 1f),
        true);
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
