using CodexGame.Core.Halli;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class HalliPileOverlapLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        HalliPileOverlapLayout.PhysicalPile(HalliActor.Player, HalliRelativeSide.Left)
            == PileSide.Left
          && HalliPileOverlapLayout.PhysicalPile(HalliActor.Ai, HalliRelativeSide.Right)
            == PileSide.Left
          && HalliPileOverlapLayout.PhysicalPile(HalliActor.Ai, HalliRelativeSide.Left)
            == PileSide.Right
          && HalliPileOverlapLayout.PhysicalPile(HalliActor.Player, HalliRelativeSide.Right)
            == PileSide.Right,
        "Player-left and AI-right must share the left pile; AI-left and player-right must share the right pile.");
      tests.Check(
        HalliPileOverlapLayout.CardX(PileSide.Left, 0, 2) == 254f
          && HalliPileOverlapLayout.CardY(0, 2) == 190f
          && HalliPileOverlapLayout.CardX(PileSide.Left, 1, 2) == 296f
          && HalliPileOverlapLayout.CardY(1, 2) == 212f
          && HalliPileOverlapLayout.CardX(PileSide.Right, 0, 2) == 614f
          && HalliPileOverlapLayout.CardX(PileSide.Right, 1, 2) == 656f,
        "The first card must remain above the later card while the shared pile expands downward.");
      tests.Check(
        HalliPileOverlapLayout.MaximumPileCards == 2
          && HalliPileOverlapLayout.DrawOrderIndex(0, 2) == 1
          && HalliPileOverlapLayout.DrawOrderIndex(1, 2) == 0,
        "Each shared pile must keep at most two cards and draw the previous card above the newest card.");
    }
  }
}
