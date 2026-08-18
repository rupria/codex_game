#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Distribution
{
  public static class PrivateCardDistributionResolver
  {
    public static PrivateCardDistributionResult Resolve(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedWinnerCards,
      PrivateCardSelectionMode selectionMode,
      IRandomSource random,
      bool pairAssistEnabled = false)
    {
      var playerSelection = winner == HalliStageWinner.Player
        ? selectedWinnerCards
        : Array.AsReadOnly(Array.Empty<CardId>());
      var aiSelection = winner == HalliStageWinner.Ai
        ? selectedWinnerCards
        : Array.AsReadOnly(Array.Empty<CardId>());
      return ResolveBoth(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        playerSelection,
        aiSelection,
        selectionMode,
        random,
        pairAssistEnabled);
    }

    public static PrivateCardDistributionResult ResolveBoth(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedPlayerCards,
      IReadOnlyList<CardId> selectedAiCards,
      PrivateCardSelectionMode playerSelectionMode,
      IRandomSource random,
      bool pairAssistEnabled = false)
    {
      return ResolveBothInternal(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        selectedPlayerCards,
        selectedAiCards,
        playerSelectionMode,
        random,
        null,
        null,
        null,
        null,
        pairAssistEnabled);
    }

    public static PrivateCardDistributionResult ResolveBothBalanced(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedPlayerCards,
      IReadOnlyList<CardId> selectedAiCards,
      PrivateCardSelectionMode playerSelectionMode,
      IRandomSource random,
      Card firstPublicCard,
      IRandomSource playerBalanceRandom,
      IRandomSource aiBalanceRandom,
      bool pairAssistEnabled = false)
    {
      return ResolveBothInternal(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        selectedPlayerCards,
        selectedAiCards,
        playerSelectionMode,
        random,
        firstPublicCard,
        null,
        playerBalanceRandom,
        aiBalanceRandom,
        pairAssistEnabled);
    }

    public static PrivateCardDistributionResult ResolveBothBalancedWithPublicCards(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedPlayerCards,
      IReadOnlyList<CardId> selectedAiCards,
      PrivateCardSelectionMode playerSelectionMode,
      IRandomSource random,
      Card firstPublicCard,
      Card secondPublicCard,
      IRandomSource playerBalanceRandom,
      IRandomSource aiBalanceRandom,
      bool pairAssistEnabled = false)
    {
      return ResolveBothInternal(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        selectedPlayerCards,
        selectedAiCards,
        playerSelectionMode,
        random,
        firstPublicCard,
        secondPublicCard,
        playerBalanceRandom,
        aiBalanceRandom,
        pairAssistEnabled);
    }

    private static PrivateCardDistributionResult ResolveBothInternal(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedPlayerCards,
      IReadOnlyList<CardId> selectedAiCards,
      PrivateCardSelectionMode playerSelectionMode,
      IRandomSource random,
      Card? firstPublicCard,
      Card? fixedSecondPublicCard,
      IRandomSource? playerBalanceRandom,
      IRandomSource? aiBalanceRandom,
      bool pairAssistEnabled)
    {
      ValidateInputs(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        selectedPlayerCards,
        selectedAiCards,
        playerSelectionMode,
        random);

      var randomCandidates = new List<Card>(otherCandidates);
      var playerPrivate = RetainOrSelect(
        playerAcquiredCards,
        selectedPlayerCards,
        playerSelectionMode,
        randomCandidates,
        random,
        PrivateCardDistributionRules.GetDirectSelectionCount(combatRoundNumber));
      var aiPrivate = RetainOrSelect(
        aiAcquiredCards,
        selectedAiCards,
        PrivateCardSelectionMode.Confirmed,
        randomCandidates,
        random,
        PrivateCardDistributionRules.GetDirectSelectionCount(combatRoundNumber));

      EnsureEnoughCandidates(
        randomCandidates.Count,
        (GameRules.RequiredPrivateCards - playerPrivate.Count)
          + (GameRules.RequiredPrivateCards - aiPrivate.Count)
          + (fixedSecondPublicCard.HasValue ? 0 : 1));
      if (firstPublicCard.HasValue)
      {
        if (playerBalanceRandom == null) throw new ArgumentNullException(nameof(playerBalanceRandom));
        if (aiBalanceRandom == null) throw new ArgumentNullException(nameof(aiBalanceRandom));
        if (pairAssistEnabled)
        {
          FillRandom(playerPrivate, randomCandidates, random, true);
          FillRandom(aiPrivate, randomCandidates, random, true);
        }
        var balanced = fixedSecondPublicCard.HasValue
          ? BalancedPokerDealResolver.ResolveWithFixedPublicCards(
            playerPrivate,
            aiPrivate,
            randomCandidates,
            firstPublicCard.Value,
            fixedSecondPublicCard.Value,
            random,
            playerBalanceRandom,
            aiBalanceRandom)
          : BalancedPokerDealResolver.Resolve(
            playerPrivate,
            aiPrivate,
            randomCandidates,
            firstPublicCard.Value,
            random,
             playerBalanceRandom,
             aiBalanceRandom);
        return new PrivateCardDistributionResult(
          winner,
          combatRoundNumber,
          balanced.PlayerPrivateCards,
          balanced.AiPrivateCards,
          balanced.SecondPublicCard,
          balanced.RemainingCandidates);
      }

      FillRandom(playerPrivate, randomCandidates, random, pairAssistEnabled);
      FillRandom(aiPrivate, randomCandidates, random, pairAssistEnabled);
      var secondPublic = TakeRandom(randomCandidates, random);

      return new PrivateCardDistributionResult(
        winner,
        combatRoundNumber,
        Array.AsReadOnly(playerPrivate.ToArray()),
        Array.AsReadOnly(aiPrivate.ToArray()),
        secondPublic,
        Array.AsReadOnly(randomCandidates.ToArray()));
    }

    private static List<Card> RetainOrSelect(
      IReadOnlyList<Card> acquired,
      IReadOnlyList<CardId> selectedIds,
      PrivateCardSelectionMode mode,
      List<Card> randomCandidates,
      IRandomSource random,
      int directSelectionCount)
    {
      if (acquired.Count <= directSelectionCount)
      {
        if (selectedIds.Count != 0)
        {
          throw new ArgumentException("Selections are accepted only when acquired cards exceed three.");
        }
        return new List<Card>(acquired);
      }

      var selected = NormalizeSelection(acquired, selectedIds);
      if (mode == PrivateCardSelectionMode.Confirmed
        && selected.Count != directSelectionCount)
      {
        throw new ArgumentException("A confirmed selection must contain the round's direct-selection count.");
      }
      if (mode == PrivateCardSelectionMode.TimedOut
        && selected.Count > directSelectionCount)
      {
        throw new ArgumentException("A timed-out selection exceeds the round's direct-selection count.");
      }

      var selectedSet = ToIdSet(selected);
      var unselectedAcquired = new List<Card>();
      for (var index = 0; index < acquired.Count; index++)
      {
        if (!selectedSet.Contains(acquired[index].Id)) unselectedAcquired.Add(acquired[index]);
      }

      for (var index = 0; index < unselectedAcquired.Count; index++)
      {
        if (!unselectedAcquired[index].IsJoker) randomCandidates.Add(unselectedAcquired[index]);
      }
      return selected;
    }

    private static void FillRandom(
      List<Card> destination,
      List<Card> candidates,
      IRandomSource random,
      bool pairAssistEnabled)
    {
      while (destination.Count < GameRules.RequiredPrivateCards)
      {
        destination.Add(
          pairAssistEnabled
            && destination.Count > 0
            && random.NextInt(100) < GameRules.PairAssistFillPercent
              ? TakeRankMatchOrRandom(destination, candidates, random)
              : TakeRandom(candidates, random));
      }
    }

    private static Card TakeRankMatchOrRandom(
      IReadOnlyList<Card> destination,
      List<Card> candidates,
      IRandomSource random)
    {
      var matchingIndexes = new List<int>();
      for (var candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
      {
        for (var destinationIndex = 0; destinationIndex < destination.Count; destinationIndex++)
        {
          if (candidates[candidateIndex].Rank != destination[destinationIndex].Rank) continue;
          matchingIndexes.Add(candidateIndex);
          break;
        }
      }

      if (matchingIndexes.Count == 0) return TakeRandom(candidates, random);
      var selectedIndex = matchingIndexes[random.NextInt(matchingIndexes.Count)];
      var selected = candidates[selectedIndex];
      candidates.RemoveAt(selectedIndex);
      return selected;
    }

    private static Card TakeRandom(List<Card> candidates, IRandomSource random)
    {
      if (candidates.Count == 0) throw new InvalidOperationException("No distribution candidate remains.");
      var index = random.NextInt(candidates.Count);
      var card = candidates[index];
      candidates.RemoveAt(index);
      return card;
    }

    private static List<Card> NormalizeSelection(
      IReadOnlyList<Card> candidates,
      IReadOnlyList<CardId> selectedIds)
    {
      var requested = new HashSet<CardId>();
      for (var index = 0; index < selectedIds.Count; index++)
      {
        if (!requested.Add(selectedIds[index]))
        {
          throw new ArgumentException("A card cannot be selected twice.", nameof(selectedIds));
        }
      }

      var result = new List<Card>();
      for (var index = 0; index < candidates.Count; index++)
      {
        if (requested.Remove(candidates[index].Id)) result.Add(candidates[index]);
      }
      if (requested.Count != 0)
      {
        throw new ArgumentException("Every selected card must belong to its actor's acquired pool.", nameof(selectedIds));
      }
      return result;
    }

    private static HashSet<CardId> ToIdSet(IReadOnlyList<Card> cards)
    {
      var result = new HashSet<CardId>();
      for (var index = 0; index < cards.Count; index++) result.Add(cards[index].Id);
      return result;
    }

    private static void ValidateInputs(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedPlayerCards,
      IReadOnlyList<CardId> selectedAiCards,
      PrivateCardSelectionMode selectionMode,
      IRandomSource random)
    {
      if (playerAcquiredCards == null) throw new ArgumentNullException(nameof(playerAcquiredCards));
      if (aiAcquiredCards == null) throw new ArgumentNullException(nameof(aiAcquiredCards));
      if (otherCandidates == null) throw new ArgumentNullException(nameof(otherCandidates));
      if (selectedPlayerCards == null) throw new ArgumentNullException(nameof(selectedPlayerCards));
      if (selectedAiCards == null) throw new ArgumentNullException(nameof(selectedAiCards));
      if (random == null) throw new ArgumentNullException(nameof(random));
      if (!Enum.IsDefined(typeof(HalliStageWinner), winner)) throw new ArgumentOutOfRangeException(nameof(winner));
      if (!Enum.IsDefined(typeof(PrivateCardSelectionMode), selectionMode)) throw new ArgumentOutOfRangeException(nameof(selectionMode));
      HalliStageRules.GetWinTarget(combatRoundNumber);

      var ids = new HashSet<CardId>();
      AddAndValidate(playerAcquiredCards, ids, nameof(playerAcquiredCards));
      AddAndValidate(aiAcquiredCards, ids, nameof(aiAcquiredCards));
      AddAndValidate(otherCandidates, ids, nameof(otherCandidates));
    }

    private static void AddAndValidate(
      IReadOnlyList<Card> cards,
      HashSet<CardId> ids,
      string parameterName)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsValid || !ids.Add(cards[index].Id))
        {
          throw new ArgumentException("Distribution inputs must contain unique valid cards.", parameterName);
        }
      }
    }

    private static void EnsureEnoughCandidates(int actual, int required)
    {
      if (actual < required)
      {
        throw new ArgumentException($"Distribution needs {required} candidates, but only {actual} remain.");
      }
    }
  }
}
