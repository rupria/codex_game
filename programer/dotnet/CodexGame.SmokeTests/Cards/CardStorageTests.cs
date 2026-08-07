using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Cards
{
  internal static class CardStorageTests
  {
    public static void Run(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var deck = Deck.CreateShuffled(
        cards,
        DeterministicRandomFactory.Create(31082026, RandomChannel.CardOrder));
      var ledger = new CardLedger(cards);
      var field = new HalliField();

      var firstPublic = DrawTo(deck, ledger, CardZone.FirstPublic);
      tests.Check(ledger.GetZone(firstPublic.Id) == CardZone.FirstPublic, "The first public card must leave the deck zone.");
      tests.Check(deck.RemainingCount == 51, "The first public card must reduce the deck repository to 51.");

      var leftFirst = ExposeFromDeck(deck, ledger, field, PileSide.Left);
      var leftSecond = ExposeFromDeck(deck, ledger, field, PileSide.Left);
      var leftThird = ExposeFromDeck(deck, ledger, field, PileSide.Left);
      var leftCards = field.GetExposedCards(PileSide.Left);

      tests.Check(leftCards.Count == 2, "A pile must expose at most two cards.");
      tests.Check(leftCards[0].Id == leftSecond.Id, "The second card must become the oldest exposed card after displacement.");
      tests.Check(leftCards[1].Id == leftThird.Id, "The newest card must remain on the pile.");
      tests.Check(ledger.GetZone(leftFirst.Id) == CardZone.UnacquiredPool, "The displaced oldest card must enter the unacquired pool.");

      var rightFirst = ExposeFromDeck(deck, ledger, field, PileSide.Right);
      var rightSecond = ExposeFromDeck(deck, ledger, field, PileSide.Right);
      var rightBeforeLeftClear = field.GetExposedCards(PileSide.Right);
      var clearedLeft = field.Clear(PileSide.Left);

      foreach (var card in clearedLeft)
      {
        ledger.Move(card.Id, CardZone.LeftPile, CardZone.PlayerAcquired);
      }

      var rightAfterLeftClear = field.GetExposedCards(PileSide.Right);
      tests.Check(rightAfterLeftClear.Count == 2, "Clearing the left pile must not clear the right pile.");
      tests.Check(rightBeforeLeftClear[0].Id == rightAfterLeftClear[0].Id, "The opposite pile order must remain unchanged.");
      tests.Check(rightAfterLeftClear[0].Id == rightFirst.Id && rightAfterLeftClear[1].Id == rightSecond.Id, "The right pile must keep both exposed cards.");
      tests.Check(ledger.Count(CardZone.PlayerAcquired) == 2, "Only the selected pile cards must move to the acquired pool.");
      tests.Check(ledger.Count(CardZone.RightPile) == 2, "The opposite pile ledger locations must remain unchanged.");
      tests.Check(ledger.Count(CardZone.Deck) == deck.RemainingCount, "The deck order repository and ledger deck zone must stay synchronized.");
      tests.Check(CountAllZones(ledger) == CardId.CardCount, "All card zones together must preserve exactly 52 cards.");
      tests.Check(ledger.TotalCount == CardId.CardCount, "The ledger must preserve 52 unique identities.");

      tests.CheckThrows<InvalidOperationException>(
        () => ledger.Move(firstPublic.Id, CardZone.Deck, CardZone.SecondPublic),
        "A move from an incorrect source zone must be rejected.");
    }

    private static Card DrawTo(Deck deck, CardLedger ledger, CardZone destination)
    {
      var card = deck.Draw();
      ledger.Move(card.Id, CardZone.Deck, destination);
      return card;
    }

    private static Card ExposeFromDeck(
      Deck deck,
      CardLedger ledger,
      HalliField field,
      PileSide side)
    {
      var destination = side == PileSide.Left ? CardZone.LeftPile : CardZone.RightPile;
      var card = DrawTo(deck, ledger, destination);
      var displaced = field.Expose(side, card);

      if (displaced.HasValue)
      {
        ledger.Move(displaced.Value.Id, destination, CardZone.UnacquiredPool);
      }

      return card;
    }

    private static int CountAllZones(CardLedger ledger)
    {
      var count = 0;

      foreach (CardZone zone in Enum.GetValues(typeof(CardZone)))
      {
        count += ledger.Count(zone);
      }

      return count;
    }
  }
}
