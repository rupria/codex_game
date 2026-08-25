using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerTableLayout
  {
    public static readonly Rect AiHealth = new Rect(252f, 112f, 116f, 52f);
    public static readonly Rect AiItem = new Rect(638f, 108f, 64f, 64f);
    public static readonly Rect PlayerHealth = new Rect(252f, 372f, 116f, 52f);
    public static readonly Rect PlayerItem = new Rect(650f, 350f, 88f, 76f);
    public static readonly Rect WinVisual = new Rect(236f, 456f, 212f, 64f);
    public static readonly Rect LoseVisual = new Rect(512f, 456f, 212f, 64f);
    public static readonly Rect WinText = WinVisual;
    public static readonly Rect LoseText = LoseVisual;
    public static readonly Rect WinHit = new Rect(228f, 448f, 228f, 80f);
    public static readonly Rect LoseHit = new Rect(504f, 448f, 228f, 80f);
    public static readonly Rect ContinueVisual = new Rect(
      PokerResultPanelLayout.ContinueVisualX,
      PokerResultPanelLayout.ContinueVisualY,
      PokerResultPanelLayout.ContinueVisualWidth,
      PokerResultPanelLayout.ContinueVisualHeight);
    public static readonly Rect ContinueText = new Rect(
      PokerResultPanelLayout.ContinueTextX,
      PokerResultPanelLayout.ContinueTextY,
      PokerResultPanelLayout.ContinueTextWidth,
      PokerResultPanelLayout.ContinueTextHeight);
    public static readonly Rect ContinueHit = new Rect(
      PokerResultPanelLayout.ContinueHitX,
      PokerResultPanelLayout.ContinueHitY,
      PokerResultPanelLayout.ContinueHitWidth,
      PokerResultPanelLayout.ContinueHitHeight);
    public static readonly Rect PredictionTitlePlate = new Rect(350f, 20f, 260f, 52f);
    public static readonly Rect PredictionStageEmblem = new Rect(360f, 26f, 40f, 40f);
    public static readonly Rect PredictionTitleText = new Rect(410f, 32f, 180f, 28f);
    public static readonly Rect ResultSummary = new Rect(316f, 18f, 328f, 76f);
    public static readonly Rect ResultWinnerText = new Rect(344f, 25f, 272f, 31f);
    public static readonly Rect ResultHandText = new Rect(344f, 56f, 272f, 25f);
    public static readonly Rect PredictionTimerText = new Rect(690f, 24f, 150f, 32f);
    public static readonly Rect InsuranceRemainingIcon = new Rect(690f, 112f, 28f, 28f);
    public static readonly Rect InsuranceRemainingText = new Rect(724f, 112f, 118f, 28f);
    public static readonly Rect PredictionSuccessPlate = new Rect(716f, 366f, 224f, 44f);
    public static readonly Rect PredictionSuccessIcon = new Rect(730f, 374f, 28f, 28f);
    public static readonly Rect PredictionSuccessText = new Rect(766f, 374f, 160f, 28f);

    public static Rect AiCard(int index) => new Rect(374f + index * 74f, 78f, 64f, 90f);
    public static Rect CommunityCard(int index) => new Rect(411f + index * 74f, 196f, 64f, 90f);
    public static Rect PlayerCard(int index) => new Rect(374f + index * 74f, 326f, 64f, 90f);
  }
}
