using System;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Halli
{
  internal static class BellTimingTests
  {
    public static void Run(TestHarness tests)
    {
      var player = new GameTimestamp(100_000);

      tests.Check(
        ReactionResolver.Resolve(player, new GameTimestamp(66_701)) == ReactionWinner.Player,
        "An AI input 33,299us earlier must be simultaneous and favor the player.");
      tests.Check(
        ReactionResolver.Resolve(player, new GameTimestamp(66_700)) == ReactionWinner.Player,
        "An AI input exactly 33,300us earlier must be simultaneous and favor the player.");
      tests.Check(
        ReactionResolver.Resolve(player, new GameTimestamp(66_699)) == ReactionWinner.Ai,
        "An AI input 33,301us earlier must win.");
      tests.Check(
        ReactionResolver.Resolve(new GameTimestamp(50_000), new GameTimestamp(100_000)) == ReactionWinner.Player,
        "A player input outside the simultaneous boundary but earlier must win.");
      tests.Check(
        ReactionResolver.Resolve(null, new GameTimestamp(1)) == ReactionWinner.Ai,
        "A lone AI input must win.");
      tests.Check(
        ReactionResolver.Resolve(null, null) == ReactionWinner.None,
        "No bell inputs must produce no winner.");

      tests.CheckThrows<ArgumentOutOfRangeException>(
        () => _ = new GameTimestamp(-1),
        "A negative game timestamp must be rejected.");
      tests.CheckThrows<ArgumentOutOfRangeException>(
        () => _ = new DurationUs(-1),
        "A negative duration must be rejected.");

      var tracker = new BellWindowTracker();
      var firstWindow = tracker.OpenForCurrentField();
      tests.Check(tracker.IsOpen, "Opening a valid field must open a bell window.");
      tests.Check(tracker.IsCurrent(firstWindow), "The current field token must be accepted.");

      tracker.CloseForNextFlip();
      tests.Check(!tracker.IsOpen, "The next flip must close the previous bell window.");
      tests.Check(!tracker.IsCurrent(firstWindow), "A scheduled AI command from the previous field must become stale.");

      var secondWindow = tracker.OpenForCurrentField();
      tests.Check(secondWindow != firstWindow, "Each field must receive a distinct bell-window token.");
      tests.Check(tracker.IsCurrent(secondWindow), "Only the newly opened field token must be current.");
      tests.Check(!tracker.IsCurrent(default), "A default bell-window token must never be current.");
    }
  }
}
