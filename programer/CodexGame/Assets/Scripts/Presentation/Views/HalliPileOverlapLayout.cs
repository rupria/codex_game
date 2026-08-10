using System;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Presentation.Views
{
  internal static class HalliPileOverlapLayout
  {
    public const float CardWidth = 96f;
    public const float CardStep = 84f;
    public const float LeftStartX = 192f;
    public const float RightStartX = 588f;
    public const float LeftHistoryStartX = 212f;
    public const float RightHistoryStartX = 584f;
    public const float HistoryLaneStep = 100f;
    public const float HistoryBaseY = 194f;
    public const float HistoryVerticalStep = 12f;
    public const int MaximumHistoryCards = 3;

    public static float X(bool left, int index)
    {
      if (index < 0 || index > 1) throw new ArgumentOutOfRangeException(nameof(index));
      return (left ? LeftStartX : RightStartX) + CardStep * index;
    }

    public static PileSide PhysicalPile(HalliActor actor, HalliRelativeSide side)
    {
      if (!Enum.IsDefined(typeof(HalliActor), actor))
      {
        throw new ArgumentOutOfRangeException(nameof(actor));
      }
      if (!Enum.IsDefined(typeof(HalliRelativeSide), side))
      {
        throw new ArgumentOutOfRangeException(nameof(side));
      }

      if (actor == HalliActor.Player)
      {
        return side == HalliRelativeSide.Left ? PileSide.Left : PileSide.Right;
      }
      return side == HalliRelativeSide.Left ? PileSide.Right : PileSide.Left;
    }

    public static float HistoryX(HalliActor actor, HalliRelativeSide side)
    {
      var pile = PhysicalPile(actor, side);
      var lane = pile == PileSide.Left
        ? actor == HalliActor.Player ? 0 : 1
        : actor == HalliActor.Ai ? 0 : 1;
      return (pile == PileSide.Left ? LeftHistoryStartX : RightHistoryStartX)
        + HistoryLaneStep * lane;
    }

    public static float HistoryY(int historyIndex, int historyCount)
    {
      if (historyCount < 1 || historyCount > MaximumHistoryCards)
      {
        throw new ArgumentOutOfRangeException(nameof(historyCount));
      }
      if (historyIndex < 0 || historyIndex >= historyCount)
      {
        throw new ArgumentOutOfRangeException(nameof(historyIndex));
      }

      var distanceFromNewest = historyCount - 1 - historyIndex;
      return HistoryBaseY + HistoryVerticalStep * distanceFromNewest;
    }
  }
}
