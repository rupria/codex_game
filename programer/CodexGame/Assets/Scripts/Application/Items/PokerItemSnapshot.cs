#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Items;

namespace CodexGame.Application.Items
{
  public sealed class PokerItemSnapshot
  {
    public PokerItemSnapshot(
      PokerItemPhase phase,
      IReadOnlyList<GameItemId> inventory,
      IReadOnlyList<Card> playerPrivateCards,
      IReadOnlyList<Card> visibleAiPrivateCards,
      IReadOnlyList<Card> publicCards,
      IReadOnlyList<Card> bottomDealCandidates,
      PokerItemFailure lastFailure,
      StageItemRestrictionSnapshot? stageRestriction = null,
      Card? revealingSecondPublicCard = null,
      float secondPublicRevealProgress = 1f,
      ItemUsePresentationSnapshot? usePresentation = null,
      CardId? wildInkCardId = null,
      bool barrelDefenseArmed = false,
      bool insuranceActivated = false,
      bool mercenaryExchangeApplied = false,
      long handConfirmationRemainingMicroseconds = 0,
      bool handConfirmationTimedOut = false,
      IReadOnlyList<CardId>? mercenaryEligibleTargets = null,
      bool canRecoverHealth = true)
    {
      Phase = phase;
      Inventory = Copy(inventory);
      PlayerPrivateCards = Copy(playerPrivateCards);
      VisibleAiPrivateCards = Copy(visibleAiPrivateCards);
      PublicCards = Copy(publicCards);
      BottomDealCandidates = Copy(bottomDealCandidates);
      LastFailure = lastFailure;
      StageRestriction = stageRestriction;
      RevealingSecondPublicCard = revealingSecondPublicCard;
      SecondPublicRevealProgress = secondPublicRevealProgress;
      UsePresentation = usePresentation ?? ItemUsePresentationSnapshot.Inactive;
      WildInkCardId = wildInkCardId;
      BarrelDefenseArmed = barrelDefenseArmed;
      InsuranceActivated = insuranceActivated;
      MercenaryExchangeApplied = mercenaryExchangeApplied;
      HandConfirmationRemainingMicroseconds = Math.Max(
        0,
        handConfirmationRemainingMicroseconds);
      HandConfirmationTimedOut = handConfirmationTimedOut;
      MercenaryEligibleTargets = Copy(
        mercenaryEligibleTargets ?? Array.AsReadOnly(Array.Empty<CardId>()));
      CanRecoverHealth = canRecoverHealth;
    }

    public PokerItemPhase Phase { get; }
    public IReadOnlyList<GameItemId> Inventory { get; }
    public IReadOnlyList<Card> PlayerPrivateCards { get; }
    public IReadOnlyList<Card> VisibleAiPrivateCards { get; }
    public IReadOnlyList<Card> PublicCards { get; }
    public IReadOnlyList<Card> BottomDealCandidates { get; }
    public PokerItemFailure LastFailure { get; }
    public StageItemRestrictionSnapshot? StageRestriction { get; }
    public Card? RevealingSecondPublicCard { get; }
    public float SecondPublicRevealProgress { get; }
    public ItemUsePresentationSnapshot UsePresentation { get; }
    public CardId? WildInkCardId { get; }
    public bool BarrelDefenseArmed { get; }
    public bool InsuranceActivated { get; }
    public bool MercenaryExchangeApplied { get; }
    public long HandConfirmationRemainingMicroseconds { get; }
    public bool HandConfirmationTimedOut { get; }
    public IReadOnlyList<CardId> MercenaryEligibleTargets { get; }
    public bool CanRecoverHealth { get; }

    private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> source)
    {
      if (source == null) throw new ArgumentNullException(nameof(source));
      var copy = new T[source.Count];
      for (var index = 0; index < copy.Length; index++) copy[index] = source[index];
      return Array.AsReadOnly(copy);
    }
  }
}
