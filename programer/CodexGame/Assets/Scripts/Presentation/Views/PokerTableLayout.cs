using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerTableLayout
  {
    public static readonly Rect AiHealth = new Rect(252f, 112f, 116f, 52f);
    public static readonly Rect AiItem = new Rect(638f, 108f, 64f, 64f);
    public static readonly Rect PlayerHealth = new Rect(252f, 372f, 116f, 52f);
    public static readonly Rect PlayerItem = new Rect(650f, 350f, 88f, 76f);
    public static readonly Rect WinVisual = new Rect(139f, 456f, 232f, 64f);
    public static readonly Rect LoseVisual = new Rect(589f, 456f, 232f, 64f);
    public static readonly Rect WinText = new Rect(161f, 467f, 188f, 38f);
    public static readonly Rect LoseText = new Rect(611f, 467f, 188f, 38f);
    public static readonly Rect WinHit = new Rect(131f, 448f, 248f, 80f);
    public static readonly Rect LoseHit = new Rect(581f, 448f, 248f, 80f);
    public static readonly Rect ContinueVisual = new Rect(398f, 490f, 164f, 44f);
    public static readonly Rect ContinueText = new Rect(424f, 498f, 112f, 28f);
    public static readonly Rect ContinueHit = new Rect(390f, 482f, 180f, 56f);
    public static readonly Rect PredictionTitlePlate = new Rect(320f, 20f, 320f, 48f);
    public static readonly Rect PredictionStageEmblem = new Rect(332f, 24f, 40f, 40f);
    public static readonly Rect PredictionTitleText = new Rect(384f, 30f, 232f, 28f);
    public static readonly Rect PredictionTimerText = new Rect(690f, 24f, 150f, 32f);
    public static readonly Rect InsuranceRemainingIcon = new Rect(690f, 112f, 28f, 28f);
    public static readonly Rect InsuranceRemainingText = new Rect(724f, 112f, 118f, 28f);
    public static readonly Rect PredictionSuccessIcon = new Rect(624f, 428f, 28f, 28f);
    public static readonly Rect PredictionSuccessText = new Rect(660f, 428f, 132f, 28f);

    public static Rect AiCard(int index) => new Rect(384f + index * 68f, 80f, 56f, 78f);
    public static Rect CommunityCard(int index) => new Rect(416f + index * 72f, 218f, 56f, 78f);
    public static Rect PlayerCard(int index) => new Rect(380f + index * 72f, 338f, 56f, 78f);
  }
}
