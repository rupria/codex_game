using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Application.Items
{
  public static class MercenaryExchangeResolver
  {
    public static bool CanResolve(
      IReadOnlyList<Card> playerCards,
      IReadOnlyList<Card> aiCards,
      IReadOnlyList<Card> publicCards,
      IReadOnlyList<Card> remainingCandidates,
      CardId playerTarget)
    {
      if (playerCards == null) throw new ArgumentNullException(nameof(playerCards));
      if (aiCards == null) throw new ArgumentNullException(nameof(aiCards));
      if (publicCards == null) throw new ArgumentNullException(nameof(publicCards));
      if (remainingCandidates == null) throw new ArgumentNullException(nameof(remainingCandidates));
      ValidatePools(playerCards, aiCards, publicCards, remainingCandidates);
      var playerTargetIndex = Find(playerCards, playerTarget);
      if (playerTargetIndex < 0 || playerCards[playerTargetIndex].IsJoker) return false;
      var aiTargetIndex = SelectAiTarget(aiCards, publicCards);
      if (aiTargetIndex < 0) return false;
      var playerDominantSuit = SelectDominantSuit(playerCards, playerTargetIndex, publicCards);
      var aiDominantSuit = SelectDominantSuit(aiCards, aiTargetIndex, publicCards);
      return HasReplacementPair(
        remainingCandidates,
        playerDominantSuit,
        aiDominantSuit);
    }

    public static bool TryResolve(
      IReadOnlyList<Card> playerCards,
      IReadOnlyList<Card> aiCards,
      IReadOnlyList<Card> publicCards,
      IReadOnlyList<Card> remainingCandidates,
      CardId playerTarget,
      IRandomSource random,
      out MercenaryExchangeResult result)
    {
      if (playerCards == null) throw new ArgumentNullException(nameof(playerCards));
      if (aiCards == null) throw new ArgumentNullException(nameof(aiCards));
      if (publicCards == null) throw new ArgumentNullException(nameof(publicCards));
      if (remainingCandidates == null) throw new ArgumentNullException(nameof(remainingCandidates));
      if (random == null) throw new ArgumentNullException(nameof(random));
      ValidatePools(playerCards, aiCards, publicCards, remainingCandidates);

      var playerTargetIndex = Find(playerCards, playerTarget);
      if (playerTargetIndex < 0 || playerCards[playerTargetIndex].IsJoker)
      {
        result = MercenaryExchangeResult.Failed(MercenaryExchangeFailure.InvalidPlayerTarget);
        return false;
      }
      var aiTargetIndex = SelectAiTarget(aiCards, publicCards);
      if (aiTargetIndex < 0)
      {
        result = MercenaryExchangeResult.Failed(MercenaryExchangeFailure.NoAiTarget);
        return false;
      }

      var playerDominantSuit = SelectDominantSuit(playerCards, playerTargetIndex, publicCards);
      var aiDominantSuit = SelectDominantSuit(aiCards, aiTargetIndex, publicCards);
      var pairs = new List<CandidatePair>();
      for (var playerIndex = 0; playerIndex < remainingCandidates.Count; playerIndex++)
      {
        var playerReplacement = remainingCandidates[playerIndex];
        if (playerReplacement.IsJoker || playerReplacement.Suit != playerDominantSuit) continue;
        for (var aiIndex = 0; aiIndex < remainingCandidates.Count; aiIndex++)
        {
          var aiReplacement = remainingCandidates[aiIndex];
          if (aiIndex == playerIndex
            || aiReplacement.IsJoker
            || aiReplacement.Suit != aiDominantSuit) continue;
          pairs.Add(new CandidatePair(playerIndex, aiIndex));
        }
      }
      if (pairs.Count == 0)
      {
        result = MercenaryExchangeResult.Failed(MercenaryExchangeFailure.NoReplacementPair);
        return false;
      }

      var selected = pairs[random.NextInt(pairs.Count)];
      var playerReplacementCard = remainingCandidates[selected.PlayerCandidateIndex];
      var aiReplacementCard = remainingCandidates[selected.AiCandidateIndex];
      var replacedPlayerCard = playerCards[playerTargetIndex];
      var replacedAiCard = aiCards[aiTargetIndex];

      var updatedPlayer = Copy(playerCards);
      var updatedAi = Copy(aiCards);
      updatedPlayer[playerTargetIndex] = playerReplacementCard;
      updatedAi[aiTargetIndex] = aiReplacementCard;
      var updatedPool = new List<Card>(remainingCandidates.Count);
      for (var index = 0; index < remainingCandidates.Count; index++)
      {
        if (index != selected.PlayerCandidateIndex && index != selected.AiCandidateIndex)
        {
          updatedPool.Add(remainingCandidates[index]);
        }
      }
      updatedPool.Add(replacedPlayerCard);
      updatedPool.Add(replacedAiCard);
      ValidatePools(updatedPlayer, updatedAi, publicCards, updatedPool);
      result = MercenaryExchangeResult.Succeeded(
        updatedPlayer,
        updatedAi,
        updatedPool,
        playerTargetIndex,
        aiTargetIndex,
        playerDominantSuit,
        aiDominantSuit);
      return true;
    }

    private static bool HasReplacementPair(
      IReadOnlyList<Card> remainingCandidates,
      CardSuit playerDominantSuit,
      CardSuit aiDominantSuit)
    {
      for (var playerIndex = 0; playerIndex < remainingCandidates.Count; playerIndex++)
      {
        var playerReplacement = remainingCandidates[playerIndex];
        if (playerReplacement.IsJoker || playerReplacement.Suit != playerDominantSuit) continue;
        for (var aiIndex = 0; aiIndex < remainingCandidates.Count; aiIndex++)
        {
          var aiReplacement = remainingCandidates[aiIndex];
          if (aiIndex != playerIndex
            && !aiReplacement.IsJoker
            && aiReplacement.Suit == aiDominantSuit) return true;
        }
      }
      return false;
    }

    private static int SelectAiTarget(IReadOnlyList<Card> aiCards, IReadOnlyList<Card> publicCards)
    {
      var selectedIndex = -1;
      var selectedCount = -1;
      for (var index = 0; index < aiCards.Count; index++)
      {
        if (aiCards[index].IsJoker) continue;
        var suit = SelectDominantSuit(aiCards, index, publicCards);
        var count = CountSuit(aiCards, index, publicCards, suit);
        if (selectedIndex < 0
          || count > selectedCount
          || count == selectedCount && IsPreferredTarget(aiCards[index], aiCards[selectedIndex]))
        {
          selectedIndex = index;
          selectedCount = count;
        }
      }
      return selectedIndex;
    }

    private static bool IsPreferredTarget(Card candidate, Card current)
    {
      var rankComparison = ((int)candidate.Rank).CompareTo((int)current.Rank);
      return rankComparison < 0
        || rankComparison == 0 && (int)candidate.Suit > (int)current.Suit;
    }

    private static CardSuit SelectDominantSuit(
      IReadOnlyList<Card> privateCards,
      int excludedPrivateIndex,
      IReadOnlyList<Card> publicCards)
    {
      var counts = new int[4];
      var highestRanks = new int[4];
      AddSuitCounts(privateCards, excludedPrivateIndex, counts, highestRanks);
      AddSuitCounts(publicCards, -1, counts, highestRanks);
      var selected = CardSuit.Clubs;
      for (var suitValue = (int)CardSuit.Hearts; suitValue <= (int)CardSuit.Spades; suitValue++)
      {
        var selectedValue = (int)selected;
        if (counts[suitValue] > counts[selectedValue]
          || counts[suitValue] == counts[selectedValue]
            && highestRanks[suitValue] > highestRanks[selectedValue]
          || counts[suitValue] == counts[selectedValue]
            && highestRanks[suitValue] == highestRanks[selectedValue]
            && suitValue > selectedValue)
        {
          selected = (CardSuit)suitValue;
        }
      }
      return selected;
    }

    private static int CountSuit(
      IReadOnlyList<Card> privateCards,
      int excludedPrivateIndex,
      IReadOnlyList<Card> publicCards,
      CardSuit suit)
    {
      var count = 0;
      for (var index = 0; index < privateCards.Count; index++)
      {
        if (index != excludedPrivateIndex
          && !privateCards[index].IsJoker
          && privateCards[index].Suit == suit) count++;
      }
      for (var index = 0; index < publicCards.Count; index++)
      {
        if (!publicCards[index].IsJoker && publicCards[index].Suit == suit) count++;
      }
      return count;
    }

    private static void AddSuitCounts(
      IReadOnlyList<Card> cards,
      int excludedIndex,
      int[] counts,
      int[] highestRanks)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (index == excludedIndex || cards[index].IsJoker) continue;
        var suit = (int)cards[index].Suit;
        counts[suit]++;
        highestRanks[suit] = Math.Max(highestRanks[suit], (int)cards[index].Rank);
      }
    }

    private static int Find(IReadOnlyList<Card> cards, CardId cardId)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == cardId) return index;
      }
      return -1;
    }

    private static Card[] Copy(IReadOnlyList<Card> source)
    {
      var copy = new Card[source.Count];
      for (var index = 0; index < copy.Length; index++) copy[index] = source[index];
      return copy;
    }

    private static void ValidatePools(params IReadOnlyList<Card>[] pools)
    {
      var ids = new HashSet<CardId>();
      for (var poolIndex = 0; poolIndex < pools.Length; poolIndex++)
      {
        var pool = pools[poolIndex] ?? throw new ArgumentNullException(nameof(pools));
        for (var index = 0; index < pool.Count; index++)
        {
          if (!pool[index].IsValid || !ids.Add(pool[index].Id))
          {
            throw new ArgumentException("Mercenary inputs must contain unique valid cards.");
          }
        }
      }
    }

    private readonly struct CandidatePair
    {
      public CandidatePair(int playerCandidateIndex, int aiCandidateIndex)
      {
        PlayerCandidateIndex = playerCandidateIndex;
        AiCandidateIndex = aiCandidateIndex;
      }
      public int PlayerCandidateIndex { get; }
      public int AiCandidateIndex { get; }
    }
  }

  public enum MercenaryExchangeFailure
  {
    None = 0,
    InvalidPlayerTarget = 1,
    NoAiTarget = 2,
    NoReplacementPair = 3
  }

  public sealed class MercenaryExchangeResult
  {
    private MercenaryExchangeResult(
      MercenaryExchangeFailure failure,
      IReadOnlyList<Card> playerCards,
      IReadOnlyList<Card> aiCards,
      IReadOnlyList<Card> remainingCandidates,
      int playerTargetIndex,
      int aiTargetIndex,
      CardSuit playerDominantSuit,
      CardSuit aiDominantSuit)
    {
      Failure = failure;
      PlayerCards = playerCards;
      AiCards = aiCards;
      RemainingCandidates = remainingCandidates;
      PlayerTargetIndex = playerTargetIndex;
      AiTargetIndex = aiTargetIndex;
      PlayerDominantSuit = playerDominantSuit;
      AiDominantSuit = aiDominantSuit;
    }

    public MercenaryExchangeFailure Failure { get; }
    public bool IsSuccess => Failure == MercenaryExchangeFailure.None;
    public IReadOnlyList<Card> PlayerCards { get; }
    public IReadOnlyList<Card> AiCards { get; }
    public IReadOnlyList<Card> RemainingCandidates { get; }
    public int PlayerTargetIndex { get; }
    public int AiTargetIndex { get; }
    public CardSuit PlayerDominantSuit { get; }
    public CardSuit AiDominantSuit { get; }

    public static MercenaryExchangeResult Failed(MercenaryExchangeFailure failure)
    {
      return new MercenaryExchangeResult(
        failure,
        Array.AsReadOnly(Array.Empty<Card>()),
        Array.AsReadOnly(Array.Empty<Card>()),
        Array.AsReadOnly(Array.Empty<Card>()),
        -1,
        -1,
        default,
        default);
    }

    public static MercenaryExchangeResult Succeeded(
      IReadOnlyList<Card> playerCards,
      IReadOnlyList<Card> aiCards,
      IReadOnlyList<Card> remainingCandidates,
      int playerTargetIndex,
      int aiTargetIndex,
      CardSuit playerDominantSuit,
      CardSuit aiDominantSuit)
    {
      return new MercenaryExchangeResult(
        MercenaryExchangeFailure.None,
        Array.AsReadOnly(Copy(playerCards)),
        Array.AsReadOnly(Copy(aiCards)),
        Array.AsReadOnly(Copy(remainingCandidates)),
        playerTargetIndex,
        aiTargetIndex,
        playerDominantSuit,
        aiDominantSuit);
    }

    private static Card[] Copy(IReadOnlyList<Card> source)
    {
      var copy = new Card[source.Count];
      for (var index = 0; index < copy.Length; index++) copy[index] = source[index];
      return copy;
    }
  }
}
