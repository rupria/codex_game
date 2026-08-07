using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;

namespace CodexGame.Application.Distribution
{
  public sealed class PrivateCardSelectionSnapshot
  {
    public PrivateCardSelectionSnapshot(
      PrivateCardSelectionPhase phase,
      HalliStageWinner winner,
      int combatRoundNumber,
      int requiredSelectionCount,
      long remainingMicroseconds,
      IReadOnlyList<Card> winnerCandidates,
      IReadOnlyList<Card> selectedCards,
      PrivateCardDistributionResult? result)
    {
      Phase = phase;
      Winner = winner;
      CombatRoundNumber = combatRoundNumber;
      RequiredSelectionCount = requiredSelectionCount;
      RemainingMicroseconds = remainingMicroseconds;
      WinnerCandidates = Copy(winnerCandidates, nameof(winnerCandidates));
      SelectedCards = Copy(selectedCards, nameof(selectedCards));
      Result = result;
    }

    public PrivateCardSelectionPhase Phase { get; }
    public HalliStageWinner Winner { get; }
    public int CombatRoundNumber { get; }
    public int RequiredSelectionCount { get; }
    public long RemainingMicroseconds { get; }
    public IReadOnlyList<Card> WinnerCandidates { get; }
    public IReadOnlyList<Card> SelectedCards { get; }
    public bool CanConfirm => Phase == PrivateCardSelectionPhase.AwaitingSelection
      && SelectedCards.Count == RequiredSelectionCount;
    public PrivateCardDistributionResult? Result { get; }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> source, string parameterName)
    {
      if (source == null)
      {
        throw new ArgumentNullException(parameterName);
      }

      var copy = new Card[source.Count];

      for (var index = 0; index < source.Count; index++)
      {
        copy[index] = source[index];
      }

      return Array.AsReadOnly(copy);
    }
  }
}
