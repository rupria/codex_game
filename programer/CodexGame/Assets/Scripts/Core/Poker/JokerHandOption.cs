using System;
using CodexGame.Core.Cards;

namespace CodexGame.Core.Poker
{
  public sealed class JokerHandOption
  {
    public JokerHandOption(
      PokerHandCategory category,
      Card replacementCard,
      PokerHandValue handValue)
    {
      if (!Enum.IsDefined(typeof(PokerHandCategory), category))
      {
        throw new ArgumentOutOfRangeException(nameof(category));
      }
      if (!replacementCard.IsValid || replacementCard.IsJoker)
      {
        throw new ArgumentException("A Joker replacement must be a standard card.", nameof(replacementCard));
      }

      Category = category;
      ReplacementCard = replacementCard;
      HandValue = handValue ?? throw new ArgumentNullException(nameof(handValue));
      if (HandValue.Category != Category)
      {
        throw new ArgumentException("The option category must match its evaluated hand.", nameof(handValue));
      }
    }

    public PokerHandCategory Category { get; }
    public Card ReplacementCard { get; }
    public PokerHandValue HandValue { get; }
  }
}
