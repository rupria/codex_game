using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.SmokeTests.Cards
{
  internal static class CardDeckTests
  {
    public static void Run(TestHarness tests)
    {
      var cards = TestCardSet.Create();
      var identities = new HashSet<CardId>();

      tests.Check(cards.Count == CardId.CardCount, "The standard card set must contain 52 cards.");

      for (var index = 0; index < cards.Count; index++)
      {
        var card = cards[index];
        tests.Check(card.IsValid, "Every generated standard card must be valid.");
        tests.Check(identities.Add(card.Id), "Every generated standard card identity must be unique.");
        tests.Check(card.Id == CardId.FromValue(index), "The standard card identity order must be stable.");
      }

      var firstDeck = CreateDeck(cards, 20260807, RandomChannel.CardOrder);
      var secondDeck = CreateDeck(cards, 20260807, RandomChannel.CardOrder);
      var differentSeedDeck = CreateDeck(cards, 20260808, RandomChannel.CardOrder);
      var firstOrder = firstDeck.SnapshotRemaining();
      var secondOrder = secondDeck.SnapshotRemaining();
      var differentOrder = differentSeedDeck.SnapshotRemaining();
      var sameOrder = true;
      var differentOrderFound = false;

      for (var index = 0; index < CardId.CardCount; index++)
      {
        sameOrder &= firstOrder[index].Id == secondOrder[index].Id;
        differentOrderFound |= firstOrder[index].Id != differentOrder[index].Id;
      }

      tests.Check(sameOrder, "The same seed and random channel must reproduce the same deck order.");
      tests.Check(differentOrderFound, "A different combat-round seed must change the deck order.");

      var cardOrderRandom = DeterministicRandomFactory.Create(20260807, RandomChannel.CardOrder);
      var aiChoiceRandom = DeterministicRandomFactory.Create(20260807, RandomChannel.AiChoice);
      var separatedChannels = false;

      for (var index = 0; index < 8; index++)
      {
        separatedChannels |= cardOrderRandom.NextInt(1_000_000) != aiChoiceRandom.NextInt(1_000_000);
      }

      tests.Check(separatedChannels, "Random channels must not share the same sequence for one combat-round seed.");

      var firstPublic = firstDeck.Draw();
      tests.Check(firstPublic.IsValid, "The first public draw must return a valid card.");
      tests.Check(firstDeck.RemainingCount == 51, "Drawing the first public card must leave 51 cards.");

      tests.CheckThrows<ArgumentException>(
        () => Deck.CreateShuffled(CreateDuplicateCardSet(cards), DeterministicRandomFactory.Create(1, RandomChannel.CardOrder)),
        "A deck with duplicate card identities must be rejected.");
      tests.CheckThrows<ArgumentOutOfRangeException>(
        () => CardSetFactory.CreateStandard52(new InvalidSkullPolicy()),
        "An unresolved or invalid skull count must not enter the card set.");
    }

    private static Deck CreateDeck(IReadOnlyList<Card> cards, long seed, RandomChannel channel)
    {
      return Deck.CreateShuffled(cards, DeterministicRandomFactory.Create(seed, channel));
    }

    private static IReadOnlyList<Card> CreateDuplicateCardSet(IReadOnlyList<Card> cards)
    {
      var duplicate = new Card[CardId.CardCount];

      for (var index = 0; index < duplicate.Length; index++)
      {
        duplicate[index] = cards[index];
      }

      duplicate[duplicate.Length - 1] = duplicate[0];
      return duplicate;
    }

    private sealed class InvalidSkullPolicy : ICardSkullPolicy
    {
      public int ResolveSkullCount(CardSuit suit, CardRank rank)
      {
        return 0;
      }
    }
  }
}
