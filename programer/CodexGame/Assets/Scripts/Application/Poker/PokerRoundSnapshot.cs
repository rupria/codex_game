#nullable enable
using System;
using System.Collections.Generic;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;

namespace CodexGame.Application.Poker
{
  public enum PokerResultPresentationStep
  {
    None = 0,
    Cards = 1,
    Outcome = 2,
    Complete = 3
  }

  public sealed class PokerRoundSnapshot
  {
    public PokerRoundSnapshot(
      PokerRoundPhase phase,
      IReadOnlyList<Card> playerPrivateCards,
      IReadOnlyList<Card> visibleAiPrivateCards,
      IReadOnlyList<Card> publicCards,
      BattleHealth health,
      long remainingMicroseconds,
      PokerRoundResult? result,
      IReadOnlyList<JokerHandOption>? legalPlayerJokerOptions = null,
      PokerHandCategory? selectedPlayerJokerCategory = null,
      PokerResultPresentationStep resultPresentationStep = PokerResultPresentationStep.None)
    {
      Phase = phase;
      PlayerPrivateCards = Copy(playerPrivateCards, nameof(playerPrivateCards));
      VisibleAiPrivateCards = Copy(visibleAiPrivateCards, nameof(visibleAiPrivateCards));
      PublicCards = Copy(publicCards, nameof(publicCards));
      Health = health;
      RemainingMicroseconds = remainingMicroseconds;
      Result = result;
      LegalPlayerJokerOptions = CopyOptions(legalPlayerJokerOptions);
      SelectedPlayerJokerCategory = selectedPlayerJokerCategory;
      ResultPresentationStep = resultPresentationStep;
    }

    public PokerRoundPhase Phase { get; }
    public IReadOnlyList<Card> PlayerPrivateCards { get; }
    public IReadOnlyList<Card> VisibleAiPrivateCards { get; }
    public IReadOnlyList<Card> PublicCards { get; }
    public BattleHealth Health { get; }
    public long RemainingMicroseconds { get; }
    public PokerRoundResult? Result { get; }
    public IReadOnlyList<JokerHandOption> LegalPlayerJokerOptions { get; }
    public PokerHandCategory? SelectedPlayerJokerCategory { get; }
    public PokerResultPresentationStep ResultPresentationStep { get; }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> cards, string parameterName)
    {
      if (cards == null) throw new ArgumentNullException(parameterName);
      var copy = new Card[cards.Count];
      for (var index = 0; index < cards.Count; index++) copy[index] = cards[index];
      return Array.AsReadOnly(copy);
    }

    private static IReadOnlyList<JokerHandOption> CopyOptions(
      IReadOnlyList<JokerHandOption>? source)
    {
      if (source == null) return Array.AsReadOnly(Array.Empty<JokerHandOption>());
      var copy = new JokerHandOption[source.Count];
      for (var index = 0; index < source.Count; index++) copy[index] = source[index];
      return Array.AsReadOnly(copy);
    }
  }
}
