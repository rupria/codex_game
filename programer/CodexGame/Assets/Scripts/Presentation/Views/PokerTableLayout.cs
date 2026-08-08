using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PokerTableLayout
  {
    public static readonly Rect AiHealth = new Rect(252f, 112f, 116f, 52f);
    public static readonly Rect AiItem = new Rect(638f, 108f, 64f, 64f);
    public static readonly Rect PlayerHealth = new Rect(252f, 372f, 116f, 52f);
    public static readonly Rect PlayerItem = new Rect(638f, 368f, 64f, 64f);
    public static readonly Rect WinVisual = new Rect(416f, 458f, 60f, 60f);
    public static readonly Rect LoseVisual = new Rect(488f, 458f, 60f, 60f);
    public static readonly Rect WinHit = new Rect(410f, 452f, 72f, 72f);
    public static readonly Rect LoseHit = new Rect(482f, 452f, 72f, 72f);

    public static Rect AiCard(int index) => new Rect(382f + index * 66f, 88f, 64f, 90f);
    public static Rect CommunityCard(int index) => new Rect(416f + index * 66f, 218f, 64f, 90f);
    public static Rect PlayerCard(int index) => new Rect(382f + index * 66f, 342f, 64f, 90f);
  }
}
