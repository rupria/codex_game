using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Distribution
{
  internal static class BalancedPokerDealResolver
  {
    private const int MaximumAttempts = 512;

    public static Deal Resolve(
      IReadOnlyList<Card> fixedPlayerCards,
      IReadOnlyList<Card> fixedAiCards,
      IReadOnlyList<Card> candidates,
      Card firstPublicCard,
      IRandomSource dealRandom,
      IRandomSource playerBalanceRandom,
      IRandomSource aiBalanceRandom)
    {
      if (!firstPublicCard.IsValid || firstPublicCard.IsJoker)
      {
        throw new ArgumentException("The first public card must be a standard card.", nameof(firstPublicCard));
      }

      var playerTarget = PokerHandDistributionProfile.Roll(playerBalanceRandom);
      var aiTarget = PokerHandDistributionProfile.Roll(aiBalanceRandom);
      Deal? best = null;
      var bestMatchCount = -1;

      for (var attempt = 0; attempt < MaximumAttempts; attempt++)
      {
        var deal = Draw(fixedPlayerCards, fixedAiCards, candidates, dealRandom);
        var playerMatches = Matches(
          deal.PlayerPrivateCards,
          firstPublicCard,
          deal.SecondPublicCard,
          playerTarget);
        var aiMatches = Matches(
          deal.AiPrivateCards,
          firstPublicCard,
          deal.SecondPublicCard,
          aiTarget);
        var matchCount = (playerMatches ? 1 : 0) + (aiMatches ? 1 : 0);
        if (matchCount > bestMatchCount)
        {
          best = deal;
          bestMatchCount = matchCount;
        }

        if (matchCount == 2) return deal;
      }

      return best ?? throw new InvalidOperationException("Balanced poker deal could not be generated.");
    }

    private static Deal Draw(
      IReadOnlyList<Card> fixedPlayerCards,
      IReadOnlyList<Card> fixedAiCards,
      IReadOnlyList<Card> candidates,
      IRandomSource random)
    {
      var pool = new List<Card>(candidates);
      var player = new List<Card>(fixedPlayerCards);
      var ai = new List<Card>(fixedAiCards);
      Fill(player, pool, random);
      Fill(ai, pool, random);
      var secondPublic = TakeRandom(pool, random);
      return new Deal(
        Array.AsReadOnly(player.ToArray()),
        Array.AsReadOnly(ai.ToArray()),
        secondPublic,
        Array.AsReadOnly(pool.ToArray()));
    }

    private static bool Matches(
      IReadOnlyList<Card> privateCards,
      Card firstPublic,
      Card secondPublic,
      PokerHandCategory target)
    {
      for (var index = 0; index < privateCards.Count; index++)
      {
        // Joker is an independent post-balance modifier. Its optimized result must
        // not feed back into the base 40/60 deal channel.
        if (privateCards[index].IsJoker) return true;
      }

      var hand = new Card[PokerEvaluator.HandSize];
      for (var index = 0; index < privateCards.Count; index++) hand[index] = privateCards[index];
      hand[3] = firstPublic;
      hand[4] = secondPublic;
      return PokerEvaluator.Evaluate(Array.AsReadOnly(hand), PokerRuleSet.Development).Category == target;
    }

    private static void Fill(List<Card> cards, List<Card> pool, IRandomSource random)
    {
      while (cards.Count < GameRules.RequiredPrivateCards)
      {
        cards.Add(TakeRandom(pool, random));
      }
    }

    private static Card TakeRandom(List<Card> cards, IRandomSource random)
    {
      if (cards.Count == 0) throw new InvalidOperationException("No poker deal candidate remains.");
      var index = random.NextInt(cards.Count);
      var card = cards[index];
      cards.RemoveAt(index);
      return card;
    }

    internal sealed class Deal
    {
      public Deal(
        IReadOnlyList<Card> playerPrivateCards,
        IReadOnlyList<Card> aiPrivateCards,
        Card secondPublicCard,
        IReadOnlyList<Card> remainingCandidates)
      {
        PlayerPrivateCards = playerPrivateCards;
        AiPrivateCards = aiPrivateCards;
        SecondPublicCard = secondPublicCard;
        RemainingCandidates = remainingCandidates;
      }

      public IReadOnlyList<Card> PlayerPrivateCards { get; }
      public IReadOnlyList<Card> AiPrivateCards { get; }
      public Card SecondPublicCard { get; }
      public IReadOnlyList<Card> RemainingCandidates { get; }
    }
  }
}
