using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class GuideModalStateTests
  {
    public static void Run(TestHarness tests)
    {
      var state = new GuideModalState();
      tests.Check(!state.IsOpen, "The guide must start closed so the main timer cannot start implicitly.");

      state.Open();
      tests.Check(
        state.IsOpen && state.PageIndex == 0 && !state.CanMovePrevious && state.CanMoveNext,
        "Opening the guide must reset it to page 1 and clamp previous navigation.");

      for (var index = 0; index < GuideModalState.PageCount + 2; index++) state.MoveNext();
      tests.Check(
        state.PageIndex == GuideModalState.PageCount - 1 && !state.CanMoveNext,
        "Guide navigation must stop at page 4.");

      state.MovePrevious();
      tests.Check(state.PageIndex == 2, "Previous navigation must move back exactly one page.");

      state.Close();
      var closedPage = state.PageIndex;
      state.MovePrevious();
      state.MoveNext();
      tests.Check(
        !state.IsOpen && state.PageIndex == closedPage,
        "A closed guide must reject page input instead of leaking it to game state.");
    }
  }
}
