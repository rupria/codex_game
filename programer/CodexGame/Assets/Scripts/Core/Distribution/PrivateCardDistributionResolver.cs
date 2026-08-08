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
      IRandomSource random)
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
        random);
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
      IRandomSource random)
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
        random);
      var aiPrivate = RetainOrSelect(
        aiAcquiredCards,
        selectedAiCards,
        PrivateCardSelectionMode.Confirmed,
        randomCandidates,
        random);

      EnsureEnoughCandidates(
        randomCandidates.Count,
        (GameRules.RequiredPrivateCards - playerPrivate.Count)
          + (GameRules.RequiredPrivateCards - aiPrivate.Count)
          + 1);
      FillRandom(playerPrivate, randomCandidates, random);
      FillRandom(aiPrivate, randomCandidates, random);
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
      IRandomSource random)
    {
      if (acquired.Count <= GameRules.RequiredPrivateCards)
      {
        if (selectedIds.Count != 0)
        {
          throw new ArgumentException("Selections are accepted only when acquired cards exceed three.");
        }
        return new List<Card>(acquired);
      }

      var selected = NormalizeSelection(acquired, selectedIds);
      if (mode == PrivateCardSelectionMode.Confirmed
        && selected.Count != GameRules.RequiredPrivateCards)
      {
        throw new ArgumentException("A confirmed overflow selection must contain exactly three cards.");
      }
      if (mode == PrivateCardSelectionMode.TimedOut
        && selected.Count > GameRules.RequiredPrivateCards)
      {
        throw new ArgumentException("A timed-out selection cannot contain more than three cards.");
      }

      var selectedSet = ToIdSet(selected);
      var unselectedAcquired = new List<Card>();
      for (var index = 0; index < acquired.Count; index++)
      {
        if (!selectedSet.Contains(acquired[index].Id)) unselectedAcquired.Add(acquired[index]);
      }

      while (selected.Count < GameRules.RequiredPrivateCards)
      {
        selected.Add(TakeRandom(unselectedAcquired, random));
      }
      randomCandidates.AddRange(unselectedAcquired);
      return selected;
    }

    private static void FillRandom(
      List<Card> destination,
      List<Card> candidates,
      IRandomSource random)
    {
      while (destination.Count < GameRules.RequiredPrivateCards)
      {
        destination.Add(TakeRandom(candidates, random));
      }
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
