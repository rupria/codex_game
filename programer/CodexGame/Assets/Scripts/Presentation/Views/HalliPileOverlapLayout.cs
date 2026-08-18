using System;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Presentation.Views
{
  internal static class HalliPileOverlapLayout
  {
    public const float CardWidth = 64f;
    public const float CardHeight = 90f;
    public const float CardStepX = 59f;
    public const float CardStepY = 3f;
    public const float LeftNewestX = 296f;
    public const float RightNewestX = 656f;
    public const float NewestY = 212f;
    public const int MaximumPileCards = 3;

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

    public static float CardX(PileSide pile, int cardIndex, int cardCount)
    {
      ValidateCardIndex(cardIndex, cardCount);
      var newestX = pile == PileSide.Left ? LeftNewestX : RightNewestX;
      var distanceFromNewest = cardCount - 1 - cardIndex;
      return newestX - CardStepX * distanceFromNewest;
    }

    public static float CardY(int cardIndex, int cardCount)
    {
      ValidateCardIndex(cardIndex, cardCount);
      var distanceFromNewest = cardCount - 1 - cardIndex;
      return NewestY - CardStepY * distanceFromNewest;
    }

    public static int DrawOrderIndex(int drawIndex, int cardCount)
    {
      ValidateCardIndex(drawIndex, cardCount);
      return cardCount - 1 - drawIndex;
    }

    private static void ValidateCardIndex(int cardIndex, int cardCount)
    {
      if (cardCount < 1 || cardCount > MaximumPileCards)
      {
        throw new ArgumentOutOfRangeException(nameof(cardCount));
      }
      if (cardIndex < 0 || cardIndex >= cardCount)
      {
        throw new ArgumentOutOfRangeException(nameof(cardIndex));
      }
    }
  }
}
