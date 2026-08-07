using System;
using System.Collections.Generic;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public static class HalliRevealSequence
  {
    private static readonly IReadOnlyList<HalliRevealStep> Steps = Array.AsReadOnly(new[]
    {
      new HalliRevealStep(1, HalliActor.Player, HalliRelativeSide.Left, PileSide.Left),
      new HalliRevealStep(2, HalliActor.Ai, HalliRelativeSide.Left, PileSide.Right),
      new HalliRevealStep(3, HalliActor.Player, HalliRelativeSide.Right, PileSide.Left),
      new HalliRevealStep(4, HalliActor.Ai, HalliRelativeSide.Right, PileSide.Right)
    });

    public static int Count => Steps.Count;

    public static HalliRevealStep GetStep(int zeroBasedIndex)
    {
      if (zeroBasedIndex < 0 || zeroBasedIndex >= Steps.Count)
      {
        throw new ArgumentOutOfRangeException(nameof(zeroBasedIndex));
      }

      return Steps[zeroBasedIndex];
    }
  }
}
