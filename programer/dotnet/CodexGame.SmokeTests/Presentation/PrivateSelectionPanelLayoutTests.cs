using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class PrivateSelectionPanelLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        PrivateSelectionPanelLayout.MaximumCandidateCount == 5
          && PrivateSelectionPanelLayout.CandidateX(0) == 326f
          && PrivateSelectionPanelLayout.CandidateX(3) == 722f
          && PrivateSelectionPanelLayout.CandidateY(3) == 140f
          && PrivateSelectionPanelLayout.CandidateX(4) == 326f
          && PrivateSelectionPanelLayout.CandidateY(4) == 286f,
        "Private selection candidates must use the approved four-plus-one layout.");
      tests.Check(
        PrivateSelectionPanelLayout.CandidateX(0) - PrivateSelectionPanelLayout.CandidateAreaX == 56f
          && PrivateSelectionPanelLayout.CandidateAreaX
            + PrivateSelectionPanelLayout.CandidateAreaWidth
            - PrivateSelectionPanelLayout.CandidateX(3)
            - PrivateSelectionPanelLayout.CandidateWidth == 56f,
        "The four-card row must keep 56px horizontal margins inside the candidate pool.");
      tests.Check(
        PrivateSelectionPanelLayout.CandidateTopY - PrivateSelectionPanelLayout.CandidateAreaY >= 16f
          && PrivateSelectionPanelLayout.CandidateAreaY
            + PrivateSelectionPanelLayout.CandidateAreaHeight
            - PrivateSelectionPanelLayout.CandidateBottomY
            - PrivateSelectionPanelLayout.CandidateHeight >= 16f,
        "Candidate rows must keep at least 16px vertical margins inside the pool.");
      tests.Check(
        PrivateSelectionPanelLayout.ConfirmVisualX == 72f
          && PrivateSelectionPanelLayout.ConfirmVisualY == 366f
          && PrivateSelectionPanelLayout.ConfirmVisualWidth == 184f
          && PrivateSelectionPanelLayout.ConfirmVisualHeight == 120f
          && PrivateSelectionPanelLayout.ConfirmHitX == 64f
          && PrivateSelectionPanelLayout.ConfirmHitY == 358f
          && PrivateSelectionPanelLayout.ConfirmHitWidth == 200f
          && PrivateSelectionPanelLayout.ConfirmHitHeight == 136f,
        "The only confirm action must use the approved tall lower-left panel and hit area.");
    }
  }
}
