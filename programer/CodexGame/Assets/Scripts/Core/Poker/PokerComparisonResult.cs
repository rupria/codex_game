using System;

namespace CodexGame.Core.Poker
{
  public sealed class PokerComparisonResult
  {
    public PokerComparisonResult(
      PokerWinner winner,
      PokerHandValue playerValue,
      PokerHandValue aiValue)
    {
      if (!Enum.IsDefined(typeof(PokerWinner), winner))
      {
        throw new ArgumentOutOfRangeException(nameof(winner));
      }

      Winner = winner;
      PlayerValue = playerValue ?? throw new ArgumentNullException(nameof(playerValue));
      AiValue = aiValue ?? throw new ArgumentNullException(nameof(aiValue));
    }

    public PokerWinner Winner { get; }
    public PokerHandValue PlayerValue { get; }
    public PokerHandValue AiValue { get; }
  }
}
