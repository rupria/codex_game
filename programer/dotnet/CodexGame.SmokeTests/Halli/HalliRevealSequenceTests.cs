using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Halli
{
  internal static class HalliRevealSequenceTests
  {
    public static void Run(TestHarness tests)
    {
      var first = HalliRevealSequence.GetStep(0);
      var second = HalliRevealSequence.GetStep(1);
      var third = HalliRevealSequence.GetStep(2);
      var fourth = HalliRevealSequence.GetStep(3);

      tests.Check(
        first.Actor == HalliActor.Player
          && first.RelativeSide == HalliRelativeSide.Left
          && first.PhysicalPile == PileSide.Left
          && second.Actor == HalliActor.Ai
          && second.RelativeSide == HalliRelativeSide.Left
          && second.PhysicalPile == PileSide.Right
          && third.Actor == HalliActor.Player
          && third.RelativeSide == HalliRelativeSide.Right
          && third.PhysicalPile == PileSide.Right
          && fourth.Actor == HalliActor.Ai
          && fourth.RelativeSide == HalliRelativeSide.Right
          && fourth.PhysicalPile == PileSide.Left,
        "The four reveal steps must alternate actor-relative left/right across the two shared physical piles.");
    }
  }
}
