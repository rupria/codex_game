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
      ValidateInputs(
        playerAcquiredCards,
        aiAcquiredCards,
        otherCandidates,
        winner,
        combatRoundNumber,
        selectedWinnerCards,
        selectionMode,
        random);

      if (winner == HalliStageWinner.None)
      {
        return ResolveWithoutWinner(
          playerAcquiredCards,
          aiAcquiredCards,
          otherCandidates,
          combatRoundNumber,
          random);
      }

      var winnerCards = winner == HalliStageWinner.Player
        ? playerAcquiredCards
        : aiAcquiredCards;
      var loserCards = winner == HalliStageWinner.Player
        ? aiAcquiredCards
        : playerAcquiredCards;
      var directSelectionCount = PrivateCardDistributionRules.GetDirectSelectionCount(combatRoundNumber);
      var normalizedSelection = NormalizeSelection(winnerCards, selectedWinnerCards);

      if (selectionMode == PrivateCardSelectionMode.Confirmed
        && normalizedSelection.Count != directSelectionCount)
      {
        throw new ArgumentException(
          "A confirmed selection must contain the round's exact direct-selection count.",
          nameof(selectedWinnerCards));
      }

      if (selectionMode == PrivateCardSelectionMode.TimedOut
        && normalizedSelection.Count > directSelectionCount)
      {
        throw new ArgumentException(
          "A timed-out selection cannot exceed the round's direct-selection count.",
          nameof(selectedWinnerCards));
      }

      var winnerPrivate = new List<Card>(normalizedSelection);
      var randomCandidates = new List<Card>(
        loserCards.Count + otherCandidates.Count + winnerCards.Count - normalizedSelection.Count);
      randomCandidates.AddRange(loserCards);
      randomCandidates.AddRange(otherCandidates);

      var selectedIds = ToIdSet(normalizedSelection);

      for (var index = 0; index < winnerCards.Count; index++)
      {
        if (!selectedIds.Contains(winnerCards[index].Id))
        {
          randomCandidates.Add(winnerCards[index]);
        }
      }

      EnsureEnoughCandidates(randomCandidates.Count, GameRules.RequiredPrivateCards - winnerPrivate.Count + GameRules.RequiredPrivateCards + 1);

      FillRandom(winnerPrivate, GameRules.RequiredPrivateCards, randomCandidates, random);
      var loserPrivate = new List<Card>(GameRules.RequiredPrivateCards);
      FillRandom(loserPrivate, GameRules.RequiredPrivateCards, randomCandidates, random);
      var secondPublic = TakeRandom(randomCandidates, random);

      return winner == HalliStageWinner.Player
        ? CreateResult(winner, combatRoundNumber, winnerPrivate, loserPrivate, secondPublic, randomCandidates)
        : CreateResult(winner, combatRoundNumber, loserPrivate, winnerPrivate, secondPublic, randomCandidates);
    }

    private static PrivateCardDistributionResult ResolveWithoutWinner(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      int combatRoundNumber,
      IRandomSource random)
    {
      var randomCandidates = new List<Card>(
        playerAcquiredCards.Count + aiAcquiredCards.Count + otherCandidates.Count);
      randomCandidates.AddRange(playerAcquiredCards);
      randomCandidates.AddRange(aiAcquiredCards);
      randomCandidates.AddRange(otherCandidates);
      EnsureEnoughCandidates(randomCandidates.Count, (GameRules.RequiredPrivateCards * 2) + 1);

      var playerPrivate = new List<Card>(GameRules.RequiredPrivateCards);
      var aiPrivate = new List<Card>(GameRules.RequiredPrivateCards);

      for (var index = 0; index < GameRules.RequiredPrivateCards; index++)
      {
        playerPrivate.Add(TakeRandom(randomCandidates, random));
        aiPrivate.Add(TakeRandom(randomCandidates, random));
      }

      var secondPublic = TakeRandom(randomCandidates, random);
      return CreateResult(
        HalliStageWinner.None,
        combatRoundNumber,
        playerPrivate,
        aiPrivate,
        secondPublic,
        randomCandidates);
    }

    private static PrivateCardDistributionResult CreateResult(
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<Card> playerPrivate,
      IReadOnlyList<Card> aiPrivate,
      Card secondPublic,
      IReadOnlyList<Card> remainingCandidates)
    {
      if (playerPrivate.Count != GameRules.RequiredPrivateCards
        || aiPrivate.Count != GameRules.RequiredPrivateCards)
      {
        throw new InvalidOperationException("Both sides must finish with exactly three private cards.");
      }

      return new PrivateCardDistributionResult(
        winner,
        combatRoundNumber,
        playerPrivate,
        aiPrivate,
        secondPublic,
        remainingCandidates);
    }

    private static void FillRandom(
      List<Card> destination,
      int requiredCount,
      List<Card> candidates,
      IRandomSource random)
    {
      while (destination.Count < requiredCount)
      {
        destination.Add(TakeRandom(candidates, random));
      }
    }

    private static Card TakeRandom(List<Card> candidates, IRandomSource random)
    {
      if (candidates.Count == 0)
      {
        throw new InvalidOperationException("No candidate remains for random distribution.");
      }

      var index = random.NextInt(candidates.Count);
      var card = candidates[index];
      candidates.RemoveAt(index);
      return card;
    }

    private static List<Card> NormalizeSelection(
      IReadOnlyList<Card> winnerCards,
      IReadOnlyList<CardId> selectedWinnerCards)
    {
      var requested = new HashSet<CardId>();

      for (var index = 0; index < selectedWinnerCards.Count; index++)
      {
        if (!requested.Add(selectedWinnerCards[index]))
        {
          throw new ArgumentException("A winner card cannot be selected twice.", nameof(selectedWinnerCards));
        }
      }

      var normalized = new List<Card>(requested.Count);

      for (var index = 0; index < winnerCards.Count; index++)
      {
        if (requested.Remove(winnerCards[index].Id))
        {
          normalized.Add(winnerCards[index]);
        }
      }

      if (requested.Count > 0)
      {
        throw new ArgumentException(
          "Every selected card must belong to the Halli winner's acquired-card pool.",
          nameof(selectedWinnerCards));
      }

      return normalized;
    }

    private static HashSet<CardId> ToIdSet(IReadOnlyList<Card> cards)
    {
      var ids = new HashSet<CardId>();

      for (var index = 0; index < cards.Count; index++)
      {
        ids.Add(cards[index].Id);
      }

      return ids;
    }

    private static void ValidateInputs(
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      IReadOnlyList<Card> otherCandidates,
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<CardId> selectedWinnerCards,
      PrivateCardSelectionMode selectionMode,
      IRandomSource random)
    {
      if (playerAcquiredCards == null)
      {
        throw new ArgumentNullException(nameof(playerAcquiredCards));
      }

      if (aiAcquiredCards == null)
      {
        throw new ArgumentNullException(nameof(aiAcquiredCards));
      }

      if (otherCandidates == null)
      {
        throw new ArgumentNullException(nameof(otherCandidates));
      }

      if (selectedWinnerCards == null)
      {
        throw new ArgumentNullException(nameof(selectedWinnerCards));
      }

      if (random == null)
      {
        throw new ArgumentNullException(nameof(random));
      }

      if (!Enum.IsDefined(typeof(HalliStageWinner), winner))
      {
        throw new ArgumentOutOfRangeException(nameof(winner));
      }

      if (!Enum.IsDefined(typeof(PrivateCardSelectionMode), selectionMode))
      {
        throw new ArgumentOutOfRangeException(nameof(selectionMode));
      }

      HalliStageRules.GetWinTarget(combatRoundNumber);

      if (winner == HalliStageWinner.None && selectedWinnerCards.Count != 0)
      {
        throw new ArgumentException("A winner-less stage cannot have direct selections.", nameof(selectedWinnerCards));
      }

      var ids = new HashSet<CardId>();
      AddAndValidateCards(playerAcquiredCards, ids, nameof(playerAcquiredCards));
      AddAndValidateCards(aiAcquiredCards, ids, nameof(aiAcquiredCards));
      AddAndValidateCards(otherCandidates, ids, nameof(otherCandidates));

      if (winner != HalliStageWinner.None)
      {
        var winnerCount = winner == HalliStageWinner.Player
          ? playerAcquiredCards.Count
          : aiAcquiredCards.Count;
        PrivateCardDistributionRules.RequiresSelectionUi(combatRoundNumber, winnerCount);
      }
    }

    private static void AddAndValidateCards(
      IReadOnlyList<Card> cards,
      HashSet<CardId> ids,
      string parameterName)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (!cards[index].IsValid || !ids.Add(cards[index].Id))
        {
          throw new ArgumentException(
            "Distribution inputs must contain valid cards with no duplicate identities.",
            parameterName);
        }
      }
    }

    private static void EnsureEnoughCandidates(int actualCount, int requiredCount)
    {
      if (actualCount < requiredCount)
      {
        throw new ArgumentException(
          $"Distribution needs at least {requiredCount} random candidates, but only {actualCount} are available.");
      }
    }
  }
}
