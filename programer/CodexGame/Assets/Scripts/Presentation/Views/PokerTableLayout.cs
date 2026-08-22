using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerTableLayout
  {
    public static readonly Rect AiHealth = new Rect(252f, 112f, 116f, 52f);
    public static readonly Rect AiItem = new Rect(638f, 108f, 64f, 64f);
    public static readonly Rect PlayerHealth = new Rect(252f, 372f, 116f, 52f);
    public static readonly Rect PlayerItem = new Rect(650f, 350f, 88f, 76f);
    public static readonly Rect WinVisual = new Rect(132f, 454f, 244f, 66f);
    public static readonly Rect LoseVisual = new Rect(584f, 454f, 244f, 66f);
    public static readonly Rect WinText = new Rect(154f, 466f, 200f, 40f);
    public static readonly Rect LoseText = new Rect(606f, 466f, 200f, 40f);
    public static readonly Rect WinHit = new Rect(124f, 446f, 260f, 82f);
    public static readonly Rect LoseHit = new Rect(576f, 446f, 260f, 82f);
    public static readonly Rect ContinueVisual = new Rect(398f, 492f, 164f, 44f);
    public static readonly Rect ContinueText = new Rect(424f, 500f, 112f, 28f);
    public static readonly Rect ContinueHit = new Rect(390f, 482f, 180f, 56f);
    public static readonly Rect PredictionTitlePlate = new Rect(326f, 24f, 308f, 52f);
    public static readonly Rect PredictionStageEmblem = new Rect(338f, 30f, 40f, 40f);
    public static readonly Rect PredictionTitleText = new Rect(390f, 36f, 220f, 28f);
    public static readonly Rect ResultSummary = new Rect(316f, 18f, 328f, 76f);
    public static readonly Rect ResultWinnerText = new Rect(344f, 25f, 272f, 31f);
    public static readonly Rect ResultHandText = new Rect(344f, 56f, 272f, 25f);
    public static readonly Rect PredictionTimerText = new Rect(690f, 24f, 150f, 32f);
    public static readonly Rect InsuranceRemainingIcon = new Rect(690f, 112f, 28f, 28f);
    public static readonly Rect InsuranceRemainingText = new Rect(724f, 112f, 118f, 28f);
    public static readonly Rect PredictionSuccessIcon = new Rect(746f, 374f, 28f, 28f);
    public static readonly Rect PredictionSuccessText = new Rect(780f, 374f, 170f, 28f);

    public static Rect AiCard(int index) => new Rect(384f + index * 68f, 80f, 56f, 78f);
    public static Rect CommunityCard(int index) => new Rect(416f + index * 72f, 218f, 56f, 78f);
    public static Rect PlayerCard(int index) => new Rect(380f + index * 72f, 338f, 56f, 78f);
  }
}
