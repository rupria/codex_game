using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Poker
{
  public static class PokerComparer
  {
    public static PokerComparisonResult Compare(
      IReadOnlyList<Card> playerPrivateCards,
      IReadOnlyList<Card> aiPrivateCards,
      IReadOnlyList<Card> publicCards,
      PokerRuleSet ruleSet)
    {
      ValidateInput(playerPrivateCards, aiPrivateCards, publicCards, ruleSet);
      var playerHand = Join(playerPrivateCards, publicCards);
      var aiHand = Join(aiPrivateCards, publicCards);
      var playerValue = PokerEvaluator.Evaluate(playerHand, ruleSet);
      var aiValue = PokerEvaluator.Evaluate(aiHand, ruleSet);
      var comparison = playerValue.CompareTo(aiValue);

      if (comparison == 0)
      {
        throw new InvalidOperationException(
          "Poker comparison reached a forbidden full tie after suit priority.");
      }

      return new PokerComparisonResult(
        comparison > 0 ? PokerWinner.Player : PokerWinner.Ai,
        playerValue,
        aiValue);
    }

    private static void ValidateInput(
      IReadOnlyList<Card> playerPrivateCards,
      IReadOnlyList<Card> aiPrivateCards,
      IReadOnlyList<Card> publicCards,
      PokerRuleSet ruleSet)
    {
      if (playerPrivateCards == null) throw new ArgumentNullException(nameof(playerPrivateCards));
      if (aiPrivateCards == null) throw new ArgumentNullException(nameof(aiPrivateCards));
      if (publicCards == null) throw new ArgumentNullException(nameof(publicCards));
      if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));

      if (playerPrivateCards.Count != GameRules.RequiredPrivateCards
        || aiPrivateCards.Count != GameRules.RequiredPrivateCards
        || publicCards.Count != 2)
      {
        throw new ArgumentException("Poker requires three private cards per side and two public cards.");
      }

      var ids = new HashSet<CardId>();
      AddUnique(publicCards, ids);
      AddUnique(playerPrivateCards, ids);
      AddUnique(aiPrivateCards, ids);
    }

    private static void AddUnique(IReadOnlyList<Card> cards, HashSet<CardId> ids)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsValid || !ids.Add(cards[index].Id))
        {
          throw new ArgumentException(
            "Poker inputs must contain valid cards and private/public identities cannot overlap.");
        }
      }
    }

    private static IReadOnlyList<Card> Join(
      IReadOnlyList<Card> privateCards,
      IReadOnlyList<Card> publicCards)
    {
      var cards = new Card[PokerEvaluator.HandSize];
      for (var index = 0; index < privateCards.Count; index++) cards[index] = privateCards[index];
      for (var index = 0; index < publicCards.Count; index++) cards[privateCards.Count + index] = publicCards[index];
      return Array.AsReadOnly(cards);
    }
  }
}
