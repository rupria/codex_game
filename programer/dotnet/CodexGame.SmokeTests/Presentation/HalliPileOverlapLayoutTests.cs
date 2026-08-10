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
        HalliPileOverlapLayout.X(true, 1) - HalliPileOverlapLayout.X(true, 0) == 84f
          && HalliPileOverlapLayout.X(false, 1) - HalliPileOverlapLayout.X(false, 0) == 84f,
        "Both Halli piles must expose the latest two 96px cards at the approved 84px step.");
      tests.Check(
        HalliPileOverlapLayout.CardWidth - HalliPileOverlapLayout.CardStep == 12f,
        "The approved Halli pile layout must retain a 12px visual overlap.");
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
        HalliPileOverlapLayout.HistoryX(HalliActor.Player, HalliRelativeSide.Left) == 212f
          && HalliPileOverlapLayout.HistoryX(HalliActor.Ai, HalliRelativeSide.Right) == 312f
          && HalliPileOverlapLayout.HistoryX(HalliActor.Ai, HalliRelativeSide.Left) == 584f
          && HalliPileOverlapLayout.HistoryX(HalliActor.Player, HalliRelativeSide.Right) == 684f,
        "Each physical pile must keep two actor lanes inside the same left or right judgment area.");
      tests.Check(
        HalliPileOverlapLayout.HistoryY(2, 3) == 194f
          && HalliPileOverlapLayout.HistoryY(1, 3) == 206f
          && HalliPileOverlapLayout.HistoryY(0, 3) == 218f,
        "A new card must occupy the top position while older cards move downward by 12px.");
    }
  }
}
