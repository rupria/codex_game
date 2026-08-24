using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class PrivateSelectionPanelLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      tests.Check(
        PrivateSelectionPanelLayout.MaximumCandidateCount == 5
          && PrivateSelectionPanelLayout.CandidateX(0, 5) == 326f
          && PrivateSelectionPanelLayout.CandidateX(3, 5) == 722f
          && PrivateSelectionPanelLayout.CandidateY(3) == 136f
          && PrivateSelectionPanelLayout.CandidateX(4, 5) == 452f
          && PrivateSelectionPanelLayout.CandidateY(4) == 294f,
        "Private selection candidates must use the approved four-plus-one layout.");
      tests.Check(
        PrivateSelectionPanelLayout.CandidateX(4, 5)
          + PrivateSelectionPanelLayout.CandidateWidth
          <= PrivateSelectionPanelLayout.ConfirmHitX,
        "The fifth candidate must not overlap the single confirm hit area.");
      tests.Check(
        PrivateSelectionPanelLayout.SelectionCountX
          + PrivateSelectionPanelLayout.SelectionCountWidth
          < PrivateSelectionPanelLayout.ConfirmVisualX,
        "Selection count and confirm action must remain visually separate.");
      tests.Check(
        PrivateSelectionPanelLayout.ConfirmVisualX
          + PrivateSelectionPanelLayout.ConfirmVisualWidth
          <= PrivateSelectionPanelLayout.PanelX + PrivateSelectionPanelLayout.PanelWidth
          && PrivateSelectionPanelLayout.ConfirmVisualY
            + PrivateSelectionPanelLayout.ConfirmVisualHeight
            <= PrivateSelectionPanelLayout.PanelY + PrivateSelectionPanelLayout.PanelHeight,
        "The confirm action must remain inside the modal panel.");
    }
  }
}
