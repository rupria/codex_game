using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;

namespace CodexGame.Core.Distribution
{
  public sealed class PrivateCardDistributionResult
  {
    public PrivateCardDistributionResult(
      HalliStageWinner winner,
      int combatRoundNumber,
      IReadOnlyList<Card> playerPrivateCards,
      IReadOnlyList<Card> aiPrivateCards,
      Card secondPublicCard,
      IReadOnlyList<Card> remainingCandidates)
    {
      if (!Enum.IsDefined(typeof(HalliStageWinner), winner))
      {
        throw new ArgumentOutOfRangeException(nameof(winner));
      }

      if (combatRoundNumber < 1)
      {
        throw new ArgumentOutOfRangeException(nameof(combatRoundNumber));
      }

      Winner = winner;
      CombatRoundNumber = combatRoundNumber;
      PlayerPrivateCards = Copy(playerPrivateCards, nameof(playerPrivateCards));
      AiPrivateCards = Copy(aiPrivateCards, nameof(aiPrivateCards));
      SecondPublicCard = secondPublicCard;
      RemainingCandidates = Copy(remainingCandidates, nameof(remainingCandidates));
    }

    public HalliStageWinner Winner { get; }
    public int CombatRoundNumber { get; }
    public IReadOnlyList<Card> PlayerPrivateCards { get; }
    public IReadOnlyList<Card> AiPrivateCards { get; }
    public Card SecondPublicCard { get; }
    public IReadOnlyList<Card> RemainingCandidates { get; }

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
