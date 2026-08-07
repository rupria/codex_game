using System;
using System.Collections.Generic;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;

namespace CodexGame.Application.Poker
{
  public sealed class PokerRoundSnapshot
  {
    public PokerRoundSnapshot(
      PokerRoundPhase phase,
      IReadOnlyList<Card> playerPrivateCards,
      IReadOnlyList<Card> visibleAiPrivateCards,
      IReadOnlyList<Card> publicCards,
      BattleHealth health,
      long remainingMicroseconds,
      int availableItemCount,
      bool handLocked,
      PokerRoundResult? result)
    {
      Phase = phase;
      PlayerPrivateCards = Copy(playerPrivateCards, nameof(playerPrivateCards));
      VisibleAiPrivateCards = Copy(visibleAiPrivateCards, nameof(visibleAiPrivateCards));
      PublicCards = Copy(publicCards, nameof(publicCards));
      Health = health;
      RemainingMicroseconds = remainingMicroseconds;
      AvailableItemCount = availableItemCount;
      HandLocked = handLocked;
      Result = result;
    }

    public PokerRoundPhase Phase { get; }
    public IReadOnlyList<Card> PlayerPrivateCards { get; }
    public IReadOnlyList<Card> VisibleAiPrivateCards { get; }
    public IReadOnlyList<Card> PublicCards { get; }
    public BattleHealth Health { get; }
    public long RemainingMicroseconds { get; }
    public int AvailableItemCount { get; }
    public bool HandLocked { get; }
    public PokerRoundResult? Result { get; }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> cards, string parameterName)
    {
      if (cards == null) throw new ArgumentNullException(parameterName);
      var copy = new Card[cards.Count];
      for (var index = 0; index < cards.Count; index++) copy[index] = cards[index];
      return Array.AsReadOnly(copy);
    }
  }
}
