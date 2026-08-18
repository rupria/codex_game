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
        HalliPileOverlapLayout.CardX(PileSide.Left, 0, 3) == 178f
          && HalliPileOverlapLayout.CardY(0, 3) == 206f
          && HalliPileOverlapLayout.CardX(PileSide.Left, 1, 3) == 237f
          && HalliPileOverlapLayout.CardY(1, 3) == 209f
          && HalliPileOverlapLayout.CardX(PileSide.Left, 2, 3) == 296f
          && HalliPileOverlapLayout.CardY(2, 3) == 212f
          && HalliPileOverlapLayout.CardX(PileSide.Right, 0, 2) == 597f
          && HalliPileOverlapLayout.CardX(PileSide.Right, 1, 2) == 656f,
        "Three cards must expose all ranks and suits with only five pixels of horizontal overlap.");
      tests.Check(
        HalliPileOverlapLayout.MaximumPileCards == 3
          && HalliPileOverlapLayout.DrawOrderIndex(0, 3) == 2
          && HalliPileOverlapLayout.DrawOrderIndex(1, 3) == 1
          && HalliPileOverlapLayout.DrawOrderIndex(2, 3) == 0,
        "Each shared pile must keep three cards and draw the oldest card last so it remains above later cards.");
    }
  }
}
