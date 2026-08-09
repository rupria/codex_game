using System;

namespace CodexGame.Presentation.Views
{
  internal static class HalliPileOverlapLayout
  {
    public const float CardWidth = 96f;
    public const float CardStep = 84f;
    public const float LeftStartX = 192f;
    public const float RightStartX = 588f;

    public static float X(bool left, int index)
    {
      if (index < 0 || index > 1) throw new ArgumentOutOfRangeException(nameof(index));
      return (left ? LeftStartX : RightStartX) + CardStep * index;
    }
  }
}
