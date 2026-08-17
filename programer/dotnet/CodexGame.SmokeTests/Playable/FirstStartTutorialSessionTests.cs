using CodexGame.Application.Playable;

namespace CodexGame.SmokeTests.Playable
{
  internal static class FirstStartTutorialSessionTests
  {
    public static void Run(TestHarness tests)
    {
      var session = new FirstStartTutorialSession();
      tests.Check(
        session.RequestStart() == FirstStartRequest.ShowTutorial && !session.IsCompleted,
        "The first START of a browser session must open the four-page tutorial before gameplay.");
      tests.Check(
        session.CompleteTutorial()
          && !session.CompleteTutorial()
          && session.RequestStart() == FirstStartRequest.StartBattle,
        "Completing or skipping the tutorial must unlock START exactly once for the current session.");
    }
  }
}
