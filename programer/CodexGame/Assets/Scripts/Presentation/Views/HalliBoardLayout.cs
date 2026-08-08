using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class HalliBoardLayout
  {
    public static readonly Rect PlayerScore = new Rect(36f, 24f, 250f, 58f);
    public static readonly Rect AiScore = new Rect(674f, 24f, 250f, 58f);
    public static readonly Rect PublicCard = new Rect(344f, 46f, 64f, 90f);
    public static readonly Rect LockedPublicCard = new Rect(552f, 46f, 64f, 90f);
    public static readonly Rect AiDeck = new Rect(440f, 40f, 80f, 100f);
    public static readonly Rect Status = new Rect(300f, 142f, 360f, 42f);

    public static readonly Rect LeftPileFirst = new Rect(248f, 194f, 96f, 135f);
    public static readonly Rect LeftPileSecond = new Rect(276f, 194f, 96f, 135f);
    public static readonly Rect RightPileFirst = new Rect(588f, 194f, 96f, 135f);
    public static readonly Rect RightPileSecond = new Rect(616f, 194f, 96f, 135f);

    public static readonly Rect LeftBellVisual = new Rect(390f, 286f, 64f, 64f);
    public static readonly Rect RightBellVisual = new Rect(506f, 286f, 64f, 64f);
    public static readonly Rect LeftBellHit = new Rect(378f, 274f, 88f, 88f);
    public static readonly Rect RightBellHit = new Rect(494f, 274f, 88f, 88f);
    public static readonly Rect PlayerDeck = new Rect(440f, 354f, 80f, 100f);
    public static readonly Rect FlipDeck = PlayerDeck;
    public static readonly Rect FlipHit = new Rect(414f, 346f, 132f, 118f);

    public static readonly Rect PlayerTray = new Rect(34f, 390f, 288f, 130f);
    public static readonly Rect AiTray = new Rect(702f, 390f, 224f, 130f);
    public static readonly Rect AiStatus = new Rect(714f, 340f, 200f, 38f);

    public static Rect PileCard(bool left, int index)
    {
      if (left) return index == 0 ? LeftPileFirst : LeftPileSecond;
      return index == 0 ? RightPileFirst : RightPileSecond;
    }

    public static Rect RevealTarget(bool left, int pileIndex)
    {
      return PileCard(left, Mathf.Clamp(pileIndex, 0, 1));
    }
  }
}
