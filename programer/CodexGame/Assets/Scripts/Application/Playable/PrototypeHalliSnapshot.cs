#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeHalliSnapshot
  {
    public PrototypeHalliSnapshot(
      PrototypeSessionPhase phase,
      LocalizedStatus status,
      long combatRoundSeed,
      int combatRoundNumber,
      int playerWins,
      int aiWins,
      int playerAcquiredCount,
      int aiAcquiredCount,
      IReadOnlyList<Card> playerAcquiredCards,
      IReadOnlyList<Card> aiAcquiredCards,
      int winTarget,
      int flipCount,
      int remainingDeckCards,
      long remainingMicroseconds,
      HalliActor leadActor,
      bool canFlip,
      bool canRing,
      int revealStepNumber,
      HalliActor? revealingActor,
      HalliRelativeSide? revealingRelativeSide,
      PileSide? revealingPile,
      Card? revealingCard,
      bool revealCommitted,
      float revealProgress,
      Card? firstPublicCard,
      IReadOnlyList<Card> leftPile,
      IReadOnlyList<Card> rightPile,
      PrototypeAcquirer lastAcquirer,
      IReadOnlyList<Card> lastAcquiredCards,
      PileSide? lastAcquiredPile,
      PileSide? lastBellPile,
      PrototypeBellFeedback bellFeedback,
      HalliStageEndReason endReason,
      HalliAiBellAuditEntry? lastAiBellAuditEntry)
    {
      Phase = phase;
      Status = status ?? throw new ArgumentNullException(nameof(status));
      CombatRoundSeed = combatRoundSeed;
      CombatRoundNumber = combatRoundNumber;
      PlayerWins = playerWins;
      AiWins = aiWins;
      PlayerAcquiredCount = playerAcquiredCount;
      AiAcquiredCount = aiAcquiredCount;
      PlayerAcquiredCards = Copy(playerAcquiredCards);
      AiAcquiredCards = Copy(aiAcquiredCards);
      WinTarget = winTarget;
      FlipCount = flipCount;
      RemainingDeckCards = remainingDeckCards;
      RemainingMicroseconds = remainingMicroseconds;
      LeadActor = leadActor;
      CanFlip = canFlip;
      CanRing = canRing;
      RevealStepNumber = revealStepNumber;
      RevealingActor = revealingActor;
      RevealingRelativeSide = revealingRelativeSide;
      RevealingPile = revealingPile;
      RevealingCard = revealingCard;
      RevealCommitted = revealCommitted;
      RevealProgress = revealProgress;
      FirstPublicCard = firstPublicCard;
      LeftPile = Copy(leftPile);
      RightPile = Copy(rightPile);
      LastAcquirer = lastAcquirer;
      LastAcquiredCards = Copy(lastAcquiredCards);
      LastAcquiredPile = lastAcquiredPile;
      LastBellPile = lastBellPile;
      BellFeedback = bellFeedback;
      EndReason = endReason;
      LastAiBellAuditEntry = lastAiBellAuditEntry;
    }

    public PrototypeSessionPhase Phase { get; }
    public LocalizedStatus Status { get; }
    public long CombatRoundSeed { get; }
    public int CombatRoundNumber { get; }
    public int PlayerWins { get; }
    public int AiWins { get; }
    public int PlayerAcquiredCount { get; }
    public int AiAcquiredCount { get; }
    public IReadOnlyList<Card> PlayerAcquiredCards { get; }
    public IReadOnlyList<Card> AiAcquiredCards { get; }
    public int WinTarget { get; }
    public int FlipCount { get; }
    public int RemainingFlipCount => Math.Max(0, GameRules.HalliDistributionLimit - FlipCount);
    public int RemainingDeckCards { get; }
    public long RemainingMicroseconds { get; }
    public HalliActor LeadActor { get; }
    public bool CanFlip { get; }
    public bool CanRing { get; }
    public int RevealStepNumber { get; }
    public HalliActor? RevealingActor { get; }
    public HalliRelativeSide? RevealingRelativeSide { get; }
    public PileSide? RevealingPile { get; }
    public Card? RevealingCard { get; }
    public bool RevealCommitted { get; }
    public float RevealProgress { get; }
    public Card? FirstPublicCard { get; }
    public IReadOnlyList<Card> LeftPile { get; }
    public IReadOnlyList<Card> RightPile { get; }
    public PrototypeAcquirer LastAcquirer { get; }
    public IReadOnlyList<Card> LastAcquiredCards { get; }
    public PileSide? LastAcquiredPile { get; }
    public PileSide? LastBellPile { get; }
    public PrototypeBellFeedback BellFeedback { get; }
    public HalliStageEndReason EndReason { get; }
    public HalliAiBellAuditEntry? LastAiBellAuditEntry { get; }

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
