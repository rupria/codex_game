using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeHalliSnapshot
  {
    public PrototypeHalliSnapshot(
      PrototypeSessionPhase phase,
      string statusMessage,
      long combatRoundSeed,
      int playerWins,
      int aiWins,
      int winTarget,
      int flipCount,
      int remainingDeckCards,
      long remainingMicroseconds,
      Card? firstPublicCard,
      IReadOnlyList<Card> leftPile,
      IReadOnlyList<Card> rightPile,
      HalliStageEndReason endReason)
    {
      Phase = phase;
      StatusMessage = statusMessage ?? throw new ArgumentNullException(nameof(statusMessage));
      CombatRoundSeed = combatRoundSeed;
      PlayerWins = playerWins;
      AiWins = aiWins;
      WinTarget = winTarget;
      FlipCount = flipCount;
      RemainingDeckCards = remainingDeckCards;
      RemainingMicroseconds = remainingMicroseconds;
      FirstPublicCard = firstPublicCard;
      LeftPile = Copy(leftPile);
      RightPile = Copy(rightPile);
      EndReason = endReason;
    }

    public PrototypeSessionPhase Phase { get; }
    public string StatusMessage { get; }
    public long CombatRoundSeed { get; }
    public int PlayerWins { get; }
    public int AiWins { get; }
    public int WinTarget { get; }
    public int FlipCount { get; }
    public int RemainingDeckCards { get; }
    public long RemainingMicroseconds { get; }
    public Card? FirstPublicCard { get; }
    public IReadOnlyList<Card> LeftPile { get; }
    public IReadOnlyList<Card> RightPile { get; }
    public HalliStageEndReason EndReason { get; }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> source)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
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
