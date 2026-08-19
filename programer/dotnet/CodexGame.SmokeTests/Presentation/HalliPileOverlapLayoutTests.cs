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
        HalliPileOverlapLayout.CardX(PileSide.Left, 0, 2) == 237f
          && HalliPileOverlapLayout.CardY(0, 2) == 209f
          && HalliPileOverlapLayout.CardX(PileSide.Left, 1, 2) == 296f
          && HalliPileOverlapLayout.CardY(1, 2) == 212f
          && HalliPileOverlapLayout.CardX(PileSide.Right, 0, 2) == 597f
          && HalliPileOverlapLayout.CardX(PileSide.Right, 1, 2) == 656f,
        "Each pile must show only the latest card and its immediate predecessor with readable ranks and suits.");
      tests.Check(
        HalliPileOverlapLayout.MaximumPileCards == 2
          && HalliPileOverlapLayout.DrawOrderIndex(0, 2) == 0
          && HalliPileOverlapLayout.DrawOrderIndex(1, 2) == 1,
        "The predecessor must draw first and the latest card last so the latest card remains on top.");
      tests.CheckThrows<System.ArgumentOutOfRangeException>(
        () => HalliPileOverlapLayout.CardX(PileSide.Left, 0, 3),
        "A third visible pile card must be rejected by the 0.1.2.6 layout contract.");
    }
  }
}
