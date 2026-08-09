using CodexGame.Core.Cards;
using CodexGame.Core.Halli;

namespace CodexGame.SmokeTests.Halli
{
  internal static class HalliRuleTests
  {
    public static void Run(TestHarness tests)
    {
      var spades1 = Create(CardSuit.Spades, CardRank.Two, 1);
      var spades2 = Create(CardSuit.Spades, CardRank.Three, 2);
      var hearts1 = Create(CardSuit.Hearts, CardRank.Four, 1);
      var hearts2 = Create(CardSuit.Hearts, CardRank.Five, 2);
      var clubs1 = Create(CardSuit.Clubs, CardRank.Six, 1);
      var clubs2 = Create(CardSuit.Clubs, CardRank.Seven, 2);
      var diamonds3 = Create(CardSuit.Diamonds, CardRank.Eight, 3);
      var diamonds1 = Create(CardSuit.Diamonds, CardRank.Ten, 1);
      var clubs3 = Create(CardSuit.Clubs, CardRank.Nine, 3);

      tests.Check(
        SkullAcquisitionResolver.Resolve(spades1, spades2) == AcquisitionKind.Both,
        "Same-suit skull 1 + 2 must acquire both cards.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(hearts2, hearts1) == AcquisitionKind.Both,
        "Same-suit skull 2 + 1 must acquire both cards.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(spades1, hearts2) == AcquisitionKind.None,
        "Different-suit skull 1 + 2 must not acquire cards.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(clubs2, hearts1) == AcquisitionKind.None,
        "Different-suit skull 2 + 1 must not acquire cards.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(spades1, hearts1) == AcquisitionKind.None,
        "Skull 1 + 1 must not acquire cards regardless of suit.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(diamonds3, clubs1) == AcquisitionKind.None,
        "A previous skull-3 card must become inert after a newer card is exposed.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(diamonds3, diamonds1) == AcquisitionKind.None,
        "A previous skull-3 card must remain inert even when the newer card has the same suit.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(clubs2, diamonds3) == AcquisitionKind.RightOnly,
        "Skull 2 + 3 must acquire only the skull-3 card regardless of suit.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(null, diamonds3) == AcquisitionKind.RightOnly,
        "A lone skull-3 card must be acquired.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(null, spades2) == AcquisitionKind.None,
        "A lone non-skull-3 card must not be acquired.");
      tests.Check(
        SkullAcquisitionResolver.Resolve(diamonds3, clubs3) == AcquisitionKind.RightOnly,
        "Skull 3 + 3 must acquire only the newest exposed skull-3 card.");
    }

    private static Card Create(CardSuit suit, CardRank rank, int skullCount)
    {
      return new Card(suit, rank, skullCount);
    }
  }
}
