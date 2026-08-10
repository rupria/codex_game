using CodexGame.Core.Halli;
using CodexGame.Core.Shared;
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

    public static readonly Rect LeftPileFirst = new Rect(
      HalliPileOverlapLayout.X(true, 0), 194f, HalliPileOverlapLayout.CardWidth, 135f);
    public static readonly Rect LeftPileSecond = new Rect(
      HalliPileOverlapLayout.X(true, 1), 194f, HalliPileOverlapLayout.CardWidth, 135f);
    public static readonly Rect RightPileFirst = new Rect(
      HalliPileOverlapLayout.X(false, 0), 194f, HalliPileOverlapLayout.CardWidth, 135f);
    public static readonly Rect RightPileSecond = new Rect(
      HalliPileOverlapLayout.X(false, 1), 194f, HalliPileOverlapLayout.CardWidth, 135f);

    public static readonly Rect LeftBellVisual = new Rect(390f, 286f, 64f, 64f);
    public static readonly Rect RightBellVisual = new Rect(506f, 286f, 64f, 64f);
    public static readonly Rect LeftBellHit = new Rect(378f, 274f, 88f, 88f);
    public static readonly Rect RightBellHit = new Rect(494f, 274f, 88f, 88f);
    public static readonly Rect PlayerDeck = new Rect(440f, 354f, 80f, 100f);
    public static readonly Rect FlipDeck = PlayerDeck;
    public static readonly Rect FlipHit = new Rect(414f, 346f, 132f, 118f);

    public static readonly Rect PlayerTray = new Rect(34f, 390f, 378f, 130f);
    public static readonly Rect PlayerAcquiredFan = new Rect(46f, 424f, 354f, 78f);
    public static readonly Rect AiTray = new Rect(702f, 390f, 224f, 130f);
    public static readonly Rect AiAcquiredFan = new Rect(714f, 424f, 200f, 78f);
    public static readonly Rect AiStatus = new Rect(714f, 340f, 200f, 38f);

    private const float AcquiredCardWidth = 56f;
    private const float AcquiredCardHeight = 78f;
    private const float PreferredAcquiredCardStep = 30f;

    public static Rect PileCard(bool left, int index)
    {
      if (left) return index == 0 ? LeftPileFirst : LeftPileSecond;
      return index == 0 ? RightPileFirst : RightPileSecond;
    }

    public static Rect RevealTarget(bool left, int pileIndex)
    {
      return PileCard(left, Mathf.Clamp(pileIndex, 0, 1));
    }

    public static Rect RevealHistoryRail(HalliActor actor, HalliRelativeSide side)
    {
      var card = RevealHistoryCard(actor, side, 0, 1);
      return new Rect(card.x - 4f, card.y - 8f, 72f, 122f);
    }

    public static Rect RevealHistoryCard(
      HalliActor actor,
      HalliRelativeSide side,
      int historyIndex,
      int historyCount)
    {
      return new Rect(
        HalliPileOverlapLayout.HistoryX(actor, side),
        HalliPileOverlapLayout.HistoryY(historyIndex, historyCount),
        64f,
        90f);
    }

    public static Rect RevealPileSource(PileSide pile)
    {
      return pile == PileSide.Left
        ? RevealHistoryCard(HalliActor.Ai, HalliRelativeSide.Right, 0, 1)
        : RevealHistoryCard(HalliActor.Player, HalliRelativeSide.Right, 0, 1);
    }

    public static Rect PlayerAcquiredCard(int index, int count)
    {
      var fan = AcquiredCardFanLayout.Create(
        count,
        PlayerAcquiredFan.x,
        PlayerAcquiredFan.width,
        AcquiredCardWidth,
        PreferredAcquiredCardStep);
      return new Rect(fan.X(index), PlayerAcquiredFan.y, AcquiredCardWidth, AcquiredCardHeight);
    }

    public static Rect AiAcquiredCard(int index, int count)
    {
      var fan = AcquiredCardFanLayout.Create(
        count,
        AiAcquiredFan.x,
        AiAcquiredFan.width,
        AcquiredCardWidth,
        PreferredAcquiredCardStep);
      return new Rect(fan.X(index), AiAcquiredFan.y, AcquiredCardWidth, AcquiredCardHeight);
    }
  }
}
