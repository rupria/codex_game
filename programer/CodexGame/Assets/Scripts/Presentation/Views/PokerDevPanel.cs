using System;
using CodexGame.Application.Poker;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PokerDevPanel
  {
    public void Draw(
      PokerRoundSnapshot snapshot,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      Action<PredictionChoice> predict,
      Action advance)
    {
      GUILayout.BeginHorizontal();
      DrawHand("PLAYER PRIVATE", snapshot.PlayerPrivateCards, false, styles, cards);
      DrawHand("PUBLIC", snapshot.PublicCards, false, styles, cards);
      DrawAi(snapshot, styles, cards);
      GUILayout.EndHorizontal();

      if (snapshot.Phase == PokerRoundPhase.AwaitingPrediction)
      {
        GUILayout.Label(
          "AI private cards are concealed. Predict the player result before reveal.",
          styles.Status,
          GUILayout.Height(48f));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("PLAYER WINS  [1]", GUILayout.Height(55f))) predict(PredictionChoice.PlayerWins);
        if (GUILayout.Button("PLAYER LOSES  [2]", GUILayout.Height(55f))) predict(PredictionChoice.PlayerLoses);
        GUILayout.EndHorizontal();
        return;
      }

      DrawResult(snapshot, styles);
      if (GUILayout.Button("CONTINUE  [ENTER / SPACE]", GUILayout.Height(52f))) advance();
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
      PlayableCardRenderer cards)
    {
      GUILayout.BeginVertical();
      GUILayout.Label("AI PRIVATE", styles.Heading);
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

    private static void DrawResult(PokerRoundSnapshot snapshot, PlayableDevStyles styles)
    {
      if (snapshot.Result == null) return;
      var comparison = snapshot.Result.Comparison;
      var winner = comparison.Winner == PokerWinner.Player ? "PLAYER" : "AI";
      var prediction = snapshot.Result.Prediction.IsCorrect ? "CORRECT" : "WRONG";
      GUILayout.Label(
        "POKER WINNER: " + winner
        + "  |  PLAYER " + CategoryName(comparison.PlayerValue.Category)
        + " vs AI " + CategoryName(comparison.AiValue.Category)
        + "  |  PREDICTION " + prediction
        + "  |  HP PLAYER " + snapshot.Health.Player + " / AI " + snapshot.Health.Ai,
        styles.Status,
        GUILayout.Height(58f));
    }

    private static string CategoryName(PokerHandCategory category)
    {
      switch (category)
      {
        case PokerHandCategory.OnePair: return "PAIR";
        case PokerHandCategory.TwoPair: return "TWO PAIR";
        case PokerHandCategory.ThreeOfAKind: return "THREE KIND";
        case PokerHandCategory.Straight: return "STRAIGHT";
        case PokerHandCategory.Flush: return "FLUSH";
        case PokerHandCategory.FullHouse: return "FULL HOUSE";
        case PokerHandCategory.FourOfAKind: return "FOUR KIND";
        case PokerHandCategory.StraightFlush: return "STRAIGHT FLUSH";
        default: return "HIGH CARD";
      }
    }
  }
}
