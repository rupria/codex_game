using System;
using CodexGame.Application.Poker;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PokerDevPanel
  {
    public void Draw(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      LocalizationRuntime localization,
      Action<PredictionChoice> predict,
      Action advance)
    {
      GUILayout.BeginHorizontal();
      DrawHand(localization.Get("UI_POKER_PLAYER_PRIVATE"), snapshot.PlayerPrivateCards, false, styles, cards);
      DrawHand(localization.Get("UI_POKER_PUBLIC"), snapshot.PublicCards, false, styles, cards);
      DrawAi(snapshot, styles, cards, localization);
      GUILayout.EndHorizontal();

      if (snapshot.Phase == PokerRoundPhase.AwaitingPrediction)
      {
        var seconds = Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d);
        GUILayout.Label(
          localization.Get(
            "UI_POKER_PREDICTION_GUIDE",
            new LocalizationArgument("seconds", seconds.ToString("0"))),
          styles.Status,
          GUILayout.Height(48f));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(localization.Get("UI_POKER_PLAYER_WINS"), GUILayout.Height(55f))) predict(PredictionChoice.PlayerWins);
        if (GUILayout.Button(localization.Get("UI_POKER_PLAYER_LOSES"), GUILayout.Height(55f))) predict(PredictionChoice.PlayerLoses);
        GUILayout.EndHorizontal();
        return;
      }

      if (snapshot.Phase == PokerRoundPhase.ResultPending)
      {
        GUILayout.Label(
          localization.Get("UI_POKER_RESULT_PENDING"),
          styles.Status,
          GUILayout.Height(48f));
        return;
      }

      DrawResult(snapshot, styles, localization);
      if (GUILayout.Button(localization.Get("UI_COMMON_CONTINUE"), GUILayout.Height(52f))) advance();
    }

    private static void DrawHand(
      string label,
      System.Collections.Generic.IReadOnlyList<Card> hand,
      bool concealed,
      PlayableDevStyles styles,
      PlayableCardRenderer cards)
    {
      GUILayout.BeginVertical();
      GUILayout.Label(label, styles.Heading);
      GUILayout.BeginHorizontal();
      for (var index = 0; index < hand.Count; index++)
      {
        if (concealed) cards.DrawBack(82f, 116f);
        else cards.Draw(hand[index], 82f, 116f);
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
    }

    private static void DrawAi(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      LocalizationRuntime localization)
    {
      GUILayout.BeginVertical();
      GUILayout.Label(localization.Get("UI_POKER_AI_PRIVATE"), styles.Heading);
      GUILayout.BeginHorizontal();
      if (snapshot.VisibleAiPrivateCards.Count == 0)
      {
        for (var index = 0; index < 3; index++) cards.DrawBack(82f, 116f);
      }
      else
      {
        for (var index = 0; index < snapshot.VisibleAiPrivateCards.Count; index++)
        {
          cards.Draw(snapshot.VisibleAiPrivateCards[index], 82f, 116f);
        }
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
    }

    private static void DrawResult(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (snapshot.Result == null) return;
      var comparison = snapshot.Result.Comparison;
      var winner = localization.Get(
        comparison.Winner == PokerWinner.Player ? "UI_ACTOR_PLAYER" : "UI_ACTOR_AI");
      var prediction = snapshot.Result.Prediction.Choice == PredictionChoice.Skipped
        ? localization.Get("UI_PREDICTION_SKIPPED")
        : localization.Get(snapshot.Result.Prediction.IsCorrect
          ? "UI_PREDICTION_CORRECT"
          : "UI_PREDICTION_WRONG");
      GUILayout.Label(
        localization.Get(
          "UI_POKER_RESULT_SUMMARY",
          new LocalizationArgument("winner", winner),
          new LocalizationArgument("playerHand", CategoryName(comparison.PlayerValue.Category, localization)),
          new LocalizationArgument("aiHand", CategoryName(comparison.AiValue.Category, localization)),
          new LocalizationArgument("prediction", prediction),
          new LocalizationArgument("playerHp", snapshot.Health.Player),
          new LocalizationArgument("aiHp", snapshot.Health.Ai)),
        styles.Status,
        GUILayout.Height(58f));
    }

    private static string CategoryName(
      PokerHandCategory category,
      LocalizationRuntime localization)
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
