using CodexGame.Application.Playable;
using CodexGame.Core.Cards;

namespace CodexGame.SmokeTests.Cards
{
  internal static class PrototypeSkullPolicyTests
  {
    private static readonly int[] ExpectedByCardId =
    {
      // Clubs: 2 through Ace.
      1, 2, 1, 1, 2, 1, 1, 2, 3, 1, 2, 1, 2,
      // Hearts: 2 through Ace.
      1, 3, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 2,
      // Diamonds: 2 through Ace.
      2, 1, 1, 2, 3, 1, 2, 1, 1, 2, 1, 2, 1,
      // Spades: 2 through Ace.
      1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 3, 2, 1
    };

    public static void Run(TestHarness tests)
    {
      var cards = CardSetFactory.CreateStandard52(new PrototypeSkullPolicy());
      var totals = new int[4];
      var skullThreePerSuit = new int[4];

      tests.Check(
        ExpectedByCardId.Length == CardId.CardCount,
        "The prototype skull balance table must cover all 52 cards.");

      for (var index = 0; index < cards.Count; index++)
      {
        var card = cards[index];
        tests.Check(
          card.SkullCount == ExpectedByCardId[index],
          $"Card {index} must use the explicit prototype skull balance value.");
        totals[card.SkullCount]++;
        if (card.SkullCount == 3) skullThreePerSuit[(int)card.Suit]++;
      }

      tests.Check(totals[1] == 30, "The prototype deck must contain 30 skull-1 cards.");
      tests.Check(totals[2] == 18, "The prototype deck must contain 18 skull-2 cards.");
      tests.Check(totals[3] == 4, "The prototype deck must contain 4 skull-3 cards.");

      for (var suit = 0; suit < skullThreePerSuit.Length; suit++)
      {
        tests.Check(
          skullThreePerSuit[suit] == 1,
          $"Suit {suit} must contain exactly one skull-3 card.");
      }
    }
  }
}
