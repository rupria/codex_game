using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class AcquiredCardFanLayoutTests
  {
    public static void Run(TestHarness tests)
    {
      const float areaX = 560f;
      const float areaWidth = 354f;
      const float cardWidth = 56f;

      var one = AcquiredCardFanLayout.Create(1, areaX, areaWidth, cardWidth, 30f);
      tests.Check(
        one.Count == 1 && NearlyEqual(one.X(0) + cardWidth, areaX + areaWidth),
        "A single acquired card must be right-aligned inside its Halli tray.");

      var five = AcquiredCardFanLayout.Create(5, areaX, areaWidth, cardWidth, 30f);
      tests.Check(
        NearlyEqual(five.Step, 30f) && NearlyEqual(five.X(4) + cardWidth, areaX + areaWidth),
        "Five acquired cards must keep the preferred rank-and-suit reveal spacing.");

      var twentyFour = AcquiredCardFanLayout.Create(24, areaX, areaWidth, cardWidth, 30f);
      var ordered = true;
      for (var index = 1; index < twentyFour.Count; index++)
      {
        ordered &= twentyFour.X(index) > twentyFour.X(index - 1);
      }
      tests.Check(
        twentyFour.Count == 24
          && ordered
          && twentyFour.X(0) >= areaX
          && NearlyEqual(twentyFour.X(23) + cardWidth, areaX + areaWidth),
        "All player acquired cards must remain present, ordered, and inside their fan area.");

      var aiBacks = AcquiredCardFanLayout.Create(24, 714f, 200f, cardWidth, 30f);
      tests.Check(
        aiBacks.Count == 24
          && aiBacks.X(0) >= 714f
          && NearlyEqual(aiBacks.X(23) + cardWidth, 914f),
        "AI acquired-card backs must fit inside the bottom-right tray without exposing faces.");

      var oneSlotTray = AcquiredCardFanLayout.CreateLeftAligned(1, 152f, 212f, cardWidth, 78f);
      var twoSlotTray = AcquiredCardFanLayout.CreateLeftAligned(2, 152f, 212f, cardWidth, 78f);
      var threeSlotTray = AcquiredCardFanLayout.CreateLeftAligned(3, 152f, 212f, cardWidth, 78f);
      tests.Check(
        NearlyEqual(oneSlotTray.X(0), 152f)
          && NearlyEqual(twoSlotTray.X(0), 152f)
          && NearlyEqual(twoSlotTray.X(1), 230f)
          && NearlyEqual(threeSlotTray.X(0), 152f)
          && NearlyEqual(threeSlotTray.X(1), 230f)
          && NearlyEqual(threeSlotTray.X(2), 308f),
        "One to three cards must keep the approved player tray slot positions.");

      var sixCardTray = AcquiredCardFanLayout.CreateLeftAligned(6, 152f, 212f, cardWidth, 78f);
      tests.Check(
        sixCardTray.Count == 6
          && NearlyEqual(sixCardTray.X(0), 152f)
          && NearlyEqual(sixCardTray.X(5) + cardWidth, 364f),
        "Every acquired card must remain visible inside the three-slot player tray area.");
    }

    private static bool NearlyEqual(float left, float right)
    {
      return Math.Abs(left - right) < 0.001f;
    }
  }
}
